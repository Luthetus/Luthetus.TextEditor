﻿using System.Collections.Immutable;
using System.Text;
using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Analysis.Html.Decoration;
using Luthetus.TextEditor.RazorLib.Analysis.Html.ExtensionMethods;
using Luthetus.TextEditor.RazorLib.Analysis.Html.Facts;
using Luthetus.TextEditor.RazorLib.Analysis.Html.InjectedLanguage;
using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxEnums;
using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxObjects;
using Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxObjects.Builders;
using Luthetus.TextEditor.RazorLib.Analysis.GenericLexer.Decoration;

namespace Luthetus.TextEditor.RazorLib.Analysis.Html.SyntaxActors;

public static class HtmlSyntaxTree
{
    public static HtmlSyntaxUnit ParseText(
        ResourceUri resourceUri,
        string content,
        InjectedLanguageDefinition? injectedLanguageDefinition = null)
    {
        var stringWalker = new StringWalker(resourceUri, content);

        var rootTagSyntaxBuilder = new TagNodeBuilder
        {
            OpenTagNameSyntax = new TagNameNode(
                new TextEditorTextSpan(
                    0,
                    8,
                    (byte)HtmlDecorationKind.None,
                    new ResourceUri(string.Empty),
                    "document")),
        };

        var textEditorHtmlDiagnosticBag = new TextEditorHtmlDiagnosticBag();

        rootTagSyntaxBuilder.Children = HtmlSyntaxTreeStateMachine
            .ParseTagChildContent(
                stringWalker,
                textEditorHtmlDiagnosticBag,
                injectedLanguageDefinition);

        var htmlSyntaxUnitBuilder = new HtmlSyntaxUnit.HtmlSyntaxUnitBuilder(
            rootTagSyntaxBuilder.Build(),
            textEditorHtmlDiagnosticBag);

        return htmlSyntaxUnitBuilder.Build();
    }

