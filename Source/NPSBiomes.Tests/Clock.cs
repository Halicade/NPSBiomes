using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;

namespace NPSBiomes.Tests;

/// <summary>
/// Runs the game's clock for a test one tick at a time, a chunk of ticks a
/// frame, with the game itself paused. Counting ticks ourselves gives an exact
/// number, where waiting between frames at speed overshoots by hundreds.
///
/// While it runs, the storyteller and random mental states are off and the
/// autosave counter is held back, so a test neither meets a raid nor writes
/// into the Saves folder; both settings are put back after. Pawns of no
/// faction, wild animals mostly, are taken off the map before every tick.
/// </summary>
internal static class Clock
{
    private const int TicksPerFrame = 500;

    private static readonly FieldInfo TicksSinceSave =
        typeof(Autosaver).GetField("ticksSinceSave", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo CurTimeSpeed =
        typeof(TickManager).GetField("curTimeSpeed", BindingFlags.NonPublic | BindingFlags.Instance);

    /// <summary>Pauses the game. Sets the field, because the speed setter
    /// refuses while a new game fades in.</summary>
    public static void Stop() {
        if (CurTimeSpeed != null) {
            CurTimeSpeed.SetValue(Find.TickManager, TimeSpeed.Paused);
        }
        else {
            Find.TickManager.Pause();
        }
    }

    /// <summary>Runs <paramref name="ticks"/> ticks, or fewer if
    /// <paramref name="done"/> turns true first (checked between
    /// chunks).</summary>
    public static IEnumerator Run(int ticks, Func<bool> done = null) {
        TickManager tickManager = Find.TickManager;
        bool storyteller = DebugSettings.enableStoryteller;
        bool mentalStates = DebugSettings.enableRandomMentalStates;
        DebugSettings.enableStoryteller = false;
        DebugSettings.enableRandomMentalStates = false;
        Stop();
        try {
            int left = ticks;
            while (left > 0 && (done == null || !done())) {
                TicksSinceSave?.SetValue(Find.Autosaver, 0);
                int chunk = Math.Min(left, TicksPerFrame);
                for (int i = 0; i < chunk; i++) {
                    SendAwayWildlife();
                    tickManager.DoSingleTick();
                }
                left -= chunk;
                yield return null;
            }
        }
        finally {
            DebugSettings.enableStoryteller = storyteller;
            DebugSettings.enableRandomMentalStates = mentalStates;
        }
    }

    private static void SendAwayWildlife() {
        foreach (Map map in Find.Maps) {
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = pawns.Count - 1; i >= 0; i--) {
                if (i < pawns.Count && pawns[i].Faction == null) {
                    pawns[i].Destroy(DestroyMode.Vanish);
                }
            }
        }
    }
}
