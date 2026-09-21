using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{
    private string Usuario;
    [HideInInspector] public string IdUsuario;
    public GameObject PanelCadastro;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        Debug.Log("UnityServices iniciado: " + UnityServices.State);

        await SignInAnonymouslyAsync();

        // Garante que o PlayerId foi gerado antes de continuar
        StartCoroutine(VerificaCadastro());
    }

    IEnumerator VerificaCadastro()
    {
        // Espera até o ID autenticar
        yield return new WaitUntil(() => !string.IsNullOrEmpty(AuthenticationService.Instance.PlayerId));

        IdUsuario = AuthenticationService.Instance.PlayerId;
        Debug.Log("Id do usuario: " + IdUsuario);

        WWWForm form = new WWWForm();
        form.AddField("id", IdUsuario);

        using (UnityWebRequest cadastroUsuario = UnityWebRequest.Post(
            "https://zeleystudios.online/corujao/consulta",
            form))
        {
            yield return cadastroUsuario.SendWebRequest();

            if (cadastroUsuario.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erro ao consultar cadastro: " + cadastroUsuario.error);
                PanelCadastro?.SetActive(true);
                yield break;
            }

            string user = cadastroUsuario.downloadHandler != null
                ? cadastroUsuario.downloadHandler.text
                : string.Empty;

            Usuario = user.Trim();

            if (!string.IsNullOrEmpty(IdUsuario) && IdUsuario == Usuario)
            {
                Debug.Log("Usuário já cadastrado. Entrando no jogo...");
                SceneManager.LoadScene("Config");
            }
            else
            {
                Debug.Log("Usuário precisa se cadastrar.");
                PanelCadastro?.SetActive(true);
            }
        }
    }

    public async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }
}