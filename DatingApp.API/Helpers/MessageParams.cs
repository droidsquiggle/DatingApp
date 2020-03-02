namespace DatingApp.API.Helpers
{
    public class MessageParams
    {
        
        private const int MaxPageSize = 50;
        // default to 1 so it will always default to page 1
        public int PageNumber { get; set; } = 1;
        private int pageSize = 10;
        public int PageSize
        {
             get {return pageSize;}
             set {pageSize = (value > MaxPageSize) ? MaxPageSize: value;} 
        }

        // filters to start with
        public int UserId { get; set; }
        // Unread will be default container, it will be for messages received that are not yet read
        public string MessageContainer { get; set; } = "Undread";
    }
}