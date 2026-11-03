using System;
using System.Collections.Generic;

namespace FilesCom.PowerShell.Models
{
    /// <summary>
    /// An InvoiceLineItem is your invoice record.
    /// </summary>
    public sealed class InvoiceLineItem : FilesObject
    {
        internal InvoiceLineItem()
        {
        }

        /// <summary>
        /// Invoice Line item Id
        /// </summary>
        public long? Id { get; internal set; }

        /// <summary>
        /// Invoice line item amount
        /// </summary>
        public decimal? Amount { get; internal set; }

        /// <summary>
        /// Invoice line item created at date/time
        /// </summary>
        public DateTime? CreatedAt { get; internal set; }

        /// <summary>
        /// Invoice line item description
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Invoice line item type
        /// </summary>
        public string Type { get; internal set; }

        /// <summary>
        /// Invoice line item service end date/time
        /// </summary>
        public DateTime? ServiceEndAt { get; internal set; }

        /// <summary>
        /// Invoice line item service start date/time
        /// </summary>
        public DateTime? ServiceStartAt { get; internal set; }

        /// <summary>
        /// Plan name
        /// </summary>
        public string Plan { get; internal set; }

        /// <summary>
        /// Site name
        /// </summary>
        public string Site { get; internal set; }

        /// <summary>
        /// Prepaid bytes purchased for this invoice line item
        /// </summary>
        public long? PrepaidBytes { get; internal set; }

        /// <summary>
        /// When the prepaid bytes expire
        /// </summary>
        public DateTime? PrepaidBytesExpireAt { get; internal set; }

        /// <summary>
        /// Total prepaid bytes used for this invoice line item
        /// </summary>
        public long? PrepaidBytesUsed { get; internal set; }

        /// <summary>
        /// Available prepaid bytes for this invoice line item
        /// </summary>
        public long? PrepaidBytesAvailable { get; internal set; }
    }
}
