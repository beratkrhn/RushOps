namespace RushOps.Save
{
    /// <summary>Where <see cref="SaveGameStore.Load"/> got the save from.</summary>
    public enum SaveLoadSource
    {
        /// <summary>No save existed; a new one was created.</summary>
        New,

        /// <summary>The main file <c>save.json</c>.</summary>
        Main,

        /// <summary>The main file was missing or broken; the backup <c>save.json.bak</c> was used.</summary>
        Backup,

        /// <summary>Main file and backup were broken; a new save was created.</summary>
        Reset,
    }
}
