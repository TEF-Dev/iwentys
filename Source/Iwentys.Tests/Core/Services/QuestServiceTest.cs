﻿using System.Collections.Generic;
using System.Linq;
using Iwentys.Features.StudentFeature;
using Iwentys.Models.Entities;
using Iwentys.Models.Transferable.Gamification;
using Iwentys.Tests.Tools;
using NUnit.Framework;

namespace Iwentys.Tests.Core.Services
{
    [TestFixture]
    public class QuestServiceTest
    {
        [Test]
        public void CreateGuild_ShouldReturnCreatorAsMember()
        {
            TestCaseContext test = TestCaseContext
                .Case()
                .WithNewStudent(out AuthorizedUser user);

            StudentEntity student = user.GetProfile(test.StudentRepository).Result;
            student.BarsPoints = 100;
            test.StudentRepository.UpdateAsync(student);

            test.WithQuest(user, 50, out QuestInfoResponse quest);

            List<QuestInfoResponse> quests = test.QuestService.GetActiveAsync().Result;

            Assert.IsTrue(quests.Any(q => q.Id == quest.Id));
        }
    }
}