using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Configuration")]
    [SerializeField] private AudioClip _clipMusic;

    [Header("UI Configuration")]
    [SerializeField] private Image _muteImageDisplay;
    [SerializeField] private Sprite _soundOnSprite;
    [SerializeField] private Sprite _soundOffSprite;

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

    private Coroutine _duckCoroutine;

    private void Update()
    {
        // S'assurer que la musique de fond joue en continu (sauf si on est en sourdine)
        if (_audioSource != null && !_audioSource.isPlaying && !_isMuted)
        {
            _audioSource.Play();
        }

        bool mutePressed = false;

        // 1. Détection via le Nouveau Système d'Input (si disponible)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.mKey.wasPressedThisFrame ||
                Keyboard.current.digit6Key.wasPressedThisFrame ||
                Keyboard.current.numpad6Key.wasPressedThisFrame)
            {
                mutePressed = true;
            }
        }

        // 2. Détection via le Système d'Input Classique (très fiable au clavier dans l'Éditeur)
        try
        {
            if (Input.GetKeyDown(KeyCode.M) ||
                Input.GetKeyDown(KeyCode.Alpha6) ||
                Input.GetKeyDown(KeyCode.Keypad6))
            {
                mutePressed = true;
            }
        }
        catch (System.Exception)
        {
            // Ignore si l'ancien système est complètement désactivé dans les Player Settings
        }

        if (mutePressed)
        {
            OnMuteClick();
        }
    }

    /// <summary>
    /// Atténue temporairement le volume de la musique de fond (effet Ducking)
    /// pour mettre en valeur les effets sonores d'impact ou d'attaque.
    /// </summary>
    public void DuckMusic(float targetVolume = 0.3f, float duration = 0.4f)
    {
        if (_isMuted || _audioSource == null) return;

        if (_duckCoroutine != null)
        {
            StopCoroutine(_duckCoroutine);
        }

        _duckCoroutine = StartCoroutine(DuckMusicCoroutine(targetVolume, duration));
    }

    private System.Collections.IEnumerator DuckMusicCoroutine(float targetVolume, float duration)
    {
        float originalVolume = 1.0f;
        float elapsed = 0f;
        float fadeTime = 0.05f; // Transition très rapide vers le bas

        // Descendre le volume rapidement (duck)
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(originalVolume, targetVolume, elapsed / fadeTime);
            yield return null;
        }
        _audioSource.volume = targetVolume;

        // Attendre pendant la durée de l'effet sonore
        yield return new WaitForSeconds(duration);

        // Remonter le volume progressivement
        elapsed = 0f;
        float restoreTime = 0.25f; // Transition fluide vers le haut
        while (elapsed < restoreTime)
        {
            elapsed += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(targetVolume, originalVolume, elapsed / restoreTime);
            yield return null;
        }
        _audioSource.volume = originalVolume;
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