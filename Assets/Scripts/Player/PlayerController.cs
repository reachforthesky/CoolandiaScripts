using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEditor.PlayerSettings;
using static UnityEngine.EventSystems.EventTrigger;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public Transform visual;
    public Camera cam;
    public Vector3 spawnPosition = Vector3.zero;

    [SerializeField] private GameObject uiCanvasPrefab;
    [SerializeField] private GameObject InventoryUIPrefab; 
    [SerializedDictionary("Stat", "Value")]
    public SerializedDictionary<Stat, int> stats = new SerializedDictionary<Stat, int>{ 
        { Stat.ToolbeltSize, 2 }, 
    };

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 initialPosition;
    private bool isGrounded;
    private Inventory inventory;
    private PlayerEquipment equipment;
    private Toolbelt toolbelt;
    public ItemUseFlash itemUseFlashUI;
    private ToolbeltUI toolbeltUI;
    private BuildingSystem builder;
    private CraftingSystem crafter;
    private IPlayerInput playerInput = new DefaultKeyboardInput();
    private NetworkTransform netTransform;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        netTransform = GetComponent<NetworkTransform>();
        controller = GetComponent<CharacterController>();
        if (!IsOwner)
        {
            transform.position = spawnPosition;
            // Disable camera and audio for non-local players
            var listener = GetComponentInChildren<AudioListener>(true);
            if (listener != null) listener.enabled = false;

            var cam = GetComponentInChildren<Camera>(true);
            if (cam != null) cam.enabled = false;
            return;
        }
        GameObject ui = Instantiate(uiCanvasPrefab, transform);
        UIManager.LocalInstance = ui.GetComponent<UIManager>();
        DragManager.Instance = ui.GetComponentInChildren<DragManager>();
        //DontDestroyOnLoad(ui);
        inventory = GetComponent<Inventory>();
        equipment = GetComponent<PlayerEquipment>();
        toolbelt = GetComponentInChildren<Toolbelt>();
        builder = GetComponent<BuildingSystem>();
        crafter = GetComponent<CraftingSystem>();
        itemUseFlashUI = ui.GetComponentInChildren<ItemUseFlash>(true);

        equipment.onPlayerEquipNew += updateEquipment;

        toolbeltUI = ui.GetComponentInChildren<ToolbeltUI>(true);
        var craftingUI = ui.GetComponentInChildren<CraftingUI>(true);
        craftingUI.SetCraftingSystem(crafter);

        if (toolbeltUI != null)
        {
            toolbeltUI.Bind(toolbelt); 
        }


        controller.enabled = false;
        transform.position = spawnPosition;
        controller.enabled = true;

        Debug.Log($"Player spawned for client {OwnerClientId}. IsOwner: {IsOwner}");
    }
    void Start()
    {
        if (!IsOwner || !controller.enabled) return;
        TeleportClientRpc(spawnPosition);
        this.transform.position = spawnPosition;
        if (!IsOwner && cam != null)
            cam.gameObject.SetActive(false);
        //initialPosition = this.transform.position;
    }

    void Update()
    {
        if (!IsOwner || !controller.enabled) return;

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // Small downward force to keep grounded

        Vector2 moveInput = playerInput.GetMoveInput();
        Vector3 move = cam.transform.right * moveInput.x + cam.transform.forward * moveInput.y;

        if (controller.enabled)
            controller.Move(move * moveSpeed * Time.deltaTime);

        if (playerInput.GetJump() && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        if(controller.enabled)
            controller.Move(velocity * Time.deltaTime);

        if((this.transform.position.y < -100 && this.transform.position.y > -900) || this.transform.position.y < -1100)
        {
            //this.transform.position = initialPosition;
        }

        BillboardToCamera();
        if (playerInput.GetUse())
        {
            TryUseItem();
        }

        if (playerInput.GetSprint())
        {
            moveSpeed = 10f;
        }
        else
        {
            moveSpeed = 5f;
        }

        if (playerInput.GetInteract())
        {
            Vector3 hitDirection = cam.transform.forward;
            hitDirection.y = 0;
            Ray ray = new Ray(transform.position + Vector3.up, hitDirection);
            if (Physics.Raycast(ray, out RaycastHit hit, 2f))
            {
                var target = hit.collider.GetComponentInParent<IInteractable>();
                if (target != null)
                {
                    ulong targetId = target.NetworkObjectId;
                    InteractWithEntityServerRpc(targetId);
                }
            }
        }
        if (playerInput.GetInventory())
        {
            OpenInventory();
        }
    }
    void TryUseItem()
    {
        var equipped = toolbelt.GetEquippedItem();
        if (equipped == null)
        {
            Debug.Log("No tool equipped.");
            return;
        }
        itemUseFlashUI.Flash(equipped.icon);
        Vector3 hitDirection = cam.transform.forward;
        hitDirection.y = 0;
        Ray ray = new Ray(transform.position + Vector3.up, hitDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
        {
            EntityData target = hit.collider.GetComponentInParent<EntityData>();
            if (target != null)
            {
                ulong targetId = target.NetworkObjectId;
                int itemId = equipped.itemId;
                UseItemOnEntityServerRpc(targetId, itemId);
            }
        }
    }
    void TryUseItem(ItemData item)
    {
        if (IsOwner)
        itemUseFlashUI.Flash(item.icon);
        Vector3 hitDirection = cam.transform.forward;
        hitDirection.y = 0;
        Ray ray = new Ray(transform.position + Vector3.up, hitDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
        {
            EntityData target = hit.collider.GetComponentInParent<EntityData>();
            if (target != null)
            {
                target.UseItemServerRpc(item.itemId);
            }
        } // send ID or enum, not whole object
    }
    [ServerRpc]
    private void UseItemOnEntityServerRpc(ulong targetId, int itemId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var netObj)) return;
        var entity = netObj.GetComponent<EntityData>();
        if (entity == null) return;

        var item = GameDatabaseManager.Instance.Items.Get(itemId);
        if (item != null)
            entity.ItemUsed(item);
    }
    [ServerRpc]
    private void InteractWithEntityServerRpc(ulong targetId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var netObj)) return;
        var entity = netObj.GetComponent<IInteractable>();
        if (entity == null) return;
        
        entity.Interact(this);
    }
    public void Teleport(Vector3 targetPosition)
    {
        if (IsServer)
            TeleportInternal(targetPosition);
        else
            TeleportServerRpc(targetPosition);
    }

    [ServerRpc]
    private void TeleportServerRpc(Vector3 pos)
    {
        TeleportInternal(pos);
        TeleportClientRpc(pos, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
        });
    }

    private void TeleportInternal(Vector3 pos)
    {
        controller.enabled = false;
        transform.position = pos;
        controller.enabled = true;

        TeleportClientRpc(pos, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
        });
    }

    [ClientRpc]
    private void TeleportClientRpc(Vector3 pos, ClientRpcParams rpcParams = default)
    {
        if (netTransform != null)
        {
            netTransform.Teleport(pos, transform.rotation, transform.localScale);
        }
        controller.enabled = false;
        transform.position = pos;
        controller.enabled = true;
    }
    public float getStat(Stat stat)
    {
        var statAccumulator = 0f;
        foreach (var armor in equipment.equippedArmor)
        {
            if (armor == null) continue;
            statAccumulator += armor.stats[stat];
        }
        return stats[stat] + statAccumulator;
    }

    void BillboardToCamera()
    {
        if (visual == null) return;
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 camForward = cam.transform.forward;
        camForward.y = 0f; // <-- LOCK the Y axis to avoid tilting
        if (camForward.sqrMagnitude > 0.01f)
        {
            visual.forward = camForward.normalized;
        }
    }
    public void OpenInventory()
    {
        if (!IsOwner) return;
        var inventory = UIManager.LocalInstance.OpenInCenter(InventoryUIPrefab);
        inventory.Bind(this.inventory);
    }
    void updateEquipment()
    {
        toolbelt.UpdateToolbeltSize((int)getStat(Stat.ToolbeltSize));
    }

}
