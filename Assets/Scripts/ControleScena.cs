using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControleScena : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void ChamarFade()
    {
        SceneManager.LoadScene(1);
    }


    public void MudarCena()
    {
        SceneManager.LoadScene(2);
    }

}
