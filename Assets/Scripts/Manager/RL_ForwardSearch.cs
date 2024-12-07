using System.Collections.Generic;
using UnityEngine;
using System;

/*
    Note from Justin: Hello! I am trying to make a very clear process of what we should get done before the project.
    Essentially, we are dropping the idea that our opponents' cards are hidden just to save time, we are going to assume we 
    can see our opponents' cards. We also are going to use Forward Search at a depth of d due to it being easier to implement. 
    If we are able to get a working RL implementation, great, feel free to do other things. 
    
    Until then, THE #1 PRIORITY SHOULD BE TO WORK ON THESE FUNCTIONS, or remove / add functions as necessary if I forgot 
    something. Please leave updates both in this comment as well as in the WhatsApp when something is implemented. The algorithm
    we are using regards pages 184 and 185 of the textbook. Thank you!

    ForwardSearch Wrapper Function: Not Started
    CheckPlayability: Not Started
    StateToActions: Initial Implementation Done
    Rollout: WIP
    CreatePolicy: Not Started
    RewardFunction: Not Started

    All terms in parentheses "()" of the function descriptions correspond to textbook parameters on pages 184 and 185.
*/



// Main class for our RL agent
public class RL_ForwardSearch:MonoBehaviour
{

    /*
        Will be used to maintain our policy (mapping from state to optimal action)
    */
    public Dictionary<GameStateUno, UnoCardData> policy;


    public UnoCardData ForwardSearch(GameStateUno currentState, int depth)
    {   
        // for the sake of belief space we need to make the first recursive call here TODO
        Debug.Log("Forward Search called with depth: " + depth);
        var (bestAction, utility) = ForwardSearchRecursive(currentState.Clone(), depth);  // depth 0, first action

       
        // Return the action with the highest utility
        return bestAction;
    }


    // Check if a card can be played (simplified for now)
    private bool CheckPlayability(GameStateUno state, UnoCardData card)
    {   
        if ( state.PublicPile.Count == 0)
        {
            return true; // No public card means any card can be played
        }
        UnoCardData lastCardData = state.PublicPile[state.PublicPile.Count - 1] as UnoCardData;
        if (lastCardData != null && card != null)
            {   
                // Debug.Log(all_players[(turnCount - 1) % all_players.Count] + " last card: " + lastCardData.color + " " + lastCardData.value);
                if (card.color == UnoColor.Wild)
                {
                    return true;
                }


                if(lastCardData.color == UnoColor.Wild)
                {
                    return (int)(card.color) == state.CurrentColor;
                }
                else{
                    // Basic Uno rule: Check if the card color or value matches
                    return card.color == lastCardData.color || card.value == lastCardData.value;
                }

             
            }

            return false;
    }

    // Get all cards that can be played given handcards
    private List<UnoCardData> GetPlayableCards(List<UnoCardData> handcards, GameStateUno state)
    {
        List<UnoCardData> playableCards = new List<UnoCardData>();

        foreach (var card in handcards)
        {
            if (CheckPlayability(state, card))
            {
                playableCards.Add(card);
            }
        }

        return playableCards;
    }

    // Apply a played card to the game state 
    // here playerTurn is 0 for our agent and 1 for opponent a and 2 for opponent b, 
    private GameStateUno ApplyAction(GameStateUno state, UnoCardData card, int playerTurn)
    {
        // Simplified: Update the game state as if the card was played
        state.PublicPile.Add(card);  // Place card on top of the pile
        state.PlayerHandCards.Remove(card);  // Remove card from hand

        if(card.color == UnoColor.Wild){
            // here we just assume the wildcard will choose random color (TODO need to solve it, but kinda complicated as it increase the branches a lot)
            int randomIndex = UnityEngine.Random.Range(0, 4);
            state.CurrentColor = (int)Enum.GetValues(typeof(UnoColor)).GetValue(randomIndex);
        }
        else{
            state.CurrentColor = (int)(card.color);
        }

        // here calculate the +2 and +4 if needed
        return state;
    }

    // Calculate the utility of the current state (after depth)
    private float CalculateUtility(GameStateUno state)
    {
        // here we need a algorithm TODO
        float utility = 0;
        utility += state.OtherPlayersHandCardCounts[0]; // opponent A
        utility += state.OtherPlayersHandCardCounts[1]; // opponent B
        utility -= state.PlayerHandCards.Count; // our agent card number

        return utility;
    }


