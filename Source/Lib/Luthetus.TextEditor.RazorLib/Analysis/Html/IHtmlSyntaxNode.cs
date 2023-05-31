using Luthetus.TextEditor.RazorLib.Lexing;
using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html;

public interface IHtmlSyntaxNode : IHtmlSyntax
{
    public ImmutableArray<IHtmlSyntax> Children { get; }
    public TextEditorTextSpan TextEditorTextSpan { get; }
}