using System.Collections.Immutable;
using Luthetus.Common.RazorLib.Keyboard;
using Luthetus.TextEditor.RazorLib.Character;
using Luthetus.TextEditor.RazorLib.Lexing;
using Luthetus.TextEditor.RazorLib.Store.Model;
using Luthetus.TextEditor.RazorLib.Commands;
using Luthetus.TextEditor.RazorLib.Cursor;
using Luthetus.TextEditor.RazorLib.Editing;
using Luthetus.TextEditor.RazorLib.Row;

namespace Luthetus.TextEditor.RazorLib.Model;

public partial class TextEditorModel
{
    private void EnsureUndoPoint(
        TextEditKind textEditKind,
        string? otherTextEditKindIdentifier = null)
    {
        if (textEditKind == TextEditKind.Other &&
            otherTextEditKindIdentifier is null)
        {
            TextEditorCommand.ThrowOtherTextEditKindIdentifierWasExpectedException(
                textEditKind);
        }

        var mostRecentEditBlock = _editBlocksPersisted.LastOrDefault();

        if (mostRecentEditBlock is null ||
            mostRecentEditBlock.TextEditKind != textEditKind)
        {
            var newEditBlockIndex = EditBlockIndex;
            
            _editBlocksPersisted.Insert(newEditBlockIndex, new EditBlock(
                textEditKind,
                textEditKind.ToString(),
                GetAllText(),
                otherTextEditKindIdentifier));

            var removeBlocksStartingAt = newEditBlockIndex + 1;
            
            _editBlocksPersisted.RemoveRange(
                removeBlocksStartingAt,
                _editBlocksPersisted.Count - removeBlocksStartingAt);
            
            EditBlockIndex++;
        }

        while (_editBlocksPersisted.Count > MAXIMUM_EDIT_BLOCKS &&
               _editBlocksPersisted.Count != 0)
        {
            EditBlockIndex--;
            _editBlocksPersisted.RemoveAt(0);
        }
    }

