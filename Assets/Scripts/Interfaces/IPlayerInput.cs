using UnityEngine;

public interface IPlayerInput
{
    Vector2 GetMoveInput();
    bool GetJump();
    bool GetSprint();
    bool GetUse();
    bool GetInteract();
    bool GetInventory();
}
