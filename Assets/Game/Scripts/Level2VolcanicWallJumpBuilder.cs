using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class Level2VolcanicWallJumpBuilder : MonoBehaviour
{
    const string RootName = "LEVEL 2 - PROFUNDIDADES VOLCANICAS";

    static Sprite whiteCircle;
    static Sprite whiteSquare;
    static Sprite deepSeaBg;
    static Sprite volcanicBg;
    static Sprite nereoSprite;
    static Sprite spiderSprite;
    static Sprite[] officialLevel2Layers;

    Transform root;
    Camera sceneCamera;

    void OnEnable()
    {
        if (Application.isPlaying)
        {
            BuildLevel();
            return;
        }

        BuildLevel();
    }

    void Start()
    {
        if (Application.isPlaying) BuildLevel();
    }

    void Update()
    {
        if (Application.isPlaying) return;
        if (GameObject.Find(RootName) == null) BuildLevel();
    }

    void BuildLevel()
    {
        Time.timeScale = 1f;
        ClearGeneratedRoots();
        LoadSprites();

        GameObject rootObject = new GameObject(RootName);
        root = rootObject.transform;

        CreateCamera();
        CreateBackgroundLayers();
        CreateVolcanicRoute();
        CreateGameplayObjects();
        CreatePresentationLabels();
    }

    void ClearGeneratedRoots()
    {
        Transform[] allTransforms = FindObjectsOfType<Transform>(true);

        for (int i = allTransforms.Length - 1; i >= 0; i--)
        {
            Transform candidate = allTransforms[i];
            if (candidate.parent != null) continue;
            if (candidate.gameObject == gameObject) continue;
            if (!candidate.name.StartsWith(RootName)) continue;

            if (Application.isPlaying) Destroy(candidate.gameObject);
            else DestroyImmediate(candidate.gameObject);
        }
    }

    void LoadSprites()
    {
        whiteCircle = CreateCircleSprite();
        whiteSquare = CreateSquareSprite();
        deepSeaBg = SpriteFromResource("Level2/BG_Level2_DeepSea_Base", 100f);
        volcanicBg = SpriteFromResource("Level2/BG_Level2_Volcanic_Midground", 100f);
        nereoSprite = SpriteFromResource("Level2/Nereo_Babosa", 220f);
        spiderSprite = SpriteFromResource("Level2/Elias_AranaMar", 250f);

        officialLevel2Layers = new Sprite[7];
        for (int i = 0; i < officialLevel2Layers.Length; i++)
        {
            officialLevel2Layers[i] = SpriteFromResource("Level2Official/Background_Level2-" + (i + 1), 100f);
        }
    }

    Sprite SpriteFromResource(string path, float pixelsPerUnit)
    {
        Texture2D texture = Resources.Load<Texture2D>(path);
        if (texture == null) return whiteSquare;

        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit
        );
    }

    void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.transform.SetParent(root, false);
        cameraObject.transform.position = new Vector3(0.45f, -5.65f, -10f);
        cameraObject.tag = "MainCamera";

        sceneCamera = cameraObject.AddComponent<Camera>();
        sceneCamera.orthographic = true;
        sceneCamera.orthographicSize = 3.95f;
        sceneCamera.backgroundColor = new Color(0.01f, 0.02f, 0.04f);
    }

    void CreateBackgroundLayers()
    {
        CreateSprite(
            "Base oscura completa para evitar vacios",
            new Vector3(0f, 0f, 9.5f),
            new Vector2(36f, 46f),
            new Color(0.005f, 0.018f, 0.030f),
            -100,
            whiteSquare
        );

        for (int i = 0; i < 4; i++)
        {
            CreateSprite(
                "Fondo oficial base oscuro " + (i + 1),
                new Vector3(0f, -8f + i * 10.5f, 8f),
                new Vector2(2.35f, 1.72f),
                Color.white,
                -90,
                OfficialLayer(0, deepSeaBg)
            );
        }

        for (int i = 0; i < 3; i++)
        {
            CreateSprite(
                "Rocas oficiales lejanas " + (i + 1),
                new Vector3(0.1f, -7f + i * 11.5f, 6f),
                new Vector2(2.10f, 1.58f),
                new Color(0.72f, 0.88f, 1f, 0.62f),
                -70,
                OfficialLayer(2, volcanicBg)
            );

            CreateSprite(
                "Rocas oficiales lejanas extension izquierda " + (i + 1),
                new Vector3(-5.8f, -7.2f + i * 11.5f, 6f),
                new Vector2(-1.15f, 1.42f),
                new Color(0.72f, 0.88f, 1f, 0.34f),
                -72,
                OfficialLayer(2, volcanicBg)
            );
        }

        CreateSprite("Capa oficial de corales y burbujas", new Vector3(-0.4f, -8.1f, 1f), new Vector2(1.72f, 1.20f), new Color(0.75f, 0.95f, 1f, 0.72f), -15, OfficialLayer(5, whiteSquare));
        CreateSprite("Capa oficial de corales extension izquierda", new Vector3(-4.6f, -8.05f, 1f), new Vector2(-0.86f, 1.08f), new Color(0.75f, 0.95f, 1f, 0.44f), -16, OfficialLayer(5, whiteSquare));
        CreateSprite("Capa oficial corales izquierda visible", new Vector3(-3.4f, -8.55f, 1f), new Vector2(-0.92f, 1.02f), new Color(0.72f, 0.92f, 1f, 0.50f), -14, OfficialLayer(5, whiteSquare));
        CreateSprite("Capa oficial rocas de primer plano", new Vector3(-0.2f, -9.55f, 1f), new Vector2(1.74f, 1.22f), new Color(0.75f, 0.9f, 1f, 0.70f), -10, OfficialLayer(3, whiteSquare));
        CreateSprite("Capa oficial rocas primer plano extension izquierda", new Vector3(-4.9f, -9.5f, 1f), new Vector2(-0.86f, 1.08f), new Color(0.75f, 0.9f, 1f, 0.40f), -11, OfficialLayer(3, whiteSquare));
        CreateSprite("Capa oficial roca izquierda visible", new Vector3(-3.6f, -8.95f, 1f), new Vector2(-0.78f, 0.92f), new Color(0.70f, 0.86f, 1f, 0.52f), -8, OfficialLayer(3, whiteSquare));
        CreateSprite("Capa oficial rocas bajas", new Vector3(0.1f, -10.45f, 1f), new Vector2(1.70f, 1.16f), new Color(0.75f, 0.9f, 1f, 0.68f), -9, OfficialLayer(4, whiteSquare));
        CreateSprite("Capa oficial rocas bajas extension izquierda", new Vector3(-4.8f, -10.25f, 1f), new Vector2(-0.82f, 1.00f), new Color(0.75f, 0.9f, 1f, 0.38f), -12, OfficialLayer(4, whiteSquare));
        CreateSprite("Capa oficial roca baja izquierda visible", new Vector3(-3.5f, -10.05f, 1f), new Vector2(-0.76f, 0.90f), new Color(0.72f, 0.88f, 1f, 0.45f), -7, OfficialLayer(4, whiteSquare));

        CreateSprite("Corriente luminosa oficial", new Vector3(0f, -2.5f, 0.5f), new Vector2(1.06f, 0.88f), new Color(0.55f, 0.95f, 1f, 0.20f), -25, OfficialLayer(1, whiteCircle));
        CreateSprite("Neblina izquierda para profundidad", new Vector3(-3.5f, -4.8f, 0.6f), new Vector2(0.62f, 0.72f), new Color(0.32f, 0.76f, 1f, 0.10f), -24, OfficialLayer(1, whiteCircle));
        CreateSprite("Geiser energia oficial", new Vector3(0.5f, -4.4f, 0.4f), new Vector2(1.1f, 1.1f), new Color(0.75f, 0.95f, 1f, 0.58f), 1, OfficialLayer(6, whiteCircle));

        CreateGlow("Luz volcanica inferior", new Vector3(0f, -11.5f, 0f), new Vector2(9f, 2.5f), new Color(1f, 0.22f, 0.08f, 0.12f));
        CreateGlow("Luz azul de superficie", new Vector3(0f, 24f, 0f), new Vector2(11f, 4f), new Color(0.42f, 0.92f, 1f, 0.10f));

        for (int i = 0; i < 34; i++)
        {
            float x = -8.5f + (i % 9) * 2.1f;
            float y = -12f + (i / 9) * 8.5f + (i % 3) * 0.9f;
            GameObject bubble = CreateSprite("Burbuja ambiental", new Vector3(x, y, 0f), new Vector2(0.10f, 0.10f), new Color(0.7f, 0.95f, 1f, 0.22f), -20, whiteCircle);
            bubble.AddComponent<BubbleRise>();
        }
    }

    Sprite OfficialLayer(int index, Sprite fallback)
    {
        if (officialLevel2Layers == null) return fallback;
        if (index < 0 || index >= officialLevel2Layers.Length) return fallback;
        return officialLevel2Layers[index] != null ? officialLevel2Layers[index] : fallback;
    }

    void CreateVolcanicRoute()
    {
        Platform("Inicio del Nivel 2", 0f, -10.5f, 6.8f, 0.55f);
        Platform("Roca de carga de luz", -3.9f, -7.2f, 2.7f, 0.48f);
        Platform("Cornisa bajo geiser", 3.8f, -3.8f, 2.8f, 0.48f);
        Platform("Descanso intermedio", -3.7f, 1.2f, 2.8f, 0.48f);
        Platform("Plataforma sobre respiradero", 3.6f, 6.2f, 2.8f, 0.48f);
        Platform("Salida a Nivel 3", 0f, 12.2f, 6.4f, 0.55f);

        Wall("Pared izquierda para Wall Jump", -5.2f, -1.0f, 0.55f, 15.8f);
        Wall("Pared derecha para Wall Jump", 5.2f, 1.0f, 0.55f, 15.8f);
        Wall("Columna volcanica central", 0f, 3.4f, 0.70f, 4.2f);

        Geyser("Geiser activo bajo - corriente ascendente", 0f, -6.0f, 1.2f, 4.6f);
        Geyser("Geiser activo medio - corriente ascendente", -0.4f, -0.7f, 1.0f, 4.8f);
        Geyser("Geiser activo alto - corriente ascendente", 0.8f, 5.4f, 1.0f, 4.5f);

        Orb(-3.9f, -6.35f);
        Orb(3.8f, -2.95f);
        Orb(-4.6f, 2.3f);
        Orb(4.5f, 7.2f);
        Orb(0f, 13.0f);

        SpiderAnchor(-5.0f, -3.0f, false);
        SpiderAnchor(5.0f, 1.7f, true);
        SpiderAnchor(-5.0f, 6.0f, false);
    }

    void CreateGameplayObjects()
    {
        GameObject meterObject = new GameObject("Sistema de Luz de Elias");
        meterObject.transform.SetParent(root, false);
        Level2LightMeter lightMeter = meterObject.AddComponent<Level2LightMeter>();

        GameObject player = CreateSprite(
            "Nereo - Hermano Oscuridad (babosa)",
            new Vector3(0f, -9.75f, 0f),
            new Vector2(0.62f, 0.62f),
            Color.white,
            30,
            nereoSprite
        );
        player.layer = 2;

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 2.7f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CapsuleCollider2D body = player.AddComponent<CapsuleCollider2D>();
        body.size = new Vector2(1.2f, 0.58f);
        body.offset = new Vector2(0f, -0.05f);

        GameObject light = CreateSprite(
            "Elias - Forma Arana de Mar",
            new Vector3(-1.0f, -9.0f, 0f),
            new Vector2(0.38f, 0.38f),
            Color.white,
            32,
            spiderSprite
        );
        Level2SeaSpider spider = light.AddComponent<Level2SeaSpider>();
        spider.player = player.transform;

        Level2PlayerController controller = player.AddComponent<Level2PlayerController>();
        controller.spider = spider;
        controller.lightMeter = lightMeter;

        if (sceneCamera != null)
        {
            Level2CameraFollow follow = sceneCamera.gameObject.AddComponent<Level2CameraFollow>();
            follow.target = player.transform;
        }
    }

    void CreatePresentationLabels()
    {
        Label("NIVEL 2: PROFUNDIDADES VOLCANICAS", new Vector3(0f, -8.7f, 0f), new Color(0.85f, 0.95f, 1f));
        Label("Objetivo: combinar Shift (levitacion) + Z (Arana de Mar) + Wall Jump", new Vector3(0f, -7.9f, 0f), new Color(1f, 0.82f, 0.35f));
        Label("Paredes verticales: rebote sucesivo para subir sobre geiseres activos", new Vector3(0f, 10.9f, 0f), new Color(0.85f, 1f, 0.95f));
    }

    void Platform(string name, float x, float y, float width, float height)
    {
        GameObject platform = CreateSprite(name, new Vector3(x, y, 0f), new Vector2(width, height), new Color(0.10f, 0.18f, 0.22f), 4, whiteSquare);
        platform.AddComponent<BoxCollider2D>().size = Vector2.one;
        CreateSprite(name + " borde bioluminiscente", new Vector3(x, y - height * 0.33f, -0.1f), new Vector2(width * 0.92f, height * 0.16f), new Color(0.35f, 0.86f, 1f, 0.36f), 5, whiteSquare);
    }

    void Wall(string name, float x, float y, float width, float height)
    {
        GameObject wall = CreateSprite(name, new Vector3(x, y, 0f), new Vector2(width, height), new Color(0.08f, 0.17f, 0.21f), 4, whiteSquare);
        wall.AddComponent<BoxCollider2D>().size = Vector2.one;
        wall.AddComponent<WallJumpSurface>();
        CreateSprite(name + " borde azul", new Vector3(x, y, -0.1f), new Vector2(width * 0.22f, height * 0.96f), new Color(0.35f, 0.9f, 1f, 0.25f), 5, whiteSquare);
    }

    void Geyser(string name, float x, float y, float width, float height)
    {
        GameObject geyser = CreateSprite(name, new Vector3(x, y, -0.2f), new Vector2(width, height), new Color(1f, 0.36f, 0.08f, 0.38f), 10, whiteSquare);
        BoxCollider2D collider = geyser.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        geyser.AddComponent<GeyserHazard>();

        CreateSprite(name + " nucleo naranja", new Vector3(x, y, -0.25f), new Vector2(width * 0.55f, height * 0.95f), new Color(1f, 0.45f, 0.08f, 0.42f), 11, whiteSquare);
        CreateGlow(name + " vapor cyan", new Vector3(x, y + 0.2f, -0.3f), new Vector2(width * 1.8f, height * 1.05f), new Color(0.55f, 0.94f, 1f, 0.18f));

        for (int i = 0; i < 6; i++)
        {
            float offset = -height * 0.45f + i * height / 6f;
            GameObject bubble = CreateSprite(name + " burbuja", new Vector3(x + Random.Range(-0.35f, 0.35f), y + offset, 0f), new Vector2(0.15f, 0.15f), new Color(1f, 0.8f, 0.45f, 0.45f), 13, whiteCircle);
            bubble.AddComponent<BubbleRise>();
        }
    }

    void Orb(float x, float y)
    {
        GameObject orb = CreateSprite("Orbe de Luz", new Vector3(x, y, 0f), new Vector2(0.42f, 0.42f), new Color(1f, 0.85f, 0.18f), 24, whiteCircle);
        CircleCollider2D collider = orb.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        orb.AddComponent<Level2LightOrb>();
        CreateGlow("Halo de orbe", new Vector3(x, y, -0.1f), new Vector2(0.95f, 0.95f), new Color(1f, 0.84f, 0.18f, 0.16f));
    }

    void SpiderAnchor(float x, float y, bool flip)
    {
        GameObject anchor = CreateSprite("Arana de Mar adherida a pared", new Vector3(x, y, 0f), new Vector2(flip ? -0.32f : 0.32f, 0.32f), new Color(1f, 1f, 1f, 0.86f), 26, spiderSprite);
        anchor.AddComponent<SpiderAnchorPulse>();
        CreateGlow("Zona habilitada para Wall Jump", new Vector3(x, y, -0.1f), new Vector2(0.75f, 1.2f), new Color(0.9f, 1f, 0.55f, 0.16f));
    }

    void Label(string text, Vector3 position, Color color)
    {
        GameObject label = new GameObject(text);
        label.transform.SetParent(root, false);
        label.transform.position = position;

        TextMesh mesh = label.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.fontSize = 48;
        mesh.characterSize = 0.055f;
        mesh.color = color;

        MeshRenderer renderer = label.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 60;
    }

    GameObject CreateGlow(string name, Vector3 position, Vector2 scale, Color color)
    {
        return CreateSprite(name, position, scale, color, 2, whiteCircle);
    }

    GameObject CreateSprite(string name, Vector3 position, Vector2 scale, Color color, int sortingOrder, Sprite sprite)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(root, false);
        obj.transform.position = position;
        obj.transform.localScale = new Vector3(scale.x, scale.y, 1f);

        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return obj;
    }

    static Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(8, 8);
        Color[] pixels = new Color[64];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8);
    }

    static Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.47f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                pixels[y * size + x] = Vector2.Distance(new Vector2(x, y), center) <= radius ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}

