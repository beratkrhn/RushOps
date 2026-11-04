using System.Collections.Generic;
using System.IO;
using RushOps.Save;

namespace RushOps.Tests.PlayMode
{
    /// <summary>In-memory <see cref="ISaveFileSystem"/>, so PlayMode tests never touch the real save game.</summary>
    public class InMemorySaveFiles : ISaveFileSystem
    {
        /// <summary>Files by path.</summary>
        public readonly Dictionary<string, string> Files = new Dictionary<string, string>();

        public bool Exists(string path)
        {
            return Files.ContainsKey(path);
        }

        public string ReadAllText(string path)
        {
            return Files.TryGetValue(path, out string text) ? text : throw new FileNotFoundException(path);
        }

        public void WriteAllText(string path, string text)
        {
            Files[path] = text;
        }

        public void Delete(string path)
        {
            Files.Remove(path);
        }

        public void Move(string from, string to)
        {
            Files[to] = ReadAllText(from);
            Files.Remove(from);
        }
    }
}
