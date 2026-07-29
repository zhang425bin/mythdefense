using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 100f;
    public float damage = 10f;
    public float hp = 20f;
    SpriteRenderer sr;
    bool active;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Setup(Sprite sprite, float hpVal, float speedVal, float dmgVal)
    {
        if (sr != null) sr.sprite = sprite;
        hp = hpVal;
        speed = speedVal;
        damage = dmgVal;
        active = true;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!active) return;
        transform.position += Vector3.down * speed * Time.deltaTime;
        if (transform.position.y < 80f)
        {
            if (GameController.Instance != null && GameController.Instance.barrier != null)
                GameController.Instance.barrier.TakeDamage(damage);
            Die(false);
        }
    }

    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        if (hp <= 0) Die(true);
    }

    void Die(bool giveXP)
    {
        active = false;
        if (giveXP && GameController.Instance != null)
            GameController.Instance.AddXP(10f + GameController.Instance.wave);
        GameController.Instance.RemoveEnemy(gameObject);
    }

    void OnDisable()
    {
        active = false;
    }
}
