using Microsoft.EntityFrameworkCore;
using SmileShop.Authorization.Models.Auth;
using System;
using System.Collections.Generic;

namespace SmileShop.Authorization.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });

            SeedData(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            Guid roleUser = new Guid("ca27ff11 - bc43 - 4c37 - aca8 - f43b39cff66d");
            Guid roleSupervisor = new Guid("db2a81bd-87e7-4588-9eaa-75ebbd14c81b");
            Guid roleManager = new Guid("64f4ec66-f130-47bd-b8ca-9c51e2faa5dc");
            Guid roleAdmin = new Guid("e49fbccc-9453-4317-8c15-ee6e2ff43310");
            Guid roleDeveloper = new Guid("a17c2e09-d89b-4716-a9b1-6f8a147aabaa");

            modelBuilder.Entity<Role>()
                .HasData(new List<Role>()
               {
                    new Role(){ Id = roleUser, Name = "user"},
                    new Role(){ Id = roleSupervisor, Name = "Supervisor"},
                    new Role(){ Id = roleManager, Name = "Manager"},
                    new Role(){ Id = roleAdmin, Name = "Admin"},
                    new Role(){ Id = roleDeveloper, Name = "Developer"}
               });
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }
    }
}