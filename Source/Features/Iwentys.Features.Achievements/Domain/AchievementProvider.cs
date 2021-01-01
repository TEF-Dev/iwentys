﻿using System.Linq;
using System.Threading.Tasks;
using Iwentys.Common.Databases;
using Iwentys.Features.Achievements.Entities;

namespace Iwentys.Features.Achievements.Domain
{
    public class AchievementProvider
    {
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly IGenericRepository<StudentAchievement> _studentAchievementRepository;
        private readonly IGenericRepository<GuildAchievement> _guildAchievementRepository;

        public AchievementProvider(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _studentAchievementRepository = _unitOfWork.GetRepository<StudentAchievement>();
            _guildAchievementRepository = _unitOfWork.GetRepository<GuildAchievement>();
        }

        public async Task Achieve(Achievement achievement, int studentId)
        {
            if (_studentAchievementRepository.Get().Any(s => s.AchievementId == achievement.Id && s.StudentId == studentId))
                return;
            
            await _studentAchievementRepository.InsertAsync(StudentAchievement.Create(studentId, achievement.Id));
            await _unitOfWork.CommitAsync();
        }

        public async Task AchieveForGuild(Achievement achievement, int guildId)
        {
            if (_guildAchievementRepository.Get().Any(s => s.AchievementId == achievement.Id && s.GuildId == guildId))
                return;

            await _guildAchievementRepository.InsertAsync(GuildAchievement.Create(guildId, achievement.Id));
            await _unitOfWork.CommitAsync();
        }
    }
}