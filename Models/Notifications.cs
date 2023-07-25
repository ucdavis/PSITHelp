namespace ITHelp.Models
{
    public class Notifications
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public int WoId { get; set; }
        public string Message { get; set; }
        public bool Pending { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Sent { get; set; }

        public Notifications()
        {
            Pending = true;
            Created = DateTime.Now;
        }
    }
}
