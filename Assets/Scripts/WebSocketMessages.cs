using System;
using UnityEngine;

[Serializable]
public class MensagemStatus
{
    public string tipo;
    public string mensagem;
    public string partidaId;
}

[Serializable]
public class MensagemIdentificado
{
    public string tipo;
    public string usuarioId;
    public string nome;
    public string avatarUrl;
}

[Serializable]
public class MensagemIdentificarUsuario
{
    public string tipo;
    public string usuarioId;
}

[Serializable]
public class MensagemMateria
{
    public string materia;
}

[Serializable]
public class MensagemParFormado
{
    public string tipo;
    public string partidaId;
    public string materia;
    public int tempoAbertura;
    public string nomeJogador;
    public string nomeOponente;
}

[Serializable]
public class PerguntaData
{
    public string tipo;
    public string partidaId;
    public string[] itens;
    public float tempoTotal;
    public long inicio;
    public long inicioServidor;
    public long terminaEmServidor;
    public long servidorAgora;
}

[Serializable]
public class MensagemResposta
{
    public string tipo;
    public string partidaId;
    public string resposta;
}

[Serializable]
public class MensagemResultadoResposta
{
    public string tipo;
    public string partidaId;
    public string resposta;
    public string correta;
    public bool acertou;
    public float pontos;
    public float pontuacaoTotal;
    public bool expirada;
    public long tempoRestante;
}

[Serializable]
public class MensagemPontuacaoOponente
{
    public string tipo;
    public string partidaId;
    public float pontos;
}

[Serializable]
public class MensagemResumoRespostas
{
    public string tipo;
    public string partidaId;
    public string respostaJogador;
    public string respostaOponente;
    public bool acertouJogador;
    public bool acertouOponente;
}

[Serializable]
public class MensagemFim
{
    public string tipo;
    public string partidaId;
    public string motivo;
    public bool partidaIndividual;
    public float pontuacaoJogador;
    public float pontuacaoOponente;
    public string nomeJogador;
    public string nomeOponente;
    public int ratingAnterior;
    public int ratingNovo;
    public int variacaoRating;
    public string faixaAntes;
    public string faixaDepois;
    public bool subiuFaixa;
}