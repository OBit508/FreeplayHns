using AmongUs.Data;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeplayHns.Patches
{
    [HarmonyPatch(typeof(LogicGameFlowHnS), "CheckEndCriteria")]
    internal static class LogicGameFlowHnSPatch
    {
        public static bool Prefix(LogicGameFlowHnS __instance)
        {
            if (!GameData.Instance)
            {
                return false;
            }
            ValueTuple<int, int, int> playerCounts = GetPlayerCounts();
            int item = playerCounts.Item1;
            int item2 = playerCounts.Item2;
            if (item2 <= 0)
            {
                __instance.Manager.RpcEndGame(GameOverReason.ImpostorDisconnect, !DataManager.Player.Ads.HasPurchasedAdRemoval);
            }
            if (item > 0)
            {
                if (__instance.AllTimersExpired())
                {
                    __instance.Manager.RpcEndGame(GameOverReason.HideAndSeek_CrewmatesByTimer, !DataManager.Player.Ads.HasPurchasedAdRemoval);
                }
                return false;
            }
            __instance.Manager.RpcEndGame(GameOverReason.HideAndSeek_ImpostorsByKills, !DataManager.Player.Ads.HasPurchasedAdRemoval);
            return false;
        }
        public static ValueTuple<int, int, int> GetPlayerCounts()
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            for (int i = 0; i < GameData.Instance.PlayerCount; i++)
            {
                NetworkedPlayerInfo networkedPlayerInfo = GameData.Instance.AllPlayers[i];
                if (!(networkedPlayerInfo == null) && !networkedPlayerInfo.Disconnected && !(networkedPlayerInfo.Role == null))
                {
                    if (networkedPlayerInfo.Role.IsImpostor)
                    {
                        num3++;
                    }
                    if (!networkedPlayerInfo.IsDead)
                    {
                        if (networkedPlayerInfo.Role.IsImpostor)
                        {
                            num2++;
                        }
                        else
                        {
                            num++;
                        }
                    }
                    else
                    {
                        ImpostorGhostRole impostorGhostRole = networkedPlayerInfo.Role as ImpostorGhostRole;
                        if (impostorGhostRole != null && impostorGhostRole.WasManuallyPicked)
                        {
                            num2++;
                        }
                    }
                }
            }
            return new ValueTuple<int, int, int>(num, num2, num3);
        }
    }
}
