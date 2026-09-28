using UnityEngine;

// Animação por troca de frames (sprite sheet gerado por código).
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    public float fps = 8f;

    SpriteRenderer sr;
    Sprite[] clip;
    float timer;
    int frame;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Play(Sprite[] newClip)
    {
        if (newClip == clip) return;
        clip = newClip;
        frame = 0;
        timer = 0f;
        sr.sprite = clip[0];
    }

    void Update()
    {
        if (clip == null || clip.Length == 0) return;
        timer += Time.deltaTime;
        if (timer >= 1f / fps)
        {
            timer = 0f;
            frame = (frame + 1) % clip.Length;
            sr.sprite = clip[frame];
        }
    }
}
