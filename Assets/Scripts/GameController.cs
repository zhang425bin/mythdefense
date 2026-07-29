using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [Header("Sprites")]
    public Sprite playerSprite;
    public Sprite[] enemySprites;
    public Sprite projectileSprite;
    public Sprite barrierSprite;
    public Sprite bgSprite;
    public Sprite hpIcon;
    public Sprite[] skillIcons;

    [Header("Balance")]
    public float playerFireRate = 0.35f;
    public float projectileSpeed = 900f;
    public float projectileDamage = 25f;
    public int maxActiveEnemies = 25;
    public float spawnInterval = 4f;

    [Header("State")]
    public int wave = 1;
    public int maxWaves = 20;
    public float gameTime = 0f;
    public int score = 0;
    public float xp = 0f;
    public float xpToNext = 80f;
    public int level = 1;
    public bool paused = false;
    public bool gameOver = false;
    public bool skillChoosing = false;

    public List<GameObject> activeEnemies = new List<GameObject>();
    public Barrier barrier;
    public ObjectPool projectilePool;
    public ObjectPool enemyPool;

    Camera mainCam;
    GameObject playerGO;
    GUIStyle labelStyle;
    GUIStyle bigLabelStyle;
    GUIStyle boxStyle;
    GUIStyle buttonStyle;
    Texture2D xpTex, hpTex, bgTex;
    List<Skill> skills;
    Skill[] currentChoices;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        LoadSprites();
        SetupCamera();
        CreateBackground();
        CreateBarrier();
        CreatePlayer();
        CreatePools();
        BuildSkills();
        InitStyles();
        StartCoroutine(SpawnLoop());
    }

    Sprite MakeSprite(Texture2D tex)
    {
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
    }

    void LoadSprites()
    {
        playerSprite = MakeSprite(Resources.Load<Texture2D>("Sprites/playerShip2_blue"));
        enemySprites = new Sprite[]
        {
            MakeSprite(Resources.Load<Texture2D>("Sprites/enemyBlack1")),
            MakeSprite(Resources.Load<Texture2D>("Sprites/enemyBlue2")),
            MakeSprite(Resources.Load<Texture2D>("Sprites/enemyGreen3")),
            MakeSprite(Resources.Load<Texture2D>("Sprites/enemyRed4"))
        };
        projectileSprite = MakeSprite(Resources.Load<Texture2D>("Sprites/laserBlue16"));
        barrierSprite = MakeSprite(Resources.Load<Texture2D>("Sprites/laserRed01"));
        bgSprite = MakeSprite(Resources.Load<Texture2D>("Sprites/darkPurple"));
        hpIcon = MakeSprite(Resources.Load<Texture2D>("Sprites/playerLife1_red"));
        skillIcons = new Sprite[]
        {
            MakeSprite(Resources.Load<Texture2D>("Sprites/bolt_gold")),
            MakeSprite(Resources.Load<Texture2D>("Sprites/shield_gold")),
            MakeSprite(Resources.Load<Texture2D>("Sprites/star_gold")),
            MakeSprite(Resources.Load<Texture2D>("Sprites/pill_blue"))
        };
    }

    void SetupCamera()
    {
        mainCam = Camera.main;
        if (mainCam == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            mainCam = camGO.AddComponent<Camera>();
        }
        mainCam.orthographic = true;
        mainCam.orthographicSize = 960f;
        mainCam.transform.position = new Vector3(540f, 960f, -10f);
        mainCam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
        mainCam.clearFlags = CameraClearFlags.SolidColor;
    }

    void CreateBackground()
    {
        var go = new GameObject("Background");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = bgSprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(1200f, 2200f);
        sr.sortingOrder = -10;
        go.transform.position = new Vector3(540f, 960f, 0f);
    }

    void CreateBarrier()
    {
        var go = new GameObject("Barrier");
        go.transform.position = new Vector3(540f, 80f, 0f);
        go.transform.localScale = new Vector3(30f, 1.5f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = barrierSprite;
        sr.sortingOrder = 0;
        barrier = go.AddComponent<Barrier>();
        barrier.maxHP = 3000f;
    }

    void CreatePlayer()
    {
        playerGO = new GameObject("Player");
        playerGO.transform.position = new Vector3(540f, 260f, 0f);
        playerGO.transform.localScale = new Vector3(1.6f, 1.6f, 1f);
        var sr = playerGO.AddComponent<SpriteRenderer>();
        sr.sprite = playerSprite;
        sr.sortingOrder = 10;
        var col = playerGO.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(60f, 60f);
        var rb = playerGO.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
        var player = playerGO.AddComponent<Player>();
        player.fireRate = playerFireRate;
    }

    void CreatePools()
    {
        var ppGO = new GameObject("ProjectilePool");
        ppGO.transform.SetParent(transform);
        projectilePool = ppGO.AddComponent<ObjectPool>();
        projectilePool.Initialize(CreateProjectile, 60);

        var epGO = new GameObject("EnemyPool");
        epGO.transform.SetParent(transform);
        enemyPool = epGO.AddComponent<ObjectPool>();
        enemyPool.Initialize(CreateEnemy, maxActiveEnemies);
    }

    GameObject CreateProjectile()
    {
        var go = new GameObject("Projectile");
        go.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = projectileSprite;
        sr.sortingOrder = 5;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 15f;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
        var p = go.AddComponent<Projectile>();
        p.speed = projectileSpeed;
        p.damage = projectileDamage;
        go.SetActive(false);
        return go;
    }

    GameObject CreateEnemy()
    {
        var go = new GameObject("Enemy");
        go.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(50f, 50f);
        var rb = go.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
        var e = go.AddComponent<Enemy>();
        go.SetActive(false);
        return go;
    }

    void BuildSkills()
    {
        skills = new List<Skill>
        {
            new Skill
            {
                name = "攻速提升",
                description = "攻击速度 +20%",
                icon = skillIcons[0],
                apply = () =>
                {
                    playerFireRate *= 0.8f;
                    if (playerGO) playerGO.GetComponent<Player>().fireRate = playerFireRate;
                }
            },
            new Skill
            {
                name = "伤害提升",
                description = "飞剑伤害 +25%",
                icon = skillIcons[1],
                apply = () => { projectileDamage *= 1.25f; }
            },
            new Skill
            {
                name = "多重飞剑",
                description = "每次多发射 1 支飞剑",
                icon = skillIcons[2],
                apply = () => { if (playerGO) playerGO.GetComponent<Player>().extraProjectiles++; }
            },
            new Skill
            {
                name = "阵法加固",
                description = "防线立即回复 500 点生命",
                icon = skillIcons[3],
                apply = () => { if (barrier) barrier.Heal(500f); }
            }
        };
    }

    void InitStyles()
    {
        labelStyle = new GUIStyle();
        labelStyle.fontSize = 40;
        labelStyle.normal.textColor = Color.white;
        labelStyle.alignment = TextAnchor.MiddleLeft;

        bigLabelStyle = new GUIStyle(labelStyle);
        bigLabelStyle.fontSize = 48;
        bigLabelStyle.alignment = TextAnchor.MiddleCenter;

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.75f));

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 40;
        buttonStyle.normal.textColor = Color.white;

        xpTex = MakeTex(2, 2, new Color(0.2f, 0.7f, 1f));
        hpTex = MakeTex(2, 2, new Color(0.9f, 0.2f, 0.2f));
        bgTex = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.7f));
    }

    Texture2D MakeTex(int w, int h, Color col)
    {
        var tex = new Texture2D(w, h);
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    IEnumerator SpawnLoop()
    {
        while (!gameOver)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (skillChoosing || paused) continue;
            SpawnWave();
            wave++;
            if (wave > maxWaves) wave = maxWaves;
        }
    }

    void SpawnWave()
    {
        int count = Mathf.Min(2 + wave, maxActiveEnemies - activeEnemies.Count);
        for (int i = 0; i < count; i++)
        {
            var go = enemyPool.Get();
            if (go == null) continue;
            var e = go.GetComponent<Enemy>();
            Sprite s = enemySprites[UnityEngine.Random.Range(0, enemySprites.Length)];
            float hp = 20f + wave * 8f;
            float speed = 80f + wave * 6f;
            float dmg = 8f + wave * 2f;
            e.Setup(s, hp, speed, dmg);
            float x = UnityEngine.Random.Range(120f, 960f);
            go.transform.position = new Vector3(x, 1900f, 0f);
            activeEnemies.Add(go);
        }
    }

    public void RemoveEnemy(GameObject go)
    {
        activeEnemies.Remove(go);
        enemyPool.Return(go);
    }

    public void AddXP(float amount)
    {
        if (gameOver || skillChoosing) return;
        xp += amount;
        if (xp >= xpToNext)
        {
            xp -= xpToNext;
            level++;
            xpToNext *= 1.25f;
            OpenSkillChoice();
        }
    }

    void OpenSkillChoice()
    {
        skillChoosing = true;
        Time.timeScale = 0f;
        currentChoices = new Skill[3];
        for (int i = 0; i < 3; i++)
        {
            currentChoices[i] = skills[UnityEngine.Random.Range(0, skills.Count)];
        }
    }

    void Update()
    {
        if (gameOver || skillChoosing) return;
        gameTime += Time.deltaTime;
    }

    void OnGUI()
    {
        if (labelStyle == null) return;
        float sw = Screen.width;
        float sh = Screen.height;

        // Top center wave/time
        GUI.Label(new Rect(sw * 0.5f - 300f, 20f, 600f, 90f), $"波次：{wave}/{maxWaves}\n时间：{gameTime:F1}s", bigLabelStyle);

        // FPS left
        float fps = 1f / Mathf.Max(Time.deltaTime, 0.0001f);
        GUI.Label(new Rect(20f, 20f, 220f, 50f), $"FPS:{fps:F0}", labelStyle);

        // XP bar bottom center
        GUI.DrawTexture(new Rect(sw * 0.1f, sh - 90f, sw * 0.8f, 40f), bgTex);
        GUI.DrawTexture(new Rect(sw * 0.1f, sh - 90f, sw * 0.8f * Mathf.Clamp01(xp / xpToNext), 40f), xpTex);
        GUI.Label(new Rect(sw * 0.1f, sh - 140f, sw * 0.8f, 50f), $"等级 {level}  经验", labelStyle);

        // HP bottom right
        float hpRatio = barrier ? barrier.HP / barrier.maxHP : 1f;
        GUI.DrawTexture(new Rect(sw - 320f, sh - 220f, 280f, 30f), bgTex);
        GUI.DrawTexture(new Rect(sw - 320f, sh - 220f, 280f * Mathf.Clamp01(hpRatio), 30f), hpTex);
        if (hpIcon) GUI.DrawTexture(new Rect(sw - 365f, sh - 230f, 40f, 40f), hpIcon.texture, ScaleMode.ScaleToFit, true);
        GUI.Label(new Rect(sw - 270f, sh - 220f, 240f, 30f), $"{barrier.HP:F0}/{barrier.maxHP}", labelStyle);

        // Right side skill/weapon icons
        float iconSize = Mathf.Min(100f, sw * 0.12f);
        float rightX = sw - iconSize - 20f;
        float startY = sh * 0.35f;
        if (playerSprite) GUI.DrawTexture(new Rect(rightX, startY, iconSize, iconSize), playerSprite.texture, ScaleMode.ScaleToFit, true);
        for (int i = 0; i < 4; i++)
        {
            var icon = skillIcons[i % skillIcons.Length];
            if (icon) GUI.DrawTexture(new Rect(rightX, startY + (i + 1) * (iconSize + 16f), iconSize, iconSize), icon.texture, ScaleMode.ScaleToFit, true);
            GUI.Label(new Rect(rightX - 55f, startY + (i + 1) * (iconSize + 16f) + 28f, 50f, 40f), (i + 1).ToString(), labelStyle);
        }

        // Skill choice
        if (skillChoosing && currentChoices != null)
        {
            GUI.Box(new Rect(0, 0, sw, sh), "", boxStyle);
            GUI.Label(new Rect(sw * 0.5f - 300f, sh * 0.2f, 600f, 80f), "选择一个强化", bigLabelStyle);
            float btnW = sw * 0.72f;
            float btnH = 140f;
            for (int i = 0; i < 3; i++)
            {
                var sk = currentChoices[i];
                if (sk == null) continue;
                Rect r = new Rect(sw * 0.5f - btnW * 0.5f, sh * 0.35f + i * (btnH + 30f), btnW, btnH);
                if (GUI.Button(r, $"{sk.name}\n{sk.description}", buttonStyle))
                {
                    sk.apply();
                    skillChoosing = false;
                    Time.timeScale = 1f;
                }
                if (sk.icon) GUI.DrawTexture(new Rect(r.x + 20f, r.y + 20f, 100f, 100f), sk.icon.texture, ScaleMode.ScaleToFit, true);
            }
        }

        if (gameOver)
        {
            GUI.Box(new Rect(0, 0, sw, sh), "", boxStyle);
            GUI.Label(new Rect(sw * 0.5f - 300f, sh * 0.38f, 600f, 120f), "阵法被破\n游戏结束", bigLabelStyle);
            if (GUI.Button(new Rect(sw * 0.5f - 220f, sh * 0.6f, 440f, 100f), "重新开始", buttonStyle))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
    }

    public void GameOver()
    {
        gameOver = true;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
