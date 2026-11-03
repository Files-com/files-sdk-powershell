using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FilesCom.PowerShell.Tests
{
    // Remote folders for directory downloads: each folder's listing, as Files.com lists a folder, and each file's details
    // and bytes. The bytes come from storage that keeps versions, as ServeVersionedDownload's does, and can fail or hold a
    // file's download from a given byte on, so that a test can interrupt a download partway and see what a later run
    // fetches again. A client of the in-memory handler is given that storage at S3Host, from which the engine binds
    // each download to a version; a client over a socket, on a second loopback port, from which it fetches files whole.
    public sealed partial class FilesTestService
    {
        private readonly ConcurrentDictionary<string, TreeFile> treeFiles = new ConcurrentDictionary<string, TreeFile>(StringComparer.Ordinal);
        private readonly object treeStorageLock = new object();
        private string treeStorageUrl;
        private int treeDelay;
        private int treeActive;
        private int treePeak;

        /// <summary>
        /// Serves the remote folder folderPath, "" for the site's root, and everything in it. files maps each file's path
        /// below the folder, with / between names, to its bytes; emptyFolders names folders below it with nothing inside.
        /// Each folder is listed at GET /folders/{path}, folders and files in path order, pageSize to a page, and each
        /// file's details at GET /files/{path}. Its storage answers with the strong entity tag "v1", a Range request with
        /// 206, its Content-Range and exactly those bytes, and a request whose If-Match is another tag with 412.
        /// </summary>
        public void ServeTree(string folderPath, IDictionary files, string[] emptyFolders = null, int pageSize = 1000)
        {
            var entries = new SortedDictionary<string, SortedDictionary<string, string>>(StringComparer.Ordinal) { [folderPath] = new SortedDictionary<string, string>(StringComparer.Ordinal) };
            foreach (DictionaryEntry file in files)
            {
                string path = TreePath(folderPath, (string)file.Key);
                byte[] content = (byte[])file.Value;
                var served = new TreeFile { Content = content, EntityTag = "v1" };
                treeFiles[path] = served;
                AddToTreeListing(entries, folderPath, path, $"{{\"path\":{Json(path)},\"display_name\":{Json(TreeName(path))},\"type\":\"file\",\"size\":{content.Length}}}");
            }
            foreach (string folder in emptyFolders ?? Array.Empty<string>())
            {
                string path = TreePath(folderPath, folder);
                entries[path] = entries.TryGetValue(path, out SortedDictionary<string, string> existing) ? existing : new SortedDictionary<string, string>(StringComparer.Ordinal);
                AddToTreeListing(entries, folderPath, path, null);
            }
            foreach (KeyValuePair<string, SortedDictionary<string, string>> folder in entries)
            {
                string[] listing = folder.Value.Values.ToArray();
                var pages = new List<string>();
                for (int start = 0; start == 0 || start < listing.Length; start += pageSize)
                {
                    pages.Add("[" + string.Join(",", listing.Skip(start).Take(pageSize)) + "]");
                }
                Pages("/folders/" + folder.Key, pages.ToArray());
            }
        }

        /// <summary>
        /// Answers every Range request for remotePath's bytes that starts at fromByte or later with status, and no bytes,
        /// until <see cref="ClearTreeFailure"/>; earlier ranges are answered as usual.
        /// </summary>
        public void FailTreeDownload(string remotePath, long fromByte, int status)
        {
            TreeFile file = treeFiles[remotePath];
            file.FailStatus = status;
            Volatile.Write(ref file.FailFrom, fromByte);
        }

        /// <summary>Answers remotePath's bytes as usual again after <see cref="FailTreeDownload"/>.</summary>
        public void ClearTreeFailure(string remotePath)
        {
            Volatile.Write(ref treeFiles[remotePath].FailFrom, long.MaxValue);
        }

        /// <summary>
        /// Holds the next Range request for remotePath's bytes that starts at atByte open, unanswered, until its client
        /// leaves; <see cref="IsTreeDownloadHeld"/> tells when it has arrived. Later requests are answered as usual.
        /// </summary>
        public void HoldTreeDownload(string remotePath, long atByte)
        {
            Volatile.Write(ref treeFiles[remotePath].HoldAt, atByte);
        }

        /// <summary>Whether the request <see cref="HoldTreeDownload"/> holds has arrived.</summary>
        public bool IsTreeDownloadHeld(string remotePath)
        {
            return treeFiles[remotePath].Held.IsSet;
        }

        /// <summary>Holds every Range request for a file's bytes for the given time before answering it, so that requests overlap.</summary>
        public void DelayTreeDownloads(int delayMilliseconds)
        {
            Volatile.Write(ref treeDelay, delayMilliseconds);
        }

        /// <summary>The most Range requests, of every file together, the storage answered at the same time since the last call.</summary>
        public int TakeTreeDownloadPeak()
        {
            return Interlocked.Exchange(ref treePeak, 0);
        }

        /// <summary>The ranges of remotePath's bytes the storage answered with 206, as "start-end", in the order answered.</summary>
        public IReadOnlyList<string> TreeRangesServed(string remotePath)
        {
            return treeFiles.TryGetValue(remotePath, out TreeFile file) ? file.Ranges.ToArray() : Array.Empty<string>();
        }

        // A served file's details, whose download URL names the storage the requesting client reaches.
        private string TreeFileDetails(RecordedRequest request, string path, TreeFile file)
        {
            string resource = "tree/" + Uri.EscapeDataString(path);
            string address = request.InMemory ? S3PresignedUrl(resource, "", DownloadSignature) : $"{TreeStorage()}{resource}?X-Files-Signature={DownloadSignature}";
            return $"{{\"path\":{Json(path)},\"display_name\":{Json(TreeName(path))},\"type\":\"file\",\"size\":{file.Content.Length},\"download_uri\":{Json(address)}}}";
        }

        // The storage's loopback address, starting it the first time.
        private string TreeStorage()
        {
            lock (treeStorageLock)
            {
                if (treeStorageUrl == null)
                {
                    var storage = new TcpListener(IPAddress.Loopback, 0);
                    storage.Start();
                    stopping.Token.Register(storage.Stop);
                    treeStorageUrl = $"http://127.0.0.1:{((IPEndPoint)storage.LocalEndpoint).Port}/";
                    _ = AcceptTreeStorage(storage);
                }
                return treeStorageUrl;
            }
        }

        private static string TreePath(string folderPath, string relativePath)
        {
            return folderPath.Length == 0 ? relativePath : folderPath + "/" + relativePath;
        }

        private static string TreeName(string path)
        {
            return path.Substring(path.LastIndexOf('/') + 1);
        }

        // Lists path in its folder, and each folder above it, up to the served folder, in the folder above that.
        private static void AddToTreeListing(SortedDictionary<string, SortedDictionary<string, string>> entries, string root, string path, string entry)
        {
            while (path != root)
            {
                int slash = path.LastIndexOf('/');
                string parent = slash < 0 ? "" : path.Substring(0, slash);
                if (!entries.TryGetValue(parent, out SortedDictionary<string, string> listing))
                {
                    entries[parent] = listing = new SortedDictionary<string, string>(StringComparer.Ordinal);
                }
                listing[path] = entry ?? $"{{\"path\":{Json(path)},\"display_name\":{Json(TreeName(path))},\"type\":\"directory\"}}";
                entry = null;
                path = parent;
            }
        }

        private async Task AcceptTreeStorage(TcpListener storage)
        {
            while (!stopping.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    // Stopping the service stops the listener, which ends a pending accept.
                    client = await storage.AcceptTcpClientAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    return;
                }
                _ = Task.Run(() => ServeTreeStorage(client));
            }
        }

        // One storage connection, which carries requests one after another until either side closes it. Its requests are
        // recorded with the service's others.
        private async Task ServeTreeStorage(TcpClient client)
        {
            Interlocked.Increment(ref openConnections);
            try
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
                            if (!await AnswerTreeStorage(request, stream).ConfigureAwait(false))
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
            finally
            {
                Interlocked.Decrement(ref openConnections);
            }
        }

        // A file's bytes: 412 unless If-Match names its version, 206 with exactly the bytes a Range asks for, or 200 with
        // all of them, each with the version's entity tag; failed or held as the test asked. False closes the connection.
        private async Task<bool> AnswerTreeStorage(RecordedRequest request, Stream stream)
        {
            const string prefix = "/tree/";
            if (!request.Path.StartsWith(prefix, StringComparison.Ordinal) || !treeFiles.TryGetValue(Uri.UnescapeDataString(request.Path.Substring(prefix.Length)), out TreeFile file))
            {
                await Write(stream, 404, Encoding.UTF8.GetBytes("<Error>NoSuchKey</Error>"), "application/xml", null).ConfigureAwait(false);
                return true;
            }
            string tag = "\"" + file.EntityTag + "\"";
            string ifMatch = request.Header("If-Match");
            if (ifMatch != null && ifMatch != tag)
            {
                await Write(stream, 412, Encoding.UTF8.GetBytes("<Error>PreconditionFailed</Error>"), "application/xml", null).ConfigureAwait(false);
                return true;
            }
            var headers = new Dictionary<string, string> { ["ETag"] = tag };
            string range = request.Header("Range");
            if (range == null || !range.StartsWith("bytes=", StringComparison.Ordinal))
            {
                await Write(stream, 200, file.Content, "application/octet-stream", headers).ConfigureAwait(false);
                return true;
            }
            string[] bounds = range.Substring("bytes=".Length).Split('-');
            long start = long.Parse(bounds[0], CultureInfo.InvariantCulture);
            long end = Math.Min(long.Parse(bounds[1], CultureInfo.InvariantCulture), file.Content.Length - 1);
            if (Interlocked.CompareExchange(ref file.HoldAt, long.MaxValue, start) == start)
            {
                file.Held.Set();
                await WaitForClientToLeave(request, stream).ConfigureAwait(false);
                return false;
            }
            if (start >= Volatile.Read(ref file.FailFrom))
            {
                await Write(stream, file.FailStatus, Encoding.UTF8.GetBytes("<Error>InternalError</Error>"), "application/xml", null).ConfigureAwait(false);
                return true;
            }
            RaisePeak(ref treePeak, Interlocked.Increment(ref treeActive));
            try
            {
                int delay = Volatile.Read(ref treeDelay);
                if (delay > 0)
                {
                    await Task.Delay(delay, stopping.Token).ConfigureAwait(false);
                }
                byte[] part = new byte[end - start + 1];
                Array.Copy(file.Content, start, part, 0, part.Length);
                headers["Content-Range"] = $"bytes {start}-{end}/{file.Content.Length}";
                file.Ranges.Enqueue($"{start}-{end}");
                await Write(stream, 206, part, "application/octet-stream", headers).ConfigureAwait(false);
                return true;
            }
            finally
            {
                Interlocked.Decrement(ref treeActive);
            }
        }

        private sealed class TreeFile
        {
            internal byte[] Content;
            internal string EntityTag;
            internal int FailStatus;
            internal long FailFrom = long.MaxValue;
            internal long HoldAt = long.MaxValue;
            internal readonly ManualResetEventSlim Held = new ManualResetEventSlim(false);
            internal readonly ConcurrentQueue<string> Ranges = new ConcurrentQueue<string>();
        }
    }
}
