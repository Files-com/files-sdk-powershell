using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// Support for the API scenarios consumer (run.ps1 and controls.ps1), compiled by build/Invoke-ApiScenarios.ps1 before
// any scenario starts. None of it takes the place of the module or the SDK: the handler only forwards what the module
// sends to a real network handler and notes what it saw, and the streams and recipes only supply a scenario's bytes.
namespace FilesCom.ApiScenarios
{
    // One request the module sent through the handler it was given with Connect-FilesAccount -HttpMessageHandler, as the
    // handler saw it. It holds no credential, body, query value or header but User-Agent. A storage address is kept in
    // memory only, to join the request to the simulator's journal; run.ps1 writes only its digest.
    public sealed class TransportObservation
    {
        public long Sequence { get; internal set; }

        // The scenario position (or cleanup or control) run.ps1 was running when the request started.
        public string Step { get; internal set; }

        public string Method { get; internal set; }

        // Whether the request went to the simulator the run was given, and no other address.
        public bool SameOrigin { get; internal set; }

        // The request's path, percent-decoded, and as sent.
        public string Path { get; internal set; }

        public string RawPath { get; internal set; }

        // api for a Files.com API path, storage for any other path at the simulator (its upload and download addresses).
        public string Kind { get; internal set; }

        public string[] QueryNames { get; internal set; }

        // api-key when the request carried exactly the run's API key, and none when it carried no credential; otherwise
        // what it carried (other-api-key, session, authorization), joined with +.
        public string Credential { get; internal set; }

        public string UserAgent { get; internal set; }

        public long? RequestBytes { get; internal set; }

        public int? Status { get; internal set; }

        // in-flight, responded, failed or cancelled; held-cancelled-unsent or held-expired-unsent for the request a
        // boundary held, which never reached the network.
        public string Outcome { get; internal set; }

        // The type of the exception a request that was not answered ended with.
        public string Error { get; internal set; }

        // The part number of a storage upload, from the last segment of its address.
        public int? StoragePart { get; internal set; }

        public double StartedMilliseconds { get; internal set; }

        public double EndedMilliseconds { get; internal set; }
    }

    // What a first-acknowledged-part boundary saw: the storage part whose successful answer armed it, and the next request,
    // which it held before that request reached the network, whether that request saw its cancellation, and how it ended.
    public sealed class BoundaryReport
    {
        public bool Acknowledged { get; internal set; }

        public long? AcknowledgedSequence { get; internal set; }

        public int? AcknowledgedPart { get; internal set; }

        public int? AcknowledgedStatus { get; internal set; }

        public bool Held { get; internal set; }

        public long? HeldSequence { get; internal set; }

        public string HeldMethod { get; internal set; }

        public string HeldKind { get; internal set; }

        public string HeldOutcome { get; internal set; }

        public double? HoldMilliseconds { get; internal set; }

        // Whether the held request saw its cancellation.
        public bool Cancelled { get; internal set; }

        // For a boundary that holds completion, whether ReleaseCompletion (true) or the hold limit (false) let the cancelled
        // request complete; null until it has, and for any other boundary.
        public bool? CompletionReleased { get; internal set; }
    }

    // A DelegatingHandler for Connect-FilesAccount -HttpMessageHandler. Every request the module's connection sends, to
    // the API and to storage, passes through it to a SocketsHttpHandler that follows no redirect, keeps no cookie and uses
    // no proxy, and this handler never retries or answers a request itself. It notes each request and its answer.
    //
    // Armed with ArmFirstAcknowledgedPartBoundary, it holds the first request that starts after a storage upload part has
    // been answered with success, before that request is sent, and signals HeldWaitHandle. In a serial upload the next
    // request starts only once the part before it was acknowledged, so the hold shows the first part's acknowledgement
    // reached the upload, and that nothing after it has been sent. The held request then waits for its own cancellation,
    // which the command's StopProcessing gives it, signals CancelledWaitHandle and ends unsent; if nothing cancels it
    // within the hold limit, it ends unsent with an HttpRequestException, which is never a cancellation.
    //
    // Armed to hold completion too, the cancelled request does not end at once: it stays unfinished, and so does the
    // command waiting for it, until ReleaseCompletion or DisarmBoundary lets it complete, or the hold limit has passed
    // since the hold began. A control can so look at a stopped command that has not settled, then let it settle.
    public sealed class ObservingHandler : DelegatingHandler
    {
        private readonly object gate = new object();
        private readonly List<TransportObservation> observations = new List<TransportObservation>();
        private readonly Stopwatch clock = Stopwatch.StartNew();
        private readonly Uri server;
        private readonly string apiPrefix;
        private readonly string expectedKeySha256;
        private readonly int limit;
        private long sequence;
        private int dropped;
        private string step = "";
        private RequestBoundary boundary;

