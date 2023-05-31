using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Clipboard;
using Luthetus.Common.RazorLib.Dialog;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.Common.RazorLib.WatchWindow;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Commands;
using Luthetus.TextEditor.RazorLib.Commands.Default;
using Luthetus.TextEditor.RazorLib.Cursor;
using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Row;
using Luthetus.TextEditor.RazorLib.Semantics;
using Luthetus.TextEditor.RazorLib.ViewModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Luthetus.Common.RazorLib.ComponentRenderers;
using Luthetus.Common.RazorLib.ComponentRenderers.Types;

namespace Luthetus.TextEditor.RazorLib.HelperComponents;

public partial class TextEditorHeader : TextEditorView
{
    [Inject]
    private IClipboardService ClipboardService { get; set; } = null!;
    [Inject]
    private IDialogService DialogService { get; set; } = null!;
    [Inject]
    private ILuthetusCommonComponentRenderers LuthetusCommonComponentRenderers { get; set; } = null!;

    [Parameter]
    public ImmutableArray<TextEditorHeaderButtonKind>? HeaderButtonKinds { get; set; }

    private TextEditorCommandParameter ConstructTextEditorCommandParameter(
        TextEditorModel textEditorModel,
        TextEditorViewModel textEditorViewModel)
    {
        return new TextEditorCommandParameter(
            textEditorModel,
            TextEditorCursorSnapshot.TakeSnapshots(textEditorViewModel.PrimaryCursor),
            ClipboardService,
            TextEditorService,
            textEditorViewModel);
    }

    private void SelectRowEndingKindOnChange(ChangeEventArgs changeEventArgs)
    {
        var textEditor = MutableReferenceToModel;
        var localTextEditorViewModel = MutableReferenceToViewModel;

        if (textEditor is null ||
            localTextEditorViewModel is null)
        {
            return;
        }

        var rowEndingKindString = (string)(changeEventArgs.Value ?? string.Empty);

        if (Enum.TryParse<RowEndingKind>(rowEndingKindString, out var rowEndingKind))
            TextEditorService.Model.SetUsingRowEndingKind(
                localTextEditorViewModel.ModelKey,
                rowEndingKind);
    }

    private async Task DoCopyOnClick(MouseEventArgs arg)
    {
        var textEditor = MutableReferenceToModel;
        var textEditorViewModel = MutableReferenceToViewModel;

        if (textEditor is null ||
            textEditorViewModel is null)
        {
            return;
        }

        var textEditorCommandParameter = ConstructTextEditorCommandParameter(
            textEditor,
            textEditorViewModel);

        var command = TextEditorCommandDefaultFacts.Copy;

        await command.DoAsyncFunc.Invoke(
            textEditorCommandParameter);
    }

    private async Task DoCutOnClick(MouseEventArgs arg)
    {
        var textEditor = MutableReferenceToModel;
        var textEditorViewModel = MutableReferenceToViewModel;

        if (textEditor is null ||
            textEditorViewModel is null)
        {
            return;
        }

        var textEditorCommandParameter = ConstructTextEditorCommandParameter(
            textEditor,
            textEditorViewModel);

        var command = TextEditorCommandDefaultFacts.Cut;

        await command.DoAsyncFunc.Invoke(
            textEditorCommandParameter);
    }

    private async Task DoPasteOnClick(MouseEventArgs arg)
    {
        var textEditor = MutableReferenceToModel;
        var textEditorViewModel = MutableReferenceToViewModel;

        if (textEditor is null ||
            textEditorViewModel is null)
        {
            return;
        }

        var textEditorCommandParameter = ConstructTextEditorCommandParameter(
            textEditor,
            textEditorViewModel);

        var command = TextEditorCommandDefaultFacts.Paste;

        await command.DoAsyncFunc.Invoke(
            textEditorCommandParameter);
    }

    private async Task DoRedoOnClick(MouseEventArgs arg)
    {
        var textEditor = MutableReferenceToModel;
        var textEditorViewModel = MutableReferenceToViewModel;

        if (textEditor is null ||
            textEditorViewModel is null)
        {
            return;
        }

        var textEditorCommandParameter = ConstructTextEditorCommandParameter(
            textEditor,
            textEditorViewModel);

        var command = TextEditorCommandDefaultFacts.Redo;

        await command.DoAsyncFunc.Invoke(
            textEditorCommandParameter);
    }

    private async Task DoSaveOnClick(MouseEventArgs arg)
    {
        var textEditor = MutableReferenceToModel;
        var textEditorViewModel = MutableReferenceToViewModel;

        if (textEditor is null ||
            textEditorViewModel is null)
        {
            return;
        }

        var textEditorCommandParameter = ConstructTextEditorCommandParameter(
            textEditor,
            textEditorViewModel);

        var command = TextEditorCommandDefaultFacts.Save;

        await command.DoAsyncFunc.Invoke(
            textEditorCommandParameter);
    }

    private async Task DoUndoOnClick(MouseEventArgs arg)
    {
        var textEditor = MutableReferenceToModel;
        var textEditorViewModel = MutableReferenceToViewModel;

        if (textEditor is null ||
            textEditorViewModel is null)
        {
            return;
        }

        var textEditorCommandParameter = ConstructTextEditorCommandParameter(
            textEditor,
            textEditorViewModel);

        var command = TextEditorCommandDefaultFacts.Undo;

        await command.DoAsyncFunc.Invoke(
            textEditorCommandParameter);
    }

