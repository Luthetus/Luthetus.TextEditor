﻿using System.Collections;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis;

public class TextEditorDiagnosticBag : IEnumerable<TextEditorDiagnostic>
{
    private readonly List<TextEditorDiagnostic> _textEditorDiagnostics = new();

    public IEnumerator<TextEditorDiagnostic> GetEnumerator()
    {
        return _textEditorDiagnostics.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Report(TextEditorDiagnosticLevel diagnosticLevel,
        string message,
        TextEditorTextSpan textEditorTextSpan)
    {
        _textEditorDiagnostics.Add(
            new TextEditorDiagnostic(
                diagnosticLevel,
                message,
                textEditorTextSpan));
    }

    public void ReportEndOfFileUnexpected(TextEditorTextSpan textEditorTextSpan)
    {
        Report(
            TextEditorDiagnosticLevel.Error,
            "'End of file' was unexpected.",
            textEditorTextSpan);
    }

    public void ReportUnexpectedToken(
        TextEditorTextSpan textEditorTextSpan,
        string unexpectedToken)
    {
        Report(
            TextEditorDiagnosticLevel.Error,
            $"Unexpected token: '{unexpectedToken}'",
            textEditorTextSpan);
    }
}