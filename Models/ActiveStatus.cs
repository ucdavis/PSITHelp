using System.ComponentModel.DataAnnotations;

namespace ITHelp.Models
{
    public class ActiveStatus
    {
        [Key]
        public int StatusCode { get; set; }
    }
}
