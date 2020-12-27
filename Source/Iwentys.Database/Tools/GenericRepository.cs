﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iwentys.Common.Databases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Iwentys.Database.Tools
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        internal readonly DbSet<TEntity> DbSet;
        internal readonly DbContext Context;

        public GenericRepository(DbContext context)
        {
            Context = context;
            DbSet = context.Set<TEntity>();
        }

        public IQueryable<TEntity> Get()
        {
            return DbSet;
        }

        public async Task<TEntity> FindByIdAsync<TKey>(TKey id)
        {
            return await DbSet.FindAsync(id);
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            EntityEntry<TEntity> result = await DbSet.AddAsync(entity);
            return result.Entity;
        }

        public async Task InsertAsync(IEnumerable<TEntity> entities)
        {
            await DbSet.AddRangeAsync(entities);
        }

        public async Task DeleteAsync<TKey>(TKey id)
        {
            TEntity entityToDelete = await DbSet.FindAsync(id);
            Delete(entityToDelete);
        }

        public void Delete(TEntity entityToDelete)
        {
            if (Context.Entry(entityToDelete).State == EntityState.Detached)
                DbSet.Attach(entityToDelete);

            DbSet.Remove(entityToDelete);
        }

        public void Update(List<TEntity> entitiesToUpdate)
        {
            entitiesToUpdate.ForEach(Update);
        }

        public void Update(TEntity entityToUpdate)
        {
            DbSet.Attach(entityToUpdate);
            Context.Entry(entityToUpdate).State = EntityState.Modified;
        }
    }
}