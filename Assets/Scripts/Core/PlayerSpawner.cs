using Unity.XR.CoreUtils;
using UnityEngine;

namespace Oasis.Core
{
    /// <summary>
    /// Place le joueur sur le <see cref="SpawnPoint"/> de plus haute priorité
    /// au chargement de la scène.
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private XROrigin _origin;

        private void Start()
        {
            if (_origin == null)
                _origin = FindFirstObjectByType<XROrigin>();

            if (_origin == null)
            {
                Debug.LogError("PlayerSpawner : aucun XR Origin trouve dans la scene.", this);
                return;
            }

            Debug.Log($"PlayerSpawner : demarre, XR Origin = '{_origin.name}'.", this);

            var spawn = BestSpawnPoint();
            if (spawn == null)
            {
                Debug.LogWarning("PlayerSpawner : aucun SpawnPoint dans la scène, le joueur reste en place.", this);
                return;
            }

            // Même calcul que la téléportation XRI : on vise la position du
            // casque, pas celle du sol, sinon la caméra se retrouve enterrée.
            var headHeight = Vector3.up * _origin.CameraInOriginSpacePos.y;
            _origin.MatchOriginUpCameraForward(Vector3.up, spawn.transform.forward);
            _origin.MoveCameraToWorldLocation(spawn.transform.position + headHeight);

            Debug.Log($"PlayerSpawner : joueur place sur '{spawn.name}' en {spawn.transform.position}.", this);
        }

        private static SpawnPoint BestSpawnPoint()
        {
            var points = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            SpawnPoint best = null;

            foreach (var point in points)
            {
                if (best == null || point.Priority > best.Priority)
                    best = point;
            }

            return best;
        }
    }
}
