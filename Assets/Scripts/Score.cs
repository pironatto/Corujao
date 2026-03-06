using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Score : MonoBehaviour
{

    private BancoDados _bancoDados;
    private MostrarItens _mostrarItens;
    private float currentTime;
    public Slider SliderA, SliderB;

  

    [HideInInspector]
    public static int numPerguntas;
    [HideInInspector]
    public static float pontuacaoTotal = 0f;
    [HideInInspector]
    public static float[] pontosPorPergunta = new float[5];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _bancoDados = FindFirstObjectByType(typeof(BancoDados)) as BancoDados;
        _mostrarItens = FindFirstObjectByType(typeof(MostrarItens)) as MostrarItens;

        currentTime = 0;
        SliderA.maxValue = 50;

        pontuacaoTotal = 0f;
        numPerguntas = 0;
              
    }

    // Update is called once per frame
    void Update()
    {
        if (_bancoDados.contandoTempo == true)
        {
          
            currentTime += Time.deltaTime;
            SliderA.value = currentTime;
         
        }

        else
        {
            SliderA.value = currentTime;

        }


    }
}
