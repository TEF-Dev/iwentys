﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iwentys.Domain.AccountManagement;
using Iwentys.Domain.Guilds;
using Iwentys.Domain.Guilds.Enums;
using Iwentys.Infrastructure.DataAccess;
using MediatR;

namespace Iwentys.Infrastructure.Application.Controllers.GuildMemberships
{
    public class GetGuildRequests
    {
        public class Query : IRequest<Response>
        {
            public AuthorizedUser User { get; set; }
            public int GuildId { get; set; }

            public Query(AuthorizedUser user, int guildId)
            {
                User = user;
                GuildId = guildId;
            }
        }

        public class Response
        {
            public List<GuildMember> GuildMembers { get; set; }

            public Response(List<GuildMember> guildMembers)
            {
                GuildMembers = guildMembers;
            }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly IwentysDbContext _context;

            public Handler(IwentysDbContext context)
            {
                _context = context;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                IwentysUser student = await _context.IwentysUsers.GetById(request.User.Id);
                Guild guild = await _context.Guilds.GetById(request.GuildId);

                student.EnsureIsGuildMentor(guild);

                List<GuildMember> result = guild.Members
                    .Where(m => m.MemberType == GuildMemberType.Requested)
                    .ToList();

                return new Response(result);
            }
        }
    }
}