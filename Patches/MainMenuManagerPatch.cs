using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace FreeplayHns.Patches
{
    [HarmonyPatch(typeof(MainMenuManager), "Start")]
    internal static class MainMenuManagerPatch
    {
        public static void SetColor(PassiveButton button, Color color)
        {
            button.activeSprites.GetComponent<SpriteRenderer>().color = color;
            button.inactiveSprites.GetComponent<SpriteRenderer>().color = color;
        }
        public static void Postfix(MainMenuManager __instance)
        {
            PassiveButton button = GameObject.Instantiate<PassiveButton>(__instance.creditsButton, __instance.mainMenuUI.transform.GetChild(1).GetChild(3));
            button.transform.localScale = Vector3.one * 1.1f;
            button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            button.OnClick.AddListener(new Action(delegate
            {
                TutorialManagerPatch.AmCrewmate = true;
                AmongUsClient.Instance.TutorialMapId = 0;
                GameOptionsManager.Instance.SwitchGameMode(GameModes.HideNSeek);
                if (!NameTextBehaviour.IsValidName(DataManager.Player.Customization.Name))
                {
                    DataManager.Player.Customization.Name = "";
                    DataManager.Player.Save();
                }
                __instance.StartCoroutine(CoStartGame(__instance.mainMenuUI.transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>()).WrapToIl2Cpp());
            }));
            button.GetComponent<AspectPosition>().anchorPoint = new Vector2(0.2f, 0.89f);
            button.GetComponent<AspectPosition>().Update();
            TextMeshPro text = button.transform.GetChild(2).GetChild(0).GetComponent<TextMeshPro>();
            text.GetComponent<TextTranslatorTMP>().enabled = false;
            text.text = "Hide and Seek\nCrewmate";
            SetColor(button, Color.cyan);
            PassiveButton button2 = GameObject.Instantiate<PassiveButton>(__instance.creditsButton, __instance.mainMenuUI.transform.GetChild(1).GetChild(3));
            button2.transform.localScale = Vector3.one * 1.1f;
            button2.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            button2.OnClick.AddListener(new Action(delegate
            {
                TutorialManagerPatch.AmCrewmate = false;
                AmongUsClient.Instance.TutorialMapId = 0;
                GameOptionsManager.Instance.SwitchGameMode(GameModes.HideNSeek);
                if (!NameTextBehaviour.IsValidName(DataManager.Player.Customization.Name))
                {
                    DataManager.Player.Customization.Name = "";
                    DataManager.Player.Save();
                }
                __instance.StartCoroutine(CoStartGame(__instance.mainMenuUI.transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>()).WrapToIl2Cpp());
            }));
            button2.GetComponent<AspectPosition>().anchorPoint = new Vector2(0.2f, 0.8f);
            button2.GetComponent<AspectPosition>().Update();
            TextMeshPro text2 = button2.transform.GetChild(2).GetChild(0).GetComponent<TextMeshPro>();
            text2.GetComponent<TextTranslatorTMP>().enabled = false;
            text2.text = "Hide and Seek\nImpostor";
            SetColor(button2, Color.red);
        }
        public static System.Collections.IEnumerator CoStartGame(SpriteRenderer FillScreen)
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
            if (FillScreen)
            {
                SoundManager.Instance.CrossFadeSound("MainBG", null, 0.5f, 1.5f);
                FillScreen.gameObject.SetActive(true);
                for (float time = 0f; time < 0.25f; time += Time.deltaTime)
                {
                    FillScreen.color = Color.Lerp(Color.clear, Color.black, time / 0.25f);
                    yield return null;
                }
                FillScreen.color = Color.black;
            }
            AmongUsClient.Instance.Connect(MatchMakerModes.HostAndClient, null);
            yield return AmongUsClient.Instance.WaitForConnectionOrFail();
            if (AmongUsClient.Instance.mode == MatchMakerModes.None && FillScreen)
            {
                for (float time = 0f; time < 0.25f; time += Time.deltaTime)
                {
                    FillScreen.color = Color.Lerp(Color.black, Color.clear, time / 0.25f);
                    yield return null;
                }
                FillScreen.color = Color.clear;
            }
            DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
        }
    }
}
