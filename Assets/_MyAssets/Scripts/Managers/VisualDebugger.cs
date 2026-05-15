using UnityEngine;

/// <summary>
/// OUTIL DE CORRECTION VISUELLE :
/// Permet de déplacer le point de tir (FirePoint) en temps réel avec le clavier.
/// Touches : I (Haut), K (Bas), J (Gauche), L (Droite).
/// </summary>
public class VisualDebugger : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _moveSpeed = 1.5f;

    private void Update()
    {
        if (_firePoint == null) return;

        Vector3 move = Vector3.zero;

        // Déplacement du curseur
        if (Input.GetKey(KeyCode.I)) move.y += _moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.K)) move.y -= _moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.J)) move.x -= _moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.L)) move.x += _moveSpeed * Time.deltaTime;

        if (move != Vector3.zero)
        {
            _firePoint.localPosition += move;
            // Affiche les coordonnées dans la console pour pouvoir les copier ensuite
            Debug.Log($"[DEBUG] Position idéale trouvée : X = {_firePoint.localPosition.x}f, Y = {_firePoint.localPosition.y}f");
        }
    }

    private void OnDrawGizmos()
    {
        // Dessine une cible rouge visible uniquement dans la fenêtre Scene pour aider à viser
        if (_firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_firePoint.position, 0.3f);
            Gizmos.DrawLine(_firePoint.position + Vector3.left * 0.5f, _firePoint.position + Vector3.right * 0.5f);
            Gizmos.DrawLine(_firePoint.position + Vector3.up * 0.5f, _firePoint.position + Vector3.down * 0.5f);
        }
    }
}