public class Level2PlayerController : MonoBehaviour
{
    public Level2SeaSpider spider;
    public Level2LightMeter lightMeter;

    public float moveSpeed = 5.0f;
    public float jumpForce = 8.5f;
    public float levitationSpeed = 3.0f;
    public float wallJumpHorizontalForce = 11.5f;
    public float wallJumpVerticalForce = 11.2f;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    bool grounded;
    bool touchingWall;
    int wallDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Time.timeScale = 1f;
        ResolveReferences();
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        ResolveReferences();
        if (rb == null || spriteRenderer == null || spider == null || lightMeter == null) return;

        if (Time.timeScale == 0f && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        ReadContacts();
        HandleMovement();
        HandleAbilities();

        if (transform.position.y < -13.5f)
        {
            GameOver("Nereo cayo a la zona volcanica inferior.");
        }
    }

    void ReadContacts()
    {
        grounded = HitsSolid(Vector2.down, 0.50f, Vector3.down * 0.15f);
        bool right = HitsWall(Vector2.right, 1.65f);
        bool left = HitsWall(Vector2.left, 1.65f);

        touchingWall = right || left;
        wallDirection = right ? 1 : (left ? -1 : 0);
    }

    void HandleMovement()
    {
        float horizontal = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;

        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        if (horizontal > 0.05f) spriteRenderer.flipX = false;
        if (horizontal < -0.05f) spriteRenderer.flipX = true;
    }

