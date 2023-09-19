using ITHelp.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ITHelp.Models
{
    public class UserRequestPermissionViewModel
    {
        public UserRequestPermissions permission { get; set; }

        public List<Employee> employees { get; set; }

        public static async Task<UserRequestPermissionViewModel> CreateEdit(ITHelpContext _context, IFullCallService _fullCall, int id)
        {

            var model = new UserRequestPermissionViewModel
			{
                permission = await _fullCall.FullUserRequestPermission().Where(p => p.Id == id).FirstOrDefaultAsync(),
                employees = await _context.Employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync()
            };
            
            return model;
        }        
    }
}

