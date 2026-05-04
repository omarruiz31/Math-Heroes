using System.Collections.Generic;
using UnityEngine;

public class QuestionGenerator : MonoBehaviour
{
    public struct Question
    {
        public string text;
        public int correctAnswer;
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

        switch (op)
        {
            case "-":
                a = Random.Range(enemy.minNumber, enemy.maxNumber + 1);
                b = Random.Range(enemy.minNumber, a + 1); // evita negativos
                answer = a - b;
                break;

            case "×":
                a = Random.Range(enemy.minNumber, Mathf.Min(enemy.maxNumber, 12) + 1);
                b = Random.Range(enemy.minNumber, Mathf.Min(enemy.maxNumber, 12) + 1);
                answer = a * b;
                break;

            case "÷":
                // Genera división exacta siempre
                answer = Random.Range(enemy.minNumber, enemy.maxNumber + 1);
                b      = Random.Range(1, Mathf.Min(enemy.maxNumber, 12) + 1);
                a      = answer * b;
                break;

            default: // suma
                a = Random.Range(enemy.minNumber, enemy.maxNumber + 1);
                b = Random.Range(enemy.minNumber, enemy.maxNumber + 1);
                answer = a + b;
                break;
        }

        return new Question { text = $"{a} {op} {b} = ?", correctAnswer = answer };
    }
}