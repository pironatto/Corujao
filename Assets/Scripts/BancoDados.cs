using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BancoDados : MonoBehaviour
{
    public Button R1, R2, R3, R4;
    public TextMeshProUGUI pergunta;
    public TextMeshProUGUI cronometro;
    public MostrarItens mostrarItens;

    private string respostaCorreta;
    private string respostaEscolhida;
    private float tempoRestante;
    private bool respostaEnviada;
    private int indicePergunta;

    public bool contandoTempo = false;

    public void OnNovaPergunta(PerguntaData data)
    {
        if (WebSocketUnity.Instance == null)
        {
            Debug.LogError(
                "WebSocketUnity.Instance não está disponível."
            );

            return;
        }

        if (
            data == null ||
            data.itens == null ||
            data.itens.Length < 7
        )
        {
            Debug.LogError(
                "Pergunta recebida com dados incompletos."
            );

            return;
        }

        if (
            data.partidaId !=
            WebSocketUnity.Instance.partidaId
        )
        {
            Debug.LogWarning(
                "Pergunta recebida para outra partida."
            );

            return;
        }

        if (pergunta == null || cronometro == null)
        {
            Debug.LogError(
                "Pergunta ou cronometro não foram configurados."
            );

            return;
        }

        if (
            R1 == null ||
            R2 == null ||
            R3 == null ||
            R4 == null
        )
        {
            Debug.LogError(
                "Botões de resposta não foram configurados."
            );

            return;
        }

        StopAllCoroutines();

        contandoTempo = false;
        respostaEnviada = false;
        respostaEscolhida = "";
        respostaCorreta = "";

        indicePergunta =
            Mathf.Clamp(
                Score.pontosPorPergunta.Length - 1,
                0,
                Score.pontosPorPergunta.Length - 1
            );

        pergunta.text = data.itens[2];

        DefinirTextoBotao(R1, data.itens[3]);
        DefinirTextoBotao(R2, data.itens[4]);
        DefinirTextoBotao(R3, data.itens[5]);
        DefinirTextoBotao(R4, data.itens[6]);

        tempoRestante =
            Mathf.Max(0f, data.tempoTotal);

        ResetarBotoes();

        if (
            mostrarItens != null &&
            mostrarItens.opcoesResposta != null
        )
        {
            mostrarItens.opcoesResposta
                .SetActive(false);
        }

        StartCoroutine(
            MostrarOpcoesEIniciarCronometro()
        );
    }

    private void DefinirTextoBotao(
        Button botao,
        string texto
    )
    {
        if (botao == null)
            return;

        TextMeshProUGUI textoBotao =
            botao.GetComponentInChildren
            <TextMeshProUGUI>();

        if (textoBotao != null)
            textoBotao.text = texto.Trim();
    }

    private IEnumerator MostrarOpcoesEIniciarCronometro()
    {
        yield return new WaitForSeconds(2f);

        if (
            mostrarItens != null &&
            mostrarItens.opcoesResposta != null
        )
        {
            mostrarItens.opcoesResposta
                .SetActive(true);
        }

        ResetarBotoes();
        IniciarCronometro();

        Debug.Log(
            "Opções de resposta ativadas."
        );
    }

    public void ResetarBotoes()
    {
        ConfigurarBotao(R1);
        ConfigurarBotao(R2);
        ConfigurarBotao(R3);
        ConfigurarBotao(R4);
    }

    private void ConfigurarBotao(Button botao)
    {
        if (botao == null)
            return;

        botao.interactable = true;
        botao.image.color = Color.white;
    }

    public void IniciarCronometro()
    {
        if (tempoRestante <= 0f)
        {
            EnviarRespostaUmaVez("");
            return;
        }

        contandoTempo = true;
    }

    private void Update()
    {
        if (!contandoTempo)
            return;

        tempoRestante -= Time.deltaTime;

        if (cronometro != null)
        {
            cronometro.text =
                Mathf.Max(
                    0,
                    Mathf.CeilToInt(tempoRestante)
                ).ToString();
        }

        if (tempoRestante <= 0f)
        {
            tempoRestante = 0f;
            contandoTempo = false;

            EnviarRespostaUmaVez("");
        }
    }

    public void BotaoResposta(
        string botaoClicado
    )
    {
        if (
            !contandoTempo ||
            respostaEnviada
        )
        {
            return;
        }

        contandoTempo = false;

        respostaEscolhida =
            botaoClicado switch
            {
                "R1" => "A",
                "R2" => "B",
                "R3" => "C",
                "R4" => "D",
                _ => ""
            };

        if (
            string.IsNullOrEmpty(
                respostaEscolhida
            )
        )
        {
            return;
        }

        Button botaoSelecionado =
            ObterBotao(botaoClicado);

        if (botaoSelecionado != null)
        {
            botaoSelecionado.image.color =
                Color.yellow;
        }

        EnviarRespostaUmaVez(
            respostaEscolhida
        );

        Debug.Log(
            "Resposta enviada. Aguardando validação do servidor."
        );
    }

    private void EnviarRespostaUmaVez(
        string resposta
    )
    {
        if (respostaEnviada)
            return;

        respostaEnviada = true;

        WebSocketUnity.Instance?
            .EnviarResposta(resposta);
    }

    public void ProcessarResultadoServidor(
        MensagemResultadoResposta resultado
    )
    {
        if (resultado == null)
            return;

        contandoTempo = false;

        respostaCorreta =
            string.IsNullOrEmpty(resultado.correta)
                ? ""
                : resultado.correta.Trim().ToUpper();

        Score.pontuacaoTotal =
            Mathf.Max(
                0f,
                resultado.pontuacaoTotal
            );

        if (
            indicePergunta >= 0 &&
            indicePergunta <
            Score.pontosPorPergunta.Length
        )
        {
            Score.pontosPorPergunta[
                indicePergunta
            ] = Mathf.Max(
                0f,
                resultado.pontos
            );
        }

        Button botaoSelecionado =
            ObterBotaoPorResposta(
                resultado.resposta
            );

        if (botaoSelecionado != null)
        {
            botaoSelecionado.image.color =
                resultado.acertou
                    ? Color.green
                    : Color.red;
        }

        if (resultado.acertou)
        {
            DesativarBotoes();
        }
        else
        {
            StartCoroutine(
                MostrarCorretaDepoisDeAtraso()
            );
        }

        Debug.Log(
            "Resultado validado pelo servidor. " +
            "Acertou: " + resultado.acertou +
            " | Pontos: " + resultado.pontos +
            " | Total: " +
            resultado.pontuacaoTotal
        );
    }

    private Button ObterBotao(
        string nome
    )
    {
        return nome switch
        {
            "R1" => R1,
            "R2" => R2,
            "R3" => R3,
            "R4" => R4,
            _ => null
        };
    }

    private Button ObterBotaoPorResposta(
        string resposta
    )
    {
        return resposta switch
        {
            "A" => R1,
            "B" => R2,
            "C" => R3,
            "D" => R4,
            _ => null
        };
    }

    private IEnumerator MostrarCorretaDepoisDeAtraso()
    {
        yield return new WaitForSeconds(1f);

        MostrarRespostaCorreta();
    }

    private void MostrarRespostaCorreta()
    {
        Button botaoCorreto =
            ObterBotaoPorResposta(
                respostaCorreta
            );

        if (botaoCorreto != null)
        {
            botaoCorreto.image.color =
                Color.green;
        }

        DesativarBotoes();
    }

    private void DesativarBotoes()
    {
        if (R1 != null)
            R1.interactable = false;

        if (R2 != null)
            R2.interactable = false;

        if (R3 != null)
            R3.interactable = false;

        if (R4 != null)
            R4.interactable = false;
    }
}