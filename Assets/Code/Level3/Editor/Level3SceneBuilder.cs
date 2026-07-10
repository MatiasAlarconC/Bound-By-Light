#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level3SceneBuilder
{
    private const string SourceScene = "Assets/Scenes/MecanicaPulpo.unity";
    private const string Level3Scene = "Assets/Scenes/Nivel3.unity";
    private const string RootName = "LEVEL3_BLOCKOUT_VERTICAL";
    private const string MockupBackground = "Assets/Level3Reference/nivel3_escenario_mockup.png";
    private const string MantaSpritePath = "Assets/Characters/LightBrother/StingrayTransform/mantarraya_nivel3.png";
    private const string WhaleSpritePath = "Assets/Characters/LightBrother/StingrayTransform/ballena_nivel3.png";
    private const string TurtleSpritePath = "Assets/Characters/LightBrother/StingrayTransform/tortuga_nivel3.png";
    private const string PlatformSheet = "Assets/Platforms/Plataformas2.png";
    private const string DecorationSheet = "Assets/Platforms/Plataformas1.png";
    private const string ProfessorTilesFolder = "Assets/Level3ProfessorTiles/";
    private const string Level3SoundFolder = "Assets/Sounds/Level3/";
    private const float PresentationWidthMultiplier = 2.35f;
    private const float PresentationHeightMultiplier = 1.45f;
    private const float GameplayHorizontalSpread = 1.45f;
    private const float GameplayElementWidthBoost = 1.38f;
    private const float GameplayElementHeightBoost = 1.22f;
    private static Sprite[] platformSprites;
    private static Sprite[] decorationSprites;
    private static Dictionary<string, Sprite> professorSprites = new Dictionary<string, Sprite>();

    [MenuItem("Bound By Light/Level 3/Crear o reconstruir blockout")]
    public static void CreateOrRebuildLevel3()
    {
        EnsureLevel3SceneExists();
        EditorSceneManager.OpenScene(Level3Scene, OpenSceneMode.Single);

        DeleteExistingRoot();
        HideDuplicatedLevel1ArtAndColliders();
        DisableCopiedRoomCameraSystem();
        DisableCopiedHudCanvas();
        DisableCopiedSounds();
        LoadArtSprites();
        GameObject root = new GameObject(RootName);
        CreateProfessorPdfScenario(root.transform);

        Transform babosa = FindByName("Babosa");
        Transform hermanoLuz = FindByName("Pulpo ") ?? FindByName("Pulpo");

        if (hermanoLuz != null) hermanoLuz.position = new Vector3(-6.5f, -17.5f, 0f);
        MakeCharactersReadable(babosa, hermanoLuz);
        ConfigureLevel3BabosaPhysics(babosa);

        GameObject inicio = CreatePlatform(root.transform, "Inicio", new Vector2(-6.5f, -19f), new Vector2(7f, 0.8f));
        PlaceCharacterOnPlatform(babosa, inicio, new Vector2(-1.5f, 0f), 0.04f);
        ConfigureLevel3Camera(babosa);

        CreateLevel3GameplayRoute(root.transform);
        CreateMovingHazards(root.transform);

        CreateDeadZone(root.transform, "Salmuera_Sombra", new Vector2(0f, -23.5f), new Vector2(24f, 3f));

        Transform manta = CreateCreature(root.transform, "Mantarraya_Plataforma", new Vector2(-1.5f, -14.5f), new Vector2(5.2f, 1.25f), new Color(0.35f, 0.85f, 1f, 0.85f));
        AddCreatureSpriteVisual(manta, "Mantarraya_Visual_Real", MantaSpritePath, new Vector2(7.2f, 3f), 18);
        Level3RideableSurface mantaRide = manta.gameObject.AddComponent<Level3RideableSurface>();
        mantaRide.ConfigureMountOffset(new Vector2(0f, 1.35f), true, true, true, true);
        Level3MantarrayaAscensor mantaLogic = root.AddComponent<Level3MantarrayaAscensor>();
        mantaLogic.babosa = babosa;
        mantaLogic.hermanoLuzBase = hermanoLuz != null ? hermanoLuz.gameObject : null;
        mantaLogic.mantarraya = manta;
        mantaLogic.rideableSurface = mantaRide;
        mantaLogic.radioActivacion = 8f;
        mantaLogic.radioMontajeAutomatico = 8f;
        mantaLogic.velocidadAscenso = 2.7f;
        mantaLogic.bloquearBabosaEnMantarraya = true;
        mantaLogic.puntoTransformacion = CreatePoint(root.transform, "Punto_Transformacion_Manta", new Vector2(-6.4f, -17.2f));
        mantaLogic.puntosAscenso = new[]
        {
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_01", new Vector2(-4.8f, -15.95f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_02", new Vector2(5.0f, -14.45f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_03", new Vector2(4.9f, -13.1f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_04", new Vector2(-5.7f, -11.15f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_05", new Vector2(-5.6f, -9.5f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_06", new Vector2(4.6f, -7.75f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_07", new Vector2(5.8f, -5.9f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_08", new Vector2(-4.8f, -4.05f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_09", new Vector2(-4.9f, -2.15f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_10", new Vector2(5.6f, -0.25f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_11", new Vector2(-5.1f, 1.65f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_12", new Vector2(-5.2f, 3.65f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_13", new Vector2(3.9f, 5.75f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_14", new Vector2(4.0f, 7.95f)),
            CreatePoint(root.transform, "Manta_ZigZag_Bloque_15", new Vector2(4.4f, 10.2f)),
            CreatePoint(root.transform, "Manta_ZigZag_Encuentro_Ballena", new Vector2(4.4f, 11.4f)),
        };

        Transform whale = CreateCreature(root.transform, "Ballena_Ascenso", new Vector2(4f, 12.5f), new Vector2(9.8f, 2.45f), new Color(0.3f, 0.45f, 0.65f, 0.95f));
        CreateLocalDecoration(whale, "Ballena_Brillo_Dinamico", Vector2.zero, new Vector2(13.8f, 5.8f), new Color(0.35f, 0.9f, 1f, 0.18f), 37, null);
        AddCreatureSpriteVisual(whale, "Ballena_Visual_Real", WhaleSpritePath, new Vector2(13.2f, 5.25f), 45);
        Level3RideableSurface whaleRide = whale.gameObject.AddComponent<Level3RideableSurface>();
        whaleRide.ConfigureMountOffset(new Vector2(-0.65f, 1.95f), true, true);
        Level3BallenaAscenso whaleLogic = whale.gameObject.AddComponent<Level3BallenaAscenso>();
        whaleLogic.ballena = whale;
        whaleLogic.babosa = babosa;
        whaleLogic.mantarraya = manta;
        whaleLogic.whaleRideableSurface = whaleRide;
        whaleLogic.controladorMantarraya = mantaLogic;
        whaleLogic.mantarrayaOffsetSobreBallena = new Vector2(1.45f, 1.45f);
        whaleLogic.babosaOffsetSobreBallena = new Vector2(-0.65f, 1.95f);
        whaleLogic.activarPorProximidad = true;
        whaleLogic.radioActivacionPorProximidad = 8.5f;
        whaleLogic.activarPorAlturaDeEncuentro = true;
        whaleLogic.alturaEncuentroBallena = 11.0f;
        whaleLogic.mantenerPasajerosMontados = true;
        whaleLogic.movimientoIdleAntesDeActivar = true;
        whaleLogic.amplitudIdle = 0.55f;
        whaleLogic.amplitudHorizontalIdle = 0.35f;
        whaleLogic.velocidadIdle = 2.2f;
        whaleLogic.velocidad = 4.2f;
        Transform[] whaleRoute = new[]
        {
            CreatePoint(root.transform, "Ballena_Ruta_Vortice", new Vector2(2f, 19f)),
            CreatePoint(root.transform, "Ballena_Ruta_PostVortice", new Vector2(1f, 25f)),
            CreatePoint(root.transform, "Ballena_Ruta_Luz_Lejana", new Vector2(2.8f, 34f)),
            CreatePoint(root.transform, "Ballena_Ruta_Ascenso_Profundo", new Vector2(-1.4f, 42f)),
            CreatePoint(root.transform, "Ballena_Ruta_PostVortice_Alto", new Vector2(3.6f, 50.5f)),
            CreatePoint(root.transform, "Ballena_Ruta_Encuentro_Tortuga", new Vector2(0.8f, 58.5f)),
            CreatePoint(root.transform, "Ballena_Ruta_Subida_Tortuga_01", new Vector2(-2.4f, 64.2f)),
            CreatePoint(root.transform, "Ballena_Ruta_Subida_Tortuga_02", new Vector2(2.6f, 69.3f)),
            CreatePoint(root.transform, "Ballena_Ruta_Junto_Tortuga", new Vector2(4.4f, 72.6f)),
            CreatePoint(root.transform, "Ballena_Ruta_Luz_Final_Superficie", new Vector2(4.8f, 76.5f)),
        };
        whaleLogic.puntosRuta = whaleRoute;
        whaleLogic.indicePuntoDespuesDelVortice = 2;
        mantaLogic.ballenaSiguiente = whaleLogic;

        Level3RuntimeDirector director = root.AddComponent<Level3RuntimeDirector>();
        director.mainCamera = Camera.main;
        director.babosa = babosa;
        director.mantarraya = manta;
        director.ballena = whale;
        director.rutaBallena = whaleRoute;
        director.ballenaAscensoOriginal = whaleLogic;
        director.mantaAscensorOriginal = mantaLogic;
        director.cameraOffset = new Vector3(2.35f, 2.15f, -10f);
        director.cameraFixedX = -3.2f;
        director.cameraMinY = -14.25f;
        director.cameraMaxY = 79f;
        director.alturaActivacionBallena = 11.0f;
        director.velocidadBallena = 4.8f;
        director.seguirSubiendoHastaSuperficie = true;
        director.puntoFinalSuperficie = new Vector2(4.8f, 76.5f);
        director.amplitudIdleVertical = 1.25f;
        director.amplitudIdleHorizontal = 0.85f;
        director.velocidadIdle = 3.1f;
        director.babosaOffsetSobreBallena = new Vector2(-0.65f, 1.95f);
        director.mantarrayaOffsetSobreBallena = new Vector2(1.45f, 1.45f);

        Transform turtle = CreateCreature(root.transform, "Tortuga_Superficie", new Vector2(4.4f, 72.2f), new Vector2(4.8f, 1.95f), new Color(0.2f, 0.9f, 0.35f, 0.95f));
        AddCreatureSpriteVisual(turtle, "Tortuga_Visual_Real_Superficie", TurtleSpritePath, new Vector2(6.4f, 3.15f), 48);
        turtle.gameObject.SetActive(true);
        Level3RideableSurface turtleRide = turtle.gameObject.AddComponent<Level3RideableSurface>();
        turtleRide.ConfigureMountOffset(new Vector2(0f, 0.95f), true, true);
        Level3TransformacionTortuga turtleLogic = root.AddComponent<Level3TransformacionTortuga>();
        turtleLogic.babosa = babosa;
        turtleLogic.mantarraya = manta.gameObject;
        turtleLogic.tortuga = turtle.gameObject;
        turtleLogic.turtleRideableSurface = turtleRide;
        turtleLogic.activarAlEntrar = false;
        turtleLogic.ocultarTortugaAlInicio = false;
        turtleLogic.puntosSalida = new[]
        {
            CreatePoint(root.transform, "Tortuga_Ruta_Luz_01", new Vector2(-2.7f, 68.9f)),
            CreatePoint(root.transform, "Tortuga_Ruta_Luz_02", new Vector2(4.8f, 74.2f)),
        };
        whaleLogic.tortugaAlFinal = turtleLogic;

        CreateLevel3Audio(root.transform, babosa, manta, whale);

        CreateTrigger(root.transform, "Trigger_Vortice_Revela_Luz", new Vector2(1f, 25f), new Vector2(5f, 4f));

        CreateMarker(root.transform, "Vortice_PostBallena", new Vector2(1f, 21f), new Vector2(4f, 4f), new Color(0.25f, 0.9f, 0.95f, 0.35f));
        CreateMarker(root.transform, "Luz_Superficie_Revelada", new Vector2(7.5f, 32.5f), new Vector2(3.2f, 3.2f), new Color(1f, 0.85f, 0.25f, 0.55f));
        CreateMarker(root.transform, "Luz_Final_Superficie", new Vector2(4.8f, 76.5f), new Vector2(6.4f, 6.4f), new Color(1f, 0.9f, 0.3f, 0.65f));

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log("Nivel3.unity creado/reconstruido. Revisa LEVEL3_BLOCKOUT_VERTICAL y asigna sprites finales cuando esten listos.");
    }

    private static void EnsureLevel3SceneExists()
    {
        if (File.Exists(Level3Scene)) return;

        if (!AssetDatabase.CopyAsset(SourceScene, Level3Scene))
        {
            throw new IOException("No se pudo duplicar la escena base: " + SourceScene);
        }

        AssetDatabase.Refresh();
    }

    private static void DeleteExistingRoot()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null) Object.DestroyImmediate(existing);
    }

    private static void DisableCopiedHudCanvas()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null) canvas.SetActive(false);
    }

    private static void DisableCopiedSounds()
    {
        AudioSource[] audioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in audioSources)
        {
            if (source == null) continue;
            if (source.gameObject.name == "Sounds" || source.gameObject.scene == SceneManager.GetActiveScene())
            {
                source.Stop();
                source.playOnAwake = false;
            }
        }

        GameObject copiedSounds = GameObject.Find("Sounds");
        if (copiedSounds != null) copiedSounds.SetActive(false);
    }

    private static void HideDuplicatedLevel1ArtAndColliders()
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            HideDuplicatedLevel1Recursive(root.transform);
        }
    }

    private static void HideDuplicatedLevel1Recursive(Transform item)
    {
        if (item == null || item.name == RootName) return;

        bool protectedObject = IsProtectedBaseObject(item.gameObject);
        if (!protectedObject)
        {
            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;

            Collider2D collider = item.GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;
        }

        for (int i = 0; i < item.childCount; i++)
        {
            HideDuplicatedLevel1Recursive(item.GetChild(i));
        }
    }

    private static bool IsProtectedBaseObject(GameObject go)
    {
        string name = go.name;
        if (name.Contains("Babosa") || name.Contains("Pulpo") || name.Contains("Hermano")) return true;
        if (name.Contains("Camera") || name.Contains("Cinemachine") || name.Contains("GameManager")) return true;
        if (name.Contains("EventSystem") || name.Contains("Canvas") || name.Contains("Audio")) return true;
        if (name.Contains("Respawn") || name.Contains("Spawn")) return true;
        if (go.GetComponent<Camera>() != null) return true;
        if (go.GetComponent("BabosaControl") != null) return true;
        if (go.GetComponent("PulpoColumpio") != null) return true;
        if (go.GetComponent("GameManager") != null) return true;
        return false;
    }

    private static void MakeCharactersReadable(Transform babosa, Transform hermanoLuz)
    {
        if (babosa != null)
        {
            babosa.localScale = new Vector3(0.62f, 0.78f, 1f);
            SpriteRenderer renderer = babosa.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.sortingOrder = 25;
        }

        if (hermanoLuz != null)
        {
            hermanoLuz.localScale = new Vector3(0.86f, 0.86f, 1f);
            SpriteRenderer renderer = hermanoLuz.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.sortingOrder = 24;
        }
    }

    private static void ConfigureLevel3BabosaPhysics(Transform babosa)
    {
        if (babosa == null) return;

        Rigidbody2D rb = babosa.GetComponent<Rigidbody2D>();
        if (rb == null) rb = babosa.gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2.4f;
        rb.mass = 0.5f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        Collider2D collider = babosa.GetComponent<Collider2D>();
        if (collider == null)
        {
            BoxCollider2D box = babosa.gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1.1f, 0.62f);
            box.offset = new Vector2(0f, -0.05f);
        }
        else
        {
            collider.isTrigger = false;
        }

        Level3BabosaPlatformerAssist assist = babosa.GetComponent<Level3BabosaPlatformerAssist>();
        if (assist == null) assist = babosa.gameObject.AddComponent<Level3BabosaPlatformerAssist>();
        assist.fuerzaSalto = 8.8f;
        assist.impulsoEscalada = 0f;
        assist.distanciaSuelo = 0.82f;
        assist.distanciaBloqueLateral = 0.65f;
        assist.permitirEscaladaLateral = false;
    }

    private static void ConfigureLevel3Camera(Transform target)
    {
        if (target == null) return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = GameObject.Find("Main Camera");
            if (cameraObject != null) mainCamera = cameraObject.GetComponent<Camera>();
        }
        if (mainCamera == null) return;

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5.0f;
        mainCamera.aspect = 16f / 9f;
        mainCamera.transform.position = new Vector3(0f, -14.25f, -10f);

        MonoBehaviour[] behaviours = mainCamera.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour != null && behaviour.GetType().Name.Contains("CinemachineBrain"))
            {
                behaviour.enabled = false;
            }
        }

        Level3CameraVerticalGuide guide = mainCamera.GetComponent<Level3CameraVerticalGuide>();
        if (guide == null) guide = mainCamera.gameObject.AddComponent<Level3CameraVerticalGuide>();
        guide.target = target;
        guide.offset = new Vector3(0f, 1.85f, -10f);
        guide.minY = -14.25f;
        guide.maxY = 79f;
        guide.soloSube = true;
        guide.suavizado = 7.5f;
        guide.activarSeguimientoEnY = -14.4f;
        guide.esperarPuntoDeActivacion = false;
        guide.seguirX = false;
        guide.bordeSuperiorActivacion = 0.56f;
        guide.bordeSuperiorCritico = 0.72f;
        guide.suavizadoRapidoSubida = 14f;
        guide.velocidadMaximaSubida = 30f;
        guide.seguimientoInstantaneoVertical = true;
    }

    private static void DisableCopiedRoomCameraSystem()
    {
        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null) continue;

            string typeName = behaviour.GetType().Name;
            string fullName = behaviour.GetType().FullName;
            bool isRoom = typeName == "Room";
            bool isVirtualCamera = typeName.Contains("CinemachineCamera") || (fullName != null && fullName.Contains("Cinemachine"));

            if (isRoom)
            {
                SerializedObject serializedRoom = new SerializedObject(behaviour);
                SerializedProperty virtualCameraProperty = serializedRoom.FindProperty("virtualCamera");
                if (virtualCameraProperty != null && virtualCameraProperty.objectReferenceValue is GameObject virtualCamera)
                {
                    virtualCamera.SetActive(false);
                }

                behaviour.enabled = false;
                Collider2D roomCollider = behaviour.GetComponent<Collider2D>();
                if (roomCollider != null) roomCollider.enabled = false;
            }
            else if (isVirtualCamera)
            {
                behaviour.enabled = false;
            }
        }
    }

    private static Transform FindByName(string name)
    {
        GameObject go = GameObject.Find(name);
        return go != null ? go.transform : null;
    }

    private static GameObject CreatePlatform(Transform parent, string name, Vector2 position, Vector2 size)
    {
        size = PresentationSize(size);
        size = new Vector2(size.x * GameplayElementWidthBoost, size.y * GameplayElementHeightBoost);
        position = name == "Inicio" ? position : GameplayPosition(position);

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(position.x, position.y, 0f);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = PickPlatformSprite(name);
        renderer.color = new Color(0.82f, 0.94f, 1f, 1f);
        renderer.sortingOrder = 7;

        Vector2 visualSize = new Vector2(size.x, Mathf.Max(0.92f, size.y * 1.65f));
        FitRendererToWorldSize(go.transform, renderer, visualSize);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(size.x / go.transform.localScale.x, size.y / go.transform.localScale.y);
        col.offset = new Vector2(0f, (visualSize.y - size.y) * 0.5f / go.transform.localScale.y);

        AddPlatformDecor(go.transform, name, visualSize);
        return go;
    }

    private static void CreateLevel3GameplayRoute(Transform parent)
    {
        CreatePlatform(parent, "Transformacion_Mantarraya", new Vector2(-3.8f, -16.1f), new Vector2(6.1f, 0.62f));
        CreatePlatform(parent, "Salto_Ola_01", new Vector2(4.9f, -13.3f), new Vector2(4.9f, 0.56f));
        CreatePlatform(parent, "Salto_Ola_02", new Vector2(-5.6f, -10.2f), new Vector2(5.1f, 0.56f));
        CreatePlatform(parent, "Salto_Ola_03", new Vector2(4.5f, -7.1f), new Vector2(4.8f, 0.56f));
        CreatePlatform(parent, "Descanso_Mantarraya_01", new Vector2(-4.9f, -3.8f), new Vector2(5.9f, 0.6f));
        CreatePlatform(parent, "Salto_Ola_04", new Vector2(5.7f, -0.6f), new Vector2(4.8f, 0.56f));
        CreatePlatform(parent, "Checkpoint_Ascenso", new Vector2(-5.2f, 2.8f), new Vector2(5.8f, 0.6f));
        CreatePlatform(parent, "Salto_Ola_05", new Vector2(3.8f, 6.5f), new Vector2(5.0f, 0.56f));
        CreatePlatform(parent, "Encuentro_Ballena", new Vector2(4.4f, 11.0f), new Vector2(6.8f, 0.7f));
        CreatePlatform(parent, "Descanso_PostBallena_01", new Vector2(-5.3f, 18.5f), new Vector2(5.6f, 0.58f));
        CreatePlatform(parent, "Post_Vortice_Superficie", new Vector2(3.5f, 25f), new Vector2(6.8f, 0.65f));
        CreatePlatform(parent, "Ruta_Tortuga_Superior", new Vector2(6.4f, 31f), new Vector2(5.4f, 0.58f));
        CreatePlatform(parent, "Ascenso_Luz_01", new Vector2(-5.2f, 40f), new Vector2(5.5f, 0.58f));
        CreatePlatform(parent, "Camino_Superficie_01", new Vector2(4.7f, 51f), new Vector2(5.5f, 0.58f));
        CreatePlatform(parent, "Salida_Final_Luz", new Vector2(4.8f, 64f), new Vector2(6.6f, 0.68f));
        CreatePlatform(parent, "Encuentro_Tortuga_CasiSuperficie", new Vector2(-4.6f, 68.5f), new Vector2(5.7f, 0.62f));
        CreatePlatform(parent, "Ultimo_Impulso_Superficie", new Vector2(4.5f, 73.5f), new Vector2(6.3f, 0.66f));

        CreateProfessorSpriteObject(parent, "Pinchos_PDF_Advertencia", "strip_dark_spikes", new Vector2(0f, -21.1f), new Vector2(7.2f, 1.1f), 8, new Color(0.78f, 0.95f, 1f, 0.86f));
    }

    private static void CreateMovingHazards(Transform parent)
    {
        CreateMovingHazard(parent, "Obstaculo_Movil_Profundidad_01", new Vector2(0.0f, -12.25f), new Vector2(3.6f, 0.46f), 3.2f, 1.25f, 0.1f);
        CreateMovingHazard(parent, "Obstaculo_Movil_Profundidad_02", new Vector2(-1.4f, -8.35f), new Vector2(3.2f, 0.46f), 3.8f, 1.45f, 1.7f);
        CreateMovingHazard(parent, "Obstaculo_Movil_Central_01", new Vector2(0.8f, -1.95f), new Vector2(3.6f, 0.46f), 4.1f, 1.35f, 2.6f);
        CreateMovingHazard(parent, "Obstaculo_Movil_Central_02", new Vector2(-1.2f, 4.65f), new Vector2(3.4f, 0.46f), 4.4f, 1.55f, 0.9f);
        CreateMovingHazard(parent, "Obstaculo_Movil_Ballena_01", new Vector2(0.5f, 17.1f), new Vector2(4.2f, 0.5f), 4.0f, 1.2f, 2.1f);
        CreateMovingHazard(parent, "Obstaculo_Movil_Vortice_01", new Vector2(-0.8f, 23.0f), new Vector2(3.7f, 0.48f), 4.5f, 1.45f, 0.4f);
        CreateMovingHazard(parent, "Obstaculo_Movil_Superficie_01", new Vector2(1.1f, 39.0f), new Vector2(4.0f, 0.48f), 4.2f, 1.3f, 2.8f);
        CreateMovingHazard(parent, "Obstaculo_Movil_Final_01", new Vector2(-0.4f, 57.0f), new Vector2(4.5f, 0.5f), 4.7f, 1.15f, 1.2f);
    }

    private static void CreateMovingHazard(Transform parent, string name, Vector2 position, Vector2 size, float distance, float speed, float phase)
    {
        GameObject hazard = CreatePlatform(parent, name, position, size);

        SpriteRenderer renderer = hazard.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = new Color(1f, 0.42f, 0.78f, 0.96f);
            renderer.sortingOrder = 13;
        }

        BoxCollider2D collider = hazard.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        Rigidbody2D rb = hazard.GetComponent<Rigidbody2D>();
        if (rb == null) rb = hazard.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        Level3MovingHazard movingHazard = hazard.AddComponent<Level3MovingHazard>();
        movingHazard.distanciaHorizontal = distance;
        movingHazard.velocidad = speed;
        movingHazard.fase = phase;
    }

    private static void LoadArtSprites()
    {
        EnsureSpriteImporter(MantaSpritePath, SpriteImportMode.Single);
        EnsureSpriteImporter(WhaleSpritePath, SpriteImportMode.Single);
        EnsureSpriteImporter(TurtleSpritePath, SpriteImportMode.Single);
        platformSprites = LoadSpritesAtPath(PlatformSheet);
        decorationSprites = LoadSpritesAtPath(DecorationSheet);
        LoadProfessorSprites();
    }

    private static void LoadProfessorSprites()
    {
        professorSprites.Clear();

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ProfessorTilesFolder.TrimEnd('/') });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnsureSpriteImporter(path, SpriteImportMode.Single);
        }

        AssetDatabase.Refresh();

        guids = AssetDatabase.FindAssets("t:Sprite", new[] { ProfessorTilesFolder.TrimEnd('/') });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) continue;

            string key = Path.GetFileNameWithoutExtension(path);
            professorSprites[key] = sprite;
        }
    }

    private static void EnsureSpriteImporter(string path, SpriteImportMode importMode)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == importMode) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = importMode;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

    private static Sprite[] LoadSpritesAtPath(string path)
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        System.Collections.Generic.List<Sprite> sprites = new System.Collections.Generic.List<Sprite>();
        foreach (UnityEngine.Object asset in assets)
        {
            Sprite sprite = asset as Sprite;
            if (sprite != null) sprites.Add(sprite);
        }
        return sprites.ToArray();
    }

    private static Sprite PickPlatformSprite(string name)
    {
        string[] professorPlatformKeys =
        {
            "wave_platform_bridge_long",
            "wave_platform_long_01",
            "wave_platform_large_02",
            "wave_platform_large_01",
            "wave_platform_mid_01",
            "wave_platform_disk_01",
            "wave_platform_disk_02",
            "wave_platform_cyan_round",
            "wave_platform_dark_round"
        };

        if (professorSprites != null && professorSprites.Count > 0)
        {
            if (name.Contains("Inicio") || name.Contains("Salida") || name.Contains("Final"))
            {
                return ProfessorSprite("wave_platform_bridge_long") ?? ProfessorSprite("wave_platform_long_01");
            }

            if (name.Contains("Checkpoint") || name.Contains("Encuentro") || name.Contains("Post_Vortice"))
            {
                return ProfessorSprite("wave_platform_large_02") ?? ProfessorSprite("wave_platform_large_01");
            }

            if (name.Contains("Bloque_Salto"))
            {
                return ProfessorSprite(Mathf.Abs(name.GetHashCode()) % 3 == 0 ? "wave_platform_mid_01" : "wave_platform_large_01");
            }

            int professorIndex = Mathf.Abs(name.GetHashCode()) % professorPlatformKeys.Length;
            Sprite professorSprite = ProfessorSprite(professorPlatformKeys[professorIndex]);
            if (professorSprite != null) return professorSprite;
        }

        if (platformSprites == null || platformSprites.Length == 0)
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        int index = Mathf.Abs(name.GetHashCode()) % platformSprites.Length;
        return platformSprites[index];
    }

    private static Sprite PickDecorationSprite(string name, int salt)
    {
        string[] professorDecorationKeys =
        {
            "decor_tube_purple",
            "decor_tube_blue",
            "decor_tube_green_01",
            "decor_tube_green_02",
            "decor_bubbles",
            "decor_ring",
            "decor_green_special_01",
            "decor_particles"
        };

        if (professorSprites != null && professorSprites.Count > 0)
        {
            int professorDecorationIndex = Mathf.Abs((name + salt).GetHashCode()) % professorDecorationKeys.Length;
            Sprite professorSprite = ProfessorSprite(professorDecorationKeys[professorDecorationIndex]);
            if (professorSprite != null) return professorSprite;
        }

        if (decorationSprites == null || decorationSprites.Length == 0) return null;
        int fallbackDecorationIndex = Mathf.Abs((name + salt).GetHashCode()) % decorationSprites.Length;
        return decorationSprites[fallbackDecorationIndex];
    }

    private static Sprite ProfessorSprite(string key)
    {
        if (professorSprites == null) return null;
        return professorSprites.TryGetValue(key, out Sprite sprite) ? sprite : null;
    }

    private static void FitRendererToWorldSize(Transform target, SpriteRenderer renderer, Vector2 worldSize)
    {
        if (renderer.sprite == null)
        {
            target.localScale = new Vector3(worldSize.x, worldSize.y, 1f);
            return;
        }

        Vector2 spriteSize = renderer.sprite.bounds.size;
        target.localScale = new Vector3(worldSize.x / spriteSize.x, worldSize.y / spriteSize.y, 1f);
    }

    private static void AddPlatformDecor(Transform platform, string platformName, Vector2 visualSize)
    {
        float halfWidth = visualSize.x * 0.5f;
        float top = visualSize.y * 0.5f;
        float bottom = -visualSize.y * 0.5f;

        CreateLocalDecoration(platform, platformName + "_Coral_Izq", new Vector2(-halfWidth * 0.32f, top + 0.12f), new Vector2(0.42f, 0.54f), new Color(0.5f, 0.32f, 0.95f, 0.82f), 10, PickDecorationSprite(platformName, 1));
        CreateLocalDecoration(platform, platformName + "_Cristal_Der", new Vector2(halfWidth * 0.36f, top + 0.13f), new Vector2(0.34f, 0.58f), new Color(0.35f, 0.92f, 1f, 0.82f), 10, PickDecorationSprite(platformName, 2));

        for (int i = 0; i < 2; i++)
        {
            float x = Mathf.Lerp(-halfWidth * 0.58f, halfWidth * 0.58f, i);
            float height = 0.45f + i * 0.18f;
            CreateLocalDecoration(platform, platformName + "_Alga_" + i, new Vector2(x, bottom - height * 0.38f), new Vector2(0.07f, height), new Color(0.12f, 0.65f, 0.5f, 0.48f), 6, null);
        }

        if (platformName.Contains("Checkpoint") || platformName.Contains("Descanso"))
        {
            CreateLocalDecoration(platform, platformName + "_BurbujaGuia", new Vector2(0f, top + 0.95f), new Vector2(0.42f, 0.42f), new Color(0.58f, 0.95f, 1f, 0.7f), 11, null);
        }
    }

    private static GameObject CreateLocalDecoration(Transform parent, string name, Vector2 localPosition, Vector2 worldSize, Color color, int sortingOrder, Sprite sprite)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = new Vector3(localPosition.x / parent.localScale.x, localPosition.y / parent.localScale.y, -0.03f);
        go.transform.localRotation = Quaternion.identity;

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite != null ? sprite : AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        Vector2 parentScale = new Vector2(parent.localScale.x, parent.localScale.y);
        FitRendererToWorldSize(go.transform, renderer, new Vector2(worldSize.x / parentScale.x, worldSize.y / parentScale.y));
        return go;
    }

    private static void AddCreatureSpriteVisual(Transform creature, string name, string spritePath, Vector2 worldSize, int sortingOrder)
    {
        if (creature == null) return;

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null) return;

        SpriteRenderer placeholder = creature.GetComponent<SpriteRenderer>();
        if (placeholder != null) placeholder.enabled = false;

        CreateLocalDecoration(creature, name, Vector2.zero, PresentationSize(worldSize), Color.white, sortingOrder, sprite);
    }

    private static void CreateProfessorPdfScenario(Transform parent)
    {
        CreateMarker(parent, "Fondo_Base_Azul_PDF", new Vector2(0f, 29f), new Vector2(31f, 116f), new Color(0.018f, 0.12f, 0.20f, 1f), -40);

        for (int i = 0; i < 11; i++)
        {
            float y = -20f + i * 10.3f;
            string panelKey = i % 4 == 0 ? "bg_current_01" : i % 4 == 1 ? "bg_depth_01" : i % 4 == 2 ? "bg_waves_01" : "bg_glow_01";
            CreateProfessorSpriteObject(parent, "Fondo_Panel_PDF_" + i, panelKey, new Vector2(0f, y), new Vector2(31f, 11.4f), -36, new Color(0.72f, 0.96f, 1f, 0.64f));
        }

        for (int i = 0; i < 6; i++)
        {
            float y = -18f + i * 18.1f;
            CreateProfessorSpriteObject(parent, "Borde_Ola_Izquierdo_PDF_" + i, "side_wave_strip_left", new Vector2(-13.2f, y), new Vector2(1.65f, 15.4f), -16, new Color(0.72f, 0.98f, 1f, 0.94f));
            CreateProfessorSpriteObject(parent, "Borde_Ola_Derecho_PDF_" + i, "side_wave_strip_right", new Vector2(13.2f, y), new Vector2(1.45f, 15.0f), -16, new Color(0.68f, 0.94f, 1f, 0.86f));
        }

        for (int i = 0; i < 7; i++)
        {
            float y = -14f + i * 13.5f;
            float x = i % 2 == 0 ? -12.0f : 12.0f;
            CreateProfessorSpriteObject(parent, "Panel_Kelp_Lateral_PDF_" + i, "bg_current_01", new Vector2(x, y), new Vector2(1.95f, 7.4f), -18, new Color(0.56f, 0.96f, 0.78f, 0.46f));
            CreateProfessorSpriteObject(parent, "Decor_Seaweed_PDF_" + i, "decor_seaweed_thin", new Vector2(x + (x < 0 ? 0.95f : -0.95f), y - 0.4f), new Vector2(0.95f, 3.35f), -9, new Color(0.78f, 1f, 0.82f, 0.76f));
        }

        CreateProfessorSpriteObject(parent, "Superficie_Panel_Turquesa_PDF", "panel_surface_turquoise", new Vector2(0f, 78.5f), new Vector2(23.8f, 5.2f), -12, new Color(0.95f, 1f, 1f, 0.92f));
        CreateProfessorSpriteObject(parent, "Techo_Estalactitas_PDF", "strip_dark_spikes", new Vector2(0f, 73.8f), new Vector2(20f, 1.65f), -7, new Color(0.8f, 0.95f, 1f, 0.9f));
        CreateProfessorSpriteObject(parent, "Fondo_Vortice_Panel_PDF", "panel_dark_waves", new Vector2(1f, 20.5f), new Vector2(12.5f, 6.2f), -14, new Color(0.7f, 1f, 1f, 0.68f));

        for (int i = 0; i < 9; i++)
        {
            CreateMarker(parent, "Orbe_Ruta_Luz_PDF_" + i, new Vector2(0.9f + Mathf.Sin(i * 1.3f) * 0.35f, -9f + i * 5.4f), new Vector2(0.28f, 0.28f), new Color(1f, 0.78f, 0.22f, 0.86f), 12);
        }

        Vector2[] bubbleColumns =
        {
            new Vector2(-6.4f, -9.0f),
            new Vector2(5.8f, -2.5f),
            new Vector2(-3.7f, 9.0f),
            new Vector2(6.4f, 25f),
            new Vector2(-4.6f, 45f)
        };

        for (int i = 0; i < bubbleColumns.Length; i++)
        {
            CreateProfessorSpriteObject(parent, "Burbujas_Guia_PDF_" + i, "decor_bubbles", bubbleColumns[i], new Vector2(0.85f, 3.0f), 3, new Color(0.82f, 0.98f, 1f, 0.72f));
        }

        Vector2[] decorPositions =
        {
            new Vector2(-8.5f, -20.2f),
            new Vector2(8.0f, -17.8f),
            new Vector2(-7.7f, -4.2f),
            new Vector2(8.0f, 5.8f),
            new Vector2(-8.2f, 19.6f),
            new Vector2(8.2f, 37.4f),
            new Vector2(-7.8f, 56.0f)
        };

        string[] decorKeys =
        {
            "decor_tube_purple",
            "decor_tube_green_01",
            "decor_tube_blue",
            "decor_tube_green_02",
            "decor_special_green_log",
            "decor_special_dark_oval",
            "decor_light_beam"
        };

        for (int i = 0; i < decorPositions.Length; i++)
        {
            CreateProfessorSpriteObject(parent, "Decoracion_PDF_" + i, decorKeys[i % decorKeys.Length], decorPositions[i], new Vector2(1.2f, 1.25f), 4, Color.white);
        }

        Vector2[] centralAccentPositions =
        {
            new Vector2(-3.6f, -13.2f),
            new Vector2(4.2f, -8.8f),
            new Vector2(-4.0f, -2.4f),
            new Vector2(4.4f, 5.4f),
            new Vector2(-4.8f, 15.8f),
            new Vector2(4.8f, 27.5f),
            new Vector2(-4.4f, 42.5f),
            new Vector2(4.2f, 58.0f)
        };

        string[] centralAccentKeys =
        {
            "decor_special_dark_oval",
            "decor_tube_blue",
            "decor_tube_green_02",
            "decor_tube_purple",
            "decor_special_green_log",
            "decor_bubbles",
            "decor_tube_green_01",
            "decor_light_beam"
        };

        for (int i = 0; i < centralAccentPositions.Length; i++)
        {
            CreateProfessorSpriteObject(parent, "Decoracion_Central_Grande_PDF_" + i, centralAccentKeys[i % centralAccentKeys.Length], centralAccentPositions[i], new Vector2(2.6f, 2.2f), 2, new Color(0.88f, 1f, 1f, 0.72f));
        }

        Vector2[] leftAccentPositions =
        {
            new Vector2(-12.0f, -18.5f),
            new Vector2(-11.7f, -7.0f),
            new Vector2(-12.2f, 5.0f),
            new Vector2(-11.6f, 18.0f),
            new Vector2(-12.0f, 32.0f),
            new Vector2(-11.7f, 50.0f)
        };

        string[] leftAccentKeys =
        {
            "decor_tube_green_01",
            "decor_tube_purple",
            "decor_special_dark_oval",
            "decor_tube_blue",
            "decor_tube_green_02",
            "decor_special_green_log"
        };

        for (int i = 0; i < leftAccentPositions.Length; i++)
        {
            CreateProfessorSpriteObject(parent, "Decoracion_Izquierda_Grande_PDF_" + i, leftAccentKeys[i % leftAccentKeys.Length], leftAccentPositions[i], new Vector2(1.7f, 1.8f), 5, new Color(0.95f, 1f, 1f, 0.92f));
        }

        CreateMarker(parent, "Luz_Final_Superficie_PDF", new Vector2(4.8f, 76.5f), new Vector2(6.4f, 6.4f), new Color(1f, 0.9f, 0.3f, 0.55f), -4);
        CreateMarker(parent, "Glow_Vortice_PDF", new Vector2(1f, 20.5f), new Vector2(9.5f, 9.5f), new Color(0.17f, 0.78f, 0.88f, 0.22f), -3);
    }

    private static GameObject CreateProfessorSpriteObject(Transform parent, string name, string spriteKey, Vector2 position, Vector2 size, int sortingOrder, Color color)
    {
        Sprite sprite = ProfessorSprite(spriteKey);
        if (sprite == null)
        {
            return CreateMarker(parent, name + "_Fallback", position, size, color, sortingOrder);
        }

        size = PresentationSize(size);

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(position.x, position.y, 1f);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        FitRendererToWorldSize(go.transform, renderer, size);
        return go;
    }

    private static void CreateCaveComposition(Transform parent)
    {
        CreateMarker(parent, "Fondo_Azul_Profundo", new Vector2(0f, 6f), new Vector2(26f, 58f), new Color(0.02f, 0.11f, 0.18f, 0.96f), -12);
        CreateMarker(parent, "Pared_Roca_Izquierda", new Vector2(-11.8f, 5.5f), new Vector2(3.2f, 57f), new Color(0.02f, 0.025f, 0.03f, 0.98f), -4);
        CreateMarker(parent, "Pared_Roca_Derecha", new Vector2(11.8f, 5.5f), new Vector2(3.2f, 57f), new Color(0.02f, 0.025f, 0.03f, 0.98f), -4);
        CreateMarker(parent, "Haz_De_Luz_Superficie", new Vector2(4.8f, 27.5f), new Vector2(4.2f, 16f), new Color(0.95f, 0.82f, 0.38f, 0.18f), -2);

        for (int i = 0; i < 9; i++)
        {
            float y = -18f + i * 5.5f;
            float side = i % 2 == 0 ? -9.6f : 9.6f;
            CreateMarker(parent, "Roca_Lateral_" + i, new Vector2(side, y), new Vector2(2.3f, 1.2f), new Color(0.18f, 0.29f, 0.38f, 0.78f), -1);
            CreateMarker(parent, "Cristales_Pared_" + i, new Vector2(side + (side < 0 ? 0.8f : -0.8f), y + 0.9f), new Vector2(0.5f, 0.8f), new Color(0.28f, 0.9f, 1f, 0.8f), 2);
        }

        for (int i = 0; i < 7; i++)
        {
            CreateMarker(parent, "Orbe_Ruta_Luz_" + i, new Vector2(1.2f + Mathf.Sin(i) * 0.45f, -11f + i * 5.4f), new Vector2(0.28f, 0.28f), new Color(1f, 0.78f, 0.2f, 0.88f), 12);
        }
    }

    private static bool CreateMockupScenario(Transform parent)
    {
        Sprite mockup = AssetDatabase.LoadAssetAtPath<Sprite>(MockupBackground);
        if (mockup == null) return false;

        CreateMockupPanel(parent, "Escenario_Mockup_Completo_Grande", mockup, new Vector2(0f, 29f), new Vector2(124f, 122f), -30);

        CreateMarker(parent, "Capa_Profundidad_Nivel3", new Vector2(0f, -19f), new Vector2(28f, 10f), new Color(0.0f, 0.08f, 0.06f, 0.32f), -21);
        CreateMarker(parent, "Guia_Luz_Superficie_Mockup", new Vector2(5.5f, 30f), new Vector2(9f, 5.5f), new Color(1f, 0.9f, 0.35f, 0.18f), -20);
        CreateMarker(parent, "Guia_Vortice_Mockup", new Vector2(0.5f, 17f), new Vector2(12f, 6f), new Color(0.35f, 0.95f, 0.95f, 0.16f), -19);
        CreateMarker(parent, "Capa_Superficie_Nivel3", new Vector2(4f, 72f), new Vector2(28f, 20f), new Color(1f, 0.88f, 0.38f, 0.14f), -18);
        CreateStaticImageCoverups(parent);
        return true;
    }

    private static void CreateStaticImageCoverups(Transform parent)
    {
        CreateMarker(parent, "Ocultador_Ballena_Estatica_Fondo", new Vector2(1.3f, 14.9f), new Vector2(18.5f, 8.5f), new Color(0.02f, 0.12f, 0.18f, 0.92f), -17);
        CreateMarker(parent, "Ocultador_Ballena_Estatica_Luz", new Vector2(1.1f, 16.8f), new Vector2(8f, 5.8f), new Color(0.22f, 0.68f, 0.78f, 0.18f), -16);
        CreateMarker(parent, "Ocultador_Ballena_Estatica_Sombra", new Vector2(5.6f, 12.1f), new Vector2(9.5f, 3.8f), new Color(0.01f, 0.08f, 0.12f, 0.58f), -15);
        CreateMarker(parent, "Ocultador_Babosa_Estatica_Fondo", new Vector2(-7.3f, -14.6f), new Vector2(5.9f, 2.8f), new Color(0.02f, 0.13f, 0.12f, 0.94f), -17);
        CreateMarker(parent, "Ocultador_Babosa_Estatica_Roca", new Vector2(-8.7f, -13.8f), new Vector2(3.5f, 1.3f), new Color(0.08f, 0.18f, 0.19f, 0.75f), -15);
        CreateMarker(parent, "Ocultador_Luz_Estatica_Babosa", new Vector2(-5.6f, -14.1f), new Vector2(2.4f, 2.2f), new Color(0.03f, 0.18f, 0.16f, 0.82f), -16);
    }

    private static void CreateMockupPanel(Transform parent, string name, Sprite mockup, Vector2 position, Vector2 size, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(position.x, position.y, 3f);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = mockup;
        renderer.color = Color.white;
        renderer.sortingOrder = sortingOrder;

        FitRendererToWorldSize(go.transform, renderer, size);
    }

    private static void PlaceCharacterOnPlatform(Transform character, GameObject platform, Vector2 localOffset, float extraGap)
    {
        if (character == null || platform == null) return;

        Renderer platformRenderer = platform.GetComponent<Renderer>();
        if (platformRenderer == null) return;

        float halfHeight = 0.5f;
        Collider2D characterCollider = character.GetComponent<Collider2D>();
        if (characterCollider != null)
        {
            halfHeight = characterCollider.bounds.extents.y;
        }
        else
        {
            Renderer characterRenderer = character.GetComponentInChildren<Renderer>();
            if (characterRenderer != null)
            {
                halfHeight = characterRenderer.bounds.extents.y;
            }
        }

        Vector3 position = platform.transform.position;
        position.x += localOffset.x;
        position.y = platformRenderer.bounds.max.y + halfHeight + extraGap + localOffset.y;
        position.z = character.position.z;
        character.position = position;

        Rigidbody2D rb = character.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private static void CreateDeadZone(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject go = CreateMarker(parent, name, position, size, new Color(0.1f, 0.45f, 0.25f, 0.55f));
        TrySetTag(go, "DeadZone");
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one;
    }

    private static void CreateLevel3Audio(Transform parent, Transform babosa, Transform manta, Transform whale)
    {
        AssetDatabase.Refresh();

        GameObject audioObject = new GameObject("Level3_Audio");
        audioObject.transform.SetParent(parent);
        audioObject.transform.position = Vector3.zero;

        Level3AudioDirector audioDirector = audioObject.AddComponent<Level3AudioDirector>();
        audioDirector.babosa = babosa;
        audioDirector.mantarraya = manta;
        audioDirector.ballena = whale;
        audioDirector.volumenAmbiente = 0.42f;
        audioDirector.volumenMovimiento = 0.3f;
        audioDirector.volumenSfx = 0.72f;
        audioDirector.alturaBallenaActiva = 12.5f;
        audioDirector.alturaVortice = 24f;
        audioDirector.alturaLuzFinal = 70f;

        audioDirector.ambienteProfundo = LoadAudioClips(
            Level3SoundFolder + "Nivel3_Ambiente_01.wav",
            Level3SoundFolder + "Nivel3_Ambiente_02.wav",
            Level3SoundFolder + "Nivel3_Ambiente_03.wav");
        audioDirector.ambienteAscenso = LoadAudioClips(
            Level3SoundFolder + "Nivel3_Ascenso_01.wav",
            Level3SoundFolder + "Nivel3_Ascenso_02.wav",
            Level3SoundFolder + "Nivel3_Ascenso_03.wav");
        audioDirector.transformacionMantarraya = LoadAudioClips(
            Level3SoundFolder + "Nivel3_Transformacion_01.wav",
            Level3SoundFolder + "Nivel3_Transformacion_02.wav");
        audioDirector.movimientoMantarraya = LoadAudioClips(
            Level3SoundFolder + "Nivel3_Mantarraya_01.wav",
            Level3SoundFolder + "Nivel3_Mantarraya_02.wav");
        audioDirector.entradaBallena = LoadAudioClips(
            Level3SoundFolder + "Nivel3_Ballena_01.wav",
            Level3SoundFolder + "Nivel3_Ballena_02.wav");
        audioDirector.vortice = LoadAudioClips(
            Level3SoundFolder + "Nivel3_Vortice_01.wav",
            Level3SoundFolder + "Nivel3_Vortice_02.wav");
        audioDirector.luzFinal = LoadAudioClips(
            Level3SoundFolder + "Nivel3_Luz_01.wav",
            Level3SoundFolder + "Nivel3_Luz_02.wav");
    }

    private static AudioClip[] LoadAudioClips(params string[] paths)
    {
        List<AudioClip> clips = new List<AudioClip>();
        foreach (string path in paths)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null)
            {
                clips.Add(clip);
            }
            else
            {
                Debug.LogWarning("Audio no encontrado para Nivel 3: " + path);
            }
        }

        return clips.ToArray();
    }

    private static GameObject CreateTrigger(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject go = CreateMarker(parent, name, position, size, new Color(0.95f, 0.75f, 0.25f, 0.35f));
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one;
        return go;
    }

    private static Transform CreateCreature(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        position = GameplayPosition(position);
        GameObject go = CreateMarker(parent, name, position, size, color);
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        return go.transform;
    }

    private static Transform CreatePoint(Transform parent, string name, Vector2 position)
    {
        position = GameplayPosition(position);
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(position.x, position.y, 0f);
        return go.transform;
    }

    private static GameObject CreateMarker(Transform parent, string name, Vector2 position, Vector2 size, Color color, int sortingOrder = 5)
    {
        size = PresentationSize(size);

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(position.x, position.y, 0f);
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        return go;
    }

    private static Vector2 PresentationSize(Vector2 size)
    {
        return new Vector2(size.x * PresentationWidthMultiplier, size.y * PresentationHeightMultiplier);
    }

    private static Vector2 GameplayPosition(Vector2 position)
    {
        return new Vector2(position.x * GameplayHorizontalSpread, position.y);
    }

    private static void TrySetTag(GameObject go, string tag)
    {
        try
        {
            go.tag = tag;
        }
        catch
        {
            Debug.LogWarning("No existe el tag '" + tag + "'. Asignalo manualmente si Unity lo requiere.");
        }
    }
}
#endif
