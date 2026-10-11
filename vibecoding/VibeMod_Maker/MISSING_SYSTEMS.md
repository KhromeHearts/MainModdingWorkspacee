# Systems Requiring Additional Class Files

If you need to implement any of these, **ask the user to add the specific file from `/Game_code/` before continuing**. Do not guess internal methods or data structures for these systems.

---

| System | File | When needed |
|---|---|---|
| Clan traits | `ClanTrait.txt`, `ClanTraitLibrary.txt` | Custom clan traits (different structure from `ActorTrait`) |
| Clan management | `ClanManager.txt`, `ClanData.txt` | Modifying clan logic, custom clan creation |
| Culture traits | `CultureTrait.txt`, `CultureTraitGroupLibrary.txt` | Custom culture traits (different from `ActorTrait`) |
| Culture management | `CultureManager.txt`, `CultureData.txt` | Modifying culture logic, custom culture system |
| AI tasks | `ActorTaskLibrary.txt` | Custom actor task trees |
| AI jobs | `ActorJobLibrary.txt`, `CitizenJobAsset.txt`, `CitizenJobLibrary.txt` | Custom citizen jobs |
| Biomes | `BiomeAsset.txt` | Custom biome definitions |
| Build orders | `BuildOrder.txt`, `BuildOrderLibrary.txt` | Custom building construction sequences |
| Books | `BookManager.txt`, `BookTypeAsset.txt` | Custom book/item types |
| Architecture | `ArchitectureAsset.txt` | Custom architecture styles |
| Building AI | `BuildingManager.txt` | Building placement/management internals (stub only — 8 lines) |
| Equipment | `ActorEquipment.txt` | Equipment slot system (stub only — 15 lines) |

---

> **Note:** Many of these systems follow the standard `AssetLibrary<T>` pattern documented in `GAME_SYSTEMS_INDEX.md`. The basic registration pattern (`AssetManager.<library>.add(asset)`) works the same way — you only need the class file to see the specific fields and methods available.
