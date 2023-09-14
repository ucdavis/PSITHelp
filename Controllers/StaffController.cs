using ITHelp.Models;
using ITHelp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ITHelp.Controllers
{
    [Authorize (Roles = "tech,manager,student,admin")]
    public class StaffController : SuperController
    {
        private readonly ITHelpContext _context;        
        private readonly INotificationService _notificationService;
		private readonly IFileIOService _fileService;
        private readonly IFullCallService _fullCall;

		public StaffController(ITHelpContext context, IFileIOService fileService, INotificationService notificationService, IFullCallService fullCall)
        {
            _context = context;
            _notificationService = notificationService;
            _fileService = fileService;
            _fullCall = fullCall;
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

        public async Task<IActionResult> Edit(int id)
        {
            var model = await WorkOrderEditCreateViewModel.EditAdmin(_context, id);
            if (model.workOrder == null)
            {
                ErrorMessage = "Work order not found";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, WorkOrderEditCreateViewModel vm)
        {
            var workOrderEditted = vm.workOrder;
            var workOrderToUpdate = await _context.WorkOrders.Where(w => w.Id == id).FirstOrDefaultAsync();
			if (workOrderToUpdate == null || workOrderToUpdate.Id != workOrderEditted.Id)
			{
				ErrorMessage = "Work order not found";
				return RedirectToAction(nameof(Index));
			}

            workOrderToUpdate.SubmittedBy = workOrderEditted.SubmittedBy;
            workOrderToUpdate.Contact = workOrderEditted.Contact;
            workOrderToUpdate.Room = workOrderEditted.Room;
            workOrderToUpdate.Building = workOrderEditted.Building;
            workOrderToUpdate.ComputerTag = workOrderEditted?.ComputerTag;

			var validationErrors = new List<ValidationResult>();
			if (Validator.TryValidateObject(workOrderToUpdate, new ValidationContext(workOrderToUpdate), validationErrors, validateAllProperties: true))
			{
				await _context.SaveChangesAsync();
				Message = "Work Order updated";
				return RedirectToAction(nameof(Details), new { workOrderToUpdate.Id });
			}

			var model = await WorkOrderEditCreateViewModel.EditAdmin(_context, id);
            ErrorMessage = "Something is wrong!";
			return View(model);

		}

        public async Task<IActionResult> Merge(int id)
        {
            var model = await _fullCall.SummaryWO().Where(w => w.Id == id).FirstOrDefaultAsync();
			if (model == null)
			{
				ErrorMessage = "Work Order not found.";
				return RedirectToAction(nameof(Index));
			}
            return View(model);
		}

        public async Task<IActionResult> GetWOSummary(int id)
        {
			var model = await _fullCall.SummaryWO().Where(w => w.Id == id).FirstOrDefaultAsync();
			if (model == null)
            {                
                return Content("Work Order not found!");
            }
            return PartialView("_WorkOrderSummary", model);
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

		public async Task<IActionResult> Details(int id)
		{
			var workOrders = await _context.WorkOrders
				.Include(w => w.StatusTranslate)
				.Include(w => w.Requester)
				.Include(w => w.Tech)
				.Include(w => w.Attachments)
                .Include(w => w.BuildingName)
                .Include(w => w.Actions)
                .ThenInclude(w => w.SubmittedEmployee)
				.FirstOrDefaultAsync(m => m.Id == id);

            if (workOrders == null)
            {
                ErrorMessage = "Work order not found";
                return RedirectToAction(nameof(Index));
            }

            return View(workOrders);
		}

		[HttpPost]
		public async Task<IActionResult> AddComment(int id, string comment, string emailRequestor, int statusChange, string serviceTag)
		{
			if(string.IsNullOrWhiteSpace(comment))
            {
                ErrorMessage = "Comment cannot be blank";
                return RedirectToAction(nameof(Details), new {id});
            }
            var woToComment = await _context.WorkOrders.Include(w => w.Requester).Where(w => w.Id == id).FirstOrDefaultAsync();
			if (woToComment == null)
			{
				ErrorMessage = "Work Order not found";
				return RedirectToAction(nameof(Index));
			}
            var newAction = new Actions
            {
                WOId = woToComment.Id,
                Date = DateTime.Now,
                Text = comment,
                SubmittedBy = GetTechId()
			};
            if(!string.IsNullOrWhiteSpace(serviceTag) && woToComment.ComputerTag != serviceTag)
            {
                woToComment.ComputerTag = serviceTag;
            }            
            if(statusChange != 0)
            {
                woToComment.Status = statusChange;
            }
            if(emailRequestor == "Yes" && statusChange != 4)
            {
                await _notificationService.WorkOrderCommentByTech(woToComment, newAction);
            }
            else if(statusChange == 4)
            {
                await _notificationService.WorkOrderClosedByTech(woToComment, newAction);
                woToComment.Resolution = comment;
            }
			if (ModelState.IsValid)
			{
				_context.Add(newAction);				
				await _context.SaveChangesAsync();
				Message = "Comment added.";
				return RedirectToAction(nameof(Details), new { woToComment.Id });
			}
			ErrorMessage = "Something went wrong";
			return RedirectToAction(nameof(Details), new { woToComment.Id });
		}

		[HttpPost]
		public async Task<IActionResult> AddFile(int id, IFormFile file)
		{
			var wo = await _context.WorkOrders.Where(w => w.Id == id).FirstOrDefaultAsync();
			if (wo == null)
			{
				ErrorMessage = "Work Order not found or you do not have permission to that work order.";
				return RedirectToAction(nameof(Index));
			}

			var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

			if (_fileService.CheckDeniedExtension(ext))
			{
				ErrorMessage = "File extension not allowed!";
				return RedirectToAction(nameof(Details), new { id });
			}

			if (file.Length > 0)
			{

				var attach = new Files();
				attach.WOId = wo.Id;
				attach.Name = file.FileName;
				attach.Extension = ext;
				_context.Add(attach);
				await _context.SaveChangesAsync();
				await _fileService.SaveWorkOrderFile(wo, attach.Id, file);
				Message = "File uploaded";
			}
			return RedirectToAction(nameof(Details), new { id });
		}

		private string GetTechId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid).Value;
        }
    }
}