    void HandleAbilities()
    {
        if (Input.GetKeyDown(KeyCode.Z) && lightMeter.UseLight(8f))
        {
            spider.ActivateSeaSpider();
        }

        bool wantsJump = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);

        if (wantsJump && spider.IsActive && touchingWall)
        {
            rb.linearVelocity = new Vector2(-wallDirection * wallJumpHorizontalForce, wallJumpVerticalForce);
            lightMeter.UseLight(3f);
            return;
        }

        if (wantsJump && grounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        bool levitating = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (levitating && lightMeter.UseLight(12f * Time.deltaTime))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, levitationSpeed);
        }
    }

    bool HitsWall(Vector2 direction, float distance)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction, distance);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i].collider;
            if (hit == null || hit.isTrigger) continue;
            if (hit.transform == transform) continue;
            if (hit.GetComponent<WallJumpSurface>() != null) return true;
        }

        return false;
    }

    bool HitsSolid(Vector2 direction, float distance, Vector3 offset)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position + offset, direction, distance);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i].collider;
            if (hit == null || hit.isTrigger) continue;
            if (hit.transform == transform) continue;
            return true;
        }

        return false;
    }

    public void GameOver(string reason)
    {
        Debug.Log("Game Over Nivel 2: " + reason);
        Time.timeScale = 0f;
    }

    public void BoostFromGeyser(float upwardForce, float sideForce)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;

        float verticalBoost = Mathf.Max(rb.linearVelocity.y, upwardForce);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalBoost);
    }

    void ResolveReferences()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spider == null) spider = FindObjectOfType<Level2SeaSpider>();
        if (lightMeter == null) lightMeter = FindObjectOfType<Level2LightMeter>();
    }
}

