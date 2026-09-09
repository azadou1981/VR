using Oasis.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace Oasis.EditorTools
{
    /// <summary>
    /// Génère la scène du hub : sol téléportable, lumière, rig VR et point
    /// d'apparition. Squelette seulement — les portails viendront en phase 3.
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

            CreateLight();
            CreateGround();
            PrefabUtility.InstantiatePrefab(rig, scene);
            ComfortSetup.AddVignette();
            CreateSpawnPoint();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            Debug.Log("HubSceneBuilder : scene creee a " + ScenePath + ".");
        }

        private static void CreateLight()
        {
            var go = new GameObject("Soleil");
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.shadows = LightShadows.Soft;
        }

        private static void CreateGround()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = "Sol";
            // Le Plane fait 10 m de cote : x5 donne 50 m, large pour un hub.
            go.transform.localScale = new Vector3(5f, 1f, 5f);

            // Sans zone de teleportation, le rig ne peut pas se deplacer.
            go.AddComponent<TeleportationArea>();
        }

        private static void CreateSpawnPoint()
        {
            var go = new GameObject("Spawn Point");
            go.transform.position = Vector3.zero;
            go.AddComponent<SpawnPoint>();
            go.AddComponent<PlayerSpawner>();
        }
    }
}
