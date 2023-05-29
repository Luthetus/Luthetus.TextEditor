using Luthetus.Common.RazorLib.Misc;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.Lexing;

public class TextEditorLexerDefault : ITextEditorLexer
{
    public TextEditorLexerDefault(ResourceUri resourceUri)
    {
        ResourceUri = resourceUri;
    }

    public RenderStateKey ModelRenderStateKey { get; private set; } = RenderStateKey.Empty;
    
    public ResourceUri ResourceUri { get; }

    public Task<ImmutableArray<TextEditorTextSpan>> Lex(
        string sourceText,
        RenderStateKey modelRenderStateKey)
    {
        return Task.FromResult(ImmutableArray<TextEditorTextSpan>.Empty);
    }
}