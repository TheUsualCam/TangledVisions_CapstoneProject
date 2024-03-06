using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Serialization;

public class ImageSlicer : MonoBehaviour
{
    [SerializeField] public Texture2D sourceImage;
    [SerializeField] private int rows = 3;
    [SerializeField] private Vector2 rowWidthRange = new Vector3(0.1f, 0.4f);
    [SerializeField] private int columns = 3;
    [SerializeField] private Vector2 columnHeightRange = new Vector3(0.1f, 0.4f);
    [SerializeField] private GameObject jigsawPiecePrefab;
    [SerializeField] private List<Texture2D> pieceTextures = new List<Texture2D>();

    public void SliceImage(Texture2D imageToSlice, int numOfRows, int numOfColumns, Vector2 widthRange, Vector2 heightRange)
    {
        sourceImage = imageToSlice;
        
        rows = numOfRows;
        rowWidthRange = widthRange;
        
        columns = numOfColumns;
        columnHeightRange = heightRange;
        
        SliceImage();
    }
    public void SliceImage()
    {
        //Clear all children (Stops multiplying children)
        if (transform.childCount > 0)
        {
            for (int i = transform.childCount - 1; i > 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        //Hide the SourceImage
        GetComponent<SpriteRenderer>().enabled = false;
        
        // Ensure that the rows and columns are valid (Minimum of 1)
        rows = Mathf.Max(1, rows);
        columns = Mathf.Max(1, columns);

        // Calculate the size of each piece
        //This evenly splits the image, e.g. each piece will be the same dimensions.
        List<float> pieceWidthsNormalized = RandomDivider.DivideIntoRandomPieces(rows, rowWidthRange.x, rowWidthRange.y);
        List<float> pieceHeightsNormalized = RandomDivider.DivideIntoRandomPieces(columns, columnHeightRange.x, columnHeightRange.y);
        
        
        List<int> pieceWidthsInPixels = new List<int>();
        int widthSum = 0;
        List<int> sliceWidthStartingPos = new List<int>();

        List<int> pieceHeightsInPixels = new List<int>();
        int heightSum = 0;
        List<int> sliceHeightStartingPos = new List<int>();

        int difference = 0;
        
        //Create a list of Dimensions (Width) for each piece
        for(int i = 0; i < pieceWidthsNormalized.Count; i++)
        {
            pieceWidthsInPixels.Add((int)(sourceImage.width * pieceWidthsNormalized[i]));
            widthSum += pieceWidthsInPixels[i];
        }
        
        //Add any missing width due to float > integer conversion.
        if (widthSum < sourceImage.width) 
        {
            difference = (sourceImage.width - widthSum) / pieceWidthsInPixels.Count;
            for (int i = 0; i < pieceWidthsInPixels.Count; i++)
            {
                pieceWidthsInPixels[i] += difference;
            }
            widthSum += difference * pieceWidthsInPixels.Count;
        }
        
        sliceWidthStartingPos.Add(0);
        for (int i = 1; i < pieceWidthsInPixels.Count; i++)
        {
            sliceWidthStartingPos.Add(sliceWidthStartingPos[i - 1] + pieceWidthsInPixels[i - 1]);
        }
        
        //Create a list of Dimensions (Height) for each piece
        for(int i = 0; i < pieceHeightsNormalized.Count; i++)
        {
            pieceHeightsInPixels.Add((int)(sourceImage.height * pieceHeightsNormalized[i]));
            heightSum += pieceHeightsInPixels[i];
        }
        
        //Add any missing height due to float > integer conversion.
        if (heightSum < sourceImage.height)
        {
            difference = (sourceImage.height - heightSum) / pieceHeightsInPixels.Count;
            for (int i = 0; i < pieceHeightsInPixels.Count; i++)
            {
                pieceHeightsInPixels[i] += difference;
            }
            heightSum += difference * pieceHeightsInPixels.Count;
        }
        
        sliceHeightStartingPos.Add(0);
        for (int i = 1; i < pieceHeightsInPixels.Count; i++)
        {
            sliceHeightStartingPos.Add(sliceHeightStartingPos[i - 1] + pieceHeightsInPixels[i - 1]);
        }

        
        // Slice the image and create jigsaw pieces
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                // Create a new Texture2D for each piece
                Texture2D pieceTexture = new Texture2D(pieceWidthsInPixels[row], pieceHeightsInPixels[column]);
                pieceTextures.Add(pieceTexture);
                // Copy the pixels from the source image to the new texture
                Color[] pixels = sourceImage.GetPixels(sliceWidthStartingPos[row], sliceHeightStartingPos[column], pieceWidthsInPixels[row], pieceHeightsInPixels[column]);
                pieceTexture.SetPixels(pixels);
                pieceTexture.Apply();

                // Create a new GameObject for each jigsaw piece
                GameObject piece = Instantiate(jigsawPiecePrefab, transform);

                // Set the texture of the jigsaw piece
                SpriteRenderer renderer = piece.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    Sprite sprite = Sprite.Create(pieceTexture, new Rect(0, 0, pieceWidthsInPixels[row], pieceHeightsInPixels[column]), Vector2.one * 0.5f);
                    renderer.sprite = sprite;
                }
            
                // Position the jigsaw piece
                float posX = -columns + column * 2;
                float posY = -rows + row * 2;
                piece.transform.position = new Vector3(posX, posY, 0);
                
            }
        }
    }

    public List<Texture2D> GetPieceTextures() 
    { return pieceTextures; }
}
