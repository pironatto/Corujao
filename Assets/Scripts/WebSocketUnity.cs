using System;
using System.Collections;
using System.Text;
using NativeWebSocket;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WebSocketUnity : MonoBehaviour
{
    private WebSocket websocket;

    public static WebSocketUnity Instance { get; private set; }

    [HideInInspector]
    public string materiaEscolhida;

    [HideInInspector]
    public string partidaId;

    [HideInInspector]
    public string nomeJogadorAtual;

    [HideInInspector]
    public string nomeOponenteAtual;

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
        {
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(AtualizarReferenciasDepoisDaCenaCarregar());
    }

    private IEnumerator AtualizarReferenciasDepoisDaCenaCarregar()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        GameSessionController.Instance?.AtualizarReferenciasDaCena();

        Debug.Log("Referências atualizadas. Cena atual: " + SceneManager.GetActiveScene().name);
    }

    private async void Start()
    {
        websocket = new WebSocket("ws://zeleystudios.online:3000");

        websocket.OnOpen += QuandoConectar;

        websocket.OnError += (erro) =>
        {
            Debug.LogError("Erro no WebSocket: " + erro);
        };

        websocket.OnClose += (codigo) =>
        {
            Debug.LogWarning("WebSocket fechado. Código: " + codigo);
        };

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
        EnviarIdentificacaoUsuario();
        StartCoroutine(AtualizarReferenciasDepoisDaCenaCarregar());
    }

    private async void EnviarIdentificacaoUsuario()
    {
        if (!WebSocketEstaAberto())
        {
            Debug.LogWarning("WebSocket ainda não está aberto. Identificação não enviada.");
            return;
        }

        string usuarioId = PlayerPrefs.GetString("usuarioId", string.Empty);

        if (string.IsNullOrWhiteSpace(usuarioId))
        {
            Debug.LogError("ID do usuário não encontrado no PlayerPrefs.");
            return;
        }

        MensagemIdentificarUsuario mensagem = new MensagemIdentificarUsuario
        {
            tipo = "identificarUsuario",
            usuarioId = usuarioId
        };

        string json = JsonUtility.ToJson(mensagem);

        try
        {
            await websocket.SendText(json);
            Debug.Log("Identificação enviada ao servidor: " + json);
        }
        catch (Exception ex)
        {
            Debug.LogError("Erro ao enviar identificação: " + ex.Message);
        }
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

        if (mensagem.Contains("\"tipo\":\"usuarioIdentificado\""))
        {
            ProcessarUsuarioIdentificado(mensagem);
            return;
        }

        if (mensagem.Contains("\"tipo\":\"status\""))
        {
            ProcessarStatus(mensagem);
            return;
        }

        if (mensagem.Contains("\"tipo\":\"parFormado\""))
        {
            ProcessarPartidaFormada(mensagem);
            return;
        }

        if (mensagem.Contains("\"tipo\":\"itens\""))
        {
            ProcessarPergunta(mensagem);
            return;
        }

        if (mensagem.Contains("\"tipo\":\"resultadoResposta\""))
        {
            ProcessarResultadoResposta(mensagem);
            return;
        }

        if (mensagem.Contains("\"tipo\":\"resumoRespostas\""))
        {
            ProcessarResumoRespostas(mensagem);
            return;
        }

        if (mensagem.Contains("\"tipo\":\"pontuacaoOponente\""))
        {
            ProcessarPontuacaoOponente(mensagem);
            return;
        }

        if (mensagem.Contains("\"tipo\":\"fim\""))
        {
            ProcessarFim(mensagem);
            return;
        }

        Debug.LogWarning("Mensagem não reconhecida pelo cliente: " + mensagem);
    }

    private void ProcessarUsuarioIdentificado(string mensagem)
    {
        MensagemIdentificado identificado = JsonUtility.FromJson<MensagemIdentificado>(mensagem);

        if (identificado != null)
        {
            Debug.Log("Usuário identificado pela Unity: " + identificado.nome + " (" + identificado.usuarioId + ")");

            if (!string.IsNullOrEmpty(identificado.nome))
            {
                PlayerPrefs.SetString("usuarioNome", identificado.nome);
            }

            if (!string.IsNullOrEmpty(identificado.avatarUrl))
            {
                PlayerPrefs.SetString("avatarUrl", identificado.avatarUrl);
                Debug.Log("[WebSocketUnity] Avatar salvo no PlayerPrefs: " + identificado.avatarUrl);
            }

            PlayerPrefs.Save();
        }
    }

    private void ProcessarStatus(string mensagem)
    {
        MensagemStatus status = JsonUtility.FromJson<MensagemStatus>(mensagem);

        if (status == null || status.tipo != "status")
        {
            return;
        }

        GameSessionController.Instance?.ProcessarStatus(status);
    }

    private void ProcessarPartidaFormada(string mensagem)
    {
        MensagemParFormado partida = JsonUtility.FromJson<MensagemParFormado>(mensagem);

        if (partida == null || partida.tipo != "parFormado")
        {
            return;
        }

        Score.ResetarPontuacao();

        partidaId = partida.partidaId;
        materiaEscolhida = partida.materia;
        nomeJogadorAtual = partida.nomeJogador;
        nomeOponenteAtual = partida.nomeOponente;

        Debug.Log("Partida multiplayer formada: " + partidaId + ". Exibindo adversário encontrado.");

        GameSessionController.Instance?.PrepararPartidaAntesDoFade();
    }

    private void ProcessarPergunta(string mensagem)
    {
        PerguntaData pergunta = JsonUtility.FromJson<PerguntaData>(mensagem);

        if (pergunta == null || pergunta.tipo != "itens")
        {
            return;
        }

        if (pergunta.partidaId != partidaId)
        {
            Debug.LogWarning("Pergunta recebida para outra partida.");
            return;
        }

        BancoDados banco = FindFirstObjectByType<BancoDados>();

        if (banco == null)
        {
            Debug.LogError("BancoDados não foi encontrado na cena Perguntas.");
            return;
        }

        banco.OnNovaPergunta(pergunta);
    }

    private void ProcessarResultadoResposta(string mensagem)
    {
        MensagemResultadoResposta resultado = JsonUtility.FromJson<MensagemResultadoResposta>(mensagem);

        if (resultado == null || resultado.tipo != "resultadoResposta")
        {
            return;
        }

        if (resultado.partidaId != partidaId)
        {
            return;
        }

        BancoDados banco = FindFirstObjectByType<BancoDados>();

        if (banco != null)
        {
            banco.ProcessarResultadoServidor(resultado);
        }
    }

    private void ProcessarResumoRespostas(string mensagem)
    {
        MensagemResumoRespostas resumo = JsonUtility.FromJson<MensagemResumoRespostas>(mensagem);

        if (resumo == null || resumo.tipo != "resumoRespostas")
        {
            return;
        }

        if (resumo.partidaId != partidaId)
        {
            Debug.LogWarning("Resumo recebido para outra partida.");
            return;
        }

        BancoDados banco = FindFirstObjectByType<BancoDados>();

        if (banco == null)
        {
            Debug.LogError("BancoDados não foi encontrado ao processar o resumo das respostas.");
            return;
        }

        banco.MostrarMarcadorOponente(resumo.respostaOponente);

        Debug.Log(
            "Resposta do adversário revelada: " +
            (string.IsNullOrEmpty(resumo.respostaOponente) ? "SEM RESPOSTA" : resumo.respostaOponente)
        );
    }

    private void ProcessarPontuacaoOponente(string mensagem)
    {
        MensagemPontuacaoOponente pontuacao = JsonUtility.FromJson<MensagemPontuacaoOponente>(mensagem);

        if (pontuacao == null || pontuacao.tipo != "pontuacaoOponente")
        {
            return;
        }

        if (pontuacao.partidaId != partidaId)
        {
            return;
        }

        Score.pontuacaoOponente = pontuacao.pontos;

        FindFirstObjectByType<Score>()?.IncrementarBarraOponente(pontuacao.pontos);

        Debug.Log("Pontuação do oponente atualizada: " + pontuacao.pontos);
    }

    private void ProcessarFim(string mensagem)
    {
        MensagemFim fim = JsonUtility.FromJson<MensagemFim>(mensagem);

        if (fim == null || fim.tipo != "fim")
        {
            return;
        }

        if (!string.IsNullOrEmpty(fim.partidaId) && fim.partidaId != partidaId)
        {
            return;
        }

        if (fim.motivo == "adversarioDesconectado")
        {
            GameSessionController.Instance?.TratarDesconexaoDoAdversario();
            return;
        }

        Score.pontuacaoTotal = fim.pontuacaoJogador;
        Score.pontuacaoOponente = fim.pontuacaoOponente;
        Score.partidaIndividual = fim.partidaIndividual;
        Score.nomeJogador = fim.nomeJogador;
        Score.nomeOponente = fim.nomeOponente;
        Score.ratingAnterior = fim.ratingAnterior;
        Score.ratingNovo = fim.ratingNovo;
        Score.variacaoRating = fim.variacaoRating;
        Score.faixaAntes = fim.faixaAntes;
        Score.faixaDepois = fim.faixaDepois;
        Score.subiuFaixa = fim.subiuFaixa;

        Debug.Log(
            "Resultado final recebido. Jogador: " +
            Score.pontuacaoTotal +
            " | Oponente: " +
            Score.pontuacaoOponente
        );

        GameSessionController.Instance?.AguardarFim();
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
        GameSessionController.Instance?.OnMateriaSelecionada(materiaSelecionada);
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
        {
            return;
        }

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