    // Recursive function to do forward search
    private (UnoCardData, float) ForwardSearchRecursive(GameStateUno state, int depth)
    {
        if (depth == 0)  //for the last depth
        {
            return (null,CalculateUtility(state));  // Evaluate the utility of this state (stop rollout)
        }

        // Get all possible actions (cards you can play)
        List<UnoCardData> playableCards = GetPlayableCards(state.PlayerHandCards, state);
        //Debug.Log("Current playable cards are: ");
        // state.LogState();
        // string s = "Current playable cards are: ";
        // foreach(var card in playableCards){
        //     s += card.color + " " + card.value + ", ";
        // }
        // Debug.Log(s);
        
        float intermediateReward = 0f;

        if (playableCards.Count == 0){
            // we just draw once and give negative reward, and continue to do the roll out
            state.PlayerHandCardsCount++;
            intermediateReward = -20; // TODO: we need to adjust this number

            float futureUtility = RolloutLookahead(state.Clone(), depth);
            return (null,futureUtility+intermediateReward);

        }

        else{
            float bestUtility = float.MinValue;
            UnoCardData bestAction = null;  // To store the best card
            // find the best possible outcome
            foreach(var card in playableCards){
                GameStateUno newState = ApplyAction(state.Clone(), card, 0);
                float futureUtility = RolloutLookahead(newState.Clone(), depth);

                // here can add intermediate reward if required (for keeping resource in hand)

                if (futureUtility > bestUtility)
                {
                    bestUtility = futureUtility;
                    bestAction = card;
                }


            }
            return (bestAction, bestUtility);   // Return the calculated utility  and the action for this recursive step
        }


    }


    // Perform a rollout lookahead (simulate opponent's random play)
    // currently it is vanilla version where we ignore the reverse
    private float RolloutLookahead(GameStateUno state, int depth)
    {   

        float intermediateReward = 0;
        float totalUtility = 0;
        // Simulate random moves for opponents (assuming two opponents)
        List<UnoCardData> opponentAPlayableCards = GetPlayableCards(state.OpponentAHandCards,state);  // Get playable cards for opponent A
        if (opponentAPlayableCards.Count == 0){
            //draw a card and go on (here it is approximation only, because we ignore the card being drawn)
            // dealing with drawn card seem far too complicated

            intermediateReward += 10; // could change this number afterward or we can skip Intermediate Reward
            state.OtherPlayersHandCardCounts[0] += 1;

            // and go for player b
            List<UnoCardData> opponentBPlayableCards = GetPlayableCards(state.OpponentBHandCards,state);  // Get playable cards for opponent B

            // if playerB ALSO HAVE NO CARD TO PLAY
            if (opponentBPlayableCards.Count == 0){
                //draw a card and go on
                intermediateReward += 10f; // could change this number afterward or we can skip Intermediate Reward
                state.OtherPlayersHandCardCounts[1] += 1;

                // agent turns
                var (bestAction, futureUtility) = ForwardSearchRecursive(state.Clone(), depth - 1);  // Look ahead for next move
                float weight = 1f; //since there is only one possibility as both opponents got no card to play
                totalUtility += (futureUtility+intermediateReward) * weight;

            }
            else{
                foreach (var cardB in opponentBPlayableCards)
                {
                    GameStateUno tempStateB = ApplyAction(state.Clone(), cardB,2);  // Opponent B plays a card

                    // then it is player's turn
                    var (bestAction, futureUtility) = ForwardSearchRecursive(tempStateB.Clone(), depth - 1);  // Look ahead for next move

                    float weight = 1f/(opponentBPlayableCards.Count); // assume all playable card have equal possibility to be played
                    totalUtility += (futureUtility+intermediateReward) * weight;
                }
            }



        }
        else{
            foreach (var cardA in opponentAPlayableCards)
            {
                GameStateUno tempStateA = ApplyAction(state.Clone(), cardA,1);  // Opponent A plays a card

                List<UnoCardData> opponentBPlayableCards = GetPlayableCards(tempStateA.OpponentBHandCards,tempStateA);  // Get playable cards for opponent B
                if (opponentBPlayableCards.Count == 0){
                    //draw a card and go back to agent turn
                    intermediateReward += 10f; // could change this number afterward or we can skip Intermediate Reward
                    tempStateA.OtherPlayersHandCardCounts[1] += 1;
                    var (bestAction, futureUtility) = ForwardSearchRecursive(tempStateA.Clone(), depth - 1);  // Look ahead for next move
                    float weight = 1f/(opponentAPlayableCards.Count);
                    totalUtility += (futureUtility+intermediateReward) * weight;
                
                }
                else{
                    foreach (var cardB in opponentBPlayableCards){
                        GameStateUno tempStateB = ApplyAction(tempStateA.Clone(), cardB,2);  // Opponent B plays a card

                        var (bestAction, futureUtility) = ForwardSearchRecursive(tempStateB.Clone(), depth - 1);  // Look ahead for next move for agent

                        float weight = 1f/(opponentBPlayableCards.Count*opponentAPlayableCards.Count); // assume all playable card have equal possibility to be played

                        totalUtility += (futureUtility+intermediateReward) * weight;
                    }
                }

            }   
        }

        // Weight the future utility based on random opponent play
        return totalUtility;
    }






