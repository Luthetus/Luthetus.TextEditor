﻿using System.Collections.Immutable;
using System.Text;
using Luthetus.Common.RazorLib.Keyboard;
using Luthetus.TextEditor.RazorLib.Model;
using Luthetus.TextEditor.RazorLib.Lexing;

namespace Luthetus.TextEditor.RazorLib.Analysis;

/// <summary>Provides common API that can be used when implementing an <see cref="ITextEditorLexer" /> for the <see cref="TextEditorModel" />.<br /><br />The marker for an out of bounds read is <see cref="ParserFacts.END_OF_FILE" />.</summary>
public class StringWalker
{
    /// <summary>Pass in the <see cref="ResourceUri"/> of a file, and its text. One can pass in <see cref="string.Empty"/> for the <see cref="ResourceUri"/> if they are only working with the text itself.</summary>
    public StringWalker(
        ResourceUri resourceUri,
        string sourceText)
    {
        SourceText = sourceText;
        ResourceUri = resourceUri;
    }

    /// <summary>The character index within the <see cref="SourceText" />.</summary>
    public int PositionIndex { get; private set; }

    /// <summary>The file that the <see cref="SourceText"/> came from.</summary>
    public ResourceUri ResourceUri { get; }

    /// <summary>The text which is to be stepped through..</summary>
    public string SourceText { get; }

    /// <summary>Returns <see cref="PeekCharacter" /> invoked with the value of zero</summary>
    public char CurrentCharacter => PeekCharacter(0);

    /// <summary>Returns <see cref="PeekCharacter" /> invoked with the value of one</summary>
    public char NextCharacter => PeekCharacter(1);

    /// <summary>Starting with <see cref="PeekCharacter" /> evaluated at 0 return that and the rest of the <see cref="SourceText" /><br /><br /><see cref="RemainingText" /> => SourceText.Substring(PositionIndex);</summary>
    public string RemainingText => SourceText.Substring(PositionIndex);

    /// <summary>Returns if the current character is the end of file character</summary>
    public bool IsEof => CurrentCharacter == ParserFacts.END_OF_FILE;

    /// <summary>If <see cref="PositionIndex" /> is within bounds of the <see cref="SourceText" />.<br /><br />Then the character within the string <see cref="SourceText" /> at index of <see cref="PositionIndex" /> is returned and <see cref="PositionIndex" /> is incremented by one.<br /><br />Otherwise, <see cref="ParserFacts.END_OF_FILE" /> is returned and the value of <see cref="PositionIndex" /> is unchanged.</summary>
    public char ReadCharacter()
    {
        if (PositionIndex >= SourceText.Length)
            return ParserFacts.END_OF_FILE;

        return SourceText[PositionIndex++];
    }

    /// <summary>If (<see cref="PositionIndex" /> + <see cref="offset" />) is within bounds of the <see cref="SourceText" />.<br /><br />Then the character within the string <see cref="SourceText" /> at index of (<see cref="PositionIndex" /> + <see cref="offset" />) is returned and <see cref="PositionIndex" /> is unchanged.<br /><br />Otherwise, <see cref="ParserFacts.END_OF_FILE" /> is returned and the value of <see cref="PositionIndex" /> is unchanged.<br /><br />offset must be > -1</summary>
    public char PeekCharacter(int offset)
    {
        if (offset <= -1)
            throw new ApplicationException($"{nameof(offset)} must be > -1");

        if (PositionIndex + offset >= SourceText.Length)
            return ParserFacts.END_OF_FILE;

        return SourceText[PositionIndex + offset];
    }

    /// <summary>If <see cref="PositionIndex" /> being decremented by 1 would result in <see cref="PositionIndex" /> being less than 0.<br /><br />Then <see cref="ParserFacts.END_OF_FILE" /> will be returned and <see cref="PositionIndex" /> will be left unchanged.<br /><br />Otherwise, <see cref="PositionIndex" /> will be decremented by one and the character within the string <see cref="SourceText" /> at index of <see cref="PositionIndex" /> is returned.</summary>
    public char BacktrackCharacter()
    {
        if (PositionIndex == 0)
            return ParserFacts.END_OF_FILE;

        PositionIndex--;

        return PeekCharacter(0);
    }

    /// <summary>Iterates a counter from 0 until the counter is equal to <see cref="length" />.<br /><br />Each iteration <see cref="ReadCharacter" /> will be invoked.<br /><br />If an iteration's invocation of <see cref="ReadCharacter" /> returned <see cref="ParserFacts.END_OF_FILE" /> then the method will short circuit and return regardless of whether it finished iterating to <see cref="length" /> or not.</summary>
    public string ReadRange(int length)
    {
        var consumeBuilder = new StringBuilder();

        for (var i = 0; i < length; i++)
        {
            var currentCharacter = ReadCharacter();

            consumeBuilder.Append(currentCharacter);

            if (currentCharacter == ParserFacts.END_OF_FILE)
                break;
        }

        return consumeBuilder.ToString();
    }

