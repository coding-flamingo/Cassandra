using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CassandraDemo.Shared
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public int Year { get; set; } = DateTime.Now.Year;
        public int TemperatureC { get; set; }
        public string Summary { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
