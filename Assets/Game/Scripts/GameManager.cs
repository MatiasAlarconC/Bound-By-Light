using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Personajes - ¡Asegúrate de ponerlos bien en el Inspector!")]
    [SerializeField] private BabosaControl hermanoMenorBabosa;
    [SerializeField] private PulpoColumpio hermanoMayorPulpo;

    [Header("Configuración de Control")]
    [SerializeField] private Key teclaCambio = Key.C;

    private bool controlandoAlPulpo = false;

    void Start()
    {
        // Forzamos el estado inicial al dar Play: Controlas a la Babosa
        controlandoAlPulpo = false;
        ActualizarControlesEstrictos();
    }

    void Update()
    {
        // Al presionar la tecla de cambio, alternamos el interruptor
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard[teclaCambio].wasPressedThisFrame)
        {
            controlandoAlPulpo = !controlandoAlPulpo;
            ActualizarControlesEstrictos();
        }
    }

    void ActualizarControlesEstrictos()
    {
        if (controlandoAlPulpo)
        {
            if (hermanoMayorPulpo != null) hermanoMayorPulpo.SetControlActivo(true);
            if (hermanoMenorBabosa != null) hermanoMenorBabosa.SetControlActivo(false);
            
            Debug.Log("<color=cyan>--- CONTROL: PULPO ACTIVO (Babosa bloqueada) ---</color>");
        }
        else
        {
            if (hermanoMayorPulpo != null) hermanoMayorPulpo.SetControlActivo(false);
            if (hermanoMenorBabosa != null) hermanoMenorBabosa.SetControlActivo(true);
            
            Debug.Log("<color=green>--- CONTROL: BABOSA ACTIVA (Pulpo bloqueado) ---</color>");
        }
    }
}