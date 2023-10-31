using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ITHelp.Models
{
    public class NewUserRequestViewModel
    {

        public NewUserRequest request { get; set; }
        public List<SelectListItem> groups { get; set; }


        public static async Task<NewUserRequestViewModel> Create(ITHelpContext _context, string userId)
        {
            var p0 = new SqlParameter("@employee_id", System.Data.SqlDbType.VarChar);
            p0.SqlValue = userId;
            var groupList = await _context.UserRequestPermissions.FromSqlRaw($"EXEC mvc_check_user_group_permissions @employee_id", p0).ToListAsync();
            var model = new NewUserRequestViewModel
            {
               request = new NewUserRequest(),
               groups = groupList.Select(x => new SelectListItem { Value = $"{x.ADGroup}--{x.BaseGroup}--{x.SDrive}", Text = x.ADGroup}).ToList(),
            };

            return model;
        }

    }
}
