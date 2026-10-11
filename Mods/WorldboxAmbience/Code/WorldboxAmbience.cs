using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using FMOD;
using HarmonyLib;

namespace Ambience
{
    public class WorldboxAmbience : MonoBehaviour
    {
        private readonly List<string> hopePlaylist = new List<string> { "hope_1.wav", "hope_2.wav", "hope_3.wav", "hope_4.wav", "hope_5.wav", "hope_6.wav" };
        private readonly List<string> sunPlaylist = new List<string> { "ash_1.wav" };
        private readonly List<string> darkPlaylist = new List<string> { "dark_1.wav", "dark_2.wav", "dark_3.wav" };
        private readonly List<string> tearsPlaylist = new List<string> { "rainMed.wav" };
        private readonly List<string> moonPlaylist = new List<string> { "dark_2.wav", "howl1.wav", "dark_3.wav", "howl2.wav", "dark_1.wav" };
        private readonly List<string> chaosPlaylist = new List<string> { "chaos_1.wav" };
        private readonly List<string> wondersPlaylist = new List<string> { "wonders_1.wav", "wonders_2.wav" };
        private readonly List<string> icePlaylist = new List<string> { "snow_1.wav", "snow_2.wav" };
        private readonly List<string> ashPlaylist = new List<string> { "ash_1.wav" };
        private readonly List<string> despairPlaylist = new List<string> { "snow_1.wav", "snow_2.wav" };

        private Coroutine activePlaylist;
        private Coroutine fader;
        private FMOD.Channel currentChannel;
        private string activeAgeId = string.Empty;

        public static Dictionary<WorldTile, FMOD.Channel> ActiveFireSounds = new Dictionary<WorldTile, FMOD.Channel>();

        private const int MAX_FIRE_SOUNDS = 15;
        private const float MIN_FIRE_SOUND_DISTANCE = 8f;

        private void Awake()
        {
            MapBox.on_world_loaded += OnWorldLoaded;
        }

        private void OnDestroy()
        {
            MapBox.on_world_loaded -= OnWorldLoaded;
            ClearAllFireSounds();
            StopCurrentAudio(fadeDuration: 0f);
        }

        private void Update()
        {
            CheckForAgeChange();
        }

        private void OnWorldLoaded()
        {
            activeAgeId = string.Empty;
            ClearAllFireSounds();
            CheckForAgeChange();
        }

        public static void ClearAllFireSounds()
        {
            foreach (var kvp in ActiveFireSounds)
            {
                if (kvp.Value.hasHandle())
                {
                    kvp.Value.stop();
                }
            }
            ActiveFireSounds.Clear();
        }

        private void CheckForAgeChange()
        {
            WorldAgeAsset currentAge = World.world_era;
            if (currentAge == null || currentAge.id == activeAgeId) return;

            activeAgeId = currentAge.id;
            List<string> selectedPlaylist = GetPlaylistForAge(activeAgeId);

            if (selectedPlaylist != null && selectedPlaylist.Count > 0)
            {
                StartPlaylist(selectedPlaylist, shouldLoop: true, shuffle: true);
            }
            else
            {
                UnityEngine.Debug.Log($"[Ambience] No playlist assigned for age: {activeAgeId}");
                StopCurrentAudio(fadeDuration: 1.5f);
            }
        }

        public List<string> GetPlaylistForAge(string ageId)
        {
            return ageId switch
            {
                "age_hope" => hopePlaylist,
                "age_sun" => sunPlaylist,
                "age_dark" => darkPlaylist,
                "age_tears" => tearsPlaylist,
                "age_moon" => moonPlaylist,
                "age_chaos" => chaosPlaylist,
                "age_wonders" => wondersPlaylist,
                "age_ice" => icePlaylist,
                "age_ash" => ashPlaylist,
                "age_despair" => despairPlaylist,
                _ => null
            };
        }

        public void StartPlaylist(List<string> tracks, bool shouldLoop, bool shuffle = true)
        {
            StopCurrentAudio(fadeDuration: 1.5f);
            activePlaylist = StartCoroutine(PlayPlaylistRoutine(tracks, shouldLoop, shuffle));
        }

