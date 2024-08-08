using ITHelp.Services;
using Microsoft.EntityFrameworkCore;


namespace ITHelp.Models
{
    public class WorkOrderReviewsViewModel
    {
        public List<WorkOrders> workOrders { get; set; }
        public List<TechRatingSummary> ratingsSummary { get; set; }

        public static async Task<WorkOrderReviewsViewModel> Create(ITHelpContext _context)
        {

            var model = new WorkOrderReviewsViewModel
            {
                ratingsSummary = await _context.TechRatingSummaries.FromSqlRaw($"EXEC mvc_tech_rate_summary").ToListAsync(),
            };

            return model;
        }

        public static async Task<WorkOrderReviewsViewModel> CreateEmployee(ITHelpContext _context, string Id)
        {

            var model = new WorkOrderReviewsViewModel
            {
                ratingsSummary = await _context.TechRatingSummaries.FromSqlRaw($"EXEC mvc_tech_rate_summary").ToListAsync(),
                workOrders = await _context.WorkOrders.Where(w => w.Technician == Id && w.Rating != null).ToListAsync(),
            };

            return model;
        }
    }
}
