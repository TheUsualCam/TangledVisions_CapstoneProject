using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveMovement : MonoBehaviour
{
    public float xMax;
    public Vector2 durationRange;

    public float speed;

    Animator animator;
    SpriteRenderer spriteRenderer;
    public Sprite[] sprites;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
        Invoke("StartDestroy", Random.Range(durationRange.x, durationRange.y));
    }
    // Update is called once per frame
    void Update()
    {
        if (transform.position.x > xMax)
        {
            StartDestroy();
        }
        
        
        transform.Translate(Vector3.right * speed * Time.deltaTime);
        
    }

    void Destroy()
    {
        Destroy(gameObject);
    }

    void StartDestroy()
    {
        animator.SetTrigger("Destroy");
    }
}
