using System;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using NeoModLoader.api;

namespace AntiStun
{
    public class Main : BasicMod<Main>
    {
        protected override void OnModLoad()
        {
            if (GameObject.Find("AntiStun_Logic") == null)
            {
                var go = new GameObject("AntiStun_Logic");
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.AddComponent<StunLoop>();
            }

            Debug.Log("[AntiStun] Mod loaded. StunLoop active.");
        }
    }

    public class StunLoop : MonoBehaviour
    {
        void Update()
        {
            if (World.world.isPaused()) return;

            foreach (Actor a in MapBox.instance.units)
            {
                if (a.isAlive())
                {
                    if (a.hasStatus("stunned"))
                    {
                        a.finishStatusEffect("stunned");
                    }
                }
            }
        }
    }
}
