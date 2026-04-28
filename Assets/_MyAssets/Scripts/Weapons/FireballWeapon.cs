using UnityEngine;

public class FireballWeapon : Weapons
{
    [SerializeField] private GameObject _fireballPrefab;
    [SerializeField] private float _cooldown = 1f;
    [SerializeField] private float _speed = 6f;

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
        GameObject fbObj = Instantiate(_fireballPrefab, _player.transform.position, Quaternion.identity);

        Fireball fb = fbObj.GetComponent<Fireball>();

        fb.Init(_speed, _player.GetLastDirection());
    }
}
