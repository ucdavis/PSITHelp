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
        Task WorkOrderMerged(WorkOrders parentWo, WorkOrders childWo, string techName);
        Task WorkOrderChangeTech(WorkOrders wo, Employee newTech);

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

        public async Task WorkOrderMerged(WorkOrders parentWo, WorkOrders childWo, string techName)
        {
            var notice = new Notifications
            {
                WoId = parentWo.Id,
                Email = parentWo.Tech.UCDEmail,
                Message = $"{techName} Merged the following work orders: Parent: {parentWo.Id} Child: {childWo.Id}",
            };
            _context.Add(notice);
            if(parentWo.Technician != childWo.Technician)
            {
				var secondNotice = new Notifications
				{
					WoId = parentWo.Id,
					Email = childWo.Tech.UCDEmail,
					Message = $"{techName} Merged the following work orders: Parent: {parentWo.Id} Child: {childWo.Id}",
				};
                _context.Add(secondNotice);
			}
        }

        public async Task WorkOrderChangeTech(WorkOrders wo, Employee newTech)
        {
            var notice = new Notifications
            {
                WoId = wo.Id,
                Email = wo.Tech.UCDEmail,
                Message = $"Work Order {wo.Id} transfered from you to {newTech.Name}; Comment: {wo.TechComments}"
            };
            _context.Add(notice);
            var secondNotice = new Notifications
            {
                WoId = wo.Id,
                Email = newTech.UCDEmail,
                Message = $"Work Order {wo.Id} transfered from {wo.Tech.FirstName} to you; Comment: {wo.TechComments}"
            };
            _context.Add(secondNotice);
        }


	}
}
