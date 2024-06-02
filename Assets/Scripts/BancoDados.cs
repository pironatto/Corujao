using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BancoDados : MonoBehaviour
{

    public string[] itens;     //ARMAZENAR OS ITENS
    public List<string> id, nome;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        //ESPERAR ATÉ QUE O BANCO DE DADOS SEJA LIDO
        using (UnityWebRequest itemdata = UnityWebRequest.Get("http://localhost/Quiz/consulta.php"))
        {
            yield return itemdata.SendWebRequest();
            string itemDataString = itemdata.downloadHandler.text;

            itens = itemDataString.Split(';');

            for (int i = 0; i < itens.Length - 1; i++)
            {
                id.Add(GetDataValue(itens[i], "id:"));
                nome.Add(GetDataValue(itens[i], "nome:"));
            }

        }


    }

    // Update is called once per frame
    void Update()
    {

    }

    //SEPARAR OS ITENS
    private string GetDataValue(string data, string index)
    {

        string value = data.Substring(data.LastIndexOf(index) + index.Length);

        if (value.Contains('|'))
        {
            value.Remove(value.IndexOf("|"));

        }


        return value;
    }


}
