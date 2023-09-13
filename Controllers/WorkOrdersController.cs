using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITHelp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using ITHelp.Services;
using System.Security.Claims;

namespace ITHelp.Controllers
{
    
    public class WorkOrdersController : SuperController
    {
        private readonly ITHelpContext _context;
        private readonly IFileIOService _fileService;
        private readonly INotificationService _notificationService;

        public WorkOrdersController(ITHelpContext context, IFileIOService fileService, INotificationService notificationService)
        {
            _context = context;
            _fileService = fileService;
            _notificationService = notificationService;
        }

        // GET: WorkOrders
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var model = await _context.WorkOrders
                .Include(w => w.StatusTranslate)
                .Include(w => w.Tech)
                .Where(w => w.SubmittedBy == userId).ToListAsync();
            return View(model);
        }

        // GET: WorkOrders/Details/5        
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workOrders == null || !CheckWOPermissison(workOrders))
            {
                ErrorMessage = "Work order not found or that is not your work order";
                return RedirectToAction(nameof(Index));
            }

            return View(workOrders);
        }

        public async Task<IActionResult> GetFile(int id, int attachId)
        {
            // TODO check permissions to file/wo
            var wo = await _context.WorkOrders.Where(w => w.Id == id).FirstOrDefaultAsync();
            if (wo == null || !CheckWOPermissison(wo))
            {
                ErrorMessage = "Work Order not found or you don't have permission to that work order.";
                return RedirectToAction(nameof(Index));
            }
            var attach = await _context.Files.Where(f => f.Id == attachId && f.WOId == wo.Id).FirstOrDefaultAsync();
            if(attach == null)
            {
                ErrorMessage = "File not found!";
                return RedirectToAction(nameof(Details), new { id });
            }
            var contentType = "APPLICATION/octet-stream";
            return File(_fileService.GetWorkOrderFile(wo, attach), contentType, attach.Name);
        }

        [HttpPost]
        public async Task<IActionResult> AddFile(int id, IFormFile file)
        {
            var wo = await _context.WorkOrders.Where(w => w.Id == id).FirstOrDefaultAsync();
            if (wo == null || !CheckWOPermissison(wo))
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

        [HttpPost]
        public async Task<IActionResult> AddComment(int id, string comment)
        { 
            var woToComment = await _context.WorkOrders.Include(w => w.Tech).Where(w => w.Id == id).FirstOrDefaultAsync();
            if (woToComment == null)
            {
                ErrorMessage = "Work Order not found";
                return RedirectToAction(nameof(Index));
            }
			if (!CheckWOPermissison(woToComment))
			{
				ErrorMessage = "You don't have permission to that work order.";
				return RedirectToAction(nameof(Index));
			}
            var newAction = new Actions
            {
                WOId = woToComment.Id,
                Date = DateTime.Now,
                Text = comment,
                SubmittedBy = GetUserId()
			};
			if (ModelState.IsValid)
			{
				_context.Add(newAction);				
				await _notificationService.WorkOrderCommentByClient(woToComment);
				await _context.SaveChangesAsync();
                Message = "Comment added and tech emailed";
				return RedirectToAction(nameof(Details), new { woToComment.Id });
			}
            ErrorMessage = "Something went wrong";
			return RedirectToAction(nameof(Details), new { woToComment.Id });
		}

		// GET: WorkOrders/Create
		public async Task<IActionResult> Create()
        {
            var model = await WorkOrderEditCreateViewModel.Create(_context, GetUserId());
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(WorkOrderEditCreateViewModel vm)
        {
            var woToCreate = new WorkOrders();
            var woSubmitted = vm.workOrder;
            var tech = await GetNextTechnician();
            var userId = GetUserId();
            woToCreate.Title = woSubmitted.Title;
            woToCreate.SubmittedBy = userId;
            woToCreate.CreatedBy = userId;
            woToCreate.Technician = tech.First().Id;
            woToCreate.Tech = tech.First();
            woToCreate.FullText = woSubmitted.FullText;
            woToCreate.Contact = woSubmitted.Contact;
            if (vm.UpdateContact)
            {
                var employeeContactUpdate = await _context.Preferences.Where(p => p.Id == userId).FirstOrDefaultAsync();
                if(employeeContactUpdate == null)
                {
                    employeeContactUpdate = new EmployeePreferences();
                    employeeContactUpdate.Id = userId;
                    employeeContactUpdate.ContactInfo = woSubmitted.Contact;
                    _context.Add(employeeContactUpdate); 
                } else
                {
                    employeeContactUpdate.ContactInfo = woSubmitted.Contact;
                }                
            }
            woToCreate.ComputerTag = woSubmitted.ComputerTag;
            woToCreate.Room = woSubmitted.Room;
            woToCreate.Building = woSubmitted.Building;

            if(ModelState.IsValid)
            {
                _context.Add(woToCreate);
                await _context.SaveChangesAsync();
                await _notificationService.WorkOrderCreated(woToCreate, tech.First().UCDEmail);
                await _context.SaveChangesAsync();
                if(vm.UpdateContact)
                {
                    Message = "Work Order created & Contact preferences updated.";
                } else
                {
                    Message = "Work Order created";
                }
                return RedirectToAction(nameof(Details), new { woToCreate.Id });
            }
            return View(vm);
        }

        private async Task<List<Employee>> GetNextTechnician()
        {
            return await _context.Employees.FromSqlRaw($"EXEC mvc_getnexttech").ToListAsync();
        }


       
        private bool CheckWOPermissison(WorkOrders wo)
        {
            if(User.IsInRole("student") || User.IsInRole("tech") || User.IsInRole("manager") || User.IsInRole("admin"))
            {
                return true;
            }
            var userId = GetUserId();
            if(wo.SubmittedBy == userId)
            {
                return true;
            }
            return false;
        }

        private string GetUserId()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid).Value;
        }

        private string GetUserName()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName).Value + " " + User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname).Value;
        }

        
    }
}
