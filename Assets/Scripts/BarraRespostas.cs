using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BarraRespostas : MonoBehaviour
{
    public Slider barraProgresso;
    public BancoDados bancoDados;
    public GameObject painelRespostas;

    private bool preenchendo = false;
    private int contadorPerguntas = 0;
    private int pontuacaoTotal = 0;

    void Update()
    {
        if (bancoDados != null && painelRespostas != null)
        {
            // Se o painel está ativo e não clicou em resposta, a barra sobe
            if (painelRespostas.activeSelf && !bancoDados.clickBotao)
            {
                preenchendo = true;
            }
            else
            {
                
                preenchendo = false;
            }

            // Continua preenchendo enquanto ativo e não congelada
            if (preenchendo)
            {
                barraProgresso.value += Time.deltaTime;

                if (barraProgresso.value >= barraProgresso.maxValue)
                {
                    preenchendo = false;
                }
            }

            // Quando o jogador clica em uma resposta, congela imediatamente
            if (bancoDados.clickBotao)
            {    // 🔹 congela no valor atual
                preenchendo = false;    // garante que pare de subir

                RegistrarPontuacao();
                bancoDados.clickBotao = false; // libera para próxima pergunta
            }
        }
    }

    private void RegistrarPontuacao()
    {
        int valorBarra = Mathf.RoundToInt(barraProgresso.value);
        int pontos = 10 - valorBarra;
        if (pontos < 0) pontos = 0;

        pontuacaoTotal += pontos;
        contadorPerguntas++;
    

        Debug.Log("Pergunta " + contadorPerguntas + " -> Pontos: " + pontos + " | Total: " + pontuacaoTotal);

        if (contadorPerguntas >= 5)
        {
            PlayerPrefs.SetInt("PontuacaoFinal", pontuacaoTotal);
            PlayerPrefs.Save();
            SceneManager.LoadScene(6);
        }
    }


}
