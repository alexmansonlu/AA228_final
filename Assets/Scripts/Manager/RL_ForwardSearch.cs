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
    public UnoCardData ForwardSearch(GameStateUno state, int depth, float utility)
    {
        // TODO: Implement forward search logic here

        UnoCardData best = null;
        return best;
    }



    /*
        CheckPlayability: Given a card current_card and the last card played, check whether our card is playable. This will be
        used to build up our possible actions.
        
        Inputs: 
        - current_card [current card we are testing]
        - last_card [last card of the public pile, card we need to play off of]

        Outputs:
        - boolean on whether the card is playable or not
    */
    public bool CheckPlayability(UnoCardData current_card, UnoCardData last_card)
    {
        // TODO: Implement a check for whether our current card is playable based on the last card of the deck 
        return false;
    }



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
