using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FilesCom.PowerShell
{
    /// <summary>
    /// A sign-in that Files.com accepted the username and password of but that needs a second factor to finish. It is
    /// the <c>TargetObject</c> of the <c>Files.TwoFactorRequired</c> error Connect-FilesAccount reports; give it to
    /// <c>Connect-FilesAccount -Challenge</c> with the code the user's device shows to finish signing in.
    /// </summary>
    /// <remarks>
    /// The identifier of the partly completed login stays inside the challenge: no property holds it, so formatting,
    /// ConvertTo-Json and Export-Clixml never write it, and a challenge read back from serialized data cannot finish a
    /// sign-in. A challenge finishes one sign-in. Only one command at a time may try to finish it: another is refused
    /// before it sends anything. When Files.com refuses the code, the challenge can be tried again with another.
    /// </remarks>
    public sealed class FilesSignInChallenge
    {
        private const int Open = 0;
        private const int Finishing = 1;
        private const int Finished = 2;
        private readonly string partialSessionId;
        private int state;

        internal FilesSignInChallenge(FilesConnectionSettings settings, string username, string partialSessionId, IReadOnlyList<string> methods)
        {
            Settings = settings;
            Username = username;
            this.partialSessionId = partialSessionId;
            // Sent by Files.com; made safe like every other text a server sent.
            Methods = methods.Select(SafeText.WithoutUrlCredentials).ToArray();
        }

        /// <summary>The site being signed in to.</summary>
        public string BaseUrl => Settings.BaseUrl;

        /// <summary>The user signing in.</summary>
        public string Username { get; }

        /// <summary>The kinds of second factor Files.com accepts for this user, such as <c>totp</c>, <c>sms</c> or <c>yubi</c>.</summary>
        public IReadOnlyList<string> Methods { get; }

        /// <summary>Whether this challenge already finished a sign-in.</summary>
        public bool IsCompleted => Volatile.Read(ref state) == Finished;

        /// <summary>Whether a command is finishing a sign-in with this challenge now.</summary>
        public bool IsInProgress => Volatile.Read(ref state) == Finishing;

        internal FilesConnectionSettings Settings { get; }

        internal string PartialSessionId => partialSessionId;

        // Claims the challenge for one attempt to finish the sign-in, before that attempt sends anything. False when
        // another attempt holds it or it already finished a sign-in; that attempt must send nothing.
        internal bool TryBegin()
        {
            return Interlocked.CompareExchange(ref state, Finishing, Open) == Open;
        }

        // The claimed attempt created the session.
        internal void Complete()
        {
            Volatile.Write(ref state, Finished);
        }

        // The claimed attempt created no session, as when Files.com refused the code; another attempt may follow.
        internal void Release()
        {
            Volatile.Write(ref state, Open);
        }

        /// <summary>Describes the challenge without its login identifier.</summary>
        public override string ToString()
        {
            return $"Two-factor sign-in of {Username} at {BaseUrl} ({string.Join(", ", Methods)})";
        }
    }
}
