using UnityEngine;

public interface IBindableUI
{
    /// <summary>
    /// Binds the UI to a controller or data source.
    /// </summary>
    /// <param name="controller">The controller or data source to bind to.</param>
    void Bind(object controller);
    /// <summary>
    /// Activates the UI, typically showing it or enabling interaction.
    /// </summary>
    //void Activate();
    /// <summary>
    /// Closes the UI, typically hiding it or disabling interaction.
    /// </summary>
    //void Close();
}