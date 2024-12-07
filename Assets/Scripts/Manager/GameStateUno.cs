using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameStateUno
{
    public int DeckCardCount;
    
    public List<int> OtherPlayersHandCardCounts;  // this is set as fake number since in rollout we ignore the card being drawn instead we jsut keep track of the final card number
    public int PlayerHandCardsCount;// same here

    public List<UnoCardData> OpponentAHandCards; 
    public List<UnoCardData> OpponentBHandCards; 
    public List<UnoCardData> PlayerHandCards; 
    public List<UnoCardData> PublicPile; 
    public int CurrentColor;
    
    public bool Clockwise;  // Gives us the order of play
   


    public GameStateUno(int deckCardCount, List<int> otherPlayersHandCardCounts, List<UnoCardData> playerHandCards, List<UnoCardData> publicPile, int currentColor = 0, bool order = true, List<UnoCardData> opponentAHandCards = null, List<UnoCardData> opponentBHandCards = null)
    {
        DeckCardCount = deckCardCount;
        OtherPlayersHandCardCounts = otherPlayersHandCardCounts;
        PlayerHandCards = playerHandCards;
        PublicPile = publicPile;
        CurrentColor = currentColor;
        Clockwise = order;

        OpponentAHandCards = opponentAHandCards;
        OpponentBHandCards = opponentBHandCards;
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

        stateInfo += "Opponent A's Hand Cards:\n";
        foreach (var card in OpponentAHandCards)
        {
            stateInfo += $"- {card.color} {card.value}\n";
        }

        stateInfo += "Opponent B's Hand Cards:\n";
        foreach (var card in OpponentBHandCards)
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

    // to prevent using teh same game state over loop
    public GameStateUno Clone()
    {
        // Create a new instance and copy the values over
        GameStateUno clone = new GameStateUno(
            this.DeckCardCount, 
            new List<int>(this.OtherPlayersHandCardCounts), 
            new List<UnoCardData>(this.PlayerHandCards), 
            new List<UnoCardData>(this.PublicPile), 
            this.CurrentColor, 
            this.Clockwise,
            new List<UnoCardData>(this.OpponentAHandCards),
            new List<UnoCardData>(this.OpponentBHandCards)
        );

        return clone;
    }
}

