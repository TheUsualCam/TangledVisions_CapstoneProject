using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePolygon : MonoBehaviour
{
    [SerializeField] private int numberOfPoints;
    [SerializeField] private Vector2 radiusRange;

    private SpriteRenderer spriteRenderer;

    
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        //Must be at least 3 numberOfInternalPoints.
        numberOfPoints = Mathf.Max(numberOfPoints, 3);
        ChangeSprite();
    }
    void ChangeSprite()
    {
        //Fetch the Sprite and vertices from the SpriteRenderer
        Sprite sprite = spriteRenderer.sprite;
        sprite = Sprite.Create(sprite.texture, new Rect(0.0f, 0.0f, sprite.texture.width, sprite.texture.height), sprite.pivot / sprite.pixelsPerUnit, sprite.pixelsPerUnit);
        spriteRenderer.sprite = sprite;

        //Set the sprite polygon vertices to the new count
        Vector2[] spriteVertices = new Vector2[numberOfPoints + 1];

        for (int i = 0; i < spriteVertices.Length; i++)
        {
            if (i == spriteVertices.Length - 1)
            {
                spriteVertices[i].x = 0;
                spriteVertices[i].y = 0;
                break;
            }
            
            spriteVertices[i].x = i;
            spriteVertices[i].y = i == 2 ? 0 : i;
            
        }
        //Override the geometry with the new vertices
        sprite.OverrideGeometry(spriteVertices, sprite.triangles);
    }



}
