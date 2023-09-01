using ITHelp.Models;
using System.Text;

namespace ITHelp.Services
{
    public interface INotificationService
    {
        Task WorkOrderCreated(WorkOrders wo, string techEmail);
        Task WorkOrderCommentByClient(WorkOrders wo);
    }

    public class NotificationService : INotificationService
    {
        private readonly ITHelpContext _context;

        public NotificationService(ITHelpContext context) 
        {
            _context = context;
        }

        public async Task WorkOrderCreated(WorkOrders wo, string techEmail)
        {
            var notice = new Notifications
            {
                WoId = wo.Id,
                Email = techEmail,
                Message = "Work Order Submitted"
            };
            _context.Add(notice);
        }

		public async Task WorkOrderCommentByClient(WorkOrders wo)
		{
			var notice = new Notifications
			{
				WoId = wo.Id,
				Email = wo.Tech.UCDEmail,
				Message = "Work Order commented by client"
			};
			_context.Add(notice);
		}
	}
}
