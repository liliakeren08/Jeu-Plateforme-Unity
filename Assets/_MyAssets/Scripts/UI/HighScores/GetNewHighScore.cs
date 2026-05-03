using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GetNewHighScore : MonoBehaviour
{
    [SerializeField] private GameObject _getNewHighScporePanel;
    [SerializeField] private GameObject _endPanel;
    [SerializeField] private GameObject _errorMessage;
    [SerializeField] private int _maxNameLength = 3;
    [SerializeField] private TextMeshProUGUI _txtName;
    [SerializeField] private Button _btSaveName;
    [SerializeField] private Button _menuButton;

    private string _tempText = "";
    private UIEnd _uiEnd;

    private void Start()
    {
        _btSaveName.onClick.AddListener(EnregistrerNom); // Ajoute un listener pour le bouton enregistrer qui appelle la méthode EnregistrerNom
        _uiEnd = FindAnyObjectByType<UIEnd>(); // Trouve une instance de la classe UIEnd dans la scène pour accéder à ses méthodes

        StartCoroutine(CloseGetHighScorePanelDelay());  // Démarre la coroutine pour fermer le panneau de saisie après un délai
    }

    // Coroutine pour fermer le panneau de saisie après un délai
    IEnumerator CloseGetHighScorePanelDelay()
    {
        yield return new WaitForSeconds(60f); // Attendre 60 secondes
        if (_getNewHighScporePanel.activeSelf)
        {
            OnCancelClick();
        }
    }

    // Méthode appeler quand une lettre est saisie (bouton) et modifie le champ texte
    public void AddLetter(string p_letter)
    {
        // s'assure que le panneau de saisie est actif
        if (_getNewHighScporePanel.activeSelf)
        {
            // Gère le caractère d'espacement
            if (p_letter == "Espace")
            {
                _tempText += " ";
            }
            // Gère la touche pour effacer le dernier caractère
            else if (p_letter == "←" && _tempText.Length > 0)
            {
                _tempText = _tempText.Remove(_tempText.Length - 1);
            }
            // Si le texte n'a pas dépasser la limite de longueur (3) on ajoute la lettre
            else
            {
                if (_tempText.Length < _maxNameLength)
                {
                    _tempText += p_letter;
                }
            }
            //Mets à jour le champ texte
            _txtName.text = _tempText;
        }
    }

        // Méthode appelé quand on appuie sur le bouton enregistrer pour sauvegardé la nouvelle entrée
    private void EnregistrerNom()
    {
        //Valide le nom entrée
        bool validName = false;
        string nameInput = _txtName.text;
        // Vérifie que le nom entrée n'est pas vide !
        foreach (char c in nameInput)
        {
            if (c != ' ') // Si le nom contient au moins un caractère différent d'un espace, il est considéré comme valide
            {
                validName = true;
            }
        }

        if (!string.IsNullOrEmpty(nameInput) && validName) // Si le nom n'est pas vide et contient au moins un caractère différent d'un espace, il est considéré comme valide
        {
            HighScoreTable highScoreTable = FindAnyObjectByType<HighScoreTable>(); // Trouve une instance de la classe HighScoreTable dans la scène
            //Appelle la méthode pour ajouté le score et le nom à la liste
            highScoreTable.AddHighScoreEntry(_uiEnd.Score, nameInput);
            
            // Affiche la nouvelle table
            highScoreTable.DisplayHighScoreTable();

            // Retourne sur l'écran de retour
            _getNewHighScporePanel.SetActive(false);
            _endPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_menuButton.gameObject);
            _errorMessage.SetActive(false);
        }
        else
        {
            // Message d'erreur si saisie non valide
            _errorMessage.SetActive(true);
        }

    }

    public void OnCancelClick()
    {
        // Retourne sur l'écran de retour
        _getNewHighScporePanel.SetActive(false);
        _endPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_menuButton.gameObject);
        _errorMessage.SetActive(false);
    }
}

