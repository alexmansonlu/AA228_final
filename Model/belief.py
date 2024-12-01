from enum import Enum
from collections import defaultdict
import numpy as np
from typing import List

class UnoColor(Enum):
    RED = "Red"
    BLUE = "Blue"
    GREEN = "Green"
    YELLOW = "Yellow"
    WILD = "Wild"  # Wildcards have no specific color


class UnoType(Enum):
    NUMBER = "Number"  # Represents cards with numbers 0-9
    REVERSE = "Reverse"
    SKIP = "Skip"
    PLUS_TWO = "+2"
    WILD = "Wild"
    WILD_PLUS_FOUR = "Wild +4"


class UnoCard:
    def __init__(self, color: UnoColor, card_type: UnoType, value: int = None):
        """
        UnoCard represents a card in Uno with a color and a type.

        Args:
            color (UnoColor): The color of the card (Red, Blue, Green, Yellow, or Wild).
            card_type (UnoType): The type of the card (Number, Reverse, Skip, etc.).
            value (int, optional): The numerical value of the card (0-9), applicable only for NUMBER type.
        """
        self.color = color
        self.card_type = card_type
        self.value = value

    def __eq__(self, other):
        if not isinstance(other, UnoCard):
            return False
        return (
            self.color == other.color
            and self.card_type == other.card_type
            and self.value == other.value
        )

    def __hash__(self):
        return hash((self.color, self.card_type, self.value))

    def __repr__(self):
        if self.card_type == UnoType.NUMBER:
            return f"{self.color.value} {self.value}"
        return f"{self.color.value} {self.card_type.value}"
    
class Belief:
    def __init__(self):
        """
        Initializes the belief system with a recording deck and sets the opponent's hand card count.

        Args:
            opponent_hand_card_count (int): The initial number of cards in the opponent's hand.
        """
        self.remaining_deck_count = self._generate_recording_deck()
        self.beta_distributions = {card: [1, 1] for card in self.remaining_deck_count.keys()}
        self.cannot_play_flags = defaultdict(bool)  # Tracks cards the opponent cannot play.

        self.cannot_play_weight = 0.3

    def _generate_recording_deck(self) -> defaultdict:
        """
        Generates the recording deck for Uno with the correct card counts.

        Returns:
            defaultdict: A dictionary with UnoCard as keys and their counts as values.
        """
        deck = defaultdict(int)
        colors = ["Red", "Blue", "Green", "Yellow"]

        # Add number cards (0-9)
        for color in colors:
            deck[(color, "Number", 0)] = 1  # One "0" per color
            for number in range(1, 10):  # Two copies of each number card (1-9)
                deck[(color, "Number", number)] = 2

        # Add special action cards (Reverse, Skip, +2)
        for color in colors:
            deck[(color, "Reverse")] = 2
            deck[(color, "Skip")] = 2
            deck[(color, "+2")] = 2

        # Add wildcards
        deck[("Wild", "Wild")] = 4
        deck[("Wild", "Wild+4")] = 4

        return deck
    
    def expose_card(self, card):
        """
        Exposes a card to the belief system, updating the remaining deck count.

        Args:
            card: The UnoCard that is revealed (e.g., played or drawn).
        """
        if card in self.remaining_deck_count and self.remaining_deck_count[card] > 0:
            self.remaining_deck_count[card] -= 1
        else:
            raise ValueError(f"Card {card} is already fully exposed or not in the deck!")
        
        # Adjust beta distribution: Remove the possibility of having that card [too much I think]
        # if card in self.beta_distributions:
        #     self.beta_distributions[card][1] += 1  # Increment "don't have" side

    def update_cannot_play(self, top_card, hand_card_count):
        """
        Updates beta distributions and flags when the opponent cannot play after a certain card.

        Args:
            top_card: The card on the top of the discard pile that the opponent cannot play.
            hand_card_count: The number of cards the opponent has when they cannot play.
        """
        color, card_type, value = top_card


        # Mark all related cards as "cannot play"
        for card in self.remaining_deck_count.keys():
            card_color, card_type_, card_value = card
            if card_color == color or card_type_ == card_type or card_value == value or card_color == "Wild":
                self.cannot_play_flags[card] = True
                self.beta_distributions[card][1] += hand_card_count * self.cannot_play_weight  # Add to "don't have" side.  # can have a weight here

    def update_draw(self):
        """
        Resets the cannot play flags and updates beta distributions for drawn cards.
        """
        total_remaining = sum(self.remaining_deck_count.values())
        if total_remaining == 0:
            raise ValueError("No cards left in the deck to draw from.")

        # Reset cannot play flags
        self.cannot_play_flags = defaultdict(bool)

        # Update the "have" side of the beta distributions
        for card, count in self.remaining_deck_count.items():
            probability = count / total_remaining
            self.beta_distributions[card][0] += probability

    def sample_hand(self, hand_card_count):
        """
        Samples a possible hand for the opponent based on beta distributions and remaining deck.

        Returns:
            List: A list of sampled cards representing the opponent's hand.
        """
        hand = []
        temp_deck = self.remaining_deck_count.copy()
        temp_probs = {}

        # Filter out impossible cards
        for card, (have, dont_have) in self.beta_distributions.items():
            if self.cannot_play_flags[card] or temp_deck[card] == 0:
                temp_probs[card] = 0
            else:
                beta_sample = np.random.beta(have, dont_have)
                temp_probs[card] = beta_sample * (temp_deck[card] / sum(temp_deck.values()))

        # Normalize probabilities
        total_prob = sum(temp_probs.values())
        if total_prob > 0:
            temp_probs = {card: prob / total_prob for card, prob in temp_probs.items()}

        # Sample cards for the hand
        for _ in range(hand_card_count):
            if not temp_probs:
                break

            sampled_card = np.random.choice(list(temp_probs.keys()), p=list(temp_probs.values()))
            hand.append(sampled_card)

            # Update temporary deck and probabilities
            temp_deck[sampled_card] -= 1
            if temp_deck[sampled_card] == 0:
                del temp_probs[sampled_card]
            else:
                temp_probs[sampled_card] *= temp_deck[sampled_card] / sum(temp_deck.values())
                temp_probs = {card: prob / sum(temp_probs.values()) for card, prob in temp_probs.items()}

        return hand

    def __repr__(self):
        """Provides a summary of the belief."""
        remaining_cards = {card: count for card, count in self.remaining_deck_count.items() if count > 0}
        return f"Opponent Hand Card Count: {self.opponent_hand_card_count}\n" \
               f"Remaining Deck Count: {remaining_cards}\n" \
               f"Cannot Play Flags: {self.cannot_play_flags}\n" \
               f"Beta Distributions: {self.beta_distributions}"