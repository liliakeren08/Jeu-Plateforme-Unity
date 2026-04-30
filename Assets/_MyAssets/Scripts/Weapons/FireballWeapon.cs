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

    private void ApplyLevelBonus(int level)
    {
        switch (level)
        {
            case 2:
                _cooldown -= 0.2f; 
                Debug.Log("Bonus lvl 2: cooldown réduit");
                break;

            case 3:
                _speed += 2f; 
                Debug.Log("Bonus lvl 3: vitesse augmentée");
                break;

            case 4:
                _cooldown -= 0.2f;
                _speed += 1f;
                Debug.Log("Bonus lvl 4: mix cooldown + speed");
                break;

            case 5:
                // exemple futur : multi shot
                Debug.Log("Bonus lvl 5: à définir");
                break;

            default:
                Debug.Log("Pas de bonus défini pour ce niveau");
                break;
        }
    }
}