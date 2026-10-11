using System;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.General;
using ReflectionUtility;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System.Threading.Tasks;
using System.Reflection;
using ai;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.CanvasScaler;
using NeoModLoader.constants;

namespace ShinobiBox
{
    public static class JutsuLibrary
    {
        private const float ChaosAgeDurationSeconds = 60f;
        private static readonly Dictionary<long, Kingdom> MadaraTamedOriginalKingdoms = new Dictionary<long, Kingdom>();
        private static bool _chaosRevertPending;
        private static float _chaosRevertTimer;
        private const float SoundSpacing = 0.3f;
        private static float _nextSoundTime;
        #region Sharingan Genjutsu 1-3
        // Genjutsu 1
        public static bool Sharingan1Action(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            if (pTarget.a.hasStatus("genjutsu_1")) return false;

            pTarget.a.addStatusEffect("genjutsu_1", 3f);
            return true;
        }

        // Genjutsu 2
        public static bool Sharingan2Action(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            if (pTarget.a.hasStatus("genjutsu_2")) return false;

            pTarget.a.addStatusEffect("genjutsu_2", 4f);
            return true;
        }

        // Genjutsu 3
        public static bool Sharingan3Action(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            if (pTarget.a.hasStatus("genjutsu_3")) return false;

            pTarget.a.addStatusEffect("genjutsu_3", 5f);
            return true;
        }
        #endregion

        #region Sharingan Progression
        public static bool SharinganProgression(BaseSimObject pSelf, WorldTile pTile = null)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

            Actor actor = pSelf.a;

            if (actor.hasTrait("mangekyo_sharingan") || actor.hasTrait("eternal_mangekyo") || actor.hasTrait("madara_eternal_mangekyo") || actor.hasTrait("trait_blind"))
            {
                ClearSharinganTomoeStatuses(actor);
                return false;
            }

            if (!actor.has_attack_target || actor.attack_target == null || actor.attack_target.a == null || !actor.attack_target.a.isAlive()) return false;

            BaseSimObject attackTarget = actor.attack_target;
            WorldTile targetTile = attackTarget.current_tile;

            int kills = actor.data.kills;
            int age = actor.data.getAge();

            if (kills >= 7 || (age >= 38 && UnityEngine.Random.value < 0.001f))
            {
                actor.addStatusEffect("status_sharingan_3t", 30f);
                actor.finishStatusEffect("status_sharingan_2t");
                actor.finishStatusEffect("status_sharingan_1t");
                return true;
            }

            if (kills >= 4 || (age >= 24 && UnityEngine.Random.value < 0.001f))
            {
                actor.addStatusEffect("status_sharingan_2t", 30f);
                actor.finishStatusEffect("status_sharingan_3t");
                actor.finishStatusEffect("status_sharingan_1t");
                return true;
            }

            if (kills >= 1 || (age >= 12 && UnityEngine.Random.value < 0.008f))
            {
                actor.addStatusEffect("status_sharingan_1t", 30f);
                actor.finishStatusEffect("status_sharingan_3t");
                actor.finishStatusEffect("status_sharingan_2t");
                return true;
            }

            return false;
        }

        public static void ClearSharinganTomoeStatuses(Actor actor)
        {
            if (actor == null) return;

            actor.finishStatusEffect("status_sharingan_1t");
            actor.finishStatusEffect("status_sharingan_2t");
            actor.finishStatusEffect("status_sharingan_3t");
        }
        #endregion

        #region Rasengan
        public static bool RasenganAction(BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
            if (pTarget.a.hasStatus("status_rasengan")) return false;
            if (pTarget.a.has_attack_target)
            {
                pTarget.a.addStatusEffect("status_rasengan", 5f);
                return true;
            }
            return false;
        }
        #endregion

        #region Chidori
        public static bool ChidoriAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

            pSelf.a.addStatusEffect("status_chidori", 4f);

            if (pTarget.current_tile != null)
            {
                pTarget.a.getHit(110f, true, AttackType.Other, pSelf.a, true);

                if (pSelf.a.hasStatus("status_chidori"))
                {
                    MapBox.spawnLightningSmall(pTarget.current_tile, 0.25f, pTarget.a);
                    pTarget.a.getHit(80f, true, AttackType.Other, pSelf.a, true);
                }
            }

