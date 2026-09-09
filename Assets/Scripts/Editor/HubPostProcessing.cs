using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Oasis.EditorTools
{
    /// <summary>
    /// Installe le post-traitement du hub. Sans lui, un matériau émissif
    /// reste un aplat de couleur : c'est le bloom qui le fait briller.
    /// </summary>
    public static class HubPostProcessing
    {
        private const string ProfilePath = "Assets/Settings/Hub_PostProcess.asset";

        [MenuItem("Oasis/Installer le post-traitement du hub")]
        public static void Setup()
        {
            var profile = BuildProfile();

            var go = new GameObject("Post-traitement");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            volume.sharedProfile = profile;

            EnableOnCamera();

            Debug.Log("HubPostProcessing : volume global installe avec " + ProfilePath + ".");
        }

        private static VolumeProfile BuildProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                    AssetDatabase.CreateFolder("Assets", "Settings");
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            var bloom = GetOrAdd<Bloom>(profile);
            // Seuil sous 1 pour que l'emissif des portails passe le filtre.
            bloom.threshold.Override(0.8f);
            bloom.intensity.Override(1.1f);
            bloom.scatter.Override(0.75f);
            bloom.highQualityFiltering.Override(true);

            var tone = GetOrAdd<Tonemapping>(profile);
            // Neutral et pas ACES : ACES ecrase les blancs, or tout le decor
            // est blanc. On perdrait exactement ce qu'on cherche a montrer.
            tone.mode.Override(TonemappingMode.Neutral);

            var color = GetOrAdd<ColorAdjustments>(profile);
            color.postExposure.Override(0.15f);
            color.contrast.Override(8f);
            color.saturation.Override(6f);

            // Volontairement absents : vignette d'ecran, aberration
            // chromatique, grain, motion blur, profondeur de champ, lens
            // distortion. Tous nefastes en VR — ils cassent la stereo, font
            // scintiller l'image ou provoquent la nausee. Le seul vignettage
            // acceptable est celui du tunneling, qui est en 3D dans la scene.

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
        {
            return profile.TryGet<T>(out var component) ? component : profile.Add<T>(true);
        }

        private static void EnableOnCamera()
        {
            var origin = Object.FindFirstObjectByType<XROrigin>();
            if (origin == null || origin.Camera == null)
            {
                Debug.LogWarning("HubPostProcessing : pas de camera XR, post-traitement non active dessus.");
                return;
            }

            var data = origin.Camera.GetUniversalAdditionalCameraData();
            if (data == null)
            {
                Debug.LogWarning("HubPostProcessing : camera sans donnees URP.");
                return;
            }

            data.renderPostProcessing = true;
            EditorUtility.SetDirty(data);
        }
    }
}
