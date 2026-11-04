namespace RushOps.Save
{
    /// <summary>
    /// One step that turns the JSON of a save from <see cref="FromVersion"/> into <see cref="FromVersion"/> + 1
    /// (F-DAT-05, ADR 0005). Steps work on the JSON text, because the old format no longer has a C# class.
    /// </summary>
    public interface ISaveMigration
    {
        /// <summary>Version this step reads.</summary>
        int FromVersion { get; }

        /// <summary>Returns the JSON in the next version; may throw if the input is broken.</summary>
        string Migrate(string json);
    }
}
