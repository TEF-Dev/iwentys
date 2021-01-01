﻿using System.Linq;
using System.Threading.Tasks;
using Iwentys.Common.Databases;
using Iwentys.Common.Exceptions;
using Iwentys.Features.Guilds.Entities;
using Iwentys.Features.Guilds.Enums;
using Iwentys.Features.Students.Entities;

namespace Iwentys.Features.Guilds.Domain
{
    public class GuildMentor
    {
        public GuildMentor(Student student, Guild guild, GuildMemberType memberType)
        {
            if (!memberType.IsMentor())
                throw InnerLogicException.GuildExceptions.IsNotGuildMentor(student.Id);

            Student = student;
            Guild = guild;
            MemberType = memberType;
        }

        public Student Student { get; }
        public Guild Guild { get; }
        public GuildMemberType MemberType { get; }
    }

    public static class GuildMentorUserExtensions
    {
        public static async Task<GuildMentor> EnsureIsGuildMentor(this Student student, IGenericRepository<Guild> guildRepository, int guildId)
        {
            Guild guild = await guildRepository.GetByIdAsync(guildId);
            return EnsureIsGuildMentor(student, guild);
        }

        public static GuildMentor EnsureIsGuildMentor(this Student student, Guild guild)
        {
            GuildMember membership = guild.Members.FirstOrDefault(m => m.MemberId == student.Id);

            if (membership is null)
                throw InnerLogicException.GuildExceptions.IsNotGuildMember(student.Id, guild.Id);

            return new GuildMentor(student, guild, membership.MemberType);
        }
    }
}