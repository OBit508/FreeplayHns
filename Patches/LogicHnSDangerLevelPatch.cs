using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeplayHns.Patches
{
    [HarmonyPatch(typeof(LogicHnSDangerLevel), "OnGameStart")]
    internal static class LogicHnSDangerLevelPatch
    {
        public static bool Prefix(LogicHnSDangerLevel __instance)
        {
            TutorialManagerPatch.CreatePlayers();
            HudManager.Instance.CrewmatesKilled.gameObject.SetActive(true);
            __instance.firstMusicActivation = true;
            if (!PlayerControl.LocalPlayer.Data.Role.IsImpostor)
            {
                __instance.dangerMeter = HudManager.Instance.transform.GetChild(4).GetChild(0).GetComponent<DangerMeter>();
                __instance.dangerMeter.transform.parent.gameObject.SetActive(true);
                __instance.dangerMeter.gameObject.SetActive(true);
            }
            __instance.impostors = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls)
            {
                NetworkedPlayerInfo data = playerControl.Data;
                if (((data != null) ? data.Role : null) != null && playerControl.Data.Role.IsImpostor)
                {
                    __instance.impostors.Add(playerControl);
                }
            }
            __instance.scaryMusicDistance = __instance.hnsManager.LogicOptionsHnS.GetScaryMusicDistance() * __instance.hnsManager.LogicOptionsHnS.PlayerSpeedBase;
            __instance.veryScaryMusicDistance = __instance.hnsManager.LogicOptionsHnS.GetVeryScaryMusicDistance() * __instance.hnsManager.LogicOptionsHnS.PlayerSpeedBase;
            if (__instance.scaryMusicDistance < __instance.veryScaryMusicDistance)
            {
                float num = __instance.veryScaryMusicDistance;
                float num2 = __instance.scaryMusicDistance;
                __instance.scaryMusicDistance = num;
                __instance.veryScaryMusicDistance = num2;
            }
            return false;
        }
    }
}
