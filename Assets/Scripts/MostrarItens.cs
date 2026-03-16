using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using NativeWebSocket;
using System.Text;
using System.Collections;

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
            textoUI.text = "Adversário encontrado! Vamos começar...";

        if (WebSocketUnity.Instance != null)
        {
            WebSocket ws = WebSocketUnity.Instance.Websocket; // 🔹 acesso direto sem reflexão
            if (ws != null)
            {
                ws.OnMessage += (bytes) =>
                {
                    string message = Encoding.UTF8.GetString(bytes);
                    Debug.Log("Mensagem recebida na CenaItens: " + message);

                    PerguntaData itensMsg = JsonUtility.FromJson<PerguntaData>(message);
                    if (itensMsg != null && itensMsg.tipo == "itens" && itensMsg.itens != null)
                    {
                        if (itensMsg.partidaId == WebSocketUnity.Instance.partidaId)
                        {
                            itensRecebidos = itensMsg.itens;
                            tempoTotalServidor = itensMsg.tempoTotal;
                            inicioServidor = itensMsg.inicio;
                            AtualizarUI();
                        }
                    }
                };

                // solicita a primeira pergunta com delay
                StartCoroutine(PedirPrimeiraPerguntaComDelay(ws));
            }
        }
    }

    private IEnumerator PedirPrimeiraPerguntaComDelay(WebSocket ws)
    {
        yield return new WaitForSeconds(2f);
        if (ws.State == WebSocketState.Open)
        {
            string msg = "{\"tipo\":\"novaPergunta\", \"partidaId\":\""
                         + WebSocketUnity.Instance.partidaId + "\"}";
            Debug.Log("Enviando novaPergunta com partidaId: " + WebSocketUnity.Instance.partidaId);
            ws.SendText(msg);
        }
    }

    private void AtualizarUI()
    {
        if (textoUI != null && itensRecebidos != null)
        {
            if (itensRecebidos.Length < 8)
            {
                Debug.LogError("Itens recebidos incompletos!");
                return;
            }

            canvasPrincipal?.SetActive(false);
            canvasAguardando?.SetActive(true);

            pergunta.text = itensRecebidos[2];
            opcoesResposta?.SetActive(false);

            if (bancoDados != null)
            {
                PerguntaData data = new PerguntaData
                {
                    partidaId = WebSocketUnity.Instance.partidaId,
                    itens = itensRecebidos,
                    tempoTotal = tempoTotalServidor,
                    inicio = inicioServidor
                };
                bancoDados.OnNovaPergunta(data);
            }

            StartCoroutine(MostrarRespostasDepoisDeAtraso());
        }
    }

    private IEnumerator MostrarRespostasDepoisDeAtraso()
    {
        yield return new WaitForSeconds(2f);
        bancoDados?.ResetarBotoes();
        opcoesResposta?.SetActive(true);
        bancoDados?.IniciarCronometro();
    }

    // chamado pelo BancoDados após feedback
    public void LiberarProximaPergunta()
    {
        Debug.Log("Liberando próxima pergunta...");
        if (WebSocketUnity.Instance != null)
        {
            WebSocket ws = WebSocketUnity.Instance.Websocket;
            if (ws != null && ws.State == WebSocketState.Open)
            {
                string msg = "{\"tipo\":\"novaPergunta\", \"partidaId\":\""
                             + WebSocketUnity.Instance.partidaId + "\"}";
                ws.SendText(msg);
                Debug.Log("Mensagem enviada ao servidor: " + msg);
            }
        }
    }

    private void Update()
    {
        if (WebSocketUnity.Instance != null)
            WebSocketUnity.Instance.Websocket.DispatchMessageQueue();
    }
}
