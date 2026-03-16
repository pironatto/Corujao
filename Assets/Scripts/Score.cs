using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Score : MonoBehaviour
{
    public Slider SliderA;
    public Slider SliderOponente;

    [HideInInspector] public static float pontuacaoTotal = 0f;
    [HideInInspector] public static float[] pontosPorPergunta = new float[5];
    [HideInInspector] public static float pontuacaoOponente = 0f;

    public Text textoPontuacaoTotal;
    public Text textoDetalhes;
    public Text textoPontuacaoOponente;

    void Start()
    {
        if (SliderA != null) SliderA.maxValue = 50;
        if (SliderOponente != null) SliderOponente.maxValue = 50;

        if (SceneManager.GetActiveScene().name == "Pontuacao")
            MostrarResultados();
    }

    void Update()
    {
        if (SliderOponente != null)
            SliderOponente.value = pontuacaoOponente;
    }

    public void IncrementarBarra(float valor)
    {
        if (SliderA != null)
            SliderA.value += valor;
    }

    public void IncrementarBarraOponente(float valor)
    {
        if (SliderOponente != null)
            SliderOponente.value = valor;
    }

    private void MostrarResultados()
    {
        if (textoPontuacaoTotal != null)
            textoPontuacaoTotal.text = "Sua Pontuação: " + pontuacaoTotal.ToString("F1");

        if (textoDetalhes != null)
        {
            string detalhes = "";
            for (int i = 0; i < pontosPorPergunta.Length; i++)
                detalhes += $"Pergunta {i + 1} : {pontosPorPergunta[i]:F1}\n";
            textoDetalhes.text = detalhes;
        }

        if (textoPontuacaoOponente != null)
            textoPontuacaoOponente.text = "Pontuação do Oponente: " + pontuacaoOponente.ToString("F1");
    }

    public void EscolherTema()
    {
        SceneManager.LoadScene(1);
    }
}
