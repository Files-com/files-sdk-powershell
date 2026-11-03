using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FilesCom.PowerShell.Tests
{
    // Reads HTTP/1.1 requests from a connection: the request line, headers, and a body sent with Content-Length or
    // chunked transfer encoding.
    internal sealed class HttpReader
    {
        private readonly Stream stream;
        private readonly byte[] buffer = new byte[64 * 1024];
        private int start;
        private int end;

        internal HttpReader(Stream stream)
        {
            this.stream = stream;
        }

        // The next request, or null when the client closed the connection between requests.
        internal async Task<RecordedRequest> ReadRequestAsync(CancellationToken cancellationToken)
        {
            string requestLine = await ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (requestLine == null)
            {
                return null;
            }
            string[] parts = requestLine.Split(' ');
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string line;
            while (!string.IsNullOrEmpty(line = await ReadLineAsync(cancellationToken).ConfigureAwait(false)))
            {
                int colon = line.IndexOf(':');
                string name = line.Substring(0, colon).Trim();
                string value = line.Substring(colon + 1).Trim();
                headers[name] = headers.TryGetValue(name, out string existing) ? existing + ", " + value : value;
            }
            byte[] body;
            if (headers.TryGetValue("Transfer-Encoding", out string encoding) && encoding.IndexOf("chunked", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                body = await ReadChunkedAsync(cancellationToken).ConfigureAwait(false);
            }
            else if (headers.TryGetValue("Content-Length", out string length))
            {
                body = await ReadBytesAsync(long.Parse(length, CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                body = Array.Empty<byte>();
            }
            return new RecordedRequest(parts[0], parts[1], headers, body);
        }

        private async Task<byte[]> ReadChunkedAsync(CancellationToken cancellationToken)
        {
            var body = new MemoryStream();
            while (true)
            {
                string sizeLine = await ReadLineAsync(cancellationToken).ConfigureAwait(false) ?? throw new EndOfStreamException();
                long size = long.Parse(sizeLine.Split(';')[0].Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                if (size == 0)
                {
                    while (!string.IsNullOrEmpty(await ReadLineAsync(cancellationToken).ConfigureAwait(false)))
                    {
                    }
                    return body.ToArray();
                }
                byte[] chunk = await ReadBytesAsync(size, cancellationToken).ConfigureAwait(false);
                body.Write(chunk, 0, chunk.Length);
                await ReadLineAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<byte[]> ReadBytesAsync(long count, CancellationToken cancellationToken)
        {
            var result = new byte[count];
            long copied = 0;
            while (copied < count)
            {
                if (start == end && !await FillAsync(cancellationToken).ConfigureAwait(false))
                {
                    throw new EndOfStreamException();
                }
                int take = (int)Math.Min(end - start, count - copied);
                Buffer.BlockCopy(buffer, start, result, (int)copied, take);
                start += take;
                copied += take;
            }
            return result;
        }

        private async Task<string> ReadLineAsync(CancellationToken cancellationToken)
        {
            var line = new StringBuilder();
            while (true)
            {
                if (start == end && !await FillAsync(cancellationToken).ConfigureAwait(false))
                {
                    return line.Length == 0 ? null : line.ToString();
                }
                byte next = buffer[start++];
                if (next == (byte)'\n')
                {
                    return line.ToString().TrimEnd('\r');
                }
                line.Append((char)next);
            }
        }

        private async Task<bool> FillAsync(CancellationToken cancellationToken)
        {
            start = 0;
            end = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            return end > 0;
        }
    }
}
