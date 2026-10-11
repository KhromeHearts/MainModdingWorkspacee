# Visible Armors: design

Visible Armors draws equipped helmets, body armor, boots, cloaks and amulets on WorldBox units, the way
Terraria draws equipped armor on the player. This document covers how the game turns a unit into pixels,
the approaches I evaluated, the one I chose and why, how it is built, its failure modes, and what I measured.
Everything here was checked against the decompiled game (build 0.51.2-719) and in the running game.
Section 9 lists the decisions you may want to change.

---

## 1. How a unit gets from data to pixels

All of this was read in the decompiled source and confirmed at runtime with a sprite dump (`SpriteDump.cs`).

**Sprite sheets.** A civ unit has two kinds of sheets, both loaded through `SpriteTextureLoader` from
`actors/species/civs/<species>/`:

* Body sheets (`male_1`, `female_3`, `warrior_2`, `king`, `child`, `zombie`...). Each frame is a
  10x10 rect (11x13 or 14x15 for animal civs and special units) with its pivot at the feet. Next to each
  frame the sheet has two marker sprites, `walk_0_head` and `walk_0_item`. Their positions become
  `AnimationFrameData.pos_head_new` and `pos_item`
  (`ActorAnimationLoader.generateFrameData`). The animations are only `walk_0..3`, `swim_0..3` and
  `idle` (`= walk_0` for civs). Attacks have no frames: they are a lunge (`move_jump_offset`) plus the held item.
* Head sheets (`heads_male`, `heads_female`, `head_warrior`, `head_king`, `head_old_*`). Heads are tiny:
  2x2 or 3x3 pixels for humans and elves, 2x3 with a beard row for dwarves (pivot y = 1), and 4x4 to 6x6
  for animal civs. Every sprite faces **left**: the face pixel is on the left and the item is held at
  x = -3.5.

Real numbers from the dump: a human is a 2x2 head on a 4-pixel-wide torso that is 3 rows tall, then one
trouser row and two leg rows. The whole unit is about 8 pixels tall.

**Placeholder colours.** Body and head sheets are drawn in key colours that are swapped at runtime.
`#FF00FF..#580058` (5 shades) is kingdom colour 1, `#00EFEF..#007777` is kingdom colour 2, and
`#B8FF96, #00FF00, #00AF00, #4A831F` are the 4 skin (phenotype) shades. Everything else is drawn as is.
`PixelBag` (cached per sprite by `PixelBagManager`) sorts every pixel of a sprite into these groups.
Visible Armors reuses those groups as a free per-pixel body-part map: shirt, trousers, skin, hair or face.

**Compositing.** `Actor.calculateColoredSprite` runs whenever the animation frame, kingdom colour or head
changes. It calls `DynamicSpriteCreator.getSpriteUnit`, which builds a cache key from
(kingdom colour, head sprite, body sprite, phenotype, shade). On a miss, `createNewSpriteUnit` draws body
plus head with the colours swapped into a shared 1024x1024 atlas (`DynamicSpritesLibrary.units`,
`UnitSpriteConstructorAtlas`) and returns `Sprite.Create(...)`. Important facts:

* The unit atlas is **append-only and never cleared** for units. Only building atlases are reset
  (`BuildingManager`), and nothing calls `DynamicSpritesLibrary.clear()`. It grows by one 1024² page at a time.
* Atlas pixels are uploaded lazily: `setAtlasDirty`, then `MapBox.updateDynamicSprites` once per frame. A sprite
  created this frame is therefore only in the atlas's `pixels` array, not yet in the texture.
* The colour swap happens inside this step, so the composite depends on kingdom and skin, not only on the body frame.

**Per-frame render.** `ActorManager.calculateVisibleActors` runs every frame:

1. `precalculateRenderDataParallel` runs on worker threads (`Parallel.For`, batches of 256). It fills
   position, scale, rotation, colour and item data and calls `calculateMainSprite` (it picks the frame from
   time and state). If `isColoredSpriteNeedsCheck` is false, it reuses `getLastColoredSprite()`. Otherwise it
   leaves `main_sprite_colored[i] = null`.
