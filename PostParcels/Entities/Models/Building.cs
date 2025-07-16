using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class Building
    {
        #region Properties
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        public required string Code { get; set; }

        [Required, MaxLength(255)]
        public required string Name { get; set; }

        public bool Active { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        #endregion Properties

        #region Navigations

        [ForeignKey(nameof(CreatedBy))]
        public virtual User? CreatedByUser { get; set; }

        #endregion Navigations


    }
}
