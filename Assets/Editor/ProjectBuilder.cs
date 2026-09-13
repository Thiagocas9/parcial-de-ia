using System.IO;
using HuntingSimulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ProjectBuilder
{
    [MenuItem("Simulacion/Crear escena de demostracion")]
    public static void BuildScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Materials");
        Random.InitState(42);
        RenderSettings.ambientLight = new Color(.65f, .7f, .8f);
        var floor = Primitive("Arena", PrimitiveType.Cube, new Vector3(0, -.3f, 0),
            new Vector3(40, .5f, 40), new Color(.075f, .12f, .18f));
        for (int axis = 0; axis < 2; axis++)
        for (int sign = -1; sign <= 1; sign += 2)
            Primitive("Limite", PrimitiveType.Cube, axis == 0 ? new Vector3(sign * 20, .5f, 0) : new Vector3(0, .5f, sign * 20),
                axis == 0 ? new Vector3(.5f, 2, 40) : new Vector3(40, 2, .5f), new Color(.15f, .28f, .36f));
        for (int i = -18; i <= 18; i += 3)
        {
            var line = Primitive("Grilla", PrimitiveType.Cube, new Vector3(i, -.038f, 0), new Vector3(.025f, .01f, 39), new Color(.13f, .2f, .26f));
            Object.DestroyImmediate(line.GetComponent<Collider>());
            line = Primitive("Grilla", PrimitiveType.Cube, new Vector3(0, -.038f, i), new Vector3(39, .01f, .025f), new Color(.13f, .2f, .26f));
            Object.DestroyImmediate(line.GetComponent<Collider>());
        }
        var camera = new GameObject("Main Camera", typeof(Camera));
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(-7, 38, -25);
        camera.transform.rotation = Quaternion.Euler(57, 0, 0);
        camera.GetComponent<Camera>().orthographic = true;
        camera.GetComponent<Camera>().orthographicSize = 25;
        camera.GetComponent<Camera>().backgroundColor = new Color(.035f, .055f, .09f);
        camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        var light = new GameObject("Sol", typeof(Light));
        light.GetComponent<Light>().type = LightType.Directional;
        light.GetComponent<Light>().intensity = 1.15f;
        light.transform.rotation = Quaternion.Euler(55, -30, 0);
        var route = new GameObject("Waypoints");
        Transform[] points = new Transform[4];
        Vector3[] positions = { new Vector3(-8,.6f,-8), new Vector3(8,.6f,-8), new Vector3(8,.6f,8), new Vector3(-8,.6f,8) };
        for (int i = 0; i < 4; i++)
        {
            var marker = Primitive("Waypoint " + (i + 1), PrimitiveType.Cylinder, positions[i] - Vector3.up * .58f,
                new Vector3(1.8f, .03f, 1.8f), new Color(.35f, .27f, .6f));
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.transform.SetParent(route.transform);
            points[i] = marker.transform;
        }
        var hunterObject = Primitive("Cazador", PrimitiveType.Sphere, positions[0], Vector3.one * 1.2f, new Color(.6f, .45f, 1));
        hunterObject.layer = 8;
        var hunter = hunterObject.AddComponent<HunterAgent>();
        hunter.waypoints = points;
        hunter.speed = 5.2f;
        var flock = new GameObject("Boids independientes");
        BoidAgent[] agents = new BoidAgent[8];
        for (int i = 0; i < agents.Length; i++)
        {
            var go = Primitive("Boid " + (i + 1).ToString("00"), PrimitiveType.Sphere,
                new Vector3(-5 + i % 4 * 2.5f, .6f, -1 + i / 4 * 2.5f), Vector3.one, new Color(.2f, .85f, .83f));
            go.layer = 8; go.transform.SetParent(flock.transform);
            agents[i] = go.AddComponent<BoidAgent>();
        }
        var hud = new GameObject("Panel de observacion").AddComponent<SimulationHUD>();
        hud.hunter = hunter; hud.boids = agents;
        PlayerSettings.companyName = "Proyecto IA";
        PlayerSettings.productName = "Boids y Cazador";
        PlayerSettings.defaultScreenWidth = 1440;
        PlayerSettings.defaultScreenHeight = 900;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Simulacion.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Simulacion.unity", true) };
        AssetDatabase.SaveAssets();
        Debug.Log("SCENE_BUILD_OK Unity " + Application.unityVersion);
    }
    static GameObject Primitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name; go.transform.position = position; go.transform.localScale = scale;
        string path = "Assets/Materials/Color_" + ColorUtility.ToHtmlStringRGB(color) + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard")); material.color = color;
            AssetDatabase.CreateAsset(material, path);
        }
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }
    public static void RunSmoke()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Simulacion.unity");
        EditorApplication.isPlaying = true;
    }
    public static void CapturePreview()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Simulacion.unity");
        var camera = Camera.main;
        var target = new RenderTexture(1440, 900, 24);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        var image = new Texture2D(1440, 900, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1440, 900), 0, 0);
        image.Apply();
        File.WriteAllBytes("Vista-previa.png", image.EncodeToPNG());
        camera.targetTexture = null; RenderTexture.active = null;
        Object.DestroyImmediate(image); Object.DestroyImmediate(target);
        Debug.Log("PREVIEW_OK");
    }
}
