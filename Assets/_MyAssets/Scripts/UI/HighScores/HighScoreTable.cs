using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreTable : MonoBehaviour
{
    // Classe pour représenter la liste des highscores, contenant une liste de HighScoreEntry
    private class HighScores
    {
        public List<HighScoreEntry> highScoreEntryList;
    }

    [System.Serializable] // Permet de sérialiser la classe HighScoreEntry pour qu'elle puisse être utilisée dans l'éditeur Unity
    // Classe pour représenter une entrée de score élevé, contenant un score et un nom
    public class HighScoreEntry
    {
        public int score;
        public string name;
    }

    [SerializeField] private int _maxHighScoreEntries = 10; // Nombre maximum d'entrées de score élevé à afficher dans l'interface utilisateur

    private Transform _entryContainer;  // Conteneur pour les entrées de highScore dans l'interface utilisateur
    private Transform _entryTemplate; // Liste pour stocker les Transform des entrées de highScore affichées dans l'interface utilisateur
    private List<Transform> _highScoreEntryTransformList; // Liste pour stocker les Transform des entrées de highScore affichées dans l'interface utilisateur
    private HighScores highScores; // Instance de la classe HighScores pour stocker la liste des scores élevés

    private void Awake()
    {
        // PlayerPrefs.DeleteKey("highScoreTable"); // Sert si l'on désire effacer les scores dans les tests pour repartir sur une table vide

        DisplayHighScoreTable();
    }

    public void DisplayHighScoreTable()
    {
        _entryContainer = transform.Find("HighScoreEntryContainer"); // Trouve le conteneur des entrées de highScore dans l'interface utilisateur
        _entryTemplate = _entryContainer.Find("HighScoreEntryTemplate"); // Trouve le modèle d'entrée de highScore dans le conteneur
        _entryTemplate.gameObject.SetActive(false); // Désactive le modèle d'entrée de highScore pour qu'il ne soit pas visible

        // Nettoie les anciennes entrées avant d'afficher
        foreach (Transform child in _entryContainer)
        {
            if (child.name != "HighScoreEntryTemplate")
                Destroy(child.gameObject);
        }


        //Utiliser seulement pour des test ceci génère manuellement des 10 entrées pour la table
        //AddHighScoreEntry(14500, "DAV");
        //AddHighScoreEntry(3400, "ALX");
        //AddHighScoreEntry(700, "JOS");
        //AddHighScoreEntry(5500, "MAX");
        //AddHighScoreEntry(7800, "DID");
        //AddHighScoreEntry(1800, "SNY");
        //AddHighScoreEntry(100, "FRA");
        //AddHighScoreEntry(2800, "FAB");
        //AddHighScoreEntry(5400, "JON");
        //AddHighScoreEntry(5400, "LIN");

        string jsonString = PlayerPrefs.GetString("highScoreTable"); // Récupère la chaîne JSON stockée dans les PlayerPrefs sous la clé "highScoreTable"
        highScores = JsonUtility.FromJson<HighScores>(jsonString); // Désérialise la chaîne JSON en une instance de la classe HighScores

        if (highScores == null) 
        {
            AddHighScoreEntry(100, "CTR");
        }

        // Parcourt la liste des highscores pour trier les entrées par ordre décroissant de score
        for (int i = 0; i < highScores.highScoreEntryList.Count; i++)
        {
            for (int j = i + 1; j < highScores.highScoreEntryList.Count; j++)
            {
                if (highScores.highScoreEntryList[j].score > highScores.highScoreEntryList[i].score)
                {
                    //Swap
                    HighScoreEntry tmp = highScores.highScoreEntryList[i];
                    highScores.highScoreEntryList[i] = highScores.highScoreEntryList[j];
                    highScores.highScoreEntryList[j] = tmp;
                }
            }
        }

        _highScoreEntryTransformList = new List<Transform>(); // Initialise la liste des Transform des entrées de highScore affichées dans l'interface utilisateur
        int highScoreEntryCount = Mathf.Min(highScores.highScoreEntryList.Count, _maxHighScoreEntries); // Calcule le nombre d'entrées de score élevé à afficher, limité par le nombre maximum défini
        int highScoreEntryIndex = 1;
        foreach (HighScoreEntry highScoreEntry in highScores.highScoreEntryList) // Parcourt chaque entrée de score élevé dans la liste des scores élevés
        {
            if (highScoreEntryIndex <= highScoreEntryCount) // Si l'index de l'entrée de score élevé est inférieur ou égal au nombre d'entrées à afficher
            {
                // Crée une nouvelle entrée de score élevé dans l'interface utilisateur en utilisant la méthode CreateHighScoreEntryTransform
                CreateHighScoreEntryTransform(highScoreEntry, _entryContainer, _highScoreEntryTransformList); 
            }
            highScoreEntryIndex++; // Incrémente l'index de l'entrée de score élevé
        }
    }


    // Méthode qui recoit l'entrée à ajouter ainsi que l'endoit ou l'ajouter dans la table
    private void CreateHighScoreEntryTransform(HighScoreEntry highScoreEntry, Transform container, List<Transform> transformList)
    {
        //positionne l'ajout dans la liste
        float templateHeight = 45f;
        //Instancie une nouvelle ligne pour écrire la donnée
        Transform entryTransform = Instantiate(_entryTemplate, container);
        RectTransform entryRectTranform = entryTransform.GetComponent<RectTransform>(); 
        entryRectTranform.anchoredPosition = new Vector2(0f, -templateHeight * transformList.Count);
        entryTransform.gameObject.SetActive(true);

        int rank = transformList.Count + 1;
        string rankString;
        // Affiche de la position différente pour les 3 premiers
        switch (rank)
        {
            case 1: rankString = "1ST"; break;
            case 2: rankString = "2ND"; break;
            case 3: rankString = "3RD"; break;
            default:
                rankString = rank + "TH"; break;
        }
        entryTransform.Find("TxtPos").GetComponent<TextMeshProUGUI>().text = rankString;

        int score = highScoreEntry.score;
        entryTransform.Find("TxtScore").GetComponent<TextMeshProUGUI>().text = score.ToString();

        string name = highScoreEntry.name;
        entryTransform.Find("TxtName").GetComponent<TextMeshProUGUI>().text = name;

        // Couleur de fond différente pour les 3 premiers
        if (rank == 1)
        {
            // Cyan Électrique (Meilleur score)
            entryTransform.Find("background").GetComponent<Image>().color = new Color32(0, 255, 255, 80);
        }
        else if (rank == 2)
        {
            // Magenta / Rose Néon
            entryTransform.Find("background").GetComponent<Image>().color = new Color32(255, 0, 255, 70);
        }
        else if (rank == 3)
        {
            // Violet profond
            entryTransform.Find("background").GetComponent<Image>().color = new Color32(157, 0, 255, 60);
        }
        else
        {
            // Transparent pour le reste de la liste
            entryTransform.Find("background").GetComponent<Image>().color = new Color32(255, 255, 255, 0);
        }

        transformList.Add(entryTransform);
    }



    // Méthode qui recoit le score et le nom et l'ajoute à la liste
    public void AddHighScoreEntry(int p_score, string p_name)
    {
        //Creer un nouvel objet HighScore Entry à partir du score et nom recu
        HighScoreEntry highScoreEntry = new HighScoreEntry { score = p_score, name = p_name };

        // Récupère la chaîne JSON stockée dans les PlayerPrefs sous la clé "highScoreTable"
        string jsonString = PlayerPrefs.GetString("highScoreTable"); // Récupère la chaîne JSON stockée dans les PlayerPrefs sous la clé "highScoreTable"
        highScores = JsonUtility.FromJson<HighScores>(jsonString); // Désérialise la chaîne JSON en une instance de la classe HighScores

        if (highScores == null)  // Si jamais la table est vide on créer une nouvelle liste
        {
            highScores = new HighScores()
            {
                highScoreEntryList = new List<HighScoreEntry>()
            };
        }

        //Ajouter la nouvelle entrée aux HighScores
        highScores.highScoreEntryList.Add(highScoreEntry);

        //Sauvegarder les nouveaux HighScores dans le playerperfs
        string json = JsonUtility.ToJson(highScores); // Sérialise l'instance de HighScores en une chaîne JSON
        PlayerPrefs.SetString("highScoreTable", json);
        PlayerPrefs.Save();
    }

    // Méthode pour récupérer la liste des entrées de score élevé
    public List<HighScoreEntry> GetHighScoreEntries() 
    {
        return highScores?.highScoreEntryList;
    }
}
