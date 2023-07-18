using System.ComponentModel.DataAnnotations;

namespace ITHelp.Models
{
    public class Status
    {
        public int Id { get; set; }
        [Display(Name ="Status")]
        public string StatusTranslated { get; set; }
    }
}
