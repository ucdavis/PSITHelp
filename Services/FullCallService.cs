using System.Linq;
using ITHelp.Models;
using Microsoft.EntityFrameworkCore;

namespace ITHelp.Services
{
	public interface IFullCallService
	{
		IQueryable<WorkOrders> FullWO();
		IQueryable<WorkOrders> SummaryWO();
	}

	public class FullCallService : IFullCallService
	{
		private readonly ITHelpContext _context;

		public FullCallService(ITHelpContext context)
		{
			_context = context;
		}

		public IQueryable<WorkOrders> FullWO()
		{
			var wo = _context.WorkOrders
				.Include(w => w.Requester)
				.Include(w => w.Tech)
				.Include(w => w.StatusTranslate)
				.Include(w=> w.Creator)
				.Include(w=> w.BuildingName)
				.Include(w=> w.Attachments)
				.Include(w => w.Actions)
				.ThenInclude(w => w.SubmittedEmployee)
				.AsQueryable();
			return wo;
		}

		public IQueryable<WorkOrders> SummaryWO()
		{
			var wo = _context.WorkOrders
				.Include(w => w.Requester)
				.Include(w => w.Tech)
				.Include(w => w.StatusTranslate)
				.Include(w => w.Creator)
				.AsQueryable();
			return wo;
		}
	}
}
