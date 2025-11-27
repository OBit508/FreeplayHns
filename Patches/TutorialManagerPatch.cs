using AmongUs.Data;
using AmongUs.GameOptions;
using FreeplayHns.Components;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FreeplayHns.Patches
{
    [HarmonyPatch(typeof(TutorialManager), "Awake")]
    internal static class TutorialManagerPatch
    {
        public static bool CustomHideAndSeek;
        public static int ImpostorCount = 2;
        public static int CrewmateCount = 30;
        public static bool Prefix(TutorialManager __instance)
        {
            if (CustomHideAndSeek)
            {
                CustomHideAndSeek = false;
                if (!TutorialManager._instance)
                {
                    TutorialManager._instance = __instance;
                    if (__instance.DontDestroy)
                    {
                        GameObject.DontDestroyOnLoad(__instance.gameObject);
                        return false;
                    }
                }
                else if (TutorialManager._instance != __instance)
                {
                    GameObject.Destroy(__instance.gameObject);
                }
                DataManager.Player.Stats.SetStatTrackingEnabled(false);
                __instance.StartCoroutine(RunTutorial(__instance).WrapToIl2Cpp());
                GameDebugCommands.AddCommands();
                TutorialDebugCommands.AddCommands(__instance.gameObject);
                return false;
            }
            return true;
        }
        public static System.Collections.IEnumerator RunTutorial(TutorialManager tutorialManager)
        {
            while (!ShipStatus.Instance)
            {
                yield return null;
            }
            ShipStatus.Instance.Timer = 15f;
            while (!PlayerControl.LocalPlayer)
            {
                yield return null;
            }
            while (!GameManager.Instance)
            {
                yield return null;
            }
            if (DestroyableSingleton<DiscordManager>.InstanceExists)
            {
                DestroyableSingleton<DiscordManager>.Instance.SetHowToPlay();
            }
            HideNSeekGameOptionsV10 normalGameOptionsV = new HideNSeekGameOptionsV10(new UnityLogger().Cast<Hazel.ILogger>());
            normalGameOptionsV.SetInt(Int32OptionNames.NumImpostors, 1);
            normalGameOptionsV.SetInt(Int32OptionNames.CrewmateVentUses, 5);
            GameOptionsManager.Instance.CurrentGameOptions = normalGameOptionsV.Cast<IGameOptions>();
            PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Engineer, false);
            PlayerControl.LocalPlayer.AdjustLighting();
            PlayerControl.LocalPlayer.cosmetics.SetAsLocalPlayer();
            switch (AmongUsClient.Instance.TutorialMapId)
            {
                case 0:
                case 3:
                    GameObject.Instantiate<GameObject>(tutorialManager.skeldDetectiveLocationsPrefab);
                    break;
                case 1:
                    GameObject.Instantiate<GameObject>(tutorialManager.miraDetectiveLocationsPrefab);
                    break;
                case 2:
                    GameObject.Instantiate<GameObject>(tutorialManager.polusDetectiveLocationsPrefab);
                    break;
                case 4:
                    GameObject.Instantiate<GameObject>(tutorialManager.airshipDetectiveLocationsPrefab);
                    break;
                case 5:
                    GameObject.Instantiate<GameObject>(tutorialManager.fungleDetectiveLocationsPrefab);
                    break;
            }
            yield return ShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();
            ShipStatus.Instance.Begin();
            GameManager.Instance.StartGame();
            ShipStatus.Instance.StartSFX();
            global::Logger.GlobalInstance.Info(string.Format("Started Freeplay Game in {0}", (MapNames)AmongUsClient.Instance.TutorialMapId), null);
            yield break;
        }
        public static void CreatePlayers()
        {
            int a = 1;
            System.Random random = new System.Random();
            for (int i = 0; i < ImpostorCount; i++)
            {
                PlayerControl playerControl = GameObject.Instantiate<PlayerControl>(TutorialManager.Instance.PlayerPrefab);
                playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();
                NetworkedPlayerInfo networkedPlayerInfo = GameData.Instance.AddDummy(playerControl);
                AmongUsClient.Instance.Spawn(networkedPlayerInfo, -2, SpawnFlags.None);
                AmongUsClient.Instance.Spawn(playerControl, -2, SpawnFlags.None);
                playerControl.isDummy = true;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName("Impostor " + (i + 1).ToString());
                byte b = (byte)random.Next(0, Palette.ColorNames.Count - 1);
                playerControl.SetColor((int)b);
                playerControl.SetHat("", (int)b);
                playerControl.SetSkin("", (int)b);
                playerControl.SetPet("");
                playerControl.SetVisor("", (int)b);
                playerControl.SetNamePlate("");
                playerControl.SetLevel(0U);
                playerControl.RpcSetRole(RoleTypes.Impostor);
                networkedPlayerInfo.RpcSetTasks(new byte[0]);
                ShipStatus.Instance.SpawnPlayer(playerControl, a, false);
                playerControl.gameObject.AddComponent<ImpostorComp>();
                a++;
            }
            for (int i = 0; i < CrewmateCount; i++)
            {
                PlayerControl playerControl = GameObject.Instantiate<PlayerControl>(TutorialManager.Instance.PlayerPrefab);
                playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();
                NetworkedPlayerInfo networkedPlayerInfo = GameData.Instance.AddDummy(playerControl);
                AmongUsClient.Instance.Spawn(networkedPlayerInfo, -2, SpawnFlags.None);
                AmongUsClient.Instance.Spawn(playerControl, -2, SpawnFlags.None);
                playerControl.isDummy = true;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName("Crewmate " + (i + 1).ToString());
                byte b = (byte)random.Next(0, Palette.ColorNames.Count - 1);
                playerControl.SetColor((int)b);
                playerControl.SetHat("", (int)b);
                playerControl.SetSkin("", (int)b);
                playerControl.SetPet("");
                playerControl.SetVisor("", (int)b);
                playerControl.SetNamePlate("");
                playerControl.SetLevel(0U);
                playerControl.RpcSetRole(RoleTypes.Engineer);
                networkedPlayerInfo.RpcSetTasks(new byte[0]);
                ShipStatus.Instance.SpawnPlayer(playerControl, a, false);
                playerControl.gameObject.AddComponent<CrewmateComp>();
                a++;
            }
        }
    }
}
