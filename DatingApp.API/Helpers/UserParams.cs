namespace DatingApp.API.Helpers
{
    // page size will top out at 50 items per call and default starting off on page 1
    public class UserParams
    {
        // set top limit for how many items return per page
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
        public string Gender { get; set; }
        public int MinAge { get; set; } = 18; 
        public int MaxAge { get; set; } = 99;
        public string OrderBy { get; set; }
    }
}