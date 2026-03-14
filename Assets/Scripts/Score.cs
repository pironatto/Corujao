using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Score : MonoBehaviour
{
    private BancoDados _bancoDados;
    private float currentTime;

    public Slider SliderA;

    [HideInInspector]
    public static float pontuacaoTotal = 0f;

    [HideInInspector]
    public static float[] pontosPorPergunta = new float[5];

    // Campos para a cena de score
    public Text textoPontuacaoTotal;
    public Text textoDetalhes;

    void Start()
    {
        _bancoDados = FindFirstObjectByType(typeof(BancoDados)) as BancoDados;

        currentTime = 0;
        if (SliderA != null)
            SliderA.maxValue = 50;

        // 🚀 Se estamos na cena de score, mostrar resultados
        if (SceneManager.GetActiveScene().buildIndex == 6) // cena de score
        {
            MostrarResultados();
        }
    }

    void Update()
    {
        if (_bancoDados != null && _bancoDados.contandoTempo)
        {
            currentTime += Time.deltaTime;
            if (SliderA != null)
                SliderA.value = currentTime;
        }
        else
        {
            if (SliderA != null)
                SliderA.value = currentTime;
        }

    }

    private void MostrarResultados()
    {
        if (textoPontuacaoTotal != null)
        {
            textoPontuacaoTotal.text = "Pontuação Total: " + pontuacaoTotal.ToString("F1");
        }

        if (textoDetalhes != null)
        {
            string detalhes = "";
            for (int i = 0; i < pontosPorPergunta.Length; i++)
            {
                detalhes += $"Pergunta {i + 1} : {pontosPorPergunta[i]:F1}\n";
            }
            textoDetalhes.text = detalhes;
        }
    }


    public void EscolherTema()
    {
        SceneManager.LoadScene(1);
    }


}