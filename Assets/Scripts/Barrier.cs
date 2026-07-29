using UnityEngine;

public class Barrier : MonoBehaviour
{
    public float maxHP = 3000f;
    public float HP { get; private set; }

    void Start()
    {
        HP = maxHP;
    }

    public void TakeDamage(float dmg)
    {
        HP -= dmg;
        if (HP <= 0)
        {
            HP = 0;
            if (GameController.Instance != null)
                GameController.Instance.GameOver();
        }
    }

    public void Heal(float amount)
    {
        HP = Mathf.Min(maxHP, HP + amount);
    }
}
