using System;
using System.Collections;
using System.Text;
using NativeWebSocket;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[Serializable]
public class MensagemStatus
{
    public string tipo;
    public string mensagem;
    public string partidaId;
}

[Serializable]
public class MensagemMateria
{
    public string materia;
}

[Serializable]
public class MensagemParFormado
{
    public string tipo;
    public string partidaId;
    public string materia;
}

[Serializable]
public class MensagemSingle
{
    public string tipo;
    public string mensagem;
    public string partidaId;
}

[Serializable]
public class PerguntaData
{
    public string tipo;
    public string partidaId;
    public string[] itens;
    public float tempoTotal;
    public long inicio;
}

[Serializable]
public class MensagemResposta
{
    public string tipo;
    public string partidaId;
    public string resposta;
}

[Serializable]
public class MensagemPontuacao
{
    public string tipo;
    public string partidaId;
    public string jogadorId;
    public float pontos;
}

[Serializable]
public class MensagemFim
{
    public string tipo;
    public string partidaId;
    public string motivo;
}

public class WebSocketUnity : MonoBehaviour
{
    public GameObject canvasPrincipal;
    public GameObject canvasAguardando;
    public TMP_Text materia;

    private WebSocket websocket;

    public static WebSocketUnity Instance { get; private set; }

    [HideInInspector] public string materiaEscolhida;
    [HideInInspector] public string partidaId;

