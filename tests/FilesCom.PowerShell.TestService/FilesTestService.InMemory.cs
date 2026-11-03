using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FilesCom.PowerShell.Tests
{
    // The service without its sockets: a terminal HttpMessageHandler, for Connect-FilesAccount -HttpMessageHandler, that
    // answers each request itself, with the same answers the service gives over a connection, and opens no connection,
    // resolves no name and negotiates no TLS. Through it the service also stands in for storage at an Amazon S3
    // general-purpose endpoint (S3Host): the download details, folder files and S3 uploads it serves name that storage
    // with presigned URLs of the form Files.com's storage URLs take, and it answers them as S3 does. Over a socket the
    // service only ever names its own addresses, so no client is sent to S3 itself. A request for any other host fails
    // as a name that does not resolve would, without leaving the process.
    public sealed partial class FilesTestService
    {
        /// <summary>The Amazon S3 endpoint the service stands in for through its in-memory handler.</summary>
        public const string S3Host = "files-test.s3.us-east-1.amazonaws.com";

        /// <summary>The query value of every download URL the service gives, which must never appear in output.</summary>
        public const string DownloadSignature = "sentinel-signed-url-secret";

        private const string S3Url = "https://" + S3Host + "/";

        /// <summary>
        /// A handler that sends every request of a connection to this service in memory, as the class remarks describe.
        /// It holds nothing to dispose.
        /// </summary>
        public HttpMessageHandler CreateHandler()
        {
            return new InMemoryHandler(this);
        }

        // A URL for resource at S3Host, presigned as AWS Signature Version 4 query authentication presigns one: signed
        // now, for 15 minutes, for the host header alone, with signature. parameters, if any, end with '&'.
        private static string S3PresignedUrl(string resource, string parameters, string signature)
        {
            DateTime now = DateTime.UtcNow;
            string credential = Uri.EscapeDataString("AKIAFILESTESTSERVICE/" + now.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "/us-east-1/s3/aws4_request");
            string signedAt = now.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
            return $"{S3Url}{resource}?{parameters}X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential={credential}&X-Amz-Date={signedAt}&X-Amz-Expires=900&X-Amz-SignedHeaders=host&X-Amz-Signature={signature}";
        }

        // One request through the in-memory handler, recorded and answered as one arriving over a connection is. At
        // S3Host only storage is served.
        private async Task AnswerInMemoryAsync(RecordedRequest request, Exchange exchange, bool storage)
        {
            lock (requests)
            {
                requests.Add(request);
            }
            Interlocked.Increment(ref activeRequests);
            try
            {
                Stream stream = exchange.ServiceSide;
                if (!storage)
                {
                    await AnswerAsync(request, stream).ConfigureAwait(false);
                }
                else if (request.Path.StartsWith("/tree/", StringComparison.Ordinal))
                {
                    await AnswerTreeStorage(request, stream).ConfigureAwait(false);
                }
                else if (request.Path.StartsWith("/download/", StringComparison.Ordinal) || request.Path.StartsWith("/upload/", StringComparison.Ordinal))
                {
                    await AnswerAsync(request, stream).ConfigureAwait(false);
                }
                else
                {
                    await Write(stream, 404, Encoding.UTF8.GetBytes("<Error>NoSuchKey</Error>"), "application/xml", null).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                request.ClientAborted = true;
                Interlocked.Increment(ref abortedRequests);
            }
            finally
            {
                Interlocked.Decrement(ref activeRequests);
                exchange.End();
            }
        }

        // How a request that reaches no answer fails, as the platform's own handler fails it: on .NET 8 and later with
        // the HttpRequestError it names; on .NET Framework, whose handler has none, as a plain HttpRequestException.
        private enum HttpRequestErrorName
        {
            NameResolution,
            ResponseEnded,
        }

        private static HttpRequestException Unreachable(HttpRequestErrorName error, string message)
        {
#if NET8_0_OR_GREATER
            return new HttpRequestException(error == HttpRequestErrorName.NameResolution ? HttpRequestError.NameResolutionError : HttpRequestError.ResponseEnded, message);
#else
            return new HttpRequestException(message);
#endif
        }

        // Completes when task does, or fails as cancelled when cancellationToken is cancelled first.
        private static async Task WhenDone(Task task, CancellationToken cancellationToken)
        {
            var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (cancellationToken.Register(() => cancelled.TrySetCanceled(cancellationToken)))
            {
                await (await Task.WhenAny(task, cancelled.Task).ConfigureAwait(false)).ConfigureAwait(false);
            }
        }

        private static byte[] Copy(byte[] source, int offset, int count)
        {
            var copy = new byte[count];
            Buffer.BlockCopy(source, offset, copy, 0, count);
            return copy;
        }

        private sealed class InMemoryHandler : HttpMessageHandler
        {
            private readonly FilesTestService service;

            internal InMemoryHandler(FilesTestService service)
            {
                this.service = service;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken cancellationToken)
            {
                Uri uri = message.RequestUri;
                string origin = uri.GetLeftPart(UriPartial.Authority) + "/";
                bool storage = string.Equals(origin, S3Url, StringComparison.OrdinalIgnoreCase);
                if (!storage && !string.Equals(origin, service.BaseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    throw Unreachable(HttpRequestErrorName.NameResolution, $"The in-memory service answers only {service.BaseUrl} and {S3Url}, not {origin}.");
                }
                RecordedRequest request = await RecordAsync(message).ConfigureAwait(false);
                var exchange = new Exchange();
                _ = service.AnswerInMemoryAsync(request, exchange, storage);
                try
                {
                    return await ReadResponseAsync(message, exchange, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    exchange.Leave();
                    throw;
                }
            }

            // The request as the service records one read from a connection: its target, its headers with Host, and its
            // body, sent with Content-Length when its length is known, chunked otherwise.
            private static async Task<RecordedRequest> RecordAsync(HttpRequestMessage message)
            {
                var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["Host"] = message.RequestUri.Authority };
                foreach (KeyValuePair<string, IEnumerable<string>> header in message.Headers)
                {
                    headers[header.Key] = string.Join(header.Key == "User-Agent" ? " " : ", ", header.Value);
                }
                byte[] body = Array.Empty<byte>();
                if (message.Content != null)
                {
                    foreach (KeyValuePair<string, IEnumerable<string>> header in message.Content.Headers)
                    {
                        headers[header.Key] = string.Join(", ", header.Value);
                    }
                    long? length = message.Content.Headers.ContentLength;
                    if (length.HasValue)
                    {
                        headers["Content-Length"] = length.Value.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        headers["Transfer-Encoding"] = "chunked";
                    }
                    body = await message.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                }
                return new RecordedRequest(message.Method.Method, message.RequestUri.PathAndQuery, headers, body) { InMemory = true };
            }

            // The response the service writes: its head once it has arrived, and its body as it arrives.
            private static async Task<HttpResponseMessage> ReadResponseAsync(HttpRequestMessage message, Exchange exchange, CancellationToken cancellationToken)
            {
                var received = new MemoryStream();
                var buffer = new byte[16 * 1024];
                int headEnd;
                while ((headEnd = HeadEnd(received)) < 0)
                {
                    int read = await exchange.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                    if (read == 0)
                    {
                        throw Unreachable(HttpRequestErrorName.ResponseEnded, "The service closed the connection without answering.");
                    }
                    received.Write(buffer, 0, read);
                }
                byte[] bytes = received.ToArray();
                string[] lines = Encoding.ASCII.GetString(bytes, 0, headEnd).Split(new[] { "\r\n" }, StringSplitOptions.None);
                var response = new HttpResponseMessage((HttpStatusCode)int.Parse(lines[0].Split(' ')[1], CultureInfo.InvariantCulture)) { RequestMessage = message, Version = HttpVersion.Version11 };
                var fields = lines.Skip(1).Select(line => (Name: line.Substring(0, line.IndexOf(':')), Value: line.Substring(line.IndexOf(':') + 1).Trim())).ToList();
                long length = long.Parse(fields.First(field => string.Equals(field.Name, "Content-Length", StringComparison.OrdinalIgnoreCase)).Value, CultureInfo.InvariantCulture);
                var content = new StreamContent(new ResponseBody(exchange, Copy(bytes, headEnd + 4, bytes.Length - headEnd - 4), length));
                foreach ((string name, string value) in fields)
                {
                    if (!response.Headers.TryAddWithoutValidation(name, value))
                    {
                        content.Headers.TryAddWithoutValidation(name, value);
                    }
                }
                response.Content = content;
                return response;
            }

            private static int HeadEnd(MemoryStream received)
            {
                byte[] bytes = received.GetBuffer();
                for (int i = 0; i + 3 < received.Length; i++)
                {
                    if (bytes[i] == '\r' && bytes[i + 1] == '\n' && bytes[i + 2] == '\r' && bytes[i + 3] == '\n')
                    {
                        return i;
                    }
                }
                return -1;
            }
        }

        // One request's exchange: the bytes the service writes as its answer, in order, which the handler reads, and
        // whether the client has left. The service reads nothing more of the request, and a read of its side ends, as a
        // connection's does, once the client leaves; what it writes after that is dropped, as a closed connection's
        // buffer would drop it.
        private sealed class Exchange
        {
            private readonly object gate = new object();
            private readonly Queue<byte[]> written = new Queue<byte[]>();
            private readonly TaskCompletionSource<bool> left = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            private TaskCompletionSource<bool> arrived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            private int readFromFirst;
            private bool ended;

            internal Exchange()
            {
                ServiceSide = new ServiceStream(this);
            }

            internal Stream ServiceSide { get; }

            internal Task Left => left.Task;

            internal void Write(byte[] buffer, int offset, int count)
            {
                lock (gate)
                {
                    if (ended || left.Task.IsCompleted || count == 0)
                    {
                        return;
                    }
                    written.Enqueue(Copy(buffer, offset, count));
                    arrived.TrySetResult(true);
                }
            }

            // The service has written all of its answer.
            internal void End()
            {
                lock (gate)
                {
                    ended = true;
                    arrived.TrySetResult(true);
                }
            }

            // The client has left: it has stopped waiting for the answer, or has disposed of it.
            internal void Leave()
            {
                lock (gate)
                {
                    left.TrySetResult(true);
                    written.Clear();
                    arrived.TrySetResult(true);
                }
            }

            // Up to count bytes of the answer, waiting until some arrive; 0 once the answer has ended and all of it was
            // read, or the client has left.
            internal async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                while (true)
                {
                    Task wait;
                    lock (gate)
                    {
                        if (written.Count > 0)
                        {
                            byte[] first = written.Peek();
                            int take = Math.Min(count, first.Length - readFromFirst);
                            Buffer.BlockCopy(first, readFromFirst, buffer, offset, take);
                            readFromFirst += take;
                            if (readFromFirst == first.Length)
                            {
                                written.Dequeue();
                                readFromFirst = 0;
                            }
                            return take;
                        }
                        if (ended || left.Task.IsCompleted)
                        {
                            return 0;
                        }
                        if (arrived.Task.IsCompleted)
                        {
                            arrived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                        }
                        wait = arrived.Task;
                    }
                    await WhenDone(wait, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        // The service's side of an exchange, which it answers through as through a connection's stream.
        private sealed class ServiceStream : Stream
        {
            private readonly Exchange exchange;

            internal ServiceStream(Exchange exchange)
            {
                this.exchange = exchange;
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            // Nothing more of the request arrives: a read ends once the client leaves.
            public override int Read(byte[] buffer, int offset, int count)
            {
                exchange.Left.Wait();
                return 0;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                await WhenDone(exchange.Left, cancellationToken).ConfigureAwait(false);
                return 0;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                exchange.Write(buffer, offset, count);
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                exchange.Write(buffer, offset, count);
                return Task.CompletedTask;
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();
        }

        // A response's body as the handler's client reads it: the bytes that arrived with the head, then the rest as the
        // service writes it, up to the Content-Length it announced. An answer that ends short of it fails the read, as
        // a connection closed early does. Disposing of the body before its end is the client leaving.
        private sealed class ResponseBody : Stream
        {
            private readonly Exchange exchange;
            private readonly byte[] arrived;
            private int arrivedRead;
            private long remaining;

            internal ResponseBody(Exchange exchange, byte[] arrived, long length)
            {
                this.exchange = exchange;
                this.arrived = arrived;
                remaining = length;
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if (remaining == 0 || count == 0)
                {
                    return 0;
                }
                int wanted = (int)Math.Min(count, remaining);
                int read;
                if (arrivedRead < arrived.Length)
                {
                    read = Math.Min(wanted, arrived.Length - arrivedRead);
                    Buffer.BlockCopy(arrived, arrivedRead, buffer, offset, read);
                    arrivedRead += read;
                }
                else
                {
                    read = await exchange.ReadAsync(buffer, offset, wanted, cancellationToken).ConfigureAwait(false);
                    if (read == 0)
                    {
#if NET8_0_OR_GREATER
                        throw new HttpIOException(HttpRequestError.ResponseEnded, $"The response ended prematurely, with {remaining} bytes of its body still to come.");
#else
                        throw new IOException($"The response ended prematurely, with {remaining} bytes of its body still to come.");
#endif
                    }
                }
                remaining -= read;
                return read;
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                if (disposing && remaining > 0)
                {
                    exchange.Leave();
                }
                base.Dispose(disposing);
            }
        }
    }
}
