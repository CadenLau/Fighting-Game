using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class Melee : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float lifetime;
    [SerializeField] private float damage;
    [SerializeField] private float knockbackStrength;
    [SerializeField] private Rigidbody2D rb;
    private GameObject owner;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (owner != null && collision.gameObject == owner)
            return;

        if (collision.gameObject.CompareTag("Projectile"))
        {
            var melee = collision.gameObject.GetComponent<Melee>();
            var projectile = collision.gameObject.GetComponent<Projectile>();
            if (melee != null && melee.GetOwner() != owner) Destroy(gameObject);
            else if (projectile != null && projectile.GetOwner() != owner) Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Special"))
        {
            var special = collision.gameObject.GetComponent<Special>();
            if (special != null && special.GetOwner() != owner) Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            PlayerScript script = collision.gameObject.GetComponent<PlayerScript>();
            script.SubtractHealth(damage);
            script.ApplyKnockback(GetComponent<Rigidbody2D>().linearVelocity, knockbackStrength);
            Destroy(gameObject);
        }
    }

    public void SetDirection(Vector2 dir)
    {
        rb.linearVelocity = dir.normalized * speed;
    }

    public void SetOwner(GameObject obj)
    {
        owner = obj;

        if (TryGetComponent<SpriteRenderer>(out var sr))
            sr.color = owner.GetComponent<SpriteRenderer>().color;
    }

    public GameObject GetOwner()
    {
        return owner;
    }
}
