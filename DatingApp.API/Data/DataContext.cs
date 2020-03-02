
using Microsoft.EntityFrameworkCore;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options){}
        public DbSet<Value> Values {get; set;}
        public DbSet<User> Users  {get; set;}
        public DbSet<Photo> Photos {get; set;}
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        // we need to overwrite OnModelCreate to generate a many to many relationship
        // read the summary defined of OnModelCreating in the DbContext object
        protected override void OnModelCreating(ModelBuilder builder)
        {
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

            
        }
    }
}