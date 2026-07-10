using UnityEngine;

/// <summary>
/// Runtime director for the Level 3 mockup. It keeps the camera and whale sequence
/// deterministic for presentation, independent from copied room/cinemachine logic.
/// </summary>
public class Level3RuntimeDirector : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Transform babosa;
    public Transform mantarraya;
    public Transform ballena;
    public Transform[] rutaBallena;
    public Level3BallenaAscenso ballenaAscensoOriginal;
    public Level3MantarrayaAscensor mantaAscensorOriginal;

    [Header("Camera")]
    public bool forzarCamara = true;
    public Vector3 cameraOffset = new Vector3(2.35f, 1.45f, -10f);
    public float cameraFixedX = -3.2f;
    public float cameraMinY = -14.25f;
    public float cameraMaxY = 79f;
    public bool camaraSoloSube = true;

    [Header("Whale Sequence")]
    public bool controlarBallena = true;
    public KeyCode teclaActivarBallena = KeyCode.B;
    public float alturaActivacionBallena = 6.8f;
    public float velocidadBallena = 4.2f;
    public float distanciaCambioPunto = 0.12f;
    public Vector2 babosaOffsetSobreBallena = new Vector2(-0.65f, 1.95f);
    public Vector2 mantarrayaOffsetSobreBallena = new Vector2(1.45f, 1.45f);
    public bool activarBallenaAlIniciar = false;
    public float retrasoActivacionAutomatica = 2.5f;
    public bool seguirSubiendoHastaSuperficie = true;
    public Vector2 puntoFinalSuperficie = new Vector2(4.8f, 76.5f);

    [Header("Whale Idle")]
    public bool ballenaIdleAntesDeActivar = true;
    public float amplitudIdleVertical = 0.75f;
    public float amplitudIdleHorizontal = 0.45f;
    public float velocidadIdle = 2.4f;
    public bool forzarIdleVisibleSiempre = true;

    private float cameraHighestY;
    private Vector3 ballenaBasePosition;
    private bool ballenaActivada;
    private int indiceRutaBallena;
    private float tiempoInicio;

    private void Awake()
    {
        tiempoInicio = Time.time;
        ResolveReferences();
        if (mainCamera != null)
        {
            mainCamera.enabled = true;
            mainCamera.depth = 100f;
            cameraHighestY = mainCamera.transform.position.y;
            DisableCameraControllers(mainCamera);
        }

        DisableOtherCameras();

        if (ballena != null)
        {
            ballenaBasePosition = ballena.position;
        }

        if (ballenaAscensoOriginal != null)
        {
            ballenaAscensoOriginal.enabled = false;
        }
    }

    private void Update()
    {
        ResolveReferences();
        UpdateWhale();
        ForceCameraFollow();
        DisableOtherCameras();
    }

    private void LateUpdate()
    {
        ForceCameraFollow();
    }

    private void OnPreCull()
    {
        ForceCameraFollow();
    }

    private void UpdateWhale()
    {
        if (!controlarBallena || ballena == null) return;

        if (!ballenaActivada)
        {
            if (ballenaIdleAntesDeActivar)
            {
                ballena.position = ballenaBasePosition + new Vector3(
                    Mathf.Cos(Time.time * velocidadIdle * 0.65f) * amplitudIdleHorizontal,
                    Mathf.Sin(Time.time * velocidadIdle) * amplitudIdleVertical,
                    0f
                );
            }

            bool autoStart = activarBallenaAlIniciar && Time.time - tiempoInicio >= retrasoActivacionAutomatica;
            if (Input.GetKeyDown(teclaActivarBallena) || autoStart || PassengerReachedWhaleHeight())
            {
                ActivateWhaleRide();
            }

            return;
        }

        if (rutaBallena == null || rutaBallena.Length == 0)
        {
            MoveWhaleToSurfaceFallback();
            return;
        }

        if (indiceRutaBallena >= rutaBallena.Length)
        {
            MoveWhaleToSurfaceFallback();
            return;
        }

        Transform destino = rutaBallena[indiceRutaBallena];
        ballena.position = Vector3.MoveTowards(ballena.position, destino.position, velocidadBallena * Time.deltaTime);
        MountPassengersOnWhale();

        if (Vector2.Distance(ballena.position, destino.position) <= distanciaCambioPunto)
        {
            indiceRutaBallena++;
        }
    }

    private void MoveWhaleToSurfaceFallback()
    {
        if (!seguirSubiendoHastaSuperficie || ballena == null) return;

        Vector3 destinoFinal = new Vector3(puntoFinalSuperficie.x, puntoFinalSuperficie.y, ballena.position.z);
        ballena.position = Vector3.MoveTowards(ballena.position, destinoFinal, velocidadBallena * Time.deltaTime);
        MountPassengersOnWhale();
    }

    private bool PassengerReachedWhaleHeight()
    {
        bool babosaArriba = babosa != null && babosa.position.y >= alturaActivacionBallena;
        bool mantarrayaArriba = mantarraya != null
            && mantarraya.gameObject.activeInHierarchy
            && mantarraya.position.y >= alturaActivacionBallena;

        return babosaArriba || mantarrayaArriba;
    }

    private void ActivateWhaleRide()
    {
        if (ballenaActivada) return;

        ballenaActivada = true;
        indiceRutaBallena = 0;
        if (mantaAscensorOriginal != null)
        {
            if (mantaAscensorOriginal.rideableSurface != null)
            {
                mantaAscensorOriginal.rideableSurface.ReleaseLock();
                mantaAscensorOriginal.rideableSurface.ForceDetach();
            }
            mantaAscensorOriginal.enabled = false;
        }
        MountPassengersOnWhale();
    }

    private void MountPassengersOnWhale()
    {
        if (ballena == null) return;

        if (babosa != null)
        {
            babosa.SetParent(ballena, true);
            babosa.position = ballena.position + new Vector3(babosaOffsetSobreBallena.x, babosaOffsetSobreBallena.y, 0f);
            StopRigidbody(babosa);
        }

        if (mantarraya != null && mantarraya.gameObject.activeInHierarchy)
        {
            mantarraya.SetParent(ballena, true);
            mantarraya.position = ballena.position + new Vector3(mantarrayaOffsetSobreBallena.x, mantarrayaOffsetSobreBallena.y, 0f);
            StopRigidbody(mantarraya);
        }
    }

    private void ForceCameraFollow()
    {
        if (!forzarCamara || mainCamera == null) return;

        Transform target = babosa != null ? babosa : mantarraya;
        if (target == null) return;

        Vector3 desired = target.position + cameraOffset;
        desired.x = cameraFixedX;
        desired.y = Mathf.Clamp(desired.y, cameraMinY, cameraMaxY);
        desired.z = cameraOffset.z;

        if (camaraSoloSube)
        {
            cameraHighestY = Mathf.Max(cameraHighestY, desired.y);
            desired.y = cameraHighestY;
        }

        Camera[] cameras = Camera.allCameras;
        foreach (Camera camera in cameras)
        {
            if (camera == null || !camera.enabled) continue;
            camera.orthographic = true;
            camera.orthographicSize = mainCamera.orthographicSize;
            camera.transform.position = desired;
        }
    }

    private void ResolveReferences()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (babosa == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) babosa = player.transform;
        }

        if (babosa == null)
        {
            GameObject babosaObject = GameObject.Find("Babosa");
            if (babosaObject != null) babosa = babosaObject.transform;
        }

        if (mantarraya == null)
        {
            GameObject mantaObject = GameObject.Find("Mantarraya_Plataforma");
            if (mantaObject != null) mantarraya = mantaObject.transform;
        }

        if (ballena == null)
        {
            GameObject whaleObject = GameObject.Find("Ballena_Ascenso");
            if (whaleObject != null) ballena = whaleObject.transform;
        }
    }

    private static void DisableCameraControllers(Camera camera)
    {
        MonoBehaviour[] behaviours = camera.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null) continue;
            string fullName = behaviour.GetType().FullName;
            if (fullName != null && fullName.Contains("Cinemachine"))
            {
                behaviour.enabled = false;
            }
        }
    }

    private void DisableOtherCameras()
    {
        if (mainCamera == null) return;

        Camera[] cameras = Camera.allCameras;
        foreach (Camera camera in cameras)
        {
            if (camera == null || camera == mainCamera) continue;
            camera.enabled = false;
        }
    }

    private static void StopRigidbody(Transform target)
    {
        if (target == null) return;
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}
