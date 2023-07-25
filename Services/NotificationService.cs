﻿using ITHelp.Models;
using System.Text;

namespace ITHelp.Services
{
    public interface INotificationService
    {
        Task WorkOrderCreated(WorkOrders wo);
    }

    public class NotificationService : INotificationService
    {
        private readonly ITHelpContext _context;

        public NotificationService(ITHelpContext context) 
        {
            _context = context;
        }

        public async Task WorkOrderCreated(WorkOrders wo)
        {
            var notice = new Notifications
            {
                WoId = wo.Id,
                Email = wo.Tech.Email,
                Message = "Work Order Submitted"
            };
            _context.Add(notice);
        }
    }
}