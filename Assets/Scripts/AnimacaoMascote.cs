using UnityEngine;

/// <summary>
/// Anima o mascote da tela de loading.
/// Combina flutuação (sobe/desce) com pulsação (aumenta/diminui de escala).
/// Mantém a posição e escala originais como "centro" da animação.
/// </summary>
public class AnimacaoMascote : MonoBehaviour
{
    [Header("Flutuação (sobe/desce)")]
    [Tooltip("Quantos pixels o mascote sobe/desce em relação ao centro.")]
    public float amplitudeFlutuacao = 15f;

    [Tooltip("Duração de um ciclo completo de flutuação, em segundos.")]
    public float duracaoFlutuacao = 2.5f;

    [Header("Pulsação (escala)")]
    [Tooltip("Quanto a escala varia. 0.05 = varia 5% pra cima e pra baixo.")]
    [Range(0f, 0.3f)]
    public float amplitudePulsacao = 0.04f;

    [Tooltip("Duração de um ciclo completo de pulsação, em segundos.")]
    public float duracaoPulsacao = 2f;

    // Guarda o estado original pra sempre voltar ao "centro"
    private Vector3 posicaoOriginal;
    private Vector3 escalaOriginal;

    private void Awake()
    {
        posicaoOriginal = transform.localPosition;
        escalaOriginal = transform.localScale;
    }

    private void Update()
    {
        AnimarFlutuacao();
        AnimarPulsacao();
    }

    private void AnimarFlutuacao()
    {
        // Senoide: vai de -1 a +1 suavemente, sem "pulos"
        float t = Time.time / duracaoFlutuacao * Mathf.PI * 2f;
        float deslocamento = Mathf.Sin(t) * amplitudeFlutuacao;

        transform.localPosition = posicaoOriginal + new Vector3(0f, deslocamento, 0f);
    }

    private void AnimarPulsacao()
    {
        float t = Time.time / duracaoPulsacao * Mathf.PI * 2f;
        float escala = 1f + (Mathf.Sin(t) * amplitudePulsacao);

        transform.localScale = escalaOriginal * escala;
    }
}