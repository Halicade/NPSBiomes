# NPS Biomes tests

These tests run inside RimWorld 1.7 under SmashPhil's
[DevTools](https://github.com/SmashPhil/DevTools). `LoadFolders.xml` loads the
`Tests` folder only while DevTools is active, so a game without DevTools never
loads them.

## Building

```
dotnet build Source/NPSBiomes.Tests/NPSBiomes.Tests.csproj -c Release
```

That builds the mod's own project first, into `1.7/Assemblies/NPSBiomes.dll`,
so the tests run against the current source. Then it writes
`NPSBiomes.Tests.dll` to `Tests/Assemblies/`. Git tracks the first and does
not ignore the second, so after a build both show up in `git status`.

The mod's project finds the game's assemblies four folders up, which holds
with the repo in the game's `Mods` folder on Windows, and that is the layout
this build needs: anywhere else the mod's project fails before the tests
build. DevTools is expected beside this mod in `Mods/`; pass
`-p:DevToolsDll=` with the path to `DevTools.dll` if it lives somewhere else.

## Running

With DevTools and NPS Biomes both active, start the game with these arguments:

```
--pid hali.npsbiomes --test -e
```

`--test` runs this mod's tests and `-e` quits the game when they finish.
DevTools writes the results to `Test.log` in the game's data folder (on macOS,
`~/Library/Application Support/ludeon.rimworld/`). Every check writes its own
PASS or FAIL line there, so a failing test lists each case it found rather
than stopping at the first.

DevTools fails a test that logs any warning, from any mod. RimWorld 1.7 logs a
"Stub!" warning the first time code calls a Unity method the engine doesn't
implement, so each fixture allows that one warning by name
(`AllowedWarnings.cs`). A bigger mod list may need more patterns there.

## What each fixture checks

The results quoted below are from commit `8a17117` on `Main`, where each of
these tests fails on the problem it was written for.

**LoadFolderTests** reads `LoadFolders.xml` and checks that every folder it
names exists on disk with exactly that spelling. Windows and macOS ignore case
when they look up a folder and Linux does not, so `1.7/Mods/MegaFauna` loads on
Windows and is skipped on Linux, where the folder is `Megafauna`. Six entries
fail: `MegaFauna` and `WaywardLotus` under both versions, and both
`DLC/Biotech` folders, which do not exist.

**SettingsTests** saves and loads a settings file of its own in the temp
folder, and puts every setting back afterwards. The first test moves each
setting off its default in turn, then saves and loads. The game writes a
setting only when it differs from its default, so a bad key stays hidden until
a player changes that one setting; then the save fails as a whole.
`SavannaMinTemp` and `TallgrassMinTemp` fail, because their keys end in a
space and XML does not allow one in an element name. The second test compares
the value each field starts at with the value loading falls back to:
`TallgrassMaxTemp` starts at 30 and loads as 22. The Reset buttons in the
settings window hold a third copy of each default, which no test reaches.

By the time a test runs, the game has loaded the player's settings over the
fields, so this test and `BiomeTests` find each setting's starting value by
reading the field initializers out of the compiled `BiomeSettings` class
(`SettingsFile.StartingValues`). That reader is a stopgap. Once each default
lives in one named constant that the field, the settings file and the Reset
button all use, which is also the fix for `TallgrassMaxTemp`, the tests can
read those constants and the reader can be deleted.

**BiomeTests** picks a biome for the same tile twice: once from the official
biomes, scored as the game scores them without this mod, and once from the
official biomes plus this mod's, scored with it loaded. The tiles are a fixed
grid of climates plus the land tiles of the world DevTools generates. Each
test starts from the settings a new game has before any settings file is read,
so the results below hold whatever the settings file on the machine says.

- Where the two picks differ, the tile has to go to one of this mod's biomes.
  The temperate forest and arid shrubland patches set vanilla's score to zero
  in bands where none of this mod's biomes takes over, so the tile falls to a
  different vanilla biome. Temperate forest becomes boreal forest on 611 grid
  points between 5 and 22 °C. The test world is new each run, so its count
  changes, but it shows the same swap in the same band.
- Odyssey's glowforest keeps the grid points vanilla gives it. Savanna,
  redwood forest and tallgrass prairie take 446 of them. The other three rare
  Odyssey biomes read noise at the tile's place on the planet, and the test
  world covers 5% of the planet, too few of their tiles to fail on.
- The redwood forest's "max rainfall" setting acts as a maximum. A 0 °C tile
  with 1500 mm of rain should score 0 under a maximum of 1100 and above 0
  under 2000. The worker treats it as a minimum, so both checks fail.

**LavaTests** runs the lava map component's per-cell check once on a single
cell, with the random state seeded so its 1-in-100,000 roll comes up. The
check is the private `LavaTile.LavaCellChecks`, called by reflection; if it is
renamed, the tests fail with a line saying so. The check adds a vent only
while the map has fewer than 10, so the fixture first removes every vent on
the test map. A steam vent must not appear
under a wall or a stool, and basalt forming on lava must not remove an item
lying there. Two more tests run the same check on bare basalt and bare lava
and expect a vent and a rock, which shows the first ones pass because the
check refuses and not because it never fires. The vent appears under both
buildings, and the 10 steel on the lava is gone.

**SaltTests** has a colonist salt a bleeding cut by targeting the patient with
the salt, as a player's click does, then runs the game until the cut is
salted. There are two salt on the map before and there should be one after;
both are still there. The second test reads the salt comp's `Props`, which
casts to the resurrector serum's properties class and throws.
