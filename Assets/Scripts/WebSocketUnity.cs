using UnityEngine;
using NativeWebSocket;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

[System.Serializable]
public class MensagemStatus
{
    public string tipo;
    public string mensagem;
    public string partidaId;
}

[System.Serializable]
public class MensagemParFormado
{
    public string tipo;
    public string partidaId;
    public string materia;
}

[System.Serializable]
public class MensagemSingle
{
    public string tipo;
    public string mensagem;
    public string partidaId;
}

[System.Serializable]
public class PerguntaData
{
    public string tipo;
    public string partidaId;
    public string[] itens;
    public float tempoTotal;
    public long inicio;
}

[System.Serializable]
public class MensagemPontuacaoOponente
{
    public string tipo;
    public string partidaId;
    public string jogadorId;
    public float pontos;
}

public class WebSocketUnity : MonoBehaviour
{
    public GameObject canvasPrincipal;
    public GameObject canvasAguardando;
    private WebSocket websocket;
    public static WebSocketUnity Instance { get; private set; }
    public TMP_Text materia;

    [HideInInspector] public string materiaEscolhida;
    [HideInInspector] public string partidaId;

    // 🔹 Expor WebSocket publicamente sem reflexão
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
        // Sempre que uma cena nova carregar, atualiza referências
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Procura todos os GameObjects, inclusive inativos
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
        //websocket = new WebSocket("ws://localhost:3000");
        websocket = new WebSocket("ws://zeleystudios.servegame.com:3000");


        websocket.OnOpen += () =>
        {
            Debug.Log("Conectado ao servidor WebSocket!");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("Mensagem recebida: " + message);

            // 1. Status genérico
            MensagemStatus statusMsg = JsonUtility.FromJson<MensagemStatus>(message);
            if (statusMsg != null && statusMsg.tipo == "status")
            {
                if (statusMsg.mensagem.StartsWith("Você escolheu"))
                {
                    canvasPrincipal?.SetActive(false);
                    canvasAguardando?.SetActive(true);
                    materia.text = "Você escolheu " + materiaEscolhida + ". Aguardando outro jogador...";
                    return;
                }
            }

            // 2. Par formado (multiplayer)
            MensagemParFormado parMsg = JsonUtility.FromJson<MensagemParFormado>(message);
            if (parMsg != null && parMsg.tipo == "parFormado")
            {
                partidaId = parMsg.partidaId;
                materiaEscolhida = parMsg.materia;
                Debug.Log("PartidaId recebido (par): " + partidaId);
                SceneManager.LoadScene("Perguntas");
                return;
            }

            // 3. Partida single
            MensagemSingle singleMsg = JsonUtility.FromJson<MensagemSingle>(message);
            if (singleMsg != null && singleMsg.tipo == "status"
                && singleMsg.mensagem.StartsWith("Nenhum adversário encontrado"))
            {
                partidaId = singleMsg.partidaId;
                Debug.Log("PartidaId recebido (single): " + partidaId);

                TMP_Text aviso = canvasAguardando?.GetComponentInChildren<TMP_Text>();
                if (aviso != null)
                {
                    aviso.text = "Nenhum adversário encontrado.\nPartida individual iniciada!";
                    Instance.StartCoroutine(FadeMensagem(aviso));
                }
                Instance.StartCoroutine(IniciarPartidaSingle());
                return;
            }

            // 4. Pergunta
            PerguntaData perguntaData = JsonUtility.FromJson<PerguntaData>(message);
            if (perguntaData != null && perguntaData.tipo == "itens")
            {
                if (perguntaData.partidaId == partidaId)
                {
                    BancoDados banco = FindFirstObjectByType<BancoDados>();
                    banco?.OnNovaPergunta(perguntaData);
                }
                return;
            }

            // 5. Fim
            if (message.Contains("\"tipo\":\"fim\""))
            {
                Debug.Log("Fim das perguntas recebido!");
                Instance.StartCoroutine(AguardarFim());
                return;
            }

            // 6. Pontuação do oponente
            MensagemPontuacaoOponente pontuacaoMsg = JsonUtility.FromJson<MensagemPontuacaoOponente>(message);
            if (pontuacaoMsg != null && pontuacaoMsg.tipo == "pontuacaoOponente")
            {
                if (pontuacaoMsg.partidaId == partidaId)
                {
                    Score.pontuacaoOponente = pontuacaoMsg.pontos;
                    FindFirstObjectByType<Score>()?.IncrementarBarraOponente(pontuacaoMsg.pontos);
                    Debug.Log("Pontuação do oponente atualizada: " + Score.pontuacaoOponente);
                }
                return;
            }
        };

        await websocket.Connect();
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
        for (float t = 0; t < 1f; t += Time.deltaTime)
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

    public async void EnviarMateria(string materia)
    {
        if (websocket.State == WebSocketState.Open)
        {
            string json = "{\"materia\":\"" + materia + "\"}";
            await websocket.SendText(json);
            materiaEscolhida = materia;
            Debug.Log("Matéria enviada: " + materia);
        }
    }

    public void OnMateriaSelecionada(string materia)
    {
        string botaoClicado = EventSystem.current.currentSelectedGameObject.name;
        if (botaoClicado == "BtHistoria") materiaEscolhida = "historia";
        if (botaoClicado == "BtCiencias") materiaEscolhida = "ciencias";
        if (botaoClicado == "BtMatematica") materiaEscolhida = "matematica";
        if (botaoClicado == "BtFisica") materiaEscolhida = "fisica";
        if (botaoClicado == "BtHarryPotter") materiaEscolhida = "harrypotter";
        if (botaoClicado == "BtGeografia") materiaEscolhida = "geografia";
        if (botaoClicado == "BtBiologia") materiaEscolhida = "biologia";
        if (botaoClicado == "BtMedicina") materiaEscolhida = "medicina";
        EnviarMateria(materiaEscolhida);
    }

    // 🔹 Novo método para enviar pontuação ao servidor
    public async void EnviarPontuacao(string jogadorId, float pontos)
    {
        if (websocket.State == WebSocketState.Open)
        {
            string json = JsonUtility.ToJson(new MensagemPontuacaoOponente
            {
                tipo = "pontuacao",
                partidaId = partidaId,
                jogadorId = jogadorId,
                pontos = pontos
            });
            await websocket.SendText(json);
            Debug.Log("Pontuação enviada ao servidor: " + json);
        }
    }

    private void Update()
    {
        websocket.DispatchMessageQueue();
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}
