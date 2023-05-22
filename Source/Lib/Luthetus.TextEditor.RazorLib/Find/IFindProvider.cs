﻿namespace Luthetus.TextEditor.RazorLib.Find;

public interface ITextEditorFindProvider
{
    public TextEditorFindProviderKey FindProviderKey { get; }
    public Type IconComponentRendererType { get; }
    public string DisplayName { get; }

    public Task SearchAsync(string searchQuery, CancellationToken cancellationToken = default);
}
