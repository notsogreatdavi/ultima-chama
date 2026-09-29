using UnityEngine;

// Ordena sprites pela altura na tela: quem está mais embaixo aparece na frente.
[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    public int baseOrder = 1000;
    public float offset;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        sr.sortingOrder = baseOrder - Mathf.RoundToInt((transform.position.y + offset) * 16f);
    }
}
