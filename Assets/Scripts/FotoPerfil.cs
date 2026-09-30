using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/*
 * Gerencia a foto de perfil na tela Config.
 *
 * Fase 2e.4: envia a foto escolhida pro avatar.php e salva a URL.
 */
public class FotoPerfil : MonoBehaviour
{
    [Header("Referências")]
    public RawImage fotoRawImage;   // O RawImage que mostra a foto
    public Button btnTrocarFoto;    // O botão invisível por cima da foto

    [Header("Configuração")]
    private const string URL_AVATAR =
        "https://zeleystudios.online/corujao/avatar";

    private void Start()
    {
        if (btnTrocarFoto != null)
        {
            btnTrocarFoto.onClick.AddListener(OnClicarTrocarFoto);
        }

        // 🆕 Carrega a foto salva (se houver) ao abrir a tela
        CarregarFotoSalva();
    }

    // ============================================================
    // Clique — abre a galeria
    // ============================================================
    private void OnClicarTrocarFoto()
    {
        Debug.Log("[FotoPerfil] Clicou em trocar foto — abrindo galeria...");

        NativeGallery.GetImageFromGallery(
            (path) => OnFotoEscolhida(path),
            "Escolha uma foto de perfil"
        );
    }

    // ============================================================
    // Foto escolhida — mostra e envia pro servidor
    // ============================================================
    private void OnFotoEscolhida(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("[FotoPerfil] Jogador cancelou a escolha.");
            return;
        }

        Debug.Log($"[FotoPerfil] Foto escolhida: {path}");

        // Carrega a textura
        Texture2D texture = NativeGallery.LoadImageAtPath(path, 512, false);

        if (texture == null)
        {
            Debug.LogError("[FotoPerfil] Falha ao carregar a textura.");
            return;
        }

        // Mostra a foto
        MostrarFoto(texture);

        // Envia pro servidor
        StartCoroutine(EnviarFotoProServidor(texture));
    }

    // ============================================================
    // Mostra a foto no RawImage
    // ============================================================
    private void MostrarFoto(Texture2D texture)
    {
        if (fotoRawImage == null) return;

        fotoRawImage.texture = texture;
        fotoRawImage.color = Color.white;
        // 🆕 Desloca a textura verticalmente para baixo dentro do RawImage
        // O valor 0.15f joga a imagem um pouco para baixo. Ajuste esse número se precisar!
        fotoRawImage.uvRect = new Rect(0f, 0.08f, 1f, 1f);
    }

    // ============================================================
    // Upload pro servidor
    // ============================================================
    private IEnumerator EnviarFotoProServidor(Texture2D texture)
    {
        // Pega o ID do usuário
        string usuarioId = PlayerPrefs.GetString("usuarioId", "");

        if (string.IsNullOrEmpty(usuarioId))
        {
            Debug.LogError("[FotoPerfil] usuarioId vazio — não dá pra enviar.");
            yield break;
        }

        // Converte a textura pra JPG (qualidade 85 — bom equilíbrio)
        byte[] bytes = texture.EncodeToJPG(85);

        Debug.Log($"[FotoPerfil] Enviando foto ({bytes.Length} bytes) pro servidor...");

        // Monta o form
        WWWForm form = new WWWForm();
        form.AddField("id", usuarioId);
        form.AddBinaryData("foto", bytes, "foto.jpg", "image/jpeg");

        // Envia
        using (UnityWebRequest www = UnityWebRequest.Post(URL_AVATAR, form))
        {
            www.timeout = 30;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[FotoPerfil] Erro no upload: {www.error}");
                yield break;
            }

            string json = www.downloadHandler.text;
            Debug.Log($"[FotoPerfil] Resposta do servidor: {json}");

            // Parse simples do JSON
            RespostaAvatar resposta =
                JsonUtility.FromJson<RespostaAvatar>(json);

            if (resposta == null || !resposta.sucesso)
            {
                Debug.LogError(
                    "[FotoPerfil] Servidor rejeitou o upload: " +
                    (resposta != null ? resposta.erro : "resposta nula")
                );
                yield break;
            }

            Debug.Log($"[FotoPerfil] ✅ Upload OK! URL: {resposta.avatarUrl}");

            // Salva a URL localmente
            PlayerPrefs.SetString("avatarUrl", resposta.avatarUrl);
            PlayerPrefs.Save();
        }
    }

    // ============================================================
    // Carrega a foto salva (do PlayerPrefs ou do banco)
    // ============================================================
    private void CarregarFotoSalva()
    {
        string url = PlayerPrefs.GetString("avatarUrl", "");

        if (string.IsNullOrEmpty(url))
        {
            Debug.Log("[FotoPerfil] Nenhuma foto salva ainda.");
            return;
        }

        Debug.Log($"[FotoPerfil] Baixando foto salva: {url}");
        StartCoroutine(BaixarFoto(url));
    }

    private IEnumerator BaixarFoto(string url)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            www.timeout = 15;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[FotoPerfil] Erro ao baixar: {www.error}");
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(www);

            if (texture != null)
            {
                MostrarFoto(texture);
                Debug.Log("[FotoPerfil] ✅ Foto salva carregada.");
            }
        }
    }

    // ============================================================
    // Classe pra parsear a resposta JSON do avatar.php
    // ============================================================
    [System.Serializable]
    private class RespostaAvatar
    {
        public bool sucesso;
        public string avatarUrl;
        public string erro;
    }
}