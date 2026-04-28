using UnityEngine;
using TMPro; // Necesario para usar TextMeshPro
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text preguntaText;
    public TMP_InputField respuestaInput;
    public TMP_Text mensajeFeedback;

    private int respuestaCorrecta;
    private int intentos = 0;

    void Start()
    {
        GenerarOperacion();
    }

    public void GenerarOperacion()
    {
        // Generamos dos números aleatorios (Nivel 1: del 1 al 10)
        int num1 = Random.Range(1, 11);
        int num2 = Random.Range(1, 11);
        
        respuestaCorrecta = num1 + num2;
        preguntaText.text = num1 + " + " + num2 + " = ?";
        
        respuestaInput.text = ""; // Limpiamos el campo
        respuestaInput.ActivateInputField(); // Ponemos el foco para escribir rápido
    }

    public void VerificarRespuesta()
    {
        int respuestaUsuario = int.Parse(respuestaInput.text);

        if (respuestaUsuario == respuestaCorrecta)
        {
            Debug.Log("¡Correcto!");
            mensajeFeedback.text = "¡Excelente!";
            intentos = 0;
            GenerarOperacion(); // Pasamos a la siguiente
            // Aquí luego añadirás el sonido y daño al enemigo
        }
        else
        {
            intentos++;
            if (intentos < 2)
            {
                mensajeFeedback.text = "Casi... ¡Intenta de nuevo!";
            }
            else
            {
                mensajeFeedback.text = "¡Oh no! El enemigo ataca.";
                intentos = 0;
                GenerarOperacion();
            }
        }
    }
}