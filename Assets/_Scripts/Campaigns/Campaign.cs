using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Vision
{
    [Header("Vision Info")]
    public string name;
    public int seed;
    public Sprite image;
    public bool useSaturation;
    public float previewDuration;
    public int requiredStars;
    public int targetTimeInSeconds;
    public int bestTimeInSeconds;
    public int bestNumberOfMoves;
    public int targetNumberOfMoves;
    public bool[] starsToEarn;

    [Header("Slicing Info")]
    public FragmentCreator.FragmentShapes fragmentShapes;
    //public int numberOfFragments; TODO Add to dictate number of fragments
    [Header("Rectangles")]
    public int rectangleColumns;
    public int rectangleRows;
    public float rectangleChanceToSplit;
    public float minWorldSize;
    public float minPercentSize;
    public float maxPercentSize;
    [Header("Tri & Quad")]
    public bool randomizePoints;
    public List<Vector2> vertices;
    public float minVertexDistanceFromBounds;
    public float vertexRadius;
    public bool debugVertices;
}
[CreateAssetMenu]
public class Campaign : ScriptableObject
{
    public string campaignName;
    public int currentVision;
    public Vision[] visions;

    public Vision GetCurrentVision()
    {
        return visions[currentVision];
    }


}
