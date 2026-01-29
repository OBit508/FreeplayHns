using FreeplayHns.Components;
using HarmonyLib;
using FreeplayHns;
using System;
using System.Collections.Generic;
using System.IO;  
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UnityEngine;

namespace FreeplayHns.Patches
{
    [HarmonyPatch(typeof(ShipStatus), "Awake")]
    internal static class ShipStatusPatch
    {
        public static NodesJson Nodes = null;
        public static void Postfix(ShipStatus __instance)
        {
            if (Nodes == null)
            {
                Nodes = JsonSerializer.Deserialize<NodesJson>(new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("FreeplayHns.Assets.SkeldPaths.json")).ReadToEnd());
            }
            Point.Points.Clear();
            foreach (string node in Nodes.Nodes)
            {
                string[] vec = node.Split("|");
                new Point(new Vector2(float.Parse(vec[0]), float.Parse(vec[1])), true, true, 1);
            }
            foreach (Vent vent in __instance.AllVents)
            {
                new VentPoint(vent, true, true, 1);
            }
            List<Point> ventPoints = Point.Points.FindAll(p => p is VentPoint);
            foreach (VentPoint ventPoint in ventPoints)
            {
                foreach (Vent vent in ventPoint.vent.NearbyVents)
                {
                    Point point = ventPoints.FirstOrDefault(p => p is VentPoint v && v.vent == vent);
                    if (point != null)
                    {
                        ventPoint.AvaibleFleePoints.Add(point);
                    }
                }
            }
        }
    }
    [Serializable]
    public class NodesJson
    {
        [JsonPropertyName("Nodes")]
        public List<string> Nodes { get; set; } = new List<string>();
    }
}
