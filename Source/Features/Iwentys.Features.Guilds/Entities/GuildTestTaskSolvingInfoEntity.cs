﻿using System;
using Iwentys.Features.GithubIntegration.Entities;
using Iwentys.Features.Guilds.Enums;
using Iwentys.Features.Students.Entities;

namespace Iwentys.Features.Guilds.Entities
{
    public class GuildTestTaskSolvingInfoEntity
    {
        public GuildTestTaskSolvingInfoEntity()
        {
        }

        public GuildTestTaskSolvingInfoEntity(int guildId, int studentId) : this()
        {
            GuildId = guildId;
            StudentId = studentId;
            StartTime = DateTime.UtcNow;
        }

        public int GuildId { get; set; }
        public virtual GuildEntity Guild { get; set; }
        
        public int StudentId { get; set; }
        public virtual StudentEntity Student { get; set; }

        public long? ProjectId { get; set; }
        public virtual GithubProjectEntity Project { get; set; }

        public int? ReviewerId { get; set; }
        public virtual StudentEntity Reviewer { get; set; }
        
        public DateTime StartTime { get; set; }
        public DateTime? SubmitTime { get; set; }
        public DateTime? CompleteTime { get; set; }

        public void SendSubmit(long projectId)
        {
            ProjectId = projectId;
            SubmitTime = DateTime.UtcNow;
        }

        public void SetCompleted(StudentEntity reviewer)
        {
            ReviewerId = reviewer.Id;
            CompleteTime = DateTime.UtcNow;
        }

        public GuildTestTaskState GetState()
        {
            if (CompleteTime != null)
                return GuildTestTaskState.Completed;

            if (SubmitTime != null)
                return GuildTestTaskState.Submitted;

            return GuildTestTaskState.Started;
        }
    }
}