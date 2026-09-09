using Oasis.Core;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Oasis.EditorTools
{
    /// <summary>
    /// Outils d'éditeur pour préparer une scène sans manipulation manuelle.
    /// Exécutable en mode batch, donc sans ouvrir l'Éditeur.
    /// </summary>
    public static class SceneSetup
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string SpawnObjectName = "Spawn Point";

        [MenuItem("Oasis/Préparer le point d'apparition dans SampleScene")]
        public static void SetUpSpawnPoint()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            var origin = Object.FindFirstObjectByType<XROrigin>();
            if (origin == null)
            {
                Debug.LogError("SceneSetup : aucun XR Origin dans " + ScenePath + ", rien fait.");
                return;
            }

            var spawn = GameObject.Find(SpawnObjectName);
            if (spawn == null)
            {
                spawn = new GameObject(SpawnObjectName);
                Debug.Log("SceneSetup : objet '" + SpawnObjectName + "' cree.");
            }
            else
            {
                Debug.Log("SceneSetup : objet '" + SpawnObjectName + "' deja present, reutilise.");
            }

            // On le pose exactement ou se trouve deja le rig : position sure,
            // garantie sur le sol. Demi-tour pour que l'effet soit visible.
            spawn.transform.SetPositionAndRotation(
                origin.transform.position,
                origin.transform.rotation * Quaternion.Euler(0f, 180f, 0f));

            if (spawn.GetComponent<SpawnPoint>() == null)
                spawn.AddComponent<SpawnPoint>();

            if (spawn.GetComponent<PlayerSpawner>() == null)
                spawn.AddComponent<PlayerSpawner>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("SceneSetup : termine. Spawn en " + spawn.transform.position
                      + ", oriente a " + spawn.transform.eulerAngles.y + " degres.");
        }
    }
}
