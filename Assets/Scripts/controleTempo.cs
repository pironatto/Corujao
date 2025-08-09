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
    [HideInInspector]
    public bool tempoEsgotado = false;

    // Start is called before the first frame update
    void Start()
    {
        _bancoDados = FindFirstObjectByType(typeof(BancoDados)) as BancoDados;

        //ACIONAR BARRA DE TEMPO
        currentTime = 10;
        currentTimeSlider = 0;
    }

    // Update is called once per frame
    void Update()
    {
        BarraLoad();

        if (tempoEsgotado == false)
        {
            SliderLoad();
        }


        if (BarraProgresso.value == 0 || _bancoDados.clickBotao == true)
        {
            tempoEsgotado = true;
            StartCoroutine("ChamarFade");

        }


    }

    public void BarraLoad()
    {
        currentTime -= Time.deltaTime;
        BarraProgresso.value = currentTime;
        Tempo.text = Mathf.RoundToInt(BarraProgresso.value).ToString();

    }


    public void SliderLoad()
    {
    
        currentTimeSlider += Time.deltaTime;
        SliderA.value = currentTimeSlider;
        SliderB.value = currentTimeSlider;

    }

    IEnumerator ChamarFade()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(3);


    }

}
