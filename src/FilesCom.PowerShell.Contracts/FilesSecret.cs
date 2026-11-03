using System.Security;

namespace FilesCom.PowerShell
{
    /// <summary>
    /// A credential Files.com returned, such as a new API key. It shows as ******** and has no property holding the
    /// value, so formatting, Export-Csv, ConvertTo-Json and Export-Clixml never write it. Call <see cref="Reveal"/> or
    /// <see cref="ToSecureString"/> when the value is needed.
    /// </summary>
    /// <remarks>
    /// The value is held in memory as ordinary .NET text, as the Files.com SDK received it; this type keeps it out of
    /// output, not out of memory.
    /// </remarks>
    public sealed class FilesSecret
    {
        private readonly string value;

        internal FilesSecret(string value)
        {
            this.value = value;
        }

        /// <summary>Returns the value as plain text.</summary>
        public string Reveal()
        {
            return value;
        }

        /// <summary>Returns the value as a read-only SecureString.</summary>
        public SecureString ToSecureString()
        {
            var secure = new SecureString();
            foreach (char character in value)
            {
                secure.AppendChar(character);
            }
            secure.MakeReadOnly();
            return secure;
        }

        /// <summary>Returns ********, never the value.</summary>
        public override string ToString()
        {
            return "********";
        }
    }
}
