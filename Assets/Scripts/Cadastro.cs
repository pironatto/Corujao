using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using JetBrains.Annotations;

public class Cadastro : MonoBehaviour
{
    private string urlFormulario = "https://zeleystudios.online/corujao/inserir";
    private string id, nome, erro;
    public TMP_InputField InputNome;
    public TextMeshProUGUI Erro;
    private Login _login;

    void Start()
    {
        _login = FindFirstObjectByType<Login>();
    }

    public void CadastrarNome()
    {
        if (_login == null)
        {
            Debug.LogError("Login não foi encontrado no cenário.");
            return;
        }

        if (string.IsNullOrWhiteSpace(InputNome?.text))
        {
            if (Erro != null)
                Erro.text = "Digite um nome para continuar.";
            return;
        }

        StartCoroutine(SubmitDados());
    }

    IEnumerator SubmitDados()
    {
        id = _login.IdUsuario;
        nome = InputNome.text.Trim();

        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("ID do usuário vazio. A autenticação ainda não está pronta.");
            if (Erro != null)
                Erro.text = "Aguarde um momento e tente novamente.";
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("nome", nome);
        form.AddField("id", id);

        using (UnityWebRequest itemdata = UnityWebRequest.Post(urlFormulario, form))
        {
            yield return itemdata.SendWebRequest();

            if (itemdata.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Erro ao cadastrar: {itemdata.error}");
                if (Erro != null)
                    Erro.text = "Erro ao cadastrar. Tente novamente.";
                yield break;
            }

            erro = itemdata.downloadHandler.text;
            print(erro);

            if (erro.Contains("00000") || erro.Trim() == "00000")
            {
                if (Erro != null)
                    Erro.text = "Cadastro efetuado! Vamos começar...";

                yield return new WaitForSeconds(3f);
                SceneManager.LoadScene("Temas");
            }
            else
            {
                if (Erro != null)
                    Erro.text = "Este nome já existe. Escolha outro.";

                yield return new WaitForSeconds(2f);
                Erro.text = "";
                InputNome.text = "";
            }
        }
    }
}
