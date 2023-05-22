using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Analysis.CSharp.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.Decoration;
using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.Tests.TestDataFolder;

namespace Luthetus.TextEditor.Tests.Basics.Lexers;

public class LexGenericTests
{
    [Fact]
    public async Task LexFunction()
    {
        var text = TestData.CSharp.EXAMPLE_TEXT_8_LINES
            .ReplaceLineEndings("\n");

        var expectedTextEditorTextSpans = new[]
        {
            new TextEditorTextSpan(0, 107, (byte)GenericDecorationKind.Function),
            new TextEditorTextSpan(225, 268, (byte)GenericDecorationKind.Function),
            new TextEditorTextSpan(338, 407, (byte)GenericDecorationKind.Function),
            new TextEditorTextSpan(436, 498, (byte)GenericDecorationKind.Function),
        };

        var cSharpLexer = new TextEditorCSharpLexer();

        var textEditorTextSpans = await cSharpLexer.Lex(
            text,
            RenderStateKey.NewRenderStateKey());

        textEditorTextSpans = textEditorTextSpans
            .Where(x => x.DecorationByte == (byte)GenericDecorationKind.Function)
            .OrderBy(x => x.StartingIndexInclusive)
            .ToImmutableArray();

        Assert.Equal(expectedTextEditorTextSpans, textEditorTextSpans);
    }
}