public class Level2SeaSpider : MonoBehaviour
{
    public Transform player;
    public float activeDuration = 12f;
    public bool IsActive { get; private set; }

    SpriteRenderer spriteRenderer;
    float timer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        if (player != null)
        {
            Vector3 target = player.position + new Vector3(-1.25f, 1.22f, 0f);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 5.5f);
        }

        if (!IsActive)
        {
            spriteRenderer.color = new Color(0.75f, 0.95f, 1f, 0.36f);
            return;
        }

        timer -= Time.deltaTime;
        float pulse = Mathf.PingPong(Time.time * 3f, 1f);
        spriteRenderer.color = Color.Lerp(new Color(1f, 0.82f, 0.18f, 1f), Color.white, pulse);

        if (timer <= 0f) IsActive = false;
    }

    public void ActivateSeaSpider()
    {
        IsActive = true;
        timer = activeDuration;
    }
}

public class Level2LightMeter : MonoBehaviour
{
    public float maxLight = 100f;
    public float currentLight = 82f;
    public float passiveDrain = 0.75f;

    void Update()
    {
        if (!Application.isPlaying) return;
        if (Time.timeScale == 0f) return;
        UseLight(passiveDrain * Time.deltaTime);
    }

    void OnGUI()
    {
        GUI.color = new Color(0.78f, 0.94f, 1f);
        GUI.Label(new Rect(20, 16, 760, 26), "Nivel 2 - Levitacion + Wall Jump con Arana de Mar");

        GUI.color = new Color(0.02f, 0.05f, 0.08f, 0.95f);
        GUI.DrawTexture(new Rect(20, 45, 280, 24), Texture2D.whiteTexture);

        GUI.color = new Color(1f, 0.82f, 0.18f);
        GUI.DrawTexture(new Rect(22, 47, 276f * Mathf.Clamp01(currentLight / maxLight), 20), Texture2D.whiteTexture);

        GUI.color = new Color(0.82f, 1f, 0.95f);
        GUI.Label(new Rect(20, Screen.height - 36, 980, 28), "A/D moverse | Espacio/W saltar | Shift levitar | Z activar Arana de Mar | Wall Jump tocando pared + W/Espacio | R reiniciar");

        if (Time.timeScale == 0f)
        {
            GUI.color = new Color(1f, 0.55f, 0.25f);
            GUI.Label(new Rect(Screen.width / 2 - 140, Screen.height / 2 - 20, 360, 40), "Pausa / Game Over - presiona R");
        }
    }

