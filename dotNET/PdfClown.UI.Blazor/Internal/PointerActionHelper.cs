using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.ComponentModel;
using System.Text.Json.Nodes;

namespace PdfClown.UI.Blazor.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PointerActionHelper
    {
        private readonly Action<PointerEventArgs> action;

        public PointerActionHelper(Action<PointerEventArgs> action)
        {
            this.action = action;
        }

        [JSInvokable]
        public void Invoke(PointerEventArgs args) => action?.Invoke(args);
        
    }
}