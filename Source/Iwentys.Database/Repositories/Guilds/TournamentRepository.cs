﻿using System.Linq;
using System.Threading.Tasks;
using Iwentys.Database.Context;
using Iwentys.Features.Guilds.Entities;
using Iwentys.Features.Guilds.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Iwentys.Database.Repositories.Guilds
{
    public class TournamentRepository : ITournamentRepository
    {
        private readonly IwentysDbContext _dbContext;

        public TournamentRepository(IwentysDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<TournamentEntity> Read()
        {
            return _dbContext.Tournaments;
        }

        public Task<TournamentEntity> ReadByIdAsync(int key)
        {
            return _dbContext.Tournaments.FirstOrDefaultAsync(v => v.Id == key);
        }

        public async Task<TournamentEntity> UpdateAsync(TournamentEntity entity)
        {
            EntityEntry<TournamentEntity> createdEntity = _dbContext.Tournaments.Update(entity);
            await _dbContext.SaveChangesAsync();
            return createdEntity.Entity;
        }

        public Task<int> DeleteAsync(int key)
        {
            return _dbContext.Tournaments.Where(t => t.Id == key).DeleteFromQueryAsync();
        }

        public TournamentEntity Create(TournamentEntity entity)
        {
            EntityEntry<TournamentEntity> createdEntity = _dbContext.Tournaments.Add(entity);
            _dbContext.SaveChanges();
            return createdEntity.Entity;
        }
    }
}