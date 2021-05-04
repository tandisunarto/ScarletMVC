using System;

namespace ScarletMVC.Models
{
    public class WeatherData
    {
        public DateTime date { get; set; }
        public int temperatureC { get; set; }
        public int temperatureF => 32 + (int)(temperatureC / 0.5556);
        public string summary { get; set; }
    }
}