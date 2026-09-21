using System.Collections;
using NativeWebSocket;
using TMPro;
using UnityEngine;

public class MostrarItens : MonoBehaviour
{
    public TMP_Text textoUI;
    public string[] itensRecebidos;

    public GameObject canvasPrincipal;
    public GameObject canvasAguardando;

    public TextMeshProUGUI pergunta;
    public GameObject opcoesResposta;

    public BancoDados bancoDados;

    private const float TEMPO_MAXIMO_ESPERA = 15f;

    private void Start()
    {
        if (textoUI != null)
            textoUI.text = "Adversário encontrado! Vamos começar...";

        StartCoroutine(AguardarWebSocketESolicitarPrimeiraPergunta());
    }

    private IEnumerator AguardarWebSocketESolicitarPrimeiraPergunta()
    {
        float tempoEsperado = 0f;

        while (true)
        {
            if (WebSocketUnity.Instance != null)
            {
                WebSocket ws = WebSocketUnity.Instance.Websocket;

                bool socketAberto =
                    ws != null &&
                    ws.State == WebSocketState.Open;

                bool partidaDisponivel =
                    !string.IsNullOrEmpty(WebSocketUnity.Instance.partidaId);

                if (socketAberto && partidaDisponivel)
                {
                    yield return new WaitForSeconds(1f);
                    EnviarSolicitacaoPrimeiraPergunta(ws);
                    yield break;
                }
            }

            tempoEsperado += Time.deltaTime;

            if (tempoEsperado >= TEMPO_MAXIMO_ESPERA)
            {
                Debug.LogError(
                    "Não foi possível solicitar a primeira pergunta: " +
                    "WebSocket fechado ou partidaId indisponível."
                );

                yield break;
            }

            yield return null;
        }
    }

    private void EnviarSolicitacaoPrimeiraPergunta(WebSocket ws)
    {
        if (WebSocketUnity.Instance == null)
        {
            Debug.LogError("WebSocketUnity.Instance não está disponível.");
            return;
        }

        string partidaId = WebSocketUnity.Instance.partidaId;

        if (string.IsNullOrEmpty(partidaId))
        {
            Debug.LogError("Não foi possível solicitar a pergunta: partidaId vazio.");
            return;
        }

        if (ws == null || ws.State != WebSocketState.Open)
        {
            Debug.LogWarning("WebSocket não está aberto. Primeira pergunta não solicitada.");
            return;
        }

        string mensagem =
            "{\"tipo\":\"novaPergunta\",\"partidaId\":\"" +
            partidaId +
            "\"}";

        ws.SendText(mensagem);

        Debug.Log("Enviando novaPergunta com partidaId: " + partidaId);
    }

    public void AtualizarUI()
    {
        Debug.Log("AtualizarUI não é necessário neste fluxo. As perguntas são processadas pelo WebSocketUnity.");
    }

    public void LiberarProximaPergunta()
    {
        Debug.Log("A próxima pergunta será solicitada pelo servidor após o recebimento das respostas.");
    }
}