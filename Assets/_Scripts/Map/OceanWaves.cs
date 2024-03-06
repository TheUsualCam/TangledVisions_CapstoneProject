using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanWaves : MonoBehaviour
{
    public GameObject wavePrefab;

    private SpriteRenderer spriteRenderer;
    private Bounds spriteBounds;

    public Vector2 waveSpawnInterval;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteBounds = spriteRenderer.bounds;
        InvokeRepeating("SpawnWave", 0, Random.Range(waveSpawnInterval.x, waveSpawnInterval.y));
    }

    void SpawnWave()
    {
        WaveMovement newWave = Instantiate(wavePrefab, transform).GetComponent<WaveMovement>();
        
        newWave.transform.position = new Vector3(Random.Range(spriteBounds.min.x * 1.1f, spriteBounds.max.x * 0.98f), Random.Range(spriteBounds.min.y * 1.1f, spriteBounds.max.y * 0.98f), transform.position.z);
        newWave.xMax = spriteBounds.max.x * 0.95f;

    }

    
}
