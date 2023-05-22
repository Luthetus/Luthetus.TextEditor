﻿using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxObjects;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html;

public class HtmlSyntaxUnit
{
    public HtmlSyntaxUnit(
        TagSyntax rootTagSyntax,
        TextEditorHtmlDiagnosticBag textEditorHtmlDiagnosticBag)
    {
        TextEditorHtmlDiagnosticBag = textEditorHtmlDiagnosticBag;
        RootTagSyntax = rootTagSyntax;
    }

    public TagSyntax RootTagSyntax { get; }
    public TextEditorHtmlDiagnosticBag TextEditorHtmlDiagnosticBag { get; }

    public class HtmlSyntaxUnitBuilder
    {
        public HtmlSyntaxUnitBuilder(TagSyntax rootTagSyntax, TextEditorHtmlDiagnosticBag textEditorHtmlDiagnosticBag)
        {
            RootTagSyntax = rootTagSyntax;
            TextEditorHtmlDiagnosticBag = textEditorHtmlDiagnosticBag;
        }

        public TagSyntax RootTagSyntax { get; }
        public TextEditorHtmlDiagnosticBag TextEditorHtmlDiagnosticBag { get; }

        public HtmlSyntaxUnit Build()
        {
            return new HtmlSyntaxUnit(
                RootTagSyntax,
                TextEditorHtmlDiagnosticBag);
        }
    }
}