using UnityEngine;

public class EnemyFireball : MonoBehaviour
{
    private float _speed = 6f;
    private Vector3 _direction = Vector3.right;
    private static readonly Vector3 BASE_SCALE = new Vector3(0.3f, 0.3f, 0.3f);
    private Camera cam;
    private SpriteRenderer sr; // AJOUT : Pour retourner l'image

    private void Start()
    {
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        UpdateSpriteDirection();
    }

    public void Init(float speed, Vector3 direction, float scale = 1f)
    {
        _speed = speed;
        _direction = direction.normalized;
        transform.localScale = BASE_SCALE * scale;
        
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        UpdateSpriteDirection();
    }

    private void Update()
    {
        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);
        if (viewPos.x < -0.1f || viewPos.x > 1.1f ||
            viewPos.y < -0.1f || viewPos.y > 1.1f)
        {
            Destroy(gameObject);
        }

        transform.position += _direction * _speed * Time.deltaTime;
        UpdateSpriteDirection(); // Met à jour l'orientation en vol
    }

    // AJOUT : Tourne l'image selon la direction
    private void UpdateSpriteDirection()
    {
        if (sr == null) return;
        
        if (_direction.x < 0)
            sr.flipX = true; // Va vers la gauche
        else
            sr.flipX = false; // Va vers la droite
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // On ignore les zones utilitaires du joueur (aimant XP, ramassage)
        // Ces zones sont grandes et ne doivent pas déclencher de dégâts
        if (collision.GetComponent<MagnetZone>() != null) return;
        if (collision.name == "PickUpZone") return;

        // GetComponentInParent car le collider peut etre sur PlayerVisual (enfant)
        // mais le script Player est sur Player_Fireball (parent)
        if (collision.GetComponentInParent<Player>() != null)
        {
            Player player = collision.GetComponentInParent<Player>();
            player.TakeDamage(1f);
            Destroy(gameObject);
        }
    }
}