using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/* ============================================================
 * Modelos de dados (espelham o que o PHP retorna em `dados`)
 * ============================================================ */

[Serializable]
public class RankingJogador
{
    public string id;
    public string nome;
    public int rating;
    public string faixa_nome;    // 🆕
    public string faixa_cor;     // 🆕
    public string faixa_emoji;   // 🆕
    public int melhor_rating;
    public int ultima_variacao_rating;
    public int partidas_rating;
    public float pontos;
    public int partidas_jogadas;
    public int partidas_vencidas;
    public int partidas_perdidas;
    public int partidas_empatadas;
}

[Serializable]
public class RankingMateria
{
    public string id;
    public string nome;
    public string materia;
    public int rating;
    public int melhor_rating;
    public int ultima_variacao_rating;
    public int partidas_rating;
    public float pontos;
    public int partidas_jogadas;
    public int partidas_vencidas;
    public int partidas_perdidas;
    public int partidas_empatadas;
}

[Serializable]
public class HistoricoPartida
{
    public string partida_id;
    public string materia;
    public string resultado;
    public float pontuacao;
    public int respostas_totais;
    public int respostas_corretas;
    public int rating_anterior;
    public int rating_novo;
    public int variacao_rating;
    public string saiu_em;
}

/* Resposta única — sempre usa o campo `dados` */
[Serializable]
public class RespostaRankingJogadores
{
    public string tipo;
    public RankingJogador[] dados;
    public string erro;
}

[Serializable]
public class RespostaRankingMateria
{
    public string tipo;
    public string materia;
    public RankingMateria[] dados;
    public string erro;
}

[Serializable]
public class RespostaHistorico
{
    public string tipo;
    public string usuarioId;
    public HistoricoPartida[] dados;
    public string erro;
}

/* ============================================================
 * Controlador da UI
 * ============================================================ */

public class TelaRankingPHP : MonoBehaviour
{
    [Header("Referências UI")]
    public TextMeshProUGUI tituloRanking;
    public Transform containerRanking;
    public GameObject prefabLinhaRanking;

    [Header("Botões de Filtro")]
    public Button botaoRankingGeral;
    public Dropdown dropdownMaterias;

    [Header("Histórico")]
    public Transform containerHistorico;
    public GameObject prefabLinhaHistorico;
    public Button botaoHistorico;

    [Header("Cabeçalhos")]
    public GameObject cabecalhoRanking;
    public GameObject cabecalhoHistorico;

    private string usuarioId;
    private const string urlBase =
        "https://zeleystudios.online/corujao/ranking";

    /* Mapeia índice do dropdown -> chave da matéria */
    private static readonly Dictionary<int, string> materiasPorIndice =
        new Dictionary<int, string>
        {
            { 1,  "historia" },
            { 2,  "ciencias" },
            { 3,  "matematica" },
            { 4,  "fisica" },
            { 5,  "harrypotter" },
            { 6,  "geografia" },
            { 7,  "biologia" },
            { 8,  "medicina" },
            { 9,  "personagens" },
            { 10, "frasesfilmes" }
        };

    private void Start()
    {
        usuarioId = PlayerPrefs.GetString("usuarioId", "");
        // 🆕 Esconde os cabeçalhos no início
        if (cabecalhoRanking != null) cabecalhoRanking.SetActive(false);
        if (cabecalhoHistorico != null) cabecalhoHistorico.SetActive(false);

        if (string.IsNullOrEmpty(usuarioId))
        {
            Debug.LogError(
                "ID do usuário não encontrado no PlayerPrefs."
            );
            return;
        }

        ConfigurarBotoes();
        PopularDropdownMaterias();
        CarregarRankingGeral();
    }




    private void ConfigurarBotoes()
    {


        if (botaoRankingGeral != null)
        {
            botaoRankingGeral.onClick.AddListener(
                CarregarRankingGeral
            );
        }

        if (botaoHistorico != null)
        {
            botaoHistorico.onClick.AddListener(
                CarregarHistorico
            );
        }

        if (dropdownMaterias != null)
        {
            dropdownMaterias.onValueChanged.AddListener(
                OnMateriaSelecionada
            );
        }
    }

