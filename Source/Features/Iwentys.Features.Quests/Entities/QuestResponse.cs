﻿using System;
using Iwentys.Features.Students.Domain;
using Iwentys.Features.Students.Entities;

namespace Iwentys.Features.Quests.Entities
{
    public class QuestResponse
    {
        public int QuestId { get; init; }
        public virtual Quest Quest { get; init; }

        public int StudentId { get; init; }
        public virtual Student Student { get; init; }

        public DateTime ResponseTime { get; init; }

        public static QuestResponse New(int questId, AuthorizedUser responseCreator)
        {
            return new QuestResponse
            {
                QuestId = questId,
                StudentId = responseCreator.Id,
                ResponseTime = DateTime.UtcNow
            };
        }
    }
}