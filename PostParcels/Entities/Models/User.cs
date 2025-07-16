using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    /*
     IdentityUser is a built-in class in ASP.NET Core Identity that represents a user in your authentication system. 
     It contains essential properties used for managing user authentication, authorization, and security.
     */
    public class User : IdentityUser
    {
        #region Properties

        [Required, MaxLength(255)]
        public required string Title { get; set; }

        [Required]
        public bool Active { get; set; }

        public DateTime? CreatedAt { get; set; }

        [MaxLength(450)]
        public string? CreatedBy { get; set; }

        public DateTime? PasswordChangedAt { get; set; }

        [Required]
        public required int Role { get; set; }

        #endregion Properties


        #region Navigations

       
        #endregion Navigations
    }
}
