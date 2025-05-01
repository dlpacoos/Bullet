using UnityEngine;

public class MapBounds : MonoBehaviour
{
    // Called when any Collider2D enters this trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        // If it’s a small or big bullet, destroy it
        if (other.CompareTag("Bullet") || other.CompareTag("BigBullet"))
        {
            Destroy(other.gameObject);
        }
    }
}
