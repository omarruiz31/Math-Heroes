using System;

/// <summary>
/// Registro individual de cada pregunta matemática respondida por el alumno.
/// Permite calcular estadísticas detalladas como porcentaje de acierto
/// por tipo de operación, tiempos de respuesta, etc.
/// </summary>
[Serializable]
public class QuestionRecord
{
    /// <summary>Tipo de operación: "+", "-", "×", "÷"</summary>
    public string operation;

    /// <summary>Primer número de la operación</summary>
    public int numberA;

    /// <summary>Segundo número de la operación</summary>
    public int numberB;

    /// <summary>La respuesta correcta de la operación</summary>
    public int correctAnswer;

    /// <summary>La respuesta que dio el alumno (-999 si no respondió / timeout)</summary>
    public int playerAnswer;

    /// <summary>True si el alumno respondió correctamente</summary>
    public bool wasCorrect;

    /// <summary>Tiempo en segundos que tardó en responder</summary>
    public float timeUsed;

    /// <summary>Fecha y hora en que se respondió la pregunta</summary>
    public string date;

    /// <summary>Nombre del enemigo/nivel donde se hizo la pregunta</summary>
    public string levelName;
}
