using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public UnityEvent<Vector2> OnMovementInput, OnPointerInput;
    public UnityEvent OnAttack;
    public UnityEvent OnDash, OnSkill, OnPause;

    [SerializeField]
    private InputActionReference movement, attack, pointerPosition, dash, skill, pause;

    private void Update()
    {
        OnMovementInput?.Invoke(movement.action.ReadValue<Vector2>().normalized);
        OnPointerInput?.Invoke(GetPointerInput());
    }

    private Vector2 GetPointerInput()
    {
        Vector3 mousePos = pointerPosition.action.ReadValue<Vector2>();
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    private void OnEnable()
    {
        attack.action.performed += PerformAttack; // Subscribe to attack input
        dash.action.performed += PerformDash;     // Subscribe to dash input
        skill.action.performed += PerformSkill;   // Subscribe to skill input
        pause.action.performed += PerformPause;
    }
    private void OnDisable()
    {
        attack.action.performed -= PerformAttack; // Unsubscribe to prevent memory leaks
        dash.action.performed -= PerformDash;     // Unsubscribe to prevent memory leaks
        skill.action.performed -= PerformSkill;   // Unsubscribe to prevent memory leaks
        pause.action.performed -= PerformPause;
    }

    private void PerformAttack(InputAction.CallbackContext obj)
    {
        OnAttack?.Invoke();
    }

    private void PerformDash(InputAction.CallbackContext obj)
    {
        OnDash?.Invoke();
    }

    private void PerformSkill(InputAction.CallbackContext obj)
    {
        OnSkill?.Invoke();
    }

    private void PerformPause(InputAction.CallbackContext obj)
    {
        OnPause?.Invoke();
    }
}
