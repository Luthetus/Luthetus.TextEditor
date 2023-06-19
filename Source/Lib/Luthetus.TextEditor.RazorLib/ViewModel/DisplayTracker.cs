using Fluxor;
using Luthetus.TextEditor.RazorLib.Store.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luthetus.TextEditor.RazorLib.ViewModel;

/// <summary>
/// One must track whether the ViewModel is currently being rendered.
/// <br/><br/>
/// The reason for this is that the UI logic is lazily invoked.
/// That is to say, if a ViewModel has its underlying Model change, BUT the ViewModel is not currently being rendered. Then that ViewModel does not
/// react to the Model having changed.
/// </summary>
public class DisplayTracker
{
    private readonly object _linksLock = new();

    /// <summary>
    /// <see cref="Links"/> refers to a Blazor TextEditorViewModelDisplay having had its OnParametersSet invoked
    /// and the ViewModelKey that was passed as a parameter matches this encompasing ViewModel's key. In this situation
    /// <see cref="Links"/> would be incremented by 1 in a concurrency safe manner.
    /// <br/><br/>
    /// As well OnParametersSet includes the case where the ViewModelKey that was passed as a parameter is changed.
    /// In this situation the previous ViewModel would have its <see cref="Links"/> decremented by 1 in a concurrency safe manner.
    /// <br/><br/>
    /// TextEditorViewModelDisplay implements IDisposable. In the Dispose implementation,
    /// the active ViewModel would have its <see cref="Links"/> decremented by 1 in a concurrency safe manner.
    /// </summary>
    public int Links { get; private set; }

    public void IncrementLinks(IState<TextEditorModelsCollection> modelsCollectionWrap)
    {
        lock (_linksLock)
        {
            Links++;

            if (_toRenderViewModelData.DisplayTracker.Links == 1)
            {
                // This ViewModel was not being displayed until this point.
                // Due to lazily updating the UI, now that it IS being displayed,
                // proceed to subscribe to the events.

                ModelsCollectionWrap.StateChanged += GeneralOnStateChangedEventHandler;
            }
        }
    }
    
    public void DecrementLinks(IState<TextEditorModelsCollection> modelsCollectionWrap, TextEditorViewModel viewModel)
    {
        lock (_linksLock)
        {
            Links--;

            if (Links == 0)
            {
                // This ViewModel will NO LONGER be rendered.
                // Due to lazily updating the UI, proceed to unsubscribe from the events.

                modelsCollectionWrap.StateChanged += TEVM;
            }
        }
    }
}
