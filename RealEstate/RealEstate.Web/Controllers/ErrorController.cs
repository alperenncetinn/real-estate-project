using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RealEstate.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult StatusCodeHandler(int statusCode)
        {
            // You can extend handling for other status codes if needed
            if (statusCode == 404)
            {
                Response.StatusCode = 404;
                return View("404");
            }

            // Fallback to generic error view
            return View("Generic");
        }

        [Route("Error")]
        public IActionResult Generic()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            ViewBag.ExceptionMessage = exceptionHandlerPathFeature?.Error.Message;
            return View("Generic");
        }
    }
}
