﻿using Iwentys.Features.AccountManagement.Domain;
using Iwentys.Features.Guilds.Entities;
using Iwentys.Features.Guilds.Enums;
using Iwentys.Features.Guilds.Models;

namespace Iwentys.Tests.TestCaseContexts
{
    public partial class TestCaseContext
    {
        public TestCaseContext WithGuild(AuthorizedUser user, out ExtendedGuildProfileWithMemberDataDto guildProfile)
        {
            var guildCreateRequest = new GuildCreateRequestDto(null, null, null, GuildHiringPolicy.Close);

            GuildProfileShortInfoDto guild = GuildService.Create(user, guildCreateRequest).Result;
            guildProfile = GuildService.Get(guild.Id, user.Id).Result;
            return this;
        }

        public TestCaseContext WithGuildMember(GuildProfileDto guild, AuthorizedUser guildEditor, out AuthorizedUser user)
        {
            user = AccountManagementTestCaseContext.WithUser();
            GuildMemberService.RequestGuild(user, guild.Id).Wait();
            GuildMemberService.AcceptRequest(guildEditor, guild.Id, user.Id).Wait();
            return this;
        }

        public TestCaseContext WithGuildMentor(GuildProfileDto guild, out AuthorizedUser user)
        {
            //TODO: make method for promoting to guild editor/mentor
            user = AccountManagementTestCaseContext.WithUser();
            _context.GuildMembers.Add(new GuildMember(guild.Id, user.Id, GuildMemberType.Mentor));
            _context.SaveChanges();
            return this;
        }

        public TestCaseContext WithGuildRequest(GuildProfileDto guild, out AuthorizedUser user)
        {
            user = AccountManagementTestCaseContext.WithUser();
            GuildMemberService.RequestGuild(user, guild.Id).Wait();
            return this;
        }

        public TestCaseContext WithGuildBlocked(GuildProfileDto guild, AuthorizedUser guildEditor, out AuthorizedUser user)
        {
            user = AccountManagementTestCaseContext.WithUser();
            GuildMemberService.RequestGuild(user, guild.Id).Wait();
            GuildMemberService.BlockGuildMember(guildEditor, guild.Id, user.Id).Wait();
            return this;
        }
    }
}