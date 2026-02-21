using Microsoft.AspNetCore.Authorization;

namespace MiWebCafe.API.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ControllerSeguridad
    {
    }
}
