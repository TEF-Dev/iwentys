﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Iwentys.Sdk;

namespace Iwentys.Endpoints.WebClient.Pages.PeerReview
{
    public partial class ReviewRequestJounalPage
    {
        private ICollection<ProjectReviewRequestInfoDto> _projectReviewRequests;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _projectReviewRequests = await PeerReviewClient.GetProjectReviewRequestsAsync();
        }

        public static string LinkToReviewRequestCreatePage() => "peer-review/create";
        public static string LinkToSendingFeedback(ProjectReviewRequestInfoDto request) => $"/peer-review/{request.Id}/feedback";
    }
}
