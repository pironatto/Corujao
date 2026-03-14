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
}

[System.Serializable]
public class PerguntaData
{
    public string tipo;
    public string[] itens;
    public float tempoTotal; // em segundos
    public long inicio;      // timestamp em ms
}

public class WebSocketUnity : MonoBehaviour
{
    public GameObject canvasPrincipal;
    public GameObject canvasAguardando;
    private WebSocket websocket;
    public static WebSocketUnity Instance { get; private set; }
    public TMP_Text materia;

    [HideInInspector]
    public string materiaEscolhida;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        //websocket = new WebSocket("ws://zeleystudios.servegame.com:3000");
        websocket = new WebSocket("ws://localhost:3000");

        websocket.OnOpen += () =>
        {
            Debug.Log("Conectado ao servidor WebSocket!");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("Mensagem recebida: " + message);

            MensagemStatus statusMsg = JsonUtility.FromJson<MensagemStatus>(message);
            if (statusMsg != null && statusMsg.tipo == "status")
            {
                if (statusMsg.mensagem.StartsWith("Você escolheu"))
                {
                    if (canvasPrincipal != null) canvasPrincipal.SetActive(false);
                    if (canvasAguardando != null) canvasAguardando.SetActive(true);
                    materia.text = "Você escolheu " + materiaEscolhida + ". Aguardando outro jogador...";
                }
                else if (statusMsg.mensagem.StartsWith("Par formado"))
                {
                    SceneManager.LoadScene("Perguntas");
                }
                else if (statusMsg.mensagem.StartsWith("Nenhum adversário encontrado"))
                {
                    Debug.Log("Partida individual detectada!");

                    if (canvasAguardando != null)
                    {
                        TMP_Text aviso = canvasAguardando.GetComponentInChildren<TMP_Text>();
                        if (aviso != null)
                        {
                            aviso.text = "Nenhum adversário encontrado.\nPartida individual iniciada!";
                            Instance.StartCoroutine(FadeMensagem(aviso));
                        }
                    }

                    Instance.StartCoroutine(IniciarPartidaSingle());
                }
            }
            else
            {
                PerguntaData perguntaData = JsonUtility.FromJson<PerguntaData>(message);
                if (perguntaData != null && perguntaData.tipo == "itens")
                {
                    BancoDados banco = FindFirstObjectByType<BancoDados>();
                    if (banco != null)
                    {
                        banco.OnNovaPergunta(perguntaData);
                    }
                }
                else
                {
                    if (message.Contains("\"tipo\":\"fim\""))
                    {
                        Debug.Log("Fim das perguntas recebido do servidor!");
                        Instance.StartCoroutine(AguardarFim());
                    }
                }
            }
        };

        await websocket.Connect();
    }

    private IEnumerator AguardarFim()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(6); // cena de score
    }

    private IEnumerator IniciarPartidaSingle()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Perguntas");
    }

    private IEnumerator FadeMensagem(TMP_Text texto)
    {
        // Fade-in
        for (float t = 0; t < 1f; t += Time.deltaTime)
        {
            texto.alpha = t;
            yield return null;
        }
        texto.alpha = 1f;

        // Mantém visível por 2 segundos
        yield return new WaitForSeconds(2f);

        // Fade-out
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
        if (botaoClicado == "BtHistoria") { materiaEscolhida = "historia"; }
        if (botaoClicado == "BtCiencias") { materiaEscolhida = "ciencias"; }
        if (botaoClicado == "BtMatematica") { materiaEscolhida = "matematica"; }
        if (botaoClicado == "BtFisica") { materiaEscolhida = "fisica"; }
        if (botaoClicado == "BtHarryPotter") { materiaEscolhida = "harrypotter"; }
        if (botaoClicado == "BtGeografia") { materiaEscolhida = "geografia"; }
        if (botaoClicado == "BtBiologia") { materiaEscolhida = "biologia"; }
        if (botaoClicado == "BtMedicina") { materiaEscolhida = "medicina"; }
        EnviarMateria(materiaEscolhida);
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