using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5f;
    public float rotationSpeed = 3f;
    public int health = 3;
    public GameObject explosionEffect;
    public AudioClip deathSound;

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<TankController>().transform;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
    }

    public void TakeDamage(Vector3 hitPoint)
    {
        health--;
        if (health <= 0)
        {
            if (explosionEffect != null)
                Instantiate(explosionEffect, hitPoint, Quaternion.identity);

            AudioSource.PlayClipAtPoint(deathSound, transform.position, 1f);

            var player = FindObjectOfType<TankController>();
            if (player) player.AddKill();

            Destroy(gameObject);
        }
    }
}