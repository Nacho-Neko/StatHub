namespace StatHub.Web.Model
{
    public class FarmHubService
    {

        
    }

    public class FarmHub { 
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public ulong Runtime { get; set; }
        public DateTime UpdateAt { get; set; }
    }
}
