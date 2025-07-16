using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class RepositoryManager (RepositoryContext repositoryContext)
    {
        private readonly RepositoryContext _repositoryContext = repositoryContext;

        private UserRepository? _userRepository;
        private UserTokenRepository? _userTokenRepository;
        private ParcelRepository? _parcelRepository;
        private UnitRepository? _unitRepository;
        private ParcelItemRepository? _parcelItemRepository;
        private ParcelDocumentRepository? _parcelDocumentRepository;

        public UserRepository User
        {
            get
            {               
                _userRepository ??= new(_repositoryContext);

                return _userRepository;
            }
        }

        public UserTokenRepository UserToken
        {
            get
            {
                _userTokenRepository ??= new(_repositoryContext);

                return _userTokenRepository;
            }
        }

        public ParcelRepository Parcel
        {
            get
            {
                _parcelRepository ??= new(_repositoryContext);

                return _parcelRepository;
            }
        }
   

        public UnitRepository Unit
        {
            get
            {
                _unitRepository ??= new(_repositoryContext);

                return _unitRepository;
            }
        }

        public ParcelItemRepository ParcelItem
        {
            get
            {
                _parcelItemRepository ??= new(_repositoryContext);

                return _parcelItemRepository;
            }
        }

        public ParcelDocumentRepository ParcelDocument
        {
            get
            {
                _parcelDocumentRepository ??= new(_repositoryContext);

                return _parcelDocumentRepository;
            }
        }

        public async Task SaveAsync() => await _repositoryContext.SaveChangesAsync();

    }
}
