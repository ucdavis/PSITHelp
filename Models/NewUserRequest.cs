using System.ComponentModel.DataAnnotations;

namespace ITHelp.Models
{
    public class NewUserRequest
    {
		public int Id { get; set; }
		public string SubmittedBy { get; set; }
		[Display(Name ="First Name")]
		public string FirstName { get; set; }
		[Display(Name = "Last Name")]
		public string LastName { get; set; }
		public string Email { get; set; }
		[Display(Name = "Undergraduate?")]
		public bool Undergraduate { get; set; }
		[Display(Name = "Needs S drive access?")]
		public bool SDrive { get; set; }
		[Display(Name = "Faculty Group")] 
		public string AdGroup { get; set; }
		public string BaseAdGroup { get; set; }
		public bool Complete { get; set; }
		public DateTime DateSubmitted { get; set; }
		public string CancelNotes { get; set; }
		public string Comments { get; set; }
	}
}
