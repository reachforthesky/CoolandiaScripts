using System;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(SpriteRenderer))]
public class CampfireController : NetworkBehaviour, IInteractable
{
    public Sprite inactiveSprite;
    public Sprite activeSprite;

    [SerializeField] private FuelConsumer fuelConsumer;
    [SerializeField] private Processor processor;

    [Header("UI")]
    [SerializeField] private GameObject campfireUIPrefab;

    private SpriteRenderer spriteRenderer;

    ulong IInteractable.NetworkObjectId
    {
        get => base.NetworkObjectId;
        set { /* NetworkObjectId is readonly in NetworkBehaviour, so setter is intentionally left empty */ }
    }

    public NetworkVariable<bool> active = new NetworkVariable<bool>();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (active.Value)
        {
            fuelConsumer.enabled = true;
            spriteRenderer.sprite = fuelConsumer.IsBurning ? activeSprite : inactiveSprite;
            processor.enabled = fuelConsumer.IsBurning;
        }
        else
        {
            spriteRenderer.sprite = inactiveSprite;
            fuelConsumer.enabled = processor.enabled = false;
        }
    }

    public void Interact(PlayerController player)
    {
        OpenCampfireUIClientRpc(player.OwnerClientId);
    }

    [ClientRpc]
    private void OpenCampfireUIClientRpc(ulong ownerClientId)
    {
        PlayerController myPlayer = null;
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].OwnerClientId == ownerClientId)
            {
                myPlayer = players[i];
                break;
            }
        }

        if (NetworkManager.Singleton.LocalClientId != ownerClientId)
            return; 

        myPlayer?.OpenInventory();

        var ui = myPlayer.GetComponentInChildren<UIManager>()?.OpenInCenter(campfireUIPrefab);

        ui?.Bind(this);
    }


    [ServerRpc(RequireOwnership = false)]
    public void ActivateCampfireServerRpc()
    {
        active.Value = !active.Value;
    }
    public Inventory GetFuelInventory() => fuelConsumer.fuelInventory;
    public Inventory GetProcessingInventory() => processor.inventory;
}