    private void PerformInsertions(TextEditorModelsCollection.KeyboardEventAction keyboardEventAction)
    {
        EnsureUndoPoint(TextEditKind.Insertion);

        foreach (var cursorSnapshot in keyboardEventAction.CursorSnapshots)
        {
            if (TextEditorSelectionHelper.HasSelectedText(
                    cursorSnapshot.ImmutableCursor.ImmutableSelection))
            {
                PerformDeletions(keyboardEventAction);

                var selectionBounds = TextEditorSelectionHelper.GetSelectionBounds(
                    cursorSnapshot.ImmutableCursor.ImmutableSelection);

                var lowerRowData = 
                    FindRowInformation(
                        selectionBounds.lowerPositionIndexInclusive);

                var lowerColumnIndex = selectionBounds.lowerPositionIndexInclusive -
                                       lowerRowData.rowStartPositionIndex;

                // Move cursor to lower bound of text selection
                cursorSnapshot.UserCursor.IndexCoordinates = 
                    (lowerRowData.rowIndex, lowerColumnIndex);
                
                var nextEdit = keyboardEventAction with
                {
                    CursorSnapshots = new []
                    {
                        new TextEditorCursorSnapshot(cursorSnapshot.UserCursor)
                    }.ToImmutableArray()
                };

                // cannot move reference of foreach variable
                // have to invoke the method again with different parameters
                PerformInsertions(nextEdit);
                return;
            }
            
            var startOfRowPositionIndex =
                GetStartOfRowTuple(cursorSnapshot.ImmutableCursor.RowIndex)
                    .positionIndex;

            var cursorPositionIndex =
                startOfRowPositionIndex + cursorSnapshot.ImmutableCursor.ColumnIndex;
            
            // If cursor is out of bounds then continue
            if (cursorPositionIndex > _content.Count)
                continue;

            var wasTabCode = false;
            var wasEnterCode = false;

            var characterValueToInsert = keyboardEventAction.KeyboardEventArgs.Key
                .First();

            if (KeyboardKeyFacts.IsWhitespaceCode(keyboardEventAction.KeyboardEventArgs.Code))
            {
                characterValueToInsert =
                    KeyboardKeyFacts.ConvertWhitespaceCodeToCharacter(keyboardEventAction.KeyboardEventArgs.Code);

                wasTabCode = KeyboardKeyFacts.WhitespaceCodes.TAB_CODE ==
                             keyboardEventAction.KeyboardEventArgs.Code;

                wasEnterCode = KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE ==
                               keyboardEventAction.KeyboardEventArgs.Code;
            }

            var characterCountInserted = 1;

            if (wasEnterCode)
            {
                var rowEndingKindToInsert = UsingRowEndingKind;

                var richCharacters = rowEndingKindToInsert
                    .AsCharacters()
                    .Select(character => new RichCharacter
                    {
                        Value = character,
                        DecorationByte = default,
                    });

                characterCountInserted = rowEndingKindToInsert.AsCharacters().Length;
                
                _content.InsertRange(cursorPositionIndex, richCharacters);

                _rowEndingPositions.Insert(cursorSnapshot.ImmutableCursor.RowIndex,
                    (cursorPositionIndex + characterCountInserted, rowEndingKindToInsert));

                MutateRowEndingKindCount(
                    UsingRowEndingKind,
                    1);

                var indexCoordinates = cursorSnapshot.UserCursor.IndexCoordinates;

                cursorSnapshot.UserCursor.IndexCoordinates = (indexCoordinates.rowIndex + 1, 0);

                cursorSnapshot.UserCursor.PreferredColumnIndex =
                    cursorSnapshot.UserCursor.IndexCoordinates.columnIndex;
            }
            else
            {
                if (wasTabCode)
                {
                    var index = _tabKeyPositions
                        .FindIndex(x =>
                            x >= cursorPositionIndex);

                    if (index == -1)
                        _tabKeyPositions.Add(cursorPositionIndex);
                    else
                    {
                        for (var i = index; i < _tabKeyPositions.Count; i++) _tabKeyPositions[i]++;

                        _tabKeyPositions.Insert(index, cursorPositionIndex);
                    }
                }

                var richCharacterToInsert = new RichCharacter
                {
                    Value = characterValueToInsert,
                    DecorationByte = default,
                };

                _content.Insert(cursorPositionIndex, richCharacterToInsert);

                var indexCoordinates = cursorSnapshot.UserCursor.IndexCoordinates;

                cursorSnapshot.UserCursor.IndexCoordinates =
                    (indexCoordinates.rowIndex, indexCoordinates.columnIndex + 1);
                cursorSnapshot.UserCursor.PreferredColumnIndex =
                    cursorSnapshot.UserCursor.IndexCoordinates.columnIndex;
            }

            var firstRowIndexToModify = wasEnterCode
                ? cursorSnapshot.ImmutableCursor.RowIndex + 1
                : cursorSnapshot.ImmutableCursor.RowIndex;

            for (var i = firstRowIndexToModify; i < _rowEndingPositions.Count; i++)
            {
                var rowEndingTuple = _rowEndingPositions[i];

                _rowEndingPositions[i] = (rowEndingTuple.positionIndex + characterCountInserted,
                    rowEndingTuple.rowEndingKind);
            }

            if (!wasTabCode)
            {
                var firstTabKeyPositionIndexToModify = _tabKeyPositions
                    .FindIndex(x => x >= cursorPositionIndex);

                if (firstTabKeyPositionIndexToModify != -1)
                {
                    for (var i = firstTabKeyPositionIndexToModify; i < _tabKeyPositions.Count; i++)
                        _tabKeyPositions[i] += characterCountInserted;
                }
            }

            //// Reposition the Diagnostic Squigglies
            /// // TODO: (2023-05-21 | 2:49 AM) Super tired and at this point am just getting nowhere. I tried this but it didn't work.
            //{
            //    var localSemanticModel = SemanticModel;

            //    if (localSemanticModel is not null)
            //    {
            //        var diagnosticTextSpans = new List<TextEditorTextSpan>();

            //        foreach (var diagnostic in localSemanticModel.DiagnosticTextSpans)
            //        {
            //            if (diagnostic.StartingIndexInclusive >= cursorPositionIndex)
            //            {
            //                diagnosticTextSpans.Add(diagnostic with
            //                {
            //                    StartingIndexInclusive = diagnostic.StartingIndexInclusive + characterCountInserted,
            //                    EndingIndexExclusive = diagnostic.EndingIndexExclusive + characterCountInserted,
            //                });
            //            }
            //            else
            //            {
            //                diagnosticTextSpans.Add(diagnostic);
            //            }
            //        }

            //        localSemanticModel.DiagnosticTextSpans = diagnosticTextSpans
            //            .ToImmutableList();
            //    }
            //}
        }
        
        // TODO: Fix tracking the MostCharactersOnASingleRowTuple this way is possibly inefficient - should instead only check the rows that changed
        {
            (int rowIndex, int rowLength) localMostCharactersOnASingleRowTuple = (0, 0);
                
            for (var i = 0; i < _rowEndingPositions.Count; i++)
            {
                var lengthOfRow = GetLengthOfRow(i);

                if (lengthOfRow > localMostCharactersOnASingleRowTuple.rowLength)
                {
                    localMostCharactersOnASingleRowTuple = (i, lengthOfRow);
                }
            }
            
            localMostCharactersOnASingleRowTuple = 
                (localMostCharactersOnASingleRowTuple.rowIndex,
                    localMostCharactersOnASingleRowTuple.rowLength + MOST_CHARACTERS_ON_A_SINGLE_ROW_MARGIN);
            
            MostCharactersOnASingleRowTuple = localMostCharactersOnASingleRowTuple;
        }
    }

