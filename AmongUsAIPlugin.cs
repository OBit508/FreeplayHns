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
	public class AmongUsAIPlugin : BasePlugin
	{
        public Harmony Harmony { get; } = new Harmony(ModId);
        public static SimpleCheck Helper;
        public override void Load()
		{
            Logger = Log;
            ClassInjector.RegisterTypeInIl2Cpp<ImpostorComp>();
            ClassInjector.RegisterTypeInIl2Cpp<CrewmateComp>();
            ClassInjector.RegisterTypeInIl2Cpp<SimpleCheck>();
            Harmony.PatchAll();
            Helper = AddComponent<SimpleCheck>();
        }
        public static ManualLogSource Logger;
        public class SimpleCheck : MonoBehaviour
        {
            public void Update()
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    FreeplayStartHelper.Play();
                }
            }
        }
        public const string ModName = "AmongUsAI";
        public const string Owner = "rafael";
        public const string ModDescription = "This is a mod that make an AI play AmongUs";
        public const string ModId = "com." + Owner + "." + ModName;
        public const string ModVersion = "1.0.0";
    }
}
