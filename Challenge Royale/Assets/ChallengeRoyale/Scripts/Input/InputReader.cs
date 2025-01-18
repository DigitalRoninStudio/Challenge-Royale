using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, InGameInputSystem.IInGameActions
{
    [SerializeField]public InGameInputSystem inputController;

    public Action<Vector2> OnMousePosition;
    public Action OnLeftClick;
    public Action OnRightClick;

    private void Awake()
    {
        inputController = new InGameInputSystem();
        inputController.InGame.SetCallbacks(this);
        inputController.InGame.Enable();
    }
    private void OnEnable()
    {
        inputController.Enable();
    }

    private void OnDisable()
    {
        inputController.Disable();
    }

    void InGameInputSystem.IInGameActions.OnMouseMove(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 screenPosition = context.ReadValue<Vector2>();
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                //Debug.Log("MOUSE POSSITION: " + "[" + hit.point.x + "," + hit.point.z + "]");
                OnMousePosition?.Invoke(new Vector2(hit.point.x, hit.point.z));
            }
        }
    }

    void InGameInputSystem.IInGameActions.OnLeftClick(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        OnLeftClick?.Invoke();
    }

    void InGameInputSystem.IInGameActions.OnRightClick(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        OnRightClick?.Invoke();
    }
}
