using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Propri�t� statique pour acc�der � l'instance du GameManager

    public event EventHandler<OnEnemyDestroyedEventArgs> OnEnemyDestroyed; // �v�nement d�clench� lorsque qu'un ennemi est d�truit
    public class OnEnemyDestroyedEventArgs : EventArgs
    {
        public string DestroyedObjectTag; // Propri�t� pour stocker le tag de l'ennemi d�truit
    }


    [Header("Gestion de la difficult� du jeu")]
    [SerializeField] private float _enemySpeed = 6.0f;
    public float EnemySpeed => _enemySpeed; // Propri�t� publique pour acc�der � la vitesse des ennemis
    [SerializeField] private float _speedAugmentationStep = 0.5f; // Valeur d'augmentation de la vitesse des ennemis � chaque augmentation de la difficult�
    [SerializeField] private int _scoreThresholdForDifficultyIncrease = 500; // Seuil de score pour augmenter la difficult� du jeu

    private int _playerScore = 0; // Score du joueur
    public int PlayerScore => _playerScore; // Propri�t� publique pour acc�der au score du joueur

    // Bool�en pour indiquer si la difficult� a �t� augment�e, utilis� pour �viter d'augmenter la difficult� plusieurs fois pour le m�me seuil de score
    private bool _difficultyIncrease = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // Assignation de l'instance du GameManager � la propri�t� statique
        }
        else
        {
            Destroy(gameObject); // Destruction de l'objet GameManager s'il existe d�j� une instance
            Debug.LogError("Un deuxi�me GameManager a �t� cr��. Assurez-vous qu'il n'y a qu'un seul GameManager dans la sc�ne.");
        }
    }

    private void Start()
    {
        PlayerPrefs.SetInt("PlayerScore", 0); // Initialisation du score du joueur dans les PlayerPrefs � 0 au d�marrage du jeu
    }

    private void Update()
    {
        // V�rifie si le score du joueur atteint un seuil d'augmentation de la difficult�, que le score n'est pas nul et que la difficult� n'a pas d�j� �t� augment�e pour ce seuil
        if (_playerScore % _scoreThresholdForDifficultyIncrease == 0 && _playerScore != 0 && !_difficultyIncrease)
        {
            _difficultyIncrease = true; // Marque que la difficult� a �t� augment�e pour ce seuil de score
            _enemySpeed += _speedAugmentationStep; // Augmentation de la vitesse des ennemis
        }
        else if (_playerScore % _scoreThresholdForDifficultyIncrease != 0)
        {
            _difficultyIncrease = false; // R�initialise le marqueur d'augmentation de la difficult� lorsque le score n'est plus � un seuil d'augmentation
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
        }); // D�clenchement de l'�v�nement de destruction d'un ennemi
    }

    public void EndGame()
    {
        // Enregistrement du score du joueur dans les PlayerPrefs � la fin du jeu
        PlayerPrefs.SetInt("PlayerScore", _playerScore);

        // V�rification si une valeur de score le plus �lev� existe d�j� dans les PlayerPrefs
        if (PlayerPrefs.HasKey("PlayerHighScore"))
        {
            int highScore = PlayerPrefs.GetInt("PlayerHighScore");
            if (_playerScore > highScore)
            {
                PlayerPrefs.SetInt("PlayerHighScore", _playerScore); // Mise � jour du score le plus �lev� si le score actuel est sup�rieur
            }
        }
        else
        {
            PlayerPrefs.SetInt("PlayerHighScore", _playerScore); // Initialisation du score le plus �lev� si aucune valeur n'existe encore
        }
        PlayerPrefs.Save(); // Sauvegarde des PlayerPrefs pour s'assurer que les donn�es sont enregistr�es

        StartCoroutine(LoadSceneDelay());

    }

    IEnumerator LoadSceneDelay()
    {
        yield return new WaitForSeconds(2f); // Attente de 1 seconde avant de charger la sc�ne de fin de jeu
        int noScene = SceneManager.GetActiveScene().buildIndex; // R�cup�ration de l'index de la sc�ne actuelle 
        SceneManager.LoadScene(noScene + 1); // Chargement de la sc�ne de fin de jeu
    }

    public void TriggerOnEnemyDestroyed(object sender)
    {
        OnEnemyDestroyed?.Invoke(this, new OnEnemyDestroyedEventArgs
        {
            DestroyedObjectTag = "Player"
        });
    }
}

