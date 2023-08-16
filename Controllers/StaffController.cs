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

        public async Task<IActionResult> ToggleReview(int id)
        {
            var woToUpdate = await _context.WorkOrders.Where(w => w.Id == id).FirstOrDefaultAsync();
            if(woToUpdate==null)
            {
                ErrorMessage = "Work Order not found.";
                return RedirectToAction(nameof(Index));
            }
            woToUpdate.Review = !woToUpdate.Review;
            await _context.SaveChangesAsync();
            Message = "Review toggled";
            return RedirectToAction(nameof(Details), new { woToUpdate.Id });
        }

        public async Task<IActionResult> Decrease(int id)
        {
            var woToUpdate = await _context.WorkOrders.Where(w => w.Id == id).FirstOrDefaultAsync();
            if(woToUpdate == null)
            {
                ErrorMessage = "Work Order not found";
                return RedirectToAction(nameof(Index));
            }
            woToUpdate.Difficulty = woToUpdate.Difficulty - 1;
            await _context.SaveChangesAsync();
            Message = "Difficulty decreased";
            return RedirectToAction(nameof(Details), new { woToUpdate.Id });
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
                return RedirectToAction(nameof(Details), new { woToCreate.Id });
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

        public async Task<IActionResult> AllOpen()
        {
            var model = await _context.WorkOrders
                .Where(w => w.Status != 4)
                .Include(w => w.StatusTranslate)
                .Include(w => w.Tech)
                .Include(w => w.Creator)
                .ToListAsync();
            return View("MyOpen",model);
        }

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.WorkOrders == null)
			{
				ErrorMessage = "Work order not found";
				return RedirectToAction(nameof(Index));
			}

			var workOrders = await _context.WorkOrders
				.Include(w => w.StatusTranslate)
				.Include(w => w.Requester)
				.Include(w => w.Tech)
				.Include(w => w.Attachments)
                .Include(w => w.BuildingName)
				.FirstOrDefaultAsync(m => m.Id == id);		

			return View(workOrders);
		}

		private string GetTechId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid).Value;
        }
    }
}
