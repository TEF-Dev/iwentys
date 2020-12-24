﻿using System.Collections.Generic;
using System.Linq;
using Iwentys.Common.Databases;
using Iwentys.Common.Exceptions;
using Iwentys.Features.Gamification.Models;
using Iwentys.Features.GithubIntegration.Services;
using Iwentys.Features.Students.Entities;
using Iwentys.Features.Study.Entities;
using Iwentys.Features.Study.Models;
using Iwentys.Features.Study.Repositories;

namespace Iwentys.Features.Gamification.Services
{
    public class StudyLeaderboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IGenericRepository<StudyGroupEntity> _studyGroupRepository;

        private readonly GithubIntegrationService _githubIntegrationService;
        private readonly ISubjectActivityRepository _subjectActivityRepository;
        

        public StudyLeaderboardService(GithubIntegrationService githubIntegrationService, ISubjectActivityRepository subjectActivityRepository, IUnitOfWork unitOfWork)
        {
            _githubIntegrationService = githubIntegrationService;
            _subjectActivityRepository = subjectActivityRepository;
            
            _unitOfWork = unitOfWork;
            _unitOfWork.GetRepository<StudentEntity>();
            _studyGroupRepository = _unitOfWork.GetRepository<StudyGroupEntity>();
        }

        public List<StudyLeaderboardRowDto> GetStudentsRatings(StudySearchParametersDto searchParametersDto)
        {
            if (searchParametersDto.CourseId is null && searchParametersDto.GroupId is null ||
                searchParametersDto.CourseId is not null && searchParametersDto.GroupId is not null)
            {
                throw new IwentysException("One of StudySearchParametersDto fields: CourseId or GroupId should be null");
            }

            List<SubjectActivityEntity> result = _subjectActivityRepository.GetStudentActivities(searchParametersDto).ToList();

            return result
                .GroupBy(r => r.StudentId)
                .Select(g => new StudyLeaderboardRowDto(g.ToList()))
                .OrderByDescending(a => a.Activity)
                .Skip(searchParametersDto.Skip)
                .Take(searchParametersDto.Take)
                .ToList();
        }

        public List<StudyLeaderboardRowDto> GetCodingRating(int? courseId, int skip, int take)
        {
            return _studyGroupRepository.GetAsync()
                .WhereIf(courseId, q => q.StudyCourseId == courseId)
                .SelectMany(g => g.Students)
                .AsEnumerable()
                .Select(s => new StudyLeaderboardRowDto(s, _githubIntegrationService.GetGithubUser(s.GithubUsername).Result?.ContributionFullInfo.Total ?? 0))
                .OrderBy(a => a.Activity)
                .Skip(skip)
                .Take(take)
                .ToList();
        }
    }
}
