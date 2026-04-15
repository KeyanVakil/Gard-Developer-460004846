using Microsoft.AspNetCore.Mvc;

namespace GardPortal.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
