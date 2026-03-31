using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using GbbExpender.Utils;

namespace GbbExpender.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetConfig()
        {
            try
            {
                var settings = ConfigHelper.GetAppSettings();
                return Ok(settings);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { status = "Error", message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateConfig([FromBody] Dictionary<string, string> newSettings)
        {
            try
            {
                ConfigHelper.UpdateAppSettings(newSettings);
                return Ok(new { status = "Success", message = "Configuration updated successfully." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { status = "Error", message = ex.Message });
            }
        }
    }
}
