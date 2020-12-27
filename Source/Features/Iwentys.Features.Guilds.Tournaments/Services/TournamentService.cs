﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iwentys.Common.Databases;
using Iwentys.Common.Tools;
using Iwentys.Features.GithubIntegration.Services;
using Iwentys.Features.Guilds.Domain;
using Iwentys.Features.Guilds.Entities;
using Iwentys.Features.Guilds.Tournaments.Domain;
using Iwentys.Features.Guilds.Tournaments.Entities;
using Iwentys.Features.Guilds.Tournaments.Models;
using Iwentys.Features.Students.Domain;
using Iwentys.Features.Students.Entities;
using Microsoft.EntityFrameworkCore;

namespace Iwentys.Features.Guilds.Tournaments.Services
{
    public class TournamentService
    {
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly IGenericRepository<StudentEntity> _studentRepository;
        private readonly IGenericRepository<GuildEntity> _guildRepository;
        private readonly IGenericRepository<TournamentEntity> _tournamentRepository;
        private readonly IGenericRepository<TournamentParticipantTeamEntity> _tournamentTeamRepository;
        private readonly IGenericRepository<CodeMarathonTournamentEntity> _codeMarathonTournamentRepository;
        private readonly GithubIntegrationService _githubIntegrationService;

        public TournamentService(GithubIntegrationService githubIntegrationService, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            _studentRepository = _unitOfWork.GetRepository<StudentEntity>();
            _guildRepository = _unitOfWork.GetRepository<GuildEntity>();
            _tournamentRepository = _unitOfWork.GetRepository<TournamentEntity>();
            _tournamentTeamRepository = _unitOfWork.GetRepository<TournamentParticipantTeamEntity>();
            _codeMarathonTournamentRepository = _unitOfWork.GetRepository<CodeMarathonTournamentEntity>();
            _githubIntegrationService = githubIntegrationService;
        }

        public async Task<List<TournamentInfoResponse>> GetAsync()
        {
            return await _tournamentRepository
                .Get()
                .Select(TournamentInfoResponse.FromEntity)
                .ToListAsync();
        }

        public List<TournamentInfoResponse> GetActive()
        {
            return _tournamentRepository
                .Get()
                .Where(t => t.StartTime < DateTime.UtcNow && t.EndTime > DateTime.UtcNow)
                .Select(TournamentInfoResponse.FromEntity)
                .ToList();
        }

        public async Task<TournamentInfoResponse> GetAsync(int tournamentId)
        {
            return await _tournamentRepository
                .Get()
                .Select(TournamentInfoResponse.FromEntity)
                .SingleAsync(t => t.Id == tournamentId);
        }

        public async Task<TournamentLeaderboardDto> GetLeaderboard(int tournamentId)
        {
            TournamentEntity tournamentEntity = await _tournamentRepository.GetByIdAsync(tournamentId);
            return tournamentEntity
                .WrapToDomain(_githubIntegrationService, _unitOfWork)
                .GetLeaderboard();
        }

        public async Task<TournamentInfoResponse> CreateCodeMarathon(AuthorizedUser user, CreateCodeMarathonTournamentArguments arguments)
        {
            var systemAdminUser = (await _studentRepository.GetByIdAsync(user.Id)).EnsureIsAdmin();
            
            var codeMarathonTournamentEntity = CodeMarathonTournamentEntity.Create(systemAdminUser, arguments);

            await _tournamentRepository.InsertAsync(codeMarathonTournamentEntity.Tournament);
            await _codeMarathonTournamentRepository.InsertAsync(codeMarathonTournamentEntity);
            await _unitOfWork.CommitAsync();

            return await GetAsync(codeMarathonTournamentEntity.Id);
        }

        public async Task RegisterToTournament(AuthorizedUser user, int guildId, int tournamentId)
        {
            var studentEntity = await _studentRepository.GetByIdAsync(user.Id);
            var guildMentorUser = await studentEntity.EnsureIsMentor(_guildRepository, guildId);
            var tournamentEntity = await _tournamentRepository.GetByIdAsync(tournamentId);
            List<GuildMemberEntity> members = await _guildRepository
                .Get()
                .SelectMany(g => g.Members)
                .ToListAsync();
            
            var tournamentParticipantTeamEntity = tournamentEntity.RegisterTeam(guildMentorUser.Guild, members);

            await _tournamentTeamRepository.InsertAsync(tournamentParticipantTeamEntity);
            await _unitOfWork.CommitAsync();
        }
    }
}