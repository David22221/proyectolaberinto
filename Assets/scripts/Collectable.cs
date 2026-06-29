using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Collectable : MonoBehaviour
{
    [Header("Efectos")]
    public AudioClip sonidoRecoger;
    public GameObject efectoRecoger;    // Partícula opcional

    // ───────────────────────────────────────────────────────────
    // Opción A: Recoger por trigger de colisión con el jugador
    void OnTriggerEnter(Collider otro)
    {
        if (otro.CompareTag("Player"))
            Recoger();
    }

    // ───────────────────────────────────────────────────────────
    // Opción B: Recoger con XR Interaction Toolkit (grab)
    // Activa esto si usas XRGrabInteractable en el objeto
    public void AlSerAgarrado(SelectEnterEventArgs args)
    {
        Recoger();
    }

    // ───────────────────────────────────────────────────────────
    void Recoger()
    {
        if (sonidoRecoger != null)
            AudioSource.PlayClipAtPoint(sonidoRecoger, transform.position);

        if (efectoRecoger != null)
            Instantiate(efectoRecoger, transform.position, Quaternion.identity);

        GameManager.Instance?.RecogerCollectable();
        Destroy(gameObject);
    }
}
