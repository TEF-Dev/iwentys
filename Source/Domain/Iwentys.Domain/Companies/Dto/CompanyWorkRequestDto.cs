﻿using System;
using System.Linq.Expressions;
using Iwentys.Domain.AccountManagement.Dto;

namespace Iwentys.Domain.Companies.Dto
{
    public record CompanyWorkRequestDto
    {
        public CompanyWorkRequestDto(CompanyInfoDto company, IwentysUserInfoDto worker)
        {
            Company = company;
            Worker = worker;
        }

        public CompanyWorkRequestDto()
        {
        }

        public static Expression<Func<CompanyWorker, CompanyWorkRequestDto>> FromEntity =>
            entity => new CompanyWorkRequestDto
            {
                Company = new CompanyInfoDto
                {
                    Id = entity.Company.Id,
                    Name = entity.Company.Name
                },
                Worker = new IwentysUserInfoDto(entity.Worker)
            };

        public CompanyInfoDto Company { get; init; }
        public IwentysUserInfoDto Worker { get; init; }
    }
}