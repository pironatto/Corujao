using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pontuacao : MonoBehaviour
{
    public Text pontosA, pontosB;

    // Start is called before the first frame update
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {

        pontosA.text = controleTempo.ValorSliderA + " Pontos";
        pontosB.text = controleTempo.ValorSliderA + " Pontos";


    }

    public void CenaTema()
    {
        SceneManager.LoadScene(1);

    }
}
