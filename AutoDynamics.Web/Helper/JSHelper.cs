using Microsoft.JSInterop;

namespace AutoDynamics.Web.Helper
{
    public class JSHelper
    {
        private readonly IJSRuntime _js;

        public JSHelper(IJSRuntime js)
        {
            _js = js;
        }

        public async Task OpenPdfAsync(string base64)
        {
            await _js.InvokeVoidAsync("openPdfInBrowser", base64);
        }
    }
}
