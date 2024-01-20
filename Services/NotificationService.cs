using ITHelp.Models;
using System.Text;

namespace ITHelp.Services
{
    public interface INotificationService
    {
        Task WorkOrderCreated(WorkOrders wo, string techEmail, string RequesterName);
		Task NewUserRequestCreated(WorkOrders wo, string techEmail, string RequesterName);
		Task WorkOrderCommentByClient(WorkOrders wo);
        Task WorkOrderCommentByTech(WorkOrders wo, Actions action, string TechName);
        Task WorkOrderClosedByTech(WorkOrders wo, Actions action, string TechName);
        Task WorkOrderMerged(WorkOrders parentWo, WorkOrders childWo, string techName);
        Task WorkOrderChangeTech(WorkOrders wo, Employee newTech);
        Task WorkOrderBulkReassign(List<WorkOrders> workOrders, Employee newTech);
        Task WorkOrderClaimed(WorkOrders wo, Employee oldTech, string techName);

	}

    public class NotificationService : INotificationService
    {
        private readonly ITHelpContext _context;

        public NotificationService(ITHelpContext context) 
        {
            _context = context;
        }

        public async Task WorkOrderCreated(WorkOrders wo, string techEmail, string RequesterName)
        {
            var notice = new Notifications
            {
                WoId = wo.Id,
                Email = techEmail,
                Message = $"New Work Order Submitted by {RequesterName}."
            };
            _context.Add(notice);
        }

		public async Task NewUserRequestCreated(WorkOrders wo, string techEmail, string RequesterName)
		{
			var notice = new Notifications
			{
				WoId = wo.Id,
				Email = techEmail,
				Message = $"New User Request created by {RequesterName}."
			};
			_context.Add(notice);
		}

		public async Task WorkOrderCommentByClient(WorkOrders wo)
		{
			var notice = new Notifications
			{
				WoId = wo.Id,
				Email = wo.Tech.UCDEmail,
				Message = "Work Order commented by client."
			};
			_context.Add(notice);
		}
        
        public async Task WorkOrderCommentByTech(WorkOrders wo, Actions action, string TechName)
        {
            var notice = new Notifications
            {
                WoId = wo.Id,
                Email = wo.Requester.UCDEmail,
                Message = $"Work Order comment by {TechName}: {action.Text}"
            };
            _context.Add(notice);
        }
		public async Task WorkOrderClosedByTech(WorkOrders wo, Actions action, string TechName)
		{
			var notice = new Notifications
			{
				WoId = wo.Id,
				Email = wo.Requester.UCDEmail,
				Message = $"Work Order completed by {TechName}: {action.Text}"
			};
			_context.Add(notice);
		}

        public async Task WorkOrderMerged(WorkOrders parentWo, WorkOrders childWo, string techName)
        {
            var notice = new Notifications
            {
                WoId = parentWo.Id,
                Email = parentWo.Tech.UCDEmail,
                Message = $"{techName} merged the following work orders: Parent: {parentWo.Id} Child: {childWo.Id}",
            };
            _context.Add(notice);
            if(parentWo.Technician != childWo.Technician)
            {
				var secondNotice = new Notifications
				{
					WoId = parentWo.Id,
					Email = childWo.Tech.UCDEmail,
					Message = $"{techName} merged the following work orders: Parent: {parentWo.Id} Child: {childWo.Id}",
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

        public async Task WorkOrderClaimed(WorkOrders wo, Employee oldTech, string techName)
        {
            var notice = new Notifications
            {
                WoId = wo.Id,
                Email = oldTech.UCDEmail,
                Message = $"Work Order {wo.Id} claimed by {techName}."
            };
            _context.Add(notice);
        }

		public async Task WorkOrderBulkReassign(List<WorkOrders> workOrders, Employee newTech)
        {
            var sbMessage = new StringBuilder();
            sbMessage.Append($"The tech for the following Work Order(s) has changed to {newTech.Name}:<br/><br/>");
            sbMessage.Append("<table><tr><th>WO ID</th><th>Title</th><th>Previous Tech</th></tr>");
			foreach (var wo in workOrders)
            {
                sbMessage.Append($"<tr><td>{wo.Id}</td><td>{wo.Title}</td><td>{wo.Tech.Name}</td></tr>");
            }
            sbMessage.Append("</table>");
            var distinctOldTechs = workOrders.Select(w => w.Tech).Distinct();
            foreach(var tech in distinctOldTechs)
            {
                var notice = new Notifications
                {
                    WoId = workOrders.First().Id,
                    Email = tech.UCDEmail,
                    Message = sbMessage.ToString()
                };
                _context.Add(notice);
            }
            var newTechNotice = new Notifications
            {
                WoId = workOrders.First().Id,
                Email = newTech.UCDEmail,
                Message= sbMessage.ToString()
            }
            ; _context.Add(newTechNotice);            
        }


	}
}
