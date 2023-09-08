using System.ComponentModel.DataAnnotations.Schema;

namespace ITHelp.Models
{
    public class Actions
    {
        public int Id { get; set; }
        public int WOId { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public string SubmittedBy { get; set; }

        [ForeignKey("SubmittedBy")]
        public Employee SubmittedEmployee { get; set; }
    }
}
