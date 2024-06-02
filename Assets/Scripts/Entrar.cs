using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Entrar : MonoBehaviour
{

    private string urlFormulario = "http://localhost/Quiz/login.php";
    private string id, nome;
    public InputField InputId, InputNome;
    public Text Retorno;


    public void btnLogin()
    {
        StartCoroutine("EntrarJogo");
    }

    public IEnumerator EntrarJogo()
    {
        id = InputId.text;
        nome = InputNome.text;


        WWWForm form = new WWWForm();
        form.AddField("id", id);
        form.AddField("nome", nome);

        UnityWebRequest itemdata = UnityWebRequest.Post(urlFormulario, form);
        yield return itemdata.SendWebRequest();
        string itemDataString = itemdata.downloadHandler.text;

        InputId.text = "";
        InputNome.text = "";

        yield return itemDataString;
        Retorno.text = itemDataString;


    }

}
