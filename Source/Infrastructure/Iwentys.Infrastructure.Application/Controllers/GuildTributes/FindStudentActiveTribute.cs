﻿using System.Linq;
using Iwentys.Domain.AccountManagement;
using Iwentys.Domain.Guilds;
using Iwentys.Domain.Guilds.Models;
using Iwentys.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Iwentys.Infrastructure.Application.Controllers.GuildTributes
{
    public class FindStudentActiveTribute
    {
        public class Query : IRequest<Response>
        {
            public Query(AuthorizedUser user)
            {
                User = user;
            }

            public AuthorizedUser User { get; set; }
        }

        public class Response
        {
            public Response(TributeInfoResponse tribute)
            {
                Tribute = tribute;
            }

            public TributeInfoResponse Tribute { get; set; }
        }

        public class Handler : RequestHandler<Query, Response>
        {
            private readonly IwentysDbContext _context;

            public Handler(IwentysDbContext context)
            {
                _context = context;
            }

            protected override Response Handle(Query request)
            {
                IwentysUser student = _context.IwentysUsers.GetById(request.User.Id).Result;
                TributeInfoResponse tributeInfoResponse = _context
                    .Tributes
                    .Where(Tribute.IsActive)
                    .Where(Tribute.BelongTo(student))
                    .Select(TributeInfoResponse.FromEntity)
                    .SingleOrDefaultAsync().Result;

                return new Response(tributeInfoResponse);
            }
        }
    }
}