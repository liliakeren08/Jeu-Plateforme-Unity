using UnityEngine;

public class PlayerPickUp : MonoBehaviour
{

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
    }
}
