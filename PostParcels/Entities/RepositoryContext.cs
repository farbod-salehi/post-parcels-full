using Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /*
     IdentityDbContext<User> is used in ASP.NET Core Identity to manage user authentication and authorization. 
     It extends DbContext and includes predefined identity-related tables, making it easier to handle user management.
     */

    /*
     In C# 12, a primary constructor is a new feature that allows you to define constructor parameters directly in a class declaration, 
     eliminating the need for an explicit constructor method.
     Here, the class RepositoryContext has constructor parameters (options) directly in its declaration without defining a separate constructor.
     */

    public class RepositoryContext(DbContextOptions<RepositoryContext> options) : IdentityDbContext<User>(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add custom configurations of tables, columns, relationships, indexes and so on here 

            //modelBuilder.Entity<User>().HasOne
        }

        public DbSet<Unit> Units { get; set; }

        public DbSet<Building> Buildings { get; set; }

        public DbSet<Parcel> Parcels { get; set; }

        public DbSet<ParcelItem> ParcelItems { get; set; }

        public DbSet<ParcelDocument> ParcelDocuments { get; set; }

        /* Use 'new' for DbSet<UserToken> to Avoids Conflicts with IdentityUserToken's Default DbSet, 
           because ASP.NET Core Identity already includes a default DbSet for it 
        */
        public new DbSet<UserToken> UserTokens { get; set; }
    }
}
