using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIStart : UI
{
    [Header("Buttons")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _returnButton;
    [SerializeField] private Button _buttonClose; // AJOUT : Pour fermer les instructions

    [Header("Panels")]
    [SerializeField] private GameObject _instructionsPanel;
    [SerializeField] private GameObject _scorePanel;
    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _gameButtons; // AJOUT : Ton objet MenuBouttons

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _txtCompteur = default;

    private void Start()
    {
        // PlayerPrefs pour le compteur de partie
        int compteur = PlayerPrefs.GetInt("GamesCount", 0);
        _txtCompteur.text = "Nombre de parties : " + compteur.ToString();

        // SÉCURITÉ : Recherche automatique du bouton démarrer s'il a perdu sa référence dans l'inspecteur
        if (_startButton == null)
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var btn in allButtons)
            {
                string nameLower = btn.name.ToLower();
                if (nameLower.Contains("demarrer") || nameLower.Contains("start") || nameLower.Contains("démarrer"))
                {
                    _startButton = btn;
                    break;
                }
            }
        }

        // Slectionne le bouton dmarrer au chargement de la scne
        if (_startButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_startButton.gameObject);
        }
    }

    public void OnShowHighscoresClick()
    {
        _scorePanel.SetActive(true);
        _startPanel.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_returnButton.gameObject);

        StartCoroutine(ReturnToStartPanelDelay());
    }

    private IEnumerator ReturnToStartPanelDelay()
    {
        yield return new WaitForSeconds(30f);
        if (_scorePanel.activeSelf) OnReturnClick();
    }

    public void OnInstructionsClick()
    {
        _instructionsPanel.SetActive(true);
        _gameButtons.SetActive(false); // Dsactive le menu principal

        // Focus sur le bouton fermer
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_buttonClose.gameObject);
    }

    public void OnReturnClick()
    {
        _scorePanel.SetActive(false);
        _instructionsPanel.SetActive(false); // Assure la fermeture des deux
        _startPanel.SetActive(true);
        _gameButtons.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_startButton.gameObject);
    }

    public void OnStartClick()
    {
        int compteur = PlayerPrefs.GetInt("GamesCount", 0);
        compteur++;
        PlayerPrefs.SetInt("GamesCount", compteur);

        int noScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(noScene + 1);
    }

    public void OnQuitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        try
        {
            System.Diagnostics.Process.Start("..\\Portail.exe");
        }
        catch (System.Exception e)
        {
            Debug.Log("Portail non trouvé: " + e.Message);
        }
        Application.Quit();
#endif
    }
}