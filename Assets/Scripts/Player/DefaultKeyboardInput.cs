using UnityEngine;

public class DefaultKeyboardInput : IPlayerInput
{
    public Vector2 GetMoveInput() =>
        new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

    public bool GetJump() => Input.GetButtonDown("Jump");
    public bool GetSprint() => Input.GetKey(KeyCode.LeftShift);
    public bool GetUse() => Input.GetMouseButtonDown(0);
    public bool GetInteract() => Input.GetKeyDown(KeyCode.E);
    public bool GetInventory() => Input.GetKeyDown(KeyCode.I);
}
