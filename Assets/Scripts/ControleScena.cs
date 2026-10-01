using UnityEngine;
using UnityEngine.SceneManagement;

public class ControleScena : MonoBehaviour
{
    public void AbrirConfiguracoes()
    {
        SceneManager.LoadScene("Config");
    }

    public void AbrirTemas()
    {
        SceneManager.LoadScene("Temas");
    }

    public void AbrirRanking()
    {
        SceneManager.LoadScene("Ranking");
    }
}