using System.ComponentModel.DataAnnotations;

namespace ITHelp.Models
{
    public class Employee
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //public bool Current { get; set; }
        public string KerberosId { get; set; }
        public string Role { get; set; }
        public string Phone { get; set; }
        public string Building { get; set; }
        public string Room { get; set; }
        public string Email { get; set; }

        public string Name { 
            get {
                return FirstName + " " + LastName;
            }
        }

        public string LastFirstName { 
            get {
                return LastName + ", " + FirstName;
            }
        }
    }
}