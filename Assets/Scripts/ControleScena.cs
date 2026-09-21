using UnityEngine;
using UnityEngine.SceneManagement;

public class ControleScena : MonoBehaviour
{
    public void ChamarFade()
    {
        SceneManager.LoadScene("Config");
    }

    public void MudarCena()
    {
        SceneManager.LoadScene("Temas");
    }

    // Método recomendado para ligar diretamente ao botão
    public void EscolherTema()
    {
        SceneManager.LoadScene("Temas");
    }
}