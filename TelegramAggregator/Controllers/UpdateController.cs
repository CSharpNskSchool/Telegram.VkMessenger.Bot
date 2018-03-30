﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using TelegramAggregator.Services;

namespace TelegramAggregator.Controllers
{
    [Route("api/[controller]")]
    public class UpdateController : Controller
    {
        private readonly IUpdateService _updateService;

        public UpdateController(IUpdateService updateService)
        {
            _updateService = updateService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            await _updateService.Update(update);
            return Ok();
        }
    }
}