﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ITHelp.Models
{
	public class WorkOrderEditCreateViewModel
	{

        public WorkOrders workOrder { get; set; }
        public List<Buildings> buildings { get; set; }
        [Display(Name ="Save contact to preferences?")]
        public bool UpdateContact { get; set; }
        public List<Employee> employees { get; set; }
        public List<Employee> technicians { get; set; }

        public WorkOrderEditCreateViewModel()
        {
            workOrder = new WorkOrders();
            UpdateContact = false;
        }


        public static async Task<WorkOrderEditCreateViewModel> Create(ITHelpContext _context, string userId)
        {

            var model = new WorkOrderEditCreateViewModel
            {                
                buildings = await _context.Buildings.OrderBy(x => x.Id).ToListAsync(),                
            };
            var pref = await _context.Preferences.Where(p  => p.Id == userId).FirstOrDefaultAsync();
            if(pref != null)
            {
                model.workOrder.Contact = pref.ContactInfo;
            } else {
                var employee = await _context.Employees.Where(e => e.Id == userId).FirstOrDefaultAsync();
                if(employee != null)
                {
                    model.workOrder.Contact = $"Phone: {employee.Phone}; Location: {employee.Room} {employee.Building}";
                }
            }
            return model;
        }

        public static async Task<WorkOrderEditCreateViewModel> CreateAdmin(ITHelpContext _context)
        {

            var model = new WorkOrderEditCreateViewModel
            {
                buildings = await _context.Buildings.OrderBy(x => x.Id).ToListAsync(),
                employees = await _context.Employees.Where(x => x.Current).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(),
            };
            
            return model;
        }

        public static async Task<WorkOrderEditCreateViewModel> EditAdmin(ITHelpContext _context, int id)
        {

            var model = new WorkOrderEditCreateViewModel
            {
                workOrder = await _context.WorkOrders
                .Include(w => w.StatusTranslate)
                .Include(w => w.Requester)
                .Include(w => w.Tech)
                .Include(w => w.BuildingName)
                .FirstOrDefaultAsync(m => m.Id == id),
                buildings = await _context.Buildings.OrderBy(x => x.Id).ToListAsync(),
                employees = await _context.Employees.Where(x => x.Current).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(),
            };

            return model;
        }

		public static async Task<WorkOrderEditCreateViewModel> EditManagement(ITHelpContext _context, int id)
		{

			var model = new WorkOrderEditCreateViewModel
			{
				workOrder = await _context.WorkOrders
				.Include(w => w.StatusTranslate)
				.Include(w => w.Requester)
				.Include(w => w.Tech)
                .Include(w => w.Actions)
				.FirstOrDefaultAsync(m => m.Id == id),
				technicians = await _context.Employees.Where(x => x.Current && x.Role != "none").OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(),
			};

			return model;
		}
	}
}
