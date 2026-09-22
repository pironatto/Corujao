using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    private float currentTime;
    private bool barraCompleta;
    private bool carregandoPerguntas;

    [Header("Contagem")]
    public float tempoInicial = 5f;
    public Slider BarraProgresso;
    public Text Tempo;

    [Header("Áudio")]
    public AudioSource fxSource;
    public AudioClip fxCronometro;

    [Header("Elementos visuais")]
    public Slider SliderA;
    public Slider SliderB;

    private void Start()
    {
        currentTime = tempoInicial;
        barraCompleta = false;
        carregandoPerguntas = false;

        if (BarraProgresso != null)
        {
            BarraProgresso.minValue = 0f;
            BarraProgresso.maxValue = tempoInicial;
            BarraProgresso.value = tempoInicial;
        }

        if (Tempo != null)
        {
            Tempo.text =
                Mathf.CeilToInt(currentTime).ToString();
        }

        InvokeRepeating(
            nameof(AudioCronometro),
            0.5f,
            1f
        );
    }

    private void Update()
    {
        BarraLoad();

        if (
            !barraCompleta &&
            currentTime <= 0f
        )
        {
            barraCompleta = true;
            CancelInvoke(nameof(AudioCronometro));
            CarregarCenaPerguntas();
        }
    }

    private void BarraLoad()
    {
        if (barraCompleta)
        {
            return;
        }

        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(0f, currentTime);

        if (BarraProgresso != null)
        {
            BarraProgresso.value = currentTime;
        }

        if (Tempo != null)
        {
            Tempo.text =
                Mathf.CeilToInt(currentTime).ToString();
        }
    }

    private void AudioCronometro()
    {
        if (
            fxSource != null &&
            fxCronometro != null &&
            !barraCompleta
        )
        {
            fxSource.PlayOneShot(fxCronometro);
        }
    }

    private void CarregarCenaPerguntas()
    {
        if (carregandoPerguntas)
        {
            return;
        }

        carregandoPerguntas = true;

        SceneManager.LoadScene("Perguntas");
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(AudioCronometro));
    }
}