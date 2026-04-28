using UnityEngine;
using System.Collections;

public class PlayerPickUp : MonoBehaviour
{

    [SerializeField] private CircleCollider2D _collider;
    [SerializeField] private float _boostMultiplier = 10f;
    [SerializeField] private float _duration = 2f;
    private float _originalRadius;
    private Coroutine _currentRoutine;

    private void Start()
    {
        _originalRadius = _collider.radius;
    }

    [SerializeField] private Player _player;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Xp"))
        {
            XPOrb orb = other.GetComponent<XPOrb>();

            if (orb != null)
            {
                _player.AddXP(orb.Value);
            }

            Destroy(other.gameObject);

            
        }

        if (other.CompareTag("Power"))
        {
            if (_currentRoutine != null)
                StopCoroutine(_currentRoutine);

            _currentRoutine = StartCoroutine(GrowTemporarily());
            Destroy(other.gameObject);
        }
    }

    private IEnumerator GrowTemporarily()
    {
        // Agrandir
        _collider.radius = _originalRadius * _boostMultiplier;

        yield return new WaitForSeconds(_duration);

        // Reset
        _collider.radius = _originalRadius;
    }
}
