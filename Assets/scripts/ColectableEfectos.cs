using UnityEngine;

public class ColectableEfectos : MonoBehaviour
{
    [Header("Rotación")]
    public bool rotar = true;
    public float velocidadRotacion = 90f;    // Grados por segundo

    [Header("Flotación")]
    public bool flotar = true;
    public float alturaFlotacion = 0.3f;     // Qué tanto sube y baja
    public float velocidadFlotacion = 2f;    // Qué tan rápido flota

    [Header("Pulso de luz")]
    public bool pulsarLuz = true;
    public Light luzPropia;                  // Point Light hijo del objeto
    public float intensidadMin = 0.5f;
    public float intensidadMax = 2f;
    public float velocidadPulso = 3f;

    [Header("Pulso de escala")]
    public bool pulsarEscala = true;
    public float escalaMin = 0.9f;
    public float escalaMax = 1.1f;
    public float velocidadPulsoEscala = 2f;

    // ── privados ───────────────────────────────────────────────
    private Vector3 posicionInicial;
    private Vector3 escalaInicial;

    // ───────────────────────────────────────────────────────────
    void Start()
    {
        posicionInicial = transform.position;
        escalaInicial   = transform.localScale;

        // Si no asignaron luz en Inspector, busca una hija
        if (luzPropia == null)
            luzPropia = GetComponentInChildren<Light>();
    }

    // ───────────────────────────────────────────────────────────
    void Update()
    {
        if (rotar)
            Rotar();

        if (flotar)
            Flotar();

        if (pulsarLuz && luzPropia != null)
            PulsarLuz();

        if (pulsarEscala)
            PulsarEscala();
    }

    // ───────────────────────────────────────────────────────────
    void Rotar()
    {
        transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);
    }

    void Flotar()
    {
        float nuevaY = posicionInicial.y + 
                       Mathf.Sin(Time.time * velocidadFlotacion) * alturaFlotacion;
        transform.position = new Vector3(
            transform.position.x,
            nuevaY,
            transform.position.z
        );
    }

    void PulsarLuz()
    {
        luzPropia.intensity = Mathf.Lerp(
            intensidadMin,
            intensidadMax,
            (Mathf.Sin(Time.time * velocidadPulso) + 1f) / 2f
        );
    }

    void PulsarEscala()
    {
        float factor = Mathf.Lerp(
            escalaMin,
            escalaMax,
            (Mathf.Sin(Time.time * velocidadPulsoEscala) + 1f) / 2f
        );
        transform.localScale = escalaInicial * factor;
    }
}
