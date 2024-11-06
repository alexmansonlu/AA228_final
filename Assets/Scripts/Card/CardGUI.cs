using UnityEngine;
using UnityEngine.UI;

public class CardGUI : MonoBehaviour
{
    public Image UnoColorBack; 
    public Image UnoNumberBack; 
    public Image CanPlayIndicator;

    // Updates the card face based on the card type and value
    public void UpdateCardFace(CardData cardData)
    {
        if (cardData is UnoCardData unoCard)
        {   
            if (unoCard.color == UnoColor.Wild)
            {
                Sprite numberSprite = LoadUnoNumberSprite(unoCard);
                UnoColorBack.enabled = false;
                UnoNumberBack.sprite = numberSprite;
            }
            else{
                Sprite colorSprite = LoadUnoColorSprite(unoCard);
                Sprite numberSprite = LoadUnoNumberSprite(unoCard);

                UnoColorBack.sprite = colorSprite;
                UnoNumberBack.sprite = numberSprite;
            }
        }

        CanPlayIndicator.enabled = false;
    }

    // Loads the appropriate sprite for an Uno card based on its color and value
    private Sprite LoadUnoColorSprite(UnoCardData unoCard)
    {
        string fileName = $"{unoCard.color}";
        //Debug.Log(fileName);
        return Resources.Load<Sprite>($"UnoCards/{fileName}_base");
    }

    // Loads the appropriate sprite for an Uno card based on its color and value
    private Sprite LoadUnoNumberSprite(UnoCardData unoCard)
    {
        string fileName = $"_{unoCard.value}";
        //Debug.Log(fileName);
        return Resources.Load<Sprite>($"UnoCards/{fileName}");
    }

    public void SetCanPlay(bool canPlay)
    {
        CanPlayIndicator.enabled = canPlay;
    }

}
