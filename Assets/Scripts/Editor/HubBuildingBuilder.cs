using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace Oasis.EditorTools
{
    /// <summary>
    /// Assemble un grand hall rectangulaire à partir des pièces modulaires du
    /// kit. Mesuré, pas deviné : module de 6 m, murs de 4 m, pivot au coin.
    /// </summary>
    public static class HubBuildingBuilder
    {
        private const string KitFolder = "Assets/Creepy_Cat";

        private const int TilesX = 8;
        private const int TilesZ = 8;
        private const float Module = 6f;
        private const float WallHeight = 4f;
        private const int WallRows = 2;

        private static float Width => TilesX * Module;
        private static float Depth => TilesZ * Module;

        private static Dictionary<string, GameObject> _kit;

        public static void Build()
        {
            _kit = LoadKit();
            if (_kit.Count == 0)
            {
                Debug.LogError("HubBuildingBuilder : aucun prefab trouve dans " + KitFolder + ".");
                return;
            }

            var root = new GameObject("Bâtiment");

            BuildFloor(root.transform);
            BuildWalls(root.transform);
            BuildRoof(root.transform);
            BuildTeleportSurface(root.transform);

            Debug.Log(string.Format("HubBuildingBuilder : hall de {0:F0} x {1:F0} m assemble, {2} pieces.",
                Width, Depth, root.GetComponentsInChildren<Transform>().Length - 1));
        }

        private static Dictionary<string, GameObject> LoadKit()
        {
            var map = new Dictionary<string, GameObject>();
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { KitFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (!map.ContainsKey(name))
                    map[name] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            return map;
        }

        private static GameObject Place(string prefabName, Transform parent, Vector3 position, float yaw)
        {
            if (!_kit.TryGetValue(prefabName, out var prefab) || prefab == null)
            {
                Debug.LogWarning("HubBuildingBuilder : prefab '" + prefabName + "' introuvable.");
                return null;
            }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.SetLocalPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            return go;
        }

        private static void BuildFloor(Transform parent)
        {
            var group = new GameObject("Sol").transform;
            group.SetParent(parent, false);

            for (var i = 0; i < TilesX; i++)
            for (var j = 0; j < TilesZ; j++)
            {
                // Le pivot d'une dalle est a un coin, elle s'etend en -X et +Z.
                // D'ou le (i+1) : la dalle i occupe [i*6, (i+1)*6].
                var name = (i + j) % 2 == 0 ? "Floor_Squared_01_6x6" : "Floor_Squared_02_6x6";
                Place(name, group, new Vector3((i + 1) * Module, 0f, j * Module), 0f);
            }
        }

        private static void BuildWalls(Transform parent)
        {
            var group = new GameObject("Murs").transform;
            group.SetParent(parent, false);

            var middleX = TilesX / 2;
            var middleZ = TilesZ / 2;

            for (var row = 0; row < WallRows; row++)
            {
                var y = row * WallHeight;
                // Le rez-de-chaussee est perce d'ouvertures, l'etage est plein.
                var pierce = row == 0;

                for (var j = 0; j < TilesZ; j++)
                {
                    if (!(pierce && j == middleZ))
                        Place(Variant(j), group, new Vector3(0f, y, j * Module), 0f);
                    if (!(pierce && j == middleZ))
                        Place(Variant(j + 1), group, new Vector3(Width, y, (j + 1) * Module), 180f);
                }

                for (var i = 0; i < TilesX; i++)
                {
                    if (!(pierce && i == middleX))
                        Place(Variant(i), group, new Vector3((i + 1) * Module, y, 0f), -90f);
                    if (!(pierce && i == middleX))
                        Place(Variant(i + 2), group, new Vector3(i * Module, y, Depth), 90f);
                }
            }
        }

        /// <summary>Alterne les variantes de mur pour casser la répétition.</summary>
        private static string Variant(int index)
        {
            var variants = new[] { "Wall_Simple_01_Long", "Wall_Simple_02_Long", "Wall_Simple_03_Long" };
            return variants[Mathf.Abs(index) % variants.Length];
        }

        private static void BuildRoof(Transform parent)
        {
            var group = new GameObject("Toit").transform;
            group.SetParent(parent, false);

            var y = WallRows * WallHeight;

            for (var i = 0; i < TilesX; i++)
            for (var j = 0; j < TilesZ; j++)
            {
                // Verriere sur la partie centrale, toit plein sur le pourtour :
                // la lumiere tombe au centre, ou on se tient.
                var central = i > 0 && i < TilesX - 1 && j > 0 && j < TilesZ - 1;
                var name = central ? "Roof_Glass_01_6x6" : "Roof_Squared_01_6x6";
                Place(name, group, new Vector3((i + 1) * Module, y, j * Module), 0f);
            }
        }

        private static void BuildTeleportSurface(Transform parent)
        {
            // Une seule surface invisible plutot qu'une TeleportationArea par
            // dalle : 64 fois moins de composants a evaluer.
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = "Surface de téléportation";
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(Width / 10f, 1f, Depth / 10f);
            go.transform.localPosition = new Vector3(Width * 0.5f, 0.02f, Depth * 0.5f);

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.enabled = false;

            go.AddComponent<TeleportationArea>();
        }
    }
}
