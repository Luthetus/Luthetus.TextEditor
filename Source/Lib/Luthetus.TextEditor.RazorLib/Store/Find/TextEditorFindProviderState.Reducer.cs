﻿using Fluxor;

namespace Luthetus.TextEditor.RazorLib.Store.Find;

public partial class TextEditorFindProviderState
{
    private class Reducer
    {
        [ReducerMethod]
        public static TextEditorFindProviderState ReduceRegisterAction(
            TextEditorFindProviderState inFindProviderState,
            RegisterAction registerAction)
        {
            var existingFindProvider = inFindProviderState.FindProvidersList.FirstOrDefault(x =>
                x.FindProviderKey == registerAction.FindProvider.FindProviderKey);

            if (existingFindProvider is not null)
                return inFindProviderState;

            var outFindProvidersList = inFindProviderState.FindProvidersList
                .Add(registerAction.FindProvider);

            return new TextEditorFindProviderState(
                outFindProvidersList,
                inFindProviderState.ActiveFindProviderKey,
                inFindProviderState.SearchQuery);
        }

        [ReducerMethod]
        public static TextEditorFindProviderState ReduceDisposeAction(
            TextEditorFindProviderState inFindProviderState,
            DisposeAction disposeAction)
        {
            var existingFindProvider = inFindProviderState.FindProvidersList.FirstOrDefault(x =>
                x.FindProviderKey == disposeAction.FindProviderKey);

            if (existingFindProvider is null)
                return inFindProviderState;

            var outFindProvidersList = inFindProviderState.FindProvidersList
                .Remove(existingFindProvider);

            return new TextEditorFindProviderState(
                outFindProvidersList,
                inFindProviderState.ActiveFindProviderKey,
                inFindProviderState.SearchQuery);
        }

        [ReducerMethod]
        public static TextEditorFindProviderState ReduceSetActiveFindProviderAction(
            TextEditorFindProviderState inFindProviderState,
            SetActiveFindProviderAction setActiveFindProviderAction)
        {
            return new TextEditorFindProviderState(
                inFindProviderState.FindProvidersList,
                setActiveFindProviderAction.FindProviderKey,
                inFindProviderState.SearchQuery);
        }
        
        [ReducerMethod]
        public static TextEditorFindProviderState ReduceSetSearchQueryAction(
            TextEditorFindProviderState inFindProviderState,
            SetSearchQueryAction setSearchQueryAction)
        {
            return new TextEditorFindProviderState(
                inFindProviderState.FindProvidersList,
                inFindProviderState.ActiveFindProviderKey,
                setSearchQueryAction.SearchQuery);
        }
    }
}
