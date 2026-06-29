using UnityEngine;
using UnityEngine.AI;

public class TRexController : MonoBehaviour
{
    public enum Estado { Patrulla, Persigue, Ataca }
    public Estado estadoActual = Estado.Patrulla;

    private NavMeshAgent agente;
    private Animator animador;
    private Transform jugador;

    [Header("Detección")]
    public float radioDeteccion = 15f;      // Distancia para detectar al jugador
    public float radioAtaque    = 2.5f;     // Distancia para atacar

    [Header("Velocidades")]
    public float velocidadPatrulla  = 2f;
    public float velocidadPersecucion = 6f;

    [Header("Patrulla")]
    public Transform[] puntosPatrulla;      // Asignar en Inspector
    private int indicePunto = 0;

    [Header("Sonidos")]
    public AudioSource audioRugido;
    public AudioSource audioPasos;
    public AudioClip   clipRugido;
    public AudioClip   clipAtaque;
    public AudioClip clipPasos; 
    private float tiempoUltimoRugido = 0f;
    public float intervaloRugido = 8f;      // Rugido cada 8 segundos al perseguir

    // Estos nombres deben coincidir con los parámetros del Animator
    private static readonly int ParamCaminando  = Animator.StringToHash("caminando");
    private static readonly int ParamCorriendo  = Animator.StringToHash("corriendo");
    private static readonly int ParamAtacando   = Animator.StringToHash("atacando");

    private bool juegoTerminado = false;

    void Start()
    {
        agente   = GetComponent<NavMeshAgent>();
        animador = GetComponent<Animator>();

        // Busca al jugador por tag (asegúrate de tagear tu XR Rig como "Player")
        jugador = GameObject.FindGameObjectWithTag("Player").transform;

        IrAlSiguientePunto();
    }

    void Update()
    {
        if (juegoTerminado) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        switch (estadoActual)
        {
            case Estado.Patrulla:
                ActualizarPatrulla();
                if (distancia <= radioDeteccion)
                    CambiarEstado(Estado.Persigue);
                break;

            case Estado.Persigue:
                ActualizarPersecucion();
                if (distancia <= radioAtaque)
                    CambiarEstado(Estado.Ataca);
                else if (distancia > radioDeteccion * 1.5f)   // Pierde al jugador
                    CambiarEstado(Estado.Patrulla);
                break;

            case Estado.Ataca:
                ActualizarAtaque();
                if (distancia > radioAtaque)
                    CambiarEstado(Estado.Persigue);
                break;
        }
    }

    void ActualizarPatrulla()
    {
        if (puntosPatrulla.Length == 0) return;

        agente.speed = velocidadPatrulla;
        SetAnimacion(caminando: true, corriendo: false, atacando: false);

        // Al llegar al punto, ir al siguiente
        if (!agente.pathPending && agente.remainingDistance < 0.5f)
            IrAlSiguientePunto();
    }

    void IrAlSiguientePunto()
    {
        if (puntosPatrulla.Length == 0) return;
        agente.destination = puntosPatrulla[indicePunto].position;
        indicePunto = (indicePunto + 1) % puntosPatrulla.Length;
    }


    void ActualizarPersecucion()
    {
        agente.speed       = velocidadPersecucion;
        agente.destination = jugador.position;
        SetAnimacion(caminando: false, corriendo: true, atacando: false);

        // Rugido periódico mientras persigue
        if (Time.time - tiempoUltimoRugido > intervaloRugido)
        {
            ReproducirRugido(clipRugido);
            tiempoUltimoRugido = Time.time;
        }
    }


    void ActualizarAtaque()
    {
        agente.ResetPath();     // Se detiene
        SetAnimacion(caminando: false, corriendo: false, atacando: true);

        // Mira al jugador
        Vector3 direccion = (jugador.position - transform.position).normalized;
        direccion.y = 0;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direccion),
            Time.deltaTime * 5f
        );

        ReproducirRugido(clipAtaque);
        GameManager.Instance?.GameOver();    // Avisa al GameManager
        juegoTerminado = true;
    }


    void CambiarEstado(Estado nuevoEstado)
    {
        estadoActual = nuevoEstado;

        if (nuevoEstado == Estado.Persigue)
        {
            ReproducirRugido(clipRugido);
            tiempoUltimoRugido = Time.time;
        }
    }

    void SetAnimacion(bool caminando, bool corriendo, bool atacando)
    {
        if (animador == null) return;
        animador.SetBool(ParamCaminando, caminando);
        animador.SetBool(ParamCorriendo, corriendo);
        animador.SetBool(ParamAtacando,  atacando);
    }

    void ReproducirRugido(AudioClip clip)
    {
        if (audioRugido != null && clip != null && !audioRugido.isPlaying)
            audioRugido.PlayOneShot(clip);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioAtaque);
    }

    // Este método lo llama la animación automáticamente
    public void SonidoPaso()
    {
        if (audioPasos != null && clipPasos != null)
            audioPasos.PlayOneShot(clipPasos);
    }
}
