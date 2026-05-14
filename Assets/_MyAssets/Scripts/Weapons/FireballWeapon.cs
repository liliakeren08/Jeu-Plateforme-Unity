using UnityEngine;

public class FireballWeapon : Weapons
{
    [SerializeField] private GameObject _fireballPrefab;
    [SerializeField] private float _cooldown = 0.1f;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _weaponLvl = 1f;

    private float _sizeMultiplier = 1f;
    private int _projectileCount = 1;
    private float _spreadAngle = 15f;

    private Player player;
    private float _timer;

    public override void Init(Player player)
    {
        this.player = player;

        if (player != null)
            player.OnPlayerUp += Player_OnPlayerUp;
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnPlayerUp -= Player_OnPlayerUp;
    }

    private void Player_OnPlayerUp(object sender, Player.OnPlayerUpEventArgs e)
    {
        _weaponLvl++;

        Debug.Log("Weapon level up ! Niveau actuel : " + _weaponLvl);
        ApplyLevelBonus((int)_weaponLvl);
    }

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
        if (player == null)
        {
            Debug.LogWarning("Player non assigné");
            return;
        }

       

        Vector2 baseDir = -player.GetLastDirection().normalized;

        Transform firePoint = player.FirePoint;
        Vector3 spawnPos = firePoint != null
            ? firePoint.position
            : player.transform.position;

        for (int i = 0; i < _projectileCount; i++)
        {
            float angleOffset = 0f;
            if (_projectileCount > 1)
            {
                float totalSpread = _spreadAngle * (_projectileCount - 1);
                angleOffset = -totalSpread / 2f + (_spreadAngle * i);
            }

            Vector2 newDir = Quaternion.Euler(0, 0, angleOffset) * baseDir;

            GameObject fbObj = Instantiate(
                _fireballPrefab,
                spawnPos,
                Quaternion.identity
            );

            Fireball fb = fbObj.GetComponent<Fireball>();
            if (fb != null)
            {
                fb.Init(_speed, newDir, _sizeMultiplier);
            }
        }
    }
    private void ApplyLevelBonus(int level)
    {
        switch (level)
        {
            case 2:
                _cooldown -= 0.5f;
                break;

            case 3:
                _speed += 5f;
                _cooldown -= 0.1f;
                _projectileCount = 2;
                break;

            case 4:
                _cooldown -= 0.3f;
                _speed += 2f;
                break;

            case 5:
                _projectileCount = 3;
                break;

            case 6:
                _sizeMultiplier += 0.5f;
                break;

            case 7:
                _sizeMultiplier += 0.2f;
                _cooldown -= 0.3f;
                _speed += 2f;
                break;

            case 8:
                _projectileCount = 5;
                _spreadAngle = 10f;
                break;

            default:
                break;
        }
    }
}