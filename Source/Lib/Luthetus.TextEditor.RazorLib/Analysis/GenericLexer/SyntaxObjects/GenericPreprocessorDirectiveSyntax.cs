using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.SyntaxEnums;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.SyntaxObjects;

public class GenericPreprocessorDirectiveSyntax : IGenericSyntax
{
    public GenericPreprocessorDirectiveSyntax(
        TextEditorTextSpan textEditorTextSpan,
        ImmutableArray<IGenericSyntax> children)
    {
        TextEditorTextSpan = textEditorTextSpan;
        Children = children;
    }

    public TextEditorTextSpan TextEditorTextSpan { get; }
    public ImmutableArray<IGenericSyntax> Children { get; }
    public GenericSyntaxKind GenericSyntaxKind => GenericSyntaxKind.PreprocessorDirective;
}