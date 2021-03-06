﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iwentys.Infrastructure.Application.Controllers.Study.Dtos;
using Iwentys.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Iwentys.Infrastructure.Application.Controllers.Study
{
    public class GetStudyCourses
    {
        public class Query : IRequest<Response>
        {
        }

        public class Response
        {
            public Response(List<StudyCourseInfoDto> courses)
            {
                Courses = courses;
            }

            public List<StudyCourseInfoDto> Courses { get; set; }
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
                List<StudyCourseInfoDto> result = await _context
                    .StudyCourses
                    .Select(StudyCourseInfoDto.FromEntity)
                    .ToListAsync();

                return new Response(result);
            }
        }
    }
}