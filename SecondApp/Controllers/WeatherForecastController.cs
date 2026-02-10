using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SecondApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        /// <summary>Token gerekmez (test için).</summary>
        [HttpGet("public")]
        public IActionResult GetPublic() => Ok(new { Message = "SecondApp - Herkese açık endpoint.", Time = DateTime.UtcNow });

        /// <summary>Sadece Admin rolü erişebilir.</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        /// <summary>Admin veya User rolü erişebilir (User için ayrı metod).</summary>
        [Authorize(Roles = "Admin,User")]
        [HttpGet("user")]
        public IActionResult GetUser()
        {
            var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value;
            return Ok(new { Message = "SecondApp - User/Admin endpoint.", User = username, Time = DateTime.UtcNow });
        }
    }
}
