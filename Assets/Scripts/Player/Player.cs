using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Player: MonoBehaviour
{
    // List of card GameObjects in the player's hand
    public List<GameObject> HandCardObjects { get; private set; }
    public Transform handArea;
    public PlayerType playerType;
    public string name;

    public Player(Transform handArea, PlayerType playerType, string name)
    {
        HandCardObjects = new List<GameObject>();
        this.handArea = handArea;
        this.playerType = playerType;
        this.name = name;
    }

    // Draws the initial hand of cards
    public void DrawInitialHand(List<CardData> deck, GameObject cardPrefab, int numberOfCards)
    {
        for (int i = 0; i < numberOfCards; i++)
        {
            DrawCard(deck, cardPrefab);
        }
        if (playerType == PlayerType.Human || playerType == PlayerType.AI_RL){
            TidyHand();
        }
        
    }

    // Draws a single card from the deck, assigns its type, and adds it to the player's hand
    public void DrawCard(List<CardData> deck, GameObject cardPrefab)
    {
        if (deck.Count > 0)
        {
            CardData cardData = deck[0]; // Retrieve card data (color, value, etc.)
            //UnityEngine.Debug.Log(name+" drawing card: " + cardData.cardType + " " + ((UnoCardData)cardData).value + " " + ((UnoCardData)cardData).color);
            deck.RemoveAt(0);

            // Instantiate the card prefab in the hand area
            GameObject cardObject = Object.Instantiate(cardPrefab, handArea);

            // Dynamically add the correct Card component based on the type in cardData
            Card cardComponent = null;
            switch (cardData.cardType)
            {
                case CardData.CardType.Uno:
                    cardComponent = cardObject.AddComponent<UnoCard>();
                    ((UnoCard)cardComponent).InitializeUnoCard((UnoCardData)cardData,this);
                    break;
                // Add additional cases here for other card types like Poker or BigTwo
            }

            cardObject.GetComponentInChildren<CardGUI>().UpdateCardFace(cardData);

            HandCardObjects.Add(cardObject); // Track the card GameObject in the hand
        }
        else{
            UnityEngine.Debug.LogError("Deck is empty");
        }

        if(playerType == PlayerType.Human || playerType == PlayerType.AI_RL){
            TidyHand();
        }
    }

    public void TidyHand()
    {
        int cardCount = HandCardObjects.Count;
        if (cardCount == 0) return;

        // Calculate spacing based on the number of cards
        float cardWidth = 100f; // Assume each card has a width of 100 units; adjust as needed
        float handWidth = (cardCount - 1) * cardWidth;
        float startX = -handWidth / 2;

        // Position each card with calculated spacing
        for (int i = 0; i < cardCount; i++)
        {
            GameObject cardObject = HandCardObjects[i];
            RectTransform cardTransform = cardObject.GetComponent<RectTransform>();
            float xPos = startX + i * cardWidth;

            // Set position with the calculated x offset and a fixed y value
            cardTransform.anchoredPosition = new Vector2(xPos, 0);
            cardTransform.localRotation = Quaternion.identity; // Reset rotation if needed
        }
    }

    public void resetHand(){
        UnityEngine.Debug.Log(HandCardObjects.Count);
        foreach (GameObject cardObject in HandCardObjects){
            Destroy(cardObject);
            //UnityEngine.Debug.Log("hello 1"+name);
        }
        HandCardObjects.Clear();
    }
}
