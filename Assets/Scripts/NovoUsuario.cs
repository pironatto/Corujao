using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NovoUsuario : MonoBehaviour
{

    private string urlFormulario = "http://localhost/Quiz/inserir.php";
    private string id, nome;
    public InputField InputId, InputNome;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }

    public void Submitdados()
    {
        StartCoroutine("SubmitDados");
    }


    private IEnumerator SubmitDados()
    {

        id = InputId.text;
        nome = InputNome.text;

        WWWForm form = new WWWForm();
        form.AddField("id", id);
        form.AddField("nome", nome);

        UnityWebRequest itemdata = UnityWebRequest.Post(urlFormulario, form);
        yield return itemdata.SendWebRequest();
        print("Enviado");

        InputId.text = "";
        InputNome.text = "";

    }
}
