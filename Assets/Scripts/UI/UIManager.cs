using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private GameObject loadingScreenPrefab;
    private GameObject currentLoadingScreen;
    private bool isLoadingVisible = false;

    private Dictionary<UIType, UIWindow> windows = new();
    private Transform centerUIGroup;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        centerUIGroup = GameObject.FindWithTag("CenterUI").GetComponent<Transform>();
        DontDestroyOnLoad(gameObject);

        RegisterAllInScene(); // Optional auto-registration
    }
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
        var canvas = GameObject.FindWithTag("MainCanvas");
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

    private void RegisterAllInScene()
    {
        foreach (var window in FindObjectsByType<UIWindow>(FindObjectsSortMode.None ))
        {
            Register(window);
        }
    }
}
