﻿using Luthetus.Common.RazorLib.Icons.Codicon;

namespace Luthetus.TextEditor.RazorLib.Find;

public class RegisteredViewModelsFindProvider : ITextEditorFindProvider
{
    public TextEditorFindProviderKey FindProviderKey { get; } =
        new TextEditorFindProviderKey(Guid.Parse("8f82c804-7813-44ea-869a-f77574f2f945"));

    public Type IconComponentRendererType { get; } = typeof(IconCopy);
    public string DisplayName { get; } = "Registered ViewModels";

    public async Task SearchAsync(
        string searchQuery,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(5_000);
    }
}
