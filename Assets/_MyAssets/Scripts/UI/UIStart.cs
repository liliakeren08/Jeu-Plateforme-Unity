using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIStart : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _returnButton;

    [SerializeField] private GameObject _scorePanel;
    [SerializeField] private GameObject _startPanel;

    [SerializeField] private TextMeshProUGUI _txtCompteur = default;

    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked; // Verrouille le curseur au centre de l'écran
        //Cursor.visible = false; // Rend le curseur invisible

        //PlayerPerfs pour le compteur de partie
        int compteur = PlayerPrefs.GetInt("GamesCount", 0);
        _txtCompteur.text = "Nombre de parties : " + compteur.ToString();

        // Sélectionne le bouton démarrer au chargement de la scène
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_startButton.gameObject);
    }

    // Appelé lorsque le bouton "Afficher les scores" est cliqué pour afficher le panel des scores
    public void OnShowHighscoresClick()
    {
        _scorePanel.SetActive(true);
        _startPanel.SetActive(false);

        // Sélectionne le bouton retour au changement de panel
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_returnButton.gameObject);

        StartCoroutine(ReturnToStartPanelDelay());
    }

    // Coroutine pour revenir automatiquement au panel de démarrage après un délai
    private IEnumerator ReturnToStartPanelDelay()
    {
        yield return new WaitForSeconds(30f);
        OnReturnClick();
    }

    // Appelé lorsque le bouton retour est cliqué pour revenir au panel de démarrage
    public void OnReturnClick()
    {
        _scorePanel.SetActive(false);
        _startPanel.SetActive(true);

        // Sélectionne le bouton démarrer au changement de panel
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_startButton.gameObject);
    }

    // Appelé lorsque le bouton "Démarrer" est cliqué pour lancer la partie
    public void OnStartClick()
    {
        // Incrémente le compteur de parties dans les PlayerPrefs à chaque démarrage de partie
        int compteur = PlayerPrefs.GetInt("GamesCount", 0);
        compteur++;
        PlayerPrefs.SetInt("GamesCount", compteur);

        // Charge la scène de jeu
        int noScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(noScene + 1);
    }

    // Appelé lorsque le bouton "Quitter" est cliqué pour quitter le jeu
    public void OnQuitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}