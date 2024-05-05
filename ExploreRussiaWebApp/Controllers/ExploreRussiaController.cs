using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExploreRussiaWebApp.Controllers
{
    public abstract class ExploreRussiaController : Controller
    {
        protected int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    }
}