    /// <summary>Iterates a counter from 0 until the counter is equal to <see cref="length" />.<br /><br />Each iteration <see cref="PeekCharacter" /> will be invoked using the (<see cref="offset" /> + counter).<br /><br />If an iteration's invocation of <see cref="PeekCharacter" /> returned <see cref="ParserFacts.END_OF_FILE" /> then the method will short circuit and return regardless of whether it finished iterating to <see cref="length" /> or not.</summary>
    public string PeekRange(int offset, int length)
    {
        var peekBuilder = new StringBuilder();

        for (var i = 0; i < length; i++)
        {
            var currentCharacter = PeekCharacter(offset + i);

            peekBuilder.Append(currentCharacter);

            if (currentCharacter == ParserFacts.END_OF_FILE)
                break;
        }

        return peekBuilder.ToString();
    }

    /// <summary>Iterates a counter from 0 until the counter is equal to <see cref="length" />.<br /><br />Each iteration <see cref="BacktrackCharacter" /> will be invoked using the.<br /><br />If an iteration's invocation of <see cref="BacktrackCharacter" /> returned <see cref="ParserFacts.END_OF_FILE" /> then the method will short circuit and return regardless of whether it finished iterating to <see cref="length" /> or not.</summary>
    public string BacktrackRange(int length)
    {
        var backtrackBuilder = new StringBuilder();

        for (var i = 0; i < length; i++)
        {
            if (PositionIndex == 0)
            {
                backtrackBuilder.Append(ParserFacts.END_OF_FILE);
                return backtrackBuilder.ToString();
            }

            var currentCharacter = BacktrackCharacter();

            backtrackBuilder.Append(currentCharacter);

            if (currentCharacter == ParserFacts.END_OF_FILE)
                break;
        }

        return backtrackBuilder.ToString();
    }

    public string PeekNextWord()
    {
        var nextWordBuilder = new StringBuilder();

        var i = 0;

        char peekedChar;

        do
        {
            peekedChar = PeekCharacter(i++);

            if (WhitespaceFacts.ALL.Contains(peekedChar) ||
                KeyboardKeyFacts.IsPunctuationCharacter(peekedChar))
            {
                break;
            }

            nextWordBuilder.Append(peekedChar);
        } while (peekedChar != ParserFacts.END_OF_FILE);

        return nextWordBuilder.ToString();
    }

    /// <summary>Form a substring of the <see cref="SourceText" /> that starts inclusively at the index <see cref="PositionIndex" /> and has a maximum length of <see cref="substring" />.Length.<br /><br />This method uses <see cref="PeekRange" /> internally and therefore will return a string that ends with <see cref="ParserFacts.END_OF_FILE" /> if an index out of bounds read was performed on <see cref="SourceText" /></summary>
    public bool CheckForSubstring(string substring)
    {
        var peekedSubstring = PeekRange(
            0,
            substring.Length);

        return peekedSubstring == substring;
    }

    public bool CheckForSubstringRange(ImmutableArray<string> substrings, out string? matchedOn)
    {
        foreach (var substring in substrings)
        {
            if (CheckForSubstring(substring))
            {
                matchedOn = substring;
                return true;
            }
        }

        matchedOn = null;
        return false;
    }

    public void WhileNotEndOfFile(Func<bool> shouldBreakFunc)
    {
        while (CurrentCharacter != ParserFacts.END_OF_FILE)
        {
            if (shouldBreakFunc.Invoke())
                break;

            _ = ReadCharacter();
        }
    }

    /// <summary><see cref="ConsumeWord"/> will return immediately upon encountering whitespace.</summary>
    public (TextEditorTextSpan textSpan, string value) ConsumeWord(
        ImmutableArray<char>? additionalCharactersToBreakOn = null)
    {
        additionalCharactersToBreakOn ??= ImmutableArray<char>.Empty;

        // The wordBuilder is appended to everytime a
        // character is consumed.
        var wordBuilder = new StringBuilder();

        // wordBuilderStartingIndexInclusive == -1 is to mean
        // that wordBuilder is empty. Once the first letter or digit
        // (non whitespace) is read, then the wordBuilderStartingIndexInclusive
        // will be set to a value other than -1.
        var wordBuilderStartingIndexInclusive = -1;

        WhileNotEndOfFile(() =>
        {
            if (WhitespaceFacts.ALL.Contains(CurrentCharacter) ||
                additionalCharactersToBreakOn.Value.Contains(CurrentCharacter))
            {
                return true;
            }

            if (wordBuilderStartingIndexInclusive == -1)
            {
                // This is the start of a word
                // as opposed to the continuation of a word

                wordBuilderStartingIndexInclusive = PositionIndex;
            }

            wordBuilder.Append(CurrentCharacter);

            return false;
        });

        return (new TextEditorTextSpan(
            wordBuilderStartingIndexInclusive,
            PositionIndex,
            0,
            ResourceUri,
            SourceText),
                wordBuilder.ToString());
    }
}