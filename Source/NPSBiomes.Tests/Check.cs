using DevTools;
using DevTools.Testing;

namespace NPSBiomes.Tests;

/// <summary>
/// One line of a test, written to DevTools' Test.log whether it passes or
/// fails, so a run reads as a full record. A failed line fails the test, and
/// the test goes on to its next line.
/// </summary>
internal static class Check
{
    public static bool That(bool ok, string what) {
        DevLog.Write($"  {(ok ? "PASS" : "FAIL")} {what}");
        Expect.IsTrue(ok, what);
        return ok;
    }

    /// <summary>A line for the record that checks nothing.</summary>
    public static void Note(string line) {
        DevLog.Write("    " + line);
    }

    /// <summary>Skips the test, and says why in the log as well.</summary>
    public static void Skip(string why) {
        DevLog.Write("  SKIP " + why);
        Test.Skip(why);
    }
}
