﻿using System.Linq;
using Iwentys.Common.Exceptions;
using Iwentys.Domain.AccountManagement;
using Iwentys.Domain.Guilds.Enums;

namespace Iwentys.Domain.Guilds
{
    public class GuildCreator
    {
        public GuildCreator(IwentysUser student, Guild guild, GuildMemberType memberType)
        {
            if (memberType != GuildMemberType.Creator)
                throw InnerLogicException.NotEnoughPermissionFor(student.Id);

            Student = student;
            Guild = guild;
            MemberType = memberType;
        }

        public IwentysUser Student { get; }
        public Guild Guild { get; }
        public GuildMemberType MemberType { get; }
    }

    public static class GuildCreatorExtensions
    {
        public static GuildCreator EnsureIsCreator(this IwentysUser student, Guild guild)
        {
            GuildMember membership = guild.Members.First(m => m.MemberId == student.Id);

            return new GuildCreator(student, guild, membership.MemberType);
        }
    }
}