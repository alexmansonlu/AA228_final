using UnityEngine;

[System.Serializable]
public class UnoCardData : CardData
{
    public UnoColor color;
    public UnoValue value;

    public UnoCardData(UnoColor color, UnoValue value)
    {
        this.cardType = CardType.Uno;
        this.color = color;
        this.value = value;
    }
}
