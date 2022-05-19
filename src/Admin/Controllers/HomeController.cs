﻿using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Bit.Admin.Models;
using Bit.Core.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bit.Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly GlobalSettings _globalSettings;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<HomeController> _logger;

        public HomeController(GlobalSettings globalSettings, ILogger<HomeController> logger)
        {
            _globalSettings = globalSettings;
            _logger = logger;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View(new HomeModel
            {
                GlobalSettings = _globalSettings,
                CurrentVersion = Core.Utilities.CoreHelpers.GetVersion()
            });
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        public async Task<IActionResult> GetLatestVersion(string repository, CancellationToken cancellationToken)
        {
            var requestUri = $"https://raw.githubusercontent.com/bitwarden/self-host/master/version.json";
            try
            {
                var response = await _httpClient.GetAsync(requestUri, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    using var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
                    var root = jsonDocument.RootElement;
                    var versionsNode = root.GetProperty("versions");
                    if (repository == "web")
                    {
                        var name = versionsNode.GetProperty("webVersion").GetString();
                        if (!string.IsNullOrWhiteSpace(name) && name.Length > 0 && char.IsNumber(name[0]))
                        {
                            return new JsonResult(name);
                        }
                    }
                    if (repository == "api")
                    {
                        var name = versionsNode.GetProperty("coreVersion").GetString();
                        if (!string.IsNullOrWhiteSpace(name) && name.Length > 0 && char.IsNumber(name[0]))
                        {
                            return new JsonResult(name);
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, $"Error encountered while sending GET request to {requestUri}");
                return new JsonResult("Unable to fetch latest version") { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return new JsonResult("-");
        }

        public async Task<IActionResult> GetInstalledWebVersion(CancellationToken cancellationToken)
        {
            var requestUri = $"{_globalSettings.BaseServiceUri.InternalVault}/version.json";
            try
            {
                var response = await _httpClient.GetAsync(requestUri, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    using var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
                    var root = jsonDocument.RootElement;
                    return new JsonResult(root.GetProperty("version").GetString());
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, $"Error encountered while sending GET request to {requestUri}");
                return new JsonResult("Unable to fetch installed version") { StatusCode = StatusCodes.Status500InternalServerError };
            }

            return new JsonResult("-");
        }
    }
}
