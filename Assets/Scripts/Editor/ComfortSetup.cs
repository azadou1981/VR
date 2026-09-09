using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

namespace Oasis.EditorTools
{
    /// <summary>
    /// Ajoute le vignettage anti-nausée au rig de la scène ouverte et le
    /// branche aux systèmes de locomotion qui en ont besoin.
    /// </summary>
    public static class ComfortSetup
    {
        private const string VignettePrefabPath =
            "Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/TunnelingVignette/TunnelingVignette.prefab";

        private const string VignetteObjectName = "Tunneling Vignette";

        [MenuItem("Oasis/Ajouter le vignettage anti-nausée à la scène ouverte")]
        public static void AddVignette()
        {
            var origin = Object.FindFirstObjectByType<XROrigin>();
            if (origin == null)
            {
                Debug.LogError("ComfortSetup : aucun XR Origin dans la scene ouverte.");
                return;
            }

            var camera = origin.Camera;
            if (camera == null)
            {
                Debug.LogError("ComfortSetup : le XR Origin n'a pas de camera.");
                return;
            }

            var existing = camera.transform.Find(VignetteObjectName);
            if (existing != null)
            {
                Debug.Log("ComfortSetup : vignettage deja present, rien fait.");
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(VignettePrefabPath);
            if (prefab == null)
            {
                Debug.LogError("ComfortSetup : prefab introuvable a " + VignettePrefabPath + ".");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, camera.transform);
            instance.name = VignetteObjectName;
            instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var controller = instance.GetComponent<TunnelingVignetteController>();
            if (controller == null)
            {
                Debug.LogError("ComfortSetup : pas de TunnelingVignetteController sur le prefab.");
                return;
            }

            // Seuls les deplacements continus provoquent la nausee. La
            // teleportation est instantanee et le snap turn est discret :
            // les vignetter gacherait la lisibilite sans rien apporter.
            // On vise les classes de base du package, pas DynamicMoveProvider
            // qui vient de l'echantillon Starter Assets : le code plateforme
            // ne doit pas dependre d'un sample, qui peut etre reimporte.
            var sources = new List<LocomotionProvider>();
            AddIfFound<ContinuousMoveProvider>(origin, sources);
            AddIfFound<ContinuousTurnProvider>(origin, sources);

            var providers = new List<LocomotionVignetteProvider>();
            foreach (var source in sources)
                providers.Add(new LocomotionVignetteProvider { locomotionProvider = source, enabled = true });

            controller.locomotionVignetteProviders = providers;
            EditorUtility.SetDirty(controller);

            Debug.Log("ComfortSetup : vignettage ajoute, branche sur " + providers.Count + " systeme(s) de locomotion.");
        }

        private static void AddIfFound<T>(XROrigin origin, List<LocomotionProvider> into)
            where T : LocomotionProvider
        {
            var found = origin.GetComponentInChildren<T>(true);
            if (found != null)
                into.Add(found);
            else
                Debug.LogWarning("ComfortSetup : " + typeof(T).Name + " introuvable sur le rig.");
        }
    }
}
