using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppSystem.Threading.Tasks;
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
        private bool fleeing = false;
        private float fleeRecalcTimer = 0f;
        public float taskTimer;
        public NormalPlayerTask task;
        public int VentUses;
        public int MaxVentUses = 3;
        public void Start()
        {
            Player = GetComponent<PlayerControl>();
            StartCoroutine(CoStart().WrapToIl2Cpp());
            StartCoroutine(MoveAlongPath().WrapToIl2Cpp());
            lastPos = transform.position;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.Role.IsImpostor)
                {
                    Impostors.Add(player);
                }
            }
        }
        public void Update()
        {
            if (!Player.Data.IsDead)
            {
                if (task == null)
                {
                    DetectStuck();
                }
                TryDetectImpostors();
            }
        }
        public void TryDetectImpostors()
        {
            PlayerControl nearest = null;
            float bestDist = float.MaxValue;
            foreach (PlayerControl imp in Impostors)
            {
                if (imp != null && imp.moveable)
                {
                    float d = Vector2.Distance(transform.position, imp.transform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        nearest = imp;
                    }
                }
            }
            if (nearest != null && bestDist <= 4.5f)
            {
                fleeing = true;
                fleeRecalcTimer -= Time.deltaTime;
                if (fleeRecalcTimer <= 0 || Path == null || Path.Count == 0)
                {
                    RecalculateFleePath(nearest.transform.position);
                    fleeRecalcTimer = 0.4f;
                }
            }
            else
            {
                if (fleeing)
                {
                    if (Player.myTasks.Count > 0)
                    {
                        RecalculateTaskPath();
                    }
                    else
                    {
                        RecalculateRandomPath();
                    }
                }
                fleeing = false;
            }
        }
        public void DoTask()
        {
            if (task != null)
            {
                taskTimer -= Time.deltaTime;
                if (taskTimer <= 0)
                {
                    task.Owner.CompleteTask(task.Id);
                    task = null;
                    RecalculateRandomPath();
                }
            }
        }
        public System.Collections.IEnumerator MoveAlongPath()
        {
            while (!Player.Data.IsDead)
            {
                if (Path == null || Path.Count == 0)
                {
                    if (task != null)
                    {
                        DoTask();
                    }
                    Player.rigidbody2D.velocity = Vector2.zero;
                }
                else
                {
                    Point target = Path[0];
                    if (target is VentPoint ventPoint && Path.Count > 1)
                    {
                        Point next = Path[1];
                        if (next is VentPoint nextVent)
                        {
                            yield return Player.MyPhysics.CoEnterVent(ventPoint.vent.Id);
                            Player.NetTransform.SnapTo(nextVent.Position);
                            yield return Player.MyPhysics.CoExitVent(nextVent.vent.Id);
                            VentUses++;
                            TryDetectImpostors();
                        }
                        else
                        {
                            Player.rigidbody2D.velocity = (target.Position - (Vector2)transform.position).normalized * Player.MyPhysics.TrueSpeed;
                            if (Vector2.Distance(transform.position, target.Position) < 0.35f)
                            {
                                Path.RemoveAt(0);
                            }
                        }
                    }
                    else
                    {
                        Player.rigidbody2D.velocity = (target.Position - (Vector2)transform.position).normalized * Player.MyPhysics.TrueSpeed;
                        if (Vector2.Distance(transform.position, target.Position) < 0.35f)
                        {
                            Path.RemoveAt(0);
                        }
                    }
                }
                yield return null;
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
                    {
                        RecalculateFleePath(nearest.transform.position);
                    }
                }
                else if (task != null)
                {
                    RecalculateTaskPath();
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
            task = null;
            try
            {
                Vector2 fleePos = (Vector2)transform.position + ((Vector2)transform.position - impostorPos).normalized * 8f;
                Point best = null;
                float bestDist = float.MaxValue;
                foreach (var p in Point.Points.FindAll(p => p.GetPoints(fleeing).Count > 2))
                {
                    float d = Vector2.Distance(p.Position, fleePos);
                    if (d < bestDist)
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
                Path = Point.FindPath(Point.GetClosestPoint(transform.position), best, fleeing && MaxVentUses > VentUses);
                if (Path != null && Path.Count > 0)
                {
                    Path.RemoveAt(0);
                }
            }
            catch { RecalculateRandomPath(); }
        }
        public void RecalculateRandomPath()
        {
            task = null;
            try
            {
                Path = Point.FindPath(Point.GetClosestPoint(transform.position), Point.Points[new System.Random().Next(0, Point.Points.Count)], fleeing && MaxVentUses > VentUses);
                if (Path != null && Path.Count > 0)
                {
                    Path.RemoveAt(0);
                }
            }
            catch { }
        }
        public void RecalculateTaskPath()
        {
            try
            {
                task = GetClosestTask();
                if (task != null)
                {
                    taskTimer = 2.5f;
                    Path = Point.FindPath(Point.GetClosestPoint(transform.position), Point.GetClosestPoint(task.Locations[0]), fleeing && MaxVentUses > VentUses);
                    if (Path != null && Path.Count > 0)
                    {
                        Path.RemoveAt(0);
                    }
                    return;
                }
                RecalculateRandomPath();
            }
            catch { RecalculateRandomPath(); task = null; }
        }
        public NormalPlayerTask GetClosestTask(float dis = float.MaxValue)
        {
            NormalPlayerTask task = null;
            foreach (PlayerTask t in Player.myTasks)
            {
                if (t.HasLocation && t != task)
                {
                    NormalPlayerTask normalPlayerTask = t.TryCast<NormalPlayerTask>();
                    if (normalPlayerTask != null)
                    {
                        float distance = Vector2.Distance(normalPlayerTask.Locations[0], transform.position);
                        if (dis > distance)
                        {
                            dis = distance;
                            task = normalPlayerTask;
                        }
                    }
                }
            }
            return task;
        }
        public System.Collections.IEnumerator CoStart()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.25f);
                if (!fleeing && (Path == null || Path.Count == 0) && task == null)
                {
                    if (Player.myTasks.Count > 0)
                    {
                        RecalculateTaskPath();
                    }
                    else
                    {
                        RecalculateRandomPath();
                    }
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
