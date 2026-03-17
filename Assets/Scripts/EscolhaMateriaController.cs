using UnityEngine;

public class EscolhaMateriaController : MonoBehaviour
{
    public void SelecionarMateria(string materia)
    {
        if (WebSocketUnity.Instance != null)
        {
            WebSocketUnity.Instance.OnMateriaSelecionada(materia);
        }
    }
}
