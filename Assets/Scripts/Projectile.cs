using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 900f;
    public float damage = 25f;
    Vector3 direction;
    bool flying;

    void Update()
    {
        if (!flying) return;
        transform.position += direction * speed * Time.deltaTime;
        if (transform.position.y > 2000f || transform.position.y < -100f || transform.position.x < -200f || transform.position.x > 1280f)
        {
            Return();
        }
    }

    public void Launch(Vector3 pos, Vector3 dir)
    {
        transform.position = pos;
        direction = dir.normalized;
        flying = true;
        gameObject.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!flying) return;
        var enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Return();
        }
    }

    void Return()
    {
        flying = false;
        GameController.Instance.projectilePool.Return(gameObject);
    }
}
