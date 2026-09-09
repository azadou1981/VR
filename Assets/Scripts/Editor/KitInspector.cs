using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Oasis.EditorTools
{
    /// <summary>
    /// Mesure les prefabs d'un kit modulaire. Assembler à l'aveugle produit
    /// des murs qui ne se touchent pas : on mesure d'abord.
    /// </summary>
    public static class KitInspector
    {
        private const string KitFolder = "Assets/Creepy_Cat";

        private static readonly string[] Interesting =
        {
            "Floor_Squared_01_6x6", "Floor_Squared_02_6x6",
            "Roof_Squared_01_6x6", "Roof_Glass_01_6x6",
            "Wall_Simple_01_Long", "Wall_Simple_02_Long", "Wall_Simple_03_Long",
            "Wall_Simple_01_Coin", "Wall_Simple_01_Half",
            "DoorWay_01_Large", "Column_01_Big",
            "Light_Corridor_01", "Light_SideLight_01",
            "Stairway_Plateform_00", "Stairway_Step_01",
        };

        [MenuItem("Oasis/Mesurer les pièces du kit")]
        public static void Measure()
        {
            var byName = new Dictionary<string, string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { KitFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                byName[System.IO.Path.GetFileNameWithoutExtension(path)] = path;
            }

            Debug.Log("KitInspector : " + byName.Count + " prefabs trouves dans " + KitFolder + ".");

            foreach (var name in Interesting)
            {
                if (!byName.TryGetValue(name, out var path))
                {
                    Debug.LogWarning("KitInspector : " + name + " absent.");
                    continue;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    continue;

                // On INSTANCIE : les bounds d'un mesh sont dans son propre
                // repere, souvent en Z-up, et ignorent la rotation que le
                // prefab applique. Seul le monde donne la vraie taille.
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

                var renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
                if (renderers.Length == 0)
                {
                    Debug.Log("MESURE " + name + " : aucun MeshRenderer.");
                    Object.DestroyImmediate(instance);
                    continue;
                }

                var bounds = renderers[0].bounds;
                foreach (var r in renderers)
                    bounds.Encapsulate(r.bounds);

                var s = bounds.size;
                var shader = "?";
                if (renderers[0].sharedMaterial != null && renderers[0].sharedMaterial.shader != null)
                    shader = renderers[0].sharedMaterial.shader.name;

                Debug.Log(string.Format(
                    "MESURE {0} : taille {1:F2} x {2:F2} x {3:F2} | centre {4:F2},{5:F2},{6:F2} | shader {7}",
                    name, s.x, s.y, s.z, bounds.center.x, bounds.center.y, bounds.center.z, shader));

                Object.DestroyImmediate(instance);
            }
        }
    }
}
