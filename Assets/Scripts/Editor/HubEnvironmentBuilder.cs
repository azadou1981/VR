using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace Oasis.EditorTools
{
    /// <summary>
    /// Construit l'architecture du hub : une rotonde blanche ouverte sur le
    /// ciel, bordée de portails bleus. Inspiration Ready Player One.
    /// </summary>
    public static class HubEnvironmentBuilder
    {
        private const int Sides = 12;
        private const int PortalCount = 6;
        private const float Radius = 20f;
        private const float WallHeight = 10f;
        private const float WallThickness = 0.4f;

        public static void Build()
        {
            var root = new GameObject("Hub");

            CreateFloor(root.transform);
            CreateWalls(root.transform);
            CreatePortals(root.transform);
        }

        private static void CreateFloor(Transform parent)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floor.name = "Sol";
            floor.transform.SetParent(parent, false);
            // Le cylindre Unity fait 2 unites de haut et 1 de diametre.
            floor.transform.localScale = new Vector3(Radius * 2f, 0.1f, Radius * 2f);
            floor.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            Paint(floor, HubMaterials.White);

            // Sans zone de teleportation, impossible de se deplacer.
            floor.AddComponent<TeleportationArea>();
        }

        private static void CreateWalls(Transform parent)
        {
            var group = new GameObject("Murs");
            group.transform.SetParent(parent, false);

            var width = 2f * Radius * Mathf.Tan(Mathf.PI / Sides) + 0.1f;

            for (var i = 0; i < Sides; i++)
            {
                var angle = i * 2f * Mathf.PI / Sides;
                var radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

                var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                panel.name = "Panneau " + (i + 1);
                panel.transform.SetParent(group.transform, false);
                panel.transform.localPosition = radial * Radius + Vector3.up * (WallHeight * 0.5f);
                panel.transform.localRotation = Quaternion.LookRotation(-radial);
                panel.transform.localScale = new Vector3(width, WallHeight, WallThickness);
                Paint(panel, HubMaterials.White);
            }
        }

        private static void CreatePortals(Transform parent)
        {
            var group = new GameObject("Portails");
            group.transform.SetParent(parent, false);

            // Un panneau sur deux, pour laisser respirer l'architecture.
            var step = Sides / PortalCount;

            for (var i = 0; i < PortalCount; i++)
            {
                var angle = i * step * 2f * Mathf.PI / Sides;
                var radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                var facing = Quaternion.LookRotation(-radial);

                var portal = new GameObject("Portail " + (i + 1));
                portal.transform.SetParent(group.transform, false);
                portal.transform.localPosition = radial * (Radius - WallThickness);
                portal.transform.localRotation = facing;

                var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                frame.name = "Cadre";
                frame.transform.SetParent(portal.transform, false);
                frame.transform.localPosition = new Vector3(0f, 3.2f, 0.05f);
                frame.transform.localScale = new Vector3(4.4f, 6.4f, 0.3f);
                Paint(frame, HubMaterials.White);

                var glow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                glow.name = "Lueur";
                glow.transform.SetParent(portal.transform, false);
                glow.transform.localPosition = new Vector3(0f, 3.2f, -0.15f);
                glow.transform.localScale = new Vector3(3.8f, 5.8f, 0.2f);
                Paint(glow, HubMaterials.Portal);
                // On traverse le portail : pas de collision.
                Object.DestroyImmediate(glow.GetComponent<Collider>());

                // Une lumiere par portail : c'est ce qui fait que le bleu
                // deborde sur le sol blanc au lieu de rester un rectangle.
                var lightObject = new GameObject("Lumiere");
                lightObject.transform.SetParent(portal.transform, false);
                lightObject.transform.localPosition = new Vector3(0f, 3f, -1.5f);

                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(0.35f, 0.75f, 1f);
                light.intensity = 4f;
                light.range = 14f;
                light.shadows = LightShadows.None;
            }
        }

        private static void Paint(GameObject go, Material material)
        {
            if (material == null)
                return;

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
        }
    }
}