        public void StopCurrentAudio(float fadeDuration = 1.5f)
        {
            if (activePlaylist != null)
            {
                StopCoroutine(activePlaylist);
                activePlaylist = null;
            }

            if (fader != null)
            {
                StopCoroutine(fader);
                fader = null;
            }

            if (currentChannel.hasHandle())
            {
                if (fadeDuration > 0f)
                {
                    fader = StartCoroutine(FadeAndStop(currentChannel, fadeDuration));
                }
                else
                {
                    currentChannel.stop();
                }
            }
        }

        public IEnumerator FadeAndStop(FMOD.Channel channelToStop, float duration)
        {
            if (!channelToStop.hasHandle()) yield break;

            channelToStop.getVolume(out float startingVolume);
            float timer = 0f;

            while (timer < duration && channelToStop.hasHandle())
            {
                timer += Time.deltaTime;
                float newVolume = Mathf.Lerp(startingVolume, 0f, timer / duration);
                channelToStop.setVolume(newVolume);
                yield return null;
            }

            if (channelToStop.hasHandle())
            {
                channelToStop.stop();
            }
        }

        public IEnumerator PlayPlaylistRoutine(List<string> tracks, bool shouldLoop, bool shuffle)
        {
            List<string> currentQueue = new List<string>(tracks);

            do
            {
                if (shuffle)
                {
                    currentQueue = currentQueue.OrderBy(_ => UnityEngine.Random.value).ToList();
                }

                for (int i = 0; i < currentQueue.Count; i++)
                {
                    string fileName = currentQueue[i];

                    currentChannel = PlayWavSound(fileName);
                    UnityEngine.Debug.Log($"[Ambience] Playing track ({i + 1}/{currentQueue.Count}): {fileName}");

                    yield return null;

                    bool isPlaying = true;
                    while (isPlaying && currentChannel.hasHandle())
                    {
                        currentChannel.isPlaying(out isPlaying);
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }
            while (shouldLoop);
        }

        public static FMOD.Channel PlayWavSound(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return default;

            string modPath = AmbienceMod.Instance.GetDeclaration().FolderPath;
            string soundPath = Path.Combine(modPath, "GameResources", "Sounds", fileName);

            return Ambience.PlayWavDirectly.Instance.PlaySoundFromFile(soundPath);
        }

        public static FMOD.Channel PlayPositionSound(string fileName, Vector3 position)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return default;

            string modPath = AmbienceMod.Instance.GetDeclaration().FolderPath;
            string soundPath = Path.Combine(modPath, "GameResources", "Sounds", fileName);

            return Ambience.PlayWavDirectly.Instance.PlaySoundAtPosition(soundPath, position);
        }

        [HarmonyPatch(typeof(WorldTile), "startFire")]
        public static class StartFlameAmbience
        {
            static void Postfix(WorldTile __instance, ref bool __result)
            {
                if (__instance == null || !__result) return;

                if (ActiveFireSounds.Count >= MAX_FIRE_SOUNDS || ActiveFireSounds.ContainsKey(__instance)) return;

                foreach (var tile in ActiveFireSounds.Keys)
                {
                    if (tile != null && Vector3.Distance(tile.posV3, __instance.posV3) < MIN_FIRE_SOUND_DISTANCE)
                    {
                        return;
                    }
                }

                FMOD.Channel channel = PlayPositionSound("fire_crackle.wav", __instance.posV3);
                if (channel.hasHandle())
                {
                    ActiveFireSounds[__instance] = channel;
                }
            }
        }

        [HarmonyPatch(typeof(WorldTile), "stopFire")]
        public static class StopFlameAmbience
        {
            static void Postfix(WorldTile __instance)
            {
                if (__instance == null) return;

                if (ActiveFireSounds.TryGetValue(__instance, out FMOD.Channel channel))
                {
                    if (channel.hasHandle())
                    {
                        channel.stop();
                    }
                    ActiveFireSounds.Remove(__instance);
                }
            }
        }
    }
}