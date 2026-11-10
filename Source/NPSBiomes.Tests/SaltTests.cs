using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using DevTools.Testing;
using RimWorld;
using Verse;

namespace NPSBiomes.Tests;

/// <summary>
/// Salt applied to a wounded colonist, through the same calls the game makes
/// when the player orders it.
/// </summary>
[TestFixture(TestType.Playing)]
[WarningsAllowed(AllowedWarnings.EngineStub)]
internal class SaltTests
{
    private static readonly CellRect Area = new CellRect(20, 20, 14, 8);

    /// <summary>Before every attempt, so a retry does not count the salt an
    /// earlier attempt left.</summary>
    [SetUp]
    private void ClearBefore() => Ground.Clear(Ground.Map, Area);

    [OneTimeTearDown]
    private void ClearAfter() => Ground.Clear(Ground.Map, Area);

    /// <summary>
    /// One colonist puts salt on another's bleeding cut: the cut is salted,
    /// and one piece of salt is used up.
    /// </summary>
    [Test]
    private IEnumerator SaltIsUsedUp() {
        Map map = Ground.Map;
        Clock.Stop();
        ThingDef saltDef = Ground.Def("TKKN_Salt");
        Pawn user = Ground.SpawnColonist(map, new IntVec3(22, 0, 24));
        Pawn patient = Ground.SpawnColonist(map, new IntVec3(30, 0, 24));
        try {
            // Drafted, so the patient stays where they are.
            patient.drafter.Drafted = true;
            BodyPartRecord torso = patient.RaceProps.body.AllParts.First(p => p.def == BodyPartDefOf.Torso);
            var cut = (Hediff_Injury)HediffMaker.MakeHediff(HediffDefOf.Cut, patient, torso);
            cut.Severity = 4f;
            patient.health.AddHediff(cut, torso);
            Check.That(cut.Bleeding, "the patient's cut bleeds");

            Thing salt = ThingMaker.MakeThing(saltDef);
            salt.stackCount = 2;
            GenSpawn.Spawn(salt, new IntVec3(24, 0, 24), map);
            Check.That(SaltOnMap(map, saltDef) == 2, $"two salt on the map before ({SaltOnMap(map, saltDef)})");

            // "Apply salt on someone" from the user's menu, then a click on
            // the patient: the salt's use job, which hands over to the
            // salt-wounds job.
            CompTargetable targetable = salt.TryGetComp<CompTargetable>();
            targetable.SelectedUseOption(user);
            targetable.OrderForceTarget(patient);
            Find.Targeter.StopTargeting();
            Check.Note($"the user's job after the click: {user.CurJobDef?.defName ?? "none"}");
            yield return Clock.Run(5000, () => Salted(patient));
            yield return Clock.Run(60);

            Check.That(Salted(patient), "the patient's cut is salted");
            int left = SaltOnMap(map, saltDef);
            Check.That(left == 1, $"one salt is used up: {left} left on the map, counting any carried or in an inventory");
        }
        finally {
            Ground.Remove(new[] { user, patient });
        }
    }

    /// <summary>The salt-wounds comp can read its own properties.</summary>
    [Test]
    private void SaltCompReadsItsProps() {
        Thing salt = ThingMaker.MakeThing(Ground.Def("TKKN_Salt"));
        ThingComp comp = salt.TryGetComp<CompTargetEffect_SaltWounds>();
        Check.That(comp != null, "salt carries the salt-wounds comp");
        PropertyInfo props = comp?.GetType().GetProperty("Props", BindingFlags.Public | BindingFlags.Instance);
        if (props == null) {
            Check.Note("the comp has no Props property to read");
            return;
        }
        Exception thrown = null;
        try {
            props.GetValue(comp);
        }
        catch (TargetInvocationException e) {
            thrown = e.InnerException;
        }
        Check.That(thrown == null, thrown == null ? "reading Props works" : $"reading Props threw {thrown.GetType().Name}: {thrown.Message}");
    }

    private static bool Salted(Pawn pawn) => pawn.health.hediffSet.hediffs.Any(h => h.def.defName == "NPS_SaltedWound");

    private static int SaltOnMap(Map map, ThingDef salt) {
        int loose = map.listerThings.ThingsOfDef(salt).Sum(t => t.stackCount);
        int held = map.mapPawns.AllPawnsSpawned.Sum(p =>
            (p.carryTracker?.CarriedThing?.def == salt ? p.carryTracker.CarriedThing.stackCount : 0) +
            (p.inventory?.innerContainer.Where(t => t.def == salt).Sum(t => t.stackCount) ?? 0));
        return loose + held;
    }
}
