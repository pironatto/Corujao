using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using NativeWebSocket;

public class GameSessionController : MonoBehaviour
{
    public GameObject canvasPrincipal;
    public GameObject canvasAguardando;
    public TMP_Text materia;

    private bool escolhaTemaEmAndamento;
    private bool carregandoCenaPerguntas;
    private bool preparandoPartida;
    private bool tratandoDesconexaoAdversario;

    public static GameSessionController Instance { get; private set; }

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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AtualizarReferenciasDaCena()
    {
        canvasPrincipal = null;
        canvasAguardando = null;
        materia = null;

        Scene cenaAtual = SceneManager.GetActiveScene();

        if (!cenaAtual.IsValid() || !cenaAtual.isLoaded)
        {
            Debug.LogWarning("A cena atual ainda não está disponível.");
            return;
        }


        // ADICIONE AQUI:
        if (cenaAtual.name == "Temas")
        {
            ResetarFlagsAoCarregarCena();
        }

             GameObject[] objetosRaiz = cenaAtual.GetRootGameObjects();

        foreach (GameObject objetoRaiz in objetosRaiz)
        {
            Transform[] objetos = objetoRaiz.GetComponentsInChildren<Transform>(true);

            foreach (Transform transformacao in objetos)
            {
                GameObject objeto = transformacao.gameObject;

                if (canvasPrincipal == null && objeto.name == "Canvas Temas")
                {
                    canvasPrincipal = objeto;
                }

                if (canvasAguardando == null && objeto.name == "Canvas Aguardando")
                {
                    canvasAguardando = objeto;
                }

                if (materia == null && objeto.name == "materia")
                {
                    materia = objeto.GetComponent<TMP_Text>();
                }
            }
        }

        Debug.Log(
            "Referências encontradas: " +
            "Canvas Temas = " + (canvasPrincipal != null) +
            " | Canvas Aguardando = " + (canvasAguardando != null) +
            " | materia = " + (materia != null)
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
            Debug.LogWarning("Canvas Temas não foi encontrado.");
        }

        if (canvasAguardando != null)
        {
            canvasAguardando.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Canvas Aguardando não foi encontrado.");
        }

        if (materia != null)
        {
            string materiaExibicao = ObterNomeMateriaExibicao(WebSocketUnity.Instance.materiaEscolhida);
            materia.text = "Você escolheu  \"<color=yellow>" + materiaExibicao + "</color>\"\n\nAguardando outro jogador...";
        }
    }

    public void ProcessarStatus(MensagemStatus status)
    {
        if (!string.IsNullOrEmpty(status.mensagem) && status.mensagem.StartsWith("Você escolheu"))
        {
            MostrarTelaAguardando();

            if (materia != null)
            {
                string materiaExibicao = ObterNomeMateriaExibicao(WebSocketUnity.Instance.materiaEscolhida);
                materia.text = "Você escolheu  \"<color=yellow>" + materiaExibicao + "</color>\"\n\nPartida Individual iniciando...";
            }

            return;
        }

        if (!string.IsNullOrEmpty(status.mensagem) && status.mensagem.Contains("Nenhum adversário encontrado"))
        {
            Score.ResetarPontuacao();

            if (!string.IsNullOrEmpty(status.partidaId))
            {
                WebSocketUnity.Instance.partidaId = status.partidaId;
            }

            MostrarTelaAguardando();

            if (materia != null)
            {
                materia.text = "Matéria: " + WebSocketUnity.Instance.materiaEscolhida.ToUpperInvariant() + "\nPartida individual iniciada!";
            }

            StartCoroutine(IniciarPartidaSingle());
            return;
        }

        if (!string.IsNullOrEmpty(status.mensagem) && status.mensagem.StartsWith("Sessão reiniciada"))
        {
            WebSocketUnity.Instance.partidaId = string.Empty;
            Debug.Log("Sessão reiniciada pelo servidor.");
            return;
        }

        if (status.mensagem == "Escolha a matéria...")
        {
            Debug.Log("Servidor aguardando escolha da matéria.");
            return;
        }
    }

    public void PrepararPartidaAntesDoFade()
    {
        StartCoroutine(PrepararPartidaCoroutine());
    }

    private IEnumerator PrepararPartidaCoroutine()
    {
        if (preparandoPartida)
        {
            yield break;
        }

        preparandoPartida = true;

        GarantirReferenciasDaCena();

        if (canvasPrincipal != null)
        {
            canvasPrincipal.SetActive(false);
        }

        if (canvasAguardando != null)
        {
            canvasAguardando.SetActive(true);
        }

        if (materia != null)
        {
            materia.text = "ADVERSÁRIO ENCONTRADO!\n\nPreparando partida...";
        }

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("Fade");
        preparandoPartida = false;
    }

