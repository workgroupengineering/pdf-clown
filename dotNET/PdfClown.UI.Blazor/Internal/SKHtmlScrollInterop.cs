﻿using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace PdfClown.UI.Blazor.Internal
{
    internal class SKHtmlScrollInterop : JSModuleInterop
    {
        private const string JsFilename = "./_content/PdfClown.UI.Blazor/SKHtmlScroll.js";
        private const string InitSymbol = "SKHtmlScroll.initById";
        private const string RequestLockSymbol = "SKHtmlScroll.requestLockById";
        private const string SetCaptureSymbol = "SKHtmlScroll.setCaptureById";
        private const string ReleaseCaptureSymbol = "SKHtmlScroll.releaseCaptureById";
        private const string ChangeCursorSymbol = "SKHtmlScroll.changeCursorById";
        private const string DeinitSymbol = "SKHtmlScroll.deinit";

        private readonly string htmlElementId;
        private readonly PointerActionHelper moveCallback;
        private readonly SizeActionHelper sizeCallback;
        private DotNetObjectReference<PointerActionHelper> moveCallbackReference;
        private DotNetObjectReference<SizeActionHelper> sizeCallbackReference;

        public static async Task<SKHtmlScrollInterop> ImportAsync(IJSRuntime js, string elementId, 
            Action<PointerEventArgs> moveAction,
            Action<float, float> sizeAction)
        {
            var interop = new SKHtmlScrollInterop(js, elementId, moveAction, sizeAction);
            await interop.ImportAsync();
            return interop;
        }

        public SKHtmlScrollInterop(IJSRuntime js, string elementId, Action<PointerEventArgs> moveAction, Action<float, float> sizeAction)
            : base(js, JsFilename)
        {
            htmlElementId = elementId;
            moveCallback = new PointerActionHelper(moveAction);
            sizeCallback = new SizeActionHelper(sizeAction);
        }

        protected override void OnDisposingModule()
        { }

        public void Init()
        {
            moveCallbackReference = DotNetObjectReference.Create(moveCallback);
            sizeCallbackReference = DotNetObjectReference.Create(sizeCallback);
            Invoke(InitSymbol, htmlElementId, moveCallbackReference, sizeCallbackReference);
        }
        public void RequestLock() =>
            Invoke(RequestLockSymbol, htmlElementId);

        public void SetCapture(long pointerId) =>
            Invoke(SetCaptureSymbol, htmlElementId, pointerId);

        public void ReleaseCapture(long pointerId) =>
            Invoke(ReleaseCaptureSymbol, htmlElementId, pointerId);

        public void ChangeCursor(CursorType cursor)
        {
            var cursorName = GetCursorName(cursor);
            Invoke(ChangeCursorSymbol, htmlElementId, cursorName);
        }

        private static string GetCursorName(CursorType cursor)
        {
            return cursor switch
            {
                CursorType.SizeWE => "ew-resize",
                CursorType.SizeNESW => "nesw-resize",
                CursorType.SizeNS => "ns-resize",
                CursorType.SizeNWSE => "nwse-resize",
                CursorType.Hand => "pointer",
                CursorType.Wait => "wait",
                CursorType.ScrollAll => "all-scroll",
                CursorType.Cross => "crosshair",
                CursorType.IBeam => "text",
                _ => "default",
            };
        }

        public void DeInit()
        {
            Invoke(DeinitSymbol, htmlElementId);
            moveCallbackReference.Dispose();
        }
    }
}
