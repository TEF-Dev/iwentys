﻿using System;
using System.Linq;
using Iwentys.Core.DomainModel;
using Iwentys.Core.DomainModel.Guilds;
using Iwentys.Core.GithubIntegration;
using Iwentys.Core.Services.Abstractions;
using Iwentys.Database.Context;
using Iwentys.Database.Repositories;
using Iwentys.Database.Repositories.Abstractions;
using Iwentys.Models.Entities;
using Iwentys.Models.Entities.Github;
using Iwentys.Models.Entities.Guilds;
using Iwentys.Models.Exceptions;
using Iwentys.Models.Tools;
using Iwentys.Models.Transferable.Guilds;
using Iwentys.Models.Types.Guilds;

namespace Iwentys.Core.Services.Implementations
{
    public class GuildService : IGuildService
    {
        private readonly IGithubUserDataService _githubUserDataService;
        private readonly IGithubApiAccessor _githubApiAccessor;

        private readonly IGuildRepository _guildRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ITributeRepository _tributeRepository;
        private readonly DatabaseAccessor _databaseAccessor;

        public GuildService(IGuildRepository guildRepository,
            IStudentRepository studentRepository,
            ITributeRepository tributeRepository,
            DatabaseAccessor databaseAccessor, IGithubUserDataService githubUserDataService, IGithubApiAccessor githubApiAccessor)
        {
            _guildRepository = guildRepository;
            _studentRepository = studentRepository;
            _tributeRepository = tributeRepository;
            _databaseAccessor = databaseAccessor;
            _githubUserDataService = githubUserDataService;
            _githubApiAccessor = githubApiAccessor;
        }

        public GuildProfileShortInfoDto Create(AuthorizedUser creator, GuildCreateArgumentDto arguments)
        {
            Student creatorUser = _studentRepository.Get(creator.Id);

            GuildEntity userGuild = _guildRepository.ReadForStudent(creatorUser.Id);
            if (userGuild != null)
                throw new InnerLogicException("Student already in guild");

            return _guildRepository.Create(creatorUser, arguments)
                .To(g => new GuildDomain(g, _databaseAccessor, _githubUserDataService, _githubApiAccessor))
                .ToGuildProfileShortInfoDto();
        }

        public GuildProfileShortInfoDto Update(AuthorizedUser user, GuildUpdateArgumentDto arguments)
        {
            Student student = user.GetProfile(_studentRepository);
            GuildEntity info = _guildRepository.Get(arguments.Id);
            student.EnsureIsGuildEditor(info);

            info.Bio = arguments.Bio ?? info.Bio;
            info.LogoUrl = arguments.LogoUrl ?? info.LogoUrl;
            info.TestTaskLink = arguments.TestTaskLink ?? info.TestTaskLink;
            info.HiringPolicy = arguments.HiringPolicy ?? info.HiringPolicy;

            if (arguments.HiringPolicy == GuildHiringPolicy.Open)
                foreach (GuildMemberEntity guildMember in info.Members.Where(guildMember => guildMember.MemberType == GuildMemberType.Requested))
                    guildMember.MemberType = GuildMemberType.Member;

            return _guildRepository.Update(info)
                .To(g => new GuildDomain(g, _databaseAccessor, _githubUserDataService, _githubApiAccessor))
                .ToGuildProfileShortInfoDto();
        }

        public GuildProfileShortInfoDto ApproveGuildCreating(AuthorizedUser user, int guildId)
        {
            _studentRepository
                .Get(user.Id)
                .EnsureIsAdmin();

            GuildEntity guild = _guildRepository.Get(guildId);
            if (guild.GuildType == GuildType.Created)
                throw new InnerLogicException("Guild already approved");

            guild.GuildType = GuildType.Created;
            return _guildRepository.Update(guild)
                .To(g => new GuildDomain(g, _databaseAccessor, _githubUserDataService, _githubApiAccessor))
                .ToGuildProfileShortInfoDto();
        }

        public GuildProfileDto[] Get()
        {
            return _guildRepository.Read().AsEnumerable().Select(g =>
                new GuildDomain(g, _databaseAccessor, _githubUserDataService, _githubApiAccessor)
                    .ToGuildProfileDto()).ToArray();
        }

