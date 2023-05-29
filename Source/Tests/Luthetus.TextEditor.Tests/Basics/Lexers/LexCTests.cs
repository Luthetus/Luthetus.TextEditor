using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Analysis.C.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.Decoration;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.Tests.Basics.Lexers;

public class LexCTests
{
    [Fact]
    public async Task SHOULD_LEX_COMMENTS_SINGLE_LINE()
    {
        string sourceText = @"#include <stdlib.h>
#include <stdio.h>

// C:\Users\hunte\Repos\Aaa\

int main()
{
	int x = 42;

	/*
		A Multi-Line Comment
	*/
	
	/* Another Multi-Line Comment */

	printf(""%d"", x);
}".ReplaceLineEndings("\n");

        var resourceUri = new ResourceUri(string.Empty);

        var expectedTextEditorTextSpans = new TextEditorTextSpan[]
        {
            new(40, 68, (byte)GenericDecorationKind.CommentSingleLine, resourceUri, sourceText),
        };

        var cLexer = new TextEditorCLexer(resourceUri);

        var textEditorTextSpans = await cLexer.Lex(
            sourceText,
            RenderStateKey.NewRenderStateKey());

        textEditorTextSpans = textEditorTextSpans
            .Where(x => x.DecorationByte == (byte)GenericDecorationKind.CommentSingleLine)
            .OrderBy(x => x.StartingIndexInclusive)
            .ToImmutableArray();

        Assert.Equal(expectedTextEditorTextSpans, textEditorTextSpans);
    }

    [Fact]
    public async Task SHOULD_LEX_COMMENTS_MULTI_LINE()
    {
        string sourceText = @"#include <stdlib.h>
#include <stdio.h>

// C:\Users\hunte\Repos\Aaa\

int main()
{
	int x = 42;

	/*
		A Multi-Line Comment
	*/
	
	/* Another Multi-Line Comment */

	printf(""%d"", x);
}".ReplaceLineEndings("\n");

        var resourceUri = new ResourceUri(string.Empty);

        var expectedTextEditorTextSpans = new TextEditorTextSpan[]
        {
            new(98, 127, (byte)GenericDecorationKind.CommentMultiLine, resourceUri, sourceText),
            new(131, 163, (byte)GenericDecorationKind.CommentMultiLine, resourceUri, sourceText),
        };

        var cLexer = new TextEditorCLexer(resourceUri);

        var textEditorTextSpans = await cLexer.Lex(
            sourceText,
            RenderStateKey.NewRenderStateKey());

        textEditorTextSpans = textEditorTextSpans
            .Where(x => x.DecorationByte == (byte)GenericDecorationKind.CommentMultiLine)
            .OrderBy(x => x.StartingIndexInclusive)
            .ToImmutableArray();

        Assert.Equal(expectedTextEditorTextSpans, textEditorTextSpans);
    }

    [Fact]
    public async Task SHOULD_LEX_KEYWORDS()
    {
        string sourceText = @"#include <stdlib.h>
#include <stdio.h>

// C:\Users\hunte\Repos\Aaa\

int main()
{
	int x = 42;

	/*
		A Multi-Line Comment
	*/
	
	/* Another Multi-Line Comment */

	printf(""%d"", x);
}".ReplaceLineEndings("\n");

        var resourceUri = new ResourceUri(string.Empty);

        var expectedTextEditorTextSpans = new TextEditorTextSpan[]
        {
            new(70, 73, (byte)GenericDecorationKind.Keyword, resourceUri, sourceText),
            new(84, 87, (byte)GenericDecorationKind.Keyword, resourceUri, sourceText),
        };

        var cLexer = new TextEditorCLexer(resourceUri);

        var textEditorTextSpans = await cLexer.Lex(
            sourceText,
            RenderStateKey.NewRenderStateKey());

        textEditorTextSpans = textEditorTextSpans
            .Where(x => x.DecorationByte == (byte)GenericDecorationKind.Keyword)
            .OrderBy(x => x.StartingIndexInclusive)
            .ToImmutableArray();

        Assert.Equal(expectedTextEditorTextSpans, textEditorTextSpans);
    }

