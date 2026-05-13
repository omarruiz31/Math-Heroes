using UnityEngine;
using UnityEngine.SceneManagement; // Requisito para cambiar de escenas

public class SceneNavigation : MonoBehaviour
{
    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu"); 
    }

    // Podemos agregar otra función para ir a la batalla (la usaremos más adelante)
    public void GoToBattle()
    {
        SceneManager.LoadScene("BattleScene");
    }
}