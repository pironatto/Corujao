using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;



public class BancoDados : MonoBehaviour
{

    private string[] itens;     //ARMAZENAR OS ITENS
    public Text pergunta, r1, r2, r3, r4;


    // Start is called before the first frame update
    IEnumerator Start()
    {

        //ESPERAR ATÉ QUE O BANCO DE DADOS SEJA LIDO
        using (UnityWebRequest itemdata = UnityWebRequest.Get("https://zeleystudios.servegame.com/corujao/perguntas.php"))
        {
            yield return itemdata.SendWebRequest();
            string itemDataString = itemdata.downloadHandler.text;

            itens = itemDataString.Split('-');

            pergunta.text = itens[2];
            r1.text = itens[3];
            r2.text = itens[4];
            r3.text = itens[5];
            r4.text = itens[6];


        }


    }

    // Update is called once per frame
    void Update()
    {

    }






}
