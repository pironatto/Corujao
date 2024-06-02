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
    public GameObject PanelCadastro, Barra;
    public Slider BarraProgresso;
    private float currentTime;
    private bool BarraCompleta;


    private async void Start()
    {

        currentTime = 0;
        BarraCompleta = false;

        await UnityServices.InitializeAsync(); // INICIALIZAR O SERVIÇO DE AUTENTICAÇÃO UNITY
        Debug.Log(UnityServices.State); // INFORMAR QUE O SERVIÇO ESTÁ ATIVO

        Conectar(); // AUTENTICAR O USUARIO COM A UNITY


    }

    void Update()
    {
        BarraLoad();
        if (BarraCompleta == false && BarraProgresso.value >= 5)
        {
            StartCoroutine("VerificaCadastro"); //VERIFICAR OS DADOS DO USUARIO AUTENTICADO COM O BANCO DE DADOS
        }


    }


    IEnumerator VerificaCadastro()
    {
        BarraCompleta = true;
        // Shows how to get the playerID
        IdUsuario = AuthenticationService.Instance.PlayerId;
        print($"Id do usuario: {IdUsuario}");

        //PEGAR OS DADOS DO BANCO DE DADOS
        using (UnityWebRequest cadastroUsuario = UnityWebRequest.Get("https://studioszeley.000webhostapp.com/consulta.php"))
        {

            yield return cadastroUsuario.SendWebRequest();
            string User = cadastroUsuario.downloadHandler.text;
            Usuario = User.Trim(); //PARA REMOVER ESPAÇOS EM BRANCO     
            if (IdUsuario == Usuario)
            {
                SceneManager.LoadScene(1);
            }
            else
            {
                print("Voce precisa se cadastrar");
                PanelCadastro.SetActive(true);
                Barra.SetActive(false);

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


    public void BarraLoad()
    {
        currentTime += Time.deltaTime;
        BarraProgresso.value = currentTime;

    }

}