    /*
        ForwardSearch: From a state, a depth, and a utility, return our most optimal action (which card 
        maximizes our utility). Should call rollout based on how the textbook does.
        
        Inputs: 
        - GameStateUno state (s)
        - int depth (d)
        - float utility (U)

        Outputs:
        - UnoCardData card (best)
    */
    // public UnoCardData ForwardSearch(GameStateUno state, int depth, float utility)
    // {
    //     // TODO: Implement forward search logic here

    //     UnoCardData best = null;
    //     return best;
    // }



    /*
        CheckPlayability: Given a card current_card and the last card played, check whether our card is playable. This will be
        used to build up our possible actions.
        
        Inputs: 
        - current_card [current card we are testing]
        - last_card [last card of the public pile, card we need to play off of]

        Outputs:
        - boolean on whether the card is playable or not
    */
    // public bool CheckPlayability(UnoCardData current_card, UnoCardData last_card)
    // {
    //     // TODO: Implement a check for whether our current card is playable based on the last card of the deck 
    //     return false;
    // }



    /*
        StateToActions: Get a list of all playable cards (our actions). This should be done by calling 
        CheckPlayability for each card in our hand.
        
        Inputs: 
        - GameStateUno state (s)

        Outputs:
        - List<UnoCardData> actions (a)- This is a list of all playable cards in our hand
    */
    // public List<UnoCardData> StateToActions(GameStateUno state)
    // {
    //     List<UnoCardData> actions = new List<UnoCardData>();

    //     foreach (UnoCardData card in state.PlayerHandCards)
    //     {
    //         UnoCardData last_card = state.PublicPile[state.PublicPile.Count - 1];
    //         if (CheckPlayability(state, card))
    //         {
    //             actions.Add(card);
    //         }
    //     }

    //     return actions;
    // }



    /*
        Rollout: Simulate the MDP version of the Uno game to a depth d, and accumulate expected reward.
        
        Inputs: 
        - GameStateUno state (s)
        - Dictionary<GameStateUno, UnoCardData> policy (pi)
            - [though i need to think about that more]
            - This is a public variable!
        - int depth (d)

        Outputs:
        - returned_reward (ret)
    */
    public float Rollout(GameStateUno state, int depth)
    {
        // TODO: Implement rollout logic here
        float returned_reward = 0.0f;
        return returned_reward;
    }

    

    /*
        CreatePolicy: Get action which maximizes our utility function given a policy. Can start as greedy but can change to
        include exploration if we have time.
        
        Inputs: 
        - GameStateUno state (s)

        Outputs:
        - greedy_action (a)
    */
    public UnoCardData CreatePolicy(GameStateUno state)
    {
        // TODO: Implement policy creation logic here
        UnoCardData greedy_action = null;
        return greedy_action;
    }



    /*
        RewardFunction: This is how we will reward state action pairs. We can implement a trivial policy such 
        as holding special cards and rewarding us for having a smaller amount of cards. Think about how to 
        implement this in code.

        Inputs:
        - GameStateUno state (s)
        - UnoCardData action (a)

        Outputs:
        - reward (Q(s, a))
    */
    public float RewardFunction(GameStateUno state, UnoCardData action) {

        float reward = 0.0f;

        if (state.PlayerHandCardsCount == 0) {
            return 1000f; 
        }

        // Penalize for # of cards in hand
        reward -= state.PlayerHandCardsCount * 5f;

        // use clockwise to skip if a player has 1 card 

        // if asked to draw 
        if (state.PublicPile[-1].value == UnoValue.DrawTwo || state.PublicPile[-1].value == UnoValue.WildDrawFour) {
            // reward if stacked 
            if (action.value == UnoValue.DrawTwo || action.value == UnoValue.WildDrawFour) {
                List<UnoCardData> next_player_cards = state.Clockwise ? state.OpponentAHandCards : state.OpponentBHandCards;
                reward += (10f / next_player_cards.Count); 
            }
            // penalize if forced to draw, penalized more if you have fewer cards 
            else {
                reward += (10f / state.PlayerHandCardsCount);
            }
        }
        // penalize playing +2, +4 if unecessary 
        else {
            if (action.value == UnoValue.DrawTwo || action.value == UnoValue.WildDrawFour) {
                reward -= 20f; 
            }
        }
        
        // penalize for playing wild card 
        if (action.color == UnoColor.Wild) {
            reward -= 20f; 
        }

        // penalize for # of cards opponent has
        foreach (int opponentCards in state.OtherPlayersHandCardCounts) {
            reward -= opponentCards * 5f; // Penalize for fewer opponent cards
        }
        return reward;
    }
}
