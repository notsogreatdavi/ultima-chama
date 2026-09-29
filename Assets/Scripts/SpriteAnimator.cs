using System;
using UnityEngine;

// Toca clipes de quadros da SpriteFactory (em loop ou uma vez só).
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    public event Action Finished;

    public string Current { get; private set; }
    public bool Done { get; private set; }

    SpriteRenderer sr;
    Sprite[] clip;
    float fps = 10f;
    bool loop;
    float timer;
    int frame;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // keepFrame: troca de variação (ex.: expressão) sem reiniciar a animação.
    public void Play(string key, bool loop = true, bool restart = false, bool keepFrame = false)
    {
        if (!restart && key == Current) return;

        Current = key;
        clip = SpriteFactory.Clip(key);
        fps = SpriteFactory.Fps(key);
        this.loop = loop;
        Done = false;
        if (keepFrame && clip.Length > 0)
        {
            frame %= clip.Length;
        }
        else
        {
            frame = 0;
            timer = 0f;
        }
        if (clip.Length > 0) sr.sprite = clip[frame];
    }

    void Update()
    {
        if (clip == null || clip.Length == 0 || Done) return;

        timer += Time.deltaTime;
        float step = 1f / fps;
        while (timer >= step)
        {
            timer -= step;
            frame++;
            if (frame >= clip.Length)
            {
                if (loop)
                {
                    frame = 0;
                }
                else
                {
                    frame = clip.Length - 1;
                    Done = true;
                    sr.sprite = clip[frame];
                    Finished?.Invoke();
                    return;
                }
            }
        }
        sr.sprite = clip[frame];
    }
}
