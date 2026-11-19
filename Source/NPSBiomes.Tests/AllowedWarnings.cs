namespace NPSBiomes.Tests;

/// <summary>
/// Warnings that do not fail a test. DevTools fails a test on any warning
/// logged while it runs, from any mod, including the test map being built
/// before a fixture's first test. Every fixture names these with
/// <c>[WarningsAllowed]</c>; add a pattern here for another mod's warning
/// when running with a bigger mod list.
/// </summary>
internal static class AllowedWarnings
{
    /// <summary>
    /// RimWorld 1.7 logs this the first time any code calls a Unity method
    /// the engine does not implement. Mods built for 1.6 can set it off when
    /// the first map starts.
    /// </summary>
    public const string EngineStub = @"^\S+ \(.*UnityStubs.*\): Stub! You may not get expected results!$";
}
