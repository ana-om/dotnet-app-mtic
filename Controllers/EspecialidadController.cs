using Microsoft.AspNetCore.Mvc;

namespace Turnos.Controllers
{
    public class EspecialidadController : Controller
    {
        public EspecialidadController()
        {

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
