using Entities;
using Entities.Models;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class UnitRepository(RepositoryContext repositoryContext) : RepositoryBase<Unit>(repositoryContext)
    {
        public async Task<(List<Unit> list, int count)> GetList(bool trackChanges,string? searchQuery = null, Guid? parentId = null, bool? active = null, int page = 1, int count = 10, Func<IQueryable<Unit>, IIncludableQueryable<Unit, object?>>? includes = null)
        {
            var queryable = Find(trackChanges,
                x => ((parentId == null && x.ParentId == null) || (parentId != null && x.ParentId == parentId)) && 
                (string.IsNullOrWhiteSpace(searchQuery) || x.Name.Contains(searchQuery)) && 
                (active == null || x.Active == active) &&
                x.DeletedAt == null,
                null,
                x => x.OrderBy(y => y.Code));

            var itemsCount = queryable.Count();

            var pagesCount = Convert.ToInt32(Math.Ceiling((double)itemsCount / count));

            if (includes != null)
            {
                queryable = includes(queryable);
            }

            return (await queryable.Skip((page - 1) * count).Take(count).ToListAsync(), pagesCount);

        }

        public async Task<List<Unit>> GetAll(bool trackChanges, bool onlyParents, bool? active = null)
        {
            return await Find(trackChanges, x => 
                (onlyParents == false || x.ParentId == null) && 
                (active == null || x.Active == active) && x.DeletedAt == null, orderBy: x => x.OrderBy(y => y.Code)).ToListAsync();
        }

        public async Task<Unit?> GetById(bool trackChanges, Guid id, bool? active = null)
        {
            return await Find(trackChanges, x => x.Id.Equals(id) && (active == null || x.Active == active) && x.DeletedAt == null, includes: x => x.Include(y => y.SubUnits)).FirstOrDefaultAsync();
        }

        public async Task<Unit?> GetByCode(bool trackChanges, string code, bool? active = null)
        {
            return await Find(trackChanges, x => x.Code.Equals(code) && (active == null || x.Active == active) && x.DeletedAt == null).FirstOrDefaultAsync();
        }

    }
}
