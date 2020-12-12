﻿using System;
using Iwentys.Features.Students.Domain;
using Iwentys.Features.Students.Entities;

namespace Iwentys.Features.Quests.Entities
{
    public class QuestResponseEntity
    {
        public virtual QuestEntity QuestEntity { get; set; }
        public int QuestId { get; set; }

        public virtual StudentEntity Student { get; set; }
        public int StudentId { get; set; }

        public DateTime ResponseTime { get; set; }

        public static QuestResponseEntity New(int questId, AuthorizedUser responseCreator)
        {
            return new QuestResponseEntity
            {
                QuestId = questId,
                StudentId = responseCreator.Id,
                ResponseTime = DateTime.UtcNow
            };
        }
    }
}