    public void TratarDesconexaoDoAdversario()
    {
        StartCoroutine(TratarDesconexaoCoroutine());
    }

    private IEnumerator TratarDesconexaoCoroutine()
    {
        if (tratandoDesconexaoAdversario)
        {
            yield break;
        }

        tratandoDesconexaoAdversario = true;

        Debug.Log("O adversário desconectou da partida.");

        carregandoCenaPerguntas = false;
        preparandoPartida = false;
        escolhaTemaEmAndamento = false;

        WebSocketUnity.Instance.partidaId = string.Empty;
        WebSocketUnity.Instance.materiaEscolhida = string.Empty;

        if (SceneManager.GetActiveScene().name != "Temas")
        {
            SceneManager.LoadScene("Temas");

            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Temas");
            yield return null;
            yield return null;
            yield return new WaitForEndOfFrame();
        }

        AtualizarReferenciasDaCena();

        yield return null;
        AtualizarReferenciasDaCena();

        if (canvasPrincipal != null)
        {
            canvasPrincipal.SetActive(false);
        }

        if (canvasAguardando != null)
        {
            canvasAguardando.SetActive(true);
        }
        else
        {
            Debug.LogError("Canvas Aguardando não foi encontrado na cena Temas.");
        }

        if (materia != null)
        {
            materia.text = "O ADVERSÁRIO SAIU DA PARTIDA.\n\nA partida foi encerrada.";
        }
        else
        {
            Debug.LogError("Texto materia não foi encontrado na cena Temas.");
        }

        yield return new WaitForSeconds(4f);

        if (canvasAguardando != null)
        {
            canvasAguardando.SetActive(false);
        }

        if (canvasPrincipal != null)
        {
            canvasPrincipal.SetActive(true);
        }

        tratandoDesconexaoAdversario = false;
    }

    public void OnMateriaSelecionada(string materiaSelecionada)
    {
        if (escolhaTemaEmAndamento)
        {
            Debug.Log("Escolha de tema já está em andamento. Clique ignorado.");
            return;
        }

        if (WebSocketUnity.Instance == null || WebSocketUnity.Instance.Websocket.State != NativeWebSocket.WebSocketState.Open)
        {
            Debug.LogWarning("WebSocket não está conectado. A matéria não será enviada.");
            return;
        }

        escolhaTemaEmAndamento = true;

        MostrarTelaAguardando();

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
            "BtPersonagens" => "personagens",
            "BtFrasesFilmes" => "frasesfilmes",
            _ => materiaSelecionada
        };

        if (string.IsNullOrWhiteSpace(materiaDoBotao))
        {
            Debug.LogWarning("Não foi possível identificar a matéria.");
            escolhaTemaEmAndamento = false;
            return;
        }

        WebSocketUnity.Instance.EnviarMateria(materiaDoBotao);
    }

    public void AguardarFim()
    {
        StartCoroutine(AguardarFimCoroutine());
    }

    private IEnumerator AguardarFimCoroutine()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Pontuacao");
    }

    private IEnumerator IniciarPartidaSingle()
    {
        yield return new WaitForSeconds(3f);
        CarregarCenaPerguntasUmaVez();
    }

    private void CarregarCenaPerguntasUmaVez()
    {
        if (carregandoCenaPerguntas)
        {
            Debug.Log("A cena Perguntas já está sendo carregada. Solicitação ignorada.");
            return;
        }

        if (SceneManager.GetActiveScene().name == "Perguntas")
        {
            Debug.Log("A cena Perguntas já está ativa.");
            return;
        }

        carregandoCenaPerguntas = true;

        Debug.Log("Carregando a cena Perguntas...");

        SceneManager.LoadScene("Perguntas");
    }

    public void ResetarFlagsAoCarregarCena()
    {
        escolhaTemaEmAndamento = false;
        carregandoCenaPerguntas = false;
        preparandoPartida = false;
        tratandoDesconexaoAdversario = false;

        Debug.Log("Flags resetadas.");
    }


    private string ObterNomeMateriaExibicao(string materiaInterna)
    {
        if (string.IsNullOrWhiteSpace(materiaInterna))
        {
            return "";
        }

        return materiaInterna.Trim().ToLowerInvariant() switch
        {
            "historia" => "História",
            "ciencias" => "Ciências",
            "matematica" => "Matemática",
            "fisica" => "Física",
            "harrypotter" => "Harry Potter",
            "география" => "Geografia",
            "biologia" => "Biologia",
            "medicina" => "Medicina",
            "personagens" => "Personagens",
            "frasesfilmes" => "Frases de filmes",
            _ => materiaInterna
        };
    }
}