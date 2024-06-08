using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Cadastro : MonoBehaviour

{

    private string urlFormulario = "https://zeleystudios.servegame.com/corujao/inserir.php";
    private string id, nome;
    public InputField InputNome;
    private Login _login;


    void Start()
    {
        _login = FindObjectOfType(typeof(Login)) as Login;
    }

    public void CadastrarNome()
    {
        StartCoroutine("SubmitDados");

    }


    IEnumerator SubmitDados()
    {
        id = _login.IdUsuario;
        nome = InputNome.text;

        WWWForm form = new WWWForm();
        form.AddField("nome", nome);
        form.AddField("id", id);


        UnityWebRequest itemdata = UnityWebRequest.Post(urlFormulario, form);

        yield return itemdata.SendWebRequest();
        print("Cadastrado");
        SceneManager.LoadScene(1);
    }

}
