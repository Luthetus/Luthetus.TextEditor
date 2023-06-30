using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Model;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.CompilerServiceCase;

public class TextEditorCompilerServiceDefault : ICompilerService
{
    public void RegisterModel(TextEditorModel textEditorModel)
    {
        throw new NotImplementedException();
    }

    public ImmutableArray<TextEditorTextSpan> GetSyntacticTextSpansFor(TextEditorModel textEditorModel)
    {
        throw new NotImplementedException();
    }

    public ImmutableArray<ITextEditorSymbol> GetSymbolsFor(TextEditorModel textEditorModel)
    {
        throw new NotImplementedException();
    }

    public ImmutableArray<TextEditorTextSpan> GetDiagnosticTextSpansFor(TextEditorModel textEditorModel)
    {
        throw new NotImplementedException();
    }

    public void ModelWasModified(TextEditorModel textEditorModel, ImmutableArray<TextEditorTextSpan> editTextSpans)
    {
        throw new NotImplementedException();
    }

    public void DisposeModel(TextEditorModel textEditorModel)
    {
        throw new NotImplementedException();
    }
}