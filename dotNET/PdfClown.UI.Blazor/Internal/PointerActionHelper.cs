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

        //new PointerEventArgs
        //{
        //    PointerId = (long)args["pointerId"],
        //    Button = (long)args["button"],
        //    ClientX = (long)args["clientX"],
        //    ClientY = (long)args["clientT"],
        //    AltKey = (bool)args["altKey"],
        //    CtrlKey = (bool)args["ctrlKey"],
        //    ShiftKey = (bool)args["shiftKey"],
        //    MetaKey = (bool)args["metaKey"],
        //});
    }
}