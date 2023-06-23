namespace ITHelp.Models
{
    public class UserReequestPermissions
    {
        public int Id { get; set; }
        public string PIEmployeeId { get; set; }
        public string DelegateId { get; set; }
        public bool Current { get; set; }
        public bool SDrive { get; set; }
        public string ADGroup { get; set; }
        public string BaseGroup { get; set; }
    }
}
