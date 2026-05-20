using UnityEngine;
using UnityEngine.SceneManagement;

public class transicionEscena : MonoBehaviour
{
    public string BattleScene;

    private void OnTriggerEnter2D(Collider2D oOtro)
    {
        // Verificamos lógicamente si el objeto que entró tiene la etiqueta "Player"
        if (oOtro.CompareTag("Player"))
        {
            cambiarDeNivel();
        }
    }

    private void cambiarDeNivel()
    {
        SceneManager.LoadScene(BattleScene);
    }
}