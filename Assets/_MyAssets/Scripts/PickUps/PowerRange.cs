using UnityEngine;

public class PowerRange : MonoBehaviour
{
    private Transform target;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _value = 1f;
    public float Value => _value;


    public void StartAttract(Transform player)
    {
        target = player;
    }

    void Update()
    {
        if (target != null)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                target.position,
                _speed * Time.deltaTime
            );
        }
    }
}
