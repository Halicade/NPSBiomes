using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DevTools.Testing;
using Verse;

namespace NPSBiomes.Tests;

/// <summary>
/// LoadFolders.xml against the folders in the mod. Windows and macOS find a
/// folder whatever the case of its name; Linux, the Steam Deck included, finds
/// only the exact spelling, and a folder it cannot find loads nothing and logs
/// nothing. This test needs the exact spelling on every system.
/// </summary>
[TestFixture(TestType.MainMenu)]
[WarningsAllowed(AllowedWarnings.EngineStub)]
internal class LoadFolderTests
{
    [Test]
    private void EveryFolderExistsWithThatSpelling() {
        string root = LoadedModManager.GetMod<BiomesController>().Content.RootDir;
        XDocument loadFolders = XDocument.Load(Path.Combine(root, "LoadFolders.xml"));
        foreach (XElement version in loadFolders.Root.Elements()) {
            foreach (XElement entry in version.Elements("li")) {
                string folder = entry.Value.Trim();
                if (folder == "/" || folder.Length == 0) {
                    continue;
                }
                string found = Find(root, folder);
                string line = found == folder
                    ? $"{version.Name.LocalName}: {folder} exists"
                    : found != null
                        ? $"{version.Name.LocalName}: {folder} is spelled {found} on disk, which only a system that ignores case finds"
                        : $"{version.Name.LocalName}: {folder} does not exist";
                Check.That(found == folder, line);
            }
        }
    }

    /// <summary>
    /// The folder's path as spelled on disk, matching each part of
    /// <paramref name="relative"/> exactly where a folder of that spelling
    /// exists and ignoring case otherwise; null when there is no such folder.
    /// </summary>
    private static string Find(string root, string relative) {
        string dir = root;
        string[] parts = relative.Split('/', '\\').Where(p => p.Length > 0).ToArray();
        for (int i = 0; i < parts.Length; i++) {
            string[] names = Directory.GetDirectories(dir).Select(Path.GetFileName).ToArray();
            string name = names.FirstOrDefault(n => n == parts[i])
                          ?? names.FirstOrDefault(n => string.Equals(n, parts[i], StringComparison.OrdinalIgnoreCase));
            if (name == null) {
                return null;
            }
            parts[i] = name;
            dir = Path.Combine(dir, name);
        }
        return string.Join("/", parts);
    }
}
