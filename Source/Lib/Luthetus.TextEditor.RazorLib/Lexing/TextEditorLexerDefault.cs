using Luthetus.Common.RazorLib.Misc;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.Lexing;

public class TextEditorLexerDefault : ITextEditorLexer
{
    public TextEditorLexerDefault(string resourceUri)
    {
        ResourceUri = resourceUri;
    }

    public RenderStateKey ModelRenderStateKey { get; private set; } = RenderStateKey.Empty;
    
    public string ResourceUri { get; }

    public Task<ImmutableArray<TextEditorTextSpan>> Lex(
        string text,
        RenderStateKey modelRenderStateKey)
    {
        return Task.FromResult(ImmutableArray<TextEditorTextSpan>.Empty);
    }
}