using ITHelp.Services;
using Microsoft.EntityFrameworkCore;

namespace ITHelp.Models
{
	public class WorkOrderBulkReassignViewModel
	{

        public List<WorkOrders> workOrders { get; set; }
		public List<Employee> technicians { get; set; }

		public static async Task<WorkOrderBulkReassignViewModel> Create(ITHelpContext _context, IFullCallService _fullCall)
		{

			var model = new WorkOrderBulkReassignViewModel
			{
				workOrders = await _fullCall.SummaryWO().Where(w => w.Status != 4).ToListAsync(),
				technicians = await _context.Employees.Where(x => x.Current && x.Role != "none").OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(),
			};

			return model;
		}
	}
}