    public bool UseLight(float amount)
    {
        if (currentLight <= 0f) return false;
        currentLight = Mathf.Clamp(currentLight - Mathf.Max(0f, amount), 0f, maxLight);
        return currentLight > 0f;
    }

    public void AddLight(float amount)
    {
        currentLight = Mathf.Clamp(currentLight + amount, 0f, maxLight);
    }
}

public class Level2LightOrb : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Level2PlayerController player = other.GetComponent<Level2PlayerController>();
        if (player == null || player.lightMeter == null) return;

        player.lightMeter.AddLight(22f);
        Destroy(gameObject);
    }
}

public class GeyserHazard : MonoBehaviour
{
    public float upwardForce = 9.5f;
    public float sideForce = 1.8f;

    void OnTriggerEnter2D(Collider2D other)
    {
        Level2PlayerController player = other.GetComponent<Level2PlayerController>();
        if (player != null) player.BoostFromGeyser(upwardForce, sideForce);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Level2PlayerController player = other.GetComponent<Level2PlayerController>();
        if (player != null) player.BoostFromGeyser(upwardForce, sideForce);
    }
}

public class WallJumpSurface : MonoBehaviour
{
}

public class Level2CameraFollow : MonoBehaviour
{
    public Transform target;
    Vector3 velocity;

    void LateUpdate()
    {
        if (!Application.isPlaying) return;
        if (target == null) return;

        Vector3 desired = new Vector3(0f, Mathf.Clamp(target.position.y + 1.2f, -6.2f, 10.5f), -10f);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, 0.12f);
    }
}

public class BubbleRise : MonoBehaviour
{
    float speed;
    float startX;

    void Awake()
    {
        speed = Random.Range(0.18f, 0.46f);
        startX = transform.position.x;
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        transform.position += new Vector3(Mathf.Sin(Time.time * 1.8f + startX) * 0.08f, speed, 0f) * Time.deltaTime;

        if (transform.position.y > 15f)
        {
            transform.position = new Vector3(startX, -12.5f, transform.position.z);
        }
    }
}

public class SlowDrift : MonoBehaviour
{
    Vector3 start;

    void Awake()
    {
        start = transform.position;
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        transform.position = start + new Vector3(Mathf.Sin(Time.time * 0.08f) * 0.18f, 0f, 0f);
    }
}

public class SpiderAnchorPulse : MonoBehaviour
{
    Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        float pulse = 1f + Mathf.Sin(Time.time * 3.2f) * 0.045f;
        transform.localScale = baseScale * pulse;
    }
}