    private void PopularDropdownMaterias()
    {
        if (dropdownMaterias == null) return;

        dropdownMaterias.ClearOptions();

        var opcoes = new List<string>
        {
            "Selecione um tema",
            "História",
            "Ciências",
            "Matemática",
            "Física",
            "Harry Potter",
            "Geografia",
            "Biologia",
            "Medicina",
            "Personagens",
            "Frases de Filmes"
        };

        dropdownMaterias.AddOptions(opcoes);
    }

    private void OnMateriaSelecionada(int index)
    {
        if (index <= 0) return;

        if (materiasPorIndice.TryGetValue(index, out string chave))
        {
            CarregarRankingMateria(chave);
        }
        else
        {
            Debug.LogWarning(
                $"Nenhuma matéria mapeada para o índice {index}."
            );
        }
    }

    /* ============================================================
     * Ações públicas (chamadas por botões)
     * ============================================================ */

    public void CarregarRankingGeral()
    {
        StopAllCoroutines();
        StartCoroutine(BuscarRankingGeral());
    }

    public void CarregarRankingMateria(string materia)
    {
        StopAllCoroutines();
        StartCoroutine(BuscarRankingMateria(materia));
    }

    public void CarregarHistorico()
    {
        StopAllCoroutines();
        StartCoroutine(BuscarHistorico());
    }

    /* ============================================================
     * Coroutines de busca
     * ============================================================ */

