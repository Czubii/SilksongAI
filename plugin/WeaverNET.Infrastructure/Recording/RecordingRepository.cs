using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Plugins.Recording;

namespace WeaverNET.Infrastructure.Recording
{
    public class RecordingRepository : IRecordingRepository
    {
        private readonly Dictionary<Guid, BossfightRecording> _entriesById =
            new Dictionary<Guid, BossfightRecording>();

        public IReadOnlyCollection<BossfightRecording> All
        {
            get
            {
                EnsureLoaded();
                return _entriesById.Values;
            }
        }

        public event Action RepositoryChanged;

        private readonly string _repositoryRoot;
        private bool _loaded;

        public RecordingRepository(string rootDirectory)
        {
            _repositoryRoot = rootDirectory ??
                throw new ArgumentNullException(nameof(rootDirectory));
        }

        private void EnsureLoaded()
        {
            if (_loaded)
                return;

            Load();
            _loaded = true;
        }

        /// <summary>
        /// Moves the actual Recording File.
        /// </summary>
        public void Add(BossfightRecording data)
        {
            EnsureLoaded();

            if (_entriesById.ContainsKey(data.Metadata.Id))
                throw new ArgumentException(
                    $"Recording '{data.Metadata.Id}' already exists.");

            var recordingRoot = GetOutputPath(
                data.Metadata.Id,
                data.Metadata.BossId);

            Directory.CreateDirectory(recordingRoot);

            string recordingPath = Path.Combine(
                recordingRoot,
                "recording" + data.Metadata.RecordingFormat);

            File.Move(
                data.RecordingFile.FullName,
                recordingPath);

            string metadataJsonString =
                JsonConvert.SerializeObject(
                    data.Metadata,
                    Formatting.Indented);

            File.WriteAllText(
                Path.Combine(recordingRoot, "metadata.json"),
                metadataJsonString);

            var storedRecording = new BossfightRecording(
                data.Metadata,
                new FileInfo(recordingPath));

            _entriesById.Add(
                data.Metadata.Id,
                storedRecording);

            RepositoryChanged?.Invoke();
        }

        /// <summary>
        /// Moves the actual Recording File.
        /// </summary>
        public void AddOrReplace(BossfightRecording data)
        {
            EnsureLoaded();

            if (_entriesById.ContainsKey(data.Metadata.Id))
            {
                Remove(data.Metadata.Id);
            }

            Add(data);
        }

        public void Remove(Guid id)
        {
            EnsureLoaded();

            BossfightRecording recording;

            if (!_entriesById.TryGetValue(id, out recording))
                return;

            string folder = recording.RecordingFile.DirectoryName;

            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            _entriesById.Remove(id);

            RepositoryChanged?.Invoke();
        }

        public BossfightRecording GetById(Guid id)
        {
            EnsureLoaded();

            return _entriesById[id];
        }

        private string GetOutputPath(Guid recordingId, string bossName)
        {
            return Path.Combine(
                _repositoryRoot,
                SanitizeFileName(bossName),
                recordingId.ToString());
        }

        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name;
        }

        private void Load()
        {
            _entriesById.Clear();

            if (!Directory.Exists(_repositoryRoot))
            {
                Directory.CreateDirectory(_repositoryRoot);
                return;
            }

            foreach (var files in FindRecordings(_repositoryRoot))
            {
                try
                {
                    string jsonString =
                        File.ReadAllText(files.MetadataFile.FullName);

                    var metadata =
                        JsonConvert.DeserializeObject<BossfightRecordingMetadata>(
                            jsonString);

                    if (metadata == null)
                        continue;

                    var recording = new BossfightRecording(
                        metadata,
                        files.RecordingFile);

                    _entriesById.Add(
                        metadata.Id,
                        recording);
                }
                catch (Exception ex)
                {
                    PluginLog.Error(
                        $"Failed loading recording: {ex.Message}");
                }
            }
        }

        private IEnumerable<(FileInfo MetadataFile, FileInfo RecordingFile)> FindRecordings(
            string path)
        {
            foreach (string directory in Directory.EnumerateDirectories(
                path,
                "*",
                SearchOption.AllDirectories))
            {
                string metadataPath =
                    Path.Combine(directory, "metadata.json");

                if (!File.Exists(metadataPath))
                    continue;

                FileInfo recordingFile = null;

                foreach (string file in Directory.EnumerateFiles(directory))
                {
                    string fileName = Path.GetFileName(file);

                    if (fileName.StartsWith(
                        "recording.",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        recordingFile = new FileInfo(file);
                        break;
                    }
                }

                if (recordingFile == null)
                    continue;

                yield return (
                    new FileInfo(metadataPath),
                    recordingFile);
            }
        }
    }
}