        public ObservingHandler(Uri server, string apiPrefix, string expectedKeySha256, int limit)
            : base(new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                UseCookies = false,
                UseProxy = false,
                AutomaticDecompression = DecompressionMethods.None,
            })
        {
            if (server == null || !server.IsAbsoluteUri)
            {
                throw new ArgumentException("The simulator's address must be absolute.", nameof(server));
            }
            if (string.IsNullOrEmpty(apiPrefix) || !apiPrefix.StartsWith("/", StringComparison.Ordinal))
            {
                throw new ArgumentException("The API path prefix must start with /.", nameof(apiPrefix));
            }
            if (limit < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(limit));
            }
            this.server = server;
            this.apiPrefix = apiPrefix.TrimEnd('/');
            this.expectedKeySha256 = expectedKeySha256 ?? "";
            this.limit = limit;
        }

        public string Step
        {
            get { return Volatile.Read(ref step); }
            set { Volatile.Write(ref step, value ?? ""); }
        }

        // How many requests have started, including any past the observation limit.
        public long Sent
        {
            get { return Interlocked.Read(ref sequence); }
        }

        // How many requests started after the observation limit was reached, and so were not kept.
        public int Dropped
        {
            get
            {
                lock (gate)
                {
                    return dropped;
                }
            }
        }

        public int Limit
        {
            get { return limit; }
        }

        public TransportObservation[] All()
        {
            lock (gate)
            {
                return observations.ToArray();
            }
        }

        // The requests that started after the one numbered afterSequence, in the order they started.
        public TransportObservation[] Since(long afterSequence)
        {
            lock (gate)
            {
                return observations.Where(observation => observation.Sequence > afterSequence).ToArray();
            }
        }

        public void ArmFirstAcknowledgedPartBoundary(TimeSpan holdLimit)
        {
            ArmFirstAcknowledgedPartBoundary(holdLimit, false);
        }

        // With holdCompletion, the held request, once cancelled, stays unfinished until released, within the hold limit.
        public void ArmFirstAcknowledgedPartBoundary(TimeSpan holdLimit, bool holdCompletion)
        {
            if (holdLimit <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(holdLimit));
            }
            lock (gate)
            {
                if (boundary != null)
                {
                    throw new InvalidOperationException("A request boundary is already armed.");
                }
                boundary = new RequestBoundary(holdLimit, holdCompletion);
            }
        }

        // Set once the armed boundary holds a request.
        public WaitHandle HeldWaitHandle
        {
            get { return Armed().Held.WaitHandle; }
        }

        // Set once the request the armed boundary holds has seen its cancellation.
        public WaitHandle CancelledWaitHandle
        {
            get { return Armed().Cancelled.WaitHandle; }
        }

        // Lets the cancelled request whose completion the armed boundary holds complete, now or once it is cancelled.
        public void ReleaseCompletion()
        {
            Armed().Release();
        }

        // Disarms the boundary, letting a request whose completion it holds complete, and reports what it saw; null when
        // none was armed.
        public BoundaryReport DisarmBoundary()
        {
            RequestBoundary current;
            lock (gate)
            {
                current = boundary;
                boundary = null;
            }
            if (current == null)
            {
                return null;
            }
            current.Release();
            return current.Report();
        }

        private RequestBoundary Armed()
        {
            RequestBoundary current = Volatile.Read(ref boundary);
            if (current == null)
            {
                throw new InvalidOperationException("No request boundary is armed.");
            }
            return current;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            TransportObservation observation = Begin(request);
            RequestBoundary current = Volatile.Read(ref boundary);
            if (current != null && current.TryHold(observation))
            {
                Stopwatch held = Stopwatch.StartNew();
                try
                {
                    await Task.Delay(current.HoldLimit, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // A boundary that holds completion keeps the cancelled request, and so the command waiting for it,
                    // unfinished until released, for at most what is left of the hold limit.
                    await current.SeeCancellation(current.HoldLimit - held.Elapsed).ConfigureAwait(false);
                    Finish(observation, null, "held-cancelled-unsent", null);
                    current.Settle("held-cancelled-unsent", held.Elapsed);
                    throw;
                }
                Finish(observation, null, "held-expired-unsent", nameof(HttpRequestException));
                current.Settle("held-expired-unsent", held.Elapsed);
                throw new HttpRequestException(
                    "The API scenarios boundary held this request for "
                    + current.HoldLimit.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture)
                    + " s after the upload's first acknowledged part, and nothing cancelled it; it was not sent.");
            }
            try
            {
                HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                Finish(observation, (int)response.StatusCode, "responded", null);
                if (current != null)
                {
                    current.Observe(observation);
                }
                return response;
            }
            catch (OperationCanceledException failure)
            {
                Finish(observation, null, "cancelled", failure.GetType().Name);
                throw;
            }
            catch (Exception failure)
            {
                Finish(observation, null, "failed", failure.GetType().Name);
                throw;
            }
        }

        private TransportObservation Begin(HttpRequestMessage request)
        {
            Uri address = request.RequestUri;
            string rawPath = address.AbsolutePath;
            string path = Uri.UnescapeDataString(rawPath);
            var observation = new TransportObservation
            {
                Step = Step,
                Method = request.Method.Method,
                SameOrigin = address.IsAbsoluteUri
                    && string.Equals(address.Scheme, server.Scheme, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(address.Host, server.Host, StringComparison.OrdinalIgnoreCase)
                    && address.Port == server.Port,
                RawPath = rawPath,
                Path = path,
                Kind = path.StartsWith(apiPrefix + "/", StringComparison.Ordinal) ? "api" : "storage",
                QueryNames = QueryNames(address.Query),
                Credential = Credential(request),
                UserAgent = UserAgent(request),
                RequestBytes = request.Content == null ? null : request.Content.Headers.ContentLength,
                StartedMilliseconds = clock.Elapsed.TotalMilliseconds,
                Outcome = "in-flight",
            };
            if (observation.Kind == "storage" && observation.Method == "PUT")
            {
                string last = path.Substring(path.LastIndexOf('/') + 1);
                int part;
                if (int.TryParse(last, NumberStyles.None, CultureInfo.InvariantCulture, out part))
                {
                    observation.StoragePart = part;
                }
            }
            lock (gate)
            {
                observation.Sequence = Interlocked.Increment(ref sequence);
                if (observations.Count < limit)
                {
                    observations.Add(observation);
                }
                else
                {
                    dropped++;
                }
            }
            return observation;
        }

        private void Finish(TransportObservation observation, int? status, string outcome, string error)
        {
            lock (gate)
            {
                observation.Status = status;
                observation.Outcome = outcome;
                observation.Error = error;
                observation.EndedMilliseconds = clock.Elapsed.TotalMilliseconds;
            }
        }

        private string Credential(HttpRequestMessage request)
        {
            var kinds = new List<string>();
            IEnumerable<string> values;
            if (request.Headers.TryGetValues("X-FilesAPI-Key", out values))
            {
                foreach (string value in values)
                {
                    kinds.Add(Recipe.Sha256(Encoding.UTF8.GetBytes(value ?? "")) == expectedKeySha256 ? "api-key" : "other-api-key");
                }
            }
            if (request.Headers.Contains("X-FilesAPI-Auth"))
            {
                kinds.Add("session");
            }
            if (request.Headers.Authorization != null)
            {
                kinds.Add("authorization");
            }
            return kinds.Count == 0 ? "none" : string.Join("+", kinds);
        }

        private static string UserAgent(HttpRequestMessage request)
        {
            string agent = request.Headers.UserAgent.ToString();
            IEnumerable<string> values;
            if (string.IsNullOrEmpty(agent) && request.Headers.TryGetValues("User-Agent", out values))
            {
                agent = string.Join(" ", values);
            }
            return agent.Length > 256 ? agent.Substring(0, 256) : agent;
        }

        // The names of a query's parameters, at most 16 of them, without their values.
        private static string[] QueryNames(string query)
        {
            if (string.IsNullOrEmpty(query) || query == "?")
            {
                return new string[0];
            }
            return query.TrimStart('?')
                .Split('&')
                .Where(pair => pair.Length > 0)
                .Select(pair => Uri.UnescapeDataString(pair.Split('=')[0].Replace('+', ' ')))
                .Take(16)
                .ToArray();
        }
    }

    internal sealed class RequestBoundary
    {
        private readonly object gate = new object();
        private readonly bool holdsCompletion;
        private readonly TaskCompletionSource<bool> release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private TransportObservation acknowledged;
        private TransportObservation held;
        private string heldOutcome;
        private double? holdMilliseconds;
        private bool? completionReleased;

        internal RequestBoundary(TimeSpan holdLimit, bool holdsCompletion)
        {
            HoldLimit = holdLimit;
            this.holdsCompletion = holdsCompletion;
            Held = new ManualResetEventSlim(false);
            Cancelled = new ManualResetEventSlim(false);
        }

        internal TimeSpan HoldLimit { get; }

        internal ManualResetEventSlim Held { get; }

        internal ManualResetEventSlim Cancelled { get; }

        // Signals that the held request has seen its cancellation. A boundary that holds completion then waits until
        // Release, or until remaining has passed, and notes which let the request complete.
        internal async Task SeeCancellation(TimeSpan remaining)
        {
            Cancelled.Set();
            if (!holdsCompletion)
            {
                return;
            }
            Task released = release.Task;
            if (!released.IsCompleted && remaining > TimeSpan.Zero)
            {
                using (var expiry = new CancellationTokenSource())
                {
                    await Task.WhenAny(released, Task.Delay(remaining, expiry.Token)).ConfigureAwait(false);
                    expiry.Cancel();
                }
            }
            lock (gate)
            {
                completionReleased = released.IsCompleted;
            }
        }

        internal void Release()
        {
            release.TrySetResult(true);
        }

        // Notes a storage upload part answered with success; the first one arms the hold.
        internal void Observe(TransportObservation observation)
        {
            lock (gate)
            {
                if (acknowledged == null && observation.Kind == "storage" && observation.Method == "PUT" && observation.Status >= 200 && observation.Status < 300)
                {
                    acknowledged = observation;
                }
            }
        }

        // Claims the first request that starts after the acknowledged part.
        internal bool TryHold(TransportObservation observation)
        {
            lock (gate)
            {
                if (acknowledged == null || held != null)
                {
                    return false;
                }
                held = observation;
            }
            Held.Set();
            return true;
        }

        internal void Settle(string outcome, TimeSpan duration)
        {
            lock (gate)
            {
                heldOutcome = outcome;
                holdMilliseconds = Math.Round(duration.TotalMilliseconds, 1);
            }
        }

        internal BoundaryReport Report()
        {
            lock (gate)
            {
                return new BoundaryReport
                {
                    Acknowledged = acknowledged != null,
                    AcknowledgedSequence = acknowledged == null ? (long?)null : acknowledged.Sequence,
                    AcknowledgedPart = acknowledged == null ? null : acknowledged.StoragePart,
                    AcknowledgedStatus = acknowledged == null ? null : acknowledged.Status,
                    Held = held != null,
                    HeldSequence = held == null ? (long?)null : held.Sequence,
                    HeldMethod = held == null ? null : held.Method,
                    HeldKind = held == null ? null : held.Kind,
                    HeldOutcome = heldOutcome,
                    HoldMilliseconds = holdMilliseconds,
                    Cancelled = Cancelled.IsSet,
                    CompletionReleased = completionReleased,
                };
            }
        }
    }

    // A stream that can only be read forward, giving at most MaxRead bytes from each read, as a pipe or a program's
    // output does. Length, Position, Seek and writing are not supported and counted when tried.
    public sealed class ForwardOnlyStream : Stream
    {
        private readonly object gate = new object();
        private readonly byte[] bytes;
        private readonly int maxRead;
        private long position;
        private long reads;
        private long endReads;
        private int maxReturned;
        private int unsupportedCalls;

        public ForwardOnlyStream(byte[] bytes, int maxRead)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            if (maxRead < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRead));
            }
            this.bytes = bytes;
            this.maxRead = maxRead;
        }

        public int MaxRead
        {
            get { return maxRead; }
        }

        public long BytesRead
        {
            get
            {
                lock (gate)
                {
                    return position;
                }
            }
        }

        public long Reads
        {
            get
            {
                lock (gate)
                {
                    return reads;
                }
            }
        }

        // Reads that found the end: asked for bytes and got none.
        public long EndReads
        {
            get
            {
                lock (gate)
                {
                    return endReads;
                }
            }
        }

        public int MaxReturned
        {
            get
            {
                lock (gate)
                {
                    return maxReturned;
                }
            }
        }

        public int UnsupportedCalls
        {
            get { return Volatile.Read(ref unsupportedCalls); }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { throw Unsupported("Length"); }
        }

        public override long Position
        {
            get { throw Unsupported("Position"); }
            set { throw Unsupported("Position"); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            return ReadInto(new Span<byte>(buffer, offset, count));
        }

        public override int Read(Span<byte> buffer)
        {
            return ReadInto(buffer);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Read(buffer, offset, count));
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new ValueTask<int>(ReadInto(buffer.Span));
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw Unsupported("Seek");
        }

        public override void SetLength(long value)
        {
            throw Unsupported("SetLength");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw Unsupported("Write");
        }

        private int ReadInto(Span<byte> destination)
        {
            lock (gate)
            {
                int remaining = (int)(bytes.Length - position);
                int count = Math.Min(Math.Min(destination.Length, maxRead), remaining);
                new ReadOnlySpan<byte>(bytes, (int)position, count).CopyTo(destination);
                position += count;
                reads++;
                if (count == 0 && destination.Length > 0)
                {
                    endReads++;
                }
                if (count > maxReturned)
                {
                    maxReturned = count;
                }
                return count;
            }
        }

        private NotSupportedException Unsupported(string member)
        {
            Interlocked.Increment(ref unsupportedCalls);
            return new NotSupportedException("A forward-only stream does not support " + member + ".");
        }
    }

    // A scenario fixture's bytes: (index * multiplier + offset) % modulus for each index below length, as
    // verification/scenarios/api.json defines its recipes.
    public static class Recipe
    {
        public static byte[] Bytes(long multiplier, long offset, long modulus, long length)
        {
            if (length < 0 || length > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            if (modulus < 1 || modulus > 256 || multiplier < 0 || offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(modulus));
            }
            var bytes = new byte[length];
            for (long index = 0; index < length; index++)
            {
                bytes[index] = (byte)(((index * multiplier) + offset) % modulus);
            }
            return bytes;
        }

        public static string Sha256(byte[] bytes)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return Hex(sha256.ComputeHash(bytes));
            }
        }

        public static string FileSha256(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            using (SHA256 sha256 = SHA256.Create())
            {
                return Hex(sha256.ComputeHash(stream));
            }
        }

        // Writes bytes to a new file; a file already at path is left alone and the write fails.
        public static void WriteNewFile(string path, byte[] bytes)
        {
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        private static string Hex(byte[] hash)
        {
            var text = new StringBuilder(hash.Length * 2);
            foreach (byte value in hash)
            {
                text.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            }
            return text.ToString();
        }
    }
}
