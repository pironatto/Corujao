using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class BancoDados : MonoBehaviour
{
    public TextMeshProUGUI r1, r2, r3, r4, pergunta, Tempo;
    public Button R1, R2, R3, R4;
    public GameObject PainelRespostas;

    [HideInInspector] public bool clickBotao, HabilitaRespostas;
    public bool Acertou = false;

    private string respostaCorreta;
    private Coroutine rotinaTimeout;

    public Slider BarraProgresso;
    private float tempoRestante;
    private bool contandoTempo = false;

    void Update()
    {
        if (contandoTempo)
        {
            tempoRestante -= Time.deltaTime;
            if (tempoRestante < 0) tempoRestante = 0;

            BarraProgresso.value = tempoRestante;
            Tempo.text = Mathf.RoundToInt(tempoRestante).ToString();

            if (tempoRestante <= 0)
            {
                contandoTempo = false;
                if (!clickBotao)
                {
                    MostrarRespostaCorreta();
                }
            }
        }
    }

    public void OnNovaPergunta(PerguntaData data)
    {
        if (data.itens == null || data.itens.Length < 8)
        {
            Debug.LogError("Itens recebidos incompletos!");
            return;
        }

        pergunta.text = data.itens[2];
        r1.text = data.itens[3];
        r2.text = data.itens[4];
        r3.text = data.itens[5];
        r4.text = data.itens[6];
        respostaCorreta = data.itens[7];

        ResetarBotoes();

        HabilitaRespostas = true;
        clickBotao = false;

        // Sempre reinicia com o tempo total da pergunta (ex.: 10 segundos)
        tempoRestante = data.tempoTotal;
        BarraProgresso.maxValue = data.tempoTotal;
        BarraProgresso.value = tempoRestante;
        Tempo.text = Mathf.RoundToInt(tempoRestante).ToString();

        contandoTempo = false; // só começa quando painel de respostas aparece
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

        if (rotinaTimeout != null) StopCoroutine(rotinaTimeout);
        rotinaTimeout = StartCoroutine(TimeoutPergunta());
    }

    public void BotaoResposta()
    {
        string botaoClicado = EventSystem.current.currentSelectedGameObject.name;
        string respostas = "";

        switch (botaoClicado)
        {
            case "R1": respostas = "A"; break;
            case "R2": respostas = "B"; break;
            case "R3": respostas = "C"; break;
            case "R4": respostas = "D"; break;
        }

        clickBotao = true;
        Acertou = (respostas == respostaCorreta);

        if (Acertou)
        {
            switch (botaoClicado)
            {
                case "R1": R1.image.color = Color.green; break;
                case "R2": R2.image.color = Color.green; break;
                case "R3": R3.image.color = Color.green; break;
                case "R4": R4.image.color = Color.green; break;
            }
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

        R1.interactable = false;
        R2.interactable = false;
        R3.interactable = false;
        R4.interactable = false;

        contandoTempo = false;
        if (rotinaTimeout != null) StopCoroutine(rotinaTimeout);
    }

    private IEnumerator MostrarCorretaDepoisDeAtraso()
    {
        yield return new WaitForSeconds(1f);
        MostrarRespostaCorreta();
    }

    private IEnumerator TimeoutPergunta()
    {
        yield return new WaitForSeconds(tempoRestante);
        if (!clickBotao)
        {
            MostrarRespostaCorreta();
        }
    }

    private void MostrarRespostaCorreta()
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

        contandoTempo = false;
        Debug.Log("Resposta correta exibida automaticamente!");

        // Aguarda 2 segundos antes de liberar próxima pergunta
        StartCoroutine(AguardarAntesDaProximaPergunta());
    }

    private IEnumerator AguardarAntesDaProximaPergunta()
    {
        yield return new WaitForSeconds(2f);

        MostrarItens mostrarItens = FindFirstObjectByType<MostrarItens>();
        if (mostrarItens != null)
        {
            mostrarItens.LiberarProximaPergunta();
        }
    }
}
