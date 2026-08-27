using UnityEngine;
using System.Collections;

// Parent script for the Sunflower Effect
public class Effect_Sunflower_Alpha : MonoBehaviour
{
    public Transform visualChild; // The sprite child to rotate visually
    public float visualRotateSpeed = 360f; // degrees per second
    
    private Vector3 travelDirection;
    private float speed;
    private float damage;
    private Rigidbody2D rb;

    public void Initialize(Vector3 direction, float spd, float dmg, int rarity)
    {
        travelDirection = direction.normalized;
        speed = spd * 0.5f; // Sunflower travels slower than main bullet?
        damage = dmg * 0.3f; // Tick damage
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        
        rb.isKinematic = false;
        rb.gravityScale = 0f;
        rb.velocity = travelDirection * (speed * 0.01f);
        
        // Find visual child if not set
        if (visualChild == null && transform.childCount > 0)
        {
            visualChild = transform.GetChild(0);
        }
        
        Destroy(gameObject, 5f); // Temporary life time
    }

    private void Update()
    {
        if (visualChild != null)
        {
            visualChild.Rotate(0, 0, visualRotateSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("MidBoss") || collision.CompareTag("Boss"))
        {
            Health hp = collision.GetComponent<Health>();
            if (hp != null) hp.TakeDamage(damage);
        }
    }
}

