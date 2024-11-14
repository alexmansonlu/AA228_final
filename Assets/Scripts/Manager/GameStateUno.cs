using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameStateUno
{
    public int DeckCardCount;
    public List<int> OtherPlayersHandCardCounts; 
    public List<UnoCardData> PlayerHandCards; 
    public List<UnoCardData> PublicPile; 
    public int CurrentColor;


    public GameStateUno(int deckCardCount, List<int> otherPlayersHandCardCounts, List<UnoCardData> playerHandCards, List<UnoCardData> publicPile, int currentColor = 0)
    {
        DeckCardCount = deckCardCount;
        OtherPlayersHandCardCounts = otherPlayersHandCardCounts;
        PlayerHandCards = playerHandCards;
        PublicPile = publicPile;
        CurrentColor = currentColor;
    }

    // Method to log the state
    public void LogState()
    {
        string stateInfo = $"Deck Card Count: {DeckCardCount}\n";

        stateInfo += "Other Players' Hand Card Counts: ";
        stateInfo += string.Join(", ", OtherPlayersHandCardCounts);
        stateInfo += "\n";

        stateInfo += "Current Player's Hand Cards:\n";
        foreach (var card in PlayerHandCards)
        {
            stateInfo += $"- {card.color} {card.value}\n";
        }

        stateInfo += "Public Pile:\n";
        foreach (var card in PublicPile)
        {
            stateInfo += $"- {card.color} {card.value}\n";
        }

        stateInfo += $"Current Color: {CurrentColor}\n";

        // Use Debug.Log to print the information
        Debug.Log(stateInfo);

       
    }

    public string EncodeState()
    {
        // Serialize the object to a JSON string
        return JsonUtility.ToJson(this);
    }
}

