using TMPro;
using UnityEngine;
using UnityEngine.UI;


/*
 * Preenche TMPs de nome em qualquer cena.
 *
 * Modos:
 *   - PARTIDA:  usa nomeJogadorAtual / nomeOponenteAtual do WebSocket
 *   - USUARIO:  usa o nome do próprio jogador (PlayerPrefs "usuarioNome")
 *
 * Uso:
 *   1. Anexe este componente num GameObject da cena
 *   2. Escolha o Modo
 *   3. Arraste os TMPs correspondentes
 */
public class NomesJogadores : MonoBehaviour
{
    public enum Modo
    {
        Partida,   // mostra nome do jogador + nome do oponente
        Usuario    // mostra só o nome do jogador logado
    }

    [Header("Modo")]
    public Modo modo = Modo.Partida;

    [Header("Textos")]
    public Text textoNomeJogador;
    public Text textoNomeOponente;

    private void Start()
    {
        AtualizarNomes();
    }

    private void Update()
    {
        AtualizarNomes();
    }

    private void AtualizarNomes()
    {
        if (modo == Modo.Usuario)
        {
            AtualizarNomeUsuario();
        }
        else
        {
            AtualizarNomesPartida();
        }
    }

    // ============================================================
    // Modo Usuário — nome do jogador logado
    // ============================================================
    private void AtualizarNomeUsuario()
    {
        if (textoNomeJogador == null)
        {
            return;
        }

        string nome = PlayerPrefs.GetString("usuarioNome", "");

        if (!string.IsNullOrEmpty(nome) &&
            textoNomeJogador.text != nome)
        {
            textoNomeJogador.text = nome;
        }
    }

    // ============================================================
    // Modo Partida — nomes do jogador + oponente
    // ============================================================
    private void AtualizarNomesPartida()
    {
        if (WebSocketUnity.Instance == null)
        {
            return;
        }

        // ---- Nome do jogador ----
        if (textoNomeJogador != null)
        {
            string nome = WebSocketUnity.Instance.nomeJogadorAtual;

            if (!string.IsNullOrEmpty(nome) &&
                textoNomeJogador.text != nome)
            {
                textoNomeJogador.text = nome;
            }
        }

        // ---- Nome do oponente ----
        if (textoNomeOponente != null)
        {
            string nome = WebSocketUnity.Instance.nomeOponenteAtual;

            if (!string.IsNullOrEmpty(nome) &&
                textoNomeOponente.text != nome)
            {
                textoNomeOponente.text = nome;
            }
        }
    }
}