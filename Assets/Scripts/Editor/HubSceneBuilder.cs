using Oasis.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Oasis.EditorTools
{
    /// <summary>
    /// Génère la scène du hub de zéro : architecture, éclairage, rig VR,
    /// confort et point d'apparition. Relançable à volonté.
    /// </summary>
    public static class HubSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Hub.unity";

        private const string RigPrefabPath =
            "Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

        [MenuItem("Oasis/Générer la scène Hub")]
        public static void BuildHub()
        {
            var rig = AssetDatabase.LoadAssetAtPath<GameObject>(RigPrefabPath);
            if (rig == null)
            {
                Debug.LogError("HubSceneBuilder : rig introuvable a " + RigPrefabPath + ", rien fait.");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            ConfigureLighting();
            HubEnvironmentBuilder.Build();
            PrefabUtility.InstantiatePrefab(rig, scene);
            ComfortSetup.AddVignette();
            CreateSpawnPoint();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();

            Debug.Log("HubSceneBuilder : scene creee a " + ScenePath + ".");
        }

        private static void ConfigureLighting()
        {
            var go = new GameObject("Soleil");
            go.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.98f, 0.95f);
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;

            // Sans ambiante remontee, un decor blanc vire au gris sale : les
            // faces qui ne recoivent pas le soleil n'ont plus rien.
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.25f;
        }

        private static void CreateSpawnPoint()
        {
            var go = new GameObject("Spawn Point");
            // Legerement en retrait du centre, face aux portails.
            go.transform.SetPositionAndRotation(new Vector3(0f, 0f, -6f), Quaternion.identity);
            go.AddComponent<SpawnPoint>();
            go.AddComponent<PlayerSpawner>();
        }
    }
}
