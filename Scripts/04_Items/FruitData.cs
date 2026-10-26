using UnityEngine;

[CreateAssetMenu(fileName = "FruitData", menuName = "Caterpillar Game/Fruit Data")]
public sealed class FruitData : ScriptableObject
{
    [SerializeField] private string fruitName;
    [SerializeField] private int calorie;
    [SerializeField] private int redValue;
    [SerializeField] private int greenValue;
    [SerializeField] private int blueValue;

    public string FruitName => fruitName;
    public int Calorie => calorie;
    public int RedValue => redValue;
    public int GreenValue => greenValue;
    public int BlueValue => blueValue;
}
