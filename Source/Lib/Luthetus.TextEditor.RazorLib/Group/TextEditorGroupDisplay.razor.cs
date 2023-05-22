﻿using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib;
using Luthetus.TextEditor.RazorLib.Store.Group;
using Fluxor;
using Luthetus.TextEditor.RazorLib.HelperComponents;
using Microsoft.AspNetCore.Components;

namespace Luthetus.TextEditor.RazorLib.Group;

public partial class TextEditorGroupDisplay : IDisposable
{
    [Inject]
    private IState<TextEditorGroupsCollection> TextEditorGroupsCollectionWrap { get; set; } = null!;
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;

    /// <summary>
    /// If the provided <see cref="TextEditorGroupKey"/> is registered using the
    /// <see cref="ITextEditorService"/>. Then this component will automatically update
    /// when the corresponding <see cref="TextEditorGroup"/> is replaced.
    /// <br/><br/>
    /// A <see cref="TextEditorGroupKey"/> which is NOT registered using the
    /// <see cref="ITextEditorService"/> can be passed in. Then if the <see cref="TextEditorGroupKey"/>
    /// ever gets registered then this Blazor Component will update accordingly.
    /// </summary>
    [Parameter, EditorRequired]
    public TextEditorGroupKey TextEditorGroupKey { get; set; } = null!;
    [Parameter]
    public string CssStyleString { get; set; } = string.Empty;
    [Parameter]
    public string CssClassString { get; set; } = string.Empty;
    /// <summary>TabIndex is used for the html attribute named: 'tabindex'</summary>
    [Parameter]
    public int TabIndex { get; set; } = -1;
    /// <summary><see cref="HeaderButtonKinds"/> contains the enum value that represents a button displayed in the optional component: <see cref="TextEditorHeader"/>.</summary>
    [Parameter]
    public ImmutableArray<TextEditorHeaderButtonKind>? HeaderButtonKinds { get; set; }

    protected override void OnInitialized()
    {
        TextEditorGroupsCollectionWrap.StateChanged += TextEditorGroupWrapOnStateChanged;

        base.OnInitialized();
    }

    private async void TextEditorGroupWrapOnStateChanged(object? sender, EventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        TextEditorGroupsCollectionWrap.StateChanged -= TextEditorGroupWrapOnStateChanged;
    }
}