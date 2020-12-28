﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iwentys.Common.Databases;
using Iwentys.Features.GithubIntegration.Services;
using Iwentys.Features.Guilds.Domain;
using Iwentys.Features.Guilds.Entities;
using Iwentys.Features.Guilds.Models;
using Iwentys.Features.Guilds.Tournaments.Entities;
using Iwentys.Features.Guilds.Tournaments.Models;
using Iwentys.Features.Students.Entities;
using Microsoft.EntityFrameworkCore;

namespace Iwentys.Features.Guilds.Tournaments.Domain
{
    public class CodeMarathonTournament : ITournamentDomain
    {
        private readonly TournamentEntity _tournament;
        private readonly GithubIntegrationService _githubIntegrationService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IGenericRepository<TournamentTeamMemberEntity> _tournamentTeamMemberRepository;


        public CodeMarathonTournament(TournamentEntity tournament, GithubIntegrationService githubIntegrationService, IUnitOfWork unitOfWork)
        {
            _tournament = tournament;
            _githubIntegrationService = githubIntegrationService;
            _unitOfWork = unitOfWork;

            _tournamentTeamMemberRepository = _unitOfWork.GetRepository<TournamentTeamMemberEntity>();
        }

        public TournamentLeaderboardDto GetLeaderboard()
        {
            List<GuildEntity> guilds = _unitOfWork
                .GetRepository<GuildEntity>()
                .Get()
                .ToList();

            Dictionary<GuildProfileShortInfoDto, int> result = guilds
                .ToDictionary(g => new GuildProfileShortInfoDto(g), CountGuildRating);

            return new TournamentLeaderboardDto
            {
                Tournament = _tournament,
                Result = result
            };
        }

        private int CountGuildRating(GuildEntity guild)
        {
            var domain = new GuildDomain(guild, _githubIntegrationService, _unitOfWork.GetRepository<StudentEntity>(), _unitOfWork.GetRepository<GuildMemberEntity>());
            //TODO: remove result
            List<GuildMemberImpactDto> users = domain.GetMemberImpacts().Result;
            
            return users
                .Select(userData => userData.Contribution.GetActivityForPeriod(_tournament.StartTime, _tournament.EndTime))
                .Sum();
        }
        
        public async Task UpdateResult()
        {
            //TODO: ensure tournament do not end
            List<TournamentTeamMemberEntity> members = await _unitOfWork.GetRepository<TournamentParticipantTeamEntity>()
                .Get()
                .Where(team => team.TournamentId == _tournament.Id)
                .SelectMany(team => team.Members)
                .Include(m => m.Member)
                .ToListAsync();

            List<int> list = new List<int>();
            foreach (TournamentTeamMemberEntity member in members)
            {
                var contributionFullInfo = await _githubIntegrationService.FindUserContributionOrEmpty(member.Member);
                member.Points = contributionFullInfo.GetActivityForPeriod(_tournament.StartTime, _tournament.EndTime);
            }

            _tournamentTeamMemberRepository.Update(members);
            await _unitOfWork.CommitAsync();
        }
    }
}