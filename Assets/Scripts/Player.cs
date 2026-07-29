using UnityEngine;

public class Player : MonoBehaviour
{
    public float fireRate = 0.35f;
    public int extraProjectiles = 0;
    float timer;

    void Update()
    {
        if (GameController.Instance == null || GameController.Instance.skillChoosing || GameController.Instance.gameOver) return;
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Fire();
            timer = fireRate;
        }
    }

    void Fire()
    {
        var target = FindClosestEnemy();
        if (target == null) return;
        int count = 1 + extraProjectiles;
        Vector3 baseDir = (target.position - transform.position).normalized;
        for (int i = 0; i < count; i++)
        {
            var p = GameController.Instance.projectilePool.Get();
            if (p == null) continue;
            var proj = p.GetComponent<Projectile>();
            Vector3 dir = baseDir;
            if (count > 1)
            {
                float angle = (i - (count - 1) * 0.5f) * 12f;
                dir = Quaternion.Euler(0, 0, angle) * baseDir;
            }
            proj.Launch(transform.position, dir);
        }
    }

    Transform FindClosestEnemy()
    {
        float best = float.MaxValue;
        Transform t = null;
        foreach (var e in GameController.Instance.activeEnemies)
        {
            if (e == null || !e.activeSelf) continue;
            float d = (e.transform.position - transform.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                t = e.transform;
            }
        }
        return t;
    }
}
