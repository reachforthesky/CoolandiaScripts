using UnityEngine;
using System.Collections.Generic;

public class Processor : MonoBehaviour
{
    [SerializeField] private List<ProcessorRecipe> recipes;
    [SerializeField] private int maxConcurrentProcesses = 3;
    [SerializeField] private float processingSpeed;

    public Inventory inventory;

    private List<ProcessingJob> activeJobs = new();

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
    }

    private void Update()
    {
        ProcessJobs(Time.deltaTime);
        TryStartJobs();
    }

    private void TryStartJobs()
    {
        if (activeJobs.Count >= maxConcurrentProcesses)
            return;

        foreach (var recipe in recipes)
        {
            if (activeJobs.Count >= maxConcurrentProcesses)
                break;

            if (CanStartRecipe(recipe))
            {
                activeJobs.Add(new ProcessingJob(recipe));
            }
        }
    }

    private void ProcessJobs(float deltaTime)
    {
        for (int i = activeJobs.Count - 1; i >= 0; i--)
        {
            var job = activeJobs[i];
            job.progress += deltaTime * processingSpeed;

            if (job.progress >= job.recipe.processingCost)
            {
                if(ConsumeInputs(job.recipe))
                    inventory.AddItem(job.recipe.output);
                activeJobs.RemoveAt(i);
            }
        }
    }

    private bool CanStartRecipe(ProcessorRecipe recipe)
    {
        foreach (var input in recipe.inputs)
        {
            if (inventory.Count(input.itemId) < input.quantity)
                return false;
        }
        return true;
    }

    private bool ConsumeInputs(ProcessorRecipe recipe)
    {
        foreach (var input in recipe.inputs)
        {
            if (!inventory.Has(input))
            {
                Debug.LogWarning($"Cannot consume inputs for recipe {recipe.recipeID}. Missing {input.quantity} of {input.itemId}.");
                return false;
            }
        }
        foreach (var input in recipe.inputs)
        {
            if (!inventory.RemoveStack(input))
            {
                return false;
            }
        }
        return true;
    }

    private class ProcessingJob
    {
        public ProcessorRecipe recipe;
        public float progress;

        public ProcessingJob(ProcessorRecipe recipe)
        {
            this.recipe = recipe;
            this.progress = 0f;
        }
    }
}
