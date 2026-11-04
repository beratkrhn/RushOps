namespace RushOps.Save
{
    /// <summary>
    /// The few file operations <see cref="SaveGameStore"/> needs, so tests can run without a real file system.
    /// Every method may throw I/O exceptions; the store catches them.
    /// </summary>
    public interface ISaveFileSystem
    {
        /// <summary>True if a file exists at <paramref name="path"/>.</summary>
        bool Exists(string path);

        /// <summary>Reads the whole file as UTF-8 text.</summary>
        string ReadAllText(string path);

        /// <summary>Creates or overwrites the file with UTF-8 text.</summary>
        void WriteAllText(string path, string text);

        /// <summary>Deletes the file; does nothing if it does not exist.</summary>
        void Delete(string path);

        /// <summary>Renames <paramref name="from"/> to <paramref name="to"/>; <paramref name="to"/> must not exist.</summary>
        void Move(string from, string to);
    }
}
