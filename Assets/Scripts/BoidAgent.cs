using System.Collections;
using UnityEngine;

namespace HuntingSimulation
{
    public sealed class BoidAgent : AgentMotor
    {
        [Header("Sensores locales (360 grados)")]
        public float perceptionRadius = 6f;
        public float separationRadius = 1.8f;
        public float threatRadius = 7f;
        [Header("Flocking")]
        public float separationWeight = 2.6f;
        public float alignmentWeight = 1f;
        public float cohesionWeight = .8f;
        [Header("Interaccion y ciclo de vida")]
        public float maxHealth = 60f;
        public float interactionInterval = .7f;
        public float respawnDelay = 5f;
        public float Health { get; private set; }
        public bool Dead => Health <= 0;
        public bool Collected { get; private set; }
        public string Behaviour { get; private set; } = "Flocking";
        public int Neighbours { get; private set; }
        public static int Respawns { get; private set; }
        float nextInteraction;
        Vector3 wander;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetCounters() => Respawns = 0;
        protected override void Awake()
        {
            base.Awake();
            Health = maxHealth;
            wander = Quaternion.Euler(0, Random.Range(0f, 360f), 0) * Vector3.forward;
        }
        void OnValidate()
        {
            perceptionRadius = Mathf.Max(.5f, perceptionRadius);
            separationRadius = Mathf.Clamp(separationRadius, .1f, perceptionRadius - .1f);
        }
        void FixedUpdate()
        {
            if (Dead || Collected) return;
            Vector3 separation = Vector3.zero, alignment = Vector3.zero, center = Vector3.zero;
            HunterAgent threat = null;
            InterestObject interest = null;
            float threatDistance = float.PositiveInfinity, interestDistance = float.PositiveInfinity;
            Neighbours = 0;
            
            foreach (Collider hit in Physics.OverlapSphere(transform.position,
                         Mathf.Max(perceptionRadius, threatRadius), 1 << 8))
            {
                Vector3 offset = hit.transform.position - transform.position;
                float distance = offset.magnitude;
                if (hit.TryGetComponent(out HunterAgent hunter) && distance <= threatRadius && distance < threatDistance)
                { threat = hunter; threatDistance = distance; }
                if (hit.TryGetComponent(out InterestObject item) && item.Available && distance <= perceptionRadius && distance < interestDistance)
                { interest = item; interestDistance = distance; }
                if (!hit.TryGetComponent(out BoidAgent other) || other == this || other.Collected) continue;
                if (distance < separationRadius && distance > .001f)
                    separation -= offset.normalized / Mathf.Max(distance, .25f);
                if (other.Dead || distance > perceptionRadius) continue;
                Neighbours++;
                alignment += other.Velocity;
                center += other.transform.position;
            }
            Vector3 desired;
            if (threat != null)
            {
                Behaviour = "Evade";
                float prediction = Mathf.Min(1f, threatDistance / (speed + threat.speed));
                Vector3 escape = transform.position - (threat.transform.position + threat.Velocity * prediction);
                if (escape.sqrMagnitude < .01f) escape = wander;
               
                desired = escape.normalized * speed + separation * separationWeight;
                Tint(new Color(1f, .45f, .2f));
            }
            else if (interest != null)
            {
                Behaviour = "Arrive";
                Vector3 offset = interest.transform.position - transform.position;
                float arrivalSpeed = speed * Mathf.Clamp01((offset.magnitude - 1.35f) / 3f);
                desired = offset.normalized * arrivalSpeed + separation * separationWeight;
                Tint(new Color(1f, .84f, .34f));
                if (offset.magnitude <= 1.65f)
                {
                    Behaviour = "Interactuar";
                    if (Time.time >= nextInteraction)
                    { interest.Damage(10); nextInteraction = Time.time + interactionInterval; }
                }
            }
            else
            {
                Behaviour = "Flocking";
                desired = wander * speed * .65f;
                if (Neighbours > 0)
                    desired = alignment / Neighbours * alignmentWeight +
                        (center / Neighbours - transform.position).normalized * speed * cohesionWeight;
                desired += separation * separationWeight;
                Tint(new Color(.2f, .85f, .83f));
            }
            Move(desired);
        }
        public void Damage(float amount)
        {
            if (Dead || Collected) return;
            Health = Mathf.Max(0, Health - amount);
            if (!Dead) return;
            Stop(); body.isKinematic = true;
            Behaviour = "Eliminado";
            Tint(new Color(.38f, .42f, .5f));
        }
        public void Collect()
        {
            if (!Dead || Collected) return;
            Collected = true;
            Behaviour = "Reapareciendo";
            visual.enabled = false;
            GetComponent<Collider>().enabled = false;
            StartCoroutine(Respawn());
        }
        IEnumerator Respawn()
        {
            yield return new WaitForSeconds(respawnDelay);
            Vector3 point = RandomFreePosition();
            while (float.IsNaN(point.x)) { yield return new WaitForSeconds(.5f); point = RandomFreePosition(); }
            body.position = point;
            transform.position = point;
            body.isKinematic = false;
            Health = maxHealth; Collected = false;
            visual.enabled = true;
            GetComponent<Collider>().enabled = true;
            Respawns++;
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, perceptionRadius);
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, separationRadius);
            Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, threatRadius);
        }
    }
}
