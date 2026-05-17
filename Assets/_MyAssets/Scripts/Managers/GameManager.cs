using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Propriï¿½tï¿½ statique pour accï¿½der ï¿½ l'instance du GameManager

    public event EventHandler<OnEnemyDestroyedEventArgs> OnEnemyDestroyed; // ï¿½vï¿½nement dï¿½clenchï¿½ lorsque qu'un ennemi est dï¿½truit
    public class OnEnemyDestroyedEventArgs : EventArgs
    {
        public string DestroyedObjectTag; // Propriï¿½tï¿½ pour stocker le tag de l'ennemi dï¿½truit
    }


    [Header("Gestion de la difficultï¿½ du jeu")]
    [SerializeField] private float _enemySpeed = 6.0f;
    public float EnemySpeed => _enemySpeed; // Propriï¿½tï¿½ publique pour accï¿½der ï¿½ la vitesse des ennemis
    [SerializeField] private float _speedAugmentationStep = 0.5f; // Valeur d'augmentation de la vitesse des ennemis ï¿½ chaque augmentation de la difficultï¿½
    [SerializeField] private int _scoreThresholdForDifficultyIncrease = 500; // Seuil de score pour augmenter la difficultï¿½ du jeu

    private int _playerScore = 0; // Score du joueur
    public int PlayerScore => _playerScore; // Propriï¿½tï¿½ publique pour accï¿½der au score du joueur

    // Boolï¿½en pour indiquer si la difficultï¿½ a ï¿½tï¿½ augmentï¿½e, utilisï¿½ pour ï¿½viter d'augmenter la difficultï¿½ plusieurs fois pour le mï¿½me seuil de score
    private bool _difficultyIncrease = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // Assignation de l'instance du GameManager ï¿½ la propriï¿½tï¿½ statique
        }
        else
        {
            Destroy(gameObject); // Destruction de l'objet GameManager s'il existe dï¿½jï¿½ une instance
            Debug.LogError("Un deuxiï¿½me GameManager a ï¿½tï¿½ crï¿½ï¿½. Assurez-vous qu'il n'y a qu'un seul GameManager dans la scï¿½ne.");
        }
    }

    private void Start()
    {
        PlayerPrefs.SetInt("PlayerScore", 0); // Initialisation du score du joueur dans les PlayerPrefs ï¿½ 0 au dï¿½marrage du jeu
    }

    private void Update()
    {
        // Vï¿½rifie si le score du joueur atteint un seuil d'augmentation de la difficultï¿½, que le score n'est pas nul et que la difficultï¿½ n'a pas dï¿½jï¿½ ï¿½tï¿½ augmentï¿½e pour ce seuil
        if (_playerScore % _scoreThresholdForDifficultyIncrease == 0 && _playerScore != 0 && !_difficultyIncrease)
        {
            _difficultyIncrease = true; // Marque que la difficultï¿½ a ï¿½tï¿½ augmentï¿½e pour ce seuil de score
            _enemySpeed += _speedAugmentationStep; // Augmentation de la vitesse des ennemis
        }
        else if (_playerScore % _scoreThresholdForDifficultyIncrease != 0)
        {
            _difficultyIncrease = false; // Rï¿½initialise le marqueur d'augmentation de la difficultï¿½ lorsque le score n'est plus ï¿½ un seuil d'augmentation
        }
    }

    public void EnemyDestroyed(int p_enemyPoints, string p_gameObjectTag)
    {
        if (p_gameObjectTag == "Bullet")
        {
            _playerScore += p_enemyPoints; // Ajout des points de l'ennemi au score du joueur
            if (UIGame.Instance != null)
            {
                UIGame.Instance.UpdateScoreDisplay();
            }
        }

        OnEnemyDestroyed?.Invoke(this, new OnEnemyDestroyedEventArgs
        {
            DestroyedObjectTag = p_gameObjectTag
        }); // Dï¿½clenchement de l'ï¿½vï¿½nement de destruction d'un ennemi
    }

    public void EndGame()
    {
        // Enregistrement du score du joueur dans les PlayerPrefs ï¿½ la fin du jeu
        PlayerPrefs.SetInt("PlayerScore", _playerScore);

        // Vï¿½rification si une valeur de score le plus ï¿½levï¿½ existe dï¿½jï¿½ dans les PlayerPrefs
        if (PlayerPrefs.HasKey("PlayerHighScore"))
        {
            int highScore = PlayerPrefs.GetInt("PlayerHighScore");
            if (_playerScore > highScore)
            {
                PlayerPrefs.SetInt("PlayerHighScore", _playerScore); // Mise ï¿½ jour du score le plus ï¿½levï¿½ si le score actuel est supï¿½rieur
            }
        }
        else
        {
            PlayerPrefs.SetInt("PlayerHighScore", _playerScore); // Initialisation du score le plus ï¿½levï¿½ si aucune valeur n'existe encore
        }
        PlayerPrefs.Save(); // Sauvegarde des PlayerPrefs pour s'assurer que les donnï¿½es sont enregistrï¿½es

        StartCoroutine(LoadSceneDelay());

    }

    IEnumerator LoadSceneDelay()
    {
        yield return new WaitForSeconds(2f); // Attente de 1 seconde avant de charger la scï¿½ne de fin de jeu
        int noScene = SceneManager.GetActiveScene().buildIndex; // Rï¿½cupï¿½ration de l'index de la scï¿½ne actuelle 
        SceneManager.LoadScene(noScene + 1); // Chargement de la scï¿½ne de fin de jeu
    }

    public void TriggerOnEnemyDestroyed(object sender)
    {
        OnEnemyDestroyed?.Invoke(this, new OnEnemyDestroyedEventArgs
        {
            DestroyedObjectTag = "Player"
        });
    }
}

