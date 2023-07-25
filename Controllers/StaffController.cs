using ITHelp.Models;
using ITHelp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITHelp.Controllers
{
    [Authorize (Roles = "tech,manager,student,admin")]
    public class StaffController : SuperController
    {
        private readonly ITHelpContext _context;        
        private readonly INotificationService _notificationService;

        public StaffController(ITHelpContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Create()
        {
            var model = await WorkOrderEditCreateViewModel.CreateAdmin(_context);
            return View(model);
        }

        private string GetTechId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid).Value;
        }
    }
}
