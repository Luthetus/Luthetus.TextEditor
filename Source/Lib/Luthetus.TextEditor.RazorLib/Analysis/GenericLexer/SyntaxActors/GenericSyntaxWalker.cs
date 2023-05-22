﻿using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.SyntaxEnums;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.SyntaxObjects;

namespace Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.SyntaxActors;

public class GenericSyntaxWalker
{
    public List<GenericStringSyntax> GenericStringSyntaxes { get; } = new();
    public List<GenericCommentSingleLineSyntax> GenericCommentSingleLineSyntaxes { get; } = new();
    public List<GenericCommentMultiLineSyntax> GenericCommentMultiLineSyntaxes { get; } = new();
    public List<GenericKeywordSyntax> GenericKeywordSyntaxes { get; } = new();
    public List<GenericFunctionSyntax> GenericFunctionSyntaxes { get; } = new();
    public List<GenericPreprocessorDirectiveSyntax> GenericPreprocessorDirectiveSyntaxes { get; } = new();
    public List<GenericDeliminationExtendedSyntax> GenericDeliminationExtendedSyntaxes { get; } = new();

    public void Visit(IGenericSyntax node)
    {
        foreach (var child in node.Children)
        {
            Visit(child);
        }

        switch (node.GenericSyntaxKind)
        {
            case GenericSyntaxKind.StringLiteral:
                VisitGenericStringSyntax((GenericStringSyntax)node);
                break;
            case GenericSyntaxKind.CommentSingleLine:
                VisitGenericCommentSingleLineSyntax((GenericCommentSingleLineSyntax)node);
                break;
            case GenericSyntaxKind.CommentMultiLine:
                VisitGenericCommentMultiLineSyntax((GenericCommentMultiLineSyntax)node);
                break;
            case GenericSyntaxKind.Keyword:
                VisitGenericKeywordSyntax((GenericKeywordSyntax)node);
                break;
            case GenericSyntaxKind.Function:
                VisitGenericFunctionSyntax((GenericFunctionSyntax)node);
                break;
            case GenericSyntaxKind.PreprocessorDirective:
                VisitGenericPreprocessorDirectiveSyntax((GenericPreprocessorDirectiveSyntax)node);
                break;
            case GenericSyntaxKind.DeliminationExtended:
                VisitGenericDeliminationExtendedSyntax((GenericDeliminationExtendedSyntax)node);
                break;
        }
    }

    private void VisitGenericStringSyntax(GenericStringSyntax node)
    {
        GenericStringSyntaxes.Add(node);
    }

    private void VisitGenericCommentSingleLineSyntax(GenericCommentSingleLineSyntax node)
    {
        GenericCommentSingleLineSyntaxes.Add(node);
    }

    private void VisitGenericCommentMultiLineSyntax(GenericCommentMultiLineSyntax node)
    {
        GenericCommentMultiLineSyntaxes.Add(node);
    }

    private void VisitGenericKeywordSyntax(GenericKeywordSyntax node)
    {
        GenericKeywordSyntaxes.Add(node);
    }

    private void VisitGenericFunctionSyntax(GenericFunctionSyntax node)
    {
        GenericFunctionSyntaxes.Add(node);
    }

    private void VisitGenericPreprocessorDirectiveSyntax(GenericPreprocessorDirectiveSyntax node)
    {
        GenericPreprocessorDirectiveSyntaxes.Add(node);
    }

    private void VisitGenericDeliminationExtendedSyntax(GenericDeliminationExtendedSyntax node)
    {
        GenericDeliminationExtendedSyntaxes.Add(node);
    }
}