    private void PerformDeletions(TextEditorModelsCollection.KeyboardEventAction keyboardEventAction)
    {
        EnsureUndoPoint(TextEditKind.Deletion);

        foreach (var cursorSnapshot in keyboardEventAction.CursorSnapshots)
        {
            var startOfRowPositionIndex =
                GetStartOfRowTuple(cursorSnapshot.ImmutableCursor.RowIndex)
                    .positionIndex;

            var cursorPositionIndex =
                startOfRowPositionIndex + cursorSnapshot.ImmutableCursor.ColumnIndex;
            
            // If cursor is out of bounds then continue
            if (cursorPositionIndex > _content.Count)
                continue;

            int startingPositionIndexToRemoveInclusive;
            int countToRemove;
            bool moveBackwards;

            // Cannot calculate this after text was deleted - it would be wrong
            int? selectionUpperBoundRowIndex = null;
            // Needed for when text selection is deleted
            (int rowIndex, int columnIndex)? selectionLowerBoundIndexCoordinates = null;

            // TODO: The deletion logic should be the same whether it be 'Delete' 'Backspace' 'CtrlModified' or 'DeleteSelection'. What should change is one needs to calculate the starting and ending index appropriately foreach case.
            if (TextEditorSelectionHelper.HasSelectedText(
                    cursorSnapshot.ImmutableCursor.ImmutableSelection))
            {
                var lowerPositionIndexInclusiveBound = cursorSnapshot.ImmutableCursor.ImmutableSelection
                    .AnchorPositionIndex ?? 0;

                var upperPositionIndexExclusive = cursorSnapshot.ImmutableCursor.ImmutableSelection
                    .EndingPositionIndex;

                if (lowerPositionIndexInclusiveBound > upperPositionIndexExclusive)
                    (lowerPositionIndexInclusiveBound, upperPositionIndexExclusive) = (upperPositionIndexExclusive, lowerPositionIndexInclusiveBound);

                var lowerRowMetaData =
                    FindRowInformation(
                        lowerPositionIndexInclusiveBound);

                var upperRowMetaData =
                    FindRowInformation(
                        upperPositionIndexExclusive);

                // Value is needed when knowing what row ending positions
                // to update after deletion is done
                selectionUpperBoundRowIndex = upperRowMetaData.rowIndex;

                // Value is needed when knowing where to position the
                // cursor after deletion is done
                selectionLowerBoundIndexCoordinates =
                    (lowerRowMetaData.rowIndex,
                        lowerPositionIndexInclusiveBound - lowerRowMetaData.rowStartPositionIndex);

                startingPositionIndexToRemoveInclusive = upperPositionIndexExclusive - 1;
                countToRemove = upperPositionIndexExclusive - lowerPositionIndexInclusiveBound;
                moveBackwards = true;

                cursorSnapshot.UserCursor.Selection.AnchorPositionIndex = null;
            }
            else if (KeyboardKeyFacts.MetaKeys.BACKSPACE == keyboardEventAction.KeyboardEventArgs.Key)
            {
                moveBackwards = true;

                if (keyboardEventAction.KeyboardEventArgs.CtrlKey)
                {
                    var columnIndexOfCharacterWithDifferingKind = GetColumnIndexOfCharacterWithDifferingKind(
                        cursorSnapshot.ImmutableCursor.RowIndex,
                        cursorSnapshot.ImmutableCursor.ColumnIndex,
                        moveBackwards);

                    columnIndexOfCharacterWithDifferingKind =
                        columnIndexOfCharacterWithDifferingKind == -1
                            ? 0
                            : columnIndexOfCharacterWithDifferingKind;
                    
                    countToRemove =
                        cursorSnapshot.ImmutableCursor.ColumnIndex -
                        columnIndexOfCharacterWithDifferingKind;

                    countToRemove = countToRemove == 0
                        ? 1
                        : countToRemove;
                }
                else
                {
                    countToRemove = 1;
                }
                
                startingPositionIndexToRemoveInclusive = cursorPositionIndex - 1;
            }
            else if (KeyboardKeyFacts.MetaKeys.DELETE == keyboardEventAction.KeyboardEventArgs.Key)
            {
                moveBackwards = false;
                
                if (keyboardEventAction.KeyboardEventArgs.CtrlKey)
                {
                    var columnIndexOfCharacterWithDifferingKind = GetColumnIndexOfCharacterWithDifferingKind(
                        cursorSnapshot.ImmutableCursor.RowIndex,
                        cursorSnapshot.ImmutableCursor.ColumnIndex,
                        moveBackwards);

                    columnIndexOfCharacterWithDifferingKind =
                        columnIndexOfCharacterWithDifferingKind == -1
                            ? GetLengthOfRow(cursorSnapshot.ImmutableCursor.RowIndex)
                            : columnIndexOfCharacterWithDifferingKind;
                    
                    countToRemove =
                        columnIndexOfCharacterWithDifferingKind -
                        cursorSnapshot.ImmutableCursor.ColumnIndex;
                    
                    countToRemove = countToRemove == 0
                        ? 1
                        : countToRemove;
                }
                else
                {
                    countToRemove = 1;
                }
                
                startingPositionIndexToRemoveInclusive = cursorPositionIndex;
            }
            else
            {
                throw new ApplicationException(
                    $"The keyboard key: {keyboardEventAction.KeyboardEventArgs.Key} was not recognized");
            }

            var charactersRemovedCount = 0;
            var rowsRemovedCount = 0;

            var indexToRemove = startingPositionIndexToRemoveInclusive;

            while (countToRemove-- > 0)
            {
                if (indexToRemove < 0 ||
                    indexToRemove > _content.Count - 1)
                    break;

                var characterToDelete = _content[indexToRemove];

                int startingIndexToRemoveRange;
                int countToRemoveRange;

                if (KeyboardKeyFacts.IsLineEndingCharacter(characterToDelete.Value))
                {
                    rowsRemovedCount++;

                    // rep.positionIndex == indexToRemove + 1
                    //     ^is for backspace
                    //
                    // rep.positionIndex == indexToRemove + 2
                    //     ^is for delete
                    var rowEndingTupleIndex = _rowEndingPositions
                        .FindIndex(rep =>
                            rep.positionIndex == indexToRemove + 1 ||
                            rep.positionIndex == indexToRemove + 2);

                    var rowEndingTuple = _rowEndingPositions[rowEndingTupleIndex];

                    _rowEndingPositions.RemoveAt(rowEndingTupleIndex);

                    var lengthOfRowEnding = rowEndingTuple.rowEndingKind
                        .AsCharacters().Length;

                    if (moveBackwards)
                        startingIndexToRemoveRange = indexToRemove - (lengthOfRowEnding - 1);
                    else
                        startingIndexToRemoveRange = indexToRemove;

                    countToRemove -= lengthOfRowEnding - 1;
                    countToRemoveRange = lengthOfRowEnding;

                    MutateRowEndingKindCount(
                        rowEndingTuple.rowEndingKind,
                        -1);
                }
                else
                {
                    if (characterToDelete.Value == KeyboardKeyFacts.WhitespaceCharacters.TAB)
                        _tabKeyPositions.Remove(indexToRemove);

                    startingIndexToRemoveRange = indexToRemove;
                    countToRemoveRange = 1;
                }

                charactersRemovedCount += countToRemoveRange;

                _content.RemoveRange(startingIndexToRemoveRange, countToRemoveRange);

                if (moveBackwards)
                    indexToRemove -= countToRemoveRange;
            }

            if (charactersRemovedCount == 0 &&
                rowsRemovedCount == 0)
                return;

            if (moveBackwards && !selectionUpperBoundRowIndex.HasValue)
            {
                var modifyRowsBy = -1 * rowsRemovedCount;

                var startOfCurrentRowPositionIndex = GetStartOfRowTuple(
                        cursorSnapshot.ImmutableCursor.RowIndex + modifyRowsBy)
                    .positionIndex;

                var modifyPositionIndexBy = -1 * charactersRemovedCount;

                var endingPositionIndex = cursorPositionIndex + modifyPositionIndexBy;

                var columnIndex = endingPositionIndex - startOfCurrentRowPositionIndex;

                var indexCoordinates = cursorSnapshot.UserCursor.IndexCoordinates;

                cursorSnapshot.UserCursor.IndexCoordinates =
                    (indexCoordinates.rowIndex + modifyRowsBy,
                        columnIndex);
            }

            int firstRowIndexToModify;

            if (selectionUpperBoundRowIndex.HasValue)
            {
                firstRowIndexToModify = selectionLowerBoundIndexCoordinates!.Value.rowIndex;

                cursorSnapshot.UserCursor.IndexCoordinates =
                    selectionLowerBoundIndexCoordinates!.Value;
            }
            else if (moveBackwards)
                firstRowIndexToModify = cursorSnapshot.ImmutableCursor.RowIndex - rowsRemovedCount;
            else
                firstRowIndexToModify = cursorSnapshot.ImmutableCursor.RowIndex;

            for (var i = firstRowIndexToModify; i < _rowEndingPositions.Count; i++)
            {
                var rowEndingTuple = _rowEndingPositions[i];

                _rowEndingPositions[i] = (rowEndingTuple.positionIndex - charactersRemovedCount,
                    rowEndingTuple.rowEndingKind);
            }

            var firstTabKeyPositionIndexToModify = _tabKeyPositions
                .FindIndex(x => x >= startingPositionIndexToRemoveInclusive);

            if (firstTabKeyPositionIndexToModify != -1)
            {
                for (var i = firstTabKeyPositionIndexToModify; i < _tabKeyPositions.Count; i++)
                    _tabKeyPositions[i] -= charactersRemovedCount;
            }

            //// Reposition the Diagnostic Squigglies
            /// // TODO: (2023-05-21 | 2:49 AM) Super tired and at this point am just getting nowhere. I tried this but it didn't work.
            //{
            //    var localSemanticModel = SemanticModel;

            //    if (localSemanticModel is not null)
            //    {
            //        var diagnosticTextSpans = new List<TextEditorTextSpan>();

            //        foreach (var diagnostic in localSemanticModel.DiagnosticTextSpans)
            //        {
            //            if (diagnostic.StartingIndexInclusive >= cursorPositionIndex)
            //            {
            //                diagnosticTextSpans.Add(diagnostic with
            //                {
            //                    StartingIndexInclusive = diagnostic.StartingIndexInclusive - charactersRemovedCount,
            //                    EndingIndexExclusive = diagnostic.EndingIndexExclusive - charactersRemovedCount,
            //                });
            //            }
            //            else
            //            {
            //                diagnosticTextSpans.Add(diagnostic);
            //            }
            //        }

            //        localSemanticModel.DiagnosticTextSpans = diagnosticTextSpans
            //            .ToImmutableList();
            //    }
            //}
        }

        // TODO: Fix tracking the MostCharactersOnASingleRowTuple this way is possibly inefficient - should instead only check the rows that changed
        {
            (int rowIndex, int rowLength) localMostCharactersOnASingleRowTuple = (0, 0);
                
            for (var i = 0; i < _rowEndingPositions.Count; i++)
            {
                var lengthOfRow = GetLengthOfRow(i);

                if (lengthOfRow > localMostCharactersOnASingleRowTuple.rowLength)
                {
                    localMostCharactersOnASingleRowTuple = (i, lengthOfRow);
                }
            }
            
            localMostCharactersOnASingleRowTuple = 
                (localMostCharactersOnASingleRowTuple.rowIndex,
                    localMostCharactersOnASingleRowTuple.rowLength + MOST_CHARACTERS_ON_A_SINGLE_ROW_MARGIN);
            
            MostCharactersOnASingleRowTuple = localMostCharactersOnASingleRowTuple;
        }
    }

