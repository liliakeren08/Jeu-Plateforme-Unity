using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadLetterInput : MonoBehaviour
{
    private Button _letterButton; // Référence au composant Button attaché à ce GameObject
    private GetNewHighScore _getNewHighScore; // Référence à une instance de la classe GetNewHighScore pour appeler la méthode d'ajout de lettre
    private ResetHighScores _resetHighScores;
    [SerializeField] private GameObject _resetHighScoresPanel;
    [SerializeField] private GameObject _getNewHighScorePanel;

    private void Start()
    {
        _getNewHighScore = FindAnyObjectByType<GetNewHighScore>(); // Trouve une instance de la classe GetNewHighScore dans la scène pour accéder à ses méthodes
        _resetHighScores = FindAnyObjectByType<ResetHighScores>();
        _letterButton = this.GetComponent<Button>(); // Récupère le composant Button attaché à ce GameObject pour pouvoir ajouter un listener à son événement onClick
        _letterButton.onClick.AddListener(LireTexte); // Ajoute un listener à l'événement onClick du bouton qui appelle la méthode LireTexte lorsque le bouton est cliqué
    }


    // Méthode appelée lorsque le bouton est cliqué pour lire le texte du composant TMP_Text enfant et l'ajouter à la saisie du nom dans GetNewHighScore
    public void LireTexte() 
    {
        if (_getNewHighScore != null && _getNewHighScorePanel.activeSelf)
        {
            // Récupère le texte du composant TMP_Text enfant de ce GameObject et l'envoie à la méthode AddLetter de l'instance de GetNewHighScore pour l'ajouter à la saisie du nom
            _getNewHighScore.AddLetter(this.GetComponentInChildren<TMP_Text>().text); 
        }
        else if(_resetHighScores != null && _resetHighScoresPanel.activeSelf)
        {
            _resetHighScores.AddLettterPass(this.GetComponentInChildren<TMP_Text>().text);
        }
    }
}