2. `precalculateRenderDataNormal` runs on the main thread. For every null entry it calls `calculateColoredSprite`.
3. `QuantumSpriteLibrary.drawUnits` copies sprite, position, scale, rotation (euler) and colour into a pool
   of `SpriteRenderer`s (`QuantumSprite`, prefab `p_mapSprite`, sorting layer `Objects`,
   material `mat_world_object` = `Sprites/Default`).

**Direction and flipping.** No `flipX` is involved. `Actor.flip` animates `flip_angle` from 0 to 180 degrees around
**Y** at 600°/s, so a turn is a short squash. Lying, dead and unconscious units rotate around **Z**.
Death first darkens `color`, then fades its alpha. Size is the transform scale (`current_scale`), so babies,
giants and so on scale the same sprite.

**Sorting.** Measured at runtime: `GraphicsSettings.transparencySortMode = CustomAxis`,
axis `(0, 1, -1)`, camera at z = 0. Sprites sort by y minus z. Unit z is `position_height`.

**Held items** are separate pooled sprites (`drawUnitItems`) placed at `pos_item` and rotated around the unit
pivot. The game already draws weapons, so Visible Armors does not.

**UI portraits** (unit window, lists) do not use the in-world sprite. `ActorAvatarData.getColoredSprite`
calls `DynamicActorSpriteCreatorUI.getUnitSpriteForUI` with a data snapshot (its own cache key, same atlas).

**Equipment.** `ActorEquipment` has six fixed slots of `ActorEquipmentSlot`: Weapon, Helmet, Armor, Boots, Ring
and Amulet (`EquipmentType` has no other values). Items are `Item` objects whose `EquipmentAsset` carries `material`
(vanilla armor: leather, copper, bronze, silver, iron, steel, mythril, adamantine; accessories also bone),
`equipment_type`, `equipment_subtype`, `metallic` and an icon. Expanded Armory adds headbands
(helmet), cloaks (armor), highcleats (boots), earrings and bracelets, and the materials `gold` and `basic`
(for its ice and scale items). On death, `Actor.die` removes every item from the unit
(`takeAwayItems` / `destroyAllEquipment`).

**Vanilla already hints at armor.** Warriors use `warrior_N` body sheets with painted armor, and a warrior
with a helmet uses `head_warrior` (a grey helmet with a kingdom-coloured plume). Kings use `head_king`
(a crown). Visible Armors paints over these when the unit really wears gear.

**ActorAsset hooks.** `get_override_sprite` replaces the main body sprite for the whole species. It bypasses
the walk, idle and swim choice in `calculateMainSprite`. `get_override_avatar_frames` replaces UI frames.
Both are per species, not per unit, so they cannot express "this unit wears iron boots" without
reimplementing animation selection. Section 2.3 covers why I did not build on them.

---

## 2. Approaches evaluated

### 2.1 Compositing armor into the generated unit sprites (chosen)

Take the coloured sprite the game just made for this frame, paint the armor into a copy, and hand that copy
back. That is a postfix on `DynamicSpriteCreator.getSpriteUnit`.

* **Visuals:** the best possible. The armor is painted per frame against the real pixels, so it moves exactly
  with the body. It gets the right frame, flip, rotation, death fade, damage flash (which redraws
  `checkSpriteToRender`), magnet and status sprites for free. Sorting is identical to vanilla.
* **Per-frame cost:** no extra renderers or draw objects. Work happens only when a frame changes
  (a dictionary hit) or on a cache miss (paint a ~12x12 sprite).
* **Atlas risk:** the vanilla key includes kingdom and skin, so composites multiply by kingdoms × skins ×
  armor sets. Putting them into the game's never-cleared atlas would grow memory without bound.
  **Mitigation:** composites live in the mod's own atlas with a size limit and eviction (section 4.4).
* **Work:** medium. It needs an anatomy model of the tiny frames and art that adapts to them.

### 2.2 Overlay child sprites that follow the unit

Draw one extra `SpriteRenderer` (or quantum sprite) per armored unit, copying the unit's transform every frame.

