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
    private bool isDrawTwoActive = false; // Checks whether a draw two is active
    private bool isDrawFourActive = false;
    private int drawTwoPenalty = 0;
    private int drawFourPenalty = 0;

    //current player
    Player current_player;

    [SerializeField] private GameObject cardPrefab; // Prefab for card GameObjects
    [SerializeField] private  List<Transform> handAreas; // UI area to display player's hand
    [SerializeField] private GameObject publicCardPrefab; // Prefab for public card GameObjects
    [SerializeField] private Transform publicCardArea; 

    UnoColor saved_Color = UnoColor.Red;
    private bool reverse_flag = false;  // Tells us if we should go in counter-clockwise or clockwise ordering

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
        player = new Player(handAreas[0],myPlayerType, "Agent");
        all_players.Add(player);
        //player.DrawInitialHand(deck, cardPrefab, 7); // Draw 7 cards as starting hand

        string[] playerNames = {"Player A", "Player B"};
        for (int i = 0; i < 2; i++){
            Player player_simulated = new Player(handAreas[i+1],PlayerType.AI_Random, playerNames[i]);
            all_players.Add(player_simulated);

            // Player player_human = new Player(handAreas[i+1],PlayerType.AI_Random, playerNames[i]);
            // all_players.Add(player_human);
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
        if (reverse_flag == false) {
            turnCount++;  
        }

        else {
            turnCount--; // Go in reverse order for players
            if (turnCount < 0) {
                turnCount += all_players.Count;  // Have to do this to avoid negative turnCount indexing with reverse
            }
        }
        
        // draw card if needed(not for uno)
        // if (deck.Count > 0)
        // {   
        //     all_players[turnCount % all_players.Count].DrawCard(deck, cardPrefab);
        //     //player.DrawCard(deck, cardPrefab);
        // }

        // check current player's hand for playable cards
        current_player = all_players[turnCount % all_players.Count];
        Debug.Log(current_player.name+"'s turn!");
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
            GameStateUno gameState = getGameState();  // States
            gameState.LogState();

            // Call forward_search function here
            
            // Loop through every index and get the card that matches forward search's card. Do not return randomIndex, get correct Index
            // Card current_card = current_player.HandCardObjects[randomIndex].GetComponent<Card>();

            // PlayCard(current_card)

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
                // Debug.Log(all_players[(turnCount - 1) % all_players.Count] + " last card: " + lastCardData.color + " " + lastCardData.value);
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
        Player cardplayer = card.owner;
        UnoCardData last_card = null;
        if (publicPile.Count > 0) {
            last_card = (UnoCardData)publicPile[publicPile.Count - 1];
        }
        
        // Draw Two Logic Starts
        if(last_card != null && last_card.value == UnoValue.DrawTwo) {
            drawTwoPenalty += 2;
            isDrawTwoActive = true;

            if (((UnoCard)card).value != UnoValue.DrawTwo) {
                for (int i = 0; i < drawTwoPenalty; i++) {
                    current_player.DrawCard(deck, cardPrefab);
                }
                Debug.Log("Player " + current_player.name + " has drawn " + drawTwoPenalty + " cards!");
                drawTwoPenalty = 0;
                isDrawTwoActive = false;
            }
        }

        // Draw Four Logic Starts
        if(last_card != null && last_card.value == UnoValue.WildDrawFour) {
            drawFourPenalty += 4;
            isDrawFourActive = true;

            if (((UnoCard)card).value != UnoValue.WildDrawFour) {
                for (int i = 0; i < drawFourPenalty; i++) {
                    current_player.DrawCard(deck, cardPrefab);
                }
                Debug.Log("Player " + current_player.name + " has drawn " + drawFourPenalty + " cards!");
                drawFourPenalty = 0;
                isDrawFourActive = false;
            }
        }

        


        Debug.Log("Turn Counter: " + turnCount);
        if (CheckPlayability(card))
        {
            // Add the card's data to the public pile
            publicPile.Add(card.cardData);
            GameObject publicCardObject = Instantiate(publicCardPrefab, publicCardArea);
            publicCardObject.GetComponentInChildren<CardGUI>().UpdateCardFace(card.cardData);
            publicCardObjects.Add(publicCardObject);
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

                    Destroy(card.gameObject); // Haven't tested, could be buggy
                }
                else{

                    // Case of Skip
                    if (((UnoCard)card).value == UnoValue.Skip) {
                        if (reverse_flag) {
                            turnCount--;
                            if (turnCount < 0) {
                                turnCount += all_players.Count;  // Out of bounds check
                            }
                        }

                        else {
                            turnCount++;
                        }
                        
                        Debug.Log("Player " + all_players[turnCount % all_players.Count].name + " has been skipped!");
                    }

                    // Case of Reverse
                    if (((UnoCard)card).value == UnoValue.Reverse) {
                        if (reverse_flag) {
                            reverse_flag = false;
                            Debug.Log("Player " + cardplayer.name + " has reversed the direction of play! Direction of player: clockwise");
                        }

                        else {
                            reverse_flag = true;
                            Debug.Log("Player " + cardplayer.name + " has reversed the direction of play! Direction of player: counter-clockwise");
                        }
                    }

                    if (((UnoCard)card).value == UnoValue.DrawTwo) {
                        // do something
                    }



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
        List<int> isPlayable = new List<int>();
        foreach (GameObject go in current_player.HandCardObjects){
            playerHandCards.Add((UnoCardData)go.GetComponent<Card>().cardData);
            isPlayable.Add(go.GetComponent<Card>().isPlayable ? 1 : 0);
        }

        List<UnoCardData> publicCards = new List<UnoCardData>();
        foreach (CardData cd in publicPile){
            publicCards.Add((UnoCardData)cd);
        }

        //get current color and value
        UnoCardData lastCardData = publicPile[publicPile.Count - 1] as UnoCardData;
        int currentColor = (int)saved_Color; // Cast saved_Color to int

        // get direction of play
        bool is_clockwise = !reverse_flag;

        // See if we should be playing draw two or draw four
        UnoCardData last_card = null;
        if (publicPile.Count > 0) {
            last_card = (UnoCardData)publicPile[publicPile.Count - 1];
        }

        bool drawTwoActive = false;
        bool drawFourActive = false;

        if (last_card.value == UnoValue.DrawTwo) {
            drawTwoActive = true;
        }

        if (last_card.value == UnoValue.WildDrawFour) {
            drawFourActive = true;
        }


        return new GameStateUno(deckCardCount, otherPlayersHandCardCounts, playerHandCards, 
            publicCards,currentColor, is_clockwise, drawTwoActive, drawFourActive);
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
