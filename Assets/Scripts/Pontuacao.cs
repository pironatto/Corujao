using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResultadoFinal : MonoBehaviour
{
    public Text textoPontuacao;
    public Text textoDetalhes;

    void Start()
    {
        // Exibe a pontuação acumulada
        textoPontuacao.text = "Pontuação final: " + Score.pontuacaoTotal.ToString("F0");

        // Exibe pontuação por pergunta
        string detalhes = "";
        for (int i = 0; i < Score.pontosPorPergunta.Length; i++)
        {
            detalhes += "Pergunta " + (i + 1) + ": " + Score.pontosPorPergunta[i].ToString("F0") + "\n";
        }

        textoDetalhes.text = detalhes;
    }
}