    public static class HtmlSyntaxTreeStateMachine
    {
        /// <summary>Invocation of this method requires the stringWalker to have <see cref="StringWalker.PeekCharacter" /> of 0 be equal to <see cref="HtmlFacts.OPEN_TAG_BEGINNING" /></summary>
        public static IHtmlSyntaxNode ParseTag(
            StringWalker stringWalker,
            TextEditorHtmlDiagnosticBag textEditorHtmlDiagnosticBag,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            if (stringWalker.CheckForSubstring(
                    HtmlFacts.COMMENT_TAG_BEGINNING))
            {
                return ParseComment(
                    stringWalker,
                    textEditorHtmlDiagnosticBag,
                    injectedLanguageDefinition);
            }

            var startingPositionIndex = stringWalker.PositionIndex;

            var tagBuilder = new TagNodeBuilder();

            // HtmlFacts.TAG_OPENING_CHARACTER
            _ = stringWalker.ReadCharacter();

            // Example: <!DOCTYPE html>
            if (stringWalker.PeekCharacter(0) == HtmlFacts.SPECIAL_HTML_TAG)
            {
                // HtmlFacts.SPECIAL_HTML_TAG_CHARACTER
                stringWalker.ReadCharacter();

                tagBuilder.HasSpecialHtmlCharacter = true;
            }

            tagBuilder.OpenTagNameSyntax = ParseTagName(
                stringWalker,
                textEditorHtmlDiagnosticBag,
                injectedLanguageDefinition);

            // Get all html attributes break when see End Of File or closing of the tag
            while (true)
            {
                // Skip Whitespace
                while (!stringWalker.IsEof)
                {
                    if (WhitespaceFacts.ALL.Contains(stringWalker.CurrentCharacter))
                        _ = stringWalker.ReadCharacter();
                    else
                        break;
                }

                // End Of File is unexpected at this point so report a diagnostic.
                if (stringWalker.CurrentCharacter == ParserFacts.END_OF_FILE)
                {
                    textEditorHtmlDiagnosticBag.ReportEndOfFileUnexpected(
                        new TextEditorTextSpan(
                            startingPositionIndex,
                            stringWalker.PositionIndex,
                            (byte)HtmlDecorationKind.Error,
                            stringWalker.ResourceUri,
                            stringWalker.SourceText));

                    return tagBuilder.Build();
                }

                if (stringWalker.CheckForSubstring(HtmlFacts.OPEN_TAG_WITH_CHILD_CONTENT_ENDING))
                {
                    // Ending of opening tag
                    tagBuilder.HtmlSyntaxKind = HtmlSyntaxKind.TagOpeningNode;

                    // Skip the '>' character to set stringWalker at the first character of the child content
                    _ = stringWalker.ReadCharacter();

                    tagBuilder.Children = ParseTagChildContent(
                        stringWalker,
                        textEditorHtmlDiagnosticBag,
                        injectedLanguageDefinition);

                    // TODO: check that the closing tag name matches the opening tag
                }
                else if (stringWalker.CheckForSubstring(HtmlFacts.OPEN_TAG_SELF_CLOSING_ENDING))
                {
                    _ = stringWalker.ReadRange(
                            HtmlFacts.OPEN_TAG_SELF_CLOSING_ENDING
                                .Length);

                    // Ending of self-closing tag
                    tagBuilder.HtmlSyntaxKind = HtmlSyntaxKind.TagSelfClosingNode;

                    return tagBuilder.Build();
                }
                else if (stringWalker.CheckForSubstring(HtmlFacts.CLOSE_TAG_WITH_CHILD_CONTENT_BEGINNING))
                {
                    tagBuilder.HtmlSyntaxKind = HtmlSyntaxKind.TagClosingNode;

                    _ = stringWalker.ReadRange(
                        HtmlFacts.CLOSE_TAG_WITH_CHILD_CONTENT_BEGINNING
                            .Length);

                    var closeTagNameStartingPositionIndex = stringWalker.PositionIndex;

                    var closeTagNameBuilder = new StringBuilder();

                    stringWalker.WhileNotEndOfFile(() =>
                    {
                        if (stringWalker.CheckForSubstring(
                                HtmlFacts.CLOSE_TAG_WITH_CHILD_CONTENT_ENDING))
                        {
                            tagBuilder.CloseTagNameSyntax = new TagNameNode(
                                new TextEditorTextSpan(
                                    closeTagNameStartingPositionIndex,
                                    stringWalker.PositionIndex,
                                    (byte)HtmlDecorationKind.TagName,
                                    stringWalker.ResourceUri,
                                    stringWalker.SourceText));

                            _ = stringWalker.ReadRange(
                                    HtmlFacts.CLOSE_TAG_WITH_CHILD_CONTENT_ENDING
                                        .Length);

                            return true;
                        }

                        closeTagNameBuilder.Append(stringWalker.CurrentCharacter);
                        return false;
                    });

                    if (tagBuilder.CloseTagNameSyntax is null)
                    {
                        // TODO: Not sure if this can happen but I am getting a warning about this and aim to get to this when I find time.
                        throw new NotImplementedException();
                    }

                    if (tagBuilder.OpenTagNameSyntax.TextEditorTextSpan.GetText() != tagBuilder.CloseTagNameSyntax.TextEditorTextSpan.GetText())
                    {
                        textEditorHtmlDiagnosticBag.ReportOpenTagWithUnMatchedCloseTag(
                            tagBuilder.OpenTagNameSyntax.TextEditorTextSpan.GetText(),
                            tagBuilder.CloseTagNameSyntax.TextEditorTextSpan.GetText(),
                            new TextEditorTextSpan(
                                closeTagNameStartingPositionIndex,
                                stringWalker.PositionIndex,
                                (byte)HtmlDecorationKind.Error,
                                stringWalker.ResourceUri,
                                stringWalker.SourceText));
                    }

                    return tagBuilder.Build();
                }
                else
                {
                    // Attribute
                    var attributeSyntax = ParseAttribute(
                        stringWalker,
                        textEditorHtmlDiagnosticBag,
                        injectedLanguageDefinition);

                    tagBuilder.AttributeSyntaxes.Add(attributeSyntax);
                }
            }
        }

