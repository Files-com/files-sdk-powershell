using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// A PaymentLineItem is your payment information.
    /// </summary>
    public sealed class PaymentLineItem : FilesObject
    {
        internal PaymentLineItem()
        {
        }

        /// <summary>
        /// Payment line item amount
        /// </summary>
        public decimal? Amount { get; internal set; }

        /// <summary>
        /// Payment line item created at date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Invoice ID
        /// </summary>
        public long? InvoiceId { get; internal set; }

        /// <summary>
        /// Payment ID
        /// </summary>
        public long? PaymentId { get; internal set; }
    }
}
