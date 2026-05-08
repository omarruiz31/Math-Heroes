using System.Collections.Generic;
using UnityEngine;

public class QuestionGenerator : MonoBehaviour
{
    public struct Question
    {
        public string text;
        public int correctAnswer;
        public string explanation;
    }

    public Question GenerateQuestion(EnemyData enemy)
    {
        // Arma la lista de operaciones habilitadas para este enemigo
        List<string> ops = new List<string>();
        if (enemy.useAddition)      ops.Add("+");
        if (enemy.useSubtraction)   ops.Add("-");
        if (enemy.useMultiplication) ops.Add("×");
        if (enemy.useDivision)      ops.Add("÷");

        if (ops.Count == 0) ops.Add("+"); // fallback

        string op = ops[Random.Range(0, ops.Count)];
        int a, b, answer;
        string explanation;

        switch (op)
        {
            case "-":
                a = Random.Range(enemy.minNumber, enemy.maxNumber + 1);
                b = Random.Range(enemy.minNumber, a + 1); // evita negativos
                answer = a - b;
                explanation = $"{a} - {b} es quitar {b} a {a}, por eso queda {answer}.";
                break;

            case "×":
                a = Random.Range(enemy.minNumber, Mathf.Min(enemy.maxNumber, 12) + 1);
                b = Random.Range(enemy.minNumber, Mathf.Min(enemy.maxNumber, 12) + 1);
                answer = a * b;
                explanation = $"{a} x {b} significa sumar {a} un total de {b} veces: el resultado es {answer}.";
                break;

            case "÷":
                // Genera división exacta siempre
                answer = Random.Range(enemy.minNumber, enemy.maxNumber + 1);
                b      = Random.Range(1, Mathf.Min(enemy.maxNumber, 12) + 1);
                a      = answer * b;
                explanation = $"{a} ÷ {b} pregunta cuántos grupos de {b} caben en {a}: caben {answer}.";
                break;

            default: // suma
                a = Random.Range(enemy.minNumber, enemy.maxNumber + 1);
                b = Random.Range(enemy.minNumber, enemy.maxNumber + 1);
                answer = a + b;
                explanation = $"{a} + {b} junta ambas cantidades: {a} y {b} forman {answer}.";
                break;
        }

        return new Question
        {
            text = $"{a} {op} {b} = ?",
            correctAnswer = answer,
            explanation = explanation
        };
    }
}
