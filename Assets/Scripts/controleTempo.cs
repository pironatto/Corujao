using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class controleTempo : MonoBehaviour
{

    private BancoDados _bancoDados;
    private float currentTime, currentTimeSlider;
    public Slider BarraProgresso, SliderA, SliderB;
    public TextMeshProUGUI Tempo;
    public static int ValorSliderA;
    [HideInInspector]
    public static int numPerguntas;

    public static int valorSlider1, valorSlider2, valorSlider3, valorSlider4, valorSlider5;
    private bool valorSliderCapturado = false;

    [HideInInspector]
    public bool tempoEsgotado = false;


    // Start is called before the first frame update
    void Start()
    {
        _bancoDados = FindFirstObjectByType(typeof(BancoDados)) as BancoDados;

        //ACIONAR BARRA DE TEMPO
        currentTime = 10.5f;
        SliderA.value = valorSlider1 + valorSlider2 + valorSlider3 + valorSlider4 + valorSlider5;
        SliderB.value = valorSlider1 + valorSlider2 + valorSlider3 + valorSlider4 + valorSlider5; // SUBSTITUIR NO MULTIPLAYER       
    }

    // Update is called once per frame
    void Update()
    {

        //PARA HABILITAR O TEMPO SOMENTE APOS AS RESPOSTAS APARECEREM
        if (_bancoDados.HabilitaRespostas == true)
        {
            BarraLoad(); //MEDIDOR DE TEMPO
        }


        if (BarraProgresso.value == 0 || _bancoDados.clickBotao == true)
        {

            //PEGAR VALOR DO SLIDER
            if (!valorSliderCapturado && _bancoDados.Acertou == true)
            {
                if (valorSlider1 == 0)
                {
                    valorSlider1 = (int)BarraProgresso.value;

                }
                else if (valorSlider1 != 0 && valorSlider2 == 0)
                {
                    valorSlider2 = (int)BarraProgresso.value;
                }
                else if (valorSlider1 != 0 && valorSlider2 != 0 && valorSlider3 == 0)
                {
                    valorSlider3 = (int)BarraProgresso.value;
                }

                else if (valorSlider1 != 0 && valorSlider2 != 0 && valorSlider3 != 0 && valorSlider4 == 0)
                {
                    valorSlider4 = (int)BarraProgresso.value;
                }

                else if (valorSlider1 != 0 && valorSlider2 != 0 && valorSlider3 != 0 && valorSlider4 != 0 && valorSlider5 == 0)
                {
                    valorSlider5 = (int)BarraProgresso.value;
                }

                valorSliderCapturado = true;
            }

            tempoEsgotado = true;
            _bancoDados.HabilitaRespostas = false;
            SliderA.value = valorSlider1 + valorSlider2 + valorSlider3 + valorSlider4 + valorSlider5;
            SliderB.value = SliderA.value; // SUBSTITUIR QUANDO TIVER MULTIPLAYER
            ValorSliderA = (int)SliderA.value; //PARA PASSAR O VALOR DO SLIDER A PONTUACAO E CENA FADE
            StartCoroutine("ChamarFade");
        }

    }

    public void BarraLoad()//MEDIDOR DE TEMPO
    {
        currentTime -= Time.deltaTime;
        BarraProgresso.value = currentTime;
        Tempo.text = Mathf.RoundToInt(BarraProgresso.value).ToString();

    }


    IEnumerator ChamarFade()
    {
        yield return new WaitForSeconds(2f);

        if (numPerguntas == 5)
        {
           
            SceneManager.LoadScene(6);
             numPerguntas = 0;
        }   
        else
        {
            SceneManager.LoadScene(3);
        }


    }

}

