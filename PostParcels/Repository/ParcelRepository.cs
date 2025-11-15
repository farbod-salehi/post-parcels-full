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
    public class ParcelRepository(RepositoryContext repositoryContext) : RepositoryBase<Parcel>(repositoryContext)
    {
        public async Task<(List<Parcel> list, int count)> GetList(bool trackChanges, string? query = null, int page = 1, int count = 10, Func<IQueryable<Parcel>, IIncludableQueryable<Parcel, object?>>? includes = null)
        {
            var queryable= Find(trackChanges, 
                x => (query == null || x.Code.Contains(query) || x.ParcelItems!.Where(x=>x.DeletedAt == null).Any(y=>y.Code == query)) && x.DeletedAt == null,
                null, 
                x => x.OrderByDescending(y=>y.CreatedAt));

            var itemsCount = queryable.Count();

            var pagesCount = Convert.ToInt32(Math.Ceiling((double)itemsCount / count));

            if (includes != null)
            {
                queryable = includes(queryable);
            }

            return (await queryable.Skip((page - 1) * count).Take(count).ToListAsync(), pagesCount);          

        }

        public async Task<(List<(string, int)> list, int count)> GetStatistic(bool trackChanges, string? from = null, string? to = null)
        {
            char[] separators = ['/'];

            var list = await Find(trackChanges,
                x => x.DeletedAt == null,
                null,
                null).Select(x=> new { x.Code }).ToListAsync();

            var finalList = list.Where( x=> (from == null || x.Code.Split(separators)[0].CompareTo(from) >= 0) && (to == null || x.Code.Split(separators)[0].CompareTo(to) <= 0)).GroupBy(x => x.Code.Split(separators)[0]).Select(x=> new { Date = x.Key, Count = x.Count() }).ToList();

            var itemsCount = finalList.Count;

            int count = itemsCount == 0 ? 1 : finalList.Count; // all items in one page
            int page = 1;

            if(string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to))
            {
                count = 48;
            }

            
            var pagesCount = Convert.ToInt32(Math.Ceiling((double)itemsCount / count));

            return (( finalList.Skip((page - 1) * count).Take(count).ToList()).Select(x=> (x.Date, x.Count)).ToList(), pagesCount);

        }

        public async Task<Parcel?> GetById(bool trackChanges, Guid id)
        {
            return await Find(trackChanges, x => x.Id.Equals(id) && x.DeletedAt == null, includes: x => x.Include(y => y.ParcelItems!).ThenInclude(y => y.SenderUnit).Include(y => y.ParcelItems!).ThenInclude(y=>y.ReceiverUnit)).FirstOrDefaultAsync();
        }

        public async Task<int> GetParcelsCountWithCodePattern(string codeBeginning)
        {
            return await Find(false, x => x.Code.StartsWith(codeBeginning)).CountAsync();
        }
    }
}
