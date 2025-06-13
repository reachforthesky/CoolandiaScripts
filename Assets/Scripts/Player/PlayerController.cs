using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public Transform visual;
    public Camera cam;

    [SerializedDictionary("Stat", "Value")]
    public SerializedDictionary<Stat, int> stats = new SerializedDictionary<Stat, int>{ 
        { Stat.toolbeltSize, 2 }, 
    };

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Inventory inventory;
    private PlayerEquipment equipment;
    private Toolbelt toolbelt;



    void Start()
    {
        controller = GetComponent<CharacterController>();
        inventory = GetComponent<Inventory>();
        equipment = GetComponent<PlayerEquipment>();
        toolbelt = GetComponent<Toolbelt>();
        toolbelt.updateToolbeltSize(getStat(Stat.toolbeltSize));    

        equipment.onPlayerEquipNew += updateEquipment;
        
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // Small downward force to keep grounded

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = cam.transform.right * horizontal + cam.transform.forward * vertical;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        BillboardToCamera();
        if (Input.GetMouseButtonDown(0))
        {
            TryUseItem();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            moveSpeed = 10f;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            moveSpeed = 5f;
        }
        /*if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            toolbelt.SetStack(0, inventory.slots[0].stack);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            toolbelt.SetStack(1, inventory.slots[1].stack);
        }*/
    }
    void TryUseItem()
    {
        var equipped = toolbelt.getEqippedItem();
        if (equipped == null)
        {
            Debug.Log("No tool equipped.");
            return;
        }
        Vector3 hitDirection = cam.transform.forward;
        hitDirection.y = 0;
        Ray ray = new Ray(transform.position + Vector3.up, hitDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
        {
            EntityData target = hit.collider.GetComponentInParent<EntityData>();
            if (target != null)
            {
                target.ReceiveHit(equipped);
            }
        }
    }
    public int getStat(Stat stat)
    {
        int statAccumulator = 0;
        foreach (var armor in equipment.equippedArmor)
        {
            if (armor == null) continue;
            statAccumulator += armor.statBonuses[stat];
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

    void updateEquipment()
    {
        toolbelt.updateToolbeltSize(getStat(Stat.toolbeltSize));
    }

}

public enum Stat { toolbeltSize }