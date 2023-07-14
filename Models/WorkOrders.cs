using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ITHelp.Models;

namespace ITHelp.Models
{
    public class WorkOrders
    {
        [Display (Name = "Work Order ID")]
        public int Id { get; set; }
        [Display(Name ="Subject")]
        public string Title { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Technician { get; set; }
        public string FullText { get; set; }
        public int? Status { get; set; }
        public string TechComments { get; set; }
        public string Phone { get; set; }
        public string Room { get; set; }
        public int? Building { get; set; }
        [Display(Name ="Service Tag")]
        public string ComputerTag { get; set; }
        public string Resolution { get; set; }
        public int? Rating { get; set; }
        public string RateComment { get; set; }
        [Display(Name ="Close Date")]
        public DateTime? CloseDate { get; set; }
        public string CreatedBy { get; set; }
        public int Difficulty { get; set; }
        public bool Review { get; set; }

        [ForeignKey("Status")]
        public Status StatusTranslate { get; set; }

        [ForeignKey("SubmittedBy")]
		public Employee Requester { get; set; }

        [ForeignKey("Technician")]
        public Employee Tech { get; set; }

        [ForeignKey("CreatedBy")]
        public Employee Creator { get; set;}

        [Display(Name ="Created By")]
        public string CreatorValue
        {
            get
            {
                if(Creator != null)
                {
                    return Creator.Name;
                } else
                {
                    return CreatedBy;
                }
                
            }
        }


    }
}
