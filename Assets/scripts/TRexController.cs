using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TRexController : MonoBehaviour
{
    public enum Estado { Patrulla, Persigue, Ataca }
    public Estado estadoActual = Estado.Patrulla;

    private NavMeshAgent agente;
    private Animator animador;
    private Transform jugador;

    [Header("Detección")]
    public float radioDeteccion = 15f;
    public float radioAtaque    = 2.5f;

    [Header("Velocidades")]
    public float velocidadPatrulla    = 2f;
    public float velocidadPersecucion = 6f;

    [Header("Patrulla")]
    public Transform[] puntosPatrulla;
    private int indicePunto = 0;

    [Header("Sonidos")]
    public AudioSource audioRugido;     // Parlante para rugidos
    public AudioSource audioPasos;      // Parlante para pasos
    public AudioClip   clipRugido;      // Rugido periódico (siempre)
    public AudioClip   clipAtaque;      // Rugido al detectar jugador
    public AudioClip   clipPasos;       // Pasos en loop

    [Header("Intervalos de rugido")]
    public float intervaloRugidoPatrulla  = 12f; // Rugido cada 12s patrullando
    public float intervaloRugidoPersigue  = 5f;  // Rugido cada 5s persiguiendo
    private float tiempoUltimoRugido = 0f;

    private static readonly int ParamCaminando = Animator.StringToHash("caminando");
    private static readonly int ParamCorriendo = Animator.StringToHash("corriendo");
    private static readonly int ParamAtacando  = Animator.StringToHash("atacando");

    private bool juegoTerminado = false;

    void Start()
    {
        agente   = GetComponent<NavMeshAgent>();
        animador = GetComponentInChildren<Animator>();

        // Configurar audio pasos
        if (audioPasos != null && clipPasos != null)
        {
            audioPasos.clip         = clipPasos;
            audioPasos.loop         = true;
            audioPasos.volume       = 0.4f;
            audioPasos.spatialBlend = 0f;
        }

        // Configurar audio rugido
        if (audioRugido != null)
        {
            audioRugido.spatialBlend = 0f;
            audioRugido.loop         = false;
            audioRugido.playOnAwake  = false;
        }

        // Jugador
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
            jugador = jugadorObj.transform;
        else
            Debug.LogError("[TRex] No hay objeto con tag 'Player'");

        Invoke("IrAlSiguientePunto", 1f);
    }

    void Update()
    {
        if (juegoTerminado) return;
        if (jugador == null) return;

        if (animador != null)
            animador.applyRootMotion = false;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        switch (estadoActual)
        {
            case Estado.Patrulla:
                ActualizarPatrulla();
                // Rugido periódico lento mientras patrulla
                RugidoPeriodico(intervaloRugidoPatrulla);
                if (distancia <= radioDeteccion)
                    CambiarEstado(Estado.Persigue);
                break;

            case Estado.Persigue:
                ActualizarPersecucion();
                // Rugido periódico rápido mientras persigue
                RugidoPeriodico(intervaloRugidoPersigue);
                if (distancia <= radioAtaque)
                    CambiarEstado(Estado.Ataca);
                else if (distancia > radioDeteccion * 1.5f)
                    CambiarEstado(Estado.Patrulla);
                break;

            case Estado.Ataca:
                ActualizarAtaque();
                if (distancia > radioAtaque)
                    CambiarEstado(Estado.Persigue);
                break;
        }
    }

    void RugidoPeriodico(float intervalo)
    {
        if (Time.time - tiempoUltimoRugido >= intervalo)
        {
            if (audioRugido != null && clipRugido != null && !audioRugido.isPlaying)
            {
                audioRugido.PlayOneShot(clipRugido);
                tiempoUltimoRugido = Time.time;
            }
        }
    }

    void ActualizarPatrulla()
    {
        if (puntosPatrulla.Length == 0) return;

        agente.speed = velocidadPatrulla;
        SetAnimacion(caminando: true, corriendo: false, atacando: false);
        ReproducirPasos(true);

        if (agente.isOnNavMesh &&
            !agente.pathPending &&
            agente.remainingDistance <= agente.stoppingDistance)
        {
            IrAlSiguientePunto();
        }
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
        ReproducirPasos(true);
    }

    void ActualizarAtaque()
    {
        agente.ResetPath();
        SetAnimacion(caminando: false, corriendo: false, atacando: true);
        ReproducirPasos(false);

        // Mira al jugador
        Vector3 direccion = (jugador.position - transform.position).normalized;
        direccion.y = 0;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direccion),
            Time.deltaTime * 5f
        );
    }

    void CambiarEstado(Estado nuevoEstado)
    {
        Estado estadoAnterior = estadoActual;
        estadoActual = nuevoEstado;
        Debug.Log("[TRex] Estado: " + nuevoEstado);

        // Al detectar al jugador → rugido de ataque
        if (nuevoEstado == Estado.Persigue && estadoAnterior == Estado.Patrulla)
        {
            if (audioRugido != null && clipAtaque != null)
            {
                audioRugido.Stop();
                audioRugido.PlayOneShot(clipAtaque);
                tiempoUltimoRugido = Time.time + 3f; // Pausa el rugido periódico 3s
            }
        }

        // Al atrapar al jugador → rugido de ataque + Game Over con delay
        if (nuevoEstado == Estado.Ataca && estadoAnterior == Estado.Persigue)
        {
            if (audioRugido != null && clipAtaque != null)
            {
                audioRugido.Stop();
                audioRugido.PlayOneShot(clipAtaque);
            }
            StartCoroutine(GameOverConDelay(1.5f)); // Espera 1.5s para que suene el rugido
        }

        // Al perder al jugador → vuelve a patrullar
        if (nuevoEstado == Estado.Patrulla)
        {
            IrAlSiguientePunto();
            tiempoUltimoRugido = Time.time; // Resetea el timer
        }
    }


    IEnumerator GameOverConDelay(float delay)
    {
        juegoTerminado = true;
        yield return new WaitForSeconds(delay);
        GameManager.Instance?.GameOver();
    }

    void SetAnimacion(bool caminando, bool corriendo, bool atacando)
    {
        if (animador == null) return;
        animador.SetBool(ParamCaminando, caminando);
        animador.SetBool(ParamCorriendo, corriendo);
        animador.SetBool(ParamAtacando,  atacando);
    }

    void ReproducirPasos(bool moviéndose)
    {
        if (audioPasos == null) return;
        if (moviéndose && !audioPasos.isPlaying)
            audioPasos.Play();
        else if (!moviéndose && audioPasos.isPlaying)
            audioPasos.Stop();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioAtaque);
    }
}