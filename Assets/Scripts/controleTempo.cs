using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class controleTempo : MonoBehaviour
{
    private BancoDados _bancoDados;
    public Slider BarraProgresso, SliderA, SliderB;
    public TextMeshProUGUI Tempo;

    [HideInInspector] public static int numPerguntas;

    // Valores acumulados de cada pergunta
    public static int valorSlider1, valorSlider2, valorSlider3, valorSlider4, valorSlider5;

    private bool respondeu = false;
    private int pontosCongelados=0;

    void Start()
    {
        _bancoDados = FindFirstObjectByType(typeof(BancoDados)) as BancoDados;

        // Definir máximo das barras laterais (5 perguntas x 10 pontos cada = 50)
        SliderA.maxValue = 50;
        SliderB.maxValue = 50;

        AtualizarBarrasLaterais();
    }

    void Update()
    {
        // Enquanto respostas estão habilitadas e jogador não respondeu, barras sobem em tempo real
        if (_bancoDados.HabilitaRespostas && !respondeu)
        {
            AtualizarBarrasLateraisTempo();
        }

        else
        {
            // Depois da resposta, mostra acumulado + pontosCongelados
            int acumulado = valorSlider1 + valorSlider2 + valorSlider3 + valorSlider4 + valorSlider5;
            SliderA.value = acumulado;
            SliderB.value = acumulado;
        }

    }

    public static void CapturarValor(int valor)
    {
        if (valorSlider1 == 0) valorSlider1 = valor;
        else if (valorSlider2 == 0) valorSlider2 = valor;
        else if (valorSlider3 == 0) valorSlider3 = valor;
        else if (valorSlider4 == 0) valorSlider4 = valor;
        else if (valorSlider5 == 0) valorSlider5 = valor;
    }

    private void AtualizarBarrasLaterais()
    {
        int acumulado = valorSlider1 + valorSlider2 + valorSlider3 + valorSlider4 + valorSlider5;
        SliderA.value = acumulado;
        SliderB.value = acumulado;
    }

    private void AtualizarBarrasLateraisTempo()
    {
        // Pontos da pergunta atual = tempo decorrido (usando a barra do BancoDados)
        float pontosAtuais = BarraProgresso.maxValue - BarraProgresso.value;

        // Soma acumulada + pontos atuais
        int acumulado = valorSlider1 + valorSlider2 + valorSlider3 + valorSlider4 + valorSlider5;
        SliderA.value = acumulado + pontosAtuais;
        SliderB.value = acumulado + pontosAtuais;
    }

    // Chamado pelo BancoDados quando jogador responde
    public void RegistrarResposta()
    {
        // Captura pontos atuais
        pontosCongelados = (int)(BarraProgresso.maxValue - BarraProgresso.value);
        CapturarValor(pontosCongelados);

        respondeu = true;
        _bancoDados.HabilitaRespostas = false;

        // Congela barra lateral no valor capturado
        AtualizarBarrasLaterais();


    }

    private void ProximaPergunta()
    {
        numPerguntas++;

        if (numPerguntas == 5)
        {
            SceneManager.LoadScene(6); // cena final
            numPerguntas = 0;
        }
        else
        {
            respondeu = false;
            _bancoDados.HabilitaRespostas = true;
            AtualizarBarrasLaterais();
        }
    }
}