    private void MutateRowEndingKindCount(RowEndingKind rowEndingKind, int changeBy)
    {
        var indexOfRowEndingKindCount = _rowEndingKindCounts
            .FindIndex(x =>
                x.rowEndingKind == rowEndingKind);

        var currentRowEndingKindCount = _rowEndingKindCounts[indexOfRowEndingKindCount]
            .count;

        _rowEndingKindCounts[indexOfRowEndingKindCount] =
            (rowEndingKind, currentRowEndingKindCount + changeBy);

        CheckRowEndingPositions(false);
    }

    private void CheckRowEndingPositions(bool setUsingRowEndingKind)
    {
        var existingRowEndings = _rowEndingKindCounts
            .Where(x => x.count > 0)
            .ToArray();

        if (!existingRowEndings.Any())
        {
            OnlyRowEndingKind = RowEndingKind.Unset;
            UsingRowEndingKind = RowEndingKind.Linefeed;
        }
        else
        {
            if (existingRowEndings.Length == 1)
            {
                var rowEndingKind = existingRowEndings
                    .Single()
                    .rowEndingKind;

                if (setUsingRowEndingKind)
                    UsingRowEndingKind = rowEndingKind;

                OnlyRowEndingKind = rowEndingKind;
            }
            else
            {
                if (setUsingRowEndingKind)
                {
                    UsingRowEndingKind = existingRowEndings
                        .MaxBy(x => x.count)
                        .rowEndingKind;
                }

                OnlyRowEndingKind = null;
            }
        }
    }

