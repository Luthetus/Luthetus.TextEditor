using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.Tests.TestDataFolder;

namespace Luthetus.TextEditor.Tests.Basics.Lexers;

public class LexPlainTests
{
    [Fact]
    public async Task LexNothing()
    {
        var text = TestData.Plain.EXAMPLE_TEXT_25_LINES
            .ReplaceLineEndings("\n");

        var resourceUri = new ResourceUri(string.Empty);

        var defaultLexer = new TextEditorLexerDefault(resourceUri);

        var textEditorTextSpans = await defaultLexer.Lex(
            text,
            RenderStateKey.NewRenderStateKey());

        Assert.Empty(textEditorTextSpans);
    }
}