* **Visuals:** the armor still has to match each walk frame (legs and arms move), so overlays need
  frame-shaped art, which means the same generation and caching as 2.1. Overlays can skip kingdom
  colour, so they need fewer variants. That is their one real advantage.
* **Sync:** they must copy position, scale, the Y-flip animation, the Z-rotation of lying units, the colour and
  death alpha, magnet rotation, shake and jump offsets. The damage flash (`mat_damaged` redraw of the unit
  sprite) would not include the armor unless it is also composited.
* **Sorting risk:** with the (0, 1, -1) custom axis, an overlay needs a tiny y/z offset to stay in front of its
  own unit without drawing over the unit standing just in front. Ties are order-undefined in Unity.
  In dense armies that shows as armor pieces floating over neighbours.
* **Cost:** one more renderer per armored visible unit (the game already pays one for held items), plus
  breaking sprite batching.
* **Work:** medium-high, with permanent sync maintenance whenever the game changes how units move.

### 2.3 Hybrids and other options

* **Swap the body sprite before the game colours it** (prefix on `getSpriteUnit` that passes an "armored
  body" sprite). The game would then colour and cache it, but in its own never-cleared atlas, keyed by a new
  body id per armor set. This is the unbounded growth problem with no way to evict. It also needs
  readable armored body textures for `PixelBag`. Rejected.
* **`ActorAsset.get_override_sprite` / per-combination sprite sets.** These are per species, so a per-unit
  look needs a per-call lookup and a reimplementation of animation selection. It also means sheets for every
  armor combination (8 materials³ × frames × species) or generating them, with the same cache problem.
  Rejected.
* **Overlay, but composited into one sprite per (body frame, head, armor set)**, kingdom-independent. It
  saves memory in worlds with many kingdoms but keeps all of 2.2's sync and sorting risks. Rejected:
  memory turned out to be small (section 6).
* What I built is a hybrid of sorts. It composites (2.1) but uses its own bounded atlas instead of the game's.

### 2.4 Decision

**Compositing (2.1) with a private, size-limited atlas.** It is the only option where armor can never
desync, never mis-sort and needs no per-frame transform copying. Its main risk, memory, is fixed by
owning the atlas. Measured costs are in section 6.

---

## 3. Art: how pieces fit 8-pixel units

At this scale, hand-drawn per-frame sprites would not survive the variety of heads, species and frames.
The art is therefore made of three things:

1. **Material palettes** (`materials/<id>.png`, 8x1): outline, shadow, dark, mid, light, highlight, accent,
   dark accent. The hues follow the game's own item icons (dumped in game): leather reddish brown, copper peach,
   bronze olive and gold, silver mint, iron dark grey, steel lavender grey with gold trim, mythril navy to cyan,
   adamantine charcoal with a lava-orange glow. Extra palettes: gold, bone, ice, scale, wood, stone.
2. **9-slice templates** for helmets (`helmets/<style>.png`, 5x5) and torso pieces (`armor/<style>.png`, 5x4).
   The middle column and middle row stretch to the size of each head or torso. The outer columns are a
   1-pixel margin in front of and behind the piece, and the top rows are margins above it. Grey pixels pick
   a palette shade, and fixed colours pick roles (accent, kingdom colour, keep, visor). Opaque pixels may add
   pixels (silhouette); semi-transparent pixels only recolour existing ones (sleeves). One template
   therefore fits 2x2, 3x3 and 2x3 heads and every frame.
3. **Procedural boots, capes and amulets.** Feet move too much between frames for a template. Boots
   recolour the feet (bare skin near the bottom, the outer pixels of the lowest row) and the shaft above them
   up to the style height. A cape is a strip behind the back from the shoulders to just above the ground,
   flaring at the bottom, only on empty pixels. An amulet is a pendant pixel plus a chain pixel on the chest.

Default looks per material (in `appearances.json`):

| Material | Helmet | Body | Boots |
|---|---|---|---|
| leather | cap with brim | vest with belt | shoes |
| copper | dome helm | mail | greaves |
| bronze | plumed (kingdom colour) | scale | greaves |
| silver | winged | breastplate | greaves |
| iron | nasal with neck guard | breastplate | sabatons |
| steel | great helm (dark visor) | plate with pauldrons | greaves |
| mythril | crested | plate | greaves |
| adamantine | horned (glowing tips) | ornate plate + kingdom cape | greaves |
| Expanded Armory | headband / cloak + cape / tall boots by keyword | | |

Each material is told apart by colour first and by helmet silhouette second. Every helmet except the
leather cap adds at least one row above the head (dome, plume, crest, wings or horns). Plate adds pauldrons
beside the neck, and cloaks and ornate plate add a cape behind the body.

**Fallbacks for unknown items.** The resolution order is: API registration, then the `items` table, then
keyword rules, then the material's palette and style. If the material has no palette, a palette is built
from the item's own icon (pixels sorted by brightness, the most saturated pixel as accent). If that fails, a
stable hue is derived from the material name. If a style is missing, the `default` style is used. Every
item that has a slot therefore gets a look; nothing crashes or turns invisible. Verified with ShinobiBox's
`armor_madara_sixpaths_cloak`, `armor_shinobi_flak_jacket` and `armor_warring_states` (material "basic").

---

## 4. Implementation

```
Code/
  VisibleArmorsMod.cs        NML entry (BasicMod), per-class patch bootstrap, runtime object (upload, stats)
  Core/                      VAConfig (NML callbacks, version counter), VALog (rate-limited guard logging)
  Appearance/                ArmorSlot (public enum), MaterialPalette, PieceTemplate (9-slice + colour roles),
                             ArtLibrary (PNG loading), AppearanceRules (appearances.json),
                             AppearanceRegistry (item -> look id, fallbacks, interning)
  Render/                    Signature, SpeciesProfiles, Anatomy (+ PixelLabel), ArmorPainter,
                             ArmorAtlas (bounded pages), Compositor (cache + compose), RenderSync (per-frame check)
  Patches/                   SpriteUnitPatch, RenderPassPatch, AvatarPatch (one file each, each says why it exists)
  Api/                       VisibleArmorsApi
  Debug/                     SpriteDump (dump_sprites.flag), SelfTest (selftest.flag)
```

### 4.1 Signature

`Signature.Compute(actor)` packs the look id of the visible slots into one `long`: 16 bits each for helmet,
armor, boots and amulet. 0 means "vanilla sprite". It is derived from the live equipment every time, so
**nothing is stored per unit and nothing goes into saves.** It is 0 in these cases: the mod is off, the
unit is dead, an egg or a baby, the species profile is `none`, or the camera is past the zoom limit. The helmet
part is 0 for kings while `keep_king_crown` is on. Identical looks share an id (interned by slot, style and
palette), so 300 steel helmets are one look.

### 4.2 Patches (3, none of them skips the original)

1. **Postfix `DynamicSpriteCreator.getSpriteUnit`.** If the signature is non-zero, the result becomes
   `Compositor.GetOrCreate(vanilla, ...)`. It only runs on the main thread (checked), and only for the
   Units atlas, which excludes boats.
2. **Prefix and postfix on `ActorManager.precalculateRenderDataNormal`** (private, called once per frame on
   the main thread). The prefix (`RenderSync`) checks every visible unit. If the sprite it is about to draw
   was built for a different signature, it nulls the unit's `_last_main_sprite` (through a Harmony field ref)
   and the render entry, so the game asks for a new sprite **in the same frame**. This is how equipment
   changes from anywhere (unit window, AI, other mods, death) show up immediately without patching a single
   equipment method. The postfix uploads new atlas pixels before the frame is drawn.
3. **Postfix `ActorAvatarData.getColoredSprite`.** Unit-window and list portraits get the same armor (it
   looks the unit up by id).

Each patch class is applied on its own, and every patch body is wrapped in try/catch with rate-limited
logging. A bad unit or item falls back to the vanilla sprite and cannot break rendering for others.

### 4.3 Composing one sprite

1. Read the vanilla composite's pixels. They come from the game atlas's `pixels` buffer if it is on the page
   still being filled (not uploaded yet), otherwise from `GetPixels` on the older page.
2. Label every pixel from the body and head `PixelBag`s (shirt, trousers, kingdom 2, skin, other, face,
   hair). The head offset is recomputed exactly the way `createNewSpriteUnit` places it.
3. Anatomy: the neck comes from `pos_head_new`. The torso core is the run of columns under the head with at
   least two body pixels in the 4 rows below the neck. The torso top is the highest body row in the core,
   the torso is 3 rows, and the legs are what remains below. Arms are the pixels beside the core, and
   anything more than one pixel away (the hands) is left alone.
4. Paint torso, boots, cape, amulet, then helmet. Rules: torso paint never covers the head; the helmet
   never covers the body and keeps the face (skin) unless a template pixel is a visor. On furry animal heads
   only the lower half counts as face. Swim frames only recolour clothes and skin (never the water splash
   pixels) and skip boots and capes.
5. Crop to the opaque pixels (the canvas has 2 px of room on each side and 2 px above for horns, plumes and
   capes) and keep the pivot where the game had it. Then copy into the mod's atlas and `Sprite.Create`
   (FullRect, 1 px per unit, like the game).

### 4.4 Cache and memory

* Key: (vanilla sprite instance id, signature) → composite. A reverse map (composite id → signature) lets
  `RenderSync` tell what a drawn sprite was built for. Failed compositions (nothing to paint, for example
  only an amulet on a head-only species) are remembered, so they are not retried every frame.
* Atlas: 512x512 RGBA pages without mipmaps (1 MB each) and 1 px padding. When there are more pages than
  "Sprite cache size" (default 8 = 8 MB), the oldest page is evicted at the start of the next render pass:
  its sprites and texture are destroyed, every unit still pointing at one of them is reset (also off-screen
  units, so nobody keeps a destroyed sprite), and visible units rebuild in the same frame. A busy sprite
  simply moves to a fresh page. Finished pages are uploaded once more and marked non-readable, which frees
  their CPU copy.
* Config changes and API registrations flush the whole cache, lazily, at the next render pass.
* Why no mipmaps: the game's atlas has them, but regenerating them on every upload made the atlas cost about
  3× higher (0.65 ms vs 0.2 ms per upload, and 2–4 ms more per frame in the stress test). Side-by-side
  screenshots at camera size 35 and 60 looked the same.

---

## 5. Failure modes and how each is handled

| Risk | Handling | Status |
|---|---|---|
| Atlas overflow / memory growth | Own atlas, page limit, FIFO eviction, CPU copies freed. The game's atlas is never touched. | Tested: eviction with a 2-page limit while ~700 armored units walked, 0 wrong sprites |
| Thousands of units | Per frame: one pass over *visible* units (field reads + 2 dictionary lookups). No allocations on the hot path. | Measured (section 6) |
| Frame desync | Impossible by construction: the armor is part of the frame sprite | Tested: walking, turning and fighting units, 0 mismatches |
| Flipped / rotated / lying units | Same sprite, same transform as vanilla | Visually checked while walking and turning |
| Dead units | The game removes items on death → signature 0 → vanilla; there is also an explicit `isAlive` check | Tested |
| Burning units | Fire is a separate effect sprite; the unit sprite (with armor) is unchanged | Not specifically tested |
| Units in boats | Hidden while inside (vanilla); boats use their own atlas and are skipped | By construction, not tested |
| Babies / eggs | No armor drawn (children do not normally wear gear, and child frames are tiny) | By design |
| Elderly ("wise" heads) | The head sprite changes and the helmet adapts to it | Not specifically tested |
| Kings | Keep their crown; the helmet is hidden (`keep_king_crown`) | By design, not tested in game |
| Warriors with painted vanilla armor | Torso is repainted with the real armor; without armor items the vanilla look stays | Visible in grid tests |
| Species and sizes | `human`, `elf`, `orc`, `dwarf`, `zombie` = humanoid. Everything else = `auto`: humanoid if the anatomy check passes on a full-body frame, otherwise helmet only. Transform scale handles giants and tiny units. | Tested 12 other species (animal civs, bandit, necromancer, alien, skeleton, demon, white mage) |
| Swimming | Only clothes and skin are recoloured; no boots or cape | Covered by the composite export |
| Unit window | Own hook, same cache | Tested |
| Save/load | Nothing stored; derived on load | Tested 3 times: saved to a temp folder, reloaded through the game's loader. Every reloaded unit drew exactly the gear it wore (0 mismatches). Clean world: 26/26 units kept identical gear. With ShinobiBox items the game itself cannot generate in the world, one unit's amulet came back as a different amulet item (a game-side item-identity issue, not investigated further; the drawing still matched) |
| Other mods replacing unit sprites | The postfix runs on whatever sprite they return; species with override sprites and no head get no armor (head-only and nothing to paint) | By construction |
| A bad item or unit | try/catch per patch, rate-limited logging, failure cached → vanilla sprite | By construction |
| Game update renames a method | Patch class fails alone and is logged; the game renders vanilla | By construction |

---

## 6. Measurements

On this machine, with the user's full mod list enabled, in a 256x256 generated world, using the
in-game self test (`debug/selftest`, see README):

* **Stress test:** about 800 humans with random armor, walking, camera size 30, about 780 units visible and
  600–730 of them armored. Three alternating 3-second windows with the mod on and off:
  **13.8 ms/frame on vs 13.2 off (+0.3 to +0.6 ms; window-to-window noise is about ±1 ms).** A second run of
  the same test measured 15.6 on vs 15.7 off, which is within the noise. Measured parts,
  per frame: visible-unit check 0.35 ms, `getSpriteUnit` postfix 0.16 ms (~140 calls, ~1.2 µs each),
  composing new sprites 0.01–0.03 ms, atlas upload 0.05–0.10 ms. One composite costs 0.012–0.018 ms.
* With mipmaps on the pages, the difference was 1.9–3.7 ms per frame, which is why they are off.
* **Memory:** the stress test built ~2,300 composites, which is 2 pages (~2 MB). The default limit is 8 MB, and
  the game's own unit atlas is unaffected.
* The texture pages are separate from the game's unit atlas, so armored and unarmored units do not batch
  together. That is part of the hidden cost included in the frame times above.

---

## 7. Configuration

All options are NML config switches (see README): master switch, one switch per piece (helmet, body,
boots, capes, amulet), unit window portraits, other species, zoom limit, cache size, debug logging and
statistics. Every change bumps a version counter. The next frame flushes and rebuilds lazily, so there is
no restart.

## 8. Public API

`VisibleArmors.Api.VisibleArmorsApi` (API level 1): `RegisterPalette`, `RegisterTemplate`,
`RegisterBootsStyle`, `SetItemLook`, `SetSpeciesProfile`, `Refresh`, `RefreshAll`, `IsArmorDrawn`. Items
from PossessLib or SurvivalBox that are real `EquipmentAsset`s already work with no code, and the API is for
giving them a specific look. Documented in the README with an example.

## 9. Decisions you may want to change

1. **Kings keep their crown** instead of showing their helmet. Set `"keep_king_crown": false` in
   `appearances.json`.
2. **Armor disappears on death.** The game itself removes the items when a unit dies, so this follows the data.
   Showing armor on corpses would need a timed per-unit memory, which goes against "store nothing".
3. **Babies never show armor.**
4. **Animal civs and special units** default to `auto`. They work, but the large heads get boxy helmets.
   Switch "Other species" off, or set them to `head_only` or `none` in `appearances.json`, if you prefer.
5. **Rings are not drawn** (one pixel on a hand would read as noise). Amulets are one pendant pixel.
6. **The style per material** (for example steel = great helm, adamantine = horns) is a taste call, made in
   `appearances.json`.
7. **The zoom limit defaults to 80** (camera size). Past about 80 the game hides units anyway. Lower it if
   you want vanilla sprites at mid zoom.
