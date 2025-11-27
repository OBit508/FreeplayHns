using BepInEx.Unity.IL2CPP.Utils.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FreeplayHns.Components
{
    internal class CrewmateComp : MonoBehaviour
    {
        public List<Point> Path;
        public PlayerControl Player;
        public List<PlayerControl> Impostors = new List<PlayerControl>();
        private Vector2 lastPos;
        private float stuckTimer = 0f;
        private float dangerDistance = 4.5f;
        private bool fleeing = false;
        private float fleeRecalcTimer = 0f;
        public bool FirstRun;
        public void Start()
        {
            Player = GetComponent<PlayerControl>();
            StartCoroutine(CoStart().WrapToIl2Cpp());
            lastPos = transform.position;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.Role.IsImpostor)
                {
                    Impostors.Add(player);
                }
            }
            RecalculateRandomPath();
            FirstRun = true;
        }
        public void Update()
        {
            if (!Player.Data.IsDead)
            {
                MoveAlongPath();
                DetectStuck();
                if (!FirstRun)
                {
                    TryDetectImpostors();
                }
            }
        }
        public void TryDetectImpostors()
        {
            PlayerControl nearest = null;
            float bestDist = float.MaxValue;
            foreach (var imp in Impostors)
            {
                if (imp != null)
                {
                    float d = Vector2.Distance(transform.position, imp.transform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        nearest = imp;
                    }
                }
            }
            if (nearest != null && bestDist <= dangerDistance)
            {
                fleeing = true;
                fleeRecalcTimer -= Time.deltaTime;
                if (fleeRecalcTimer <= 0f || Path == null || Path.Count == 0)
                {
                    RecalculateFleePath(nearest.transform.position);
                    fleeRecalcTimer = 0.4f;
                }
            }
            else
            {
                fleeing = false;
            }
        }
        public void MoveAlongPath()
        {
            if (Path == null || Path.Count == 0)
            {
                Player.rigidbody2D.velocity = Vector2.zero;
                return;
            }
            Vector2 target = Path[0].Position;
            Player.rigidbody2D.velocity = (target - (Vector2)transform.position).normalized * Player.MyPhysics.Speed;
            if (Vector2.Distance(transform.position, target) < 0.35f)
            {
                Path.RemoveAt(0);
            }
        }
        public void DetectStuck()
        {
            if (Vector2.Distance(transform.position, lastPos) < 0.015f)
            {
                stuckTimer += Time.deltaTime;
            }
            else
            {
                stuckTimer = 0f;
            }
            lastPos = transform.position;
            if (stuckTimer > 0.8f)
            {
                if (fleeing)
                {
                    PlayerControl nearest = GetClosestImpostor();
                    if (nearest != null)
                        RecalculateFleePath(nearest.transform.position);
                }
                else
                {
                    RecalculateRandomPath();
                }
                stuckTimer = 0f;
            }
        }
        public void RecalculateFleePath(Vector2 impostorPos)
        {
            try
            {
                var start = Point.GetClosestPoint(transform.position);
                Point best = null;
                float bestDist = -1f;
                foreach (var p in Point.Points)
                {
                    float d = Vector2.Distance(p.Position, impostorPos);
                    if (d > bestDist)
                    {
                        bestDist = d;
                        best = p;
                    }
                }
                if (best == null)
                {
                    RecalculateRandomPath();
                    return;
                }
                Path = Point.FindPath(start, best);
                if (Path != null && Path.Count > 0)
                {
                    Path.RemoveAt(0);
                }
            }
            catch { }
        }
        public void RecalculateRandomPath()
        {
            FirstRun = false;
            try
            {
                Path = Point.FindPath(Point.GetClosestPoint(transform.position), Point.Points[new System.Random().Next(0, Point.Points.Count)]);
                if (Path != null && Path.Count > 0)
                {
                    Path.RemoveAt(0);
                }
            }
            catch { }
        }
        public System.Collections.IEnumerator CoStart()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.25f);
                if (!fleeing && (Path == null || Path.Count == 0))
                {
                    RecalculateRandomPath();
                    yield return null;
                }
            }
        }
        public PlayerControl GetClosestImpostor()
        {
            PlayerControl nearest = null;
            float best = float.MaxValue;
            foreach (var imp in Impostors)
            {
                float d = Vector2.Distance(transform.position, imp.transform.position);
                if (d < best)
                {
                    best = d;
                    nearest = imp;
                }
            }
            return nearest;
        }
    }
}
