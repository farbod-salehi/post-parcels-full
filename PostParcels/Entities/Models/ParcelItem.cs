using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class ParcelItem
    {
        #region Properties
        public Guid Id { get; set; }

        public string? Code { get; set; }

        public Guid ParcelId { get; set; }

        public int PacketNumber { get; set; }

        public int Type { get; set; }

        public Guid? SenderId { get; set; }

        [MaxLength(255)]
        public string? UndefinedSenderTitle { get; set; }

        public Guid? ReceiverId { get; set; }

        [MaxLength(255)]
        public string? UndefinedReceiverTitle { get; set; }

        public string? Description { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }

        public string? DeletedBy { get; set; }

        #endregion Properties


        #region Navigations

        [ForeignKey(nameof(CreatedBy))]
        public virtual User? CreatedByUser { get; set; }

        [ForeignKey(nameof(DeletedBy))]
        public virtual User? DeletedByUser { get; set; }

        [ForeignKey(nameof(ParcelId))]
        public virtual Parcel? Parcel { get; set; }

        [ForeignKey(nameof(SenderId))]
        public virtual Unit? SenderUnit { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        public virtual Unit? ReceiverUnit { get; set; }

        #endregion Navigations
    }
}
