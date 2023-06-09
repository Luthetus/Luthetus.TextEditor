﻿using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Analysis.FSharp.Facts;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.FSharp.SyntaxActors;

public class TextEditorFSharpLexer
{
    public static readonly GenericPreprocessorDefinition FSharpPreprocessorDefinition = new(
        "#",
        ImmutableArray<DeliminationExtendedSyntaxDefinition>.Empty);

    public static readonly GenericLanguageDefinition FSharpLanguageDefinition = new GenericLanguageDefinition(
        "\"",
        "\"",
        "(",
        ")",
        ".",
        "//",
        new[]
        {
            WhitespaceFacts.CARRIAGE_RETURN.ToString(),
            WhitespaceFacts.LINE_FEED.ToString()
        }.ToImmutableArray(),
        "(*",
        "*)",
        FSharpKeywords.ALL,
        FSharpPreprocessorDefinition);

    private readonly GenericSyntaxTree _fSharpSyntaxTree;

    public TextEditorFSharpLexer(ResourceUri resourceUri)
    {
        _fSharpSyntaxTree = new GenericSyntaxTree(FSharpLanguageDefinition);
        ResourceUri = resourceUri;
    }

    public RenderStateKey ModelRenderStateKey { get; private set; } = RenderStateKey.Empty;

    public ResourceUri ResourceUri { get; }

    public Task<ImmutableArray<TextEditorTextSpan>> Lex(
        string sourceText,
        RenderStateKey modelRenderStateKey)
    {
        var fSharpSyntaxUnit = _fSharpSyntaxTree.ParseText(
            ResourceUri,
            sourceText);

        var fSharpSyntaxWalker = new GenericSyntaxWalker();

        fSharpSyntaxWalker.Visit(fSharpSyntaxUnit.GenericDocumentSyntax);

        var textEditorTextSpans = new List<TextEditorTextSpan>();

        textEditorTextSpans
            .AddRange(fSharpSyntaxWalker.GenericStringSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(fSharpSyntaxWalker.GenericCommentSingleLineSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(fSharpSyntaxWalker.GenericCommentMultiLineSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(fSharpSyntaxWalker.GenericKeywordSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(fSharpSyntaxWalker.GenericFunctionSyntaxes
                .Select(x => x.TextEditorTextSpan));

        return Task.FromResult(textEditorTextSpans.ToImmutableArray());
    }
}