using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class BancoDados : MonoBehaviour
{

    private string[] itens;     //ARMAZENAR OS ITENS
    public TextMeshProUGUI r1, r2, r3, r4, pergunta;
    public Button R1, R2, R3, R4;
    private controleTempo _controleTempo;
    private string botaoClicado, resposta, respostaCorreta;
    private string materia;

    [HideInInspector]
    public bool clickBotao, HabilitaRespostas;
    public GameObject PainelRespostas;
    public bool Acertou = false;



    // Start is called before the first frame update
    void Start()
    {
        _controleTempo = FindFirstObjectByType(typeof(controleTempo)) as controleTempo;
        StartCoroutine("VerificaPergunta");
        //_controleTempo.enabled = false;
    }

    IEnumerator VerificaPergunta()
    {
        //PARA ESCOLHER O TEMA
        materia = ControleTemas.materia;
        WWWForm form = new WWWForm();
        form.AddField("materia", materia);

        //ESPERAR ATÃ‰ QUE O BANCO DE DADOS SEJA LIDO
         using (UnityWebRequest itemdata = UnityWebRequest.Post("https://zeleystudios.servegame.com/corujao//perguntas.php", form))            
        //using (UnityWebRequest itemdata = UnityWebRequest.Post("http://localhost/corujao//perguntas.php", form))

        {
            yield return itemdata.SendWebRequest();
            string itemDataString = itemdata.downloadHandler.text;


            itens = itemDataString.Split('-');

            pergunta.text = itens[2];

            yield return new WaitForSeconds(2f); //LAG ENTRE A PERGUNTA E AS RESPOSTAS
            PainelRespostas.SetActive(true);
            HabilitaRespostas = true;
            //_controleTempo.enabled = true;

            r1.text = itens[3];
            r2.text = itens[4];
            r3.text = itens[5];
            r4.text = itens[6];


        }

    }

    // Update is called once per frame
    void Update()
    {

        verificaResposta();
    }


    void verificaResposta()
    {

        if (_controleTempo.tempoEsgotado == true || clickBotao == true)
        {

            R1.interactable = false;
            R2.interactable = false;
            R3.interactable = false;
            R4.interactable = false;

            switch (itens[7])
            {
                case "A":
                    R1.image.color = Color.green;
                    break;
                case "B":
                    R2.image.color = Color.green;
                    break;
                case "C":
                    R3.image.color = Color.green;
                    break;
                case "D":
                    R4.image.color = Color.green;
                    break;
            }

        }

    }

    public void BotaoResposta()
    {
        //VER QUAL BOTAO FOI CLICADO   
        botaoClicado = EventSystem.current.currentSelectedGameObject.name;
        respostaCorreta = itens[7];

        switch (botaoClicado)
        {
            case "R1":
                resposta = "A";
                R1.image.color = new Color(0.49f,0.84f,0.91f,1f);
                break;
            case "R2":
                resposta = "B";
                R2.image.color = new Color(0.49f,0.84f,0.91f,1f);
                break;
            case "R3":
                resposta = "C";
                R3.image.color =new Color(0.49f,0.84f,0.91f,1f);
                break;
            case "R4":
                resposta = "D";
                R4.image.color = new Color(0.49f,0.84f,0.91f,1f);
                break;
        }
        //AVISAR QUE O BOTAO FOI CLICADO
        clickBotao = true;

        if (resposta == respostaCorreta)
        {
            Acertou = true;

        }
        else
        {
            Acertou = false;
        }

    }




}

