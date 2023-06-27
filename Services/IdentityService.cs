
using Microsoft.EntityFrameworkCore;
using ITHelp.Models;

namespace ITHelp.Services
{
    public interface IIdentityService
    {
        Task<Employee> GetByKerb(string kerb);
    }

    public class IdentityService : IIdentityService
    {
        private readonly ITHelpContext _context;

        public IdentityService(ITHelpContext context)
        {
            _context = context;
        }

        public async Task<Employee> GetByKerb(string kerb)
        {
            var model = await _context.Employees.Where(e => e.KerberosId == kerb).FirstOrDefaultAsync();
            return model;
        }
    }
}