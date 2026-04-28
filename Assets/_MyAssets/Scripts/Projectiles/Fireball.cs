using UnityEngine;

public class Fireball : MonoBehaviour
{
    private float _speed = 6f;
    private Vector3 _direction = Vector3.right;

    public void Init(float speed, Vector3 direction)
    {
        _speed = speed;
        _direction = direction.normalized;
    }

    private void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }
}