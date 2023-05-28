using Luthetus.Common.RazorLib.Misc;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.Lexing;

public interface TextEditorLexerResult
{
    public ImmutableArray<TextEditorTextSpan> TextSpans { get; }
    public string ResourceUri { get; }
    public RenderStateKey ModelRenderStateKey { get; }
}