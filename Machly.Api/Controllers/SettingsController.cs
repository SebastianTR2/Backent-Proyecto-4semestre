using Machly.Api.Models;
using Machly.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("settings")]
    public class SettingsController : ControllerBase
    {
        private readonly IMongoCollection<GlobalSettings> _settings;

        public SettingsController(MongoDbContext context)
        {
            _settings = context.GetCollection<GlobalSettings>("settings");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var settings = await _settings.Find(_ => true).FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new GlobalSettings 
                { 
                    AllowedCategories = new List<string> { "Tractor", "Excavadora", "Cosechadora", "Sembradora" },
                    CommissionRate = 0.10m,
                    TermsAndConditions = "Términos y condiciones por defecto...",
                    SupportEmail = "support@machly.com"
                };
                await _settings.InsertOneAsync(settings);
            }
            return Ok(settings);
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update([FromBody] GlobalSettings settings)
        {
            var existing = await _settings.Find(_ => true).FirstOrDefaultAsync();
            if (existing == null)
            {
                settings.Id = ObjectId.GenerateNewId().ToString();
                await _settings.InsertOneAsync(settings);
            }
            else
            {
                settings.Id = existing.Id;
                await _settings.ReplaceOneAsync(s => s.Id == existing.Id, settings);
            }
            return Ok(settings);
        }
    }
}
