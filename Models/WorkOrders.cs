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
        [Required]
        public string Title { get; set; }
        [Display(Name ="Submitted By")]
        public string SubmittedBy { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string Technician { get; set; }
        [Display(Name = "Description of problem")]
        [Required]
        public string FullText { get; set; }
        public int? Status { get; set; }
        public string TechComments { get; set; }
        [MaxLength(500)]
        public string Contact { get; set; }
        public string Room { get; set; }
        public int? Building { get; set; }
        [Display(Name ="Service Tag/Serial#")]
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

        [ForeignKey("Building")]
        public Buildings BuildingName { get; set; }

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

        [ForeignKey("WOId")]
        public ICollection<Files> Attachments { get; set; }

        [ForeignKey("WOId")]
        public ICollection<Actions> Actions { get; set; }

        public WorkOrders()
        {
            RequestDate = DateTime.Now;
            Status = 1;
            Review = false;
            Difficulty = 1;
            DueDate = getDueDate();
        }

        private DateTime getDueDate()
        {
            DateTime dueDay = DateTime.Now.Date;
            switch (DateTime.Now.DayOfWeek)
            {
                case DayOfWeek.Wednesday: dueDay = DateTime.Now.AddDays(5).Date;
                    break;
                case DayOfWeek.Thursday: dueDay = DateTime.Now.AddDays(5).Date;
                    break;
                case DayOfWeek.Friday: dueDay = DateTime.Now.AddDays(5).Date;
                    break;
                case DayOfWeek.Saturday: dueDay = DateTime.Now.AddDays(4).Date;
                    break;
                default: dueDay = DateTime.Now.AddDays(3).Date;
                    break;
            }
            return dueDay + new TimeSpan(12,0,0);
        }

    }
}
