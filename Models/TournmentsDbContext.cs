using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace TournamentsProject.Models
{
    public class TournmentsDbContext : DbContext
    {
        public DbSet<TournmentModel> Tournments { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public object Users { get; internal set; }

        // Allows Entity Framework to run properly
        protected override void OnModelCreating(DbModelBuilder builder)
        {
            builder.Entity<IdentityUserLogin>().HasKey<string>(l => l.UserId);
            builder.Entity<IdentityRole>().HasKey<string>(r => r.Id);
            builder.Entity<IdentityUserRole>().HasKey(r => new { r.RoleId, r.UserId });
        }
    }

}