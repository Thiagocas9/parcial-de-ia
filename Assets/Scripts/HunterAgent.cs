using System.Collections.Generic;
using UnityEngine;

namespace HuntingSimulation
{
    public sealed class HunterAgent : AgentMotor
    {
        public float TBA = 1.8f;
        public float RangeAttackRadius = 6f;
        public float MeleeAttackRadius = 2.4f;
        public float meleeContactRadius = 1.3f;
        public float perceptionRadius = 10f;
        public float gatherDuration = 1.5f;
        public float interestInterval = 5f;
        public Transform[] waypoints;
        public BoidAgent Target { get; internal set; }
        public HunterFSM FSM { get; private set; }
        public float NextAttackTime { get; private set; }
        public float Cooldown => Mathf.Max(0, NextAttackTime - Time.time);
        public string LastAction { get; private set; } = "Iniciando patrulla";
        public int Attacks { get; private set; }
        public int Gathered { get; private set; }
        public List<BoidAgent> Detected { get; } = new List<BoidAgent>();
        float nextInterest;
        int waypointIndex;
        LineRenderer shot;
        float shotUntil;

        protected override void Awake()
        {
            base.Awake();
            FSM = new HunterFSM(this);
            shot = gameObject.AddComponent<LineRenderer>();
            shot.material = new Material(Shader.Find("Sprites/Default"));
            shot.startColor = shot.endColor = new Color(1, .3f, .4f);
            shot.startWidth = shot.endWidth = .1f;
            shot.positionCount = 2; shot.enabled = false;
        }
        void FixedUpdate()
        {
            Sense();
            FSM.Tick(Time.fixedDeltaTime);
            if (Time.time >= nextInterest)
            {
                nextInterest = Time.time + interestInterval;
                if (InterestObject.ActiveCount < 5)
                {
                    Vector3 point = RandomFreePosition();
                    if (!float.IsNaN(point.x)) InterestObject.Spawn(point);
                }
            }
            shot.enabled = Time.time < shotUntil;
        }
        void Sense()
        {
            Detected.Clear();
            foreach (Collider hit in Physics.OverlapSphere(transform.position, perceptionRadius, 1 << 8))
                if (hit.TryGetComponent(out BoidAgent boid) && !boid.Collected &&
                    Vector3.Distance(transform.position, boid.transform.position) <= perceptionRadius) Detected.Add(boid);
        }
        public bool Visible(BoidAgent boid) => boid != null && !boid.Collected && Detected.Contains(boid);
        public BoidAgent Nearest(bool dead)
        {
            BoidAgent result = null; float best = float.PositiveInfinity;
            foreach (var boid in Detected)
            {
                if (boid.Dead != dead) continue;
                float distance = Vector3.SqrMagnitude(boid.transform.position - transform.position);
                if (distance < best) { result = boid; best = distance; }
            }
            return result;
        }
        public float TargetDistance => Target == null ? float.PositiveInfinity : Vector3.Distance(transform.position, Target.transform.position);
        public void Pursue() => Move((Target.transform.position - transform.position).normalized * speed);
        public void Patrol()
        {
            Tint(new Color(.6f, .45f, 1f));
            if (waypoints == null || waypoints.Length == 0) { Stop(); return; }
            Vector3 offset = waypoints[waypointIndex].position - transform.position; offset.y = 0;
            if (offset.magnitude < .8f) waypointIndex = (waypointIndex + 1) % waypoints.Length;
            Move(offset.normalized * speed);
        }
        public void AttackFeedback() => Tint(new Color(1f, .2f, .35f));
        public void GatherFeedback() => Tint(new Color(.4f, 1f, .4f));
        public void PerformAttack(bool melee)
        {
            Stop(); Target.Damage(melee ? 60 : 30);
            NextAttackTime = Time.time + TBA; Attacks++;
            LastAction = (melee ? "Golpe cuerpo a cuerpo: " : "Disparo a distancia: ") + Target.name;
            shot.SetPosition(0, transform.position + Vector3.up * .4f);
            shot.SetPosition(1, Target.transform.position + Vector3.up * .4f);
            shotUntil = Time.time + .22f;
        }
        public void FinishGather()
        {
            LastAction = "Recolectado: " + Target.name;
            Target.Collect(); Gathered++;
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(transform.position, perceptionRadius);
            Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, RangeAttackRadius);
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, MeleeAttackRadius);
        }
    }
}
