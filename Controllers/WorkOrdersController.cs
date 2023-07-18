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
            var iTHelpContext = _context.WorkOrders.Include(w => w.StatusTranslate);
            return View(await iTHelpContext.ToListAsync());
        }

        // GET: WorkOrders/Details/5        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.WorkOrders == null)
            {
                return NotFound();
            }

            var workOrders = await _context.WorkOrders
                .Include(w => w.StatusTranslate)
                .Include(w => w.Requester)
                .Include(w => w.Tech)
                .Include(w => w.Attachments)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workOrders == null)
            {
                return NotFound();
            }

            return View(workOrders);
        }

        public async Task<IActionResult> GetFile(int id, int attachId)
        {
            // TODO check permissions to file/wo
            var wo = await _context.WorkOrders.Where(w => w.Id == id).FirstOrDefaultAsync();
            if (wo == null)
            {
                ErrorMessage = "Work Order not found";
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
            if (wo == null)
            {
                ErrorMessage = "Work Order not found";
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
        public IActionResult Create()
        {
            ViewData["Status"] = new SelectList(_context.Set<Status>(), "Id", "Id");
            return View();
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workOrders);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkOrdersExists(workOrders.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Status"] = new SelectList(_context.Set<Status>(), "Id", "Id", workOrders.Status);
            return View(workOrders);
        }

        // GET: WorkOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.WorkOrders == null)
            {
                return NotFound();
            }

            var workOrders = await _context.WorkOrders
                .Include(w => w.StatusTranslate)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workOrders == null)
            {
                return NotFound();
            }

            return View(workOrders);
        }

        // POST: WorkOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.WorkOrders == null)
            {
                return Problem("Entity set 'ITHelpContext.WorkOrders'  is null.");
            }
            var workOrders = await _context.WorkOrders.FindAsync(id);
            if (workOrders != null)
            {
                _context.WorkOrders.Remove(workOrders);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkOrdersExists(int id)
        {
          return _context.WorkOrders.Any(e => e.Id == id);
        }
    }
}
