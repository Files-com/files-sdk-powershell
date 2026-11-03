using System;
using System.Text;

namespace FilesCom.PowerShell
{
    // The FilesCom SDK's own rule for text it puts in exception messages and ordinary logs, which it does not make
    // public: every URL in the text keeps its scheme, host, port and path, and loses the parts where it can carry
    // credentials, which are its user information, its query, where signed URLs keep their signatures, and its fragment.
    // Only URLs, written with a scheme and "://", are recognized. This is WithoutUrlCredentials from the SDK's
    // DiagnosticText, identical at 7fe074e7 and at the engine the module ships, 340f8697, unchanged but for its name. The
    // public types whose members can hold what Files.com or a storage server sent, FilesException, FilesUploadedPart
    // and FilesSignInChallenge, apply it when they are made, so no path that makes one can skip it. The engine request
    // for a public version is tracked with the module's other engine dependencies.
    //
    // The text is read once, from each "://" in turn, so its length alone never makes this slow.
    internal static class SafeText
    {
        private const string Redacted = "[redacted]";
        private static readonly char[] AuthorityEnds = { '/', '?', '#' };
        private static readonly char[] PathEnds = { '?', '#' };
        private static readonly char[] FragmentStart = { '#' };

        internal static string WithoutUrlCredentials(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf("://", StringComparison.Ordinal) < 0)
            {
                return text;
            }
            var result = new StringBuilder(text.Length);
            int copied = 0;
            int search = 0;
            int separator;
            while ((separator = text.IndexOf("://", search, StringComparison.Ordinal)) >= 0)
            {
                int start = SchemeStart(text, separator, copied);
                if (start < 0)
                {
                    search = separator + 3;
                    continue;
                }
                int end = UrlEnd(text, start, separator + 3);
                result.Append(text, copied, start - copied);
                AppendWithoutCredentials(result, text, start, separator + 3, end);
                copied = search = end;
            }
            return result.Append(text, copied, text.Length - copied).ToString();
        }

        // Where the scheme before the "://" at separator starts, not before from: at its first letter, followed by
        // letters, digits, "+", "-" and ".". It is -1 when there is no letter there, and so no URL.
        private static int SchemeStart(string text, int separator, int from)
        {
            int start = separator;
            while (start > from && IsSchemeCharacter(text[start - 1]))
            {
                start--;
            }
            while (start < separator && !IsLetter(text[start]))
            {
                start++;
            }
            return start < separator ? start : -1;
        }

        // Where the URL that starts at start ends: at white space, a double quote or an angle bracket, which a URL
        // cannot hold, or at the scheme of another URL in its path. An apostrophe can be part of a URL, as in its user
        // information or path, so it ends one only when the URL follows an apostrophe: then the last apostrophe that
        // only closing punctuation follows is the closing quote.
        private static int UrlEnd(string text, int start, int bodyStart)
        {
            int end = bodyStart;
            bool inQueryOrFragment = false;
            while (end < text.Length && !IsTerminator(text[end]))
            {
                char c = text[end];
                if (c == '?' || c == '#')
                {
                    inQueryOrFragment = true;
                }
                else if (!inQueryOrFragment && c == ':' && string.CompareOrdinal(text, end, "://", 0, 3) == 0)
                {
                    int nextStart = SchemeStart(text, end, bodyStart);
                    if (nextStart >= 0)
                    {
                        return nextStart;
                    }
                }
                end++;
            }
            if (start > 0 && text[start - 1] == '\'')
            {
                int closing = end;
                while (closing > bodyStart && IsClosingPunctuation(text[closing - 1]))
                {
                    closing--;
                }
                if (closing > bodyStart && text[closing - 1] == '\'')
                {
                    return closing - 1;
                }
            }
            return end;
        }

        // Appends the URL text[start, end) with its user information, query and fragment replaced by Redacted.
        private static void AppendWithoutCredentials(StringBuilder result, string text, int start, int bodyStart, int end)
        {
            int authorityEnd = IndexOfAny(text, bodyStart, end, AuthorityEnds);
            int userInformationEnd = authorityEnd > bodyStart ? text.LastIndexOf('@', authorityEnd - 1, authorityEnd - bodyStart) : -1;
            int pathEnd = IndexOfAny(text, authorityEnd, end, PathEnds);

            result.Append(text, start, bodyStart - start);
            if (userInformationEnd >= 0)
            {
                result.Append(Redacted).Append('@');
            }
            int hostStart = userInformationEnd >= 0 ? userInformationEnd + 1 : bodyStart;
            result.Append(text, hostStart, pathEnd - hostStart);
            if (pathEnd < end && text[pathEnd] == '?')
            {
                result.Append('?').Append(Redacted);
            }
            if (IndexOfAny(text, pathEnd, end, FragmentStart) < end)
            {
                result.Append('#').Append(Redacted);
            }
        }

        // The first of characters in text[from, end), or end when there is none.
        private static int IndexOfAny(string text, int from, int end, char[] characters)
        {
            int index = from < end ? text.IndexOfAny(characters, from, end - from) : -1;
            return index >= 0 ? index : end;
        }

        private static bool IsLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private static bool IsSchemeCharacter(char c)
        {
            return IsLetter(c) || (c >= '0' && c <= '9') || c == '+' || c == '-' || c == '.';
        }

        private static bool IsTerminator(char c)
        {
            return char.IsWhiteSpace(c) || c == '"' || c == '<' || c == '>';
        }

        private static bool IsClosingPunctuation(char c)
        {
            return c == '.' || c == ',' || c == ';' || c == ':' || c == '!' || c == '?' || c == ')' || c == ']' || c == '}';
        }
    }
}