        public GuildProfilePreviewDto[] GetOverview(Int32 skippedCount, Int32 takenCount)
        {
            return _guildRepository.Read()
                .ToList()
                .Select(g => new GuildDomain(g, _databaseAccessor, _githubUserDataService, _githubApiAccessor).ToGuildProfilePreviewDto())
                .OrderByDescending(g => g.Rating)
                .Skip(skippedCount)
                .Take(takenCount)
                .ToArray();
        }

        public GuildProfileDto Get(int id, int? userId)
        {
            return _guildRepository.Get(id)
                .To(g => new GuildDomain(g, _databaseAccessor, _githubUserDataService, _githubApiAccessor))
                .ToGuildProfileDto(userId);
        }

        public GuildProfileDto GetStudentGuild(int userId)
        {
            return _guildRepository.ReadForStudent(userId).To(g =>
                    new GuildDomain(g, _databaseAccessor, _githubUserDataService, _githubApiAccessor))
                .ToGuildProfileDto(userId);
        }

        public GuildProfileDto EnterGuild(AuthorizedUser user, Int32 guildId)
        {
            GuildDomain guild = _guildRepository.Get(guildId).To(g => new GuildDomain(g, _databaseAccessor, _githubUserDataService, _githubApiAccessor));

            if (guild.GetUserMembershipState(user.Id) != UserMembershipState.CanEnter)
                throw new InnerLogicException($"Student unable to enter this guild! UserId: {user.Id} GuildId: {guildId}");

            _guildRepository.AddMember(guild.Profile, user.GetProfile(_studentRepository), GuildMemberType.Member);

            return Get(guildId, user.Id);
        }

        public GuildProfileDto RequestGuild(AuthorizedUser user, Int32 guildId)
        {
            GuildDomain guild = _guildRepository.Get(guildId).To(g =>
                new GuildDomain(g, _databaseAccessor, _githubUserDataService, _githubApiAccessor));

            if (guild.GetUserMembershipState(user.Id) != UserMembershipState.CanRequest)
                throw new InnerLogicException($"Student unable to send request to this guild! UserId: {user.Id} GuildId: {guildId}");

            _guildRepository.AddMember(guild.Profile, user.GetProfile(_studentRepository), GuildMemberType.Requested);
            return Get(guildId, user.Id);
        }

        public GuildProfileDto LeaveGuild(AuthorizedUser user, int guildId)
        {
            GuildEntity studentGuild = _guildRepository.ReadForStudent(user.Id);
            if (studentGuild == null || studentGuild.Id != guildId)
                throw InnerLogicException.Guild.IsNotGuildMember(user.Id, guildId);

            TributeEntity userTribute = _tributeRepository.ReadStudentActiveTribute(studentGuild.Id, user.Id);
            if (userTribute != null)
                _tributeRepository.Delete(userTribute.ProjectId);

            _guildRepository.RemoveMember(guildId, user.Id);

            return Get(guildId, user.Id);
        }

        public GuildMemberEntity[] GetGuildRequests(AuthorizedUser user, Int32 guildId)
        {
            Student student = user.GetProfile(_studentRepository);
            GuildEntity guild = _guildRepository.Get(guildId);
            student.EnsureIsGuildEditor(guild);

            return guild.Members
                .Where(m => m.MemberType == GuildMemberType.Requested)
                .ToArray();
        }

        public GuildMemberEntity[] GetGuildBlocked(AuthorizedUser user, Int32 guildId)
        {
            Student student = user.GetProfile(_studentRepository);
            GuildEntity guild = _guildRepository.Get(guildId);
            student.EnsureIsGuildEditor(guild);

            return guild.Members
                .Where(m => m.MemberType == GuildMemberType.Blocked)
                .ToArray();
        }

        public void BlockGuildMember(AuthorizedUser user, Int32 guildId, Int32 memberId)
        {
            var guildDomain = new GuildDomain(_guildRepository.Get(guildId), _databaseAccessor, _githubUserDataService, _githubApiAccessor);
            GuildMemberEntity memberToKick = guildDomain.EnsureMemberCanRestrictPermissionForOther(user, memberId);

            memberToKick.Member.GuildLeftTime = DateTime.UtcNow.ToUniversalTime();
            memberToKick.MemberType = GuildMemberType.Blocked;
            _guildRepository.UpdateMember(memberToKick);
        }

