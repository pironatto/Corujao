using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pontuacao : MonoBehaviour
{
    public Text pontosA, pontosB;
    private controlePontuacao _controlePontuacao ;

    // Start is called before the first frame update
    void Start()
    {
        _controlePontuacao = FindFirstObjectByType(typeof(controlePontuacao)) as controlePontuacao;
    }

    // Update is called once per frame
    void Update()
    {

        pontosA.text = _controlePontuacao.pontuacao.ToString()+ " Pontos";
        pontosB.text = _controlePontuacao.pontuacao.ToString()+ " Pontos";
        

    }

    public void CenaTema(){
        SceneManager.LoadScene(3);
    }
}
