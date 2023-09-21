using System.ComponentModel.DataAnnotations.Schema;

namespace ITHelp.Models
{
    public class UserRequestPermissions
    {
        public int Id { get; set; }
        public string PIEmployeeId { get; set; }
        public string DelegateId { get; set; }
        public bool Current { get; set; }
        public bool SDrive { get; set; }
        public string ADGroup { get; set; }
        public string BaseGroup { get; set; }

        [ForeignKey("PIEmployeeId")]
        public Employee PIEmployee { get; set; }

        [ForeignKey("DelegateId")]
        public Employee DelegateEmployee { get; set; }
    }
}
