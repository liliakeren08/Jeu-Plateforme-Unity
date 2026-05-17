using System.Collections;
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

    [Header("Configuration Aide Sourdine")]
    [SerializeField] private GameObject _muteInstructionText;

    // Instance statique pour y accÃ©der facilement depuis le script Player
    public static UIGame Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateScoreDisplay();
        // Initialisation de la couleur du dÃ©gradÃ© au dÃ©marrage (Pleine vie = Vert)
        _fillImage.color = _healthGradient.Evaluate(1f);

        // Recherche automatique intelligente si le slot de l'inspecteur est vide
        if (_muteInstructionText == null)
        {
            TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var txt in allTexts)
            {
                if (txt.text.Contains("M OU 6") || 
                    txt.text.Contains("MUSIC") || 
                    txt.text.Contains("sourdine") || 
                    txt.text.Contains("SOURDINE") || 
                    txt.text.Contains("Mute") || 
                    txt.text.Contains("MUTE"))
                {
                    _muteInstructionText = txt.gameObject;
                    Debug.Log($"[UIGame] Texte de sourdine trouvé automatiquement : '{txt.text}' !");
                    break;
                }
            }
        }

        // Masquer l'aide de la sourdine après 10 secondes de jeu
        if (_muteInstructionText != null)
        {
            StartCoroutine(HideMuteInstructionAfterDelay());
        }
        else
        {
            Debug.LogWarning("[UIGame] Impossible de trouver le texte d'explication de la sourdine. Pensez à le lier dans l'inspecteur si son texte est différent !");
        }
    }
    
    private IEnumerator HideMuteInstructionAfterDelay()
    {
        yield return new WaitForSeconds(10f);
        if (_muteInstructionText != null)
        {
            _muteInstructionText.SetActive(false);
            Debug.Log("[UIGame] Texte de sourdine masqué après 10 secondes !");
        }
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
        // On rÃ©cupÃ¨re le score via le Singleton du GameManager
        int currentScore = GameManager.Instance.PlayerScore;
        _scoreText.text = $"{currentScore}";
    }
 // --- LOGIQUE DE LA VIE (AppelÃ©e par le script Player) ---
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float healthPercentage = currentHealth / maxHealth;

        // Met Ã  jour la position de la barre
        _healthSlider.value = healthPercentage;

        // Met Ã  jour la couleur selon le dÃ©gradÃ© (Vert -> Jaune -> Rouge)
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