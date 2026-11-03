using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FilesCom.PowerShell
{
    /// <summary>
    /// Whether a connection's commands move on to the address Files.com names when it answers that the site is served
    /// from another region: <see cref="Automatic"/>, the default, <see cref="Disabled"/>, or only between listed pairs of
    /// origins. Connect-FilesAccount -RegionRelocation takes Automatic, Disabled, or a hashtable of pairs, such as
    /// <c>@{ 'https://files.example.com' = 'https://files.example.eu' }</c>.
    /// </summary>
    /// <remarks>
    /// A command that moves keeps the new address for the rest of its own requests, and so do the objects and pages it
    /// writes, for the commands given them later. The connection itself is unchanged: every other command starts at its
    /// BaseUrl. Moving takes one of the command's retries (-MaxNetworkRetries).
    /// </remarks>
    public sealed class FilesRegionRelocationPolicy
    {
        private FilesRegionRelocationPolicy(FilesRegionRelocation kind, IReadOnlyList<FilesOriginPair> pairs)
        {
            Kind = kind;
            Pairs = pairs;
        }

        /// <summary>
        /// Moves when Files.com answers from the connection's address, over HTTPS whose certificate was checked as .NET
        /// does by default, naming an HTTPS address on port 443 with a public DNS name. The default.
        /// </summary>
        public static FilesRegionRelocationPolicy Automatic { get; } = new FilesRegionRelocationPolicy(FilesRegionRelocation.Automatic, Array.Empty<FilesOriginPair>());

        /// <summary>Never moves: the command fails with Files.RegionMismatch, naming the site's address.</summary>
        public static FilesRegionRelocationPolicy Disabled { get; } = new FilesRegionRelocationPolicy(FilesRegionRelocation.Disabled, Array.Empty<FilesOriginPair>());

        /// <summary>How the policy decides.</summary>
        public FilesRegionRelocation Kind { get; }

        /// <summary>
        /// For <see cref="FilesRegionRelocation.Listed"/>, the moves allowed, each from one origin to another and not
        /// back unless listed; otherwise empty.
        /// </summary>
        public IReadOnlyList<FilesOriginPair> Pairs { get; }

        /// <summary>
        /// A policy that moves only between the given pairs, keyed by the origin the answer must come from, each value
        /// the origin, or origins, Files.com may name. A listed pair also allows what Automatic never does, such as a
        /// port other than 443, a name that is not public, an IP address or plain HTTP, as a development site may need,
        /// and an answer through a handler whose certificate checks cannot be seen.
        /// </summary>
        public static FilesRegionRelocationPolicy Between(IDictionary pairs)
        {
            if (pairs == null)
            {
                throw new ArgumentNullException(nameof(pairs));
            }
            var listed = new List<FilesOriginPair>();
            foreach (DictionaryEntry entry in pairs)
            {
                string from = Origin(entry.Key);
                IEnumerable<object> targets = entry.Value is IEnumerable values && !(entry.Value is string) ? values.Cast<object>() : new[] { entry.Value };
                foreach (object target in targets)
                {
                    listed.Add(new FilesOriginPair(from, Origin(target)));
                }
            }
            if (listed.Count == 0)
            {
                throw new ArgumentException("A region relocation policy of pairs needs at least one pair of origins.", nameof(pairs));
            }
            return new FilesRegionRelocationPolicy(
                FilesRegionRelocation.Listed,
                listed.Distinct().OrderBy(pair => pair.From, StringComparer.Ordinal).ThenBy(pair => pair.To, StringComparer.Ordinal).ToArray());
        }

        /// <summary>The policy for the text Automatic or Disabled.</summary>
        public static FilesRegionRelocationPolicy Parse(string policy)
        {
            if (string.Equals(policy, nameof(Automatic), StringComparison.OrdinalIgnoreCase))
            {
                return Automatic;
            }
            if (string.Equals(policy, nameof(Disabled), StringComparison.OrdinalIgnoreCase))
            {
                return Disabled;
            }
            throw new ArgumentException($"'{policy}' is not a region relocation policy. Give Automatic, Disabled, or a hashtable of origin pairs such as @{{ 'https://files.example.com' = 'https://files.example.eu' }}.", nameof(policy));
        }

        /// <summary>Automatic, Disabled, or the listed pairs, such as <c>https://a.example.com -> https://b.example.com</c>.</summary>
        public override string ToString()
        {
            return Kind == FilesRegionRelocation.Listed ? string.Join(", ", Pairs.Select(pair => pair.ToString())) : Kind.ToString();
        }

        // An HTTP or HTTPS origin, a scheme, a host and an optional port, as the engine takes it: with no user
        // information, path, query or fragment. A trailing slash is dropped.
        private static string Origin(object value)
        {
            string text = value as string;
            if (!Uri.TryCreate(text?.TrimEnd('/'), UriKind.Absolute, out Uri uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                || !string.IsNullOrEmpty(uri.UserInfo)
                || !string.Equals(uri.GetLeftPart(UriPartial.Authority), text.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"'{value}' is not an origin: give a scheme, a host and an optional port, such as https://files.example.com or http://localhost:8443, with no path, query or credentials.", nameof(value));
            }
            return uri.GetLeftPart(UriPartial.Authority);
        }
    }

    /// <summary>How a <see cref="FilesRegionRelocationPolicy"/> decides whether a command moves to its site's region.</summary>
    public enum FilesRegionRelocation
    {
        /// <summary>Moves to a verified HTTPS address on port 443 with a public DNS name.</summary>
        Automatic,

        /// <summary>Never moves.</summary>
        Disabled,

        /// <summary>Moves only between the listed pairs of origins.</summary>
        Listed,
    }

    /// <summary>A move a <see cref="FilesRegionRelocationPolicy"/> allows: from one origin to another.</summary>
    public sealed class FilesOriginPair : IEquatable<FilesOriginPair>
    {
        internal FilesOriginPair(string from, string to)
        {
            From = from;
            To = to;
        }

        /// <summary>The origin Files.com's answer must come from.</summary>
        public string From { get; }

        /// <summary>The origin Files.com may name as the site's address.</summary>
        public string To { get; }

        /// <summary>Whether the pair allows the same move.</summary>
        public bool Equals(FilesOriginPair other)
        {
            return other != null && From == other.From && To == other.To;
        }

        /// <summary>Whether the pair allows the same move.</summary>
        public override bool Equals(object obj) => Equals(obj as FilesOriginPair);

        /// <summary>A hash of both origins.</summary>
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(From) ^ StringComparer.Ordinal.GetHashCode(To);

        /// <summary>The move, such as <c>https://a.example.com -> https://b.example.com</c>.</summary>
        public override string ToString() => $"{From} -> {To}";
    }

    /// <summary>Why a command did not move to the address Files.com named for its site.</summary>
    public enum FilesRegionRelocationRefusal
    {
        /// <summary>The connection's -RegionRelocation is Disabled.</summary>
        Disabled,

        /// <summary>The command had moved to another address once already.</summary>
        AlreadyRelocated,

        /// <summary>
        /// The answer cannot be trusted to name the site's address, and the message says why: it was not a 403, it came
        /// from another address or after a redirect to another origin, or it came over plain HTTP or a handler whose
        /// certificate checks cannot be seen, and no listed pair allows the move.
        /// </summary>
        UntrustedIssuer,

        /// <summary>The answer named no address, or one that is not a DNS name or an origin with no path.</summary>
        InvalidHost,

        /// <summary>The answer named the address the command already sent its request to.</summary>
        SameOrigin,

        /// <summary>
        /// The policy does not move there: Automatic moves only to HTTPS on port 443 with a public DNS name, and a
        /// policy of pairs only between the origins it lists.
        /// </summary>
        NotAllowed,

        /// <summary>The command had no retry left, of the connection's -MaxNetworkRetries, to move with.</summary>
        RetriesExhausted,
    }
}
