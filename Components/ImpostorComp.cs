using AmongUs.Data;
using FreeplayHns.Patches;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using InnerNet;
using PowerTools;
using FreeplayHns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

namespace FreeplayHns.Components
{
    public class ImpostorComp : MonoBehaviour
    {
        public List<Point> Path;
        public PlayerControl Player;
        public PlayerControl Target;
        private float recalcTimer = 0f;
        private float stuckTimer = 0f;
        private Vector2 lastPos;
        public void Start()
        {
            Player = GetComponent<PlayerControl>();
            StartCoroutine(CoStart().WrapToIl2Cpp());
            lastPos = transform.position;
        }
        public void Update()
        {
            if (Player.moveable)
            {
                UpdateTargetDetection();
                UpdateMovement();
                DetectStuck();
            }
        }
        public void UpdateTargetDetection()
        {
            if (Target != null && Target.Data.IsDead)
            {
                Target = null;
            }
            if (Player.moveable)
            {
                PlayerControl bestTarget = null;
                bool finalTimer = GameManager.Instance.LogicFlow.Cast<LogicGameFlowHnS>().beepCoroutine == null;
                float bestDist = finalTimer ? 6 : float.MaxValue;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.Role.IsImpostor && !p.inVent && !p.Data.IsDead)
                    {
                        float d = Vector2.Distance(transform.position, p.transform.position);
                        if (d <= bestDist && (!Point.AnythingBetween(transform.position, p.transform.position) || finalTimer))
                        {
                            bestTarget = p;
                            bestDist = d;
                        }
                    }
                }
                if (bestTarget != null)
                {
                    Target = bestTarget;
                }
            }
        }
        public float GetPathDistance()
        {
            float distance = 0;
            Point last = null;
            foreach (Point point in Path)
            {
                distance += Vector2.Distance(point.Position, last == null ? transform.position : last.Position);
                last = point;
            }
            return distance;
        }
        public void UpdateMovement()
        {
            if (Path == null || Path.Count == 0 || !Player.moveable)
            {
                Player.rigidbody2D.velocity = Vector2.zero;
                return;
            }
            Player.rigidbody2D.velocity = (Path[0].Position - (Vector2)transform.position).normalized * Player.MyPhysics.TrueSpeed;
            if (Vector2.Distance(transform.position, Path[0].Position) <= 0.35f)
            {
                Path.RemoveAt(0);
            }
        }
        public void DetectStuck()
        {
            float moved = Vector2.Distance(transform.position, lastPos);
            if (moved < 0.01f)
            {
                stuckTimer += Time.deltaTime;
            }
            else
            {
                stuckTimer = 0f;
            }
            lastPos = transform.position;
            if (stuckTimer > 0.7f)
            {
                if (Target != null)
                {
                    Path = Point.FindPath(Point.GetClosestPoint(transform.position, true), Point.GetClosestPoint(Target.transform.position, true));
                }
                stuckTimer = 0f;
            }
        }
        public System.Collections.IEnumerator CoHunt()
        {
            yield return new WaitForSeconds(0.2f);
            while (true)
            {
                yield return new WaitForSeconds(0.2f);
                if (Target != null && Player.moveable)
                {
                    if (Target.inVent)
                    {
                        Target = null;
                        yield return null;
                    }
                    if (GetPathDistance() > 5)
                    {
                        yield return new WaitForSeconds(1);
                        if (Target != null && GetPathDistance() > 5)
                        {
                            Target = null;
                            yield return null;
                        }
                    }
                }
            }
        }
        public System.Collections.IEnumerator CoKill()
        {
            yield return new WaitForSeconds(0.2f);
            while (true)
            {
                yield return new WaitForSeconds(0.2f);
                if (Player.moveable && Target != null && !Target.Data.IsDead && Vector2.Distance(transform.position, Target.transform.position) <= 0.8f)
                {
                    Player.MurderPlayer(Target, MurderResultFlags.Succeeded);
                    Target = null;
                    yield return new WaitForSeconds(1);
                }
                yield return null;
            }
        }
        public System.Collections.IEnumerator CoStart()
        {
            yield return CoDoAnimation(Player);
            StartCoroutine(CoHunt().WrapToIl2Cpp());
            StartCoroutine(CoKill().WrapToIl2Cpp());
            yield return new WaitForSeconds(0.2f);
            Player.moveable = true;
            while (true)
            {
                yield return new WaitForSeconds(0.25f);
                try
                {
                    if (Target != null)
                    {
                        if (recalcTimer <= 0f || Vector2.Distance((Vector2)lastPos, Target.transform.position) > 0.6f)
                        {
                            Path = Point.FindPath(Point.GetClosestPoint(transform.position, true), Point.GetClosestPoint(Target.transform.position, true));
                            if (Path != null && Path.Count > 0)
                            {
                                Path.RemoveAt(0);
                            }
                            recalcTimer = 0.4f;
                        }
                    }
                    else
                    {
                        if (Path == null || Path.Count == 0)
                        {
                            Path = Point.FindPath(Point.GetClosestPoint(transform.position, true), Point.Points[new System.Random().Next(0, Point.Points.Count - 1)]);
                            if (Path != null && Path.Count > 0)
                            {
                                Path.RemoveAt(0);
                            }
                        }
                    }
                    recalcTimer -= 0.25f;
                }
                catch { }
            }
        }
        public static System.Collections.IEnumerator CoDoAnimation(PlayerControl Player)
        {
            yield return new WaitForSeconds(0.1f);
            yield return Player.MyPhysics.CoAnimateCustom(HudManager.Instance.IntroPrefab.HnSSeekerSpawnAnim);
        }
    }
}