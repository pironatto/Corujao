using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ControleTemas : MonoBehaviour
{

    [HideInInspector]
    public static string materia;

    public void Awake()
    {
        controleTempo.valorSlider1 = 0;
        controleTempo.valorSlider2 = 0;
        controleTempo.valorSlider3 = 0;
        controleTempo.valorSlider4 = 0;
        controleTempo.valorSlider5 = 0;
        controleTempo.ValorSliderA = 0;
    }


    public void ChamarTelaEspera()
    {


        string botaoClicado = EventSystem.current.currentSelectedGameObject.name;
        if (botaoClicado == "BtHistoria") { materia = "historia"; SceneManager.LoadScene(4); }
        if (botaoClicado == "BtCiencias") { materia = "ciencias"; SceneManager.LoadScene(4); }
        if (botaoClicado == "BtMatematica") { materia = "matematica"; SceneManager.LoadScene(4); }
        if (botaoClicado == "BtFisica") { materia = "fisica"; SceneManager.LoadScene(4); }
        if (botaoClicado == "BtHarryPotter") { materia = "harrypotter"; SceneManager.LoadScene(4); }
        if (botaoClicado == "BtGeografia") { materia = "geografia"; SceneManager.LoadScene(4); }
        if (botaoClicado == "BtBiologia") { materia = "biologia"; SceneManager.LoadScene(4); }
        if (botaoClicado == "BtMedicina") { materia = "medicina"; SceneManager.LoadScene(4); }

    }

}
