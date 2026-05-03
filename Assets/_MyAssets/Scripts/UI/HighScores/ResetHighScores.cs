using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResetHighScores : MonoBehaviour
{
    private const string PASSWORD = "TOTO";  // Mot de passe pour réinitialiser les meilleurs pointages

    [SerializeField] private GameObject _resetHighScoresPanel;     // Panneau de saisie du mot de passe pour réinitialiser les meilleurs pointages
    [SerializeField] private GameObject _endPanel;                 // Panneau de fin de partie avec les options pour le joueur
    [SerializeField] private GameObject _errorMessage;             // Message d'erreur affiché lorsque le mot de passe est incorrect
    [SerializeField] private GameObject _correctPasswordMessage;   // Message de succès affiché lorsque le mot de passe est correct
    [SerializeField] private int _maxPasswordLength = 10;          // Longueur maximale du mot de passe
    [SerializeField] private TextMeshProUGUI _txtPass;             // Champ de texte pour afficher les * correspondant au mot de passe saisi
    [SerializeField] private Button _btResetPass;                  // Bouton pour valider le mot de passe
    [SerializeField] private Button _menuButton;                   // Bouton pour retourner au menu

    private string _tempText = "";                                 // Variable temporaire pour stocker le mot de passe saisi par le joueur
    private string _tempTextHidden = "";                           // Variable temporaire pour stocker les * correspondant au mot de passe saisi par le joueur
    private UIEnd _uiEnd;                                          // Référence à la classe UIEnd pour accéder à ses méthodes et propriétés

    private void Start()
    {
        _btResetPass.onClick.AddListener(ValidatePassword); // Ajoute un listener pour le bouton enregistrer qui appelle la méthode EnregistrerNom
        _uiEnd = FindAnyObjectByType<UIEnd>(); // Trouve une instance de la classe UIEnd dans la scène pour accéder à ses méthodes

        StartCoroutine(CloseResetHighScorePanelDelay() ); // Démarre la coroutine pour fermer le panneau de réinitialisation des meilleurs pointages après un délai
    }

    // Coroutine pour fermer le panneau de réinitialisation des meilleurs pointages après un délai
    IEnumerator CloseResetHighScorePanelDelay()
    {
        yield return new WaitForSeconds(60f); // Attendre 60 secondes
        OnCancelClick();
    }

    // Méthode appeler quand une lettre est saisie (bouton) et modifie le champ texte
    public void AddLettterPass(string lettre)
    {
        // s'assure que le panneau de saisie est actif
        if (_resetHighScoresPanel.activeSelf)
        {
            //Gère l'ajout des lettre mais affiche des * pour garder le mot de passe secret
            if (lettre == "Espace")
            {
                _tempText += " ";
                _tempTextHidden += "*";
            }
            else if (lettre == "←" && _tempText.Length > 0)
            {
                _tempText = _tempText.Remove(_tempText.Length - 1);
                _tempTextHidden = _tempTextHidden.Remove(_tempTextHidden.Length - 1);
            }
            else
            {
                if (_tempText.Length < _maxPasswordLength)
                {
                    _tempText += lettre;
                    _tempTextHidden += "*";
                }
            }
            // Mets à jour le champ texte avec les *
            _txtPass.text = _tempTextHidden;
        }
    }

    // Permet de valider le mot de passe pour effacer les meilleurs pointages
    private void ValidatePassword()
    {
        if (_tempText == PASSWORD)
        {
            // Si le mot de passe est valide on efface la table et retroune sur la scène de départ
            _errorMessage.SetActive(false);
            _correctPasswordMessage.SetActive(true);
            PlayerPrefs.DeleteKey("highScoreTable");
            StartCoroutine(LoadMenuSceneDelay());            
        }
        else
        {
            // Si mot de passe incorrect on affiche message erreur
            _errorMessage.SetActive(true);
            _correctPasswordMessage.SetActive(false);
        }

        // Vider les variables de stockage du mot de passe et le champ de texte pour la prochaine tentative
        _tempText = "";
        _tempTextHidden = "";
        _txtPass.text = "";
    }

    // Coroutine pour charger la scène du menu après un délai
    IEnumerator LoadMenuSceneDelay()
    {
        yield return new WaitForSeconds(2f); // Attendre 2 secondes avant de charger la scène du menu
        SceneManager.LoadScene(0); // Charger la scène du menu (index 0)
    }

    public void OnCancelClick()
    {
        // Retourne sur l'écran de retour
        _resetHighScoresPanel.SetActive(false);
        _endPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(_menuButton.gameObject);
        _errorMessage.SetActive(false);
    }
}
