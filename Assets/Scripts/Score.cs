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
    public Text textoResultado;

    public static float pontuacaoTotal;
    public static float pontuacaoOponente;
    public static float[] pontosPorPergunta = new float[5];

    public static bool partidaIndividual;
    public static bool adversarioDesconectou;

    private void Start()
    {
        ConfigurarSliders();

        if (
            SceneManager.GetActiveScene().name ==
            "Pontuacao"
        )
        {
            MostrarResultados();
        }
    }

    private void Update()
    {
        AtualizarSliders();
    }

    private void ConfigurarSliders()
    {
        if (SliderA != null)
        {
            SliderA.minValue = 0f;
            SliderA.maxValue = 50f;
            SliderA.value = Mathf.Clamp(
                pontuacaoTotal,
                SliderA.minValue,
                SliderA.maxValue
            );
        }

        if (SliderOponente != null)
        {
            SliderOponente.minValue = 0f;
            SliderOponente.maxValue = 50f;
            SliderOponente.value = Mathf.Clamp(
                pontuacaoOponente,
                SliderOponente.minValue,
                SliderOponente.maxValue
            );
        }
    }

    private void AtualizarSliders()
    {
        if (SliderA != null)
        {
            SliderA.value = Mathf.Clamp(
                pontuacaoTotal,
                SliderA.minValue,
                SliderA.maxValue
            );
        }

        if (SliderOponente != null)
        {
            SliderOponente.value = Mathf.Clamp(
                pontuacaoOponente,
                SliderOponente.minValue,
                SliderOponente.maxValue
            );
        }
    }

    public void IncrementarBarra(float valor)
    {
        if (SliderA == null)
        {
            return;
        }

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

    private void MostrarResultados()
    {
        if (textoPontuacaoTotal != null)
        {
            textoPontuacaoTotal.text =
                "Sua Pontuação: " +
                pontuacaoTotal.ToString("F1");
        }

        if (textoPontuacaoOponente != null)
        {
            if (partidaIndividual)
            {
                textoPontuacaoOponente.text =
                    "Sem oponente";
            }
            else if (adversarioDesconectou)
            {
                textoPontuacaoOponente.text =
                    "Oponente desconectado";
            }
            else
            {
                textoPontuacaoOponente.text =
                    "Pontuação do Oponente: " +
                    pontuacaoOponente.ToString("F1");
            }
        }

        if (textoDetalhes != null)
        {
            string detalhes = "";

            for (
                int i = 0;
                i < pontosPorPergunta.Length;
                i++
            )
            {
                detalhes +=
                    $"Pergunta {i + 1}: " +
                    $"{pontosPorPergunta[i]:F1}\n";
            }

            textoDetalhes.text = detalhes;
        }

        MostrarResultadoFinal();
    }

    private void MostrarResultadoFinal()
    {
        if (textoResultado == null)
        {
            Debug.LogWarning(
                "O campo textoResultado não foi associado " +
                "no Inspector."
            );

            return;
        }

        if (partidaIndividual)
        {
            textoResultado.text =
                "PARTIDA INDIVIDUAL";

            textoResultado.color =
                Color.cyan;

            return;
        }

        if (adversarioDesconectou)
        {
            textoResultado.text =
                "OPONENTE DESCONECTADO";

            textoResultado.color =
                Color.yellow;

            return;
        }

        if (
            Mathf.Approximately(
                pontuacaoTotal,
                pontuacaoOponente
            )
        )
        {
            textoResultado.text =
                "EMPATE!";

            textoResultado.color =
                Color.yellow;
        }
        else if (
            pontuacaoTotal >
            pontuacaoOponente
        )
        {
            textoResultado.text =
                "VOCÊ VENCEU!";

            textoResultado.color =
                Color.green;
        }
        else
        {
            textoResultado.text =
                "VOCÊ PERDEU!";

            textoResultado.color =
                Color.red;
        }
    }

    public static void ResetarPontuacao()
    {
        pontuacaoTotal = 0f;
        pontuacaoOponente = 0f;
        pontosPorPergunta = new float[5];

        partidaIndividual = false;
        adversarioDesconectou = false;
    }

    public async void EscolherTema()
    {
        WebSocket ws =
            WebSocketUnity.Instance != null
                ? WebSocketUnity.Instance.Websocket
                : null;

        if (
            ws != null &&
            ws.State == WebSocketState.Open
        )
        {
            await ws.SendText(
                "{\"tipo\":\"reset\"}"
            );
        }

        ResetarPontuacao();

        SceneManager.LoadScene("Temas");
    }
}