using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An AccountLineItem is a single line item in the accounting ledger for a billing account.
    /// </summary>
    public sealed class AccountLineItem : FilesObject
    {
        internal AccountLineItem()
        {
        }

        /// <summary>
        /// Line item Id
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Line item amount
        /// </summary>
        public decimal? Amount { get; internal set; }

        /// <summary>
        /// Line item balance
        /// </summary>
        public decimal? Balance { get; internal set; }

        /// <summary>
        /// Line item created at
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Line item currency
        /// </summary>
        public string Currency { get; internal set; }

        /// <summary>
        /// Line item download uri
        /// </summary>
        public string DownloadUri { get; internal set; }

        /// <summary>
        /// Associated invoice line items
        /// </summary>
        public Models.InvoiceLineItem[] InvoiceLineItems { get; internal set; }

        /// <summary>
        /// Line item payment method
        /// </summary>
        public string Method { get; internal set; }

        /// <summary>
        /// Associated payment line items
        /// </summary>
        public Models.PaymentLineItem[] PaymentLineItems { get; internal set; }

        /// <summary>
        /// Date/time payment was reversed if applicable
        /// </summary>
        public DateTime? PaymentReversedAt { get; internal set; }

        /// <summary>
        /// Type of payment if applicable
        /// </summary>
        public string PaymentType { get; internal set; }

        /// <summary>
        /// Site name this line item is for
        /// </summary>
        public string SiteName { get; internal set; }

        /// <summary>
        /// Type of line item, either payment or invoice
        /// </summary>
        public string Type { get; internal set; }
    }
}
