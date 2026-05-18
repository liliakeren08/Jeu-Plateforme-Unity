using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreTable : MonoBehaviour
{
    // Classe pour reprÃ©senter la liste des highscores, contenant une liste de HighScoreEntry
    private class HighScores
    {
        public List<HighScoreEntry> highScoreEntryList;
    }

    [System.Serializable] // Permet de sÃ©rialiser la classe HighScoreEntry pour qu'elle puisse Ãªtre utilisÃ©e dans l'Ã©diteur Unity
    // Classe pour reprÃ©senter une entrÃ©e de score Ã©levÃ©, contenant un score et un nom
    public class HighScoreEntry
    {
        public int score;
        public string name;
    }

    [SerializeField] private int _maxHighScoreEntries = 10; // Nombre maximum d'entrÃ©es de score Ã©levÃ© Ã  afficher dans l'interface utilisateur

    private Transform _entryContainer;  // Conteneur pour les entrÃ©es de highScore dans l'interface utilisateur
    private Transform _entryTemplate; // Liste pour stocker les Transform des entrÃ©es de highScore affichÃ©es dans l'interface utilisateur
    private List<Transform> _highScoreEntryTransformList; // Liste pour stocker les Transform des entrÃ©es de highScore affichÃ©es dans l'interface utilisateur
    private HighScores highScores; // Instance de la classe HighScores pour stocker la liste des scores Ã©levÃ©s

    private void Awake()
    {
        // PlayerPrefs.DeleteKey("highScoreTable"); // Sert si l'on dÃ©sire effacer les scores dans les tests pour repartir sur une table vide

        DisplayHighScoreTable();
    }

    public void DisplayHighScoreTable()
    {
        _entryContainer = transform.Find("HighScoreEntryContainer"); // Trouve le conteneur des entrÃ©es de highScore dans l'interface utilisateur
        _entryTemplate = _entryContainer.Find("HighScoreEntryTemplate"); // Trouve le modÃ¨le d'entrÃ©e de highScore dans le conteneur
        _entryTemplate.gameObject.SetActive(false); // DÃ©sactive le modÃ¨le d'entrÃ©e de highScore pour qu'il ne soit pas visible

        // Nettoie les anciennes entrÃ©es avant d'afficher
        foreach (Transform child in _entryContainer)
        {
            if (child.name != "HighScoreEntryTemplate")
                Destroy(child.gameObject);
        }


        //Utiliser seulement pour des test ceci gÃ©nÃ¨re manuellement des 10 entrÃ©es pour la table
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

        string jsonString = PlayerPrefs.GetString("highScoreTable"); // RÃ©cupÃ¨re la chaÃ®ne JSON stockÃ©e dans les PlayerPrefs sous la clÃ© "highScoreTable"
        highScores = JsonUtility.FromJson<HighScores>(jsonString); // DÃ©sÃ©rialise la chaÃ®ne JSON en une instance de la classe HighScores

        // SÉCURITÉ : Si la table ou sa liste d'entrées est nulle (suite à une réinitialisation), on l'initialise proprement
        if (highScores == null || highScores.highScoreEntryList == null) 
        {
            AddHighScoreEntry(100, "CTR");
            // Re-charger la table après l'initialisation de sécurité
            jsonString = PlayerPrefs.GetString("highScoreTable");
            highScores = JsonUtility.FromJson<HighScores>(jsonString);
        }

        // Parcourt la liste des highscores pour trier les entrÃ©es par ordre dÃ©croissant de score
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

        _highScoreEntryTransformList = new List<Transform>(); // Initialise la liste des Transform des entrÃ©es de highScore affichÃ©es dans l'interface utilisateur
        int highScoreEntryCount = Mathf.Min(highScores.highScoreEntryList.Count, _maxHighScoreEntries); // Calcule le nombre d'entrÃ©es de score Ã©levÃ© Ã  afficher, limitÃ© par le nombre maximum dÃ©fini
        int highScoreEntryIndex = 1;
        foreach (HighScoreEntry highScoreEntry in highScores.highScoreEntryList) // Parcourt chaque entrÃ©e de score Ã©levÃ© dans la liste des scores Ã©levÃ©s
        {
            if (highScoreEntryIndex <= highScoreEntryCount) // Si l'index de l'entrÃ©e de score Ã©levÃ© est infÃ©rieur ou Ã©gal au nombre d'entrÃ©es Ã  afficher
            {
                // CrÃ©e une nouvelle entrÃ©e de score Ã©levÃ© dans l'interface utilisateur en utilisant la mÃ©thode CreateHighScoreEntryTransform
                CreateHighScoreEntryTransform(highScoreEntry, _entryContainer, _highScoreEntryTransformList); 
            }
            highScoreEntryIndex++; // IncrÃ©mente l'index de l'entrÃ©e de score Ã©levÃ©
        }
    }


    // MÃ©thode qui recoit l'entrÃ©e Ã  ajouter ainsi que l'endoit ou l'ajouter dans la table
    private void CreateHighScoreEntryTransform(HighScoreEntry highScoreEntry, Transform container, List<Transform> transformList)
    {
        //positionne l'ajout dans la liste
        float templateHeight = 45f;
        //Instancie une nouvelle ligne pour Ã©crire la donnÃ©e
        Transform entryTransform = Instantiate(_entryTemplate, container);
        RectTransform entryRectTranform = entryTransform.GetComponent<RectTransform>(); 
        entryRectTranform.anchoredPosition = new Vector2(0f, -templateHeight * transformList.Count);
        entryTransform.gameObject.SetActive(true);

        int rank = transformList.Count + 1;
        string rankString;
        // Affiche de la position diffÃ©rente pour les 3 premiers
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

        // Couleur de fond diffÃ©rente pour les 3 premiers
        if (rank == 1)
        {
            // Cyan Ã‰lectrique (Meilleur score)
            entryTransform.Find("background").GetComponent<Image>().color = new Color32(0, 255, 255, 80);
        }
        else if (rank == 2)
        {
            // Magenta / Rose NÃ©on
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
        string jsonString = PlayerPrefs.GetString("highScoreTable"); 
        highScores = JsonUtility.FromJson<HighScores>(jsonString); 

        if (highScores == null)  // Si jamais la table est vide on créer une nouvelle liste
        {
            highScores = new HighScores()
            {
                highScoreEntryList = new List<HighScoreEntry>()
            };
        }

        // Ajouter la nouvelle entrée aux HighScores
        highScores.highScoreEntryList.Add(highScoreEntry);

        // Trier la liste par ordre décroissant pour s'assurer que les meilleurs scores soient en premier
        highScores.highScoreEntryList.Sort((a, b) => b.score.CompareTo(a.score));

        // Garder uniquement les 10 meilleurs scores (tronquer le reste pour éviter une croissance infinie de la sauvegarde)
        if (highScores.highScoreEntryList.Count > _maxHighScoreEntries)
        {
            highScores.highScoreEntryList.RemoveRange(_maxHighScoreEntries, highScores.highScoreEntryList.Count - _maxHighScoreEntries);
        }

        // Sauvegarder les nouveaux HighScores dans le playerperfs
        string json = JsonUtility.ToJson(highScores); 
        PlayerPrefs.SetString("highScoreTable", json);
        PlayerPrefs.Save();
    }

    // MÃ©thode pour rÃ©cupÃ©rer la liste des entrÃ©es de score Ã©levÃ©
    public List<HighScoreEntry> GetHighScoreEntries() 
    {
        return highScores?.highScoreEntryList;
    }
}
