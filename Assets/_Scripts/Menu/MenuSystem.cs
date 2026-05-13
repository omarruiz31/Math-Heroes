using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSystem : MonoBehaviour
{

    public GameObject optionsMenu;
    public GameObject mainMenu;

    public void OpenOptionsPanel()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }
    public void OpenMainMenuPanel()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    public void Jugar()
    {
        SceneManager.LoadScene("WorldMap");
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego");
        Application.Quit();
    }


}
