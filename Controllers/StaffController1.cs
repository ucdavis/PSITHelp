using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITHelp.Controllers
{
    [Authorize (Roles = "tech,manager,student")]
    public class StaffController1 : SuperController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
