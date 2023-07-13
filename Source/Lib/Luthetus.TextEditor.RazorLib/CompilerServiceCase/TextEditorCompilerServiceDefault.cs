using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Model;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.CompilerServiceCase;

public class TextEditorCompilerServiceDefault : ICompilerService
{
    public void RegisterModel(TextEditorModel textEditorModel)
    {
        // Do nothing
    }

    public ImmutableArray<TextEditorTextSpan> GetSyntacticTextSpansFor(TextEditorModel textEditorModel)
    {
        return ImmutableArray<TextEditorTextSpan>.Empty;
    }

    public ImmutableArray<ITextEditorSymbol> GetSymbolsFor(TextEditorModel textEditorModel)
    {
        return ImmutableArray<ITextEditorSymbol>.Empty;
    }

    public ImmutableArray<TextEditorTextSpan> GetDiagnosticTextSpansFor(TextEditorModel textEditorModel)
    {
        return ImmutableArray<TextEditorTextSpan>.Empty;
    }

    public void ModelWasModified(TextEditorModel textEditorModel, ImmutableArray<TextEditorTextSpan> editTextSpans)
    {
        // Do nothing
    }

    public void DisposeModel(TextEditorModel textEditorModel)
    {
        // Do nothing
    }
}
