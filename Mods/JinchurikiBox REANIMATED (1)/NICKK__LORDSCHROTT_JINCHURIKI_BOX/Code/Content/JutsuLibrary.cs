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
using JinchurikiBox.Core;
using JinchurikiBox.Actions;


namespace JinchurikiBox.Content;
public static class JutsuLibrary {
    public static Jutsu Amaterasu = new Jutsu() {
        id = "amaterasu",
        tier = "S",
        cost = 20,
        cooldown = 15f,
        action = JutsuActions.amaterasuActivation
    };

    public static Jutsu ShadowClone = new Jutsu() {
        id = "shadow_clone",
        tier = "B",
        cost = 5,
        cooldown = 2.5f,
        action = JutsuActions.MakeShadowClone
    };

    public static Jutsu SusanooRib = new Jutsu() {
        id = "susanoorib",
        tier = "S",
        cost = 20,
        cooldown = 25f,
        action = JutsuActions.Susanoo
    };

    public static Jutsu SharinganGenjutsu = new Jutsu() {
        id = "sharingan_genjutsu",
        tier = "D",
        cost = 10,
        cooldown = 8f,
        action = JutsuActions.SharinganGenjutsu
    };

    public static Jutsu Kirin = new Jutsu() {
        id = "kirin",
        tier = "S",
        cost = 30,
        cooldown = 20f,
        action = JutsuActions.Kirin
    };

    public static Jutsu Rasengan = new Jutsu() {
        id = "rasengan",
        tier = "A",
        cost = 15,
        castTime = 1.25f,
        cooldown = 8f,
        action = JutsuActions.Rasengan
    };

    public static Jutsu Chidori = new Jutsu() {
        id = "chidori",
        tier = "A",
        cost = 15,
        castTime = 1.25f,
        cooldown = 8f,
        action = JutsuActions.Chidori
    };

    public static Jutsu EightGates = new Jutsu() {
        id = "gates",
        tier = "?",
        cost = 5,
        castTime = 2.0f,
        cooldown = 60f,
        requiredLevel = 4,
        action = JutsuActions.InnerGates
    };

    public static Jutsu Multisize = new Jutsu() {
        id = "multis",
        tier = "C",
        cost = 25,
        castTime = 0.0f,
        cooldown = 30f,
        requiredLevel = 3,
        action = JutsuActions.Multisize
    };

    public static void init() {

    }
}