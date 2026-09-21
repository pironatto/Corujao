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
    private float tempoRestante;
    private bool respostaEnviada;
    public bool contandoTempo = false;

    public void OnNovaPergunta(PerguntaData data)
    {
        if (WebSocketUnity.Instance == null)
        {
            Debug.LogError("WebSocketUnity.Instance não está disponível.");
            return;
        }

        if (data == null || data.itens == null || data.itens.Length < 8)
        {
            Debug.LogError("Pergunta recebida com dados incompletos.");
            return;
        }

        if (data.partidaId != WebSocketUnity.Instance.partidaId)
        {
            Debug.LogWarning("Pergunta recebida para outra partida.");
            return;
        }

        if (pergunta == null || cronometro == null)
        {
            Debug.LogError("pergunta ou cronometro não foram configurados no Inspector.");
            return;
        }

        if (R1 == null || R2 == null || R3 == null || R4 == null)
        {
            Debug.LogError("Botões de resposta não foram configurados no Inspector.");
            return;
        }

        contandoTempo = false;
        respostaEnviada = false;

        pergunta.text = data.itens[2];

        DefinirTextoBotao(R1, data.itens[3]);
        DefinirTextoBotao(R2, data.itens[4]);
        DefinirTextoBotao(R3, data.itens[5]);
        DefinirTextoBotao(R4, data.itens[6]);

        respostaCorreta = data.itens[7].Trim().ToUpper();
        tempoRestante = Mathf.Max(0f, data.tempoTotal);

        ResetarBotoes();

        if (mostrarItens != null && mostrarItens.opcoesResposta != null)
            mostrarItens.opcoesResposta.SetActive(false);

        StartCoroutine(MostrarOpcoesEIniciarCronometro());
    }

    private void DefinirTextoBotao(Button botao, string texto)
    {
        if (botao == null)
            return;

        TextMeshProUGUI textoBotao = botao.GetComponentInChildren<TextMeshProUGUI>();
        if (textoBotao != null)
            textoBotao.text = texto.Trim();
    }

    private IEnumerator MostrarOpcoesEIniciarCronometro()
    {
        yield return new WaitForSeconds(2f);

        if (mostrarItens != null && mostrarItens.opcoesResposta != null)
            mostrarItens.opcoesResposta.SetActive(true);

        ResetarBotoes();
        IniciarCronometro();

        Debug.Log("Opções de resposta ativadas.");
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
            MostrarRespostaCorreta(false);
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
            cronometro.text = Mathf.Max(0, Mathf.CeilToInt(tempoRestante)).ToString();

        if (tempoRestante <= 0f)
        {
            tempoRestante = 0f;
            contandoTempo = false;

            EnviarRespostaUmaVez("");
            MostrarRespostaCorreta(false);
        }
    }

    public void BotaoResposta(string botaoClicado)
    {
        if (!contandoTempo || respostaEnviada)
            return;

        contandoTempo = false;

        string resposta = botaoClicado switch
        {
            "R1" => "A",
            "R2" => "B",
            "R3" => "C",
            "R4" => "D",
            _ => ""
        };

        if (string.IsNullOrEmpty(resposta))
            return;

        EnviarRespostaUmaVez(resposta);

        bool acertou = resposta == respostaCorreta;

        Button botaoSelecionado = ObterBotao(botaoClicado);

        if (botaoSelecionado != null)
            botaoSelecionado.image.color = acertou ? Color.green : Color.red;

        if (acertou)
        {
            float pontos = Mathf.Max(0f, tempoRestante);

            Score.pontuacaoTotal += pontos;
            RegistrarPontuacao(pontos);

            FindFirstObjectByType<Score>()?.IncrementarBarra(pontos);
            DesativarBotoes();
        }
        else
        {
            StartCoroutine(MostrarCorretaDepoisDeAtraso());
        }
    }

    private void EnviarRespostaUmaVez(string resposta)
    {
        if (respostaEnviada)
            return;

        respostaEnviada = true;
        WebSocketUnity.Instance?.EnviarResposta(resposta);
    }

    private Button ObterBotao(string nome)
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

    private IEnumerator MostrarCorretaDepoisDeAtraso()
    {
        yield return new WaitForSeconds(1f);
        MostrarRespostaCorreta(false);
    }

    private void MostrarRespostaCorreta(bool acertou)
    {
        switch (respostaCorreta)
        {
            case "A":
                if (R1 != null) R1.image.color = Color.green;
                break;
            case "B":
                if (R2 != null) R2.image.color = Color.green;
                break;
            case "C":
                if (R3 != null) R3.image.color = Color.green;
                break;
            case "D":
                if (R4 != null) R4.image.color = Color.green;
                break;
        }

        DesativarBotoes();

        if (!acertou)
            RegistrarPontuacao(0f);
    }

    private void DesativarBotoes()
    {
        if (R1 != null) R1.interactable = false;
        if (R2 != null) R2.interactable = false;
        if (R3 != null) R3.interactable = false;
        if (R4 != null) R4.interactable = false;
    }

private void RegistrarPontuacao(float pontos)
{
    Score.RegistrarPontuacao(pontos);

    WebSocketUnity.Instance?.EnviarPontuacao(
        "JogadorLocal",
        Score.pontuacaoTotal
    );
}
}