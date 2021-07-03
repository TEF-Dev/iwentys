﻿using System;
using System.Threading.Tasks;
using Iwentys.Domain.SubjectAssignments.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Iwentys.Infrastructure.Application.Controllers.SubjectAssignments
{
    [Route("api/subject-assignment")]
    [ApiController]
    public class SubjectAssignmentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubjectAssignmentController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpPost(nameof(Create))]
        public async Task<ActionResult> Create(SubjectAssignmentCreateArguments arguments)
        {
            AuthorizedUser authorizedUser = this.TryAuthWithToken();
            CreateSubjectAssignment.Response response = await _mediator.Send(new CreateSubjectAssignment.Query(authorizedUser, arguments));
            return Ok();
        }

        [HttpPost(nameof(Update))]
        public async Task<ActionResult> Update(SubjectAssignmentUpdateArguments arguments)
        {
            AuthorizedUser authorizedUser = this.TryAuthWithToken();
            throw new NotImplementedException();
            return Ok();
        }

        [HttpPost(nameof(Delete))]
        public async Task<ActionResult> Delete(int subjectAssignmentId)
        {
            AuthorizedUser authorizedUser = this.TryAuthWithToken();
            throw new NotImplementedException();
            return Ok();
        }
    }
}