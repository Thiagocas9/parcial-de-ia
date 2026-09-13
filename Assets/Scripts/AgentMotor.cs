using UnityEngine;

namespace HuntingSimulation
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class AgentMotor : MonoBehaviour
    {
        public float speed = 4f;
        public float acceleration = 12f;
        protected Rigidbody body;
        protected Renderer visual;
        public Vector3 Velocity => body.linearVelocity;
        public const float ArenaHalfSize = 19f;

        protected virtual void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            visual = GetComponent<Renderer>();
        }

        protected void Move(Vector3 desired)
        {
            desired.y = 0;
            Vector3 p = body.position;
            if (Mathf.Abs(p.x) > ArenaHalfSize - 2) desired.x += -Mathf.Sign(p.x) * speed * 2;
            if (Mathf.Abs(p.z) > ArenaHalfSize - 2) desired.z += -Mathf.Sign(p.z) * speed * 2;
            body.linearVelocity = Vector3.MoveTowards(body.linearVelocity,
                Vector3.ClampMagnitude(desired, speed), acceleration * Time.fixedDeltaTime);
            if (body.linearVelocity.sqrMagnitude > .05f)
                transform.rotation = Quaternion.LookRotation(body.linearVelocity);
        }

        public void Stop() { body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero; }
        protected void Tint(Color color) => visual.material.color = color;
        public static Vector3 RandomFreePosition()
        {
            for (int i = 0; i < 200; i++)
            {
                Vector3 point = new Vector3(Random.Range(-16f, 16f), .6f, Random.Range(-16f, 16f));
                if (Physics.OverlapSphere(point, .85f, 1 << 8).Length == 0) return point;
            }
            
            return new Vector3(float.NaN, 0, 0);
        }
    }
}
