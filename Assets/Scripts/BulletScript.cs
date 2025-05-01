using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletLife = 1f;
    public float speed = 1f;
    public int damageAmount = 1; // Add a variable for damage amount

    private float timer = 0f;

    // Make sure the Bullet Prefab has the tag "Bullet" in the Inspector!

    void Update()
    {
        // Move the bullet forward based on its local right direction
        transform.position += transform.right * speed * Time.deltaTime;

        // Track lifetime and destroy the bullet if it expires
        timer += Time.deltaTime;
        if (timer > bulletLife)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            bool isBig = CompareTag("BigBullet");
            health.TakeDamage(damageAmount, isBig);
        }
        Destroy(gameObject);
    }

    // Optional: Add collision logic for other objects like walls
    // else if (other.CompareTag("Wall"))
    // {
    //     Destroy(gameObject); // Destroy bullet if it hits a wall
    // }
}