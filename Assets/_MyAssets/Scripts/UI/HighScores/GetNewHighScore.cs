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
        _btSaveName.onClick.AddListener(EnregistrerNom); // Ajoute un listener pour le bouton enregistrer qui appelle la mÃ©thode EnregistrerNom
        _uiEnd = FindAnyObjectByType<UIEnd>(); // Trouve une instance de la classe UIEnd dans la scÃ¨ne pour accÃ©der Ã  ses mÃ©thodes

        StartCoroutine(CloseGetHighScorePanelDelay());  // DÃ©marre la coroutine pour fermer le panneau de saisie aprÃ¨s un dÃ©lai
    }

    // Coroutine pour fermer le panneau de saisie aprÃ¨s un dÃ©lai
    IEnumerator CloseGetHighScorePanelDelay()
    {
        yield return new WaitForSeconds(60f); // Attendre 60 secondes
        if (_getNewHighScporePanel.activeSelf)
        {
            OnCancelClick();
        }
    }

    // MÃ©thode appeler quand une lettre est saisie (bouton) et modifie le champ texte
    public void AddLetter(string p_letter)
    {
        // s'assure que le panneau de saisie est actif
        if (_getNewHighScporePanel.activeSelf)
        {
            // GÃ¨re le caractÃ¨re d'espacement
            if (p_letter == "Espace")
            {
                _tempText += " ";
            }
            // GÃ¨re la touche pour effacer le dernier caractÃ¨re
            else if (p_letter == "â†" && _tempText.Length > 0)
            {
                _tempText = _tempText.Remove(_tempText.Length - 1);
            }
            // Si le texte n'a pas dÃ©passer la limite de longueur (3) on ajoute la lettre
            else
            {
                if (_tempText.Length < _maxNameLength)
                {
                    _tempText += p_letter;
                }
            }
            //Mets Ã  jour le champ texte
            _txtName.text = _tempText;
        }
    }

        // MÃ©thode appelÃ© quand on appuie sur le bouton enregistrer pour sauvegardÃ© la nouvelle entrÃ©e
    private void EnregistrerNom()
    {
        //Valide le nom entrÃ©e
        bool validName = false;
        string nameInput = _txtName.text;
        // VÃ©rifie que le nom entrÃ©e n'est pas vide !
        foreach (char c in nameInput)
        {
            if (c != ' ') // Si le nom contient au moins un caractÃ¨re diffÃ©rent d'un espace, il est considÃ©rÃ© comme valide
            {
                validName = true;
            }
        }

        if (!string.IsNullOrEmpty(nameInput) && validName) // Si le nom n'est pas vide et contient au moins un caractÃ¨re diffÃ©rent d'un espace, il est considÃ©rÃ© comme valide
        {
            HighScoreTable highScoreTable = FindAnyObjectByType<HighScoreTable>(); // Trouve une instance de la classe HighScoreTable dans la scÃ¨ne
            //Appelle la mÃ©thode pour ajoutÃ© le score et le nom Ã  la liste
            highScoreTable.AddHighScoreEntry(_uiEnd.Score, nameInput);
            
            // Affiche la nouvelle table
            highScoreTable.DisplayHighScoreTable();

            // Retourne sur l'Ã©cran de retour
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
        // Retourne sur l'Ã©cran de retour
        _getNewHighScporePanel.SetActive(false);
        _endPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_menuButton.gameObject);
        _errorMessage.SetActive(false);
    }
}

