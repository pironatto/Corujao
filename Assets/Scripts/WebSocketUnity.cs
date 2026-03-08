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
        // Ajuste conforme necessário (localhost ou servidor remoto)
        websocket = new WebSocket("ws://localhost:3000");

        websocket.OnOpen += () =>
        {
            Debug.Log("Conectado ao servidor WebSocket!");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("Mensagem recebida: " + message);

            // Primeiro tenta interpretar como status
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
            }
            else
            {
                // Se não for status, tenta interpretar como pergunta ou fim
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
                    // 🚀 Novo: interpretar mensagem de fim
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

    // Corrotina para delay antes da cena final
    private IEnumerator AguardarFim()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(6); // cena de score
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
