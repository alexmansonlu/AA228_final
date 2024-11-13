using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateUno
{
    public int DeckCardCount { get; set; }
    public List<int> OtherPlayersHandCardCounts { get; set; }
    public List<UnoCardData> PlayerHandCards { get; set; }

    public GameStateUno(int deckCardCount, List<int> otherPlayersHandCardCounts, List<UnoCardData> playerHandCards)
    {
        DeckCardCount = deckCardCount;
        OtherPlayersHandCardCounts = otherPlayersHandCardCounts;
        PlayerHandCards = playerHandCards;
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

        // Use Debug.Log to print the information
        Debug.Log(stateInfo);
    }
}