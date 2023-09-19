using ITHelp.Models;
using ITHelp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITHelp.Controllers
{
    [Authorize(Roles = "admin,manager")]
    public class ManagementController : SuperController
    {
        private readonly ITHelpContext _context;
        private readonly INotificationService _notificationService;
        private readonly IFileIOService _fileService;
        private readonly IFullCallService _fullCall;

        public ManagementController(ITHelpContext context, IFileIOService fileService, INotificationService notificationService, IFullCallService fullCall)
        {
            _context = context;
            _notificationService = notificationService;
            _fileService = fileService;
            _fullCall = fullCall;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> NewUserPermissions()
        {
            var model = await _fullCall.FullUserRequestPermission()
                .OrderBy(p => p.Current)
                .ThenBy(p=> p.PIEmployee.LastName)
                .ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> EditUserPermission(int id)
        {
            var model = await UserRequestPermissionViewModel.CreateEdit(_context, _fullCall, id);
            if(model.permission == null)
            {
                ErrorMessage = "User Request Permission not found";
                return RedirectToAction(nameof(NewUserPermissions));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserPermission(int id, UserRequestPermissionViewModel vm)
        {
            var submittedPerm = vm.permission;
            var permToUpdate = await _context.UserRequestPermissions.Where(p=> p.Id == id).FirstOrDefaultAsync();
            if(permToUpdate == null || permToUpdate.Id != submittedPerm.Id)
            {
                ErrorMessage = "User Request Permission not found";
                return RedirectToAction(nameof(NewUserPermissions));
            }

            permToUpdate.PIEmployeeId = submittedPerm.PIEmployeeId;
            permToUpdate.DelegateId = submittedPerm.DelegateId;
            permToUpdate.Current = submittedPerm.Current;
            permToUpdate.SDrive = submittedPerm.SDrive;
            permToUpdate.ADGroup =  submittedPerm.ADGroup;
            permToUpdate.BaseGroup = submittedPerm.BaseGroup;

            if (ModelState.IsValid)
            {
                await _context.SaveChangesAsync();
                Message = "User request permission updated";
            }
			return RedirectToAction(nameof(NewUserPermissions));
		}

	}
}
