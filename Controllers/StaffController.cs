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

        [HttpPost]
        public async Task<IActionResult> Create(WorkOrderEditCreateViewModel vm)
        {
            var woToCreate = new WorkOrders();
            var woSubmitted = vm.workOrder;
            var techId = GetTechId();
            woToCreate.Title = woSubmitted.Title;
            woToCreate.SubmittedBy = woSubmitted.SubmittedBy;
            woToCreate.CreatedBy = techId;
            woToCreate.Technician = techId;
            woToCreate.FullText = woSubmitted.FullText;
            woToCreate.Contact = woSubmitted.Contact;            
            woToCreate.ComputerTag = woSubmitted.ComputerTag;
            woToCreate.Room = woSubmitted.Room;
            woToCreate.Building = woSubmitted.Building;

            if (ModelState.IsValid)
            {
                _context.Add(woToCreate);
                await _context.SaveChangesAsync();                
                Message = "Work Order created";               
                return RedirectToAction("Details", "WorkOrders", new { woToCreate.Id });
            }
            return View(vm);
        }

        public async Task<IActionResult> MyOpen()
        {
            var model = await _context.WorkOrders
                .Where(w => w.Technician == GetTechId() && w.Status != 4)
                .Include(w => w.StatusTranslate)
                .Include(w => w.Tech)
                .Include(w => w.Creator)
                .ToListAsync();
            return View(model);
        }

        private string GetTechId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid).Value;
        }
    }
}
