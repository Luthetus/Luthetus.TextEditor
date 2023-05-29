using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Analysis.Html.Decoration;
using Luthetus.TextEditor.RazorLib.Analysis.Razor.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.Tests.TestDataFolder;

namespace Luthetus.TextEditor.Tests.Basics.Lexers;

public class LexRazorTests
{
    [Fact]
    public async Task LexTagNames()
    {
        var sourceText = TestData.Razor.EXAMPLE_TEXT
            .ReplaceLineEndings("\n");

        var resourceUri = new ResourceUri(string.Empty);

        var expectedTagNameTextEditorTextSpans = new[]
        {
            new TextEditorTextSpan(1, 4, (byte)HtmlDecorationKind.TagName, resourceUri, sourceText),
            new TextEditorTextSpan(44, 47, (byte)HtmlDecorationKind.TagName, resourceUri, sourceText),
            new TextEditorTextSpan(76, 78, (byte)HtmlDecorationKind.TagName, resourceUri, sourceText),
            new TextEditorTextSpan(106, 108, (byte)HtmlDecorationKind.TagName, resourceUri, sourceText),
            new TextEditorTextSpan(119, 120, (byte)HtmlDecorationKind.TagName, resourceUri, sourceText),
            new TextEditorTextSpan(154, 155, (byte)HtmlDecorationKind.TagName, resourceUri, sourceText),
            new TextEditorTextSpan(166, 172, (byte)HtmlDecorationKind.TagName, resourceUri, sourceText),
            new TextEditorTextSpan(196, 202, (byte)HtmlDecorationKind.TagName, resourceUri, sourceText),
            new TextEditorTextSpan(210, 213, (byte)HtmlDecorationKind.TagName, resourceUri, sourceText),
            new TextEditorTextSpan(217, 220, (byte)HtmlDecorationKind.TagName, resourceUri, sourceText),
        };

        var razorLexer = new TextEditorRazorLexer(resourceUri);

        var textEditorTextSpans = await razorLexer.Lex(
            sourceText,
            RenderStateKey.NewRenderStateKey());

        textEditorTextSpans = textEditorTextSpans
            .Where(x => x.DecorationByte == (byte)HtmlDecorationKind.TagName)
            .OrderBy(x => x.StartingIndexInclusive)
            .ToImmutableArray();

        Assert.Equal(expectedTagNameTextEditorTextSpans, textEditorTextSpans);
    }

    [Fact]
    public async Task LexInjectedLanguageKeywords()
    {
        var sourceText = TestData.Razor.EXAMPLE_TEXT
            .ReplaceLineEndings("\n");

        var resourceUri = new ResourceUri(string.Empty);

        var expectedKeywordTextEditorTextSpans = new[]
        {
            new TextEditorTextSpan(250, 256, (byte)HtmlDecorationKind.InjectedLanguageKeyword, resourceUri, sourceText),
            new TextEditorTextSpan(288, 291, (byte)HtmlDecorationKind.InjectedLanguageKeyword, resourceUri, sourceText),
            new TextEditorTextSpan(293, 296, (byte)HtmlDecorationKind.InjectedLanguageKeyword, resourceUri, sourceText),
            new TextEditorTextSpan(321, 327, (byte)HtmlDecorationKind.InjectedLanguageKeyword, resourceUri, sourceText),
            new TextEditorTextSpan(328, 334, (byte)HtmlDecorationKind.InjectedLanguageKeyword, resourceUri, sourceText),
            new TextEditorTextSpan(344, 347, (byte)HtmlDecorationKind.InjectedLanguageKeyword, resourceUri, sourceText),
            new TextEditorTextSpan(349, 352, (byte)HtmlDecorationKind.InjectedLanguageKeyword, resourceUri, sourceText),
            new TextEditorTextSpan(361, 368, (byte)HtmlDecorationKind.InjectedLanguageKeyword, resourceUri, sourceText),
            new TextEditorTextSpan(369, 373, (byte)HtmlDecorationKind.InjectedLanguageKeyword, resourceUri, sourceText),
        };

        var razorLexer = new TextEditorRazorLexer(resourceUri);

        var textEditorTextSpans = await razorLexer.Lex(
            sourceText,
            RenderStateKey.NewRenderStateKey());

        textEditorTextSpans = textEditorTextSpans
            .Where(x => x.DecorationByte == (byte)HtmlDecorationKind.InjectedLanguageKeyword)
            .OrderBy(x => x.StartingIndexInclusive)
            .ToImmutableArray();

        Assert.Equal(expectedKeywordTextEditorTextSpans, textEditorTextSpans);
    }
}