    public WebSocket Websocket => websocket;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (Instance == this)
            Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AtualizarReferenciasDaCena();
    }

    private void AtualizarReferenciasDaCena()
    {
        var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (var obj in allObjects)
        {
            if (obj.name == "Canvas Temas")
                canvasPrincipal = obj;
            else if (obj.name == "Canvas Aguardando")
                canvasAguardando = obj;
            else if (obj.name == "materia")
                materia = obj.GetComponent<TMP_Text>();
        }
    }

    private async void Start()
    {
        websocket = new WebSocket("ws://zeleystudios.online:3000");

        websocket.OnOpen += QuandoConectar;
        websocket.OnError += (erro) => Debug.LogError("Erro no WebSocket: " + erro);
        websocket.OnClose += (codigo) => Debug.LogWarning("WebSocket fechado. Código: " + codigo);
        websocket.OnMessage += QuandoReceberMensagem;

        try
        {
            await websocket.Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError("Não foi possível conectar ao WebSocket: " + ex.Message);
        }
    }

    private void QuandoConectar()
    {
        Debug.Log("Conectado ao servidor WebSocket!");
        AtualizarReferenciasDaCena();
    }

    private void QuandoReceberMensagem(byte[] bytes)
    {
        string mensagem = Encoding.UTF8.GetString(bytes);
        Debug.Log("Mensagem recebida: " + mensagem);
        ProcessarMensagem(mensagem);
    }

    private void ProcessarMensagem(string mensagem)
    {
        if (string.IsNullOrWhiteSpace(mensagem))
        {
            Debug.LogWarning("Mensagem WebSocket vazia.");
            return;
        }

        MensagemStatus status = JsonUtility.FromJson<MensagemStatus>(mensagem);

        if (status != null && status.tipo == "status")
        {
            if (!string.IsNullOrEmpty(status.mensagem) &&
                status.mensagem.StartsWith("Você escolheu"))
            {
                canvasPrincipal?.SetActive(false);
                canvasAguardando?.SetActive(true);

                if (materia != null)
                    materia.text = "Você escolheu " + materiaEscolhida + ". Aguardando outro jogador...";

                return;
            }

            if (!string.IsNullOrEmpty(status.mensagem) &&
                status.mensagem.StartsWith("Nenhum adversário encontrado"))
            {
                if (!string.IsNullOrEmpty(status.partidaId))
                    partidaId = status.partidaId;

                Debug.Log("Partida single iniciada: " + partidaId);

                TMP_Text aviso = canvasAguardando?.GetComponentInChildren<TMP_Text>();
                if (aviso != null)
                {
                    aviso.text = "Nenhum adversário encontrado.\nPartida individual iniciada!";
                    StartCoroutine(FadeMensagem(aviso));
                }

                StartCoroutine(IniciarPartidaSingle());
                return;
            }

            if (!string.IsNullOrEmpty(status.mensagem) &&
                status.mensagem.StartsWith("Sessão reiniciada"))
            {
                partidaId = string.Empty;
                Debug.Log("Sessão reiniciada pelo servidor.");
                return;
            }
        }

        MensagemParFormado partida = JsonUtility.FromJson<MensagemParFormado>(mensagem);

        if (partida != null && partida.tipo == "parFormado")
        {
            partidaId = partida.partidaId;
            materiaEscolhida = partida.materia;

            Debug.Log("Partida multiplayer formada: " + partidaId);
            SceneManager.LoadScene("Perguntas");
            return;
        }

        PerguntaData pergunta = JsonUtility.FromJson<PerguntaData>(mensagem);

        if (pergunta != null && pergunta.tipo == "itens")
        {
            if (pergunta.partidaId != partidaId)
            {
                Debug.LogWarning("Pergunta recebida para outra partida. Esperada: " + partidaId + " | Recebida: " + pergunta.partidaId);
                return;
            }

            BancoDados banco = FindFirstObjectByType<BancoDados>();
            if (banco == null)
            {
                Debug.LogError("BancoDados não foi encontrado na cena Perguntas.");
                return;
            }

            banco.OnNovaPergunta(pergunta);
            return;
        }

        MensagemFim fim = JsonUtility.FromJson<MensagemFim>(mensagem);

        if (fim != null && fim.tipo == "fim")
        {
            if (!string.IsNullOrEmpty(fim.partidaId) && fim.partidaId != partidaId)
                return;

            Debug.Log("Fim das perguntas recebido.");
            if (!string.IsNullOrEmpty(fim.motivo))
                Debug.Log("Motivo do encerramento: " + fim.motivo);

            StartCoroutine(AguardarFim());
            return;
        }

        MensagemPontuacao pontuacao = JsonUtility.FromJson<MensagemPontuacao>(mensagem);

        if (pontuacao != null && pontuacao.tipo == "pontuacaoOponente")
        {
            if (pontuacao.partidaId != partidaId)
                return;

            Score.pontuacaoOponente = pontuacao.pontos;
            FindFirstObjectByType<Score>()?.IncrementarBarraOponente(pontuacao.pontos);
            Debug.Log("Pontuação do oponente atualizada: " + pontuacao.pontos);
            return;
        }

        Debug.LogWarning("Mensagem não reconhecida pelo cliente: " + mensagem);
    }

    private IEnumerator AguardarFim()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Pontuacao");
    }

    private IEnumerator IniciarPartidaSingle()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Perguntas");
    }

    private IEnumerator FadeMensagem(TMP_Text texto)
    {
        if (texto == null)
            yield break;

        texto.alpha = 0f;

        for (float t = 0f; t < 1f; t += Time.deltaTime)
        {
            texto.alpha = t;
            yield return null;
        }

        texto.alpha = 1f;
        yield return new WaitForSeconds(2f);

        for (float t = 1f; t > 0f; t -= Time.deltaTime)
        {
            texto.alpha = t;
            yield return null;
        }

        texto.alpha = 0f;
    }

    public async void EnviarMateria(string materiaSelecionada)
    {
        if (!WebSocketEstaAberto())
        {
            Debug.LogWarning("WebSocket não está conectado. Matéria não enviada.");
            return;
        }

        if (string.IsNullOrWhiteSpace(materiaSelecionada))
        {
            Debug.LogWarning("Matéria inválida.");
            return;
        }

        materiaEscolhida = materiaSelecionada.Trim().ToLower();

        MensagemMateria mensagem = new MensagemMateria
        {
            materia = materiaEscolhida
        };

        string json = JsonUtility.ToJson(mensagem);

        try
        {
            await websocket.SendText(json);
            Debug.Log("Matéria enviada: " + json);
        }
        catch (Exception ex)
        {
            Debug.LogError("Erro ao enviar matéria: " + ex.Message);
        }
    }

    public void OnMateriaSelecionada(string materiaSelecionada)
    {
        string botaoClicado = EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null
            ? EventSystem.current.currentSelectedGameObject.name
            : "";

        string materiaDoBotao = botaoClicado switch
        {
            "BtHistoria" => "historia",
            "BtCiencias" => "ciencias",
            "BtMatematica" => "matematica",
            "BtFisica" => "fisica",
            "BtHarryPotter" => "harrypotter",
            "BtGeografia" => "geografia",
            "BtBiologia" => "biologia",
            "BtMedicina" => "medicina",
            _ => materiaSelecionada
        };

        EnviarMateria(materiaDoBotao);
    }

    public async void EnviarResposta(string resposta)
    {
        if (!WebSocketEstaAberto())
        {
            Debug.LogWarning("WebSocket não está conectado. Resposta não enviada.");
            return;
        }

        MensagemResposta mensagem = new MensagemResposta
        {
            tipo = "resposta",
            partidaId = partidaId,
            resposta = string.IsNullOrWhiteSpace(resposta) ? string.Empty : resposta.Trim().ToUpper()
        };

        string json = JsonUtility.ToJson(mensagem);
        try
        {
            await websocket.SendText(json);
            Debug.Log("Resposta enviada ao servidor: " + json);
        }
        catch (Exception ex)
        {
            Debug.LogError("Erro ao enviar resposta: " + ex.Message);
        }
    }

    public async void EnviarPontuacao(string jogadorId, float pontos)
    {
        if (!WebSocketEstaAberto())
        {
            Debug.LogWarning("WebSocket não está conectado. Pontuação não enviada.");
            return;
        }

        MensagemPontuacao mensagem = new MensagemPontuacao
        {
            tipo = "pontuacao",
            partidaId = partidaId,
            jogadorId = jogadorId,
            pontos = pontos
        };

        string json = JsonUtility.ToJson(mensagem);

        try
        {
            await websocket.SendText(json);
            Debug.Log("Pontuação enviada ao servidor: " + json);
        }
        catch (Exception ex)
        {
            Debug.LogError("Erro ao enviar pontuação: " + ex.Message);
        }
    }

    private bool WebSocketEstaAberto()
    {
        return websocket != null && websocket.State == WebSocketState.Open;
    }

    private void Update()
    {
        websocket?.DispatchMessageQueue();
    }

    private async void OnApplicationQuit()
    {
        if (websocket == null)
            return;

        try
        {
            await websocket.Close();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Erro ao fechar WebSocket: " + ex.Message);
        }
    }
}