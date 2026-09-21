using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NativeWebSocket;

public class Score : MonoBehaviour
{
    public Slider SliderA;
    public Slider SliderOponente;

    public Text textoPontuacaoTotal;
    public Text textoDetalhes;
    public Text textoPontuacaoOponente;

    public static float pontuacaoTotal;
    public static float pontuacaoOponente;
    public static float[] pontosPorPergunta = new float[5];

    private static int quantidadePerguntasRegistradas;

    private void Start()
    {
        ConfigurarSliders();

        if (SceneManager.GetActiveScene().name == "Pontuacao")
            MostrarResultados();
    }

    private void ConfigurarSliders()
    {
        if (SliderA != null)
        {
            SliderA.minValue = 0f;
            SliderA.maxValue = 50f;
            SliderA.value = pontuacaoTotal;
        }

        if (SliderOponente != null)
        {
            SliderOponente.minValue = 0f;
            SliderOponente.maxValue = 50f;
            SliderOponente.value = pontuacaoOponente;
        }
    }

    private void Update()
    {
        if (SliderA != null)
            SliderA.value = Mathf.Clamp(pontuacaoTotal, 0f, SliderA.maxValue);

        if (SliderOponente != null)
        {
            SliderOponente.value =
                Mathf.Clamp(pontuacaoOponente, 0f, SliderOponente.maxValue);
        }
    }

    public void IncrementarBarra(float valor)
    {
        if (SliderA == null)
            return;

        SliderA.value = Mathf.Clamp(
            SliderA.value + Mathf.Max(0f, valor),
            SliderA.minValue,
            SliderA.maxValue
        );
    }

    public void IncrementarBarraOponente(float valor)
    {
        pontuacaoOponente = Mathf.Max(0f, valor);

        if (SliderOponente != null)
        {
            SliderOponente.value = Mathf.Clamp(
                pontuacaoOponente,
                SliderOponente.minValue,
                SliderOponente.maxValue
            );
        }
    }

    public static void RegistrarPontuacao(float pontos)
    {
        if (quantidadePerguntasRegistradas >= pontosPorPergunta.Length)
            return;

        pontos = Mathf.Max(0f, pontos);

        pontosPorPergunta[quantidadePerguntasRegistradas] = pontos;
        quantidadePerguntasRegistradas++;
    }

    private void MostrarResultados()
    {
        if (textoPontuacaoTotal != null)
        {
            textoPontuacaoTotal.text =
                "Sua Pontuação: " + pontuacaoTotal.ToString("F1");
        }

        if (textoPontuacaoOponente != null)
        {
            textoPontuacaoOponente.text =
                "Pontuação do Oponente: " +
                pontuacaoOponente.ToString("F1");
        }

        if (textoDetalhes != null)
        {
            string detalhes = string.Empty;

            for (int i = 0; i < pontosPorPergunta.Length; i++)
            {
                detalhes +=
                    $"Pergunta {i + 1}: " +
                    $"{pontosPorPergunta[i]:F1}\n";
            }

            textoDetalhes.text = detalhes;
        }
    }

    public static void ResetarPontuacao()
    {
        pontuacaoTotal = 0f;
        pontuacaoOponente = 0f;
        pontosPorPergunta = new float[5];
        quantidadePerguntasRegistradas = 0;
    }

    public async void EscolherTema()
    {
        WebSocket ws =
            WebSocketUnity.Instance != null
                ? WebSocketUnity.Instance.Websocket
                : null;

        if (ws != null && ws.State == WebSocketState.Open)
        {
            await ws.SendText("{\"tipo\":\"reset\"}");
        }

        ResetarPontuacao();

        SceneManager.LoadScene("Temas");
    }
}