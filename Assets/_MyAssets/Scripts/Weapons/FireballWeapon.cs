using UnityEngine;

public class FireballWeapon : Weapons
{
    [SerializeField] private GameObject _fireballPrefab;
    [SerializeField] private float _cooldown = 1f;

    private float _timer;

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _cooldown)
        {
            Attack();
            _timer = 0f;
        }
    }

    public override void Attack()
    {
        Instantiate(_fireballPrefab, _player.transform.position, Quaternion.identity);
    }
}