        /// <summary>Invocation of this method requires the stringWalker to have <see cref="StringWalker.PeekCharacter" /> of 0 be equal to the first character that is part of the tag's name</summary>
        public static TagNameNode ParseTagName(
            StringWalker stringWalker,
            TextEditorHtmlDiagnosticBag textEditorHtmlDiagnosticBag,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            var startingPositionIndex = stringWalker.PositionIndex;

            var tagNameBuilder = new StringBuilder();

            stringWalker.WhileNotEndOfFile(() =>
            {
                if (stringWalker.CheckForSubstringRange(
                        HtmlFacts.TAG_NAME_STOP_DELIMITERS,
                        out var matchedOn))
                    return true;

                tagNameBuilder.Append(stringWalker.CurrentCharacter);

                return false;
            });

            var tagName = tagNameBuilder.ToString();

            if (tagNameBuilder.Length == 0)
            {
                if (stringWalker.CurrentCharacter == ParserFacts.END_OF_FILE)
                {
                    textEditorHtmlDiagnosticBag.ReportEndOfFileUnexpected(
                        new TextEditorTextSpan(
                            startingPositionIndex,
                            stringWalker.PositionIndex,
                            (byte)HtmlDecorationKind.Error,
                            stringWalker.ResourceUri,
                            stringWalker.SourceText));
                }
                else
                {
                    // Report a diagnostic for the missing 'tag name'
                    textEditorHtmlDiagnosticBag.ReportTagNameMissing(
                        new TextEditorTextSpan(
                            startingPositionIndex,
                            stringWalker.PositionIndex,
                            (byte)HtmlDecorationKind.Error,
                            stringWalker.ResourceUri,
                            stringWalker.SourceText));

                    // Fabricate a value for the string variable: 'tagName' so the rest of the file can still be parsed.
                    tagName =
                        $"__{nameof(textEditorHtmlDiagnosticBag.ReportTagNameMissing)}__";
                }
            }

            return new TagNameNode(
                new TextEditorTextSpan(
                    startingPositionIndex,
                    stringWalker.PositionIndex,
                    (byte)HtmlDecorationKind.TagName,
                    stringWalker.ResourceUri,
                    stringWalker.SourceText));
        }

        public static List<IHtmlSyntax> ParseTagChildContent(
            StringWalker stringWalker,
            TextEditorHtmlDiagnosticBag textEditorHtmlDiagnosticBag,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            var startingPositionIndex = stringWalker.PositionIndex;

            List<IHtmlSyntax> htmlSyntaxes = new();

            int? textNodeStartingPositionIndex = null;

            // Make a TagTextSyntax - HTML TextNode if there was anything in the current builder
            void AddTextNode()
            {
                if (textNodeStartingPositionIndex is null)
                    return;

                var tagTextSyntax = new TextNode(
                    new TextEditorTextSpan(
                        textNodeStartingPositionIndex.Value,
                        stringWalker.PositionIndex,
                        (byte)GenericDecorationKind.None,
                        stringWalker.ResourceUri,
                        stringWalker.SourceText));

                htmlSyntaxes.Add(tagTextSyntax);
                textNodeStartingPositionIndex = null;
            }

            stringWalker.WhileNotEndOfFile(() =>
            {
                if (stringWalker.CheckForSubstring(
                        HtmlFacts.CLOSE_TAG_WITH_CHILD_CONTENT_BEGINNING))
                    return true;

                if (stringWalker.CurrentCharacter ==
                    HtmlFacts.OPEN_TAG_BEGINNING)
                {
                    // If there is text in textNodeBuilder add a new TextNode to the List of TagSyntax
                    AddTextNode();

                    if (stringWalker.CheckForSubstring(
                            HtmlFacts.COMMENT_TAG_BEGINNING))
                    {
                        htmlSyntaxes.Add(
                            ParseComment(
                                stringWalker,
                                textEditorHtmlDiagnosticBag,
                                injectedLanguageDefinition));
                    }
                    else
                    {
                        htmlSyntaxes.Add(
                            ParseTag(
                                stringWalker,
                                textEditorHtmlDiagnosticBag,
                                injectedLanguageDefinition));
                    }

                    return false;
                }

                if (injectedLanguageDefinition is not null && stringWalker
                        .CheckForInjectedLanguageCodeBlockTag(injectedLanguageDefinition))
                {
                    // If there is text in textNodeBuilder add a new TextNode to the List of TagSyntax
                    AddTextNode();

                    htmlSyntaxes.AddRange(
                        ParseInjectedLanguageCodeBlock(
                            stringWalker,
                            textEditorHtmlDiagnosticBag,
                            injectedLanguageDefinition));

                    return false;
                }

                textNodeStartingPositionIndex ??= stringWalker.PositionIndex;

                return false;
            });

            if (stringWalker.CurrentCharacter == ParserFacts.END_OF_FILE)
            {
                textEditorHtmlDiagnosticBag.ReportEndOfFileUnexpected(
                    new TextEditorTextSpan(
                        startingPositionIndex,
                        stringWalker.PositionIndex,
                        (byte)HtmlDecorationKind.Error,
                        stringWalker.ResourceUri,
                        stringWalker.SourceText));
            }

            // If there is text in textNodeBuilder add a new TextNode to the List of TagSyntax
            AddTextNode();

            return htmlSyntaxes;
        }

