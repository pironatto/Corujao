using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;




public class BancoDados : MonoBehaviour
{

    private string[] itens;     //ARMAZENAR OS ITENS
    public TextMeshProUGUI r1, r2, r3, r4;
    public Text pergunta;
    public Button R1, R2, R3, R4;
    private controleTempo _controleTempo;
    [HideInInspector]
    public bool clickBotao;


    // Start is called before the first frame update
    IEnumerator Start()
    {

        _controleTempo = FindObjectOfType(typeof(controleTempo)) as controleTempo;

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
            print(itens[7]);

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
            switch (itens[7])
            {
                case "A":
                    R1.image.color = Color.red;
                    break;
                case "B":
                    R2.image.color = Color.red;
                    break;
                case "C":
                    R3.image.color = Color.red;
                    break;
                case "D":
                    R4.image.color = Color.red;
                    break;
            }
        }

    }

    public void BotaoResposta()
    {
        string botaoClicado = EventSystem.current.currentSelectedGameObject.name;
        clickBotao = true;

    }








}
