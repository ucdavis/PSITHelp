using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ITHelp.Models
{
    public class AssignmentsViewModel
    {

        public List<AssignmentStats>     ticketCounts { get; set; }
        public List<AssignScheme> scheme { get; set; }
        public List<Employee> employees { get; set; }

        public static async Task<AssignmentsViewModel> Create(ITHelpContext _context)
        {

            var model = new AssignmentsViewModel
            {
                ticketCounts = await _context.AssignmentStats.FromSqlRaw($"EXEC mvc_assignment_stats").ToListAsync(),
                scheme = await _context.AssignSchemes.FromSqlRaw($"EXEC mvc_assignment_scheme_search").ToListAsync()
            };
            
            return model;
        }
		public static async Task<AssignmentsViewModel> CreateRoles(ITHelpContext _context)
		{

			var model = new AssignmentsViewModel
			{
				employees = await _context.Employees.Where(e => e.Role != "none").ToListAsync(),
				scheme = await _context.AssignSchemes.FromSqlRaw($"EXEC mvc_assignment_scheme_search").ToListAsync()
			};

			return model;
		}

	}
}

