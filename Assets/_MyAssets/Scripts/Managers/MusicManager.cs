using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Nécessaire pour le callback

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Configuration")]
    [SerializeField] private AudioClip _clipMusic;

    [Header("UI Configuration")]
    [SerializeField] private Image _muteImageDisplay; // On glisse l'image du bouton ici
    [SerializeField] private Sprite _soundOnSprite;   // Ton image "Son ON" (Cyan)
    [SerializeField] private Sprite _soundOffSprite;  // Ton image "Son OFF" (Magenta)

    private InputSystem_Actions _inputSystem_Actions;
    private AudioSource _audioSource;
    private bool _isMuted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 1. Logique Input System du prof
        _inputSystem_Actions = new InputSystem_Actions();
        _inputSystem_Actions.Player.Enable();
        _inputSystem_Actions.Player.Mute.performed += Mute_performed;

        // 2. Initialisation de l'AudioSource
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = _clipMusic;
        _audioSource.loop = true;
        _audioSource.Play();

        // 3. Mise à jour visuelle initiale
        UpdateVisuals();
    }

    private void Mute_performed(InputAction.CallbackContext obj)
    {
        OnMuteClick();
    }

    public void OnMuteClick()
    {
        // On inverse l'état
        _isMuted = !_isMuted;

        // On applique au moteur audio
        _audioSource.mute = _isMuted;

        // On change l'image
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_muteImageDisplay != null)
        {
            // On switch le sprite selon l'état
            _muteImageDisplay.sprite = _isMuted ? _soundOffSprite : _soundOnSprite;
        }
    }

    private void OnDestroy()
    {
        // Nettoyage de l'Input System (très important)
        if (_inputSystem_Actions != null)
        {
            _inputSystem_Actions.Player.Mute.performed -= Mute_performed;
            _inputSystem_Actions.Player.Disable();
        }
    }
}