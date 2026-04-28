using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] private float _speed = 5f;

    private Vector3 _direction = Vector3.right;

    private void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }
}