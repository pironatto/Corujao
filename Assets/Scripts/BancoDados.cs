using System.Collections;
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
    //  private ControleTemas _controleTemas;
    [HideInInspector]
    public bool clickBotao;
    private string materia;
    public GameObject PainelRespostas;


    // Start is called before the first frame update
    void Start()
    {
        _controleTempo = FindFirstObjectByType(typeof(controleTempo)) as controleTempo;
        StartCoroutine("VerificaPergunta");
        _controleTempo.enabled = false;
    }

    IEnumerator VerificaPergunta()
    {
        //PARA ESCOLHER O TEMA
        materia = ControleTemas.materia;
        WWWForm form = new WWWForm();
        form.AddField("materia", materia);

        //ESPERAR ATÉ QUE O BANCO DE DADOS SEJA LIDO
        using (UnityWebRequest itemdata = UnityWebRequest.Post("http://localhost/corujao//perguntas.php", form))
        {
            yield return itemdata.SendWebRequest();
            string itemDataString = itemdata.downloadHandler.text;


            itens = itemDataString.Split('-');

            pergunta.text = itens[2];

            yield return new WaitForSeconds(2f); //LAG ENTRE A PERGUNTA E AS RESPOSTAS
            PainelRespostas.SetActive(true);
            _controleTempo.enabled = true;
       
            
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
            if (itens[7] == "A") { R1.image.color = Color.green; }
            if (itens[7] == "B") { R2.image.color = Color.green; }
            if (itens[7] == "C") { R3.image.color = Color.green; }
            if (itens[7] == "D") { R4.image.color = Color.green; }

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
        //string botaoClicado = EventSystem.current.currentSelectedGameObject.name;
        clickBotao = true;

    }



}
