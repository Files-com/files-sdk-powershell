using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FilesCom.PowerShell.Tests
{
    /// <summary>One request the service received.</summary>
    public sealed class RecordedRequest
    {
        internal RecordedRequest(string method, string target, Dictionary<string, string> headers, byte[] body)
        {
            Method = method;
            // A request sent to a proxy names the whole URL; its path is what the site itself would receive.
            if (target.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                AbsoluteTarget = target;
                int pathStart = target.IndexOf('/', target.IndexOf("://", StringComparison.Ordinal) + 3);
                target = pathStart < 0 ? "/" : target.Substring(pathStart);
            }
            int query = target.IndexOf('?');
            Path = query < 0 ? target : target.Substring(0, query);
            RawQuery = query < 0 ? "" : target.Substring(query + 1);
            Query = ParseQuery(RawQuery);
            Headers = headers;
            BodyBytes = body;
            Body = Encoding.UTF8.GetString(body);
        }

        public string Method { get; }

        /// <summary>The whole URL of a request sent to the service as a proxy, or null.</summary>
        public string AbsoluteTarget { get; }

        /// <summary>The path as sent, still percent-encoded.</summary>
        public string Path { get; }

        public string RawQuery { get; }

        /// <summary>The decoded query parameters.</summary>
        public Dictionary<string, string> Query { get; }

        public Dictionary<string, string> Headers { get; }

        public string Body { get; }

        public byte[] BodyBytes { get; }

        /// <summary>Whether the client closed the connection before the response was written.</summary>
        public bool ClientAborted { get; internal set; }

        /// <summary>Whether the request came through the service's in-memory handler rather than over a socket.</summary>
        public bool InMemory { get; internal set; }

        public string Header(string name)
        {
            return Headers.TryGetValue(name, out string value) ? value : null;
        }

        public override string ToString()
        {
            return Method + " " + Path + (RawQuery.Length > 0 ? "?" + RawQuery : "");
        }

        /// <summary>
        /// The parts of a multipart/form-data body, in order: each with its field name, the file name and media type of
        /// a file part, and its exact bytes. Empty when the body is not multipart.
        /// </summary>
        public IReadOnlyList<FormPart> FormParts()
        {
            var parts = new List<FormPart>();
            Match boundary = Regex.Match(Header("Content-Type") ?? "", "multipart/form-data;.*boundary=\"?([^\";]+)\"?", RegexOptions.IgnoreCase);
            if (!boundary.Success)
            {
                return parts;
            }
            byte[] delimiter = Encoding.ASCII.GetBytes("--" + boundary.Groups[1].Value);
            int start = IndexOf(BodyBytes, delimiter, 0);
            while (start >= 0)
            {
                int headersStart = start + delimiter.Length;
                if (headersStart + 1 < BodyBytes.Length && BodyBytes[headersStart] == '-' && BodyBytes[headersStart + 1] == '-')
                {
                    break;
                }
                headersStart += 2;
                int headersEnd = IndexOf(BodyBytes, Encoding.ASCII.GetBytes("\r\n\r\n"), headersStart);
                int next = IndexOf(BodyBytes, delimiter, headersEnd);
                if (headersEnd < 0 || next < 0)
                {
                    break;
                }
                string headers = Encoding.UTF8.GetString(BodyBytes, headersStart, headersEnd - headersStart);
                int contentStart = headersEnd + 4;
                byte[] content = new byte[next - 2 - contentStart];
                Array.Copy(BodyBytes, contentStart, content, 0, content.Length);
                parts.Add(new FormPart(
                    HeaderParameter(headers, "name"),
                    HeaderParameter(headers, "filename"),
                    Regex.Match(headers, "^Content-Type:\\s*(.+?)\\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline) is Match type && type.Success ? type.Groups[1].Value : null,
                    content));
                start = next;
            }
            return parts;
        }

        // A number at the top level of a JSON body, such as "size": 10.
        internal long? JsonNumber(string name)
        {
            Match match = Regex.Match(Body, "\"" + Regex.Escape(name) + "\"\\s*:\\s*(\\d+)");
            return match.Success ? long.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : (long?)null;
        }

        private static string HeaderParameter(string headers, string name)
        {
            Match match = Regex.Match(headers, "Content-Disposition:[^\\r\\n]*?;\\s*" + name + "=(\"(?<quoted>[^\"]*)\"|(?<plain>[^;\\r\\n]*))", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return null;
            }
            return match.Groups["quoted"].Success ? match.Groups["quoted"].Value : match.Groups["plain"].Value.Trim();
        }

        private static int IndexOf(byte[] data, byte[] pattern, int from)
        {
            for (int i = Math.Max(from, 0); i <= data.Length - pattern.Length; i++)
            {
                int j = 0;
                while (j < pattern.Length && data[i + j] == pattern[j])
                {
                    j++;
                }
                if (j == pattern.Length)
                {
                    return i;
                }
            }
            return -1;
        }

        private static Dictionary<string, string> ParseQuery(string query)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string pair in query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int equals = pair.IndexOf('=');
                string key = Uri.UnescapeDataString((equals < 0 ? pair : pair.Substring(0, equals)).Replace('+', ' '));
                string value = equals < 0 ? "" : Uri.UnescapeDataString(pair.Substring(equals + 1).Replace('+', ' '));
                result[key] = value;
            }
            return result;
        }
    }

    /// <summary>One part of a multipart/form-data request body.</summary>
    public sealed class FormPart
    {
        internal FormPart(string name, string fileName, string contentType, byte[] content)
        {
            Name = name;
            FileName = fileName;
            ContentType = contentType;
            Content = content;
        }

        /// <summary>The form field's name.</summary>
        public string Name { get; }

        /// <summary>The file name of a file part, or null.</summary>
        public string FileName { get; }

        /// <summary>The part's media type, or null when it gave none.</summary>
        public string ContentType { get; }

        /// <summary>The part's exact bytes.</summary>
        public byte[] Content { get; }

        /// <summary>The part's bytes as UTF-8 text.</summary>
        public string Text => Encoding.UTF8.GetString(Content);
    }
}
