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
    [SerializeField] PlayerType myPlayerType = PlayerType.Human;


    // tcp server
    private TCPServer tcpServer;



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
        // open tcp server
        // Start the TCP server on port 8080
        tcpServer = gameObject.AddComponent<TCPServer>();
        tcpServer.StartServer(8080,this);



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
        player = new Player(handAreas[0],myPlayerType, "Me");
        all_players.Add(player);
        //player.DrawInitialHand(deck, cardPrefab, 7); // Draw 7 cards as starting hand

        string[] playerNames = {"Player A", "Player B"};
        for (int i = 0; i < 2; i++){
            Player player_simulated = new Player(handAreas[i+1],PlayerType.AI_Random, playerNames[i]);
            all_players.Add(player_simulated);
        }

        foreach (Player player in all_players)
        {
            player.DrawInitialHand(deck, cardPrefab, 7); // Draw 7 cards as starting hand
        }

        if(player.playerType != PlayerType.AI_RL){
            StartTurn(); //don't start the turn when player is RL, wait until the server is connected
        }
        else{
            Debug.Log("Waiting for server connection...");
        }

        
    }

    // start the game as RL
    public void StartRLGame(){
        if(player.playerType == PlayerType.AI_RL){
            StartTurn(); 
        }
        else{
            Debug.LogWarning("Player is not AI_RL, no need to connect the server");
        }
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

        // below is for drawing until a playable card rules!!!
        // while (no_card_to_play){
        //     foreach (GameObject cardObject in current_player.HandCardObjects)
        //     {
        //         Card cardComponent = cardObject.GetComponent<Card>();
        //         cardComponent.isPlayable = CheckPlayability(cardComponent);
        //         if(no_card_to_play && cardComponent.isPlayable){
        //             no_card_to_play = false;
        //         }

        //         if(current_player.playerType == PlayerType.Human || current_player.playerType == PlayerType.AI_RL){
        //             //Debug.Log("can play: " + cardComponent.isPlayable);
        //             cardObject.GetComponentInChildren<CardGUI>().SetCanPlay(cardComponent.isPlayable);
        //         }
        //     }

        //     if (no_card_to_play){
        //         Debug.Log(current_player.name+" has no playable cards. Drawing a card.");
        //         current_player.DrawCard(deck, cardPrefab);

        //     }
        // }

        //Rule of drawing one card if no playable card, if the card can be played it will automatically play it, if not skip the turn
        foreach (GameObject cardObject in current_player.HandCardObjects)
        {
            Card cardComponent = cardObject.GetComponent<Card>();
            cardComponent.isPlayable = CheckPlayability(cardComponent);
            if(no_card_to_play && cardComponent.isPlayable){
                no_card_to_play = false;
            }

            if(current_player.playerType == PlayerType.Human || current_player.playerType == PlayerType.AI_RL){
                //Debug.Log("can play: " + cardComponent.isPlayable);
                cardObject.GetComponentInChildren<CardGUI>().SetCanPlay(cardComponent.isPlayable);
            }
        }

            if (no_card_to_play){
                Debug.Log(current_player.name+" has no playable cards. Drawing a card.");
                current_player.DrawCard(deck, cardPrefab);
                //check the last card
                Card lastCard = current_player.HandCardObjects[current_player.HandCardObjects.Count-1].GetComponent<Card>();
                lastCard.isPlayable = CheckPlayability(lastCard);
                if (!lastCard.isPlayable){
                    //skip turn if the last card is not playable
                    Debug.Log(current_player.name+ " draw a card but it is not playable. Skipping turn.");
                    StartTurn();
                    return;
                }

            }
        
        if (current_player.playerType == PlayerType.Human){
            // Human player play HERE
            
            // maybe provided the state here
            GameStateUno gameState = getGameState();
            gameState.LogState();

        }

        if (current_player.playerType == PlayerType.AI_RL){
            // Random AI play HERE
            GameStateUno gameState = getGameState();
            gameState.LogState();
            string encodedState = gameState.EncodeState();

            // here please call the function to send TCP 
            // Send encoded data over TCP
            Debug.Log("Sending encoded state: " + encodedState);
            tcpServer.SendData(encodedState);
            Debug.Log("Waiting for response...");
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

    public void EndGame(){
        Debug.Log("Game Over");
    }

    // Function to play a card and add it to the public pile
    public void PlayCard(Card card, UnoColor chosenColor = UnoColor.Wild)
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
            
            //check win
            if (cardplayer.HandCardObjects.Count == 0){
                Debug.Log("Player " + cardplayer.name + " wins!");
                EndGame();
                return;
            }

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
                    else if (cardplayer.playerType == PlayerType.AI_RL){
                        colorPick(chosenColor);
                    }

                }
                else{
                    colorPickerManager.updateColorIndicator(((UnoCard)card).color);
                    Destroy(card.gameObject); // Destroy the card GameObject
                    // After a successful play, draw a new card if the deck isn't empty


                    StartTurn(); // Start the next turn


                }

                if (cardplayer.playerType == PlayerType.Human || cardplayer.playerType == PlayerType.AI_RL){
                    cardplayer.TidyHand();
                }

                
            }

            

        }
        else
        {
            Debug.LogWarning("ERROR: This card cannot be played. Please check the RL model");

            // maybe resent the state to the server
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

        List<UnoCardData> publicCards = new List<UnoCardData>();
        foreach (CardData cd in publicPile){
            publicCards.Add((UnoCardData)cd);
        }

        //get current color and valu
        UnoCardData lastCardData = publicPile[publicPile.Count - 1] as UnoCardData;
        int currentColor = (int)saved_Color; // Cast saved_Color to int
        



        return new GameStateUno(deckCardCount, otherPlayersHandCardCounts, playerHandCards, publicCards,currentColor);
    }


    // function for our agent to play card:
    public void AutoPlayCard(int cardIndex, int colorIndex){
        Debug.Log("Response recieved: Auto play card called with index: " + cardIndex + " and color index: " + colorIndex);
        // only for ai rl
        if(current_player.playerType != PlayerType.AI_RL){
            Debug.LogWarning("ERROR: Is not an AI_RL player!");
            return;
        }

        Card card = current_player.HandCardObjects[cardIndex].GetComponent<Card>();
        PlayCard(card, (UnoColor)Enum.GetValues(typeof(UnoColor)).GetValue(colorIndex));

    }
}