    private async Task DoSelectAllOnClick(MouseEventArgs arg)
    {
        var textEditor = MutableReferenceToModel;
        var textEditorViewModel = MutableReferenceToViewModel;

        if (textEditor is null ||
            textEditorViewModel is null)
        {
            return;
        }

        var textEditorCommandParameter = ConstructTextEditorCommandParameter(
            textEditor,
            textEditorViewModel);

        var command = TextEditorCommandDefaultFacts.SelectAll;

        await command.DoAsyncFunc.Invoke(
            textEditorCommandParameter);
    }

    private async Task DoRemeasureOnClick(MouseEventArgs arg)
    {
        var textEditor = MutableReferenceToModel;
        var textEditorViewModel = MutableReferenceToViewModel;

        if (textEditor is null ||
            textEditorViewModel is null)
        {
            return;
        }

        var textEditorCommandParameter = ConstructTextEditorCommandParameter(
            textEditor,
            textEditorViewModel);

        var command = TextEditorCommandDefaultFacts.Remeasure;

        await command.DoAsyncFunc.Invoke(
            textEditorCommandParameter);
    }

    private void ShowWatchWindowDisplayDialogOnClick()
    {
        var model = MutableReferenceToModel;

        if (model is null)
            return;

        if (model.SemanticModel is not null)
            model.SemanticModel.Parse(model);

        var watchWindowObjectWrap = new WatchWindowObjectWrap(
            model,
            typeof(TextEditorModel),
            "TextEditorModel",
            true);

        var dialogRecord = new DialogRecord(
            DialogKey.NewDialogKey(),
            $"WatchWindow: {model.ResourceUri}",
            typeof(WatchWindowDisplay),
            new Dictionary<string, object?>
            {
                {
                    nameof(WatchWindowDisplay.WatchWindowObjectWrap),
                    watchWindowObjectWrap
                }
            },
            null)
        {
            IsResizable = true
        };

        DialogService.RegisterDialogRecord(dialogRecord);

        ChangeLastPresentationLayer();
    }
    
    private void RunFileOnClick()
    {
        if (LuthetusCommonComponentRenderers.RunFileDisplayRenderer is null)
            return;
        
        var model = MutableReferenceToModel;

        if (model is null)
            return;

        var sourceText = model.GetAllText();

        var dialogRecord = new DialogRecord(
            DialogKey.NewDialogKey(),
            $"RunFile: {model.ResourceUri}",
            LuthetusCommonComponentRenderers.RunFileDisplayRenderer,
            new Dictionary<string, object?>
            {
                {
                    nameof(IRunFileDisplayRenderer.SourceText),
                    sourceText
                }
            },
            null)
        {
            IsResizable = true
        };

        DialogService.RegisterDialogRecord(dialogRecord);
    }

    private async Task DoRefreshOnClick()
    {
        var textEditor = MutableReferenceToModel;
        var textEditorViewModel = MutableReferenceToViewModel;

        if (textEditor is null ||
            textEditorViewModel is null)
        {
            return;
        }

        var textEditorCommandParameter = ConstructTextEditorCommandParameter(
            textEditor,
            textEditorViewModel);

        var command = TextEditorCommandDefaultFacts.Remeasure;

        await command.DoAsyncFunc.Invoke(
            textEditorCommandParameter);
    }

    /// <summary>
    /// disabled=@GetUndoDisabledAttribute()
    /// will toggle the attribute
    /// <br/><br/>
    /// disabled="@GetUndoDisabledAttribute()"
    /// will toggle the value of the attribute
    /// </summary>
    private bool GetUndoDisabledAttribute()
    {
        var textEditor = MutableReferenceToModel;
        var textEditorViewModel = MutableReferenceToViewModel;

        if (textEditor is null ||
            textEditorViewModel is null)
        {
            return true;
        }

        return !textEditor.CanUndoEdit();
    }

    /// <summary>
    /// disabled=@GetRedoDisabledAttribute()
    /// will toggle the attribute
    /// <br/><br/>
    /// disabled="@GetRedoDisabledAttribute()"
    /// will toggle the value of the attribute
    /// </summary>
    private bool GetRedoDisabledAttribute()
    {
        var textEditor = MutableReferenceToModel;
        var textEditorViewModel = MutableReferenceToViewModel;

        if (textEditor is null ||
            textEditorViewModel is null)
        {
            return true;
        }

        return !textEditor.CanRedoEdit();
    }

    private void ChangeLastPresentationLayer()
    {
        var viewModel = MutableReferenceToViewModel;

        if (viewModel is null)
            return;

        TextEditorService.ViewModel.With(
            viewModel.ViewModelKey,
            inViewModel =>
            {
                var outPresentationLayer = inViewModel.FirstPresentationLayer;

                var inPresentationModel = outPresentationLayer
                    .FirstOrDefault(x =>
                        x.TextEditorPresentationKey == SemanticFacts.PresentationKey);

                if (inPresentationModel is null)
                {
                    inPresentationModel = SemanticFacts.EmptyPresentationModel;

                    outPresentationLayer = outPresentationLayer.Add(
                        inPresentationModel);
                }

                var model = TextEditorService.ViewModel
                    .FindBackingModelOrDefault(viewModel.ViewModelKey);

                var outPresentationModel = inPresentationModel with
                {
                    TextEditorTextSpans = model?.SemanticModel?.DiagnosticTextSpanTuples.Select(x => x.textSpan).ToImmutableList()
                        ?? ImmutableList<TextEditorTextSpan>.Empty
                };

                outPresentationLayer = outPresentationLayer.Replace(
                    inPresentationModel,
                    outPresentationModel);

                return inViewModel with
                {
                    FirstPresentationLayer = outPresentationLayer,
                    RenderStateKey = RenderStateKey.NewRenderStateKey()
                };
            });
    }
}