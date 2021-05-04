using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScarletMVC.Models;
using ScarletMVC.Services;

namespace ScarletMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITokenService tokenService;

        public HomeController(
            ILogger<HomeController> logger,
            ITokenService tokenService)
        {
            _logger = logger;
            this.tokenService = tokenService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Weather()
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            var data = new List<WeatherData>();

            var httpClient = new HttpClient();

            var tokenResponse = await tokenService.GetToken("weatherapi.read");

            httpClient.SetBearerToken(tokenResponse.AccessToken);

            var response = await httpClient.GetAsync("https://localhost:5011/weatherforecast");
            
            if (response.IsSuccessStatusCode)
            {
                var forecast = await response.Content.ReadAsStringAsync();
                data = JsonSerializer.Deserialize<List<WeatherData>>(forecast);
                return View(data);
            }
            else
            {
                throw new Exception("Unable to get Weather Forecast data");
            }

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
