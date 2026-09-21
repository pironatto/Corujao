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
public class MensagemResultadoResposta
{
    public string tipo;
    public string partidaId;
    public string resposta;
    public string correta;
    public bool acertou;
    public float pontos;
    public float pontuacaoTotal;
}

[Serializable]
public class MensagemPontuacaoOponente
{
    public string tipo;
    public string partidaId;
    public float pontos;
}

[Serializable]
public class MensagemFim
{
    public string tipo;
    public string partidaId;
    public string motivo;
    public float pontuacaoJogador;
    public float pontuacaoOponente;
}

public class WebSocketUnity : MonoBehaviour
{
    public GameObject canvasPrincipal;
    public GameObject canvasAguardando;
    public TMP_Text materia;

    private WebSocket websocket;

    public static WebSocketUnity Instance { get; private set; }

    [HideInInspector]
    public string materiaEscolhida;

    [HideInInspector]
    public string partidaId;

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

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode
    )
    {
        StartCoroutine(
            AtualizarReferenciasDepoisDaCenaCarregar()
        );
    }

    private IEnumerator AtualizarReferenciasDepoisDaCenaCarregar()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        AtualizarReferenciasDaCena();

        Debug.Log(
            "Referências atualizadas. Cena atual: " +
            SceneManager.GetActiveScene().name
        );
    }

    private void AtualizarReferenciasDaCena()
    {
        // Remove referências da cena anterior.
        canvasPrincipal = null;
        canvasAguardando = null;
        materia = null;

        Scene cenaAtual = SceneManager.GetActiveScene();

        if (!cenaAtual.IsValid() || !cenaAtual.isLoaded)
        {
            Debug.LogWarning(
                "A cena atual ainda não está disponível."
            );

            return;
        }

        GameObject[] objetosRaiz =
            cenaAtual.GetRootGameObjects();

        foreach (GameObject objetoRaiz in objetosRaiz)
        {
            Transform[] objetos =
                objetoRaiz.GetComponentsInChildren<Transform>(
                    true
                );

            foreach (Transform transformacao in objetos)
            {
                GameObject objeto =
                    transformacao.gameObject;

                if (
                    canvasPrincipal == null &&
                    objeto.name == "Canvas Temas"
                )
                {
                    canvasPrincipal = objeto;
                }

                if (
                    canvasAguardando == null &&
                    objeto.name == "Canvas Aguardando"
                )
                {
                    canvasAguardando = objeto;
                }

                if (
                    materia == null &&
                    objeto.name == "materia"
                )
                {
                    materia =
                        objeto.GetComponent<TMP_Text>();
                }
            }
        }

        Debug.Log(
            "Referências encontradas: " +
            "Canvas Temas = " +
            (canvasPrincipal != null) +
            " | Canvas Aguardando = " +
            (canvasAguardando != null) +
            " | materia = " +
            (materia != null)
        );
    }

    private void GarantirReferenciasDaCena()
    {
        bool referenciasInvalidas =
            canvasPrincipal == null ||
            canvasAguardando == null ||
            materia == null;

        if (referenciasInvalidas)
        {
            AtualizarReferenciasDaCena();
        }
    }

    private void MostrarTelaAguardando()
    {
        GarantirReferenciasDaCena();

        if (canvasPrincipal != null)
        {
            canvasPrincipal.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "Canvas Temas não foi encontrado."
            );
        }

        if (canvasAguardando != null)
        {
            canvasAguardando.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "Canvas Aguardando não foi encontrado."
            );
        }

        if (materia != null)
        {
            materia.text =
                "Aguardando outro jogador...";
        }
    }

    private async void Start()
    {
        websocket =
            new WebSocket(
                "ws://zeleystudios.online:3000"
            );

        websocket.OnOpen += QuandoConectar;

        websocket.OnError += (erro) =>
        {
            Debug.LogError(
                "Erro no WebSocket: " + erro
            );
        };

        websocket.OnClose += (codigo) =>
        {
            Debug.LogWarning(
                "WebSocket fechado. Código: " +
                codigo
            );
        };

        websocket.OnMessage +=
            QuandoReceberMensagem;

        try
        {
            await websocket.Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Não foi possível conectar ao WebSocket: " +
                ex.Message
            );
        }
    }

    private void QuandoConectar()
    {
        Debug.Log(
            "Conectado ao servidor WebSocket!"
        );

        StartCoroutine(
            AtualizarReferenciasDepoisDaCenaCarregar()
        );
    }

    private void QuandoReceberMensagem(byte[] bytes)
    {
        string mensagem =
            Encoding.UTF8.GetString(bytes);

        Debug.Log(
            "Mensagem recebida: " + mensagem
        );

        ProcessarMensagem(mensagem);
    }

    private void ProcessarMensagem(string mensagem)
    {
        if (string.IsNullOrWhiteSpace(mensagem))
        {
            Debug.LogWarning(
                "Mensagem WebSocket vazia."
            );

            return;
        }

        MensagemStatus status =
            JsonUtility.FromJson<MensagemStatus>(
                mensagem
            );

        if (
            status != null &&
            status.tipo == "status"
        )
        {
            if (
                !string.IsNullOrEmpty(status.mensagem) &&
                status.mensagem.StartsWith(
                    "Você escolheu"
                )
            )
            {
                MostrarTelaAguardando();

                if (materia != null)
                {
                    materia.text =
                        "Você escolheu " +
                        materiaEscolhida +
                        ". Aguardando outro jogador...";
                }

                return;
            }

            if (
                !string.IsNullOrEmpty(status.mensagem) &&
                status.mensagem.Contains(
                    "Nenhum adversário encontrado"
                )
            )
            {
                if (
                    !string.IsNullOrEmpty(
                        status.partidaId
                    )
                )
                {
                    partidaId = status.partidaId;
                }

                MostrarTelaAguardando();

                Debug.Log(
                    "Nenhum adversário encontrado. " +
                    "Partida individual iniciada: " +
                    partidaId
                );

                TMP_Text aviso = null;

                if (canvasAguardando != null)
                {
                    aviso =
                        canvasAguardando
                        .GetComponentInChildren<TMP_Text>(
                            true
                        );
                }

                if (aviso != null)
                {
                    aviso.text =
                        "Nenhum adversário encontrado.\n" +
                        "Partida individual iniciada!";

                    StartCoroutine(
                        FadeMensagem(aviso)
                    );
                }

                StartCoroutine(
                    IniciarPartidaSingle()
                );

                return;
            }

            if (
                !string.IsNullOrEmpty(status.mensagem) &&
                status.mensagem.StartsWith(
                    "Sessão reiniciada"
                )
            )
            {
                partidaId = string.Empty;

                Debug.Log(
                    "Sessão reiniciada pelo servidor."
                );

                return;
            }
        }

        MensagemParFormado partida =
            JsonUtility.FromJson<MensagemParFormado>(
                mensagem
            );

        if (
            partida != null &&
            partida.tipo == "parFormado"
        )
        {
            partidaId = partida.partidaId;
            materiaEscolhida = partida.materia;

            Debug.Log(
                "Partida multiplayer formada: " +
                partidaId
            );

            SceneManager.LoadScene("Perguntas");
            return;
        }

        PerguntaData pergunta =
            JsonUtility.FromJson<PerguntaData>(
                mensagem
            );

        if (
            pergunta != null &&
            pergunta.tipo == "itens"
        )
        {
            if (pergunta.partidaId != partidaId)
            {
                Debug.LogWarning(
                    "Pergunta recebida para outra partida."
                );

                return;
            }

            BancoDados banco =
                FindFirstObjectByType<BancoDados>();

            if (banco == null)
            {
                Debug.LogError(
                    "BancoDados não foi encontrado " +
                    "na cena Perguntas."
                );

                return;
            }

            banco.OnNovaPergunta(pergunta);
            return;
        }

        MensagemResultadoResposta resultado =
            JsonUtility.FromJson
            <MensagemResultadoResposta>(
                mensagem
            );

        if (
            resultado != null &&
            resultado.tipo == "resultadoResposta"
        )
        {
            if (resultado.partidaId != partidaId)
            {
                return;
            }

            BancoDados banco =
                FindFirstObjectByType<BancoDados>();

            if (banco != null)
            {
                banco.ProcessarResultadoServidor(
                    resultado
                );
            }

            return;
        }

        MensagemPontuacaoOponente pontuacao =
            JsonUtility.FromJson
            <MensagemPontuacaoOponente>(
                mensagem
            );

        if (
            pontuacao != null &&
            pontuacao.tipo == "pontuacaoOponente"
        )
        {
            if (pontuacao.partidaId != partidaId)
            {
                return;
            }

            Score.pontuacaoOponente =
                pontuacao.pontos;

            FindFirstObjectByType<Score>()?
                .IncrementarBarraOponente(
                    pontuacao.pontos
                );

            Debug.Log(
                "Pontuação do oponente atualizada: " +
                pontuacao.pontos
            );

            return;
        }

        MensagemFim fim =
            JsonUtility.FromJson<MensagemFim>(
                mensagem
            );

        if (
            fim != null &&
            fim.tipo == "fim"
        )
        {
            if (
                !string.IsNullOrEmpty(fim.partidaId) &&
                fim.partidaId != partidaId
            )
            {
                return;
            }

            Score.pontuacaoTotal =
                fim.pontuacaoJogador;

            Score.pontuacaoOponente =
                fim.pontuacaoOponente;

            Debug.Log(
                "Resultado final recebido. " +
                "Jogador: " +
                Score.pontuacaoTotal +
                " | Oponente: " +
                Score.pontuacaoOponente
            );

            StartCoroutine(
                AguardarFim()
            );

            return;
        }

        Debug.LogWarning(
            "Mensagem não reconhecida pelo cliente: " +
            mensagem
        );
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

    private IEnumerator FadeMensagem(
        TMP_Text texto
    )
    {
        if (texto == null)
        {
            yield break;
        }

        texto.alpha = 0f;

        for (
            float t = 0f;
            t < 1f;
            t += Time.deltaTime
        )
        {
            texto.alpha = t;
            yield return null;
        }

        texto.alpha = 1f;

        yield return new WaitForSeconds(2f);

        for (
            float t = 1f;
            t > 0f;
            t -= Time.deltaTime
        )
        {
            texto.alpha = t;
            yield return null;
        }

        texto.alpha = 0f;
    }

    public async void EnviarMateria(
        string materiaSelecionada
    )
    {
        if (!WebSocketEstaAberto())
        {
            Debug.LogWarning(
                "WebSocket não está conectado. " +
                "Matéria não enviada."
            );

            return;
        }

        if (
            string.IsNullOrWhiteSpace(
                materiaSelecionada
            )
        )
        {
            Debug.LogWarning(
                "Matéria inválida."
            );

            return;
        }

        materiaEscolhida =
            materiaSelecionada.Trim().ToLower();

        MensagemMateria mensagem =
            new MensagemMateria
            {
                materia = materiaEscolhida
            };

        string json =
            JsonUtility.ToJson(mensagem);

        try
        {
            await websocket.SendText(json);

            Debug.Log(
                "Matéria enviada: " + json
            );
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Erro ao enviar matéria: " +
                ex.Message
            );
        }
    }

    public void OnMateriaSelecionada(
        string materiaSelecionada
    )
    {
        string botaoClicado =
            EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null
                ? EventSystem.current
                    .currentSelectedGameObject.name
                : "";

        string materiaDoBotao =
            botaoClicado switch
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

        // Mostra a tela de espera imediatamente,
        // antes mesmo da resposta do servidor.
        MostrarTelaAguardando();

        EnviarMateria(materiaDoBotao);
    }

    public async void EnviarResposta(
        string resposta
    )
    {
        if (!WebSocketEstaAberto())
        {
            Debug.LogWarning(
                "WebSocket não está conectado. " +
                "Resposta não enviada."
            );

            return;
        }

        MensagemResposta mensagem =
            new MensagemResposta
            {
                tipo = "resposta",
                partidaId = partidaId,
                resposta =
                    string.IsNullOrWhiteSpace(resposta)
                        ? string.Empty
                        : resposta.Trim().ToUpper()
            };

        string json =
            JsonUtility.ToJson(mensagem);

        try
        {
            await websocket.SendText(json);

            Debug.Log(
                "Resposta enviada ao servidor: " +
                json
            );
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "Erro ao enviar resposta: " +
                ex.Message
            );
        }
    }

    private bool WebSocketEstaAberto()
    {
        return websocket != null &&
               websocket.State == WebSocketState.Open;
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
            Debug.LogWarning(
                "Erro ao fechar WebSocket: " +
                ex.Message
            );
        }
    }
}