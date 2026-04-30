using UnityEngine;

public class FireballWeapon : Weapons
{
    [SerializeField] private GameObject _fireballPrefab;
    [SerializeField] private float _cooldown = 1f;
    [SerializeField] private float _speed = 6f;
    [SerializeField] private float _weaponLvl = 1f;

    private Player player; 

    private float _timer;

   
    public override void Init(Player player)
    {
        this.player = player;

        
        player.OnPlayerUp += Player_OnPlayerUp;
    }

    

    private void OnDisable()
    {
       
        if (player != null)
        {
            player.OnPlayerUp -= Player_OnPlayerUp;
        }
    }

    private void Player_OnPlayerUp(object sender, Player.OnPlayerUpEventArgs e)
    {
        _weaponLvl++;

        Debug.Log("Weapon level up ! Niveau actuel : " + _weaponLvl);
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
            Debug.LogWarning("Player non assigné dans FireballWeapon !");
            return;
        }

        GameObject fbObj = Instantiate(
            _fireballPrefab,
            player.transform.position,
            Quaternion.identity
        );

        Fireball fb = fbObj.GetComponent<Fireball>();

        if (fb != null)
        {
            fb.Init(_speed, player.GetLastDirection());
        }
    }
}