        public void UnblockStudent(AuthorizedUser user, Int32 guildId, Int32 studentId)
        {
            Student student = user.GetProfile(_studentRepository);
            GuildEntity guild = _guildRepository.Get(guildId);
            student.EnsureIsGuildEditor(guild);

            GuildMemberEntity member = guild.Members.Find(m => m.MemberId == studentId);

            if (member is null || member.MemberType != GuildMemberType.Blocked)
                throw new InnerLogicException($"Student is not blocked in guild! StudentId: {studentId} GuildId: {guildId}");

            _guildRepository.RemoveMember(guildId, studentId);
        }

        public void KickGuildMember(AuthorizedUser user, Int32 guildId, Int32 memberId)
        {
            var guildDomain = new GuildDomain(_guildRepository.Get(guildId), _databaseAccessor, _githubUserDataService, _githubApiAccessor);
            GuildMemberEntity memberToKick = guildDomain.EnsureMemberCanRestrictPermissionForOther(user, memberId);

            memberToKick.Member.GuildLeftTime = DateTime.UtcNow.ToUniversalTime();
            _guildRepository.RemoveMember(guildId, memberId);
        }

        public void AcceptRequest(AuthorizedUser user, Int32 guildId, Int32 studentId)
        {
            Student student = user.GetProfile(_studentRepository);
            GuildEntity guild = _guildRepository.Get(guildId);
            student.EnsureIsGuildEditor(guild);

            GuildMemberEntity member = guild.Members.Find(m => m.MemberId == studentId);

            if (member is null || member.MemberType != GuildMemberType.Requested)
                throw InnerLogicException.Guild.RequestWasNotFound(studentId, guildId);

            member.MemberType = GuildMemberType.Member;

            _guildRepository.UpdateMember(member);
        }

        public void RejectRequest(AuthorizedUser user, Int32 guildId, Int32 studentId)
        {
            Student student = user.GetProfile(_studentRepository);
            GuildEntity guild = _guildRepository.Get(guildId);
            student.EnsureIsGuildEditor(guild);

            GuildMemberEntity member = guild.Members.Find(m => m.MemberId == studentId);

            if (member is null || member.MemberType != GuildMemberType.Requested)
                throw InnerLogicException.Guild.RequestWasNotFound(studentId, guildId);

            _guildRepository.RemoveMember(guildId, studentId);
        }

        public GithubRepository AddPinnedRepository(AuthorizedUser user, int guildId, string owner, string projectName)
        {
            GuildEntity guild = _guildRepository.Get(guildId);
            user.GetProfile(_studentRepository).EnsureIsGuildEditor(guild);

            GithubRepository repository = _githubApiAccessor.GetRepository(owner, projectName);
            _guildRepository.PinProject(guildId, owner, projectName);
            return repository;
        }

        public void UnpinProject(AuthorizedUser user, int pinnedProjectId)
        {
            GuildPinnedProjectEntity guildPinnedProject = _databaseAccessor.Context.GuildPinnedProjects.Find(pinnedProjectId) ?? throw EntityNotFoundException.PinnedRepoWasNotFound(pinnedProjectId);
            GuildEntity guild = _guildRepository.ReadById(guildPinnedProject.GuildId);
            user.GetProfile(_studentRepository).EnsureIsGuildEditor(guild);

            user.GetProfile(_studentRepository).EnsureIsGuildEditor(guild);

            _guildRepository.UnpinProject(pinnedProjectId);
        }

        public GuildMemberLeaderBoard GetGuildMemberLeaderBoard(int guildId)
        {
            return _guildRepository.Get(guildId)
                .To(g => new GuildDomain(g, _databaseAccessor, _githubUserDataService, _githubApiAccessor))
                .GetMemberDashboard();
        }

        public void AcceptTestTask(AuthorizedUser user, int guildId)
        {
            GuildEntity studentGuild = _guildRepository.ReadForStudent(user.Id);
            if (studentGuild == null || studentGuild.Id != guildId)
                throw InnerLogicException.Guild.IsNotGuildMember(user.Id, guildId);

            _databaseAccessor.GuildTestTaskSolvingInfo.Create(studentGuild, user.GetProfile(_studentRepository));
        }
    }
}