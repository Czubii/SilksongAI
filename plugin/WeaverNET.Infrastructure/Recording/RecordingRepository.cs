using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Plugins.Recording;

namespace WeaverNET.Infrastructure.Recording
{
    public class RecordingRepository : LiteDBRepository<FileReference<BossfightRecordingMetadata>>, IRecordingRepository
    {
        private readonly string _recordingRootDirectory;

        public RecordingRepository(string recordingRootDirectory)
            : base(EnsureAndGetDbPath(recordingRootDirectory))
        {
            _recordingRootDirectory = recordingRootDirectory;
        }

        private static string EnsureAndGetDbPath(string rootDir)
        {
            if (!Directory.Exists(rootDir))
            {
                Directory.CreateDirectory(rootDir);
            }
            return Path.Combine(rootDir, "recordings.db");
        }

        public override void ConfigureMapper(BsonMapper mapper)
        {
            try
            {
                mapper.IncludeFields = true;

                // Register FileInfo mapping so LiteDB serializes it as a string path
                mapper.RegisterType<FileInfo>(
                    fileInfo => fileInfo.FullName,
                    bson => new FileInfo(bson.AsString)
                );

                mapper.Entity<FileReference<BossfightRecordingMetadata>>()
                    .Id(x => x.Id);
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Error configuring LiteDB mapper: {ex.Message}");
                throw;
            }
        }

        public void Import(FileReference<BossfightRecordingMetadata> fileReference)
        {
            var recordingRoot = GetOutputPath(fileReference.Metadata.BossId);

            Directory.CreateDirectory(recordingRoot);

            string recordingPath = Path.Combine(
                recordingRoot,
                fileReference.Metadata.Id + fileReference.FileExtension);

            File.Move(
                fileReference.File.FullName,
                recordingPath);

            var storedRecording = new FileReference<BossfightRecordingMetadata>(
                fileReference.Metadata.Id,
                fileReference.Metadata,
                new FileInfo(recordingPath),
                fileReference.FileExtension);

            // Bug fix: Save storedRecording instead of fileReference
            AddOrReplace(storedRecording);
        }

        public int CountForBoss(string bossId)
        {
            return Collection.Count(x => x.Metadata.BossId == bossId);
        }
        private string GetOutputPath(string bossName)
        {
            return Path.Combine(
                _recordingRootDirectory,
                SanitizeFileName(bossName));
        }

        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name;
        }
        public IReadOnlyList<string> GetDistinctBossIds()
        {
            return Collection.FindAll().Select(x => x.Metadata.BossId).Distinct().ToList();
        }
        public IReadOnlyList<Loadout> GetDistinctLoadouts()
        {
            return Collection.FindAll().Select(x => x.Metadata.Loadout)
            .Distinct(new LoadoutValueComparer()).ToList();
        }
        public int CountRecordings(string bossId = null, Loadout loadout = null)
        {
            return Collection.Count(x =>
                (bossId == null || x.Metadata.BossId == bossId) &&
                (loadout == null || new LoadoutValueComparer().Equals(x.Metadata.Loadout, loadout)));
        }
    }
}