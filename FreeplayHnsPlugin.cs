using FreeplayHns;
using FreeplayHns.Components;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.Injection;
using InnerNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using xCloud;

namespace FreeplayHns
{
	[BepInProcess("Among Us.exe")]
	[BepInPlugin(ModId, ModName, ModVersion)]
	public class FreeplayHnsPlugin : BasePlugin
	{
        public Harmony Harmony { get; } = new Harmony(ModId);
        public override void Load()
		{
            ClassInjector.RegisterTypeInIl2Cpp<ImpostorComp>();
            ClassInjector.RegisterTypeInIl2Cpp<CrewmateComp>();
            Harmony.PatchAll();
        }
        public const string ModName = "AmongUsAI";
        public const string Owner = "rafael";
        public const string ModDescription = "This is a mod that make an AI play AmongUs";
        public const string ModId = "com." + Owner + "." + ModName;
        public const string ModVersion = "1.0.0";
    }
}
