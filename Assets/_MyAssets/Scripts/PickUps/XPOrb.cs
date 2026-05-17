using UnityEngine;

public class XPOrb : MonoBehaviour
{
    private Transform target;
    [SerializeField] private float _speed = 8f; // Vitesse d'attraction légèrement plus fluide
    [SerializeField] private float _value = 1f;
    public float Value => _value;

    private bool _isCollected = false;
    public bool IsCollected => _isCollected;

    public void StartAttract(Transform player)
    {
        target = player;
    }

    void Update()
    {
        if (target != null && !_isCollected)
        {
            // Déplacement fluide vers le joueur
            transform.position = Vector2.MoveTowards(
                transform.position,
                target.position,
                _speed * Time.deltaTime
            );

            // Solution anti-tunneling physique : collecte immédiate si l'orbe est très proche du joueur
            float distance = Vector2.Distance(transform.position, target.position);
            if (distance < 0.5f)
            {
                Collect();
            }
        }
    }

    /// <summary>
    /// Gère la collecte sécurisée de l'orbe pour éviter les doublons et les blocages physiques.
    /// </summary>
    public void Collect()
    {
        if (_isCollected) return;
        _isCollected = true;

        Player player = null;
        if (target != null)
        {
            player = target.GetComponent<Player>();
            if (player == null)
            {
                player = target.GetComponentInParent<Player>();
            }
        }

        if (player != null)
        {
            player.AddXP(_value);
        }

        Destroy(gameObject);
    }
}