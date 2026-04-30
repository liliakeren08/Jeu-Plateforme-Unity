using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Propriété statique pour accéder à l'instance du GameManager

    public event EventHandler<OnEnemyDestroyedEventArgs> OnEnemyDestroyed; // Événement déclenché lorsque qu'un ennemi est détruit
    public class OnEnemyDestroyedEventArgs : EventArgs
    {
        public string DestroyedObjectTag; // Propriété pour stocker le tag de l'ennemi détruit
    }


    [Header("Gestion de la difficulté du jeu")]
    [SerializeField] private float _enemySpeed = 6.0f;
    public float EnemySpeed => _enemySpeed; // Propriété publique pour accéder à la vitesse des ennemis
    [SerializeField] private float _speedAugmentationStep = 0.5f; // Valeur d'augmentation de la vitesse des ennemis à chaque augmentation de la difficulté
    [SerializeField] private int _scoreThresholdForDifficultyIncrease = 500; // Seuil de score pour augmenter la difficulté du jeu

    private int _playerScore = 0; // Score du joueur
    public int PlayerScore => _playerScore; // Propriété publique pour accéder au score du joueur

    // Booléen pour indiquer si la difficulté a été augmentée, utilisé pour éviter d'augmenter la difficulté plusieurs fois pour le même seuil de score
    private bool _difficultyIncrease = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // Assignation de l'instance du GameManager à la propriété statique
        }
        else
        {
            Destroy(gameObject); // Destruction de l'objet GameManager s'il existe déjà une instance
            Debug.LogError("Un deuxième GameManager a été créé. Assurez-vous qu'il n'y a qu'un seul GameManager dans la scène.");
        }
    }

    private void Start()
    {
        PlayerPrefs.SetInt("PlayerScore", 0); // Initialisation du score du joueur dans les PlayerPrefs à 0 au démarrage du jeu
    }

    private void Update()
    {
        // Vérifie si le score du joueur atteint un seuil d'augmentation de la difficulté, que le score n'est pas nul et que la difficulté n'a pas déjà été augmentée pour ce seuil
        if (_playerScore % _scoreThresholdForDifficultyIncrease == 0 && _playerScore != 0 && !_difficultyIncrease)
        {
            _difficultyIncrease = true; // Marque que la difficulté a été augmentée pour ce seuil de score
            _enemySpeed += _speedAugmentationStep; // Augmentation de la vitesse des ennemis
        }
        else if (_playerScore % _scoreThresholdForDifficultyIncrease != 0)
        {
            _difficultyIncrease = false; // Réinitialise le marqueur d'augmentation de la difficulté lorsque le score n'est plus à un seuil d'augmentation
        }
    }

    public void EnemyDestroyed(int p_enemyPoints, string p_gameObjectTag)
    {
        if (p_gameObjectTag == "Laser")
        {
            _playerScore += p_enemyPoints; // Ajout des points de l'ennemi au score du joueur
        }

        OnEnemyDestroyed?.Invoke(this, new OnEnemyDestroyedEventArgs
        {
            DestroyedObjectTag = p_gameObjectTag
        }); // Déclenchement de l'événement de destruction d'un ennemi
    }

    public void EndGame()
    {
        // Enregistrement du score du joueur dans les PlayerPrefs à la fin du jeu
        PlayerPrefs.SetInt("PlayerScore", _playerScore);

        // Vérification si une valeur de score le plus élevé existe déjà dans les PlayerPrefs
        if (PlayerPrefs.HasKey("PlayerHighScore"))
        {
            int highScore = PlayerPrefs.GetInt("PlayerHighScore");
            if (_playerScore > highScore)
            {
                PlayerPrefs.SetInt("PlayerHighScore", _playerScore); // Mise à jour du score le plus élevé si le score actuel est supérieur
            }
        }
        else
        {
            PlayerPrefs.SetInt("PlayerHighScore", _playerScore); // Initialisation du score le plus élevé si aucune valeur n'existe encore
        }
        PlayerPrefs.Save(); // Sauvegarde des PlayerPrefs pour s'assurer que les données sont enregistrées

        StartCoroutine(LoadSceneDelay());

    }

    IEnumerator LoadSceneDelay()
    {
        yield return new WaitForSeconds(2f); // Attente de 1 seconde avant de charger la scène de fin de jeu
        int noScene = SceneManager.GetActiveScene().buildIndex; // Récupération de l'index de la scène actuelle 
        SceneManager.LoadScene(noScene + 1); // Chargement de la scène de fin de jeu
    }

    public void TriggerOnEnemyDestroyed(object sender)
    {
        OnEnemyDestroyed?.Invoke(this, new OnEnemyDestroyedEventArgs
        {
            DestroyedObjectTag = "Player"
        });
    }
}

