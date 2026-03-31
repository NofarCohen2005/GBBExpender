using Microsoft.AspNetCore.Mvc;
using GbbExpender.Models;
using GbbExpender.Services;

namespace GbbExpender.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GbbController : ControllerBase
    {
        private readonly GbbGeneratorService _generatorService;

        public GbbController(GbbGeneratorService generatorService)
        {
            _generatorService = generatorService;
        }

        [HttpPost("generate")]
        public ActionResult<dynamic> Generate([FromBody] GeneratorRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ObjectName))
                return BadRequest("Invalid request");

            // Normalization
            NormalizeRequest(request);

            try
            {
                _generatorService.Generate(request);
                return Ok(new { 
                    Status = "Success", 
                    Message = "Files generated and registration updated successfully.",
                    ObjectName = request.ObjectName
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error saving files: {ex.Message}");
            }
        }

        private void NormalizeRequest(GeneratorRequest request)
        {
            request.ObjectName = Capitalize(request.ObjectName);
            request.EntryType = Capitalize(request.EntryType); 
            foreach (var prop in request.Properties)
            {
                prop.Name = Capitalize(prop.Name);
            }
        }

        private string Capitalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            s = s.Trim();
            return char.ToUpper(s[0]) + (s.Length > 1 ? s.Substring(1) : "");
        }
    }
}