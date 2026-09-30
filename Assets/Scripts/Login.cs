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

    [HideInInspector]
    public string IdUsuario;

    public GameObject PanelCadastro;

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            Debug.Log(
                "UnityServices iniciado: " +
                UnityServices.State
            );

            // 🆕 Agora retorna bool — se falhar, paramos aqui
            bool logado = await SignInAnonymouslyAsync();

            if (!logado)
            {
                Debug.LogError(
                    "Falha no login anônimo. Abrindo painel de cadastro."
                );

                PanelCadastro?.SetActive(true);
                return;
            }

            StartCoroutine(
                VerificaCadastro()
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError(
                "Erro ao iniciar autenticação: " +
                ex.Message
            );

            Debug.LogException(ex);
            PanelCadastro?.SetActive(true);
        }
    }

    private IEnumerator VerificaCadastro()
    {
        yield return new WaitUntil(() =>
            AuthenticationService.Instance != null &&
            !string.IsNullOrEmpty(
                AuthenticationService.Instance.PlayerId
            )
        );

        IdUsuario =
            AuthenticationService.Instance.PlayerId;

        Debug.Log(
            "ID do usuário: " +
            IdUsuario
        );

        PlayerPrefs.SetString(
            "usuarioId",
            IdUsuario
        );

        PlayerPrefs.Save();

        WWWForm form = new WWWForm();
        form.AddField("id", IdUsuario);

        using (
            UnityWebRequest cadastroUsuario =
                UnityWebRequest.Post(
                    "https://zeleystudios.online/corujao/consulta",
                    form
                )
        )
        {
            // 🆕 Timeout de 10s — evita travar se o servidor não responder
            cadastroUsuario.timeout = 10;

            yield return cadastroUsuario.SendWebRequest();

            if (
                cadastroUsuario.result !=
                UnityWebRequest.Result.Success
            )
            {
                Debug.LogError(
                    "Erro ao consultar cadastro: " +
                    cadastroUsuario.error
                );

                PanelCadastro?.SetActive(true);
                yield break;
            }

            string resposta =
                cadastroUsuario.downloadHandler != null
                    ? cadastroUsuario.downloadHandler.text
                    : string.Empty;

            Usuario =
                resposta.Trim();

            if (
                !string.IsNullOrEmpty(IdUsuario) &&
                IdUsuario == Usuario
            )
            {
                Debug.Log(
                    "Usuário já cadastrado. " +
                    "Entrando no jogo..."
                );
                yield return new WaitForSeconds(5f);
                SceneManager.LoadScene("Config");
            }
            else
            {
                Debug.Log(
                    "Usuário precisa se cadastrar."
                );

                PanelCadastro?.SetActive(true);
            }
        }
    }

    // 🆕 Agora retorna bool em vez de Task
    public async Task<bool> SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance
                .SignInAnonymouslyAsync();

            return true;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            return false;
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            return false;
        }
    }
}