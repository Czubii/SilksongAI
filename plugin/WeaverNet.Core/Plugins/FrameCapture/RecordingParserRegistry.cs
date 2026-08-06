using System;
using System.Collections.Generic;

namespace WeaverNet.Core.Plugins.FrameCapture
{
    public class RecordingParserRegistry
    {
        private readonly Dictionary<string, IRecordingParser> _parsers = new Dictionary<string, IRecordingParser>();

        public void Register(IRecordingParser parser)
        {
            if (_parsers.ContainsKey(parser.FileExtension)) throw new InvalidOperationException($"Parser for file exstension '{parser.FileExtension}' is already registered!");
            _parsers.Add(parser.FileExtension, parser);
        }

        public bool TryGet(string extension, out IRecordingParser parser)
        {
            parser = null;

            if (!_parsers.TryGetValue(extension, out var value))
            {
                return false;
            }

            if (!(value is IRecordingParser p))
            {
                parser = null;
                return false;
            }

            parser = p;
            return true;
        }
    }
}
