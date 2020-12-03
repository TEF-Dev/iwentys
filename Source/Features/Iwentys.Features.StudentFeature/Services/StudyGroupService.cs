﻿using System.Threading.Tasks;
using Iwentys.Features.StudentFeature.Entities;
using Iwentys.Features.StudentFeature.Repositories;
using Iwentys.Features.StudentFeature.ViewModels;

namespace Iwentys.Features.StudentFeature.Services
{
    public class StudyGroupService
    {
        private readonly IStudyGroupRepository _studyGroupRepository;

        public StudyGroupService(IStudyGroupRepository studyGroupRepository)
        {
            _studyGroupRepository = studyGroupRepository;
        }

        public async Task<GroupProfileResponse> Get(string groupName)
        {
            var fixedGroupName = new GroupName(groupName);
            StudyGroupEntity group = await _studyGroupRepository.ReadByNamePattern(fixedGroupName);
            return GroupProfileResponse.Create(group);
        }
    }
}