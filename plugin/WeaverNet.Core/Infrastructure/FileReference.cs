using System;
using System.IO;
namespace WeaverNet.Core.Infrastructure
{
    public class FileReference<TMetadata> where TMetadata : class
    {
        public Guid Id { get; set; }
        public TMetadata Metadata { get; set; }
        public FileInfo File { get; set; }
        public string FileExtension { get; set; }
        public FileReference() { }
        public FileReference(
            Guid id,
            TMetadata metadata,
            FileInfo file,
            string fileExtension)
        {
            Id = id;
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            File = file ?? throw new ArgumentNullException(nameof(file));
            FileExtension = fileExtension;
        }
    }
}
