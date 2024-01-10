using ITHelp.Models;
using ITHelp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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

        public async Task<IActionResult> Search()
        {
            var model = await WorkOrderSearchViewModel.Create(_context, null, _fullCall);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Search(WorkOrderSearchViewModel vm)
        {
            var model = await WorkOrderSearchViewModel.Create(_context, vm, _fullCall);
            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var model = await WorkOrderEditCreateViewModel.CreateAdmin(_context);
            return View(model);
        }

        public async Task<IActionResult> Assignments()
        {
            var model = await AssignmentsViewModel.Create(_context);
            return View(model);
        }

        public async Task<IActionResult> MigratedGroups()
        {
            var model = await _context.MigratedGroups.OrderBy(w => w.LastName).ThenBy(w => w.FirstName).ToListAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> NewGroup(string firstName, string lastName)
        {
            var newGroup = new MigratedGroups
            {
                FirstName = firstName,
                LastName = lastName
            };
            if (ModelState.IsValid)
            {
                _context.Add(newGroup);
                await _context.SaveChangesAsync();
                Message = "New group created";
                return RedirectToAction(nameof(MigratedGroups));
            }
			var model = await _context.MigratedGroups.OrderBy(w => w.LastName).ThenBy(w => w.FirstName).ToListAsync();
			return View(model);
		}

        public async Task<IActionResult> EditGroup(int id)
        {
            var model = await _context.MigratedGroups.Where(g => g.Id == id).FirstOrDefaultAsync();
            if(model == null)
            {
                ErrorMessage = "Group not found";
                return RedirectToAction(nameof(MigratedGroups));
            }
            return View(model);
        }

        [HttpPost]
		public async Task<IActionResult> EditGroup(int id, MigratedGroups update)
		{
            var groupToUpdate = await _context.MigratedGroups.Where(g => g.Id == id).FirstOrDefaultAsync();
			if (groupToUpdate == null || groupToUpdate.Id != update.Id)
			{
				ErrorMessage = "Group not found";
				return RedirectToAction(nameof(MigratedGroups));
			}
            groupToUpdate.FirstName = update.FirstName;
            groupToUpdate.LastName = update.LastName;   

            if(ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                Message = "Group updated";
                return RedirectToAction(nameof(MigratedGroups));
            }

			return View(update);
		}

        public async Task<IActionResult> UserPermissions()
        {
            var model = await _context.UserReequestPermissionsSummary.FromSqlRaw($"EXEC mvc_new_user_permission_list").ToListAsync();
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

        [HttpPost]
        public async Task<IActionResult> CompleteMerge(int parentId, int childId)
        {
            if(parentId == childId || parentId == 0 || childId == 0)
            {
                ErrorMessage = "Work Order ID's either equal or set to zero";
                return RedirectToAction(nameof(Index));
            }
            var parentWo = await _context.WorkOrders.Include(w=> w.Tech).Where(w => w.Id == parentId).FirstOrDefaultAsync();
            var childWo = await _context.WorkOrders.Include(w=> w.Tech).Where(w => w.Id == childId).FirstOrDefaultAsync();

			if (parentWo == null || childWo == null)
			{
				ErrorMessage = "Work Order not found!";
				return RedirectToAction(nameof(Index));
			}
			var childActions = await _context.Actions.Where(w => w.WOId == childId).ToListAsync();
            var who = GetTechId();
            
            childActions.ForEach(a => a.WOId = parentId);
			childWo.Status = 4;

            var ParentComment = new Actions
            {
                WOId = parentId,
                Date = DateTime.Now,
                Text = $"Text from merged WO ID: {childWo.Id} Description: {childWo.FullText}",
                SubmittedBy = who,
            };
            var ChildComment = new Actions
            {
                WOId = childId,
                Date = DateTime.Now,
                Text = $"Work Order merged with WO ID: {parentWo.Id}",
                SubmittedBy = who,
            };
            _context.Add(ParentComment);
            _context.Add(ChildComment);
            await _notificationService.WorkOrderMerged(parentWo, childWo, GetTechName());
            await _context.SaveChangesAsync();

			Message = "Merge completed";
			return RedirectToAction(nameof(Details), new { parentWo.Id });
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
            var DecreaseComment = new Actions
            {
                WOId = woToUpdate.Id,
                Date = DateTime.Now,
                Text = $"Difficulty/weight lowered: From: {woToUpdate.Difficulty}",
                SubmittedBy = GetTechId(),
            };
            woToUpdate.Difficulty = woToUpdate.Difficulty - 1;

            _context.Add(DecreaseComment);
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
            var model = await _fullCall.SummaryWO().Where(w => w.Technician == GetTechId() && w.Status != 4).ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> AllOpen()
        {
            var model = await _fullCall.SummaryWO().Where(w => w.Status != 4).ToListAsync();
            return View("MyOpen",model);
        }

		public async Task<IActionResult> Details(int id)
		{
			var workOrders = await _fullCall.FullWO().FirstOrDefaultAsync(m => m.Id == id);
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

        private string GetTechName()
        {
            var firstName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName).Value;
            var lastName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname).Value;
			return $"{firstName} {lastName}";
		}
    }
}
