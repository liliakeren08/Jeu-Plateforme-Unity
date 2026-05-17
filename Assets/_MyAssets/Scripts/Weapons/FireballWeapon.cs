using UnityEngine;

/// <summary>
/// Arme principale du joueur. 
/// Tire des boules de feu et s'amÃ©liore automatiquement quand le joueur monte de niveau.
/// </summary>
public class FireballWeapon : Weapons
{
    [SerializeField] private GameObject _fireballPrefab;
    [SerializeField] private AudioClip _shootSound;
    
    // Temps de recharge (cooldown) entre chaque tir. Plus il est bas, plus le tir est rapide.
    [SerializeField] private float _cooldown = 0.1f; 
    
    // Vitesse de dÃ©placement de la boule de feu Ã  l'Ã©cran
    [SerializeField] private float _speed = 15f; 
    
    [SerializeField] private float _weaponLvl = 1f;
    private float _sizeMultiplier = 1f;
    private int _projectileCount = 1;
    private float _spreadAngle = 15f;
    private Player player;
    
    // ChronomÃ¨tre interne pour savoir quand l'arme est prÃªte Ã  tirer le prochain coup
    private float _timer;

    public override void Init(Player player)
    {
        this.player = player;
        
        // On s'abonne Ã  l'Ã©vÃ©nement du joueur pour s'amÃ©liorer quand il gagne un niveau
        if (player != null)
            player.OnPlayerUp += Player_OnPlayerUp;
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnPlayerUp -= Player_OnPlayerUp;
    }

    /// <summary>
    /// Fonction appelÃ©e automatiquement dÃ¨s que le joueur gagne un niveau d'XP.
    /// </summary>
    private void Player_OnPlayerUp(object sender, Player.OnPlayerUpEventArgs e)
    {
        _weaponLvl++;
        Debug.Log("L'arme monte de niveau ! Niveau actuel : " + _weaponLvl);
        ApplyLevelBonus((int)_weaponLvl);
    }

    private void Update()
    {
        // Le timer augmente en permanence (l'arme se recharge tout le temps en arriÃ¨re-plan)
        _timer += Time.deltaTime;

        // Si le bouton de tir est enfoncÃ© ET que l'arme est rechargÃ©e (timer >= cooldown)
        if (player != null && player.IsShooting && _timer >= _cooldown)
        {
            Attack();
            _timer = 0f; // On rÃ©initialise le timer aprÃ¨s chaque tir
        }
    }

    public override void Attack()
    {
        if (player == null) return;
        
        // On dÃ©clenche l'animation d'attaque du joueur juste au moment oÃ¹ l'arme tire !
        if (player.PlayerAnimator != null)
        {
            player.PlayerAnimator.SetTrigger("attack");
        }

        // Joue le son de tir s'il est assignÃ©
        if (_shootSound != null)
        {
            AudioSource.PlayClipAtPoint(_shootSound, Camera.main != null ? Camera.main.transform.position : transform.position);
        }

        // AttÃ©nue la musique pour faire ressortir le son du tir
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.DuckMusic(0.5f, 0.15f);
        }

        // Utilisation d'une Coroutine pour permettre un mini-dÃ©lai si besoin pour l'animation
        StartCoroutine(AttackWithDelay());
    }

    private System.Collections.IEnumerator AttackWithDelay()
    {
        // On attend juste 1 frame pour s'assurer que l'animation "attack" a le temps de dÃ©marrer
        yield return null;

        if (player == null) yield break;

        // On rÃ©cupÃ¨re la direction du joueur (qui a Ã©tÃ© verrouillÃ©e Ã  Gauche ou Droite)
        Vector2 baseDir = player.GetLastDirection().normalized;
        Transform firePoint = player.FirePoint;
        
        // On dÃ©finit la position d'apparition de la boule de feu
        Vector3 spawnPos = firePoint != null ? firePoint.position : player.transform.position;

        // Boucle pour gÃ©nÃ©rer le bon nombre de boules de feu (qui augmente avec les niveaux)
        for (int i = 0; i < _projectileCount; i++)
        {
            float angleOffset = 0f;
            
            // S'il y a plusieurs projectiles, on calcule un angle pour les envoyer en "Ã©ventail" (spread)
            if (_projectileCount > 1)
            {
                float totalSpread = _spreadAngle * (_projectileCount - 1);
                angleOffset = -totalSpread / 2f + (_spreadAngle * i);
            }

            Vector2 newDir = Quaternion.Euler(0, 0, angleOffset) * baseDir;

            // CrÃ©ation de la boule de feu et envoi des bonnes stats (vitesse, direction, taille)
            GameObject fbObj = Instantiate(_fireballPrefab, spawnPos, Quaternion.identity);
            Fireball fb = fbObj.GetComponent<Fireball>();
            if (fb != null)
                fb.Init(_speed, newDir, _sizeMultiplier);
        }
    }

    /// <summary>
    /// Applique les bonus incroyables Ã  l'arme selon le niveau atteint.
    /// Ex: Tir de plus en plus rapide, plus de projectiles tirÃ©s simultanÃ©ment, etc.
    /// </summary>
    private void ApplyLevelBonus(int level)
    {
        switch (level)
        {
            case 2: 
                _cooldown = Mathf.Max(0.08f, _cooldown - 0.05f); // Le tir devient plus rapide
                break;
            case 3:
                _speed += 5f;
                _cooldown = Mathf.Max(0.08f, _cooldown - 0.1f);
                _projectileCount = 2; // On tire 2 boules de feu Ã  la fois !
                break;
            case 4:
                _cooldown = Mathf.Max(0.08f, _cooldown - 0.15f); // Soustraction calibrÃ©e
                _speed += 2f;
                break;
            case 5: 
                _projectileCount = 3; 
                break;
            case 6: 
                _sizeMultiplier += 0.5f; // Les boules deviennent Ã©normes
                break; 
            case 7:
                _sizeMultiplier += 0.2f;
                _cooldown = Mathf.Max(0.08f, _cooldown - 0.1f);
                _speed += 2f;
                break;
            case 8:
                _projectileCount = 5; // DÃ©chaÃ®nement absolu : 5 boules de feu en Ã©ventail !
                _spreadAngle = 10f;
                break;
        }
    }
}