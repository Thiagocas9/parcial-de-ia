using UnityEngine;

namespace HuntingSimulation
{
    public sealed class SimulationHUD : MonoBehaviour
    {
        public HunterAgent hunter;
        public BoidAgent[] boids;
        bool paused;
        GUIStyle title, text, small;
        void OnGUI()
        {
            if (hunter == null) return;
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
                text = new GUIStyle(GUI.skin.label) { fontSize = 15 };
                small = new GUIStyle(GUI.skin.label) { fontSize = 12 };
                title.normal.textColor = new Color(.25f, .9f, .85f);
            }
            GUI.Box(new Rect(14, 14, 320, 470), "");
            GUILayout.BeginArea(new Rect(30, 28, 288, 445));
            GUILayout.Label("BOIDS / HUNTER", title);
            GUILayout.Label("SIMULACION AUTONOMA · UNITY 6.3", small);
            GUILayout.Space(16);
            GUILayout.Label("CAZADOR  /  " + hunter.FSM.Current, text);
            GUILayout.Label("Objetivo: " + (hunter.Target != null ? hunter.Target.name : "—"), text);
            GUILayout.Label("Detectados: " + hunter.Detected.Count + "   |   TBA: " + hunter.Cooldown.ToString("F1") + " s", text);
            GUILayout.Label("Intereses activos: " + InterestObject.ActiveCount + " / 5", text);
            GUILayout.Label("Ataques: " + hunter.Attacks + "   Recolectados: " + hunter.Gathered, text);
            if (hunter.FSM.Current == HunterFSM.State.Gather)
                GUILayout.Label("Recoleccion: " + (100 * hunter.FSM.GatherProgress / hunter.gatherDuration).ToString("F0") + "%", text);
            GUILayout.Space(6);
            GUILayout.Label(hunter.LastAction, small);
            GUILayout.Space(12);
            foreach (var boid in boids)
                GUILayout.Label(boid.name + "  |  " + boid.Health.ToString("F0") + " PV  |  " + boid.Behaviour, small);
            GUILayout.Space(12);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(paused ? "Continuar" : "Pausar"))
            { paused = !paused; Time.timeScale = paused ? 0 : 1; }
            if (GUILayout.Button("1x")) { paused = false; Time.timeScale = 1; }
            if (GUILayout.Button("3x")) { paused = false; Time.timeScale = 3; }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.Label(new Rect(350, Screen.height - 34, 900, 26),
                "CIAN Flocking   /   NARANJA Evade   /   AMARILLO Arrive   /   GRIS Eliminado   /   VERDE Gather", small);
            Camera camera = Camera.main;
            if (camera == null) return;
            foreach (var boid in boids)
            {
                if (boid.Collected) continue;
                Vector3 p = camera.WorldToScreenPoint(boid.transform.position + Vector3.up);
                if (p.z > 0) GUI.Label(new Rect(p.x - 40, Screen.height - p.y - 12, 100, 20),
                    boid.name + " · " + boid.Health.ToString("F0"), small);
            }
        }
    }
}
