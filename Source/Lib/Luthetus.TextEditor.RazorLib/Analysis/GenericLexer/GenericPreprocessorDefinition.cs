using System.Collections.Immutable;

namespace Luthetus.TextEditor.RazorLib.Analysis.GenericLexer;

public class GenericPreprocessorDefinition
{
    public GenericPreprocessorDefinition(
        string transitionSubstring,
        ImmutableArray<DeliminationExtendedSyntaxDefinition> deliminationExtendedSyntaxes)
    {
        TransitionSubstring = transitionSubstring;
        DeliminationExtendedSyntaxes = deliminationExtendedSyntaxes;
    }

    public string TransitionSubstring { get; }
    public ImmutableArray<DeliminationExtendedSyntaxDefinition> DeliminationExtendedSyntaxes { get; }
}