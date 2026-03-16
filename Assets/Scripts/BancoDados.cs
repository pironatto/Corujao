using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BancoDados : MonoBehaviour
{
    public Button R1, R2, R3, R4;
    public TextMeshProUGUI pergunta;
    public TextMeshProUGUI cronometro;
    public MostrarItens mostrarItens;

    private string respostaCorreta;
    private float tempoTotal;
    private float tempoRestante;
    public bool contandoTempo = false;

    private string partidaIdAtual;

    public void OnNovaPergunta(PerguntaData data)
    {
        if (data.partidaId != WebSocketUnity.Instance.partidaId) return;

        partidaIdAtual = data.partidaId;

        pergunta.text = data.itens[2];
        R1.GetComponentInChildren<TextMeshProUGUI>().text = data.itens[3];
        R2.GetComponentInChildren<TextMeshProUGUI>().text = data.itens[4];
        R3.GetComponentInChildren<TextMeshProUGUI>().text = data.itens[5];
        R4.GetComponentInChildren<TextMeshProUGUI>().text = data.itens[6];

        respostaCorreta = data.itens[7];
        tempoTotal = data.tempoTotal;

        // 🔹 Sincroniza cronômetro com servidor
        long agora = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float atraso = (agora - data.inicio) / 1000f;
        tempoRestante = Mathf.Max(tempoTotal - atraso, 0);

        ResetarBotoes();
        IniciarCronometro();
    }

    public void ResetarBotoes()
    {
        R1.interactable = true; R1.image.color = Color.white;
        R2.interactable = true; R2.image.color = Color.white;
        R3.interactable = true; R3.image.color = Color.white;
        R4.interactable = true; R4.image.color = Color.white;
    }

    public void IniciarCronometro()
    {
        contandoTempo = true;
    }

    private void Update()
    {
        if (contandoTempo)
        {
            tempoRestante -= Time.deltaTime;
            cronometro.text = Mathf.CeilToInt(tempoRestante).ToString();

            if (tempoRestante <= 0)
            {
                contandoTempo = false;
                MostrarRespostaCorreta(false);
            }
        }
    }

    public void BotaoResposta(string botaoClicado)
    {
        contandoTempo = false;
        bool acertou = false;

        switch (botaoClicado)
        {
            case "R1": acertou = (respostaCorreta == "A"); break;
            case "R2": acertou = (respostaCorreta == "B"); break;
            case "R3": acertou = (respostaCorreta == "C"); break;
            case "R4": acertou = (respostaCorreta == "D"); break;
        }

        if (acertou)
        {
            switch (botaoClicado)
            {
                case "R1": R1.image.color = Color.green; break;
                case "R2": R2.image.color = Color.green; break;
                case "R3": R3.image.color = Color.green; break;
                case "R4": R4.image.color = Color.green; break;
            }

            Score.pontuacaoTotal += tempoRestante;
            RegistrarPontuacao(tempoRestante);

            FindFirstObjectByType<Score>()?.IncrementarBarra(tempoRestante);

            StartCoroutine(AguardarAntesDaProximaPergunta());
        }
        else
        {
            switch (botaoClicado)
            {
                case "R1": R1.image.color = Color.red; break;
                case "R2": R2.image.color = Color.red; break;
                case "R3": R3.image.color = Color.red; break;
                case "R4": R4.image.color = Color.red; break;
            }
            StartCoroutine(MostrarCorretaDepoisDeAtraso());
        }
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
            case "A": R1.image.color = Color.green; break;
            case "B": R2.image.color = Color.green; break;
            case "C": R3.image.color = Color.green; break;
            case "D": R4.image.color = Color.green; break;
        }

        R1.interactable = false;
        R2.interactable = false;
        R3.interactable = false;
        R4.interactable = false;

        if (!acertou)
            RegistrarPontuacao(0f);

        StartCoroutine(AguardarAntesDaProximaPergunta());
    }

    private IEnumerator AguardarAntesDaProximaPergunta()
    {
        yield return new WaitForSeconds(2f);
        mostrarItens.LiberarProximaPergunta();
    }

    private void RegistrarPontuacao(float pontos)
    {
        for (int i = 0; i < Score.pontosPorPergunta.Length; i++)
        {
            if (Score.pontosPorPergunta[i] == 0f)
            {
                Score.pontosPorPergunta[i] = pontos;
                break;
            }
        }

        // 🔹 Envia pontuação ao servidor
        WebSocketUnity.Instance.EnviarPontuacao("JogadorLocal", Score.pontuacaoTotal);
    }
}
