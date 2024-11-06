using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPickerManager : MonoBehaviour
{   
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject colorPickers;

    bool canPick = false;

    [SerializeField] Image colorIndicator;

    public void updateColorIndicator(UnoColor color){
        if (color == UnoColor.Wild){
            colorIndicator.color = Color.black;
        }
        else if (color == UnoColor.Red){
            colorIndicator.color = Color.red;
        }
        else if (color == UnoColor.Blue){
            colorIndicator.color = Color.blue;
        }
        else if (color == UnoColor.Green){
            colorIndicator.color = Color.green;
        }
        else if (color == UnoColor.Yellow){
            colorIndicator.color = Color.yellow;
        }
    }

    public void chooseBlue()
    {   
        if (canPick == false) return;
        colorPickers.SetActive(false);
        gameManager.colorPick(UnoColor.Blue);
        canPick = false;
    }

    public void chooseRed()
    {   
        if (canPick == false) return;
        colorPickers.SetActive(false);
        gameManager.colorPick(UnoColor.Red);
        canPick = false;
    }

    public void chooseGreen()
    {      
        if (canPick == false) return;
        colorPickers.SetActive(false);
        gameManager.colorPick(UnoColor.Green);
        canPick = false;
    }

    public void chooseYellow()    
    {
        if (canPick == false) return;
        colorPickers.SetActive(false);
        gameManager.colorPick(UnoColor.Yellow);
        canPick = false;
    }

    public void showColorPicker(){
        colorPickers.SetActive(true);
        canPick = true;

    }
}
