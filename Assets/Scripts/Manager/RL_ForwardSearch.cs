using System.Collections.Generic;
using UnityEngine;

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
public class RL_ForwardSearch
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
        // Simple check: card must match current color or value, or be a wild card
        if (card.color == state.CurrentColor || card.value == state.PublicPile[0].value || card.color == UnoColor.Wild)
        {
            return true;
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
        state.PublicPile.Insert(0,card);  // Place card on top of the pile
        state.PlayerHandCards.Remove(card);  // Remove card from hand

        if(card.color == UnoColor.Wild){
            // here we just assume the wildcard will choose random color (TODO need to solve it, but kinda complicated as it increase the branches a lot)
            int randomIndex = UnityEngine.Random.Range(0, 4);
            state.CurrentColor = (UnoColor)Enum.GetValues(typeof(UnoColor)).GetValue(randomIndex);
        }
        else{
            state.CurrentColor = card.color;
        }

        // here calculate the +2 and +4 if needed
        return state;
    }

    // Calculate the utility of the current state (after depth)
    private float CalculateUtility(GameStateUno state)
    {
        // here we need a algorithm TODO
        float utility = 0;
        utility += state.otherPlayersHandCardCounts[0]; // opponent A
        utility += state.otherPlayersHandCardCounts[1]; // opponent B
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
        intermediateReward = 0;

        if (playableCards.Count == 0){
            // we just draw once and give negative reward, and continue to do the roll out
            state.PlayerHandCardsCount++;
            intermdediateReward = -20; // TODO: we need to adjust this number

            float futureUtility = RolloutLookahead(state.Clone(), depth);
            return (null,futureUtility+intermediateReward);

        }

        else{
            float bestUtility = float.MinValue;
            UnoCardData bestAction = null;  // To store the best card
            // find the best possible outcome
            foreach(var card in playableCards){
                GameStateUno newState = ApplyAction(state.Clone(), currentAction, 0);
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

        int intermediateRewardA = 0;
        float totalUtility = 0;
        // Simulate random moves for opponents (assuming two opponents)
        List<UnoCardData> opponentAPlayableCards = GetPlayableCards(state.OpponentAHandCards,state);  // Get playable cards for opponent A
        if (opponentAPlayableCards.Count == 0){
            //draw a card and go on (here it is approximation only, because we ignore the card being drawn)
            // dealing with drawn card seem far too complicated

            intermediateRewardA += 10; // could change this number afterward or we can skip Intermediate Reward
            state.otherPlayersHandCardCounts[0] += 1;

            // and go for player b
            intermediateRewardB = intermediateRewardA;
            List<UnoCardData> opponentBPlayableCards = GetPlayableCards(state.OpponentBHandCards,state);  // Get playable cards for opponent B

            // if playerB ALSO HAVE NO CARD TO PLAY
            if (opponentBPlayableCards.Count == 0){
                //draw a card and go on
                intermediateReward += 10f; // could change this number afterward or we can skip Intermediate Reward
                state.otherPlayersHandCardCounts[1] += 1;

                // agent turns
                float futureUtility = ForwardSearchRecursive(state.Clone(), depth - 1);  // Look ahead for next move
                float weight = 1f; //since there is only one possibility as both opponents got no card to play
                totalUtility += futureUtility * weight;

            }
            else{
                foreach (var cardb in opponentBPlayableCards)
                {
                    GameStateUno tempStateB = ApplyAction(state.Clone(), cardb,2);  // Opponent B plays a card

                    // then it is player's turn
                    float futureUtility = ForwardSearchRecursive(tempStateB.Clone(), depth - 1);  // Look ahead for next move

                    float weight = 1f/(opponentBPlayableCards.Count); // assume all playable card have equal possibility to be played
                    totalUtility += futureUtility * weight;
                }
            }



        }
        else{
            foreach (var carda in opponentAPlayableCards)
            {
                GameStateUno tempStateA = ApplyAction(state.Clone(), cardA,1);  // Opponent A plays a card

                List<UnoCardData> opponentBPlayableCards = GetPlayableCards(tempStateA.OpponentBHandCards,tempStateA);  // Get playable cards for opponent B
                if (opponentBPlayableCards.Count == 0){
                    //draw a card and go back to agent turn
                    intermediateReward += 10f; // could change this number afterward or we can skip Intermediate Reward
                    tempStateA.otherPlayersHandCardCounts[1] += 1;
                    float futureUtility = ForwardSearchRecursive(tempStateA.Clone(), depth - 1);  
                    float weight = 1f/(opponentAPlayableCards.Count);
                    totalUtility += futureUtility * weight;
                
                }
                else{
                    for (var cardB in opponentBPlayableCards){
                        GameStateUno tempStateB = ApplyAction(tempStateA.Clone(), cardB,2);  // Opponent B plays a card

                        float futureUtility = ForwardSearchRecursive(tempStateB.Clone(), depth - 1);  // Look ahead for next move for agent

                        float weight = 1f/(opponentBPlayableCards.Count*opponentAPlayableCards.Count); // assume all playable card have equal possibility to be played

                        toitalUtility += futureUtility * weight;
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
    public List<UnoCardData> StateToActions(GameStateUno state)
    {
        List<UnoCardData> actions = new List<UnoCardData>();

        foreach (UnoCardData card in state.PlayerHandCards)
        {
            UnoCardData last_card = state.PublicPile[state.PublicPile.Count - 1];
            if (CheckPlayability(card, last_card))
            {
                actions.Add(card);
            }
        }

        return actions;
    }



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

        // TODO: Implement reward functionhere
        float reward = 0.0f;
        return reward;
    }

}
