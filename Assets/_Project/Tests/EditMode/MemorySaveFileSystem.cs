using System.Collections.Generic;
using System.IO;
using RushOps.Save;

namespace RushOps.Tests.EditMode
{
    /// <summary>In-memory <see cref="ISaveFileSystem"/> for tests; can fail on a chosen operation.</summary>
    public class MemorySaveFileSystem : ISaveFileSystem
    {
        /// <summary>Files by path.</summary>
        public readonly Dictionary<string, string> Files = new Dictionary<string, string>();

        /// <summary>If set, moving a file onto this path throws, like a crash in the middle of a save.</summary>
        public string FailMoveTo;

        /// <summary>If true, every write throws, like a full disk.</summary>
        public bool FailWrites;

        public bool Exists(string path)
        {
            return Files.ContainsKey(path);
        }

        public string ReadAllText(string path)
        {
            if (!Files.TryGetValue(path, out string text))
            {
                throw new FileNotFoundException(path);
            }

            return text;
        }

        public void WriteAllText(string path, string text)
        {
            if (FailWrites)
            {
                throw new IOException("Disk full");
            }

            Files[path] = text;
        }

        public void Delete(string path)
        {
            Files.Remove(path);
        }

        public void Move(string from, string to)
        {
            if (to == FailMoveTo)
            {
                throw new IOException("Crash");
            }

            if (Files.ContainsKey(to))
            {
                throw new IOException($"{to} exists");
            }

            Files[to] = ReadAllText(from);
            Files.Remove(from);
        }
    }
}
