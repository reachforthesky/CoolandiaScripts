using UnityEngine;

public class WindowManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private KeyCode toggleKey;

    private bool isVisible = false;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isVisible = !isVisible;
        inventoryPanel.SetActive(isVisible);

        // Optional: Lock/unlock player controls or mouse here
        Cursor.visible = isVisible;
        Cursor.lockState = isVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
