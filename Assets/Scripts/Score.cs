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

    private void Start()
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

        if (SceneManager.GetActiveScene().name == "Pontuacao")
            MostrarResultados();
    }

    private void Update()
    {
        if (SliderA != null)
            SliderA.value = Mathf.Clamp(pontuacaoTotal, 0f, SliderA.maxValue);

        if (SliderOponente != null)
            SliderOponente.value = Mathf.Clamp(pontuacaoOponente, 0f, SliderOponente.maxValue);
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
            SliderOponente.value = Mathf.Clamp(pontuacaoOponente, SliderOponente.minValue, SliderOponente.maxValue);
    }

    private void MostrarResultados()
    {
        if (textoPontuacaoTotal != null)
            textoPontuacaoTotal.text = "Sua Pontuação: " + pontuacaoTotal.ToString("F1");

        if (textoDetalhes != null)
        {
            string detalhes = "";

            for (int i = 0; i < pontosPorPergunta.Length; i++)
                detalhes += $"Pergunta {i + 1}: {pontosPorPergunta[i]:F1}\n";

            textoDetalhes.text = detalhes;
        }

        if (textoPontuacaoOponente != null)
            textoPontuacaoOponente.text = "Pontuação do Oponente: " + pontuacaoOponente.ToString("F1");
    }

    public static void ResetarPontuacao()
    {
        pontuacaoTotal = 0f;
        pontuacaoOponente = 0f;
        pontosPorPergunta = new float[5];
    }

    public async void EscolherTema()
    {
        var ws = WebSocketUnity.Instance != null ? WebSocketUnity.Instance.Websocket : null;

        if (ws != null && ws.State == WebSocketState.Open)
            await ws.SendText("{\"tipo\":\"reset\"}");

        ResetarPontuacao();
        SceneManager.LoadScene("Temas");
    }
}