            return true;
        }
        #endregion

        #region Rasenshuriken
        public static bool RasenshurikenAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
            bool hasAttackTarget = pSelf.a.has_attack_target;

            if (pTarget.current_tile != null && pSelf.current_tile != null && hasAttackTarget)
            {
                World.world.projectiles.spawn(pSelf, pTarget, "projectile_rasenshuriken", pSelf.current_tile.posV3, pTarget.current_tile.posV3);
                return true;
            }
            return false;
        }
        #endregion

        #region Rinne-Sharingan Jutsus

        #region Rinne-Sharingan: Infinite Tsukuyomi
        public static bool InfiniteTsukuyomiAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            if (!pSelf.a.hasTrait("yin_release") && !pSelf.a.hasTrait("yinyang_release")) return false;

            int used = 0;
            pSelf.a.data.get("it_used_once", out used);
            if (used > 0) return false;

            int affectedCount = 0;
            foreach (Actor unit in MapBox.instance.units)
            {
                if (unit == null || !unit.isAlive()) continue;
                unit.addStatusEffect("status_infinite_tsukuyomi", 60f);
                unit.addStatusEffect("sleeping", 60f);
                unit.addStatusEffect("had_good_dream", 60f);
                affectedCount++;
            }

            // 0.1% max chakra bonus per living unit affected
            float bonusMultiplier = affectedCount * 0.001f;
            pSelf.a.data.set("it_bonus_multiplier", bonusMultiplier);

            float max = ChakraSystem.RefreshMax(pSelf.a);
            ChakraSystem.SetCurrent(pSelf.a, max);

            pSelf.a.restoreHealth(pSelf.a.getMaxHealth());
            pSelf.a.finishStatusEffect("sleeping");

            SetWorldAgeChaos();
            pSelf.a.data.set("it_used_once", 1);

            ShinobiWorldLogs.AddWorldLog("log_infinite_tsukuyomi", "worldlog_infinite_tsukuyomi", "ui/icons/rinne_sharingan", pSelf.a);

            return true;
        }
        #endregion

        #region Rinne-Sharingan: Heavenly Ice Chamber
        public static bool HeavenlyIceChamberAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

            if (pSelf.current_tile == null) return false;

            for (int i = 0; i < 40; ++i)
            {
                World.world.loopWithBrush(pSelf.current_tile, Brush.get(16, "circ_"), new PowerActionWithID(PowerLibrary.drawTemperatureMinus), "temperatureMinus");
            }

            pSelf.a.finishStatusEffect("frozen");
            return true;
        }
        #endregion
        #endregion

        #region Rinnegan Abilities

        #region Rinnegan Meteor
        public static bool RinneganMeteorAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            if (pTarget.current_tile != null)
            {
                Meteorite.spawnMeteorite(pTarget.current_tile, pSelf.a);

                ShinobiWorldLogs.AddWorldLog("log_rinnegan_meteor", "worldlog_rinnegan_meteor", "ui/icons/rinnegan", pSelf.a);
                return true;
            }
            return false;
        }
        #endregion

        #region Rinnegan: Naraka Path (King of Hell)
        public static bool NarakaPathHealAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || !pSelf.a.isAlive()) return false;

            if (pSelf.a.data.health >= pSelf.a.getMaxHealth() * 0.3f) return false;

            if (pSelf.a.hasStatus("koh_sprite")) return false;

            pSelf.a.addStatusEffect("frozen", 4f);
            pSelf.a.addStatusEffect("koh_sprite", 4f);
            pSelf.a.addStatusEffect("powerup", 25f);
            int healAmount = (int)(pSelf.a.getMaxHealth() * 0.40f) + (pSelf.a.data.kills * 5);
            pSelf.a.restoreHealth(healAmount);

            if (ShinobiConfig.EnableWorldTips)
            {
                ShinobiWorldLogs.AddWorldLog("log_naraka_path", "worldlog_naraka_path", "ui/icons/rinnegan", pSelf.a);
            }

            return true;
        }
        #endregion

        #region Rinnegan: Six Paths Technique

        public static bool SixPathsTechnique(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            float roll = UnityEngine.Random.value;
            if (roll < 0.10f)
            {
                return AsuraPathAttack(pSelf, pTarget);
            }
            if (roll < 0.35f)
            {
                return HumanPathAttack(pSelf, pTarget);
            }
            return AnimalPathSummon(pSelf, pTarget);
        }

        // Deva Path: Shinra Tensei (Almighty Push)
        public static bool DevaShinraTensei(BaseSimObject pSelf, BaseSimObject pAttackedBy, WorldTile pTile = null)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            if (pSelf.current_tile == null) return false;

            WorldTile forceTile = pSelf.current_tile;
            if (pAttackedBy != null && pAttackedBy.a != null && pAttackedBy.a.isAlive() && pAttackedBy.current_tile != null)
            {
                forceTile = pAttackedBy.current_tile;
            }
            else if (pTile != null)
            {
                forceTile = pTile;
            }

            EffectsLibrary.spawnExplosionWave(forceTile.posV3, 2.2f, 1.2f);
            World.world.applyForceOnTile(forceTile, 10, 2.6f, true, 0);

            return true;
        }

        private static bool AsuraPathAttack(BaseSimObject pSelf, BaseSimObject pTarget)
        {
            if (pTarget.current_tile == null || pSelf.current_tile == null) return false;

            ActionLibrary.throwBombAtTile(pSelf, pTarget.current_tile);
            pTarget.a.getHit(75f, true, AttackType.Fire, pSelf.a, true);
            return true;
        }

        private static bool HumanPathAttack(BaseSimObject pSelf, BaseSimObject pTarget)
        {
            pTarget.a.getHit(250f, true, AttackType.Other, pSelf.a, true);

            if (UnityEngine.Random.value < 0.95f && !pSelf.a.hasTrait("strong_minded"))
            {
                pSelf.a.restoreHealth((int)(pSelf.a.getMaxHealth() * 0.10f));
            }

            if (UnityEngine.Random.value < 0.40f)
            {
                pSelf.a.stats["intelligence"] += 1f;
            }
            else if (UnityEngine.Random.value < 0.02f)
            {
                pSelf.a.stats["intelligence"] += 3f;
            }

            return true;
        }

        private static bool AnimalPathSummon(BaseSimObject pSelf, BaseSimObject pTarget)
        {
            if (pSelf.current_tile == null) return false;

            string[] summonPool = { "wolf", "bear", "snake", "crocodile" };
            int summonCount = UnityEngine.Random.Range(1, 3);
            int successCount = 0;

            for (int i = 0; i < summonCount; i++)
            {
                string unitId = summonPool[UnityEngine.Random.Range(0, summonPool.Length)];
                WorldTile spawnTile = Toolbox.getRandomTileWithinDistance(pSelf.current_tile, 2) ?? pSelf.current_tile;
                Actor summon = World.world.units.createNewUnit(unitId, spawnTile, true, 1f);
                if (summon == null) continue;

                if (pSelf.a.kingdom != null)
                {
                    summon.setKingdom(pSelf.a.kingdom);
                }
                if (pSelf.a.city != null)
                {
                    summon.joinCity(pSelf.a.city);
                }

                summon.addStatusEffect("status_rinnegan_controlled", 120f);

                summon.attack_target = pTarget.a;
                summon.beh_actor_target = pTarget.a;
                successCount++;
            }

            return successCount > 0;
        }
        #endregion
        #endregion

        #region Tailed Beast Bombs
        // KCM/BARYON Explosion
        public static bool KCMBlastAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            if (pTarget.current_tile != null)
            {
                MapAction.damageWorld(pTarget.current_tile, 8, AssetManager.terraform.get("bomb"), null);
                EffectsLibrary.spawnAtTile("fx_explosion_tiny", pTarget.current_tile, 0.1f);
                EffectsLibrary.spawnExplosionWave(pTile.posV3, 1f, 1f);
                World.world.applyForceOnTile(pTile: pTarget.current_tile, pByWho: pSelf);
                pTarget.a.getHit(50f, true, AttackType.Other, pSelf, false);
            }
            return true;
        }

        public static bool TailedBeastBombAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
            if (pSelf.current_tile == null || pTarget.current_tile == null) return false;

            float distance = pSelf.a.distanceToActorTile(pTarget.current_tile);
            if (distance > 80f) return false;

            Vector2Int targetPos = pTarget.current_tile.pos;
            float pointDistance = Vector2.Distance(pTarget.current_position, targetPos);
            Vector3 from = Toolbox.getNewPoint(
                pSelf.current_position.x,
                pSelf.current_position.y + 1.5f,
                targetPos.x + 1f,
                targetPos.y + 1f,
                pointDistance,
                true
            );
            Vector3 to = Toolbox.getNewPoint(
                pTarget.current_position.x,
                pTarget.current_position.y,
                targetPos.x,
                targetPos.y,
                Mathf.Max(0.35f, pTarget.a.stats["scale"]),
                true
            );

            World.world.projectiles.spawn(pSelf, pTarget, "projectile_tailed_beast_bomb", from, to);

            return true;
        }

        public static bool TruthSeekingOrbAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
            if (pSelf.current_tile == null || pTarget.current_tile == null) return false;
            if (!pSelf.a.hasStatus("status_six_paths_senjutsu")) return false;

            World.world.projectiles.spawn(
                pSelf,
                pTarget,
                "projectile_truth_seeking_orb",
                pSelf.current_tile.posV3,
                pTarget.current_tile.posV3
            );
            return true;
        }
        #endregion

        #region Mangekyo & EMS Abilities
        // Amaterasu
        public static bool AmaterasuAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            if (!pTarget.a.hasStatus("status_amaterasu"))
            {
                pTarget.a.addStatusEffect("status_amaterasu");

                // Blindness Logic
                ChakraSystem.AddMangekyoUsage(pSelf.a, 1);

                return true;
            }
            return false;
        }

        // Susanoo Ribcage
        public static bool SusanooRibcage(BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            // Only triggers if health below 35%
            float healthPct = (float)pTarget.a.data.health / (float)pTarget.a.getMaxHealth();
            if (healthPct < 0.35f)
            {
                if (!pTarget.a.hasStatus("susanoo_ribcage"))
                {
                    pTarget.a.addStatusEffect("susanoo_ribcage");

                    // Blindness Logic
                    ChakraSystem.AddMangekyoUsage(pTarget.a, 1);

                    return true;
                }
            }
            return false;
        }

        // Base Susanoo (Mangekyo)
        public static bool BaseSusanooAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

            if (!pSelf.a.hasStatus("status_base_susanoo"))
            {
                pSelf.a.addStatusEffect("status_base_susanoo");

                // Blindness Logic
                ChakraSystem.AddMangekyoUsage(pSelf.a, 1);
            }

            EffectsLibrary.spawnExplosionWave(pSelf.current_tile.posV3, 1f, 1f);
            World.world.applyForceOnTile(pTile: pTarget.current_tile, pByWho: pSelf);
            return true;
        }

        // Perfect Susanoo (EMS)
        public static bool PerfectSusanooAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

            if (!pSelf.a.hasStatus("status_perfect_susanoo"))
            {
                pSelf.a.addStatusEffect("status_perfect_susanoo");
            }

            EffectsLibrary.spawnExplosionWave(pSelf.current_tile.posV3, 2f, 1f);
            World.world.applyForceOnTile(pTile: pTarget.current_tile, pByWho: pSelf);

            return true;
        }

        #region Amenotejikara

        public static bool Amenotejikara(BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            Actor actor = pTarget.a;
            WorldTile actorTile = actor.current_tile;
            if (actorTile == null) return false;
            if (!actor.isTask("run_away")) return false;

            List<Actor> nearbyUnits = new List<Actor>();
            foreach (var unit in Finder.getUnitsFromChunk(actorTile, 2, 30f))
            {
                if (unit == null || unit.a == null) continue;
                if (!unit.a.isAlive() || unit.a == actor) continue;
                if (unit.current_tile == null) continue;

                nearbyUnits.Add(unit.a);
            }

            if (nearbyUnits.Count == 0) return false;

            Actor swapTarget = nearbyUnits[UnityEngine.Random.Range(0, nearbyUnits.Count)];

            if (swapTarget == null || swapTarget.current_tile == null) return false;

            WorldTile targetTile = swapTarget.current_tile;

            actor.spawnOn(targetTile);
            swapTarget.spawnOn(actorTile);
            actor.cancelAllBeh();
            swapTarget.cancelAllBeh();

            return true;
        }
        #endregion
        #endregion

        #region Madaras EMS Abilities

        public static bool MadaraPerfectSusanoo(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

            if (!pSelf.a.hasStatus("madara_ps"))
            {
                pSelf.a.addStatusEffect("madara_ps");
            }

            EffectsLibrary.spawnExplosionWave(pSelf.current_tile.posV3, 2f, 1f);
            // World.world.applyForceOnTile(pTile: pTarget.current_tile, pByWho: pSelf);

            return true;
        }

        public static bool MadaraTengaiShinsei(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

            if (pTarget.current_tile != null)
            {
                pSelf.a.addStatusEffect("tengai_shinsei", 3);
                EffectsLibrary.spawnExplosionWave(pSelf.current_tile.posV3, 2f, 1f);
                Meteorite.spawnMeteorite(pTarget.current_tile, pSelf.a);

                ShinobiWorldLogs.AddWorldLog("log_tengai_shinsei", "worldlog_tengai_shinsei", "ui/icons/madara_ems", pSelf.a);
                return true;
            }
            return false;
        }

        public static bool MadaraEMSGenjutsu(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            if (pSelf.a.kingdom == null) return false;

            if (!ShinobiActors.IsTailedBeast(pTarget.a)) return false;
            if (pTarget.a.hasStatus("status_madara_tamed")) return false;

            long targetId = pTarget.a.getID();
            if (!MadaraTamedOriginalKingdoms.ContainsKey(targetId))
            {
                MadaraTamedOriginalKingdoms[targetId] = pTarget.a.kingdom;
            }

            pTarget.a.setKingdom(pSelf.a.kingdom);
            pTarget.a.addStatusEffect("status_madara_tamed", 15f);

            return true;
        }

        public static bool RestoreKingdom(BaseSimObject pTarget, WorldTile pTile = null)
        {
            if (pTarget == null || pTarget.a == null) return false;

            long targetId = pTarget.a.getID();
            if (!MadaraTamedOriginalKingdoms.TryGetValue(targetId, out Kingdom originalKingdom)) return false;

            MadaraTamedOriginalKingdoms.Remove(targetId);

            if (!pTarget.a.isAlive()) return true;
            if (originalKingdom == null) return true;

            pTarget.a.setKingdom(originalKingdom);
            return true;
        }

        // Madara Susanoo Ribcage
        public static bool MadaraSusanooRibcage(BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            float healthPct = (float)pTarget.a.data.health / (float)pTarget.a.getMaxHealth();
            if (healthPct < 0.40f)
            {
                if (!pTarget.a.hasStatus("susanoo_ribcage_madara"))
                {
                    pTarget.a.addStatusEffect("susanoo_ribcage_madara");

                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Baryon Mode Lifespan Drain
        // Baryon Mode Life Decay
        public static bool BaryonLifespanDrainAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            if (!pTarget.a.hasStatus("baryon_life_decay"))
            {
                pTarget.a.addStatusEffect("baryon_life_decay", 5f);
                pTarget.getHit(150f, true, AttackType.Fire, null, true, false);
                return true;
            }

            return false;
        }
        #endregion

        #region Byakugan: Eight Trigrams 64 Palms
        public static bool EightTrigrams64PalmsAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            float dist = Toolbox.Dist(pSelf.current_tile.x, pSelf.current_tile.y, pTarget.current_tile.x, pTarget.current_tile.y);
            if (dist > 4.0f) return false;

            pSelf.a.addStatusEffect("eightAttack");
            pTarget.a.addStatusEffect("status_getting_hit_64");

            return true;
        }
        #endregion

        #region Byakugan: Eight Trigrams Palm Rotation
        public static bool PalmRotationAttackAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

            if (pSelf.a.hasStatus("status_kaiten")) return false;

            pSelf.a.addStatusEffect("status_kaiten");

            EffectsLibrary.spawnExplosionWave(pTile.posV3, 1f, 1f);

            return true;
        }
        #endregion

        #region Armor Break (Gentle Fist / Taijutsu)
        public static bool ArmorBreakAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            if (pTarget.a.hasStatus("chakra_break")) return false;

            pTarget.a.addStatusEffect("chakra_break");
            return true;
        }

        public static bool StrongPunch(BaseSimObject pSelf, BaseSimObject pAttackedBy, WorldTile pTile = null)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            if (pAttackedBy == null || pAttackedBy.a == null || !pAttackedBy.a.isAlive()) return false;

            Actor self = pSelf.a;
            Actor attacker = pAttackedBy.a;

            if (attacker == self) return false;

            if (attacker.current_tile != null)
            {
                World.world.applyForceOnTile(attacker.current_tile, 4, 3f, true, 80);
            }

            return true;
        }
        #endregion

        #region Wood Release: Mokuton Tree
        public static bool WoodReleaseAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            pTarget.a.addStatusEffect("status_mokuton_tree");
            pTarget.a.getHit(50f, true, AttackType.Fire, pSelf.a, true);
            return true;
        }
        #endregion

        #region Kurama Avatar
        /* Idk if i wanna use this or not
        // Kurama Avatar
        public static bool KuramaAvatar(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
    
            if (Randy.randomChance(0.25f) && pSelf.a.hasStatus("status_jinchuriki_kcm2"))
            {
                if (Randy.randomChance(0.25f) && !pSelf.a.hasStatus("status_kurama_avatar"))
                {
                    pSelf.a.addStatusEffect("status_kurama_avatar");

                    // Spawn a Tailed Beast Bomb effect (Explosion)
                    EffectsLibrary.spawnExplosionWave(pSelf.current_tile.posV3, 3f, 1f);
                    return true;
                }
            }
            return false;
        }
        */
        #endregion

        #region Avatar: Bijuu Bite
        // Bijuu Bite (Avatar)
        /*public static bool BijuuBiteAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            EffectsLibrary.spawnAtTile("fx_bijuu_bite", pTarget.current_tile, 0.25f);
            pTarget.a.getHit(300f, true, AttackType.Other, pSelf.a, true);
            EffectsLibrary.spawnExplosionWave(pTarget.current_tile.posV3, 2f, 0.5f);
            return true;
        } */
        #endregion

        #region Avatar: Chakra Arm Swipe
        // Chakra Arm Swipe
        public static bool ChakraArmSwipeAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pTarget == null) return false;

            EffectsLibrary.spawnAtTile("fx_chakra_arm", pSelf.current_tile, 0.1f);
            foreach (Actor unit in Finder.getUnitsFromChunk(pTarget.current_tile, 1, 3f))
            {
                if (unit == pSelf.a || (pSelf.a.kingdom != null && unit.kingdom == pSelf.a.kingdom)) continue;
                if (!unit.isAlive()) continue;

                unit.getHit(250f, true, AttackType.Other, pSelf.a, true);
                World.world.applyForceOnTile(unit.current_tile, 5, 1f, true, 0);
            }
            EffectsLibrary.spawnExplosionWave(pTarget.current_tile.posV3, 2f, 0.5f);
            return true;
        }
        #endregion

        #region Uzumaki Chains
        public static bool AdamantiteChainsAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || !pTarget.a.isAlive()) return false;

            pTarget.a.addStatusEffect("status_adamantite_chains");

            float drain = Mathf.Min(pTarget.a.getMaxHealth() * 0.15f, 250f);
            pTarget.a.getHit(drain, true, AttackType.Other, pSelf.a, true);
            pSelf.a.restoreHealth((int)drain);

            return true;
        }
        #endregion

        public static bool HasAnyNatureTrait(Actor actor)
        {
            if (actor == null) return false;

            return actor.hasTrait("fire_release") || actor.hasTrait("earth_release") || actor.hasTrait("water_release") ||
                   actor.hasTrait("wind_release") || actor.hasTrait("lightning_release") || actor.hasTrait("storm_release") ||
                   actor.hasTrait("wood_release") || actor.hasTrait("yin_release") ||
                   actor.hasTrait("yang_release") || actor.hasTrait("yinyang_release");
        }

        #region Fire Release Ability
        public static bool FireReleaseAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
            if (pTarget.current_tile != null && pSelf.current_tile != null)
            {
                World.world.projectiles.spawn(pSelf, pTarget, "red_orb", pSelf.current_tile.posV3, pTarget.current_tile.posV3);
                pTarget.a.addStatusEffect("katon_burn", 2f);
                return true;
            }
            return false;
        }
        #endregion

        #region Earth Release Ability
        public static bool EarthReleaseAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

            pSelf.addStatusEffect("weight_rock", 5f);
            return true;
        }
        #endregion

        #region Water Release Ability
        public static bool WaterReleaseAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            if (pTarget.current_tile != null)
            {
                pTarget.a.addStatusEffect("water_prison", 5f);
                pTarget.a.addStatusEffect("drowning", 5f);
                return true;
            }
            return false;
        }
        #endregion

        #region Wind Release Ability
        public static bool WindReleaseAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;

            if (pTarget.current_tile != null)
            {
                if (pSelf.current_tile == null) return false;
                World.world.projectiles.spawn(pSelf, pTarget, "projectile_wind_cutter", pSelf.current_tile.posV3, pTarget.current_tile.posV3);
                return true;
            }
            return false;
        }
        #endregion

        #region Lightning Release Ability
        public static bool LightningReleaseAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
            if (pTarget.current_tile != null)
            {
                ActionLibrary.castLightning(pSelf, pTarget, pTile);
                return true;
            }
            return false;
        }
        #endregion

        #region Storm Release Ability
        public static bool StormReleaseAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pTarget == null || pTarget.a == null || !pTarget.a.isAlive()) return false;
            ActionLibrary.castLightning(pSelf, pTarget, pTile);
            if (pTarget.current_tile != null)
            {
                World.world.projectiles.spawn(pSelf, pTarget, "torch", pSelf.current_tile.posV3, pTarget.current_tile.posV3);
            }
            return true;
        }
        #endregion

        #region Akimichi Size Manipulation
        public static bool AkimichiSizeManipulationAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;

            if (pSelf.a.hasStatus("status_akimichi_grown") || pSelf.a.hasStatus("status_akimichi_shrunk")) return false;

            if (Randy.randomChance(0.50f))
            {
                pSelf.a.addStatusEffect("status_akimichi_grown", 20f);
            }
            else
            {
                pSelf.a.addStatusEffect("status_akimichi_shrunk", 20f);
            }

            return true;
        }
        #endregion

        #region Damage Tick
        public static bool DamageTick(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            if (pTarget.a == null || !pTarget.a.isAlive()) return false;
            Actor a = pTarget.a;

            if (Randy.randomChance(0.20f))
            {
                a.getHit((float)a.getMaxHealth() * 0.05f, true, AttackType.Other, null, true);
                return true;
            }
            return false;
        }
        #endregion

        #region Aburame Insect Swarm
        public static bool AburameInsectSwarm(BaseSimObject pSelf, BaseSimObject pAttackedBy, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            if (pAttackedBy == null || pAttackedBy.a == null || !pAttackedBy.a.isAlive()) return false;
            if (pSelf.current_tile == null) return false;
            Actor a = pSelf.a;

            string[] insectIds = { "scorpion" };
            int summonCount = UnityEngine.Random.Range(1, Mathf.Max(2, a.data.level));

            for (int i = 0; i < summonCount; i++)
            {
                string unitId = insectIds[UnityEngine.Random.Range(0, insectIds.Length)];
                WorldTile spawnTile = Toolbox.getRandomTileWithinDistance(pSelf.current_tile, 2) ?? pSelf.current_tile;
                Actor summon = World.world.units.createNewUnit(unitId, spawnTile, true, 1f);
                if (summon == null) continue;

                if (pSelf.a.kingdom != null)
                {
                    summon.setKingdom(pSelf.a.kingdom);
                }
                if (pSelf.a.city != null)
                {
                    summon.joinCity(pSelf.a.city);
                }

                summon.data.health = (int)(pSelf.a.getMaxHealth() * 0.10f);

                summon.addStatusEffect("angry", 15f);
                summon.addStatusEffect("status_aburame_mark", 10f);

                summon.attack_target = pAttackedBy.a;
                summon.beh_actor_target = pAttackedBy.a;
            }
            return true;
        }
        #endregion
        
        /*
        #region Jugo Transformation
        public static bool JugoTransformation(BaseSimObject pTarget, WorldTile pTile)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            Actor a = pSelf.a;

            ChakraSystem.AddChakra(actor, 5f * deltaTime);

            if (actor.data.health < (actor.getMaxHealth() * 0.5f))
            {
                if (!actor.hasStatus("jugo_rage"))
                {
                    actor.addStatusEffect("jugo_rage");
                }
                {
                    actor.addStatusEffect("status_jugo_transformation");
                }
            }
                return false;
        }
        #endregion
        */
        
        #region Helpers
        private static bool SetWorldAgeChaos()
        {
            WorldAgeAsset chaosAge = AssetManager.era_library.get("age_chaos");
            if (chaosAge == null || World.world?.era_manager == null) return false;

            int slotIndex = World.world.era_manager.getCurrentSlotIndex();
            World.world.era_manager.setAgeToSlot(chaosAge, slotIndex);

            _chaosRevertPending = true;
            _chaosRevertTimer = ChaosAgeDurationSeconds;
            return true;
        }

        public static void TickWorldAgeRevert(float dt)
        {
            if (!_chaosRevertPending) return;

            _chaosRevertTimer -= dt;
            if (_chaosRevertTimer > 0f) return;

            _chaosRevertPending = false;
            RevertWorldAgeToHope();
        }

        private static void RevertWorldAgeToHope()
        {
            if (World.world?.era_manager == null) return;

            string currentAgeId = GetCurrentWorldAgeId();
            if (currentAgeId != null && currentAgeId != "age_chaos") return;

            WorldAgeAsset hopeAge = AssetManager.era_library.get("age_hope");
            if (hopeAge == null) return;

            int slotIndex = World.world.era_manager.getCurrentSlotIndex();
            World.world.era_manager.setAgeToSlot(hopeAge, slotIndex);
        }

        private static string GetCurrentWorldAgeId()
        {
            object eraManager = World.world?.era_manager;
            if (eraManager == null) return null;

            Type managerType = eraManager.GetType();
            object currentAge = null;

            MethodInfo getCurrentAge = managerType.GetMethod("getCurrentAge", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (getCurrentAge != null)
            {
                currentAge = getCurrentAge.Invoke(eraManager, null);
            }

            if (currentAge == null)
            {
                FieldInfo field = managerType.GetField("current_age", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    currentAge = field.GetValue(eraManager);
                }
            }

            if (currentAge == null) return null;

            Type ageType = currentAge.GetType();
            FieldInfo idField = ageType.GetField("id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (idField != null)
            {
                return idField.GetValue(currentAge) as string;
            }

            PropertyInfo idProp = ageType.GetProperty("id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (idProp != null)
            {
                return idProp.GetValue(currentAge, null) as string;
            }

            return null;
        }

        public static bool COHDeathAction(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            if (pTarget == null || pTarget.a == null) return false;

            Actor target = pTarget.a;

            if (target.city != null && target.current_tile != null)
            {
                var nearby = Finder.getUnitsFromChunk(target.current_tile, 1);
                foreach (var u in nearby)
                {
                    if (u.a != null && u.a.isAlive() && u.a != target && u.a.city == target.city)
                    {
                        u.a.addStatusEffect("cursed", 25f);
                    }
                }
            }

            return true;
        }

        public static bool WOFDeathAction(BaseSimObject pTarget = null, WorldTile pTile = null)
        {
            if (pTarget == null || pTarget.a == null) return false;

            Actor target = pTarget.a;

            if (target.city != null && target.current_tile != null)
            {
                var nearby = Finder.getUnitsFromChunk(target.current_tile, 1);
                foreach (var u in nearby)
                {
                    if (u.a != null && u.a.isAlive() && u.a != target && u.a.city == target.city)
                    {
                        u.a.addStatusEffect("power_up", 25f);
                    }
                }
            }

            return true;
        }

        public static bool ShadowCloneAction(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            Actor actor = pSelf.a;

            if (actor.isClone()) return false;
            if (actor.current_tile == null) return false;

            int level = actor.data.level;
            int maxClones = Mathf.Clamp(level, 1, 9);
            float costPerClone = 25f;
            int spawned = 0;

            for (int i = 0; i < maxClones; i++)
            {
                if (!ChakraSystem.TryConsume(actor, ChakraSystem.GetJutsuCost(actor, costPerClone))) break;

                WorldTile spawnTile = Toolbox.getRandomTileWithinDistance(actor.current_tile, 1) ?? actor.current_tile;

                actor.prepareForSave();
                ActorData tOriginalData = actor.data;
                ActorData tCloneData = new ActorData();

                ActorTool.copyImportantData(tOriginalData, tCloneData, true);

                tCloneData.created_time = World.world.getCurWorldTime();
                tCloneData.id = World.world.map_stats.getNextId("unit");
                tCloneData.name = $"Clone {actor.getName()}";
                tCloneData.custom_name = tOriginalData.custom_name;
                tCloneData.x = spawnTile.x;
                tCloneData.y = spawnTile.y;

                Actor clone = World.world.units.loadObject(tCloneData);
                if (clone == null) continue;

                clone.created_time_unscaled = (double)Time.time;

                foreach (ActorTrait tTrait in actor.getTraits())
                {
                    clone.addTrait(tTrait.id, false);
                }
                clone.addTrait("clone", false);
                clone.removeTrait("shadow_clonej");

                clone.data.cloneCustomDataFrom(actor.data);
                clone.setReligion(actor.religion);
                clone.setClan(actor.clan);
                clone.setCulture(actor.culture);
                clone.setSubspecies(actor.subspecies);
                clone.joinLanguage(actor.language);
                clone.setFamily(actor.family);
                clone.kingdom = actor.kingdom;
                clone.joinCity(actor.city);

                if (tOriginalData.saved_items != null)
                {
                    foreach (long tID in tOriginalData.saved_items)
                    {
                        Item tItem = World.world.items.get(tID);
                        if (tItem != null)
                        {
                            Item tNewItem = World.world.items.generateItem(tItem.getAsset(), null, null, 1, clone, 0, false);
                            tNewItem.data.modifiers.Clear();
                            tNewItem.data.modifiers.AddRange(tItem.data.modifiers);
                            tNewItem.data.modifiers.Remove("eternal");
                            tNewItem.initItem();
                            clone.equipment.setItem(tNewItem, clone);
                        }
                    }
                }
                clone.setHealth((int)(actor.getMaxHealth() * 0.40f), false);
                clone.setStamina(tOriginalData.stamina, false);
                clone.setMana(tOriginalData.mana, false);
                clone.data.level = actor.data.level;
                clone.data.favorite = false;
                clone.asset.can_be_favorited = false;

                clone.addStatusEffect("shadow_clone", 12f);
                clone.removeTrait("shadow_clonej");

                clone.attack_target = actor.attack_target;
                clone.beh_actor_target = actor.attack_target?.a;

                float chakraShare = Mathf.Max(0f, ChakraSystem.GetCurrent(actor) / (maxClones + 1f));
                ChakraSystem.SetCurrent(clone, chakraShare);

                clone.applyRandomForce(1.5f, 2f);
                EffectsLibrary.spawnAtTile("fx_smoke", spawnTile, 0.05f);

                spawned++;
            }

            return spawned > 0;
        }

        public static bool KamuiAction(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null)
        {
            if (pSelf == null || pSelf.a == null || !pSelf.a.isAlive()) return false;
            Actor actor = pSelf.a;

            if (actor.current_tile == null) return false;

            actor.addStatusEffect("kamui_intangibility", 6f);
            actor.addStatusEffect("invincible", 6f);

            if (actor.hasTrait("mangekyo_sharingan"))
            {
                ChakraSystem.AddMangekyoUsage(pSelf.a, 1);
            }

            return true;
        }

        public static void PlayWavSound(string fileName, Vector3 position)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;
            if (!MapBox.isRenderGameplay()) return;
            if (Time.unscaledTime < _nextSoundTime) return;

            Camera camera = Camera.main;
            if (camera == null) return;
            Vector3 view = camera.WorldToViewportPoint(position);
            if (view.x < 0f || view.x > 1f || view.y < 0f || view.y > 1f) return;

            _nextSoundTime = Time.unscaledTime + SoundSpacing;

            string soundPath = Path.Combine(Main.Instance.GetDeclaration().FolderPath, "GameResources", "sounds", fileName);

            Isekai.PlayWavDirectly.Instance.PlaySoundAtPosition(soundPath, position);
        }
        #endregion
    }
}