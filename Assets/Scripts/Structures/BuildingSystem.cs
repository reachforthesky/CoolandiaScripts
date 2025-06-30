using Microsoft.Unity.VisualStudio.Editor;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class BuildingSystem : NetworkBehaviour
{
    [Header("Buildable Structures")]
    [SerializeField] private List<StructureRecipe> availableRecipes;

    [Header("Preview Settings")]
    [SerializeField] private GameObject previewHolder; // Parent object to hold preview instance
    [SerializeField] private GameObject buildGhostPrefab; // Parent object to hold preview instance
    [SerializeField] private LayerMask placementLayer; // E.g., "Ground"

    private StructureRecipe currentRecipe;
    private GameObject currentPreviewInstance;
    private Camera mainCamera;
    private Sprite previewSprite;

    private void Start()
    {
        mainCamera = Camera.main;
        HidePreview();
    }

    private void Update()
    {
        if (!IsOwner) return;
        HandleRecipeSelection();
        UpdatePreviewPosition();
        HandlePlacement();
    }

    private void HandleRecipeSelection()
    {
        for (int i = 0; i < Mathf.Min(availableRecipes.Count, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                SelectRecipe(availableRecipes[i]);
                Cursor.lockState = CursorLockMode.Confined;
                break;
            }
        }
    }

    private void SelectRecipe(StructureRecipe recipe)
    {
        currentRecipe = recipe;
        previewSprite = currentRecipe.structurePrefab.GetComponent<SpriteRenderer>().sprite; 
        SpawnPreviewFromSprite(recipe);
    }

    private void SpawnPreviewFromSprite(StructureRecipe recipe)
    {
        if (currentPreviewInstance != null)
            Destroy(currentPreviewInstance);

        if (recipe != null)
        {
            currentPreviewInstance = Instantiate(previewHolder);

            var sr = currentPreviewInstance.GetComponent<SpriteRenderer>();
            sr.sprite = previewSprite;
            sr.color = new Color(1f, 1f, 1f, 0.5f); // Semi-transparent

            // Optional: Assign preview layer/material if needed
        }
    }

    private void UpdatePreviewPosition()
    {
        if (currentPreviewInstance == null)
            return;

        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hit, 100f, placementLayer))
        {
            Vector3 targetPos = hit.point;
            currentPreviewInstance.transform.position = SnapToGrid(targetPos);
        }
    }

    private void HandlePlacement()
    {
        if (Input.GetMouseButtonDown(0) && currentPreviewInstance != null && currentRecipe != null)
        {
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hit, 100f, placementLayer))
            {
                Vector3 position = SnapToGrid(hit.point);
                DropStructureGhostServerRpc(position, currentRecipe.structureId);
            }
        }
        if (Input.GetKeyDown("escape") && currentPreviewInstance != null && currentRecipe != null)
        {
            HidePreview();
            currentRecipe = null; // Deselect the recipe
            Destroy(currentPreviewInstance);
            currentPreviewInstance = null;
            previewSprite = null;
            Cursor.lockState = CursorLockMode.Locked; // Lock cursor
        }
    }

    [ServerRpc]
    private void DropStructureGhostServerRpc(Vector3 position, int recipeId)
    {
        if (!IsServer)
        {
            Debug.LogError("DropStructureGhost called on client, but should be on server.");
            return;
        }

        var recipe = GameDatabaseManager.Instance.Structures.Get(recipeId);
        if (recipe == null)
        {
            Debug.LogWarning($"No recipe matching ID {recipeId}.");
            return;
        }

        int prefabId = PersistentEntityManager.Instance.FindIndex(buildGhostPrefab); 

        var buildablePed = new PersistentEntityData
        {
            prefabId = prefabId,
            position = position,
            rotation = Quaternion.identity,
            isDestroyed = false,
        };

        var buildGhost = PersistentEntityManager.Instance.RegisterEntity(buildablePed);

        // Optional: handle collider fix
        var collider = buildGhost.GetComponent<CapsuleCollider>();
        if (collider != null)
            collider.height = Mathf.Max(collider.height, 3);

        // Assign networked structure data
        var buildGhostEntity = buildGhost.GetComponent<BuildableEntityData>();
        buildGhostEntity.structureRecipeId.Value = recipe.structureId;
        buildGhostEntity.finishedStructurePrefab = recipe.structurePrefab;

        // Look up the sprite ID from the prefab's sprite and sync it
        var spriteRenderer = recipe.structurePrefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            string spriteId = GameDatabaseManager.Instance.Sprites.GetId(spriteRenderer.sprite);
            buildGhostEntity.spriteId.Value = spriteId;
        }
    }

    private Vector3 SnapToGrid(Vector3 rawPos)
    {
        float gridSize = 1f;
        return new Vector3(
            Mathf.Floor(rawPos.x / gridSize) * gridSize + gridSize / 2,
            rawPos.y+previewSprite.bounds.extents.y, // Adjust height based on sprite bounds
            Mathf.Floor(rawPos.z / gridSize) * gridSize + gridSize / 2
        );
    }

    private void HidePreview()
    {
        if (currentPreviewInstance != null)
            currentPreviewInstance.SetActive(false);
    }
}