using ITHelp.Models;
using System.Text;

namespace ITHelp.Services
{
    public interface INotificationService
    {
        Task WorkOrderCreated(WorkOrders wo, string techEmail);
        Task WorkOrderCommentByClient(WorkOrders wo);
        Task WorkOrderCommentByTech(WorkOrders wo, Actions action);
        Task WorkOrderClosedByTech(WorkOrders wo, Actions action);

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
        
        public async Task WorkOrderCommentByTech(WorkOrders wo, Actions action)
        {
            var notice = new Notifications
            {
                WoId = wo.Id,
                Email = wo.Requester.UCDEmail,
                Message = $"Work Order comment by tech: {action.Text}"
            };
            _context.Add(notice);
        }
		public async Task WorkOrderClosedByTech(WorkOrders wo, Actions action)
		{
			var notice = new Notifications
			{
				WoId = wo.Id,
				Email = wo.Requester.UCDEmail,
				Message = $"Work Order completed by tech: {action.Text}"
			};
			_context.Add(notice);
		}
	}
}
