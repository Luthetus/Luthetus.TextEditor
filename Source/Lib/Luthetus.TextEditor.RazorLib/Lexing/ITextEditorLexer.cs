using Luthetus.Common.RazorLib.Misc;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.Lexing;

public interface ITextEditorLexer
{
    public RenderStateKey ModelRenderStateKey { get; }
    public ResourceUri ResourceUri { get; }

    public Task<ImmutableArray<TextEditorTextSpan>> Lex(
        string sourceText,
        RenderStateKey modelRenderStateKey);
}
