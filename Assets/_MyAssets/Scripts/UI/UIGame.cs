using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIGame :  UI
{
    [Header("Configuration Score")]
    [SerializeField] private TextMeshProUGUI _scoreText;

    [Header("Configuration Chrono")]
    [SerializeField] private TextMeshProUGUI _timeText;
    private float _elapsedTime = 0f;

    [Header("Configuration Barre de Vie")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Gradient _healthGradient;

    // Instance statique pour y accéder facilement depuis le script Player
    public static UIGame Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateScoreDisplay();
        // Initialisation de la couleur du dégradé au démarrage (Pleine vie = Vert)
        _fillImage.color = _healthGradient.Evaluate(1f);
    }
    

    private void Update()
    {
        // GESTION DU TEMPS : Calcul du chrono en secondes
        _elapsedTime += Time.deltaTime;
        UpdateTimerDisplay();
    }

    // --- LOGIQUE DU SCORE ---
    public void UpdateScoreDisplay()
    {
        // On récupère le score via le Singleton du GameManager
        int currentScore = GameManager.Instance.PlayerScore;
        _scoreText.text = $"{currentScore}";
    }
 // --- LOGIQUE DE LA VIE (Appelée par le script Player) ---
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float healthPercentage = currentHealth / maxHealth;

        // Met à jour la position de la barre
        _healthSlider.value = healthPercentage;

        // Met à jour la couleur selon le dégradé (Vert -> Jaune -> Rouge)
        _fillImage.color = _healthGradient.Evaluate(healthPercentage);
    }

    // --- LOGIQUE DU CHRONO ---
    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(_elapsedTime / 60);
        int seconds = Mathf.FloorToInt(_elapsedTime % 60);
        _timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}