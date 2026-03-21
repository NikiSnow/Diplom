using UnityEngine;

public class BulletControl : MonoBehaviour
{
    [SerializeField] private float freezeDuration = 2.5f;
    [SerializeField] private float lifeTime = 5f;

    private bool hasImpacted;

    private void Awake()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleImpact(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleImpact(other);
    }

    private void HandleImpact(Collider2D other)
    {
        if (hasImpacted || other == null)
            return;

        if (IsPlayerCollider(other))
            return;

        hasImpacted = true;

        FishController fish = other.GetComponent<FishController>();
        if (fish == null)
            fish = other.GetComponentInParent<FishController>();

        if (fish != null)
            fish.Freeze(freezeDuration);

        Destroy(gameObject);
    }

    private bool IsPlayerCollider(Collider2D other)
    {
        if (other.CompareTag("Player"))
            return true;

        if (other.attachedRigidbody != null && other.attachedRigidbody.gameObject.CompareTag("Player"))
            return true;

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Player");
    }
}