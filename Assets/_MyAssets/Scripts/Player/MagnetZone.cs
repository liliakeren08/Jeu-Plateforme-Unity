using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MagnetZone : MonoBehaviour


{

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Xp"))
        {
            other.GetComponent<XPOrb>().StartAttract(transform.parent);
        }
        if (other.CompareTag("Power"))
        {
            other.GetComponent<PowerRange>().StartAttract(transform.parent);
        }
    }
}
