using UnityEngine;
using System.Collections.Generic;

public class UnoCard : Card
{

    public UnoColor color;

    public UnoValue value;

    public void InitializeUnoCard(UnoCardData data, Player owner)
    {
        color = data.color;
        value = data.value;
        isPlayable = false; // Initially set to false; GameManager will update this if playable
        cardData = data;
        this.owner = owner;
    }

    public static List<CardData> GenerateDeck()
    {
        List<CardData> deck = new List<CardData>();

        foreach (UnoColor color in System.Enum.GetValues(typeof(UnoColor)))
        {
            if (color == UnoColor.Wild) continue; // Skip wild cards for now

            foreach (UnoValue value in System.Enum.GetValues(typeof(UnoValue)))
            {   
                if (value == UnoValue.Wild || value == UnoValue.WildDrawFour) continue; // Skip wild cards for now
                // Each color has one Zero card and two of each other number
                int cardCount = value == UnoValue.Zero ? 1 : 2;

                for (int i = 0; i < cardCount; i++)
                {
                    deck.Add(new UnoCardData(color, value));
                }
            }
        }

        // Add Wild and WildDrawFour cards
        for (int i = 0; i < 4; i++)
        {
            deck.Add(new UnoCardData(UnoColor.Wild, UnoValue.Wild));
            deck.Add(new UnoCardData(UnoColor.Wild, UnoValue.WildDrawFour));
        }

        return deck;
    }
}
