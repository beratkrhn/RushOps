using System.IO;
using System.Text;

namespace RushOps.Save
{
    /// <summary><see cref="ISaveFileSystem"/> on the device's file system.</summary>
    public class DiskSaveFileSystem : ISaveFileSystem
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        /// <inheritdoc />
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        /// <inheritdoc />
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path, Utf8);
        }

        /// <inheritdoc />
        public void WriteAllText(string path, string text)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, text, Utf8);
        }

        /// <inheritdoc />
        public void Delete(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <inheritdoc />
        public void Move(string from, string to)
        {
            File.Move(from, to);
        }
    }
}
