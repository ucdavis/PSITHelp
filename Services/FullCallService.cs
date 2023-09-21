using System.Linq;
using ITHelp.Models;
using Microsoft.EntityFrameworkCore;

namespace ITHelp.Services
{
	public interface IFullCallService
	{
		IQueryable<WorkOrders> FullWO();
		IQueryable<WorkOrders> SummaryWO();
		IQueryable<UserRequestPermissions> FullUserRequestPermission();
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
			return _context.WorkOrders
				.Include(w => w.Requester)
				.Include(w => w.Tech)
				.Include(w => w.StatusTranslate)
				.Include(w=> w.Creator)
				.Include(w=> w.BuildingName)
				.Include(w=> w.Attachments)
				.Include(w => w.Actions)
				.ThenInclude(w => w.SubmittedEmployee)
				.AsQueryable();
		}

		public IQueryable<WorkOrders> SummaryWO()
		{
			return _context.WorkOrders
				.Include(w => w.Requester)
				.Include(w => w.Tech)
				.Include(w => w.StatusTranslate)
				.Include(w => w.Creator)
				.AsQueryable();
		}

		public IQueryable<UserRequestPermissions> FullUserRequestPermission()
		{
			return _context.UserRequestPermissions
				.Include(p => p.PIEmployee)
				.Include(p => p.DelegateEmployee)
				.AsQueryable();
		}
	}
}
