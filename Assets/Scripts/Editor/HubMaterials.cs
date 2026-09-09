using UnityEditor;
using UnityEngine;

namespace Oasis.EditorTools
{
    /// <summary>
    /// Crée et met en cache les matériaux du hub. Un blanc légèrement lustré
    /// pour l'architecture, un bleu émissif pour les portails.
    /// </summary>
    public static class HubMaterials
    {
        private const string Folder = "Assets/Materials";
        private const string WhitePath = Folder + "/Hub_Blanc.mat";
        private const string PortalPath = Folder + "/Hub_Portail.mat";

        public static Material White => GetOrCreate(WhitePath, ConfigureWhite);
        public static Material Portal => GetOrCreate(PortalPath, ConfigurePortal);

        private static Material GetOrCreate(string path, System.Action<Material> configure)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                configure(existing);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogError("HubMaterials : shader URP/Lit introuvable.");
                return null;
            }

            var material = new Material(shader);
            configure(material);

            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets", "Materials");

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ConfigureWhite(Material m)
        {
            // Pas un blanc pur : a 1.0 les surfaces se clippent et le relief
            // disparait. 0.92 garde du modele dans les zones eclairees.
            m.SetColor("_BaseColor", new Color(0.92f, 0.93f, 0.95f));
            m.SetFloat("_Smoothness", 0.55f);
            m.SetFloat("_Metallic", 0f);
        }

        private static void ConfigurePortal(Material m)
        {
            m.SetColor("_BaseColor", new Color(0.05f, 0.15f, 0.35f));
            m.SetFloat("_Smoothness", 0.8f);
            m.SetFloat("_Metallic", 0f);

            // HDR au-dela de 1 : c'est ce qui declenche le bloom et fait que
            // le portail eclaire vraiment au lieu d'etre un aplat bleu.
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            m.SetColor("_EmissionColor", new Color(0.2f, 1.4f, 3.2f));
        }
    }
}
