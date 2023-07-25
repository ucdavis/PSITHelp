using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITHelp.Controllers
{
    [Authorize(Roles = "admin,manager")]
    public class ManagementController : SuperController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
