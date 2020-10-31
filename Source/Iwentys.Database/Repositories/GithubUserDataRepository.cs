﻿using System.Linq;
using System.Threading.Tasks;
using Iwentys.Database.Context;
using Iwentys.Models.Entities.Github;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Iwentys.Database.Repositories
{
    public class GithubUserDataRepository : IGenericRepository<GithubUserEntity, int>
    {
        private readonly IwentysDbContext _dbContext;

        public GithubUserDataRepository(IwentysDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public GithubUserEntity Create(GithubUserEntity entity)
        {
            EntityEntry<GithubUserEntity> createdEntry = _dbContext.GithubUsersData.Add(entity);
            _dbContext.SaveChanges();
            return createdEntry.Entity;
        }

        public IQueryable<GithubUserEntity> Read()
        {
            return _dbContext.GithubUsersData;
        }

        public Task<GithubUserEntity> ReadByIdAsync(int key)
        {
            return _dbContext.GithubUsersData.FirstOrDefaultAsync(v => v.StudentId == key);
        }

        public async Task<GithubUserEntity> UpdateAsync(GithubUserEntity entity)
        {
            EntityEntry<GithubUserEntity> createdEntry = _dbContext.GithubUsersData.Update(entity);
            await _dbContext.SaveChangesAsync();
            return createdEntry.Entity;
        }

        public Task<int> DeleteAsync(int key)
        {
            return _dbContext.GithubUsersData.Where(gu => gu.StudentId == key).DeleteFromQueryAsync();
        }

        public GithubUserEntity FindByUsername(string username)
        {
            return Read().SingleOrDefault(g => g.Username == username);
        }
    }
}