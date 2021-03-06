﻿using Iwentys.Domain.AccountManagement;
using Iwentys.Domain.Study;
using Iwentys.Domain.Study.Enums;
using Iwentys.Domain.SubjectAssignments;
using Iwentys.Domain.SubjectAssignments.Enums;
using Iwentys.Domain.SubjectAssignments.Models;
using Iwentys.Infrastructure.DataAccess.Seeding.FakerEntities;
using Iwentys.Infrastructure.DataAccess.Seeding.FakerEntities.Study;
using Iwentys.Tests.TestCaseContexts;
using NUnit.Framework;

namespace Iwentys.Tests.Modules.SubjectAssignments
{
    [TestFixture]
    public class SubjectAssignmentTest
    {
        [Test]
        public void CreateSubjectAssignment_Ok()
        {
            TestCaseContext testCase = TestCaseContext.Case();

            IwentysUser admin = testCase.AccountManagementTestCaseContext.WithIwentysUser(true);
            Subject subject = SubjectFaker.Instance.Generate();
            StudyGroup studyGroup = StudyGroupFaker.Instance.CreateGroup();
            SubjectAssignmentCreateArguments arguments = SubjectAssignmentFaker.Instance.CreateSubjectAssignmentCreateArguments(subject.Id);

            GroupSubject groupSubject = subject.AddGroup(studyGroup, StudySemesterExtensions.GetDefault(), admin, admin);
            var subjectAssignment = SubjectAssignment.Create(admin, subject, arguments);
            GroupSubjectAssignment groupSubjectAssignment = subjectAssignment.AddAssignmentForGroup(admin, groupSubject);

            Assert.AreEqual(1, subjectAssignment.GroupSubjectAssignments.Count);
        }

        [Test]
        public void CreateSubjectAssignmentSubmit_Ok()
        {
            TestCaseContext testCase = TestCaseContext.Case();

            IwentysUser admin = testCase.AccountManagementTestCaseContext.WithIwentysUser(true);
            Subject subject = SubjectFaker.Instance.Generate();
            StudyGroup studyGroup = StudyGroupFaker.Instance.CreateGroup();
            SubjectAssignmentCreateArguments arguments = SubjectAssignmentFaker.Instance.CreateSubjectAssignmentCreateArguments(subject.Id);

            GroupSubject groupSubject = subject.AddGroup(studyGroup, StudySemesterExtensions.GetDefault(), admin, admin);
            var subjectAssignment = SubjectAssignment.Create(admin, subject, arguments);
            GroupSubjectAssignment groupSubjectAssignment = subjectAssignment.AddAssignmentForGroup(admin, groupSubject);


            var student = Student.Create(UsersFaker.Instance.Students.Generate());
            studyGroup.AddStudent(student);
            groupSubjectAssignment.CreateSubmit(student, SubjectAssignmentFaker.Instance.CreateSubjectAssignmentSubmitCreateArguments(subjectAssignment.Id));

            Assert.AreEqual(1, groupSubjectAssignment.SubjectAssignmentSubmits.Count);
        }

        [Test]
        public void ApproveSubjectAssignmentSubmit_Ok()
        {
            TestCaseContext testCase = TestCaseContext.Case();

            IwentysUser admin = testCase.AccountManagementTestCaseContext.WithIwentysUser(true);
            Subject subject = SubjectFaker.Instance.Generate();
            StudyGroup studyGroup = StudyGroupFaker.Instance.CreateGroup();
            SubjectAssignmentCreateArguments arguments = SubjectAssignmentFaker.Instance.CreateSubjectAssignmentCreateArguments(subject.Id);

            GroupSubject groupSubject = subject.AddGroup(studyGroup, StudySemesterExtensions.GetDefault(), admin, admin);
            var subjectAssignment = SubjectAssignment.Create(admin, subject, arguments);
            GroupSubjectAssignment groupSubjectAssignment = subjectAssignment.AddAssignmentForGroup(admin, groupSubject);

            var student = Student.Create(UsersFaker.Instance.Students.Generate());
            studyGroup.AddStudent(student);
            SubjectAssignmentSubmit subjectAssignmentSubmit = groupSubjectAssignment.CreateSubmit(student, SubjectAssignmentFaker.Instance.CreateSubjectAssignmentSubmitCreateArguments(subjectAssignment.Id));

            subjectAssignmentSubmit.AddFeedback(admin, SubjectAssignmentFaker.Instance.CreateFeedback(subjectAssignmentSubmit.Id, FeedbackType.Approve));

            Assert.AreEqual(SubmitState.Approved, subjectAssignmentSubmit.State);
        }
    }
}