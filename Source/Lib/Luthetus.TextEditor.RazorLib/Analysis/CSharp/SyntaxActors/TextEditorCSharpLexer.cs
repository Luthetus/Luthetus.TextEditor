﻿using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Misc;
using Luthetus.TextEditor.RazorLib.Analysis.CSharp.Facts;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.SyntaxActors;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis.CSharp.SyntaxActors;

public class TextEditorCSharpLexer
{
    public static readonly GenericPreprocessorDefinition CSharpPreprocessorDefinition = new(
        "#",
        ImmutableArray<DeliminationExtendedSyntaxDefinition>.Empty);

    public static readonly GenericLanguageDefinition CSharpLanguageDefinition = new GenericLanguageDefinition(
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
        "/*",
        "*/",
        CSharpKeywords.ALL,
        CSharpPreprocessorDefinition);

    private readonly GenericSyntaxTree _cSharpSyntaxTree;

    public TextEditorCSharpLexer(ResourceUri resourceUri)
    {
        _cSharpSyntaxTree = new GenericSyntaxTree(CSharpLanguageDefinition);
        ResourceUri = resourceUri;
    }

    public RenderStateKey ModelRenderStateKey { get; private set; } = RenderStateKey.Empty;
    public ResourceUri ResourceUri { get; }

    public Task<ImmutableArray<TextEditorTextSpan>> Lex(
        string sourceText,
        RenderStateKey modelRenderStateKey)
    {
        var cSharpSyntaxUnit = _cSharpSyntaxTree.ParseText(
            ResourceUri,
            sourceText);

        var cSharpSyntaxWalker = new GenericSyntaxWalker();

        cSharpSyntaxWalker.Visit(cSharpSyntaxUnit.GenericDocumentSyntax);

        var textEditorTextSpans = new List<TextEditorTextSpan>();

        textEditorTextSpans
            .AddRange(cSharpSyntaxWalker.GenericStringSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(cSharpSyntaxWalker.GenericCommentSingleLineSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(cSharpSyntaxWalker.GenericCommentMultiLineSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(cSharpSyntaxWalker.GenericKeywordSyntaxes
                .Select(x => x.TextEditorTextSpan));

        textEditorTextSpans
            .AddRange(cSharpSyntaxWalker.GenericFunctionSyntaxes
                .Select(x => x.TextEditorTextSpan));

        return Task.FromResult(textEditorTextSpans.ToImmutableArray());
    }
}