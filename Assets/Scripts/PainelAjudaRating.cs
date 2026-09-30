using UnityEngine;
using UnityEngine.UI;

public class PainelAjudaRating : MonoBehaviour
{
    [Header("Referências")]
    public GameObject painel;         // O painel (com overlay) que será mostrado/escondido
    public Button botaoAbrir;         // Botão "?" que abre o painel
    public Button botaoFechar;        // Botão "Entendi" que fecha o painel

    private bool aberto = false;

    private void Start()
    {
        // Garante que o painel começa escondido
        if (painel != null)
        {
            painel.SetActive(false);
        }

        aberto = false;

        // Configura os botões
        if (botaoAbrir != null)
        {
            botaoAbrir.onClick.AddListener(Abrir);
        }

        if (botaoFechar != null)
        {
            botaoFechar.onClick.AddListener(Fechar);
        }
    }

    /// <summary>
    /// Abre o painel. Se já estiver aberto, fecha (toggle).
    /// </summary>
    public void Abrir()
    {
        if (painel == null) return;

        if (aberto)
        {
            Fechar();
            return;
        }

        painel.SetActive(true);
        aberto = true;

        Debug.Log("[AjudaRating] Painel aberto.");
    }

    /// <summary>
    /// Fecha o painel.
    /// </summary>
    public void Fechar()
    {
        if (painel == null) return;

        painel.SetActive(false);
        aberto = false;

        Debug.Log("[AjudaRating] Painel fechado.");
    }
}