using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class Parcel
    {
        #region Properties
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        public required string Code { get; set; }

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

        public virtual ICollection<ParcelItem>? ParcelItems { get; set; }

        public virtual ICollection<ParcelDocument>? ParcelDocuments { get; set; }

        #endregion Navigations
    }
}
