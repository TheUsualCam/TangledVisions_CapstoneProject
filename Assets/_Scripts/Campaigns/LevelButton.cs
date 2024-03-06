using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{

    public Sprite puzzleImage;
    public Texture2D gameTexture;
    
    Image buttonImage;

    public LevelManager levelManager;


    // Start is called before the first frame update
    void Start()
    {
        buttonImage = GetComponent<Image>();                   
        buttonImage.sprite = puzzleImage;
    }

    public void UpdateGameImage()
    {
        levelManager.UpdateLevelImage(puzzleImage);
    }
}
