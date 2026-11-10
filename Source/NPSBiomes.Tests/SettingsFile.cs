using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace NPSBiomes.Tests;

/// <summary>
/// The mod's settings as the game saves and loads them, written to a file of
/// the test's own instead of the player's settings file.
/// </summary>
internal static class SettingsFile
{
    /// <summary>Every public static on <see cref="BiomeSettings"/> that can
    /// be set, so not a const or a static readonly.</summary>
    private static IEnumerable<FieldInfo> Settable =>
        typeof(BiomeSettings).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => !f.IsLiteral && !f.IsInitOnly);

    /// <summary>
    /// Every setting: each settable public static bool and float on
    /// <see cref="BiomeSettings"/>, except <c>MedievalOverhaulActive</c>,
    /// which the mod sets at startup from the mod list and never saves.
    /// </summary>
    public static List<FieldInfo> Settings =>
        Settable
            .Where(f => f.FieldType == typeof(bool) || f.FieldType == typeof(float))
            .Where(f => f.Name != nameof(BiomeSettings.MedievalOverhaulActive))
            .ToList();

    public static Dictionary<FieldInfo, object> Snapshot() {
        return Settable.ToDictionary(f => f, f => f.GetValue(null));
    }

    public static void Restore(Dictionary<FieldInfo, object> snapshot) {
        foreach (KeyValuePair<FieldInfo, object> entry in snapshot) {
            entry.Key.SetValue(null, entry.Value);
        }
    }

    /// <summary>
    /// The value each setting starts at in a game before any settings file is
    /// read: its field initializer. The runtime runs a static constructor only
    /// once, and BiomeSettings' ran before the player's settings were loaded,
    /// so this reads the initializers from the constructor's IL instead, where
    /// each is a constant followed by a store to its field. A setting with no
    /// initializer starts at false or 0. Other statics may start at anything;
    /// a setting that starts at something other than a constant throws.
    /// Changes nothing.
    /// </summary>
    public static Dictionary<FieldInfo, object> StartingValues() {
        ConstructorInfo init = typeof(BiomeSettings).TypeInitializer;
        Dictionary<FieldInfo, object> found = init == null ? new Dictionary<FieldInfo, object>() : Initializers(init);
        return Settings.ToDictionary(f => f,
            f => found.TryGetValue(f, out object value) ? value : Activator.CreateInstance(f.FieldType));
    }

    /// <summary>Every IL opcode, by its value.</summary>
    private static readonly Dictionary<short, OpCode> OpCodesByValue = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(f => (OpCode)f.GetValue(null))
        .ToDictionary(op => op.Value);

    private static Dictionary<FieldInfo, object> Initializers(ConstructorInfo init) {
        byte[] il = init.GetMethodBody().GetILAsByteArray();
        List<FieldInfo> settings = Settings;
        var found = new Dictionary<FieldInfo, object>();
        object constant = null;
        int i = 0;
        while (i < il.Length) {
            bool twoBytes = il[i] == 0xFE;
            short value = twoBytes ? unchecked((short)(0xFE00 | il[i + 1])) : il[i];
            i += twoBytes ? 2 : 1;
            if (!OpCodesByValue.TryGetValue(value, out OpCode op)) {
                throw new NotSupportedException($"BiomeSettings' static constructor has an unknown IL opcode 0x{value:X}");
            }
            int operand = i;
            i += OperandSize(op.OperandType, il, operand);
            switch (value) {
                case 0x00: // nop
                    break;
                case 0x15: // ldc.i4.m1
                    constant = -1;
                    break;
                case >= 0x16 and <= 0x1E: // ldc.i4.0 to ldc.i4.8
                    constant = value - 0x16;
                    break;
                case 0x1F: // ldc.i4.s
                    constant = (int)(sbyte)il[operand];
                    break;
                case 0x20: // ldc.i4
                    constant = BitConverter.ToInt32(il, operand);
                    break;
                case 0x22: // ldc.r4
                    constant = BitConverter.ToSingle(il, operand);
                    break;
                case 0x80: { // stsfld
                    FieldInfo field = init.Module.ResolveField(BitConverter.ToInt32(il, operand));
                    if (settings.Contains(field)) {
                        if (constant == null) {
                            throw new NotSupportedException($"BiomeSettings.{field.Name} starts at a value that is not a constant");
                        }
                        found[field] = field.FieldType == typeof(bool)
                            ? Convert.ToInt32(constant) != 0
                            : (object)Convert.ToSingle(constant);
                    }
                    constant = null;
                    break;
                }
                default:
                    constant = null;
                    break;
            }
        }
        return found;
    }

    private static int OperandSize(OperandType type, byte[] il, int at) => type switch {
        OperandType.InlineNone => 0,
        OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
        OperandType.InlineVar => 2,
        OperandType.InlineI8 or OperandType.InlineR => 8,
        OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, at),
        _ => 4,
    };

    /// <summary>
    /// The value each setting takes when the settings file does not mention
    /// it, as on loading a file written while it sat at that value. Loads a
    /// file with an empty settings block. The caller puts the old values back.
    /// </summary>
    public static Dictionary<FieldInfo, object> LoadDefaults(string path) {
        File.WriteAllText(path, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<SettingsBlock>\n\t<ModSettings />\n</SettingsBlock>\n");
        Exception thrown = Read(path);
        if (thrown != null) {
            throw new InvalidOperationException("Could not read an empty settings file", thrown);
        }
        return Snapshot();
    }

    /// <summary>
    /// Writes the settings the way <c>LoadedModManager.WriteModSettings</c>
    /// does, to <paramref name="path"/>. Returns null when the file is
    /// written, or else what went wrong: the first error the game logged while
    /// writing, which names the cause, or failing that the first exception.
    /// </summary>
    public static string Write(string path) {
        LogMessage lastBefore = Log.Messages.LastOrDefault();
        Exception first = null;
        ModSettings settings = new BiomeSettings();
        try {
            Scribe.saver.InitSaving(path, "SettingsBlock");
            try {
                Scribe_Deep.Look(ref settings, "ModSettings");
            }
            catch (Exception e) {
                first = e;
            }
            try {
                Scribe.saver.FinalizeSaving();
            }
            catch (Exception e) {
                first ??= e;
            }
        }
        catch (Exception e) {
            first ??= e;
        }
        finally {
            if (Scribe.mode != LoadSaveMode.Inactive) {
                Scribe.ForceStop();
            }
        }
        if (first == null) {
            return null;
        }
        List<LogMessage> messages = Log.Messages.ToList();
        string logged = messages.Skip(lastBefore == null ? 0 : messages.IndexOf(lastBefore) + 1)
            .Where(m => m.type == LogMessageType.Error)
            .Select(m => m.text.Split('\n')[0])
            .FirstOrDefault();
        return logged ?? $"{first.GetType().Name}: {first.Message}";
    }

    /// <summary>
    /// Reads the settings the way <c>LoadedModManager.ReadModSettings</c>
    /// does, from <paramref name="path"/>. Returns the exception thrown, or
    /// null.
    /// </summary>
    public static Exception Read(string path) {
        BiomeSettings settings = null;
        try {
            Scribe.loader.InitLoading(path);
            try {
                Scribe_Deep.Look(ref settings, "ModSettings");
            }
            finally {
                Scribe.loader.FinalizeLoading();
            }
        }
        catch (Exception e) {
            return e;
        }
        finally {
            if (Scribe.mode != LoadSaveMode.Inactive) {
                Scribe.ForceStop();
            }
        }
        return null;
    }

    /// <summary>A value no setting has as its default.</summary>
    public static object OffDefault(object loadDefault) {
        return loadDefault is bool b ? !b : (object)((float)loadDefault * 1.5f + 7.25f);
    }

    public static bool Same(object a, object b) {
        if (a is float x && b is float y) {
            return Math.Abs(x - y) < 0.0001f;
        }
        return Equals(a, b);
    }
}
