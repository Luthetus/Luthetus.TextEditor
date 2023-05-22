using Fluxor;
using Luthetus.TextEditor.RazorLib.Store.Find;
using Luthetus.TextEditor.RazorLib;
using Luthetus.TextEditor.RazorLib.Find;

namespace Luthetus.TextEditor.RazorLib;

public partial interface ITextEditorService
{
    public interface IFindProviderApi
    {
        public void Register(ITextEditorFindProvider findProvider);
        public void DisposeAction(TextEditorFindProviderKey findProviderKey);
        public void SetActiveFindProvider(TextEditorFindProviderKey findProviderKey);
        public ITextEditorFindProvider? FindOrDefault(TextEditorFindProviderKey findProviderKey);
    }

    public class FindProviderApi : IFindProviderApi
    {
        private readonly ITextEditorService _textEditorService;
        private readonly IDispatcher _dispatcher;
        private readonly LuthetusTextEditorOptions _luthetusTextEditorOptions;

        public FindProviderApi(
            IDispatcher dispatcher,
            LuthetusTextEditorOptions luthetusTextEditorOptions,
            ITextEditorService textEditorService)
        {
            _dispatcher = dispatcher;
            _luthetusTextEditorOptions = luthetusTextEditorOptions;
            _textEditorService = textEditorService;
        }

        public void DisposeAction(
            TextEditorFindProviderKey findProviderKey)
        {
            _dispatcher.Dispatch(
                new TextEditorFindProviderState.DisposeAction(
                    findProviderKey));
        }

        public ITextEditorFindProvider? FindOrDefault(
            TextEditorFindProviderKey findProviderKey)
        {
            return _textEditorService.FindProviderState.Value.FindProvidersList
                .FirstOrDefault(x => x.FindProviderKey == findProviderKey);
        }

        public void Register(
            ITextEditorFindProvider findProvider)
        {
            _dispatcher.Dispatch(
                new TextEditorFindProviderState.RegisterAction(
                    findProvider));
        }

        public void SetActiveFindProvider(
            TextEditorFindProviderKey findProviderKey)
        {
            _dispatcher.Dispatch(
                new TextEditorFindProviderState.SetActiveFindProviderAction(
                    findProviderKey));
        }
    }
}
