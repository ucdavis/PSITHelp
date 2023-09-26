namespace ITHelp.Models
{
    public class NewUserRequest
    {
		public int Id { get; set; }
		public string SubmittedBy { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public bool Undergraduate { get; set; }
		public bool SDrive { get; set; }
		public string AdGroup { get; set; }
		public string BaseAdGroup { get; set; }
		public bool Complete { get; set; }
		public DateTime DateSubmitted { get; set; }
		public string CancelNotes { get; set; }
		public string Comments { get; set; }
	}
}
