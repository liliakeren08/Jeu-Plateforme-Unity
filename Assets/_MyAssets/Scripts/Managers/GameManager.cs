using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

    public event EventHandler<OnEnemyDestroyedEventArgs> OnEnemyDestroyed; 
    public class OnEnemyDestroyedEventArgs : EventArgs
    {
        public string DestroyedObjectTag; 
    }


    [Header("Gestion de la difficulté du jeu")]
    [SerializeField] private float _enemySpeed = 6.0f;
    public float EnemySpeed => _enemySpeed; 
    [SerializeField] private float _speedAugmentationStep = 0.5f; 
    [SerializeField] private int _scoreThresholdForDifficultyIncrease = 500; 

    private int _playerScore = 0; 
    public int PlayerScore => _playerScore;

    private bool _difficultyIncrease = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; 
        }
        else
        {
            Destroy(gameObject); 
            Debug.LogError("Un deuxi�me GameManager a été crée. Assurez-vous qu'il n'y a qu'un seul GameManager dans la scéne.");
        }
    }

    private void Start()
    {
        PlayerPrefs.SetInt("PlayerScore", 0);
        Debug.Log("[GameManager] Partie démarrée — Score initialisé à 0.");
    }

    private void Update()
    {
        if (_playerScore % _scoreThresholdForDifficultyIncrease == 0 && _playerScore != 0 && !_difficultyIncrease)
        {
            _difficultyIncrease = true; 
            _enemySpeed += _speedAugmentationStep; 
        }
        else if (_playerScore % _scoreThresholdForDifficultyIncrease != 0)
        {
            _difficultyIncrease = false; 
        }
    }

    public void EnemyDestroyed(int p_enemyPoints, string p_gameObjectTag)
    {
        if (p_gameObjectTag == "Bullet")
        {
            _playerScore += p_enemyPoints; 
        }

        OnEnemyDestroyed?.Invoke(this, new OnEnemyDestroyedEventArgs
        {
            DestroyedObjectTag = p_gameObjectTag
        }); 
    }

    public void TriggerOnEnemyDestroyed(object sender)
    {
        OnEnemyDestroyed?.Invoke(this, new OnEnemyDestroyedEventArgs
        {
            DestroyedObjectTag = "Player"
        });
    }
}

