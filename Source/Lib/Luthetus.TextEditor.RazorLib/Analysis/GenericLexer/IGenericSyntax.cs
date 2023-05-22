using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.SyntaxEnums;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.GenericLexer;

public interface IGenericSyntax
{
    public TextEditorTextSpan TextEditorTextSpan { get; }
    public ImmutableArray<IGenericSyntax> Children { get; }
    public GenericSyntaxKind GenericSyntaxKind { get; }
}