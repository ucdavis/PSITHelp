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
        private readonly IFullCallService _fullCall;

        public ManagementController(ITHelpContext context, INotificationService notificationService, IFullCallService fullCall)
        {
            _context = context;
            _notificationService = notificationService;
            _fullCall = fullCall;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Membership()
        {
            var model = await _context.ManEmployees.ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> EditMember(string id)
        {
            var model = await _context.ManEmployees.Where(m => m.Id == id).FirstOrDefaultAsync();
            return View(model);
        }


		public async Task<IActionResult> ChangeTechOrRating(int id)
        {
            var model = await WorkOrderEditCreateViewModel.EditManagement(_context, id);
            if(model.workOrder == null)
            {
                ErrorMessage = "Work Order not found";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeTechOrRating(int id, WorkOrderEditCreateViewModel vm)
        {
            var updatedWo = vm.workOrder;
            var workOrderToUpdate = await _context.WorkOrders.Include(w=>w.Tech).Where(w => w.Id == id).FirstOrDefaultAsync();
            if(workOrderToUpdate == null || workOrderToUpdate.Id != updatedWo.Id)
            {
                ErrorMessage = "Work Order not found";
                return RedirectToAction(nameof(Index));
            }
			workOrderToUpdate.Difficulty = updatedWo.Difficulty;
            workOrderToUpdate.TechComments = updatedWo.TechComments;
			if (workOrderToUpdate.Technician != updatedWo.Technician)
            {
                var tech = await _context.Employees.Where(e => e.Id == updatedWo.Technician).FirstOrDefaultAsync();
                await _notificationService.WorkOrderChangeTech(workOrderToUpdate, tech);
				workOrderToUpdate.Technician = updatedWo.Technician;
			}
            await _context.SaveChangesAsync();
            Message = "Updates saved";
            return RedirectToAction("Details","Staff",new {workOrderToUpdate.Id});
        }

        public async Task<IActionResult> BulkReassign()
        {
            var model = await WorkOrderBulkReassignViewModel.Create(_context, _fullCall);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BulkReassign(int[] reassign, string Technician)
        {
            var workOrdersToReassign = await _context.WorkOrders.Include(w => w.Tech).Where(w => reassign.Contains(w.Id)).ToListAsync();
			var newTech = await _context.Employees.Where(e => e.Id == Technician).FirstOrDefaultAsync();
            await _notificationService.WorkOrderBulkReassign(workOrdersToReassign, newTech);
            workOrdersToReassign.ForEach(w=> w.Technician = Technician);
            await _context.SaveChangesAsync();
            Message = "Work Order(s) reassigned";
            return RedirectToAction(nameof(BulkReassign));
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
				return RedirectToAction(nameof(NewUserPermissions));
			}
            ErrorMessage = "Something went wrong";
			var model = await UserRequestPermissionViewModel.CreateEdit(_context, _fullCall, id);
			return View(model);
		}

        public async Task<IActionResult> NewUserPermission()
        {
			var model = await UserRequestPermissionViewModel.CreateNew(_context);
            return View(model);
		}

        [HttpPost]
        public async Task<IActionResult> NewUserPermission(UserRequestPermissionViewModel vm)
        {
            var newPermission = vm.permission;
            var permissionToCreate = new UserRequestPermissions();

			permissionToCreate.PIEmployeeId = newPermission.PIEmployeeId;
			permissionToCreate.DelegateId = newPermission.DelegateId;
			permissionToCreate.Current = true;
			permissionToCreate.SDrive = newPermission.SDrive;
			permissionToCreate.ADGroup = newPermission.ADGroup;
			permissionToCreate.BaseGroup = newPermission.BaseGroup;

            if(ModelState.IsValid)
            {
                _context.Add(permissionToCreate);
                _context.SaveChangesAsync();
                Message = "User request permission created";
                return RedirectToAction(nameof(NewUserPermissions));
			}
            ErrorMessage = "Something went wrong";
			var model = await UserRequestPermissionViewModel.CreateNew(_context);
			return View(model);
		}

	}
}
