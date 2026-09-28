using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using NativeWebSocket;

public class Score : MonoBehaviour
{
    public Slider SliderA;
    public Slider SliderOponente;

    public Text textoPontuacaoTotal;
    public Text textoDetalhes;
    public Text textoPontuacaoOponente;
    public Text textoResultado;

    // 🆕 Textos de rating (arraste os TMPs no Inspector)
    public TextMeshProUGUI textoRating;
    public TextMeshProUGUI textoVariacao;
    public TextMeshProUGUI textoSubiuFaixa;

    public static float pontuacaoTotal;
    public static float pontuacaoOponente;
    public static float[] pontosPorPergunta = new float[5];

    public static bool partidaIndividual;

    // 🆕 Campos de rating (preenchidos no fim da partida multiplayer)
    public static int ratingAnterior;
    public static int ratingNovo;
    public static int variacaoRating;
    public static string faixaAntes;
    public static string faixaDepois;
    public static bool subiuFaixa;

    private void Start()
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
        MostrarRatingFinal();
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

        if (
            Mathf.Approximately(
                pontuacaoTotal,
                pontuacaoOponente
            )
        )
        {
            textoResultado.text = "EMPATE!";
            textoResultado.color = Color.yellow;
        }
        else if (
            pontuacaoTotal > pontuacaoOponente
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

    // ============================================================
    // 🆕 Exibe as informações de rating na tela
    // ============================================================
    private void MostrarRatingFinal()
    {
        // Partida individual → esconde tudo
        if (partidaIndividual)
        {
            if (textoRating != null)
                textoRating.gameObject.SetActive(false);

            if (textoVariacao != null)
                textoVariacao.gameObject.SetActive(false);

            if (textoSubiuFaixa != null)
                textoSubiuFaixa.gameObject.SetActive(false);

            return;
        }

        // ---- TxtRating ----
        if (textoRating != null)
        {
            textoRating.gameObject.SetActive(true);
            textoRating.text =
                $"Rating: {ratingAnterior} → {ratingNovo}";
            textoRating.color = Color.white;
        }

        // ---- TxtVariacao ----
        if (textoVariacao != null)
        {
            textoVariacao.gameObject.SetActive(true);

            if (variacaoRating > 0)
            {
                textoVariacao.text = $"▲ +{variacaoRating}";
                textoVariacao.color = Color.green;
            }
            else if (variacaoRating < 0)
            {
                textoVariacao.text = $"▼ {variacaoRating}";
                textoVariacao.color = Color.red;
            }
            else
            {
                textoVariacao.text = "— 0";
                textoVariacao.color = Color.gray;
            }
        }

        // ---- TxtSubiuFaixa ----
        if (textoSubiuFaixa != null)
        {
            if (subiuFaixa)
            {
                textoSubiuFaixa.gameObject.SetActive(true);
                textoSubiuFaixa.text =
                    $"Você subiu para {faixaDepois}!";
                textoSubiuFaixa.color =
                    new Color(1f, 0.84f, 0f); // dourado
            }
            else
            {
                textoSubiuFaixa.gameObject.SetActive(false);
            }
        }
    }

    public static void ResetarPontuacao()
    {
        pontuacaoTotal = 0f;
        pontuacaoOponente = 0f;
        pontosPorPergunta = new float[5];
        partidaIndividual = false;

        // 🆕 Zera os campos de rating
        ratingAnterior = 0;
        ratingNovo = 0;
        variacaoRating = 0;
        faixaAntes = null;
        faixaDepois = null;
        subiuFaixa = false;
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