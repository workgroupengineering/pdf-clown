using Microsoft.JSInterop;
using System.ComponentModel;

namespace PdfClown.UI.Blazor.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SizeActionHelper
    {
        private readonly Action<float, float> action;

        public SizeActionHelper(Action<float, float> action)
        {
            this.action = action;
        }

        [JSInvokable]
        public void Invoke(float width, float height) => action?.Invoke(width, height);

    }
}