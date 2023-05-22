using System.Collections.Immutable;
using Luthetus.TextEditor.RazorLib.Model;

namespace Luthetus.TextEditor.RazorLib.Autocomplete;

public interface IAutocompleteIndexer : IDisposable
{
    public ImmutableArray<string> IndexedStrings { get; }

    public Task IndexTextEditorAsync(TextEditorModel textEditorModel);
    public Task IndexWordAsync(string word);
}