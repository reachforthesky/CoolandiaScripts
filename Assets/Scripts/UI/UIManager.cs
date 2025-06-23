using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager LocalInstance { get; set; }
    [SerializeField] private GameObject loadingScreenPrefab;
    [SerializeField] private Transform centerUIGroup;
    private GameObject currentLoadingScreen;
    private bool isLoadingVisible = false;

    private Dictionary<UIType, UIWindow> windows = new();

    private void Awake()
    {

        DontDestroyOnLoad(gameObject);
    }

    /*public GameObject InstantiateCanvas(GameObject prefab)
    {
        var ui = Instantiate(prefab, transform);
        centerUIGroup = ui.transform.Find("UIOverlayPanel/CenterHorizontalPanel");
        return ui;
    }*/

    public void ShowLoadingScreen()
    {
        if (isLoadingVisible) return;

        if (currentLoadingScreen == null)
        {
            var canvas = GameObject.FindWithTag("MainCanvas").transform;
            currentLoadingScreen = Instantiate(loadingScreenPrefab, canvas);
        }

        currentLoadingScreen.SetActive(true);
        isLoadingVisible = true;
    }
    public void HideLoadingScreen()
    {
        if (!isLoadingVisible) return;

        if (currentLoadingScreen != null)
            currentLoadingScreen.SetActive(false);

        isLoadingVisible = false;
    }
    public void Register(UIWindow window)
    {
        if (!windows.ContainsKey(window.uiType))
        {
            windows.Add(window.uiType, window);
        }
    }

    public void Unregister(UIWindow window)
    {
        if (windows.ContainsKey(window.uiType))
        {
            windows.Remove(window.uiType);
        }
    }

    public void Toggle(UIType type)
    {
        if (!windows.TryGetValue(type, out var window))
            return;

        if (window.IsOpen)
            window.Close();
        else
            window.Open();
    }

    public void Show(UIType type)
    {
        if (!windows.TryGetValue(type, out var window))
            return;
        window.Open();
    }
    public IBindableUI OpenInCenter(GameObject UIPrefab)
    {
        var canvas = GetComponent<Canvas>();
        if (!canvas) return null;

        foreach(Transform child in centerUIGroup)
        {
            if (child.gameObject == UIPrefab)
                return null;
        }

        var creatorUI = Instantiate(UIPrefab, centerUIGroup.transform);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        return creatorUI.GetComponent<IBindableUI>();
    }
    public void CloseAll()
    {
        foreach (Transform child in centerUIGroup)
            Destroy(child.gameObject);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Close all on ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAll();
        }

        // Toggle inventory
        if (Input.GetKeyDown(KeyCode.I))
        {
            Toggle(UIType.PlayerInventory);
        }

        // Add more hotkeys here if needed
    }

}
