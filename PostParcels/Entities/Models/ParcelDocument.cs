using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public  class ParcelDocument
    {
        #region Properties

        public Guid Id { get; set; }

        [Required]
        public Guid ParcelId { get; set; }

        [Required,MaxLength(255)]
        public required string FileName { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required, MaxLength(450)]
        public required string CreatedBy { get; set; }

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

        #endregion Navigations

    }
}
