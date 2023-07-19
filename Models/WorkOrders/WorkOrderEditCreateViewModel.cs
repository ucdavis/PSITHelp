using Microsoft.EntityFrameworkCore;

namespace ITHelp.Models
{
	public class WorkOrderEditCreateViewModel
	{

        public WorkOrders workOrder { get; set; }
        public List<Buildings> buildings { get; set; }


        public static async Task<WorkOrderEditCreateViewModel> Create(ITHelpContext _context)
        {
            var model = new WorkOrderEditCreateViewModel
            {
                workOrder = new WorkOrders(),
                buildings = await _context.Buildings.OrderBy(x => x.Id).ToListAsync(),
            };
            return model;
        }
    }
}
