using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{

    private string Usuario;
    [HideInInspector]
    public string IdUsuario;
    public GameObject PanelCadastro;


    private async void Start()
    {


        await UnityServices.InitializeAsync(); // INICIALIZAR O SERVIÇO DE AUTENTICAÇÃO UNITY
        Debug.Log(UnityServices.State); // INFORMAR QUE O SERVIÇO ESTÁ ATIVO
        Conectar(); // AUTENTICAR O USUARIO COM A UNITY
        StartCoroutine("VerificaCadastro"); //VERIFICAR OS DADOS DO USUARIO AUTENTICADO COM O BANCO DE DADOS

    }

    void Update()
    {

    }


    IEnumerator VerificaCadastro()
    {

        yield return new WaitForSeconds(2f);
        // Shows how to get the playerID
        IdUsuario = AuthenticationService.Instance.PlayerId;
        print($"Id do usuario: {IdUsuario}");

        string id = IdUsuario.ToString();
        WWWForm form = new WWWForm();
        form.AddField("id", id);

        //PEGAR OS DADOS DO BANCO DE DADOS
        using (UnityWebRequest cadastroUsuario = UnityWebRequest.Post("http://localhost/corujao//consulta.php", form))
        {
            yield return cadastroUsuario.SendWebRequest();
            string User = cadastroUsuario.downloadHandler.text;
            print(User);
            Usuario = User.Trim(); //PARA REMOVER ESPAÇOS EM BRANCO     
            if (IdUsuario == Usuario)
            {
                SceneManager.LoadScene(1);
            }
            else
            {
                print("Voce precisa se cadastrar");
                PanelCadastro.SetActive(true);
            }

        }

    }

    public async void Conectar()
    {
        await SignInAnonymouslyAsync();
    }

    public async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }

    }


}
