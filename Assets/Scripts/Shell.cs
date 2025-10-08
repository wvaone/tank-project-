using UnityEngine;

public class Shell : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(transform.position);
        }
        Destroy(gameObject);
    }
}