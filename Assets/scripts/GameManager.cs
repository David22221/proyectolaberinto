using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject pantallaGameOver;
    public GameObject pantallaVictoria;

    [Header("Colectables")]
    public int totalColectables = 8;
    private int colectablesRecogidos = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (pantallaGameOver) pantallaGameOver.SetActive(false);
        if (pantallaVictoria) pantallaVictoria.SetActive(false);
    }

    // Llamado por cada collectable al ser recogido
    public void RecogerCollectable()
    {
        colectablesRecogidos++;
        Debug.Log($"Colectables: {colectablesRecogidos}/{totalColectables}");

        if (colectablesRecogidos >= totalColectables)
            Victoria();
    }

    public void GameOver()
    {
        Debug.Log("GAME OVER - El T-Rex atrapó al jugador");
        if (pantallaGameOver) pantallaGameOver.SetActive(true);
        //Time.timeScale = 0f;    // Pausa el juego
    }

    public void Victoria()
    {
        Debug.Log("VICTORIA - Todos los colectables recogidos");
        if (pantallaVictoria) pantallaVictoria.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
