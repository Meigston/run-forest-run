namespace RunForestRun.RabbitMq.Publishers.Api.Controllers
{
    using System;
    using System.Linq;

    using Brokers.Common.Interfaces;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using RunForestRun.RabbitMq.Events.Example;

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IEventPublisher eventPublisher;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IEventPublisher eventPublisher)
        {
            this._logger = logger;
            this.eventPublisher = eventPublisher;
        }

        [HttpPost("TestMessages")]
        public IActionResult Post()
        {
            var rng = new Random();
            Enumerable.Range(1, 500).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
           .ToList().ForEach(a =>
           {
               this.eventPublisher.Send(new WeatherForecastEvent()
               {
                   Date = a.Date,
                   TemperatureC = a.TemperatureC,
                   TemperatureF = a.TemperatureF,
                   Summary = a.Summary
               });
           });

            return this.Ok();
        }
    }
}
