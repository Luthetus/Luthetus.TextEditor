using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luthetus.TextEditor.RazorLib.CompilerServiceCase;

public interface ICompilerService
{
    /// <summary>Expected to be concurrency safe with <see cref="DisposeModel"/></summary>
    public void RegisterModel(TextEditorModel textEditorModel);

    /// <summary>Expected to be an IHostedService (or anything which performs background task work)</summary>
    public void ModelWasModified(TextEditorModel textEditorModel, ImmutableArray<TextEditorTextSpan> editTextSpans);

    /// <summary>
    /// Expected to be a synchronous read operation on the cached diagnostic text spans.
    /// When it comes to the need for recalculating them.
    /// As of this moment (2023-06-25) I expect re-calculation to be done when the 'ModelWasModified' method is invoked.
    /// </summary>
    public ImmutableArray<TextEditorTextSpan> GetDiagnosticTextSpansFor(TextEditorModel textEditorModel);

    /// <summary>
    /// Expected to be a synchronous read operation on the cached symbols.
    /// When it comes to the need for recalculating them.
    /// As of this moment (2023-06-25) I expect re-calculation to be done when the 'ModelWasModified' method is invoked.
    /// </summary>
    public ImmutableArray<ITextEditorSymbol> GetSymbolsFor(TextEditorModel textEditorModel);

    /// <summary>Expected to be concurrency safe with <see cref="RegisterModel"/></summary>
    public void DisposeModel(TextEditorModel textEditorModel);
}
