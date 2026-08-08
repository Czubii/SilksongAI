using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Infrastructure.Interfaces
{
    public interface IFileReferenceRepository<TMetadata, TId> : 
        IRepository<FileReference<TMetadata>, TId> where TMetadata : class
    {
        /// <summary>
        /// Copies the referenced file to the repository and adds it to the database.
        /// </summary>
        void Import(FileReference<TMetadata> fileReference);
    }
}
