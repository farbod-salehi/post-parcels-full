using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class Unit
    {
        #region Properties
        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        [Required, MaxLength(50)]
        public required string Code { get; set; }

        [Required, MaxLength(255)]
        public required string Name { get; set; }

        [Required]
        public bool Active { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }

        public string? DeletedBy { get; set; }

        #endregion Properties


        #region Navigations

        [ForeignKey(nameof(ParentId))]
        public virtual Unit? ParentUnit { get; set; }

        public virtual ICollection<Unit>? SubUnits { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User? CreatedByUser { get; set; }

        [ForeignKey(nameof(DeletedBy))]
        public virtual User? DeletedByUser { get; set; }

        #endregion Navigations


    }
}
