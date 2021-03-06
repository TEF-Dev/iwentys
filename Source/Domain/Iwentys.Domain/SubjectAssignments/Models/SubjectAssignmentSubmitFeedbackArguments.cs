﻿using Iwentys.Domain.SubjectAssignments.Enums;

namespace Iwentys.Domain.SubjectAssignments.Models
{
    public class SubjectAssignmentSubmitFeedbackArguments
    {
        public int SubjectAssignmentSubmitId { get; set; }
        public string Comment { get; set; }
        public FeedbackType FeedbackType { get; set; }

        /// <summary>
        /// If feedback type is "Reject" - value must be null.
        /// </summary>
        public int? Points { get; set; }
    }
}