    public void SetContent(string content)
    {
        ResetStateButNotEditHistory();
        
        var rowIndex = 0;
        var previousCharacter = '\0';

        var charactersOnRow = 0;

        var carriageReturnCount = 0;
        var linefeedCount = 0;
        var carriageReturnLinefeedCount = 0;

        for (var index = 0; index < content.Length; index++)
        {
            var character = content[index];

            charactersOnRow++;

            if (character == KeyboardKeyFacts.WhitespaceCharacters.CARRIAGE_RETURN)
            {
                if (charactersOnRow > MostCharactersOnASingleRowTuple.rowLength - MOST_CHARACTERS_ON_A_SINGLE_ROW_MARGIN) 
                    MostCharactersOnASingleRowTuple = (rowIndex, charactersOnRow + MOST_CHARACTERS_ON_A_SINGLE_ROW_MARGIN);
                
                _rowEndingPositions.Add((index + 1, RowEndingKind.CarriageReturn));
                rowIndex++;

                charactersOnRow = 0;

                carriageReturnCount++;
            }
            else if (character == KeyboardKeyFacts.WhitespaceCharacters.NEW_LINE)
            {
                if (charactersOnRow > MostCharactersOnASingleRowTuple.rowLength - MOST_CHARACTERS_ON_A_SINGLE_ROW_MARGIN) 
                    MostCharactersOnASingleRowTuple = (rowIndex, charactersOnRow + MOST_CHARACTERS_ON_A_SINGLE_ROW_MARGIN);
                
                if (previousCharacter == KeyboardKeyFacts.WhitespaceCharacters.CARRIAGE_RETURN)
                {
                    var lineEnding = _rowEndingPositions[rowIndex - 1];

                    _rowEndingPositions[rowIndex - 1] =
                        (lineEnding.positionIndex + 1, RowEndingKind.CarriageReturnLinefeed);

                    carriageReturnCount--;
                    carriageReturnLinefeedCount++;
                }
                else
                {
                    _rowEndingPositions.Add((index + 1, RowEndingKind.Linefeed));
                    rowIndex++;

                    linefeedCount++;
                }
                
                charactersOnRow = 0;
            }

            if (character == KeyboardKeyFacts.WhitespaceCharacters.TAB)
                _tabKeyPositions.Add(index);

            previousCharacter = character;

            _content.Add(new RichCharacter
            {
                Value = character,
                DecorationByte = default,
            });
        }

        _rowEndingKindCounts.AddRange(new List<(RowEndingKind rowEndingKind, int count)>
        {
            (RowEndingKind.CarriageReturn, carriageReturnCount),
            (RowEndingKind.Linefeed, linefeedCount),
            (RowEndingKind.CarriageReturnLinefeed, carriageReturnLinefeedCount),
        });

        CheckRowEndingPositions(true);

        _rowEndingPositions.Add((content.Length, RowEndingKind.EndOfFile));
    }

    private void ResetStateButNotEditHistory()
    {
        _content.Clear();
        _rowEndingKindCounts.Clear();
        _rowEndingPositions.Clear();
        _tabKeyPositions.Clear();
        OnlyRowEndingKind = null;
        UsingRowEndingKind = RowEndingKind.Unset;
    }
}