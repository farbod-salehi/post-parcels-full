using Entities;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class ParcelDocumentRepository(RepositoryContext repositoryContext) : RepositoryBase<ParcelDocument>(repositoryContext)
    {
        public Task<List<ParcelDocument>> GetAll(bool trackChanges, Guid parcelId)
        {
            return Find(trackChanges, x => x.ParcelId.Equals(parcelId) && x.DeletedAt == null).ToListAsync();
        }

        public Task<ParcelDocument?> GetById(bool trackChanges, Guid id)
        {
            return Find(trackChanges, x => x.Id.Equals(id) && x.DeletedAt == null).FirstOrDefaultAsync();
        }
    }
}
