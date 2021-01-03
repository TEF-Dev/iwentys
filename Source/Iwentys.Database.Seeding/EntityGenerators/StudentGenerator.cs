﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Iwentys.Database.Seeding.FakerEntities;
using Iwentys.Database.Seeding.Tools;
using Iwentys.Features.Study.Entities;
using Iwentys.Features.Study.Enums;
using Microsoft.EntityFrameworkCore;

namespace Iwentys.Database.Seeding.EntityGenerators
{
    public class StudentGenerator : IEntityGenerator
    {
        private const int StudentCount = 200;

        public StudentGenerator(List<StudyGroup> studyGroups)
        {
            var faker = new StudentFaker(() => studyGroups.GetRandom().Id);

            Students = faker.Generate(StudentCount);
            Students.Add(new Student
            {
                Id = 228617,
                FirstName = "Фреди",
                MiddleName = "Кисикович",
                SecondName = "Катс",
                IsAdmin = true,
                GroupId = studyGroups.First(g => g.GroupName.Contains("3505")).Id,
                GithubUsername = "InRedikaWB",
                CreationTime = DateTime.UtcNow,
                LastOnlineTime = DateTime.UtcNow,
                BarsPoints = short.MaxValue,
                AvatarUrl = new Faker().Image.PicsumUrl()
            });

            Students
                .GroupBy(s => s.GroupId)
                .Select(g => g.FirstOrDefault(s => s.Role == StudentRole.Common))
                .Where(s => s is not null)
                .ToList()
                .ForEach(s => s.Role = StudentRole.GroupAdmin);
        }

        public List<Student> Students { get; set; }

        public void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().HasData(Students);
        }
    }
}