    [Fact]
    public async Task SHOULD_LEX_STRING_LITERALS()
    {
        string sourceText = @"#include <stdlib.h>
#include <stdio.h>

// C:\Users\hunte\Repos\Aaa\

int main()
{
	int x = 42;

	/*
		A Multi-Line Comment
	*/
	
	/* Another Multi-Line Comment */

	printf(""%d"", x);
}".ReplaceLineEndings("\n");

        var resourceUri = new ResourceUri(string.Empty);

        var expectedTextEditorTextSpans = new TextEditorTextSpan[]
        {
            new(173, 177, (byte)GenericDecorationKind.StringLiteral, resourceUri, sourceText),
        };

        var cLexer = new TextEditorCLexer(resourceUri);

        var textEditorTextSpans = await cLexer.Lex(
            sourceText,
            RenderStateKey.NewRenderStateKey());

        textEditorTextSpans = textEditorTextSpans
            .Where(x => x.DecorationByte == (byte)GenericDecorationKind.StringLiteral)
            .OrderBy(x => x.StartingIndexInclusive)
            .ToImmutableArray();

        Assert.Equal(expectedTextEditorTextSpans, textEditorTextSpans);
    }

    [Fact]
    public async Task SHOULD_LEX_FUNCTIONS()
    {
        string sourceText = @"#include <stdlib.h>
#include <stdio.h>

// C:\Users\hunte\Repos\Aaa\

int main()
{
	int x = 42;

	/*
		A Multi-Line Comment
	*/
	
	/* Another Multi-Line Comment */

	printf(""%d"", x);
}".ReplaceLineEndings("\n");

        var resourceUri = new ResourceUri(string.Empty);

        var expectedTextEditorTextSpans = new TextEditorTextSpan[]
        {
            new(74, 78, (byte)GenericDecorationKind.Function, resourceUri, sourceText),
            new(166, 172, (byte)GenericDecorationKind.Function, resourceUri, sourceText),
        };

        var cLexer = new TextEditorCLexer(resourceUri);

        var textEditorTextSpans = await cLexer.Lex(
            sourceText,
            RenderStateKey.NewRenderStateKey());

        textEditorTextSpans = textEditorTextSpans
            .Where(x => x.DecorationByte == (byte)GenericDecorationKind.Function)
            .OrderBy(x => x.StartingIndexInclusive)
            .ToImmutableArray();

        Assert.Equal(expectedTextEditorTextSpans, textEditorTextSpans);
    }

    [Fact]
    public async Task SHOULD_LEX_PREPROCESSOR_DIRECTIVES()
    {
        string sourceText = @"#include <stdlib.h>
#include <stdio.h>

// C:\Users\hunte\Repos\Aaa\

int main()
{
	int x = 42;

	/*
		A Multi-Line Comment
	*/
	
	/* Another Multi-Line Comment */

	printf(""%d"", x);
}".ReplaceLineEndings("\n");

        var resourceUri = new ResourceUri(string.Empty);

        var expectedTextEditorTextSpans = new TextEditorTextSpan[]
        {
            new(0, 8, (byte)GenericDecorationKind.PreprocessorDirective, resourceUri, sourceText),
            new(20, 28, (byte)GenericDecorationKind.PreprocessorDirective, resourceUri, sourceText),
        };

        var cLexer = new TextEditorCLexer(resourceUri);

        var textEditorTextSpans = await cLexer.Lex(
            sourceText,
            RenderStateKey.NewRenderStateKey());

        textEditorTextSpans = textEditorTextSpans
            .Where(x => x.DecorationByte == (byte)GenericDecorationKind.PreprocessorDirective)
            .OrderBy(x => x.StartingIndexInclusive)
            .ToImmutableArray();

        Assert.Equal(expectedTextEditorTextSpans, textEditorTextSpans);
    }
}