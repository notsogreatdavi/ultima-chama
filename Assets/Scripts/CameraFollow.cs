using UnityEngine;

// Câmera com seguimento suave, antecipação na direção da mira e tremor (shake).
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    public PlayerController target;
    public float smooth = 8f;
    public float lookAhead = 0.25f;

    Camera cam;
    Vector3 basePos;
    float shake;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        basePos = transform.position;
    }

    public void Shake(float amount)
    {
        shake = Mathf.Max(shake, amount);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 goal = target.transform.position;
        if (target.Alive)
        {
            Vector3 aim = Inp.MouseWorld(cam) - goal;
            goal += Vector3.ClampMagnitude(aim * lookAhead, 2.5f);
        }
        goal.z = -10f;

        float t = 1f - Mathf.Exp(-smooth * Time.unscaledDeltaTime);
        basePos = Vector3.Lerp(basePos, goal, t);

        Vector3 offset = Vector3.zero;
        if (shake > 0f && Time.timeScale > 0f)
        {
            offset = (Vector3)(Random.insideUnitCircle * shake);
            shake = Mathf.MoveTowards(shake, 0f, Time.deltaTime * 2.5f);
        }
        transform.position = basePos + offset;
    }
}
