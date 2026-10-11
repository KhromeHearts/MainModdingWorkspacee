using System;
using System.Collections.Generic;
using System.IO;
using FMOD;
using FMODUnity;
using UnityEngine;

namespace Ambience
{
    public class PlayWavDirectly
    {
        private static PlayWavDirectly _instance;
        public static PlayWavDirectly Instance => _instance ??= new PlayWavDirectly();
        private FMOD.System fmodSystem;
        private ChannelGroup masterChannelGroup;

        private readonly Dictionary<string, FMOD.Sound> soundCache = new Dictionary<string, FMOD.Sound>();

        private PlayWavDirectly()
        {
            InitializeFMODSystem();
        }

        private void InitializeFMODSystem()
        {
            var result = RuntimeManager.StudioSystem.getCoreSystem(out fmodSystem);
            if (result != FMOD.RESULT.OK)
            {
                UnityEngine.Debug.LogError($"Failed to initialize FMOD Core System. Result: {result}");
                return;
            }
            result = fmodSystem.getMasterChannelGroup(out masterChannelGroup);
            if (result != FMOD.RESULT.OK)
            {
                UnityEngine.Debug.LogError($"Failed to retrieve master channel group. Result: {result}");
            }
        }

        private FMOD.Sound GetOrCreateSound(string filePath, MODE mode)
        {
            if (soundCache.TryGetValue(filePath, out FMOD.Sound cachedSound) && cachedSound.hasHandle())
            {
                return cachedSound;
            }

            if (!File.Exists(filePath)) return default;

            RESULT result = fmodSystem.createSound(filePath, mode, out FMOD.Sound sound);
            if (result == RESULT.OK)
            {
                soundCache[filePath] = sound;
                return sound;
            }

            UnityEngine.Debug.LogError($"[Ambience] FMOD createSound failed: {result}");
            return default;
        }

        public Channel PlaySoundFromFile(string filePath, float volume = 0.80f)
        {
            Channel channel = default;
            FMOD.Sound sound = GetOrCreateSound(filePath, MODE._2D);
            if (!sound.hasHandle()) return channel;

            try
            {
                RESULT result = fmodSystem.playSound(sound, masterChannelGroup, false, out channel);
                if (result == RESULT.OK)
                {
                    channel.setVolume(volume);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error playing 2D sound: {ex.Message}");
            }

            return channel;
        }

        public Channel PlaySoundAtPosition(string filePath, Vector3 position, float minDistance = 5f, float maxDistance = 45f, float volume = 0.45f)
        {
            Channel channel = default;
            FMOD.Sound sound = GetOrCreateSound(filePath, MODE._3D | MODE._3D_LINEARROLLOFF | MODE.LOOP_NORMAL);
            if (!sound.hasHandle()) return channel;

            try
            {
                sound.set3DMinMaxDistance(minDistance, maxDistance);

                RESULT result = fmodSystem.playSound(sound, masterChannelGroup, true, out channel);
                if (result != RESULT.OK) return channel;

                FMOD.VECTOR fmodPos = new FMOD.VECTOR { x = position.x, y = position.y, z = position.z };
                FMOD.VECTOR fmodVel = new FMOD.VECTOR { x = 0f, y = 0f, z = 0f };
                channel.set3DAttributes(ref fmodPos, ref fmodVel);

                channel.setVolume(volume);
                channel.setPaused(false);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error playing 3D sound: {ex.Message}");
            }

            return channel;
        }
    }
}