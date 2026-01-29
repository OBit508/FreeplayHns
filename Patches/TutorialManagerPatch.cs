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
        public static bool AmCrewmate;
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
            normalGameOptionsV.SetInt(Int32OptionNames.CrewmateVentUses, 3);
            GameOptionsManager.Instance.CurrentGameOptions = normalGameOptionsV.Cast<IGameOptions>();
            PlayerControl.LocalPlayer.RpcSetRole(AmCrewmate ? RoleTypes.Engineer : RoleTypes.Impostor, false);
            PlayerControl.LocalPlayer.AdjustLighting();
            PlayerControl.LocalPlayer.cosmetics.SetAsLocalPlayer();
            PlayerControl.LocalPlayer.moveable = AmCrewmate;
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
            CreatePlayers();
            if (!AmCrewmate)
            {
                GameManager.Instance.StartCoroutine(ImpostorComp.CoDoAnimation(PlayerControl.LocalPlayer).WrapToIl2Cpp());
            }
            ShipStatus.Instance.StartSFX();
            global::Logger.GlobalInstance.Info(string.Format("Started Freeplay Game in {0}", (MapNames)AmongUsClient.Instance.TutorialMapId), null);
            yield break;
        }
        public static void CreatePlayers()
        {
            System.Random random = new System.Random();
            if (AmCrewmate)
            {
                PlayerControl playerControl = GameObject.Instantiate<PlayerControl>(TutorialManager.Instance.PlayerPrefab);
                playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();
                NetworkedPlayerInfo networkedPlayerInfo = GameData.Instance.AddDummy(playerControl);
                AmongUsClient.Instance.Spawn(networkedPlayerInfo, -2, SpawnFlags.None);
                AmongUsClient.Instance.Spawn(playerControl, -2, SpawnFlags.None);
                playerControl.isDummy = true;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName("Impostor");
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
                ShipStatus.Instance.SpawnPlayer(playerControl, 1, true);
                playerControl.moveable = false;
                playerControl.gameObject.AddComponent<ImpostorComp>();
            }
            for (int i = 0; i < (AmCrewmate ? 13 : 14); i++)
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
                List<byte> tasks = new List<byte>();
                List<PlayerTask> prefabs = ShipStatus.Instance.GetAllTasks().ToList();
                while (tasks.Count < 5 && prefabs.Count > 0)
                {
                    int index = random.Next(0, prefabs.Count - 1);
                    PlayerTask t = prefabs[index];                    
                    tasks.Add((byte)t.Index);
                    prefabs.RemoveAt(index);
                }
                playerControl.Data.SetTasks(tasks.ToArray());
                ShipStatus.Instance.SpawnPlayer(playerControl, PlayerControl.AllPlayerControls.Count, true);
                playerControl.gameObject.AddComponent<CrewmateComp>();
            }
        }
    }
}
