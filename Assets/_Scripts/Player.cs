using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //private AgentAnimations agentAnimations;
    private AgentMover agentMover;

    private Vector2 pointerInput, movementInput;
    private Vector2 PointerInput => pointerInput;

    //private WeaponAgent weaponAgent;

    private void Awake()
    {
        agentMover = GetComponent<AgentMover>();
    }

    [SerializeField]
    private InputActionReference movement, attack, pointerPosition;

    private void Update()
    {
        pointerInput = GetPointerInput();

        movementInput = movement.action.ReadValue<Vector2>();

        agentMover.MovementInput = movementInput;   

    }

    private Vector2 GetPointerInput()
    {
        Vector3 mousePos = pointerPosition.action.ReadValue<Vector2>();
        mousePos.z = Camera.main.nearClipPlane;

        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
