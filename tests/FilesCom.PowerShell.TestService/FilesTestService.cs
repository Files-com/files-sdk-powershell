using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FilesCom.PowerShell.Tests
{
    /// <summary>
    /// A Files.com site on a loopback port. Tests script its answers per route and read back every request it received,
    /// in order, with its headers and body. Routes are relative to /api/rest/v1. It speaks HTTP/1.1 over its own socket
    /// handling so it can tell when a client abandons a request, which cancellation tests need, and how many connections
    /// clients hold open. It also answers requests sent to it as an HTTP proxy, for any site, as that site. Its
    /// <see cref="CreateHandler"/> gives the same answers in memory, where it also stands in for Amazon S3 storage.
    /// </summary>
    public sealed partial class FilesTestService : IDisposable
    {
        private readonly TcpListener listener;
        private readonly CancellationTokenSource stopping = new CancellationTokenSource();
        private readonly ConcurrentDictionary<string, RouteQueue> routes = new ConcurrentDictionary<string, RouteQueue>(StringComparer.Ordinal);
        private readonly List<RecordedRequest> requests = new List<RecordedRequest>();
        private readonly ConcurrentDictionary<string, UploadState> uploads = new ConcurrentDictionary<string, UploadState>(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, byte[]> downloads = new ConcurrentDictionary<string, byte[]>(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, int> downloadFailures = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, Dictionary<string, string>> downloadHeaders = new ConcurrentDictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, int> downloadStalls = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, DownloadHold> downloadHolds = new ConcurrentDictionary<string, DownloadHold>(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, VersionedDownload> versionedDownloads = new ConcurrentDictionary<string, VersionedDownload>(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, RangeFailure> rangeFailures = new ConcurrentDictionary<string, RangeFailure>(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, DownloadHold> rangeHolds = new ConcurrentDictionary<string, DownloadHold>(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, DownloadHold> routeHolds = new ConcurrentDictionary<string, DownloadHold>(StringComparer.Ordinal);
        private int activeRequests;
        private int abortedRequests;
        private int openConnections;
        private int activeParts;
        private int peakParts;

        public FilesTestService()
        {
            listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            BaseUrl = $"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/";
            _ = AcceptLoop();
        }

        public string BaseUrl { get; }

        public IReadOnlyList<RecordedRequest> Requests
        {
            get
            {
                lock (requests)
                {
                    return requests.ToList();
                }
            }
        }

        /// <summary>Requests whose response has not been written yet.</summary>
        public int ActiveRequests => Volatile.Read(ref activeRequests);

        /// <summary>Requests whose client closed the connection before the response was written.</summary>
        public int AbortedRequests => Volatile.Read(ref abortedRequests);

        /// <summary>Connections clients have open to the service now.</summary>
        public int OpenConnections => Volatile.Read(ref openConnections);

        /// <summary>The most part requests, of all uploads together, the service was answering at the same time.</summary>
        public int PeakPartsOfAllUploads => Volatile.Read(ref peakParts);

        /// <summary>
        /// When set, as "user:password", a request sent to the service as a proxy must carry those credentials in
        /// Proxy-Authorization; one without them is answered 407 with a Basic challenge.
        /// </summary>
        public string RequiredProxyCredential { get; set; }

        public void ClearRequests()
        {
            lock (requests)
            {
                requests.Clear();
            }
        }

        /// <summary>
        /// Answers method requests to path with status and body. Queued answers are used in order; the last one repeats.
        /// Headers is a hashtable of response headers.
        /// </summary>
        public void Respond(string method, string path, int status, string body, System.Collections.IDictionary headers = null, int delayMilliseconds = 0)
        {
            Queue(method, path).Add(new Answer { Status = status, Body = body, Headers = ToHeaders(headers), Delay = delayMilliseconds });
        }

        /// <summary>Holds method requests to path open until the client gives up on them or the service stops.</summary>
        public void Hang(string method, string path)
        {
            Queue(method, path).Add(new Answer { Hang = true });
        }

        /// <summary>Closes the connection of method requests to path without answering.</summary>
        public void Drop(string method, string path)
        {
            Queue(method, path).Add(new Answer { Drop = true });
        }

        /// <summary>
        /// Answers GET path with the pages given, each a JSON array. Page n+1 is requested with cursor=page-(n+1), which
        /// page n announces in X-Files-Cursor-Next; the last page announces none.
        /// </summary>
        public void Pages(string path, params string[] pages)
        {
            RouteQueue queue = Queue("GET", path);
            queue.Pages = pages;
        }

        /// <summary>
        /// Accepts uploads to remotePath in parts of partSize bytes, sent back to this service. The final request answers
        /// with finalStatus, or drops its connection when finalStatus is 0.
        /// </summary>
        public void ServeUpload(string remotePath, long partSize, bool parallelParts = true, int finalStatus = 200)
        {
            uploads[remotePath] = new UploadState { PartSize = partSize, ParallelParts = parallelParts, FinalStatus = finalStatus };
        }

        /// <summary>
        /// Sends the parts of uploads to remotePath straight to Amazon S3 as an ordinary multipart upload, as Files.com
        /// does for files it stores in S3: each part's grant is an ordinary write (action put/write) by PUT to an UploadPart
        /// URL at <see cref="S3Host"/>, presigned for the host alone with <see cref="PartSignature"/>, that asks for nothing
        /// but the part's length and bytes, with available_parts when availableParts is given. partSize is then only
        /// Files.com's recommendation: the engine may choose the sizes. Only clients of the in-memory handler are given
        /// these URLs; over a socket, parts go to the service's own address.
        /// </summary>
        public void SendPartsToS3(string remotePath, long? availableParts = null)
        {
            UploadState upload = uploads[remotePath];
            upload.S3 = true;
            upload.AvailableParts = availableParts;
        }

        /// <summary>The signature of S3 part addresses, which must never appear in output.</summary>
        public const string PartSignature = "sentinel-part-signature-3V8";

        /// <summary>The length of each accepted part of the upload to remotePath, in part number order.</summary>
        public long[] PartLengths(string remotePath)
        {
            UploadState upload = uploads[remotePath];
            lock (upload)
            {
                return upload.Parts.OrderBy(part => part.Key).Select(part => (long)part.Value.Length).ToArray();
            }
        }

        /// <summary>
        /// The SHA-256, in hex, of the body of every attempt to send the given part of the upload to remotePath, failed or
        /// accepted, in the order they arrived.
        /// </summary>
        public string[] AttemptDigests(string remotePath, long partNumber)
        {
            UploadState upload = uploads[remotePath];
            lock (upload)
            {
                return upload.Attempts.TryGetValue(partNumber, out List<string> digests) ? digests.ToArray() : Array.Empty<string>();
            }
        }

        /// <summary>Fails the next attempt of the given part of an upload to remotePath with status.</summary>
        public void FailPart(string remotePath, long partNumber, int status)
        {
            uploads[remotePath].PartFailures[partNumber] = status;
        }

        /// <summary>
        /// Makes the first URL issued for the given part of uploads to remotePath expire: its request is answered as S3
        /// answers a presigned URL used after its expiry (403, AccessDenied, "Request has expired"), and a URL issued for
        /// that part again, which carries a later grant number, is accepted.
        /// </summary>
        public void ExpirePartGrant(string remotePath, long partNumber)
        {
            uploads[remotePath].ExpiringParts[partNumber] = true;
        }

        /// <summary>How many URLs begin_upload has issued for a part given to ExpirePartGrant.</summary>
        public int GrantsIssued(string remotePath, long partNumber)
        {
            return uploads[remotePath].Grants.TryGetValue(partNumber, out int issued) ? issued : 0;
        }

        /// <summary>Leaves the given part of an upload to remotePath unanswered until its client leaves.</summary>
        public void StallPart(string remotePath, long partNumber)
        {
            uploads[remotePath].PartStalls[partNumber] = true;
        }

        /// <summary>The bytes an upload to remotePath delivered, assembled from its parts in order.</summary>
        /// <summary>Holds every part request of the upload for the given time before answering it, so parts overlap.</summary>
        public void DelayParts(string remotePath, int delayMilliseconds)
        {
            uploads[remotePath].PartDelay = delayMilliseconds;
        }

        /// <summary>The most part requests of the upload the service was answering at the same time.</summary>
        public int PeakParts(string remotePath)
        {
            return Volatile.Read(ref uploads[remotePath].Peak);
        }

        public byte[] Uploaded(string remotePath)
        {
            UploadState upload = uploads[remotePath];
            lock (upload)
            {
                return upload.Parts.OrderBy(part => part.Key).SelectMany(part => part.Value).ToArray();
            }
        }

        /// <summary>Serves remotePath for download with content, and with headers on the download response if given.</summary>
        public void ServeDownload(string remotePath, byte[] content, System.Collections.IDictionary headers = null)
        {
            downloads[remotePath] = content;
            downloadHeaders[remotePath] = ToHeaders(headers);
        }

        /// <summary>
        /// Serves remotePath from storage that keeps versions, as Amazon S3 does: its contents answer every request with
        /// the strong entity tag entityTag, a Range request with 206, its Content-Range and exactly those bytes, and a
        /// request whose If-Match is not the current version's with 412. A client of the in-memory handler is given its
        /// address at <see cref="S3Host"/>, from which the engine binds a download to the version its first range
        /// answers with; a client over a socket is given the service's own address, from which it fetches the file whole.
        /// </summary>
        public void ServeVersionedDownload(string remotePath, byte[] content, string entityTag)
        {
            downloads[remotePath] = content;
            versionedDownloads[remotePath] = new VersionedDownload { EntityTag = entityTag };
        }

        /// <summary>A source with modification dates but no strong entity tag; its second response has reversed bytes.</summary>
        public void ServeDatedDownload(string remotePath, byte[] content, string initialDate, string laterDate)
        {
            ServeVersionedDownload(remotePath, content, null);
            versionedDownloads[remotePath].InitialDate = initialDate;
            versionedDownloads[remotePath].LaterDate = laterDate;
        }

        /// <summary>The Range requests a versioned download of remotePath was answered with 206 for, as "start-end", in the order answered.</summary>
        public IReadOnlyList<string> RangesServed(string remotePath)
        {
            return versionedDownloads.TryGetValue(remotePath, out VersionedDownload versioned) ? versioned.Ranges.ToArray() : Array.Empty<string>();
        }

        /// <summary>
        /// Makes a versioned download of remotePath a new version from now on: content, with entityTag. Requests naming
        /// the earlier version are answered with 412, as storage answers for a version it no longer has.
        /// </summary>
        public void ChangeVersion(string remotePath, byte[] content, string entityTag)
        {
            VersionedDownload versioned = versionedDownloads[remotePath];
            lock (versioned)
            {
                downloads[remotePath] = content;
                versioned.EntityTag = entityTag;
            }
        }

        /// <summary>Answers the next times requests for the range of a versioned download of remotePath that starts at start with status.</summary>
        public void FailRange(string remotePath, long start, int status, int times = 1)
        {
            rangeFailures[RangeKey(remotePath, start)] = new RangeFailure { Status = status, Remaining = times };
        }

        /// <summary>
        /// Holds requests for the range of a versioned download of remotePath that starts at start, before answering them,
        /// until <see cref="ReleaseRange"/>; they are then answered as the download is at that time. <see cref="IsRangeHeld"/>
        /// tells when a request has reached the hold.
        /// </summary>
        public void HoldRange(string remotePath, long start)
        {
            rangeHolds[RangeKey(remotePath, start)] = new DownloadHold();
        }

        /// <summary>Whether a request for the held range of remotePath that starts at start is waiting.</summary>
        public bool IsRangeHeld(string remotePath, long start)
        {
            return rangeHolds.TryGetValue(RangeKey(remotePath, start), out DownloadHold hold) && hold.Reached.IsSet;
        }

        /// <summary>Answers the held requests for the range of remotePath that starts at start, and later ones at once.</summary>
        public void ReleaseRange(string remotePath, long start)
        {
            rangeHolds[RangeKey(remotePath, start)].Release.Set();
        }

        private static string RangeKey(string remotePath, long start)
        {
            return remotePath + "\n" + start.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Makes the download of remotePath's contents fail with status, after its details are served.</summary>
        public void FailDownload(string remotePath, int status)
        {
            downloadFailures[remotePath] = status;
        }

        /// <summary>Sends only the first byteCount bytes of remotePath's contents, then keeps the download open until its client leaves.</summary>
        public void StallDownload(string remotePath, int byteCount)
        {
            downloadStalls[remotePath] = byteCount;
        }

        /// <summary>
        /// Sends the first byteCount bytes of remotePath's contents, then waits for <see cref="ReleaseDownload"/> before
        /// sending the rest. <see cref="IsDownloadHeld"/> tells when the download has reached the hold.
        /// </summary>
        public void HoldDownload(string remotePath, int byteCount)
        {
            downloadHolds[remotePath] = new DownloadHold { ByteCount = byteCount };
        }

        /// <summary>Whether a download of remotePath is waiting at its hold.</summary>
        public bool IsDownloadHeld(string remotePath)
        {
            return downloadHolds.TryGetValue(remotePath, out DownloadHold hold) && hold.Reached.IsSet;
        }

        /// <summary>Sends the rest of a held download.</summary>
        public void ReleaseDownload(string remotePath)
        {
            downloadHolds[remotePath].Release.Set();
        }

        /// <summary>
        /// Holds the next API request to method path, such as a folder's listing or a file's details, until
        /// <see cref="ReleaseRoute"/>, then answers it as usual. Later requests are not held. <see cref="IsRouteHeld"/>
        /// tells when the request has arrived.
        /// </summary>
        public void HoldRoute(string method, string path)
        {
            routeHolds[method + " " + path] = new DownloadHold();
        }

        /// <summary>Whether a request held by <see cref="HoldRoute"/> has arrived and waits.</summary>
        public bool IsRouteHeld(string method, string path)
        {
            return routeHolds.TryGetValue(method + " " + path, out DownloadHold hold) && hold.Reached.IsSet;
        }

        /// <summary>Answers a request held by <see cref="HoldRoute"/>.</summary>
        public void ReleaseRoute(string method, string path)
        {
            routeHolds[method + " " + path].Release.Set();
        }

        public void Dispose()
        {
            stopping.Cancel();
            listener.Stop();
        }

        private RouteQueue Queue(string method, string path)
        {
            return routes.GetOrAdd(method.ToUpperInvariant() + " " + path, _ => new RouteQueue());
        }

        private static Dictionary<string, string> ToHeaders(System.Collections.IDictionary headers)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (headers != null)
            {
                foreach (System.Collections.DictionaryEntry entry in headers)
                {
                    result[entry.Key.ToString()] = entry.Value?.ToString();
                }
            }
            return result;
        }

        private async Task AcceptLoop()
        {
            while (!stopping.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    // Dispose stops the listener, which ends a pending accept.
                    client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    return;
                }
                _ = Task.Run(() => Serve(client));
            }
        }

        // One connection, which carries requests one after another until either side closes it.
        private async Task Serve(TcpClient client)
        {
            Interlocked.Increment(ref openConnections);
            try
            {
                await ServeRequests(client).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Decrement(ref openConnections);
            }
        }

        private async Task ServeRequests(TcpClient client)
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                var reader = new HttpReader(stream);
                while (!stopping.IsCancellationRequested)
                {
                    RecordedRequest request;
                    try
                    {
                        request = await reader.ReadRequestAsync(stopping.Token).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    if (request == null)
                    {
                        return;
                    }
                    lock (requests)
                    {
                        requests.Add(request);
                    }
                    Interlocked.Increment(ref activeRequests);
                    try
                    {
                        bool keepOpen = ProxyAuthorized(request)
                            ? await AnswerAsync(request, stream).ConfigureAwait(false)
                            : await ChallengeProxy(stream).ConfigureAwait(false);
                        if (!keepOpen)
                        {
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        request.ClientAborted = true;
                        Interlocked.Increment(ref abortedRequests);
                        return;
                    }
                    finally
                    {
                        Interlocked.Decrement(ref activeRequests);
                    }
                }
            }
        }

        // Writes the answer to one request; false closes the connection.
        private async Task<bool> AnswerAsync(RecordedRequest request, Stream stream)
        {
            const string api = "/api/rest/v1";
            if (request.Path.StartsWith("/upload/", StringComparison.Ordinal))
            {
                return await AnswerPart(request, stream).ConfigureAwait(false);
            }
            if (request.Path.StartsWith("/download/", StringComparison.Ordinal))
            {
                string remote = Uri.UnescapeDataString(request.Path.Substring("/download/".Length));
                if (downloadFailures.TryGetValue(remote, out int failure))
                {
                    await Write(stream, failure, Encoding.UTF8.GetBytes("<Error>Signature expired</Error>"), "application/xml", null).ConfigureAwait(false);
                    return true;
                }
                if (downloadHolds.TryGetValue(remote, out DownloadHold hold))
                {
                    byte[] content = downloads[remote];
                    await Write(stream, 200, content, "application/octet-stream", null, hold.ByteCount).ConfigureAwait(false);
                    hold.Reached.Set();
                    await Task.Run(() => hold.Release.Wait(stopping.Token)).ConfigureAwait(false);
                    await stream.WriteAsync(content, hold.ByteCount, content.Length - hold.ByteCount, stopping.Token).ConfigureAwait(false);
                    await stream.FlushAsync(stopping.Token).ConfigureAwait(false);
                    return true;
                }
                if (versionedDownloads.TryGetValue(remote, out VersionedDownload versioned))
                {
                    await AnswerVersionedDownload(request, stream, remote, versioned).ConfigureAwait(false);
                    return true;
                }
                if (downloadStalls.TryGetValue(remote, out int sent))
                {
                    await Write(stream, 200, downloads[remote], "application/octet-stream", null, sent).ConfigureAwait(false);
                    await WaitForClientToLeave(request, stream).ConfigureAwait(false);
                    return false;
                }
                await Write(stream, 200, downloads[remote], "application/octet-stream", downloadHeaders.TryGetValue(remote, out Dictionary<string, string> extra) ? extra : null).ConfigureAwait(false);
                return true;
            }
            string path = request.Path.StartsWith(api, StringComparison.Ordinal) ? request.Path.Substring(api.Length) : request.Path;
            string decoded = Uri.UnescapeDataString(path);
            if (routeHolds.TryGetValue(request.Method + " " + decoded, out DownloadHold routeHold) && !routeHold.Reached.IsSet)
            {
                routeHold.Reached.Set();
                await Task.Run(() => routeHold.Release.Wait(stopping.Token)).ConfigureAwait(false);
            }
            if (request.Method == "POST" && decoded.StartsWith("/file_actions/begin_upload/", StringComparison.Ordinal))
            {
                return await AnswerBeginUpload(request, decoded.Substring("/file_actions/begin_upload/".Length), stream).ConfigureAwait(false);
            }
            if (request.Method == "POST" && decoded.StartsWith("/files/", StringComparison.Ordinal) && uploads.ContainsKey(decoded.Substring("/files/".Length)) && request.Body.Contains("\"action\":\"end\""))
            {
                return await AnswerUploadEnd(request, decoded.Substring("/files/".Length), stream).ConfigureAwait(false);
            }
            if (request.Method == "GET" && decoded.StartsWith("/files/", StringComparison.Ordinal) && downloads.ContainsKey(decoded.Substring("/files/".Length)))
            {
                // A file's details, as Files.com gives them: its download URL names the storage it is fetched from.
                string remote = decoded.Substring("/files/".Length);
                string address = request.InMemory && versionedDownloads.ContainsKey(remote)
                    ? S3PresignedUrl("download/" + Uri.EscapeDataString(remote), "", DownloadSignature)
                    : $"{BaseUrl}download/{Uri.EscapeDataString(remote)}?X-Files-Signature={DownloadSignature}";
                string details = $"{{\"path\":{Json(remote)},\"type\":\"file\",\"size\":{downloads[remote].Length},\"download_uri\":{Json(address)}}}";
                await Write(stream, 200, Encoding.UTF8.GetBytes(details), "application/json", null).ConfigureAwait(false);
                return true;
            }
            if (request.Method == "GET" && decoded.StartsWith("/files/", StringComparison.Ordinal) && treeFiles.TryGetValue(decoded.Substring("/files/".Length), out TreeFile treeFile))
            {
                await Write(stream, 200, Encoding.UTF8.GetBytes(TreeFileDetails(request, decoded.Substring("/files/".Length), treeFile)), "application/json", null).ConfigureAwait(false);
                return true;
            }

            if (!routes.TryGetValue(request.Method + " " + decoded, out RouteQueue queue) && !routes.TryGetValue(request.Method + " " + path, out queue))
            {
                await Write(stream, 404, Encoding.UTF8.GetBytes("{\"error\":\"Not Found\",\"http-code\":404,\"title\":\"Not Found\",\"type\":\"not-found\"}"), "application/json", null).ConfigureAwait(false);
                return true;
            }
            if (queue.Pages != null)
            {
                int page = 1;
                if (request.Query.TryGetValue("cursor", out string cursor) && cursor.StartsWith("page-", StringComparison.Ordinal))
                {
                    page = int.Parse(cursor.Substring(5), CultureInfo.InvariantCulture);
                }
                var headers = new Dictionary<string, string>();
                if (page < queue.Pages.Length)
                {
                    headers["X-Files-Cursor-Next"] = "page-" + (page + 1).ToString(CultureInfo.InvariantCulture);
                }
                await Write(stream, 200, Encoding.UTF8.GetBytes(queue.Pages[page - 1]), "application/json", headers).ConfigureAwait(false);
                return true;
            }
            Answer answer = queue.Next();
            if (answer.Hang)
            {
                await WaitForClientToLeave(request, stream).ConfigureAwait(false);
                return false;
            }
            if (answer.Drop)
            {
                return false;
            }
            if (answer.Delay > 0)
            {
                await Task.Delay(answer.Delay, stopping.Token).ConfigureAwait(false);
            }
            await Write(stream, answer.Status, answer.Body == null ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(answer.Body), "application/json", answer.Headers).ConfigureAwait(false);
            return true;
        }

        private bool ProxyAuthorized(RecordedRequest request)
        {
            string required = RequiredProxyCredential;
            return required == null || request.AbsoluteTarget == null || request.Header("Proxy-Authorization") == "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(required));
        }

        private async Task<bool> ChallengeProxy(Stream stream)
        {
            await Write(stream, 407, Array.Empty<byte>(), "text/plain", new Dictionary<string, string> { ["Proxy-Authenticate"] = "Basic realm=\"test proxy\"" }).ConfigureAwait(false);
            return true;
        }

        // A client that gives up closes its connection, which makes a read return nothing.
        private async Task WaitForClientToLeave(RecordedRequest request, Stream stream)
        {
            var buffer = new byte[1];
            try
            {
                int read = await stream.ReadAsync(buffer, 0, 1, stopping.Token).ConfigureAwait(false);
                if (read == 0)
                {
                    request.ClientAborted = true;
                    Interlocked.Increment(ref abortedRequests);
                }
            }
            catch (Exception)
            {
                request.ClientAborted = true;
                Interlocked.Increment(ref abortedRequests);
            }
        }

        private async Task<bool> AnswerBeginUpload(RecordedRequest request, string remote, Stream stream)
        {
            if (!uploads.TryGetValue(remote, out UploadState upload))
            {
                await Write(stream, 404, Encoding.UTF8.GetBytes("{\"error\":\"Not Found\",\"http-code\":404,\"type\":\"not-found\"}"), "application/json", null).ConfigureAwait(false);
                return true;
            }
            long first = request.JsonNumber("part") ?? 1;
            long count = request.JsonNumber("parts") ?? 1;
            bool s3 = upload.S3 && request.InMemory;
            string available = upload.AvailableParts.HasValue ? $",\"available_parts\":{upload.AvailableParts.Value}" : "";
            var parts = new List<string>();
            for (long number = first; number < first + count; number++)
            {
                string target;
                if (s3)
                {
                    target = S3PresignedUrl("upload/" + Uri.EscapeDataString(remote), $"partNumber={number}&uploadId={upload.UploadId}&", PartSignature);
                }
                else
                {
                    string grant = upload.ExpiringParts.ContainsKey(number) ? "?grant=" + upload.Grants.AddOrUpdate(number, 1, (_, issued) => issued + 1).ToString(CultureInfo.InvariantCulture) : "";
                    target = $"{BaseUrl}upload/{Uri.EscapeDataString(remote)}/{number}{grant}";
                }
                parts.Add($"{{\"ref\":\"ref-{Math.Abs(remote.GetHashCode())}\",\"action\":\"put/write\",\"part_number\":{number},\"partsize\":{upload.PartSize},\"upload_uri\":{Json(target)},\"http_method\":\"PUT\",\"parallel_parts\":{(upload.ParallelParts ? "true" : "false")},\"retry_parts\":true,\"headers\":{{}},\"send\":{{\"partsize\":\"required-header Content-Length\",\"partdata\":\"body\"}}{available}}}");
            }
            await Write(stream, 200, Encoding.UTF8.GetBytes("[" + string.Join(",", parts) + "]"), "application/json", null).ConfigureAwait(false);
            return true;
        }

        private async Task<bool> AnswerPart(RecordedRequest request, Stream stream)
        {
            // The service's own part addresses end with the part number; S3's name it in the query.
            string[] segments = request.Path.Substring("/upload/".Length).Split('/');
            string remote = Uri.UnescapeDataString(segments[0]);
            long number = long.Parse(segments.Length > 1 ? segments[1] : request.Query["partNumber"], CultureInfo.InvariantCulture);
            UploadState upload = uploads[remote];
            int active = Interlocked.Increment(ref upload.Active);
            int activeOfAll = Interlocked.Increment(ref activeParts);
            try
            {
                RaisePeak(ref upload.Peak, active);
                RaisePeak(ref peakParts, activeOfAll);
                if (upload.PartDelay > 0)
                {
                    await Task.Delay(upload.PartDelay, stopping.Token).ConfigureAwait(false);
                }
                return await AnswerPartNow(request, stream, upload, number).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Decrement(ref activeParts);
                Interlocked.Decrement(ref upload.Active);
            }
        }

        private static void RaisePeak(ref int peak, int value)
        {
            int current;
            while (value > (current = Volatile.Read(ref peak)) && Interlocked.CompareExchange(ref peak, value, current) != current)
            {
            }
        }

        private async Task<bool> AnswerPartNow(RecordedRequest request, Stream stream, UploadState upload, long number)
        {
            lock (upload)
            {
                if (!upload.Attempts.TryGetValue(number, out List<string> digests))
                {
                    upload.Attempts[number] = digests = new List<string>();
                }
                using (SHA256 sha = SHA256.Create())
                {
                    digests.Add(BitConverter.ToString(sha.ComputeHash(request.BodyBytes)).Replace("-", ""));
                }
            }
            if (request.Query.ContainsKey("partNumber") && !(request.Query.TryGetValue("X-Amz-Signature", out string signature) && signature == PartSignature))
            {
                await Write(stream, 403, Array.Empty<byte>(), "text/plain", null).ConfigureAwait(false);
                return true;
            }
            if (upload.ExpiringParts.ContainsKey(number) && request.Query.TryGetValue("grant", out string grant) && grant == "1")
            {
                await Write(stream, 403, Encoding.UTF8.GetBytes(ExpiredPresignedUrl), "application/xml", null).ConfigureAwait(false);
                return true;
            }
            if (upload.PartFailures.TryRemove(number, out int failure))
            {
                await Write(stream, failure, Array.Empty<byte>(), "text/plain", null).ConfigureAwait(false);
                return true;
            }
            if (upload.PartStalls.TryRemove(number, out _))
            {
                await WaitForClientToLeave(request, stream).ConfigureAwait(false);
                return false;
            }
            lock (upload)
            {
                upload.Parts[number] = request.BodyBytes;
            }
            await Write(stream, 200, Array.Empty<byte>(), "text/plain", new Dictionary<string, string> { ["ETag"] = $"\"etag-{number}\"" }).ConfigureAwait(false);
            return true;
        }

        private async Task<bool> AnswerUploadEnd(RecordedRequest request, string remote, Stream stream)
        {
            UploadState upload = uploads[remote];
            if (upload.FinalStatus == 0)
            {
                return false;
            }
            long size = request.JsonNumber("size") ?? 0;
            string body = upload.FinalStatus == 200
                ? $"{{\"path\":{Json(remote)},\"type\":\"file\",\"size\":{size}}}"
                : "{\"error\":\"Internal failure\",\"http-code\":500,\"type\":\"internal-server-error\"}";
            await Write(stream, upload.FinalStatus, Encoding.UTF8.GetBytes(body), "application/json", null).ConfigureAwait(false);
            return true;
        }

        // Writes a response announcing all of body, of which only the first sentBytes are sent when given.
        private async Task Write(Stream stream, int status, byte[] body, string contentType, IDictionary<string, string> headers, int? sentBytes = null)
        {
            var head = new StringBuilder();
            head.Append("HTTP/1.1 ").Append(status.ToString(CultureInfo.InvariantCulture)).Append(' ').Append(Reason(status)).Append("\r\n");
            head.Append("Content-Type: ").Append(contentType).Append("\r\n");
            head.Append("Content-Length: ").Append(body.Length.ToString(CultureInfo.InvariantCulture)).Append("\r\n");
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    head.Append(header.Key).Append(": ").Append(header.Value).Append("\r\n");
                }
            }
            head.Append("\r\n");
            byte[] headBytes = Encoding.ASCII.GetBytes(head.ToString());
            await stream.WriteAsync(headBytes, 0, headBytes.Length, stopping.Token).ConfigureAwait(false);
            await stream.WriteAsync(body, 0, sentBytes ?? body.Length, stopping.Token).ConfigureAwait(false);
            await stream.FlushAsync(stopping.Token).ConfigureAwait(false);
        }

        // A versioned download's contents: a held range waits for its release, then a failing range fails; otherwise 412
        // unless If-Match names the current version, 206 with exactly the bytes a Range asks for, or 200 with all of them,
        // each with the version's entity tag.
        private async Task AnswerVersionedDownload(RecordedRequest request, Stream stream, string remote, VersionedDownload versioned)
        {
            string range = request.Header("Range");
            string[] bounds = range != null && range.StartsWith("bytes=", StringComparison.Ordinal) ? range.Substring("bytes=".Length).Split('-') : null;
            long start = bounds == null ? 0 : long.Parse(bounds[0], CultureInfo.InvariantCulture);
            if (bounds != null && rangeHolds.TryGetValue(RangeKey(remote, start), out DownloadHold hold))
            {
                hold.Reached.Set();
                await Task.Run(() => hold.Release.Wait(stopping.Token)).ConfigureAwait(false);
            }
            if (bounds != null && rangeFailures.TryGetValue(RangeKey(remote, start), out RangeFailure failure) && Interlocked.Decrement(ref failure.Remaining) >= 0)
            {
                await Write(stream, failure.Status, Encoding.UTF8.GetBytes("<Error>InternalError</Error>"), "application/xml", null).ConfigureAwait(false);
                return;
            }
            string tag;
            string modified;
            byte[] content;
            lock (versioned)
            {
                tag = versioned.EntityTag == null ? null : "\"" + versioned.EntityTag + "\"";
                content = downloads[remote];
                modified = ++versioned.Responses == 1 ? versioned.InitialDate : versioned.LaterDate;
                if (versioned.InitialDate != null && versioned.Responses > 1)
                {
                    content = content.Reverse().ToArray();
                }
            }
            string ifMatch = request.Header("If-Match");
            if (ifMatch != null && ifMatch != tag)
            {
                await Write(stream, 412, Encoding.UTF8.GetBytes("<Error>PreconditionFailed</Error>"), "application/xml", null).ConfigureAwait(false);
                return;
            }
            var headers = new Dictionary<string, string>();
            if (tag != null)
            {
                headers["ETag"] = tag;
            }
            if (modified != null)
            {
                headers["Last-Modified"] = modified;
            }
            if (bounds == null)
            {
                await Write(stream, 200, content, "application/octet-stream", headers).ConfigureAwait(false);
                return;
            }
            long end = Math.Min(long.Parse(bounds[1], CultureInfo.InvariantCulture), content.Length - 1);
            byte[] part = new byte[end - start + 1];
            Array.Copy(content, start, part, 0, part.Length);
            headers["Content-Range"] = $"bytes {start}-{end}/{content.Length}";
            versioned.Ranges.Enqueue($"{start}-{end}");
            await Write(stream, 206, part, "application/octet-stream", headers).ConfigureAwait(false);
        }

        private static string Reason(int status)
        {
            return status switch { 200 => "OK", 204 => "No Content", 206 => "Partial Content", 412 => "Precondition Failed", 400 => "Bad Request", 401 => "Unauthorized", 403 => "Forbidden", 404 => "Not Found", 407 => "Proxy Authentication Required", 422 => "Unprocessable Entity", 429 => "Too Many Requests", 500 => "Internal Server Error", 503 => "Service Unavailable", _ => "Status" };
        }

        private static string Json(string text)
        {
            return "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        private sealed class Answer
        {
            internal int Status;
            internal string Body;
            internal Dictionary<string, string> Headers;
            internal int Delay;
            internal bool Hang;
            internal bool Drop;
        }

        private sealed class RouteQueue
        {
            private readonly Queue<Answer> answers = new Queue<Answer>();
            private Answer last;

            internal string[] Pages;

            internal void Add(Answer answer)
            {
                lock (answers)
                {
                    answers.Enqueue(answer);
                }
            }

            internal Answer Next()
            {
                lock (answers)
                {
                    if (answers.Count > 0)
                    {
                        last = answers.Dequeue();
                    }
                    return last;
                }
            }
        }

        private sealed class VersionedDownload
        {
            internal string EntityTag;
            internal string InitialDate;
            internal string LaterDate;
            internal int Responses;

            internal ConcurrentQueue<string> Ranges { get; } = new ConcurrentQueue<string>();
        }

        private sealed class DownloadHold
        {
            internal int ByteCount;
            internal readonly ManualResetEventSlim Reached = new ManualResetEventSlim(false);
            internal readonly ManualResetEventSlim Release = new ManualResetEventSlim(false);
        }

        private sealed class RangeFailure
        {
            internal int Status;
            internal int Remaining;
        }

        // S3's answer to a presigned URL used after it expired.
        private const string ExpiredPresignedUrl = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<Error><Code>AccessDenied</Code><Message>Request has expired</Message><X-Amz-Expires>900</X-Amz-Expires><Expires>2026-09-27T09:00:00Z</Expires><ServerTime>2026-09-27T09:15:01Z</ServerTime><RequestId>0000000000000000</RequestId><HostId>test-service</HostId></Error>";

        private sealed class UploadState
        {
            internal long PartSize;
            internal bool ParallelParts;
            internal int FinalStatus;
            internal int PartDelay;
            internal int Active;
            internal int Peak;
            internal bool S3;
            internal long? AvailableParts;
            internal readonly string UploadId = Guid.NewGuid().ToString("N");
            internal readonly Dictionary<long, byte[]> Parts = new Dictionary<long, byte[]>();
            internal readonly Dictionary<long, List<string>> Attempts = new Dictionary<long, List<string>>();
            internal readonly ConcurrentDictionary<long, int> PartFailures = new ConcurrentDictionary<long, int>();
            internal readonly ConcurrentDictionary<long, bool> PartStalls = new ConcurrentDictionary<long, bool>();
            internal readonly ConcurrentDictionary<long, bool> ExpiringParts = new ConcurrentDictionary<long, bool>();
            internal readonly ConcurrentDictionary<long, int> Grants = new ConcurrentDictionary<long, int>();
        }
    }
}
