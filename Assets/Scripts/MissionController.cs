using System.Collections.Generic;
using UnityEngine;

public class MissionController : MonoBehaviour
{
    public GameObject productName;
    public GameObject productIcon;

    public int[] productIngredients = new int[3];
    public GameObject[] productImages = new GameObject[3];

    public List<Sanduiche> products = new List<Sanduiche>(); 

    // Start is called before the first frame update
    void Start()
    {
        RandomizeMission();
    }

    public void RandomizeMission()
    {
        int productid = Random.Range(0, products.Count - 1);
        productName.GetComponent<TextMesh>().text = products[productid].nome;
        productIcon.GetComponent<SpriteRenderer>().sprite = products[productid].icone;
        productIngredients = products[productid].ingredientsID;
        for (int i = 0; i < productImages.Length; i++)
            productImages[i].GetComponent<MeshRenderer>().material = products[productid].ingredientsImages[i];
    }
}