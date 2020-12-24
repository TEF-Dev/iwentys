﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Iwentys.Features.Achievements.Models;
using Iwentys.Features.Achievements.Services;
using Microsoft.AspNetCore.Mvc;

namespace Iwentys.Endpoint.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AchievementController : ControllerBase
    {
        private readonly AchievementService _achievementService;

        public AchievementController(AchievementService achievementService)
        {
            _achievementService = achievementService;
        }

        [HttpGet("for-student")]
        public async Task<ActionResult<List<AchievementDto>>> GetForStudent(int studentId)
        {
            List<AchievementDto> achievementDtos = await _achievementService.GetForStudent(studentId);
            return Ok(achievementDtos);
        }
    }
}