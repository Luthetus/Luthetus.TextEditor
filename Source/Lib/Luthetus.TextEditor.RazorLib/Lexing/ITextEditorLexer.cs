using Luthetus.Common.RazorLib.Misc;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.Lexing;

public interface ITextEditorLexer
{
    public RenderStateKey ModelRenderStateKey { get; }

    public Task<ImmutableArray<TextEditorTextSpan>> Lex(
        string text,
        RenderStateKey modelRenderStateKey);
}

public interface TextEditorLexerResult
{
    public RenderStateKey ModelRenderStateKey { get; }
    public ImmutableArray<TextEditorTextSpan> TextSpans { get; }
}