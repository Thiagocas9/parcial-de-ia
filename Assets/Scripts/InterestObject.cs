using UnityEngine;

namespace HuntingSimulation
{
    public sealed class InterestObject : MonoBehaviour
    {
        public float health = 45f;
        public static int ActiveCount { get; private set; }
        public static int DestroyedCount { get; private set; }
        public bool Available => health > 0 && isActiveAndEnabled;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetCounters() { ActiveCount = 0; DestroyedCount = 0; }
        void OnEnable() => ActiveCount++;
        void OnDisable() => ActiveCount--;
        public void Damage(float amount)
        {
            if (!Available) return;
            health = Mathf.Max(0, health - amount);
            if (health <= 0) { DestroyedCount++; gameObject.SetActive(false); Destroy(gameObject); }
        }
        public static InterestObject Spawn(Vector3 point)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Objeto de interes";
            go.layer = 8;
            go.transform.position = point;
            go.transform.localScale = new Vector3(1.2f, .6f, 1.2f);
            go.GetComponent<Renderer>().material.color = new Color(.98f, .72f, .18f);
            return go.AddComponent<InterestObject>();
        }
    }
}
