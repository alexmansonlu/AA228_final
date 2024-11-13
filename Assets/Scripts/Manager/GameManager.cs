using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Photon.Pun.UtilityScripts;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Enum to specify the current game type
    public enum GameType { Uno, Poker, BigTwo }
    public GameType currentGame = GameType.Uno;

    // Deck of cards and public card pile
    public List<CardData> deck = new List<CardData>(); // All cards in the deck
    public List<CardData> publicPile = new List<CardData>(); // Cards played on the table
    public List<GameObject> publicCardObjects = new List<GameObject>();

    // Player information
    public List<Player> all_players = new List<Player>();
    private Player player; // The single player in this game
    private int turnCount = 0; // Tracks how many turns have been played

    //current player
    Player current_player;

    [SerializeField] private GameObject cardPrefab; // Prefab for card GameObjects
    [SerializeField] private  List<Transform> handAreas; // UI area to display player's hand
    [SerializeField] private GameObject publicCardPrefab; // Prefab for public card GameObjects
    [SerializeField] private Transform publicCardArea; 

    UnoColor saved_Color = UnoColor.Red;

    [SerializeField] ColorPickerManager colorPickerManager;






    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializeGame();
    }

    private void InitializeGame()
    {
        if (currentGame == GameType.Uno)
        {
            SetupUnoGame();
        }
        // Additional setup for other games can go here
    }

    private void SetupUnoGame()
    {
        // Initialize Uno deck
        deck = UnoCard.GenerateDeck();
        ShuffleDeck();

        // Initialize public pile as empty at the start of the game
        publicPile = new List<CardData>();

        // Draw initial card for the public pile (if needed)
        //DrawInitialPublicCard();

        // Initialize player with a hand of Uno cards
        player = new Player(handAreas[0],PlayerType.Human, "Me");
        all_players.Add(player);
        //player.DrawInitialHand(deck, cardPrefab, 7); // Draw 7 cards as starting hand

        string[] playerNames = {"Player A", "Player B", "Player C"};
        for (int i = 0; i < 3; i++){
            Player player_simulated = new Player(handAreas[i+1],PlayerType.AI_Random, playerNames[i]);
            all_players.Add(player_simulated);
        }

        foreach (Player player in all_players)
        {
            player.DrawInitialHand(deck, cardPrefab, 7); // Draw 7 cards as starting hand
        }

        StartTurn();
    }

    private void DrawInitialPublicCard()
    {
        if (deck.Count > 0)
        {
            CardData initialCard = deck[0];
            deck.RemoveAt(0);
            publicPile.Add(initialCard);
        }
    }

    public void StartTurn()
    {   
        turnCount++;
        
        // draw card if needed(not for uno)
        // if (deck.Count > 0)
        // {   
        //     all_players[turnCount % all_players.Count].DrawCard(deck, cardPrefab);
        //     //player.DrawCard(deck, cardPrefab);
        // }

        // check current player's hand for playable cards
        current_player = all_players[turnCount % all_players.Count];
        Debug.Log(current_player.name+" turn!");
        Boolean no_card_to_play = true;
        // remove while loop for other rules
        while (no_card_to_play){
            foreach (GameObject cardObject in current_player.HandCardObjects)
            {
                Card cardComponent = cardObject.GetComponent<Card>();
                cardComponent.isPlayable = CheckPlayability(cardComponent);
                if(no_card_to_play && cardComponent.isPlayable){
                    no_card_to_play = false;
                }

                if(current_player.playerType == PlayerType.Human){
                    //Debug.Log("can play: " + cardComponent.isPlayable);
                    cardObject.GetComponentInChildren<CardGUI>().SetCanPlay(cardComponent.isPlayable);
                }
            }

            if (no_card_to_play){
                Debug.Log(current_player.name+" has no playable cards. Drawing a card.");
                current_player.DrawCard(deck, cardPrefab);
            }
        }
        
        if (current_player.playerType == PlayerType.Human){
            // Human player play HERE

            // maybe provided the state here
            Debug.Log(current_player.name+" has no playable cards. Drawing a card.");
            GameStateUno gameState = getGameState();
            gameState.LogState();
        }
        

        if (current_player.playerType == PlayerType.AI_Random){
            // Random AI play HERE
            while(true)
            {
                int randomIndex = UnityEngine.Random.Range(0, current_player.HandCardObjects.Count);
                Card randomCard = current_player.HandCardObjects[randomIndex].GetComponent<Card>();
                if (randomCard.isPlayable){
                    PlayCard(randomCard);
                    break;
                }  
            }
        }


    }

    // Playability check specific to Uno rules
    private bool CheckPlayability(Card card)
    {   
        if (currentGame == GameType.Uno){
            if (publicPile.Count == 0)
            {
                return true; // No public card means any card can be played
            }

            UnoCardData lastCardData = publicPile[publicPile.Count - 1] as UnoCardData;
            UnoCard currentCard = card as UnoCard;

            if (lastCardData != null && currentCard != null)
            {   
                Debug.Log("last card: " + lastCardData.color + " " + lastCardData.value);
                Debug.Log("current card: " + currentCard.color + " " + currentCard.value);
                if (currentCard.color == UnoColor.Wild)
                {
                    return true;
                }


                if(lastCardData.color == UnoColor.Wild)
                {
                    return currentCard.color == saved_Color;
                }
                else{
                    // Basic Uno rule: Check if the card color or value matches
                    return currentCard.color == lastCardData.color || currentCard.value == lastCardData.value;
                }

             
            }

            return false;
        }

        return false;
    }

    // Function to play a card and add it to the public pile
    public void PlayCard(Card card)
    {
        if (CheckPlayability(card))
        {
            // Add the card's data to the public pile
            publicPile.Add(card.cardData);
            GameObject publicCardObject = Instantiate(publicCardPrefab, publicCardArea);
            publicCardObject.GetComponentInChildren<CardGUI>().UpdateCardFace(card.cardData);
            publicCardObjects.Add(publicCardObject);

            Player cardplayer = card.owner;
            cardplayer.HandCardObjects.Remove(card.gameObject); // Remove card GameObject from player's hand
            

            Debug.Log($"{cardplayer.name} Played {card.GetType().Name} - {((UnoCard)card).color} {((UnoCard)card).value}");

            //post play card ability for uno
            if (currentGame == GameType.Uno){
                if (((UnoCard)card).color == UnoColor.Wild){
                    colorPickerManager.updateColorIndicator(UnoColor.Wild);
                    if(cardplayer.playerType == PlayerType.Human){
                        // gonna choose a color
                        colorPickerManager.showColorPicker();
                        Destroy(card.gameObject); // Destroy the card GameObject
                    }
                    else if (cardplayer.playerType == PlayerType.AI_Random){
                        // Random AI play HERE
                        int randomIndex = UnityEngine.Random.Range(0, 4);
                        colorPick((UnoColor)Enum.GetValues(typeof(UnoColor)).GetValue(randomIndex));
                    }

                }
                else{
                    colorPickerManager.updateColorIndicator(((UnoCard)card).color);
                    Destroy(card.gameObject); // Destroy the card GameObject
                    // After a successful play, draw a new card if the deck isn¡¦t empty


                    StartTurn(); // Start the next turn
                }

                if (cardplayer.playerType == PlayerType.Human){
                    cardplayer.TidyHand();
                }
            }

            

        }
        else
        {
            Debug.Log("ERROR: This card cannot be played.");
        }

        
    }

    // Helper to shuffle the deck
    private void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            CardData temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
    }

    public void colorPick(UnoColor color){
        saved_Color = color;
        Debug.Log("chosen color: " + saved_Color);
        colorPickerManager.updateColorIndicator(saved_Color);

        StartTurn();

    }


    public GameStateUno getGameState(){
        int deckCardCount = deck.Count;
        List<int> otherPlayersHandCardCounts = new List<int>();
        foreach (Player player in all_players){
            if (player != current_player){
                otherPlayersHandCardCounts.Add(player.HandCardObjects.Count);
            }
        }

        List<UnoCardData> playerHandCards = new List<UnoCardData>();
        foreach (GameObject go in current_player.HandCardObjects){
            playerHandCards.Add((UnoCardData)go.GetComponent<Card>().cardData);
        }
        return new GameStateUno(deckCardCount, otherPlayersHandCardCounts, playerHandCards);
    }
}