    private IEnumerator BuscarRankingGeral()
    {
        LimparTudo();


        // (opcional) já esconde os cabeçalhos até ter dados
        if (cabecalhoRanking != null) cabecalhoRanking.SetActive(false);
        if (cabecalhoHistorico != null) cabecalhoHistorico.SetActive(false);

        string url = $"{urlBase}?acao=geral";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"Erro ao buscar ranking geral: {request.error}"
                );
                ExibirMensagem("Erro ao carregar ranking");
                yield break;
            }

            string json = request.downloadHandler.text;

            RespostaRankingJogadores resposta;

            try
            {
                resposta = JsonUtility.FromJson
                    <RespostaRankingJogadores>(json);
            }
            catch (Exception erro)
            {
                Debug.LogError(
                    $"Erro ao processar JSON: {erro.Message}\n{json}"
                );
                ExibirMensagem("Erro ao processar dados");
                yield break;
            }

            if (resposta == null)
            {
                ExibirMensagem("Resposta vazia do servidor");
                yield break;
            }

            if (!string.IsNullOrEmpty(resposta.erro))
            {
                ExibirMensagem($"Erro: {resposta.erro}");
                yield break;
            }

            if (resposta.dados == null || resposta.dados.Length == 0)
            {
                ExibirMensagem("Sem dados no ranking");
                yield break;
            }

            // 🆕 Mostra o cabeçalho de RANKING
            if (cabecalhoRanking != null) cabecalhoRanking.SetActive(true);
            if (cabecalhoHistorico != null) cabecalhoHistorico.SetActive(false);

            tituloRanking.text = "RANKING GERAL";
            ExibirRankingGeral(resposta.dados);
        }
    }

    private IEnumerator BuscarRankingMateria(string materia)
    {
        LimparTudo();

        if (cabecalhoRanking != null) cabecalhoRanking.SetActive(false);
        if (cabecalhoHistorico != null) cabecalhoHistorico.SetActive(false);

        WWWForm form = new WWWForm();
        form.AddField("materia", materia);

        string url = $"{urlBase}?acao=materia";

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"Erro ao buscar ranking de {materia}: {request.error}"
                );
                ExibirMensagem($"Erro ao carregar ranking de {materia}");
                yield break;
            }

            string json = request.downloadHandler.text;

            RespostaRankingMateria resposta;

            try
            {
                resposta = JsonUtility.FromJson
                    <RespostaRankingMateria>(json);
            }
            catch (Exception erro)
            {
                Debug.LogError(
                    $"Erro ao processar JSON: {erro.Message}\n{json}"
                );
                ExibirMensagem("Erro ao processar dados");
                yield break;
            }

            if (resposta == null)
            {
                ExibirMensagem("Resposta vazia do servidor");
                yield break;
            }

            if (!string.IsNullOrEmpty(resposta.erro))
            {
                ExibirMensagem($"Erro: {resposta.erro}");
                yield break;
            }

            if (resposta.dados == null || resposta.dados.Length == 0)
            {
                ExibirMensagem($"Sem dados para {materia}");
                yield break;
            }


            // 🆕 Mostra o cabeçalho de RANKING
            if (cabecalhoRanking != null) cabecalhoRanking.SetActive(true);
            if (cabecalhoHistorico != null) cabecalhoHistorico.SetActive(false);

            tituloRanking.text =
                $"RANKING - {materia.ToUpperInvariant()}";

            ExibirRankingMateria(resposta.dados);
        }
    }

    private IEnumerator BuscarHistorico()
    {
        LimparTudo();

        if (cabecalhoRanking != null) cabecalhoRanking.SetActive(false);
        if (cabecalhoHistorico != null) cabecalhoHistorico.SetActive(false);

        WWWForm form = new WWWForm();
        form.AddField("usuarioId", usuarioId);

        string url = $"{urlBase}?acao=historico";

        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"Erro ao buscar histórico: {request.error}"
                );
                ExibirMensagem("Erro ao carregar histórico");
                yield break;
            }

            string json = request.downloadHandler.text;

            RespostaHistorico resposta;

            try
            {
                resposta = JsonUtility.FromJson
                    <RespostaHistorico>(json);
            }
            catch (Exception erro)
            {
                Debug.LogError(
                    $"Erro ao processar JSON: {erro.Message}\n{json}"
                );
                ExibirMensagem("Erro ao processar dados");
                yield break;
            }

            if (resposta == null)
            {
                ExibirMensagem("Resposta vazia do servidor");
                yield break;
            }

            if (!string.IsNullOrEmpty(resposta.erro))
            {
                ExibirMensagem($"Erro: {resposta.erro}");
                yield break;
            }

            if (resposta.dados == null || resposta.dados.Length == 0)
            {
                ExibirMensagem("Sem histórico de partidas");
                yield break;
            }


            // 🆕 Mostra o cabeçalho de HISTÓRICO
            if (cabecalhoRanking != null) cabecalhoRanking.SetActive(false);
            if (cabecalhoHistorico != null) cabecalhoHistorico.SetActive(true);

            tituloRanking.text = "SEU HISTÓRICO";
            ExibirHistorico(resposta.dados);
        }
    }

    /* ============================================================
     * Renderização
     * ============================================================ */
    private void ExibirRankingGeral(RankingJogador[] ranking)
    {
        for (int i = 0; i < ranking.Length; i++)
        {
            RankingJogador jogador = ranking[i];

            GameObject linha = Instantiate(
                prefabLinhaRanking,
                containerRanking
            );

            TextMeshProUGUI[] textos =
                linha.GetComponentsInChildren<TextMeshProUGUI>();

            if (textos.Length < 4)
            {
                Debug.LogWarning(
                    $"Prefab de linha de ranking precisa de pelo menos 4 " +
                    $"TextMeshProUGUI. Encontrados: {textos.Length}"
                );
                continue;
            }

            textos[0].text = (i + 1).ToString();
            textos[1].text = jogador.nome;
            textos[2].text = jogador.faixa_nome;
            textos[3].text = jogador.pontos.ToString("F0");

            // 🆕 Colorir a coluna "Nível" com a cor da faixa
            if (ColorUtility.TryParseHtmlString(jogador.faixa_cor, out Color corFaixa))
            {
                textos[2].color = corFaixa;
            }
            else
            {
                Debug.LogWarning($"Cor inválida para faixa '{jogador.faixa_nome}': {jogador.faixa_cor}");
            }
        }
    }


    private void ExibirRankingMateria(RankingMateria[] ranking)
    {
        for (int i = 0; i < ranking.Length; i++)
        {
            RankingMateria jogador = ranking[i];

            GameObject linha = Instantiate(
                prefabLinhaRanking,
                containerRanking
            );

            TextMeshProUGUI[] textos =
                linha.GetComponentsInChildren<TextMeshProUGUI>();

            if (textos.Length < 4)
            {
                Debug.LogWarning(
                    $"Prefab de linha de ranking precisa de pelo menos 4 " +
                    $"TextMeshProUGUI. Encontrados: {textos.Length}"
                );
                continue;
            }

            textos[0].text = (i + 1).ToString();        // posição
            textos[1].text = jogador.nome;              // nome
            textos[2].text = jogador.rating.ToString(); // rating
            textos[3].text = jogador.pontos.ToString("F0"); // pontos
        }
    }

    private void ExibirHistorico(HistoricoPartida[] historico)
    {
        for (int i = 0; i < historico.Length; i++)
        {
            HistoricoPartida partida = historico[i];

            GameObject linha = Instantiate(
                prefabLinhaHistorico,
                containerHistorico
            );

            TextMeshProUGUI[] textos =
                linha.GetComponentsInChildren<TextMeshProUGUI>();

            if (textos.Length < 6)
            {
                Debug.LogWarning(
                    $"Prefab de histórico precisa de pelo menos 6 " +
                    $"TextMeshProUGUI. Encontrados: {textos.Length}"
                );
                continue;
            }

            textos[0].text = FormatarData(partida.saiu_em);
            textos[1].text = (partida.materia ?? "").ToUpperInvariant();
            textos[2].text = FormatarResultado(partida.resultado);
            textos[3].text = partida.pontuacao.ToString("F0");
            textos[4].text =
                $"{partida.respostas_corretas}/{partida.respostas_totais}";

            // 🆕 Variação com cor dinâmica
            textos[5].text = FormatarVariacao(partida.variacao_rating);

            if (partida.variacao_rating > 0)
            {
                textos[5].color = Color.green;
            }
            else if (partida.variacao_rating < 0)
            {
                textos[5].color = Color.red;
            }
            else
            {
                textos[5].color = Color.gray;
            }
        }
    }

    /* ============================================================
     * Helpers de formatação
     * ============================================================ */

    private string FormatarResultado(string resultado)
    {
        if (string.IsNullOrEmpty(resultado)) return "—";

        switch (resultado.ToLowerInvariant())
        {
            case "vitoria": return "VITÓRIA";
            case "derrota": return "DERROTA";
            case "empate": return "EMPATE";
            case "individual": return "INDIVIDUAL";
            case "abandono": return "ABANDONO";
            default: return resultado.ToUpperInvariant();
        }
    }

    private string FormatarVariacao(int variacao)
    {
        if (variacao > 0) return $"+{variacao}";
        if (variacao < 0) return variacao.ToString();
        return "—";
    }

    private string FormatarData(string dataSql)
    {
        if (string.IsNullOrEmpty(dataSql)) return "—";

        /*
         * O backend SEMPRE envia este campo em UTC.
         * (MySQL armazena em UTC; PHP repassa cru; Node/JS usa Date UTC.)
         *
         * Forçamos o parse como UTC e depois convertemos
         * para o fuso local do dispositivo do jogador.
         */
        if (DateTime.TryParse(
            dataSql,
            null,
            System.Globalization.DateTimeStyles.AssumeUniversal
                | System.Globalization.DateTimeStyles.AdjustToUniversal,
            out DateTime dataUtc))
        {
            DateTime dataLocal = dataUtc.ToLocalTime();
            return dataLocal.ToString("dd/MM HH:mm");
        }

        return dataSql;
    }

    /* ============================================================
     * Limpeza de UI
     * ============================================================ */

    private void LimparTudo()
    {
        LimparContainer(containerRanking);
        LimparContainer(containerHistorico);
    }

    private void LimparContainer(Transform container)
    {
        if (container == null) return;

        foreach (Transform filho in container)
        {
            Destroy(filho.gameObject);
        }
    }

    private void ExibirMensagem(string mensagem)
    {
        LimparTudo();

        // 🆕 Esconde os cabeçalhos quando há erro
        if (cabecalhoRanking != null) cabecalhoRanking.SetActive(false);
        if (cabecalhoHistorico != null) cabecalhoHistorico.SetActive(false);

        if (tituloRanking != null)
        {
            tituloRanking.text = mensagem;
        }

        Debug.LogWarning(mensagem);
    }
}