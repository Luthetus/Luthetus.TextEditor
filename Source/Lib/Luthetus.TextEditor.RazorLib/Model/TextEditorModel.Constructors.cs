﻿using Luthetus.TextEditor.RazorLib.Keymap.Default;
using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Character;
using Luthetus.TextEditor.RazorLib.Decoration;
using Luthetus.TextEditor.RazorLib.Keymap;
using Luthetus.TextEditor.RazorLib.CompilerServiceCase;

namespace Luthetus.TextEditor.RazorLib.Model;

/// <summary>Stores the <see cref="RichCharacter"/> class instances that represent the text.<br/><br/>Each TextEditorModel has a unique underlying resource uri.<br/><br/>Therefore, if one has a text file named "myHomework.txt", then only one TextEditorModel can exist with the resource uri of "myHomework.txt".</summary>
public partial class TextEditorModel
{
    public TextEditorModel(
        ResourceUri resourceUri,
        DateTime resourceLastWriteTime,
        string fileExtension,
        string content,
        ICompilerService? compilerService,
        IDecorationMapper? decorationMapper,
        ITextEditorKeymap? textEditorKeymap,
        TextEditorSaveFileHelper textEditorSaveFileHelper)
    {
        ResourceUri = resourceUri;
        ResourceLastWriteTime = resourceLastWriteTime;
        FileExtension = fileExtension;
        CompilerService = compilerService ?? new TextEditorCompilerServiceDefault();
        DecorationMapper = decorationMapper ?? new TextEditorDecorationMapperDefault();
        TextEditorKeymap = textEditorKeymap ?? new TextEditorKeymapDefault();
        TextEditorSaveFileHelper = textEditorSaveFileHelper;
        
        SetContent(content);
    }

    public TextEditorModel(
        ResourceUri resourceUri,
        DateTime resourceLastWriteTime,
        string fileExtension,
        string content,
        ICompilerService? compilerService,
        IDecorationMapper? decorationMapper,
        ITextEditorKeymap? textEditorKeymap,
        TextEditorSaveFileHelper textEditorSaveFileHelper,
        TextEditorModelKey modelKey)
        : this(
            resourceUri,
            resourceLastWriteTime,
            fileExtension,
            content,
            compilerService,
            decorationMapper,
            textEditorKeymap,
            textEditorSaveFileHelper)
    {
        ModelKey = modelKey;
    }

    /// <summary>Clone the TextEditorModel using shallow copy so that Fluxor will notify all the <see cref="TextEditorView"/> of the <see cref="TextEditorModel"/> having been replaced<br/><br/>Do not use a record would that do a deep value comparison and be incredibly slow? (i.e.) compare every RichCharacter in the list.</summary>
    public TextEditorModel(TextEditorModel original)
    {
        ResourceUri = original.ResourceUri;
        ResourceLastWriteTime = original.ResourceLastWriteTime;
        FileExtension = original.FileExtension;
        _content = original._content;
        _editBlocksPersisted = original._editBlocksPersisted;
        _rowEndingKindCounts = original._rowEndingKindCounts;
        _rowEndingPositions = original._rowEndingPositions;
        _tabKeyPositions = original._tabKeyPositions;
        ModelKey = original.ModelKey;

        OnlyRowEndingKind = original.OnlyRowEndingKind;
        UsingRowEndingKind = original.UsingRowEndingKind;
        CompilerService = original.CompilerService;
        DecorationMapper = original.DecorationMapper;
        TextEditorKeymap = original.TextEditorKeymap;
        TextEditorSaveFileHelper = original.TextEditorSaveFileHelper;
        EditBlockIndex = original.EditBlockIndex;
        MostCharactersOnASingleRowTuple = original.MostCharactersOnASingleRowTuple;
        TextEditorOptions = original.TextEditorOptions;
    }
}
