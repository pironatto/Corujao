using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class controlePontuacao: MonoBehaviour
{

   
    [HideInInspector]
    public int pontuacao;   
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    pontuacao = 50 - (int)controleTempo.contagemSlider;

    }



}
