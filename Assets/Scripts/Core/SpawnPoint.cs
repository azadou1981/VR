using UnityEngine;

namespace Oasis.Core
{
    /// <summary>
    /// Marque un endroit où un joueur peut apparaître. La rotation de l'objet
    /// donne la direction du regard à l'arrivée.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private int _priority;

        /// <summary>Le point de plus haute priorité gagne.</summary>
        public int Priority => _priority;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.9f, 0.25f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.8f);
            Gizmos.DrawRay(transform.position + Vector3.up * 1.6f, transform.forward * 0.6f);
        }
    }
}
