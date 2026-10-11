using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Config;
using Random=UnityEngine.Random;
using ReflectionUtility;
using System.Reflection;
using UnityEngine.Tilemaps;
using Newtonsoft.Json;
using System.IO;
using NCMS;
using ai;


namespace JinchurikiBox.Core;
public class Jutsu {
    public string id = "";
    public float cost = 0;
    public string tier = "";
    public delegate bool JutsuAction(BaseSimObject pTarget, BaseSimObject pSelf, WorldTile pTile = null); 
    public JutsuAction action;
    public float cooldown = 0;
    bool onCooldown = false;
    public float castTime = 0;
    public int requiredLevel = 0;

    public void Cast(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null) {
        Actor a = pSelf.a;
        if (ChakraSystem.currentChakra(a) < cost || action == null || pSelf == null || pTarget == null || onCooldown || a.data.level < requiredLevel) {
            return;
        }
        ChakraSystem.removeChakra(a, cost);
        action?.Invoke(pTarget, pSelf, pTile);
        pSelf.a.makeWait(castTime);
        onCooldown = true;
        Task t = Task.Run(async () => {
            await Task.Delay((int)(cooldown * 1000));
            onCooldown = false;
        });
    }
}