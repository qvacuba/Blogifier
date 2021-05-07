using System;
using Blogifier.Core.Providers;
using Blogifier.Shared.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blogifier.Controllers
{
    [Route("api/config")]
	[ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationProvider configurationProvider;

        public ConfigurationController(IConfigurationProvider provider) {
            configurationProvider = provider;
        }

        [HttpGet("{key}")]
        public async Task<IActionResult> GetByKey(string key) {
            try {
                var resp = await configurationProvider.GetConfiguration(key);
                return Ok(resp);
            } catch(Exception) {
                return BadRequest();
            }
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetConfigurations() {
            try {
                var resp = await configurationProvider.GetAllConfigurations();
                return Ok(resp);
            } catch(Exception) {
                return BadRequest();
            }
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Configuration request) {  
            try {
                var resp = await configurationProvider.Add(request.Name, request.Active);
                return Ok(resp);
            } catch(Exception) {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpPut("{key}")]
        public async Task<IActionResult> Update([FromBody] Configuration request, string key) {    
            try {
                var resp = await configurationProvider.Update(key, request.Active);
                return resp ? Ok(resp) : NotFound(key);
            } catch(Exception) {
                return BadRequest();
            }
        }
    }
}
