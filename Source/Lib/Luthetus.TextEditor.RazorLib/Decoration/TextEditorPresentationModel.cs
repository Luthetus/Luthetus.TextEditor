﻿using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Decoration;

/// <param name="TextEditorPresentationKey">Unique Identifier. Perhaps one wants to repaint the Diff presentation only. They use this to identify the Diff presentation</param>
/// <param name="Rank">The order the presentations get rendered. A given a ranks of [ 7, -3, 1, 2 ] the rendering order is ascending therefore it will be reordered as [ -3, 1, 2, 7 ] when iterating over the containing presentation layer</param>
/// <param name="CssClassString">A div is rendered with this css class. Inside this div are the divs gotten from the text spans. One likely only will use this css class as an identifier while viewing the browser's debugging tools.</param>
/// <param name="DecorationMapper">The decoration mapper that is responsible for converting a decoration byte to a css class</param>
/// <param name="TextEditorTextSpans">The list of position indices that are to painted with certain decoration bytes.</param>
public record TextEditorPresentationModel(
    TextEditorPresentationKey TextEditorPresentationKey,
    int Rank,
    string CssClassString,
    IDecorationMapper DecorationMapper,
    ImmutableList<TextEditorTextSpan> TextEditorTextSpans);