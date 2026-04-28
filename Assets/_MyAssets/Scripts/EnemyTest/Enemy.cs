using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] private float _enemySpeed = 3f;
    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        GetComponent<Rigidbody2D>().linearVelocity = direction * _enemySpeed;
    }

}
