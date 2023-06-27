using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITHelp.Controllers
{
    [Authorize(Roles = "tech,manager,student")]
    public class ManagementController1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
