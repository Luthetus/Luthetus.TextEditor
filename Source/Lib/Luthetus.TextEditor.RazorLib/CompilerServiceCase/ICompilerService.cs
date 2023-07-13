using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Model;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.CompilerServiceCase;

public interface ICompilerService
{
    /// <summary>Expected to be concurrency safe with <see cref="DisposeModel"/></summary>
    public void RegisterModel(TextEditorModel textEditorModel);

    /// <summary>Expected to be an <see cref="Microsoft.Extensions.Hosting.IHostedService"/> (or anything which performs background task work)</summary>
    public void ModelWasModified(TextEditorModel textEditorModel, ImmutableArray<TextEditorTextSpan> editTextSpans);

    /// <summary>
    /// Provides syntax highlighting from the lexing result.
    /// This method is invoked, and applied, before <see cref="GetSymbolsFor"/>
    /// </summary>
    public ImmutableArray<TextEditorTextSpan> GetSyntacticTextSpansFor(TextEditorModel textEditorModel);

    /// <summary>
    /// Provides syntax highlighting that cannot be determined by lexing alone.
    /// This method is invoked, and applied, after <see cref="GetSyntacticTextSpansFor"/>
    /// </summary>
    public ImmutableArray<ITextEditorSymbol> GetSymbolsFor(TextEditorModel textEditorModel);

    /// <summary>
    /// Provides 'squigglies' which when hovered over display a message, along with
    /// a serverity level.
    /// </summary>
    public ImmutableArray<TextEditorTextSpan> GetDiagnosticTextSpansFor(TextEditorModel textEditorModel);

    /// <summary>Expected to be concurrency safe with <see cref="RegisterModel"/></summary>
    public void DisposeModel(TextEditorModel textEditorModel);
}
