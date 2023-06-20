namespace Luthetus.TextEditor.RazorLib.ViewModel;

/// <summary>
/// The name of this class, '<see cref="ToRenderViewModelData"/>', is a bit awkwardly worded.
/// This class is to track the currently "rendered" ViewModel's ViewModelKey and its <see cref="DisplayTracker"/>.
/// The wording "ToRender" is because this object is set in 'OnParametersSet' and cannot
/// be guaranteed to be the actively rendered ViewModel. Instead its 'to be' once a render occurs.
/// </summary>
public class ToRenderViewModelData
{
    public ToRenderViewModelData(TextEditorViewModelKey textEditorViewModelKey, DisplayTracker displayTracker)
    {
        ViewModelKey = textEditorViewModelKey;
        DisplayTracker = displayTracker;
    }

    public TextEditorViewModelKey ViewModelKey { get; }
    public DisplayTracker DisplayTracker { get; }
}
