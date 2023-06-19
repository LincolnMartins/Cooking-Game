using UnityEngine;

[CreateAssetMenu(menuName = "Sanduiche")]
public class Sanduiche : ScriptableObject
{
    public string nome;
    public Sprite icone;
    public Material[] ingredientsImages = new Material[3];
    public int[] ingredientsID = new int[3];
}
