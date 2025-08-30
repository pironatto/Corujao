using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Fade : MonoBehaviour
{

    private float currentTime;
    private bool BarraCompleta;
    public Slider BarraProgresso;
    public Text Tempo;
    public AudioSource fxSource;
    public AudioClip fxCronometro;
    public Slider SliderA, SliderB;


    // Start is called before the first frame update
    void Start()
    {

        SliderA.value = controleTempo.ValorSliderA;
        SliderB.value = controleTempo.ValorSliderA;

        //ACIONAR BARRA DE TEMPO
        currentTime = 5;
        BarraCompleta = false;
        InvokeRepeating("AudioCronometro", 0.5f, 1f);
        print(controleTempo.numPerguntas++); // PARA VERIFICAR QUANTAS PERGUNTAS FORAM FEITAS

    }

    // Update is called once per frame
    void Update()
    {
        BarraLoad();

        if (BarraCompleta == false && BarraProgresso.value == 0)
        {
            SceneManager.LoadScene(2);
        }

    }

    public void BarraLoad()
    {
        currentTime -= Time.deltaTime;
        BarraProgresso.value = currentTime;
        int contadorTempo = Convert.ToInt32(BarraProgresso.value);
        Tempo.text = contadorTempo.ToString();



    }

    private void AudioCronometro()
    {
        fxSource.PlayOneShot(fxCronometro);
    }
    
}
