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
    public class ParcelItemRepository(RepositoryContext repositoryContext) : RepositoryBase<ParcelItem>(repositoryContext)
    {
        public async Task<(List<ParcelItem> list, int count)> GetList(bool trackChanges, string? code = null, int page = 1, int count = 10)
        {
            var queryable = Find(trackChanges,
                x => (code == null || (x.Code != null && x.Code!.Contains(code))) && x.DeletedAt == null,
                x => x.Include(y => y.SenderUnit).Include(y => y.ReceiverUnit).Include(y => y.Parcel),
                x => x.OrderByDescending(y => y.CreatedAt));

            var itemsCount = queryable.Count();

            var pagesCount = Convert.ToInt32(Math.Ceiling((double)itemsCount / count));

            return (await queryable.Skip((page - 1) * count).Take(count).ToListAsync(), pagesCount);

        }
    }
}
