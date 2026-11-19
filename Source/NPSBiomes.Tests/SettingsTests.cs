using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DevTools.Testing;

namespace NPSBiomes.Tests;

/// <summary>
/// The settings file. Each test writes to a temporary file, never to the
/// player's own, and puts every setting back as it found it.
/// </summary>
[TestFixture(TestType.MainMenu)]
[WarningsAllowed(AllowedWarnings.EngineStub)]
internal class SettingsTests
{
    private Dictionary<FieldInfo, object> before;
    private string path;

    [SetUp]
    private void Remember() {
        before = SettingsFile.Snapshot();
        path = Path.Combine(Path.GetTempPath(), $"NPSBiomes.Tests.{Guid.NewGuid():N}.xml");
    }

    [TearDown]
    private void PutBack() {
        SettingsFile.Restore(before);
        if (File.Exists(path)) {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Each setting on its own, moved off its default, saved and loaded
    /// again. The game saves a setting only when it differs from its default,
    /// so a setting with a bad key breaks the file only once a player changes
    /// it, and then the whole file is lost, not just that setting.
    /// </summary>
    [Test]
    private void EverySettingSurvivesASaveAndLoad() {
        Dictionary<FieldInfo, object> defaults = SettingsFile.LoadDefaults(path);
        foreach (FieldInfo field in SettingsFile.Settings) {
            SettingsFile.Restore(defaults);
            object changed = SettingsFile.OffDefault(defaults[field]);
            field.SetValue(null, changed);

            string problem = SettingsFile.Write(path);
            string writeLine = $"{field.Name} = {changed}: the settings file is written";
            if (!Check.That(problem == null, problem == null ? writeLine : $"{writeLine} ({problem})")) {
                continue;
            }

            field.SetValue(null, defaults[field]);
            Exception readError = SettingsFile.Read(path);
            object read = field.GetValue(null);
            Check.That(readError == null && SettingsFile.Same(read, changed),
                readError == null
                    ? $"{field.Name} = {changed}: reads back as {read}"
                    : $"{field.Name} = {changed}: reading the file threw {readError.GetType().Name}: {readError.Message}");
        }
    }

    /// <summary>
    /// Each setting has one default: the value its field starts at is the
    /// value loading falls back to. The Reset buttons in the settings window
    /// hold a third copy of each default, which this test cannot reach.
    /// </summary>
    [Test]
    private void EverySettingHasOneDefault() {
        Dictionary<FieldInfo, object> starting = SettingsFile.StartingValues();
        Dictionary<FieldInfo, object> loaded = SettingsFile.LoadDefaults(path);
        foreach (FieldInfo field in SettingsFile.Settings) {
            Check.That(SettingsFile.Same(starting[field], loaded[field]),
                $"{field.Name} starts at {starting[field]}; its load default is {loaded[field]}");
        }
    }
}
