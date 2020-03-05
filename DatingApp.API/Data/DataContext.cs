
using Microsoft.EntityFrameworkCore;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Data
{
    public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, 
                                                 IdentityUserLogin<int>, IdentityRoleClaim<int>,
                                                 IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options){}
        public DbSet<Value> Values {get; set;}
        public DbSet<Photo> Photos {get; set;}
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        // we need to overwrite OnModelCreate to generate a many to many relationship
        // read the summary defined of OnModelCreating in the DbContext object
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // need to call base.OnModelCreating(builder) when using Identity to prevent errors
            base.OnModelCreating(builder);

            // define relationships between users and roles with the UserRole crosswalk table
            builder.Entity<UserRole>(userRole =>
            {
                userRole.HasKey(ur => new {ur.UserId, ur.RoleId});

                userRole.HasOne(ur => ur.Role)
                        .WithMany(r => r.UserRoles)
                        .HasForeignKey(ur => ur.RoleId)
                        .IsRequired();

                userRole.HasOne(ur => ur.User)
                        .WithMany(r => r.UserRoles)
                        .HasForeignKey(ur => ur.UserId)
                        .IsRequired();
            });

            // many to many configuration for likes
            builder.Entity<Like>()
                .HasKey(k => new {k.LikerId, k.LikeeId});
            
            builder.Entity<Like>()
                .HasOne(u => u.Likee)
                .WithMany(u => u.Likers)
                .HasForeignKey(u => u.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Like>()
                .HasOne(u => u.Liker)
                .WithMany(u => u.Likees)
                .HasForeignKey(u => u.LikerId)
                .OnDelete(DeleteBehavior.Restrict);

            // many to many configuration for messages
            builder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Photo>().HasQueryFilter(p => p.IsApproved);
            
        }
    }
}