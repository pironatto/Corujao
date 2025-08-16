using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using JetBrains.Annotations;


public class Cadastro : MonoBehaviour

{

    private string urlFormulario = "https://zeleystudios.servegame.com/corujao//inserir.php";
    private string id, nome, erro;
    public TMP_InputField InputNome;
    public TextMeshProUGUI Erro;
    private Login _login;


    void Start()
    {
        _login = FindFirstObjectByType(typeof(Login)) as Login;

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
        erro = itemdata.downloadHandler.text;
        print(erro);

        //ERRO 1062 SIGNIFICA "MYSQL_ER_DUP_ENTRY", OU SEJA ENTRADA DUPLICADA 
        //ERRO 0 SIGNIFICA TUDO OK - O SISTEMA TRAZ DOIS ESPAÇOS INICIAIS
        if (erro == "  00000")
        {
            Erro.text = "Cadastro efetuado! Vamos começar...";
            yield return new WaitForSeconds(3f);
            SceneManager.LoadScene(1);

        }
        else
        {
            Erro.text = "Este nome já existe. Escolha outro.";
            yield return new WaitForSeconds(2f);
            
            Erro.text = "";
            InputNome.text = "";
        }

    }

    public void EscolherTema()
    {
        SceneManager.LoadScene(2);
        print("Escolheu");
    }

}
