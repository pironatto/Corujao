using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Rotacao : MonoBehaviour
{
    public int giro;
    //public GameObject Seta, Ok;
    //public TextMeshProUGUI Red, Green;
    public GameObject Seta;

    // Start is called before the first frame update
    void Start()
    {
      //  Ok.SetActive(false);
        Seta.SetActive(true);
      //  Red.enabled = true;
       // Green.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        Seta.transform.Rotate(0, 0, giro * Time.deltaTime);
       // StartCoroutine("EncontrarAdversario");
    }

    IEnumerator EncontrarAdversario()
    {
        yield return new WaitForSeconds(5f);
        Seta.SetActive(false);
      //  Ok.SetActive(true);
      //  Red.enabled = false;
      //  Green.enabled = true;

        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(3);



    }

}
