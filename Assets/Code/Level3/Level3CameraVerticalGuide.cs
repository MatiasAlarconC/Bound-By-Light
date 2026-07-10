using UnityEngine;

/// <summary>
/// Optional simple camera guide for Level 3 blockout.
/// Use Cinemachine Rooms for final polish, but this helps if a temporary camera is needed.
/// </summary>
[DefaultExecutionOrder(32000)]
public class Level3CameraVerticalGuide : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.5f, -10f);
    public float suavizado = 5f;
    public bool soloSube = true;
    public float minY = -20f;
    public float maxY = 80f;
    public float activarSeguimientoEnY = -14.5f;
    public bool esperarPuntoDeActivacion = true;
    public bool seguirX = false;
    public float bordeSuperiorActivacion = 0.58f;
    public float bordeSuperiorCritico = 0.74f;
    public float suavizadoRapidoSubida = 13f;
    public float velocidadMaximaSubida = 26f;
    public bool seguimientoInstantaneoVertical = true;

    private float highestY;
    private Vector3 posicionInicial;
    private bool seguimientoActivado;
    private Camera camara;

    private void Awake()
    {
        posicionInicial = transform.position;
        highestY = transform.position.y;
        camara = GetComponent<Camera>();
        DisableInheritedCameraControllers();
        RefreshTargetIfNeeded();
    }

    private void Start()
    {
        FollowTarget(true);
    }

    private void LateUpdate()
    {
        FollowTarget(false);
    }

    private void OnPreCull()
    {
        FollowTarget(false);
    }

    private void FollowTarget(bool instantaneo)
    {
        RefreshTargetIfNeeded();
        if (target == null) return;

        if (esperarPuntoDeActivacion && !seguimientoActivado)
        {
            seguimientoActivado = target.position.y >= activarSeguimientoEnY;
            if (!seguimientoActivado)
            {
                transform.position = posicionInicial;
                return;
            }
        }

        Vector3 desired = target.position + offset;
        if (!seguirX)
        {
            desired.x = posicionInicial.x;
        }

        desired.y = Mathf.Clamp(desired.y, minY, maxY);
        desired.z = offset.z;

        float velocidadSeguimiento = suavizado;
        bool objetivoCercaDelBordeSuperior = false;
        if (camara != null && camara.orthographic)
        {
            float viewportY = camara.WorldToViewportPoint(target.position).y;
            objetivoCercaDelBordeSuperior = viewportY >= bordeSuperiorActivacion;
            if (objetivoCercaDelBordeSuperior)
            {
                velocidadSeguimiento = Mathf.Max(velocidadSeguimiento, suavizadoRapidoSubida);
            }

            if (viewportY >= bordeSuperiorCritico)
            {
                velocidadSeguimiento = Mathf.Max(velocidadSeguimiento, velocidadMaximaSubida);
            }
        }

        if (soloSube)
        {
            highestY = Mathf.Max(highestY, desired.y);
            desired.y = highestY;
        }

        if (instantaneo || seguimientoInstantaneoVertical)
        {
            transform.position = desired;
            return;
        }

        if (objetivoCercaDelBordeSuperior)
        {
            transform.position = Vector3.MoveTowards(transform.position, desired, velocidadSeguimiento * Time.deltaTime);
            return;
        }

        transform.position = Vector3.Lerp(transform.position, desired, velocidadSeguimiento * Time.deltaTime);
    }

    private void RefreshTargetIfNeeded()
    {
        if (target != null && target.gameObject.activeInHierarchy)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            return;
        }

        GameObject babosa = GameObject.Find("Babosa");
        if (babosa != null)
        {
            target = babosa.transform;
        }
    }

    private void DisableInheritedCameraControllers()
    {
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null || behaviour == this) continue;

            string typeName = behaviour.GetType().Name;
            string fullName = behaviour.GetType().FullName;
            if (typeName.Contains("Cinemachine") || (fullName != null && fullName.Contains("Cinemachine")))
            {
                behaviour.enabled = false;
            }
        }
    }
}
