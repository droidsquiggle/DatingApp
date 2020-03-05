using System;

namespace DatingApp.API.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }
        public string PublicId { get; set; }
        // these create a link back to users for a cascade delete
        // if a user is deleted, the photos will be deleted too
        public virtual User User { get; set; }
        public int UserId { get; set; }
        public bool IsApproved { get; set; }
    }
}