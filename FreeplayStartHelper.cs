using AmongUs.Data;
using AmongUs.GameOptions;
using FreeplayHns.Patches;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using InnerNet;
using FreeplayHns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FreeplayHns
{
    internal static class FreeplayStartHelper
    {
        public static void Play()
        {
            AmongUsClient.Instance.TutorialMapId = 0;
            GameOptionsManager.Instance.SwitchGameMode(GameModes.HideNSeek);
            if (!NameTextBehaviour.IsValidName(DataManager.Player.Customization.Name))
            {
                DataManager.Player.Customization.Name = "";
                DataManager.Player.Save();
            }
            AmongUsAIPlugin.Helper.StartCoroutine(CoStartGame().WrapToIl2Cpp());
        }
        public static System.Collections.IEnumerator CoStartGame()
        {
            TutorialManagerPatch.CustomHideAndSeek = true;
            try
            {
                SoundManager.Instance.StopAllSound();
                AmongUsClient.Instance.NetworkMode = NetworkModes.FreePlay;
                DestroyableSingleton<InnerNetServer>.Instance.StartAsLocalServer();
                AmongUsClient.Instance.SetEndpoint("127.0.0.1", 22023, false);
                AmongUsClient.Instance.MainMenuScene = "MainMenu";
                AmongUsClient.Instance.OnlineScene = "Tutorial";
            }
            catch (Exception ex)
            {
                Debug.LogError("HostGameButton::CoStartGame: Exception:");
                DestroyableSingleton<DisconnectPopup>.Instance.ShowCustom(ex.Message);
                DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
            SoundManager.Instance.CrossFadeSound("MainBG", null, 0.5f, 1.5f);
            AmongUsClient.Instance.Connect(MatchMakerModes.HostAndClient, null);
            yield return AmongUsClient.Instance.WaitForConnectionOrFail();
            DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
        }
    }
}
