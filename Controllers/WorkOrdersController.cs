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

namespace ITHelp.Controllers
{
    
    public class WorkOrdersController : SuperController
    {
        private readonly ITHelpContext _context;

        public WorkOrdersController(ITHelpContext context)
        {
            _context = context;
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workOrders == null)
            {
                return NotFound();
            }

            return View(workOrders);
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
