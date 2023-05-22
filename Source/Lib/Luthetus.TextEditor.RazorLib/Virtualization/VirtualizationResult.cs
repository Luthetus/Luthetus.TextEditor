﻿using System.Collections.Immutable;
using Luthetus.Common.RazorLib.JavaScriptObjects;
using Luthetus.TextEditor.RazorLib.Character;
using Luthetus.TextEditor.RazorLib.Measurement;

namespace Luthetus.TextEditor.RazorLib.Virtualization;

public record VirtualizationResult<T> : IVirtualizationResultWithoutTypeMask
{
    public VirtualizationResult(
        ImmutableArray<VirtualizationEntry<T>> entries,
        VirtualizationBoundary leftVirtualizationBoundary,
        VirtualizationBoundary rightVirtualizationBoundary,
        VirtualizationBoundary topVirtualizationBoundary,
        VirtualizationBoundary bottomVirtualizationBoundary,
        ElementMeasurementsInPixels elementMeasurementsInPixels,
        CharacterWidthAndRowHeight characterWidthAndRowHeight)
    {
        Entries = entries;
        LeftVirtualizationBoundary = leftVirtualizationBoundary;
        RightVirtualizationBoundary = rightVirtualizationBoundary;
        TopVirtualizationBoundary = topVirtualizationBoundary;
        BottomVirtualizationBoundary = bottomVirtualizationBoundary;
        ElementMeasurementsInPixels = elementMeasurementsInPixels;
        CharacterWidthAndRowHeight = characterWidthAndRowHeight;
    }

    public static VirtualizationResult<List<RichCharacter>> GetEmptyRichCharacters() => new(
        ImmutableArray<VirtualizationEntry<List<RichCharacter>>>.Empty,
        new VirtualizationBoundary(0, 0, 0, 0),
        new VirtualizationBoundary(0, 0, 0, 0),
        new VirtualizationBoundary(0, 0, 0, 0),
        new VirtualizationBoundary(0, 0, 0, 0),
        new ElementMeasurementsInPixels(0, 0, 0, 0, 0, 0, 0, CancellationToken.None),
        new CharacterWidthAndRowHeight(0, 0));

    public ImmutableArray<VirtualizationEntry<T>> Entries { get; init; }
    public VirtualizationBoundary LeftVirtualizationBoundary { get; init; }
    public VirtualizationBoundary RightVirtualizationBoundary { get; init; }
    public VirtualizationBoundary TopVirtualizationBoundary { get; init; }
    public VirtualizationBoundary BottomVirtualizationBoundary { get; init; }
    public ElementMeasurementsInPixels ElementMeasurementsInPixels { get; init; }
    public CharacterWidthAndRowHeight CharacterWidthAndRowHeight { get; set; }
}