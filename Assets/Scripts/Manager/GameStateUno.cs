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
    public bool Clockwise;  // Gives us the order of play
   


    public GameStateUno(int deckCardCount, List<int> otherPlayersHandCardCounts, List<UnoCardData> playerHandCards, List<UnoCardData> publicPile, int currentColor = 0, bool order = true)
    {
        DeckCardCount = deckCardCount;
        OtherPlayersHandCardCounts = otherPlayersHandCardCounts;
        PlayerHandCards = playerHandCards;
        PublicPile = publicPile;
        CurrentColor = currentColor;
        Clockwise = order;
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

        stateInfo += $"Rotation of play is clockwise: {Clockwise}\n";

        // Use Debug.Log to print the information
        Debug.Log(stateInfo);

       
    }
}

