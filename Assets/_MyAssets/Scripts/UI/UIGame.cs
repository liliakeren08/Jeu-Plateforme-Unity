//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;

//public class UIGame : MonoBehaviour
//{
//    [SerializeField] private TextMeshProUGUI _scoreText; // Référence au composant TextMeshProUGUI pour afficher le score du joueur
//    [SerializeField] private Image _playerLivesImage; // Référence au composant Image pour afficher les vies du joueur
//    [SerializeField] private Sprite[] _playerLifesSpritesArray; // Tableau de sprites représentant les différentes vies du joueur

//    private void Start()
//    {
//        // Initialisation de l'affichage des vies du joueur avec le sprite correspondant au nombre de vies maximum
//        ChangeLivesDisplayImage(_playerLifesSpritesArray.Length - 1); 
//        UpdateScore(); // Mise à jour de l'affichage du score du joueur au démarrage

//        GameManager.Instance.OnEnemyDestroyed += GameManager_OnEnemyDestroyed;
//    }

//    private void GameManager_OnEnemyDestroyed(object sender, GameManager.OnEnemyDestroyedEventArgs e)
//    {
//        if(e.DestroyedObjectTag == "Player")
//        {
//            // Mise à jour de l'affichage des vies du joueur lorsque le joueur perd une vie
//            Player player = FindAnyObjectByType<Player>();
//            if (player != null)
//            {
//                ChangeLivesDisplayImage(player.PlayerLifes);
//            }
//            else
//            {
//                ChangeLivesDisplayImage(0); // Si le joueur n'est pas trouvé, afficher l'image correspondant à 0 vie
//            }
//        }
//        else if(e.DestroyedObjectTag == "Laser")
//        {
//            UpdateScore(); // Mise à jour de l'affichage du score du joueur lorsque le joueur détruit un ennemi
//        }
//    }

//    private void ChangeLivesDisplayImage(int p_noImage)
//    {
//        if(p_noImage < 0)
//        {
//            p_noImage = 0;
//        }

//        // Mise à jour de l'image des vies du joueur en fonction du nombre de vies restantes
//        _playerLivesImage.sprite = _playerLifesSpritesArray[p_noImage]; 
//    }

//    private void UpdateScore()
//    {
//        _scoreText.text = $"Pointage : {GameManager.Instance.PlayerScore}"; // Mise à jour du texte du score avec le score actuel du joueur
//    }
//}


