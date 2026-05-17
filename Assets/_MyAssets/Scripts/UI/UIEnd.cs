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
        _highScoresTable = FindAnyObjectByType<HighScoreTable>(); // Trouve une instance de la classe HighScoreTable dans la scÃ¨ne pour accÃ©der Ã  la liste des scores Ã©levÃ©s
        var highScoresEntryList = _highScoresTable.GetHighScoreEntries();  // RÃ©cupÃ¨re la liste des entrÃ©es de score Ã©levÃ© Ã  partir de la classe HighScoreTable

        // RÃ©cupÃ©ration du score du joueur Ã  partir des PlayerPrefs, avec une valeur par dÃ©faut de 0    
        _score = PlayerPrefs.GetInt("PlayerScore", 0);
        _txtScore.text = $"Pointage final: {_score}"; // Mise Ã  jour du texte du score

        //Lance le clignotement du message fin de partie
        GameOverSequence();

        // Vérifier si la table est pleine avec au moins 10 pointages sauvegardés
        if (highScoresEntryList != null && highScoresEntryList.Count >= 10)
        {
            // On trie la liste par ordre décroissant pour être certain de l'ordre des scores
            highScoresEntryList.Sort((a, b) => b.score.CompareTo(a.score));

            // Si le joueur égalise ou dépasse le 10e score (index 9), il se qualifie dans le Top 10 !
            int scoreABattre = highScoresEntryList[9].score;

            if (_score >= scoreABattre) 
            { 
                _endPanel.SetActive(false);
                _getNewHighScorePanel.SetActive(true);
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(_firstLetterEnterName.gameObject);
            }
            // Si le score ne qualifie pas le joueur dans le Top 10, on va directement à l'écran de fin
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(_menuButton.gameObject);
            }
        }
        // Si la table des scores n'est pas encore pleine, on se qualifie d'office !
        else
        {
            _endPanel.SetActive(false);
            _getNewHighScorePanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_firstLetterEnterName.gameObject);
        }
    }

    // MÃ©thode qui affiche le texte fin de partie et le fais clignoter avec la coroutine
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
        SceneManager.LoadScene(0); // Charge la scÃ¨ne de menu principal (index 0)
    }
}