        public static List<IHtmlSyntaxNode> ParseInjectedLanguageCodeBlock(
            StringWalker stringWalker,
            TextEditorHtmlDiagnosticBag textEditorHtmlDiagnosticBag,
            InjectedLanguageDefinition injectedLanguageDefinition)
        {
            var injectedLanguageFragmentSyntaxes = new List<IHtmlSyntaxNode>();

            var injectedLanguageFragmentSyntaxStartingPositionIndex = stringWalker.PositionIndex;

            // Track text span of the "@" sign (example in .razor files)
            injectedLanguageFragmentSyntaxes.Add(
                new InjectedLanguageFragmentNode(
                    ImmutableArray<IHtmlSyntax>.Empty,
                    new TextEditorTextSpan(
                        injectedLanguageFragmentSyntaxStartingPositionIndex,
                        stringWalker.PositionIndex + 1,
                        (byte)HtmlDecorationKind.InjectedLanguageFragment,
                        stringWalker.ResourceUri,
                        stringWalker.SourceText)));

            injectedLanguageFragmentSyntaxes.AddRange(
                injectedLanguageDefinition.ParseInjectedLanguageFunc
                    .Invoke(
                        stringWalker,
                        textEditorHtmlDiagnosticBag,
                        injectedLanguageDefinition));

            return injectedLanguageFragmentSyntaxes;
        }

        public static AttributeNode ParseAttribute(
            StringWalker stringWalker,
            TextEditorHtmlDiagnosticBag textEditorHtmlDiagnosticBag,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            var attributeNameSyntax = ParseAttributeName(
                stringWalker,
                textEditorHtmlDiagnosticBag,
                injectedLanguageDefinition);

            _ = TryReadAttributeValue(
                    stringWalker,
                    textEditorHtmlDiagnosticBag,
                    injectedLanguageDefinition,
                    out var attributeValueSyntax);

            return new AttributeNode(
                attributeNameSyntax,
                attributeValueSyntax);
        }

        /// <summary>currentCharacterIn:<br/> -Any character that can start an attribute name<br/> currentCharacterOut:<br/> -<see cref="WhitespaceFacts.ALL"/> (whitespace)<br/> -<see cref="HtmlFacts.SEPARATOR_FOR_ATTRIBUTE_NAME_AND_ATTRIBUTE_VALUE"/><br/> -<see cref="HtmlFacts.OPEN_TAG_ENDING_OPTIONS"/></summary>
        public static AttributeNameNode ParseAttributeName(
            StringWalker stringWalker,
            TextEditorHtmlDiagnosticBag textEditorHtmlDiagnosticBag,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            // When ParseAttributeName is invoked the PositionIndex is always 1 character too far
            _ = stringWalker.BacktrackCharacter();

            var startingPositionIndex = stringWalker.PositionIndex;

            while (!stringWalker.IsEof)
            {
                _ = stringWalker.ReadCharacter();

                if (WhitespaceFacts.ALL.Contains(stringWalker.CurrentCharacter) ||
                    HtmlFacts.SEPARATOR_FOR_ATTRIBUTE_NAME_AND_ATTRIBUTE_VALUE == stringWalker.CurrentCharacter ||
                    stringWalker.CheckForSubstringRange(HtmlFacts.OPEN_TAG_ENDING_OPTIONS, out var matchedOn))
                {
                    break;
                }
            }

            var attributeNameTextSpan = new TextEditorTextSpan(
                startingPositionIndex,
                stringWalker.PositionIndex,
                (byte)HtmlDecorationKind.AttributeName,
                stringWalker.ResourceUri,
                stringWalker.SourceText);

            return new AttributeNameNode(attributeNameTextSpan);
        }

