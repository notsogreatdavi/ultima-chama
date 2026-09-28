using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Camada de input que funciona tanto com o Input System novo quanto com o Input Manager antigo.
public static class Inp
{
    public static Vector2 Move()
    {
        Vector2 v = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k != null)
        {
            if (k.aKey.isPressed || k.leftArrowKey.isPressed) v.x -= 1f;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) v.x += 1f;
            if (k.sKey.isPressed || k.downArrowKey.isPressed) v.y -= 1f;
            if (k.wKey.isPressed || k.upArrowKey.isPressed) v.y += 1f;
        }
#else
        v.x = Input.GetAxisRaw("Horizontal");
        v.y = Input.GetAxisRaw("Vertical");
#endif
        return v.sqrMagnitude > 1f ? v.normalized : v;
    }

    public static bool Fire()
    {
#if ENABLE_INPUT_SYSTEM
        bool mouse = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool key = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
        return mouse || key;
#else
        return Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
#endif
    }

    public static bool Dash()
    {
#if ENABLE_INPUT_SYSTEM
        bool mouse = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        bool key = Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame;
        return mouse || key;
#else
        return Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.LeftShift);
#endif
    }

    public static bool Nova()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        return k != null && (k.qKey.wasPressedThisFrame || k.eKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E);
#endif
    }

    public static bool Pause()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        return k != null && (k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
#endif
    }

    public static bool Confirm()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        return k != null && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
    }

    public static Vector3 MouseWorld(Camera cam)
    {
        Vector2 screen;
#if ENABLE_INPUT_SYSTEM
        screen = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
        screen = Input.mousePosition;
#endif
        Vector3 w = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
        w.z = 0f;
        return w;
    }
}
