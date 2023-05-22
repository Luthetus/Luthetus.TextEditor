﻿namespace Luthetus.TextEditor.RazorLib.Autocomplete;

public class AutocompleteService : IAutocompleteService
{
    private readonly IAutocompleteIndexer _autocompleteIndexer;

    public AutocompleteService(IAutocompleteIndexer autocompleteIndexer)
    {
        _autocompleteIndexer = autocompleteIndexer;
    }

    public List<string> GetAutocompleteOptions(string word)
    {
        var indexedStrings = _autocompleteIndexer.IndexedStrings;

        return new List<string>(
            indexedStrings
                .Where(x => x.StartsWith(word))
                .Take(5));
    }
}