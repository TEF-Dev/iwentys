﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iwentys.Common.Databases;
using Iwentys.Domain;
using Iwentys.Domain.Guilds;
using Iwentys.Domain.Models;
using Iwentys.Domain.Services;
using Iwentys.Features.Guilds.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Iwentys.Endpoint.Controllers.Guilds
{
    public class CreateTribute
    {
        public class Query : IRequest<Response>
        {
            public Query(AuthorizedUser user, CreateProjectRequestDto arguments)
            {
                User = user;
                Arguments = arguments;
            }

            public AuthorizedUser User { get; set; }
            public CreateProjectRequestDto Arguments { get; set; }
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
            private readonly GithubIntegrationService _githubIntegrationService;
            private readonly IGenericRepository<GuildMember> _guildMemberRepository;
            private readonly IGenericRepository<Guild> _guildRepositoryNew;
            private readonly IGenericRepository<Tribute> _guildTributeRepository;
            private readonly IGenericRepository<GithubProject> _studentProjectRepository;

            private readonly IGenericRepository<IwentysUser> _studentRepository;
            private readonly IUnitOfWork _unitOfWork;

            public Handler(IUnitOfWork unitOfWork, GithubIntegrationService githubIntegrationService)
            {
                _githubIntegrationService = githubIntegrationService;
                _unitOfWork = unitOfWork;
                _studentRepository = _unitOfWork.GetRepository<IwentysUser>();
                _guildRepositoryNew = _unitOfWork.GetRepository<Guild>();
                _guildMemberRepository = _unitOfWork.GetRepository<GuildMember>();
                _studentProjectRepository = _unitOfWork.GetRepository<GithubProject>();
                _guildTributeRepository = _unitOfWork.GetRepository<Tribute>();
            }

            protected override Response Handle(Query request)
            {
                IwentysUser student = _studentRepository.FindByIdAsync(request.User.Id).Result;
                GithubRepositoryInfoDto githubProject = _githubIntegrationService.Repository.GetRepository(request.Arguments.Owner, request.Arguments.RepositoryName).Result;
                GithubProject project = GetOrCreate(githubProject, student).Result;
                Guild guild = _guildMemberRepository.ReadForStudent(student.Id);
                List<Tribute> allTributes = _guildTributeRepository.Get().ToListAsync().Result;

                var tribute = Tribute.Create(guild, student, project, allTributes);

                _guildTributeRepository.InsertAsync(tribute).Wait();
                _unitOfWork.CommitAsync().Wait();

                TributeInfoResponse tributeInfoResponse = _guildTributeRepository
                    .Get()
                    .Where(t => t.ProjectId == tribute.ProjectId)
                    .Select(TributeInfoResponse.FromEntity)
                    .SingleAsync().Result;

                return new Response(tributeInfoResponse);
            }

            public async Task<GithubProject> GetOrCreate(GithubRepositoryInfoDto project, IwentysUser creator)
            {
                GithubProject githubProject = await _studentProjectRepository.FindByIdAsync(project.Id);
                if (githubProject is not null)
                    return githubProject;

                GithubUser githubUser = await _githubIntegrationService.User.Get(creator.Id);
                //TODO: need to get this from GithubService
                var newProject = new GithubProject(githubUser, project);

                await _studentProjectRepository.InsertAsync(newProject);
                await _unitOfWork.CommitAsync();
                return newProject;
            }
        }
    }
}