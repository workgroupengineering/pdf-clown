using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SkiaSharp.Views.Blazor.Internal;

namespace PdfClown.UI.Blazor.Internal
{
    internal class SKHtmlScrollInterop : JSModuleInterop
    {
        private const string JsFilename = "./_content/PdfClown.UI.Blazor/SKHtmlScroll.js";
        private const string InitSymbol = "SKHtmlScroll.initById";
        private const string RequestLockSymbol = "SKHtmlScroll.requestLockById";
        private const string SetCaptureSymbol = "SKHtmlScroll.setCaptureById";
        private const string ReleaseCaptureSymbol = "SKHtmlScroll.releaseCaptureById";
        private const string DeinitSymbol = "SKHtmlScroll.deinit";

        private readonly string htmlElementId;
        private readonly PointerActionHelper moveCallback;
        private DotNetObjectReference<PointerActionHelper> moveCallbackReference;

        public static async Task<SKHtmlScrollInterop> ImportAsync(IJSRuntime js, string elementId, Action<PointerEventArgs> moveAction)
        {
            var interop = new SKHtmlScrollInterop(js, elementId, moveAction);
            await interop.ImportAsync();
            return interop;
        }

        public SKHtmlScrollInterop(IJSRuntime js, string elementId, Action<PointerEventArgs> moveAction)
            : base(js, JsFilename)
        {
            htmlElementId = elementId;
            moveCallback = new PointerActionHelper(moveAction);
        }

        protected override void OnDisposingModule()
        { }

        public void Init()
        {
            moveCallbackReference = DotNetObjectReference.Create(moveCallback);
            Invoke(InitSymbol, htmlElementId, moveCallbackReference);
        }
        public void RequestLock() =>
            Invoke(RequestLockSymbol, htmlElementId);

        public void SetCapture(long pointerId) =>
            Invoke(SetCaptureSymbol, htmlElementId, pointerId);

        public void ReleaseCapture(long pointerId) =>
            Invoke(ReleaseCaptureSymbol, htmlElementId, pointerId);

        public void DeInit()
        {
            Invoke(DeinitSymbol, htmlElementId);
            moveCallbackReference.Dispose();
        }
    }
}
