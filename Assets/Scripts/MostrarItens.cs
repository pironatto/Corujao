using UnityEngine;
using TMPro;
using NativeWebSocket;
using System.Text;
using System.Collections;
using System;

[System.Serializable]
public class MensagemItens
{
    public string tipo;
    public string[] itens;
    public float tempoTotal; // em segundos
    public long inicio;      // timestamp em ms
}


public class MostrarItens : MonoBehaviour
{
    public TMP_Text textoUI;
    public string[] itensRecebidos;
    public GameObject canvasPrincipal;
    public GameObject canvasAguardando;
    public TextMeshProUGUI pergunta;
    public GameObject opcoesResposta;

    public BancoDados bancoDados; // arraste no Inspector

    private float tempoTotalServidor = 10f;
    private long inicioServidor = 0;

    private void Start()
    {
        if (textoUI != null)
        {
            textoUI.text = "Adversário encontrado....";
        }

        if (WebSocketUnity.Instance != null)
        {
            WebSocket ws = GetWebSocket();
            if (ws != null)
            {
                ws.OnMessage += (bytes) =>
                {
                    string message = Encoding.UTF8.GetString(bytes);
                    Debug.Log("Mensagem recebida na CenaItens: " + message);

                    MensagemItens itensMsg = JsonUtility.FromJson<MensagemItens>(message);
                    if (itensMsg != null && itensMsg.tipo == "itens" && itensMsg.itens != null)
                    {
                        itensRecebidos = itensMsg.itens;
                        tempoTotalServidor = itensMsg.tempoTotal;
                        inicioServidor = itensMsg.inicio;
                        AtualizarUI();
                    }
                };
            }
        }
    }

    private WebSocket GetWebSocket()
    {
        var wsUnity = typeof(WebSocketUnity)
            .GetField("websocket", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(WebSocketUnity.Instance) as WebSocket;

        return wsUnity;
    }

    private void AtualizarUI()
    {
        if (textoUI != null && itensRecebidos != null)
        {
            if (itensRecebidos.Length < 8)
            {
                Debug.LogError("Itens recebidos incompletos! Tamanho: " + itensRecebidos.Length);
                return;
            }

            if (canvasPrincipal != null) canvasPrincipal.SetActive(false);
            if (canvasAguardando != null) canvasAguardando.SetActive(true);

            // Mostra a pergunta imediatamente
            pergunta.text = itensRecebidos[2];

            // Garante que painel de respostas fique oculto
            if (opcoesResposta != null) opcoesResposta.SetActive(false);

            // Prepara BancoDados com a nova pergunta (cronômetro resetado mas ainda parado)
            if (bancoDados != null)
            {
                PerguntaData data = new PerguntaData
                {
                    itens = itensRecebidos,
                    tempoTotal = tempoTotalServidor,
                    inicio = inicioServidor
                };
                bancoDados.OnNovaPergunta(data);
            }

            // Só ativa o painel depois de 1 segundo
            StartCoroutine(MostrarRespostasDepoisDeAtraso());
        }
    }
    private IEnumerator MostrarRespostasDepoisDeAtraso()
    {
        yield return new WaitForSeconds(1f);

        // Resetar botões antes de mostrar painel
        if (bancoDados != null)
        {
            bancoDados.ResetarBotoes();
        }

        if (opcoesResposta != null) opcoesResposta.SetActive(true);

        // Agora sim inicia o cronômetro
        if (bancoDados != null)
        {
            bancoDados.IniciarCronometro();
        }
    }



    // Método chamado pelo BancoDados após aguardar alguns segundos de feedback
    public void LiberarProximaPergunta()
    {
        Debug.Log("Liberando próxima pergunta...");

        if (WebSocketUnity.Instance != null)
        {
            WebSocket ws = GetWebSocket();
            if (ws != null && ws.State == WebSocketState.Open)
            {
                // Envia mensagem ao servidor pedindo nova pergunta
                string msg = "{\"tipo\":\"novaPergunta\"}";
                ws.SendText(msg);
                Debug.Log("Mensagem enviada ao servidor: " + msg);
            }
            else
            {
                Debug.LogWarning("WebSocket não está aberto. Não foi possível pedir nova pergunta.");
            }
        }
    }

    private void Update()
    {
        if (WebSocketUnity.Instance != null)
        {
            typeof(WebSocketUnity)
                .GetMethod("Update", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(WebSocketUnity.Instance, null);
        }
    }
}
