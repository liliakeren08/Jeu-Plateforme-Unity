using UnityEngine;

/// <summary>
/// Arme principale du joueur. 
/// Tire des boules de feu et s'améliore automatiquement quand le joueur monte de niveau.
/// </summary>
public class FireballWeapon : Weapons
{
    [SerializeField] private GameObject _fireballPrefab;
    
    // Temps de recharge (cooldown) entre chaque tir. Plus il est bas, plus le tir est rapide.
    [SerializeField] private float _cooldown = 0.1f; 
    
    // Vitesse de déplacement de la boule de feu à l'écran
    [SerializeField] private float _speed = 15f; 
    
    [SerializeField] private float _weaponLvl = 1f;
    private float _sizeMultiplier = 1f;
    private int _projectileCount = 1;
    private float _spreadAngle = 15f;
    private Player player;
    
    // Chronomètre interne pour savoir quand l'arme est prête à tirer le prochain coup
    private float _timer;

    public override void Init(Player player)
    {
        this.player = player;
        
        // On s'abonne à l'événement du joueur pour s'améliorer quand il gagne un niveau
        if (player != null)
            player.OnPlayerUp += Player_OnPlayerUp;
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnPlayerUp -= Player_OnPlayerUp;
    }

    /// <summary>
    /// Fonction appelée automatiquement dès que le joueur gagne un niveau d'XP.
    /// </summary>
    private void Player_OnPlayerUp(object sender, Player.OnPlayerUpEventArgs e)
    {
        _weaponLvl++;
        Debug.Log("L'arme monte de niveau ! Niveau actuel : " + _weaponLvl);
        ApplyLevelBonus((int)_weaponLvl);
    }

    private void Update()
    {
        // Le timer augmente en permanence (l'arme se recharge tout le temps en arrière-plan)
        _timer += Time.deltaTime;

        // Si le bouton de tir est enfoncé ET que l'arme est rechargée (timer >= cooldown)
        if (player != null && player.IsShooting && _timer >= _cooldown)
        {
            Attack();
            _timer = 0f; // On réinitialise le timer après chaque tir
        }
    }

    public override void Attack()
    {
        if (player == null) return;
        
        // On déclenche l'animation d'attaque du joueur juste au moment où l'arme tire !
        if (player.PlayerAnimator != null)
        {
            player.PlayerAnimator.SetTrigger("attack");
        }

        // Utilisation d'une Coroutine pour permettre un mini-délai si besoin pour l'animation
        StartCoroutine(AttackWithDelay());
    }

    private System.Collections.IEnumerator AttackWithDelay()
    {
        // On attend juste 1 frame pour s'assurer que l'animation "attack" a le temps de démarrer
        yield return null;

        if (player == null) yield break;

        // On récupère la direction du joueur (qui a été verrouillée à Gauche ou Droite)
        Vector2 baseDir = player.GetLastDirection().normalized;
        Transform firePoint = player.FirePoint;
        
        // On définit la position d'apparition de la boule de feu
        Vector3 spawnPos = firePoint != null ? firePoint.position : player.transform.position;

        // Boucle pour générer le bon nombre de boules de feu (qui augmente avec les niveaux)
        for (int i = 0; i < _projectileCount; i++)
        {
            float angleOffset = 0f;
            
            // S'il y a plusieurs projectiles, on calcule un angle pour les envoyer en "éventail" (spread)
            if (_projectileCount > 1)
            {
                float totalSpread = _spreadAngle * (_projectileCount - 1);
                angleOffset = -totalSpread / 2f + (_spreadAngle * i);
            }

            Vector2 newDir = Quaternion.Euler(0, 0, angleOffset) * baseDir;

            // Création de la boule de feu et envoi des bonnes stats (vitesse, direction, taille)
            GameObject fbObj = Instantiate(_fireballPrefab, spawnPos, Quaternion.identity);
            Fireball fb = fbObj.GetComponent<Fireball>();
            if (fb != null)
                fb.Init(_speed, newDir, _sizeMultiplier);
        }
    }

    /// <summary>
    /// Applique les bonus incroyables à l'arme selon le niveau atteint.
    /// Ex: Tir de plus en plus rapide, plus de projectiles tirés simultanément, etc.
    /// </summary>
    private void ApplyLevelBonus(int level)
    {
        switch (level)
        {
            case 2: _cooldown -= 0.05f; break; // Le tir devient plus rapide
            case 3:
                _speed += 5f;
                _cooldown -= 0.1f;
                _projectileCount = 2; // On tire 2 boules de feu à la fois !
                break;
            case 4:
                _cooldown -= 0.3f;
                _speed += 2f;
                break;
            case 5: _projectileCount = 3; break;
            case 6: _sizeMultiplier += 0.5f; break; // Les boules deviennent énormes
            case 7:
                _sizeMultiplier += 0.2f;
                _cooldown -= 0.3f;
                _speed += 2f;
                break;
            case 8:
                _projectileCount = 5; // Déchaînement absolu : 5 boules de feu en éventail !
                _spreadAngle = 10f;
                break;
        }
    }
}