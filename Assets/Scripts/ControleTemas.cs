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


    public void Start()
    {
      
    }

/*
    public void Awake()
    {
        controleTempo.valorSlider1 = 0;
        controleTempo.valorSlider2 = 0;
        controleTempo.valorSlider3 = 0;
        controleTempo.valorSlider4 = 0;
        controleTempo.valorSlider5 = 0;
        controleTempo.ValorSliderA = 0;
    }

*/
    public void ChamarTelaEspera()
    {
       
        string botaoClicado = EventSystem.current.currentSelectedGameObject.name;
        if (botaoClicado == "BtHistoria") { materia = "historia"; }
        if (botaoClicado == "BtCiencias") { materia = "ciencias"; }
        if (botaoClicado == "BtMatematica") { materia = "matematica"; }
        if (botaoClicado == "BtFisica") { materia = "fisica"; }
        if (botaoClicado == "BtHarryPotter") { materia = "harrypotter"; }
        if (botaoClicado == "BtGeografia") { materia = "geografia"; }
        if (botaoClicado == "BtBiologia") { materia = "biologia"; }
        if (botaoClicado == "BtMedicina") { materia = "medicina"; }


    }

}
