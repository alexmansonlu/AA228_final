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
    public bool D2Active;  // Was a Draw 2 the last card? If so play Draw-2 no matter what if we have.
    public bool D4Active;  // Was a Draw 4 the last card? If so play Draw-4 no matter what if we have.
   


    public GameStateUno(int deckCardCount, List<int> otherPlayersHandCardCounts, List<UnoCardData> playerHandCards, 
        List<UnoCardData> publicPile, int currentColor = 0, bool order = true, bool d2Active = false, bool d4Active = false)
    {
        DeckCardCount = deckCardCount;
        OtherPlayersHandCardCounts = otherPlayersHandCardCounts;
        PlayerHandCards = playerHandCards;
        PublicPile = publicPile;
        CurrentColor = currentColor;
        Clockwise = order;
        D2Active = d2Active;
        D4Active = d4Active;
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

        stateInfo += $"Draw two is active: {D2Active}\n";

        stateInfo += $"Draw four is active: {D4Active}\n";

        // Use Debug.Log to print the information
        Debug.Log(stateInfo);

       
    }
}

