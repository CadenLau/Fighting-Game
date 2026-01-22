using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Special : MonoBehaviour
{
    [SerializeField] private float angularSpeed; // degrees per second
    [SerializeField] private float radius;
    [SerializeField] private float damage;
    [SerializeField] private float knockbackStrength;
    [SerializeField] private Transform tf;

    private float startAngle;
    private float currentAngle;
    private float rotatedAngle;

    private GameObject owner;
    private Transform ownerTf;

    private void Update()
    {
        if (ownerTf == null)
            return;

        float delta = angularSpeed * Time.deltaTime;

        currentAngle += delta;
        rotatedAngle += delta;

        Vector2 offset = (Vector2)(Quaternion.AngleAxis(currentAngle, Vector3.forward) * Vector2.right) * radius;
        transform.SetPositionAndRotation((Vector2)ownerTf.position + offset, Quaternion.AngleAxis(currentAngle - 90f, Vector3.forward));

        // Stop after full circle
        if (rotatedAngle >= 360f || rotatedAngle <= -360f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (owner != null && collision.gameObject == owner)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerScript script = collision.gameObject.GetComponent<PlayerScript>();

            script.SubtractHealth(damage);

            Vector2 dir = (Vector2)tf.right;
            if (Mathf.Sign(angularSpeed) > 0) dir *= -1;
            script.ApplyKnockback(dir, knockbackStrength);
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            box.isTrigger = false;
            box.excludeLayers = LayerMask.GetMask("Player");
        }
    }

    public void SetStartDirection(Vector2 dir)
    {
        dir.Normalize();
        startAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        currentAngle = startAngle;
        rotatedAngle = 0f;
        if (-90f < startAngle && startAngle < 90f)
        {
            angularSpeed *= -1f; // Reverse direction
        }
    }

    public void SetOwner(GameObject obj)
    {
        owner = obj;
        ownerTf = owner.transform;

        if (TryGetComponent<SpriteRenderer>(out var sr))
            sr.color = owner.GetComponent<SpriteRenderer>().color;

        // float duration = 360f / Mathf.Abs(angularSpeed);
        // owner.GetComponent<PlayerScript>().SetDodgingTrue(duration);
    }

    public GameObject GetOwner()
    {
        return owner;
    }
}
