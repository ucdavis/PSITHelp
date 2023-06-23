namespace ITHelp.Models
{
    public class UserRequest
    {
        public int Id { get; set; }
        public string SubmittedBy { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UCDMailID { get; set; }
        public bool Undergraduate { get; set; }
        public bool SDriveAccess { get; set; }
        public string ADGroup { get; set; }
        public string BaseGroup { get; set; }
        public bool Complete { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string CancelNotes { get; set; }
        public string Comments { get; set; }
    }
}
