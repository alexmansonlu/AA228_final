import socket
import json
import time

class UnoCardData:
    def __init__(self, cardType, color, value):
        self.cardType = cardType
        self.color = color
        self.value = value

    def __repr__(self):
        return f"UnoCardData(cardType={self.cardType}, color={self.color}, value={self.value})"
    

class Action:
    def __init__(self, actionType, data):
        self.actionType = actionType
        self.data = data

    def to_dict(self):
        return {
            "actionType": self.actionType,
            "data": self.data
        }
    
    def __repr__(self):
        return (
            f"ActionType={self.actionType}, data={self.data} "


        )

class GameStateUno:
    def __init__(self, deck_card_count, other_players_hand_card_counts, player_hand_cards, public_pile, current_color):
        self.deck_card_count = deck_card_count
        self.other_players_hand_card_counts = other_players_hand_card_counts
        self.player_hand_cards = [UnoCardData(**card) for card in player_hand_cards]
        self.public_pile = [UnoCardData(**card) for card in public_pile]
        self.current_color = current_color

    def __repr__(self):
        return (
            f"GameStateUno(deck_card_count={self.deck_card_count}, "
            f"other_players_hand_card_counts={self.other_players_hand_card_counts}, "
            f"player_hand_cards={self.player_hand_cards}, "
            f"public_pile={self.public_pile}),"
            f"current_color={self.current_color}, "

        )

def decode_game_state(json_string):
    data = json.loads(json_string)
    return GameStateUno(
        deck_card_count=data["DeckCardCount"],
        other_players_hand_card_counts=data["OtherPlayersHandCardCounts"],
        player_hand_cards=data["PlayerHandCards"],
        public_pile=data["PublicPile"],
        current_color=data["CurrentColor"]
    )

def play_card(action_index, chosen_color_index = 0):
    # ACTION INDEX IS WHAT CARD TO PLAY
    # COLOR INDEX, 0 = RED, 1 = GREEN, 2 = BLUE, 3 = YELLOW
    action = Action("PlayCard", {"index": action_index, "colorIndex": chosen_color_index})
    print(action)
    return encode_game_action(action.to_dict())



def encode_game_action(action):
    return json.dumps(action)

def main():
    host = "localhost"  # Replace with your server's IP if necessary
    port = 8080         # Port used by the server
    connected = False
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as client_socket:
        while not connected:
            try:
                client_socket.connect((host, port))
                client_socket.setblocking(False)  # Make the socket non-blocking
                print("Connected to the server.")
                connected = True
            except (ConnectionRefusedError, socket.error) as e:
                print(f"Connection failed: {e}. Retrying in 2 seconds...")
                time.sleep(2)  # Wait before retrying

        while True:
            try:
                data = client_socket.recv(4096)
                if data:
                    json_string = data.decode("utf-8")
                    game_state = decode_game_state(json_string)
                    print("Received game state:")
                    print(game_state)

                    # return a action
                    action = play_card(0)
                    client_socket.sendall(action.encode("utf-8"))

            except BlockingIOError:
                # No data available; continue looping
                pass
            except KeyboardInterrupt:
                print("\nTerminating client.")
                break

if __name__ == "__main__":
    main()
