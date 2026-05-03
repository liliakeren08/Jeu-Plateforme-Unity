using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIEnd : UI
{
    [SerializeField] private TextMeshProUGUI _txtGameOver;
    [SerializeField] private TextMeshProUGUI _txtScore;
    [SerializeField] private Button _menuButton;
    [SerializeField] private Button _firstLetterEnterName;
    [SerializeField] private Button _firstLetterResetHighScores;
    [SerializeField] private GameObject _getNewHighScorePanel;
    [SerializeField] private GameObject _resetHighScoresPanel;
    [SerializeField] private GameObject _endPanel;

    private int _score;
    public int Score => _score;
    private HighScoreTable _highScoresTable;

    private void Start()
    {
        _highScoresTable = FindAnyObjectByType<HighScoreTable>(); // Trouve une instance de la classe HighScoreTable dans la scène pour accéder à la liste des scores élevés
        var highScoresEntryList = _highScoresTable.GetHighScoreEntries();  // Récupère la liste des entrées de score élevé à partir de la classe HighScoreTable

        // Récupération du score du joueur à partir des PlayerPrefs, avec une valeur par défaut de 0    
        _score = PlayerPrefs.GetInt("PlayerScore", 0);
        _txtScore.text = $"Pointage final: {_score}"; // Mise à jour du texte du score

        //Lance le clignotement du message fin de partie
        GameOverSequence();

        //Vérifier si la table est pleine avec au moins 10 pointages sauvegardé
        if (highScoresEntryList.Count >= 10)
        {
            // Vérifie si le score du joueur est supérieur au score le plus bas de la table des scores élevés
            if (_score > highScoresEntryList[highScoresEntryList.Count - 1].score) { 
                _endPanel.SetActive(false);
                _getNewHighScorePanel.SetActive(true);
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(_firstLetterEnterName.gameObject);
            }
            // Si le score du joueur n'est pas supérieur au score le plus bas de la table des scores élevés, affiche le panel de fin de partie avec les options
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(_menuButton.gameObject);
            }
        }
        // Si la table des scores élevés n'est pas encore pleine, affiche directement le panel de saisie du nouveau score élevé       
        else
        {
            _endPanel.SetActive(false);
            _getNewHighScorePanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_firstLetterEnterName.gameObject);
        }
    }

    // Méthode qui affiche le texte fin de partie et le fais clignoter avec la coroutine
    private void GameOverSequence()
    {
        _txtGameOver.gameObject.SetActive(true);
        StartCoroutine(GameOverBlinkRoutine());
    }

    // Coroutine pour le clignotement du texte fin de partie
    IEnumerator GameOverBlinkRoutine()
    {
        while (true)
        {
            _txtGameOver.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.7f);
            _txtGameOver.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.7f);
        }
    }

    public void OnResetHighScoresClick()
    {
        _endPanel.SetActive(false);
        _resetHighScoresPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_firstLetterResetHighScores.gameObject);
    }

    public void OnMenuClick()
    {
        SceneManager.LoadScene(0); // Charge la scène de menu principal (index 0)
    }
}
