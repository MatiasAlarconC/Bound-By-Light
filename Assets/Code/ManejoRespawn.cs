using UnityEngine;

public class ManejoRespawn : MonoBehaviour
{
    [Header("Configuración de Personajes")]
    public Transform babosa;      // Arrastra aquí a la babosa en el Inspector
    public Transform pulpo;        // Arrastra aquí al pulpo en el Inspector

    private Vector3 puntoDeReaparicion;

    void Start()
    {
        // El punto inicial será donde empiece la babosa al darle Play
        if (babosa != null)
        {
            puntoDeReaparicion = babosa.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Si CUALQUIERA de los dos personajes toca la burbuja Checkpoint
        if (collision.CompareTag("Checkpoint"))
        {
            puntoDeReaparicion = collision.transform.position;
            
            Animator animatorBurbuja = collision.GetComponent<Animator>();
            if (animatorBurbuja != null)
            {
                animatorBurbuja.Play("Burbuja_Explotar");
            }
            
            collision.enabled = false; // Desactiva la burbuja para que no explote dos veces
        }

        // 2. Si CUALQUIERA de los dos toca una zona de muerte
        if (collision.CompareTag("DeadZone"))
        {
            MorirYReaparecerJuntos();
        }
    }

    private void MorirYReaparecerJuntos()
    {
        // Teletransportamos a AMBOS al mismo checkpoint
        if (babosa != null) babosa.position = puntoDeReaparicion;
        if (pulpo != null) pulpo.position = puntoDeReaparicion;

        // Frenamos las físicas de ambos para que no aparezcan con impulsos raros
        FrenarFisicas(babosa);
        FrenarFisicas(pulpo);
    }

    private void FrenarFisicas(Transform personaje)
    {
        if (personaje != null)
        {
            Rigidbody2D rb = personaje.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}
