using CassandraDemo.Server.Services;
using CassandraDemo.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CassandraDemo.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IDbService _dbService;
        
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
            IDbService dbService)
        {
            _logger = logger;
            _dbService = dbService;
        }

        [HttpGet ("GetForcast")]
        public IEnumerable<WeatherForecast> Get()
        {
            var forcasts = _dbService.GetForcast();
            return forcasts;
        }
        [HttpGet("AddEntry")]
        public async Task AddEntryAsync()
        {
            var forcasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            });
            await _dbService.AddValuesAsync(forcasts);
        }
        [HttpDelete("DeleteForcasts")]
        public async Task DeleteStuff()
        {
            try
            {
                await _dbService.DeleteForcastsAsync();
            }
            catch(Exception ex)
            {

            }
        }
    }
}
