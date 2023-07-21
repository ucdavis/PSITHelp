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

        public WorkOrdersController(ITHelpContext context, IFileIOService fileService)
        {
            _context = context;
            _fileService = fileService;
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

        // GET: WorkOrders/Create
        public async Task<IActionResult> Create()
        {
            var model = await WorkOrderEditCreateViewModel.Create(_context, GetUserId());
            return View(model);
        }

        // POST: WorkOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,SubmittedBy,RequestDate,Technician,FullText,Status,TechComments,Phone,Room,Building,ComputerTag,Resolution,Rating,RateComment,CloseDate,CreatedBy,Difficulty,Review")] WorkOrders workOrders)
        {
            if (ModelState.IsValid)
            {
                _context.Add(workOrders);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Status"] = new SelectList(_context.Set<Status>(), "Id", "Id", workOrders.Status);
            return View(workOrders);
        }

        // GET: WorkOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.WorkOrders == null)
            {
                return NotFound();
            }

            var workOrders = await _context.WorkOrders.FindAsync(id);
            if (workOrders == null)
            {
                return NotFound();
            }
            ViewData["Status"] = new SelectList(_context.Set<Status>(), "Id", "Id", workOrders.Status);
            return View(workOrders);
        }

        // POST: WorkOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,SubmittedBy,RequestDate,Technician,FullText,Status,TechComments,Phone,Room,Building,ComputerTag,Resolution,Rating,RateComment,CloseDate,CreatedBy,Difficulty,Review")] WorkOrders workOrders)
        {
            if (id != workOrders.Id)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            //    try
            //    {
            //        _context.Update(workOrders);
            //        await _context.SaveChangesAsync();
            //    }
            //    catch (DbUpdateConcurrencyException)
            //    {
            //        if (!WorkOrdersExists(workOrders.Id))
            //        {
            //            return NotFound();
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }
            //    return RedirectToAction(nameof(Index));
            //}
            ViewData["Status"] = new SelectList(_context.Set<Status>(), "Id", "Id", workOrders.Status);
            return View(workOrders);
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

        
    }
}
