namespace ITHelp.Models
{
    public class AssignScheme
    {
        public bool AssignRoundRobin { get; set; }
        public DateTime ResetDate { get; set; }

        public string NextTech { get; set; }
    }

    public class AssignmentSchemeTable
    {
        public int Id { get; set; }
        public bool AssignRoundRobin { get; set; }
        public DateTime ResetDate { get; set; }
       
    }
}