        /// <summary>Returns placeholder match attribute value if fails to read an attribute value<br/> <br/> currentCharacterIn:<br/> -<see cref="WhitespaceFacts.ALL"/> (whitespace)<br/> -<see cref="HtmlFacts.SEPARATOR_FOR_ATTRIBUTE_NAME_AND_ATTRIBUTE_VALUE"/><br/> -<see cref="HtmlFacts.OPEN_TAG_ENDING_OPTIONS"/><br/> currentCharacterOut:<br/> -<see cref="HtmlFacts.ATTRIBUTE_VALUE_ENDING"/><br/> -<see cref="HtmlFacts.OPEN_TAG_ENDING_OPTIONS"/></summary>
        private static bool TryReadAttributeValue(
            StringWalker stringWalker,
            TextEditorHtmlDiagnosticBag textEditorHtmlDiagnosticBag,
            InjectedLanguageDefinition? injectedLanguageDefinition,
            out AttributeValueNode attributeValueSyntax)
        {
            if (WhitespaceFacts.ALL.Contains(stringWalker.CurrentCharacter))
            {
                // Move to the first non-whitespace
                while (!stringWalker.IsEof)
                {
                    _ = stringWalker.ReadCharacter();

                    if (!WhitespaceFacts.ALL.Contains(stringWalker.CurrentCharacter))
                        break;
                }
            }

            if (HtmlFacts.SEPARATOR_FOR_ATTRIBUTE_NAME_AND_ATTRIBUTE_VALUE == stringWalker.CurrentCharacter)
            {
                attributeValueSyntax = ParseAttributeValue(
                    stringWalker,
                    textEditorHtmlDiagnosticBag,
                    injectedLanguageDefinition);

                return true;
            }

            // Set out variable as a 'matched attribute value' so there aren't any cascading error diagnostics due to having expected an attribute value.
            var attributeValueTextSpan = new TextEditorTextSpan(
                0,
                0,
                (byte)HtmlDecorationKind.AttributeValue,
                stringWalker.ResourceUri,
                stringWalker.SourceText);

            attributeValueSyntax = new AttributeValueNode(attributeValueTextSpan);

            return false;
        }

        /// <summary> currentCharacterIn:<br/> -<see cref="HtmlFacts.SEPARATOR_FOR_ATTRIBUTE_NAME_AND_ATTRIBUTE_VALUE"/><br/> currentCharacterOut:<br/> -<see cref="HtmlFacts.ATTRIBUTE_VALUE_ENDING"/></summary>
        public static AttributeValueNode ParseAttributeValue(
            StringWalker stringWalker,
            TextEditorHtmlDiagnosticBag textEditorHtmlDiagnosticBag,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            var startingPositionIndex = stringWalker.PositionIndex;

            // Move to the first non-whitespace which follows the HtmlFacts.SEPARATOR_FOR_ATTRIBUTE_NAME_AND_ATTRIBUTE_VALUE
            while (!stringWalker.IsEof)
            {
                _ = stringWalker.ReadCharacter();

                if (!WhitespaceFacts.ALL.Contains(stringWalker.CurrentCharacter))
                    break;
            }

            var foundOpenTagEnding = stringWalker.CheckForSubstringRange(
                HtmlFacts.OPEN_TAG_ENDING_OPTIONS,
                out _);

            if (!foundOpenTagEnding)
            {
                var beganWithAttributeValueStarting =
                    HtmlFacts.ATTRIBUTE_VALUE_STARTING == stringWalker.CurrentCharacter;

                while (!stringWalker.IsEof)
                {
                    _ = stringWalker.ReadCharacter();

                    if (!beganWithAttributeValueStarting &&
                        WhitespaceFacts.ALL.Contains(stringWalker.CurrentCharacter))
                    {
                        break;
                    }

                    if (stringWalker.CheckForSubstringRange(
                            HtmlFacts.OPEN_TAG_ENDING_OPTIONS,
                            out _))
                    {
                        foundOpenTagEnding = true;
                        break;
                    }

                    if (HtmlFacts.ATTRIBUTE_VALUE_ENDING == stringWalker.CurrentCharacter)
                        break;
                }
            }

            var endingIndexExclusive = stringWalker.PositionIndex;

            if (!foundOpenTagEnding)
                endingIndexExclusive++;

            var attributeValueTextSpan = new TextEditorTextSpan(
                startingPositionIndex,
                endingIndexExclusive,
                (byte)HtmlDecorationKind.AttributeValue,
                stringWalker.ResourceUri,
                stringWalker.SourceText);

            return new AttributeValueNode(attributeValueTextSpan);
        }

        public static CommentNode ParseComment(
            StringWalker stringWalker,
            TextEditorHtmlDiagnosticBag textEditorHtmlDiagnosticBag,
            InjectedLanguageDefinition? injectedLanguageDefinition)
        {
            var startingPositionIndex = stringWalker.PositionIndex;

            while (!stringWalker.IsEof)
            {
                _ = stringWalker.ReadCharacter();

                if (stringWalker.CheckForSubstring(HtmlFacts.COMMENT_TAG_ENDING))
                    break;
            }

            // Skip the remaining characters in the comment tag ending string
            _ = stringWalker.ReadRange(HtmlFacts.COMMENT_TAG_ENDING.Length - 1);

            var commentTagTextSpan = new TextEditorTextSpan(
                startingPositionIndex,
                stringWalker.PositionIndex + 1,
                (byte)HtmlDecorationKind.Comment,
                stringWalker.ResourceUri,
                stringWalker.SourceText);

            return new CommentNode(commentTagTextSpan);
        }
    }
}