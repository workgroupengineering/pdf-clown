using PdfClown.Documents;
using PdfClown.Documents.Contents;
using PdfClown.Documents.Interaction.Actions;
using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Documents.Interaction.Annotations.ControlPoints;
using PdfClown.Documents.Interaction.Navigation;
using PdfClown.Tools;
using PdfClown.Util.Math.Geom;
using PdfClown.UI.Operations;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PdfClown.UI
{
    public class PdfPageViewModel : IPdfPageViewModel, IDisposable
    {
        private SKPicture picture;
        private SKMatrix matrix = SKMatrix.Identity;
        private PdfPage page;
        private SKImage image;
        private float imageScale;
        private PageAnnotations pageAnnotations;
        private SKRect? bounds;
        private SKSize size;

        public event EventHandler BoundsChanged;

        public PdfPageViewModel()
        {
        }

        public PdfDocumentViewModel Document { get; set; }

        IPdfDocumentViewModel IPdfPageViewModel.Document
        {
            get => Document;
            set => Document = (PdfDocumentViewModel)value;
        }

        public PdfPage Page
        {
            get => page;
            set => page = value;
        }

        public SKSize Size
        {
            get => size;
            set
            {
                if (size != value)
                {
                    size = value;
                    OnBoundsChanged();
                }
            }
        }

        public SKRect Bounds
        {
            get => bounds ??= CalculateBounds();
            set => bounds = value;
        }

        public int Index { get; internal set; }

        public int Order { get; internal set; }

        public float XOffset
        {
            get => matrix.TransX;
            set
            {
                if (XOffset != value)
                {
                    matrix.TransX = value;
                    OnBoundsChanged();
                }
            }
        }

        public float YOffset
        {
            get => matrix.TransY;
            set
            {
                if (YOffset != value)
                {
                    matrix.TransY = value;
                    OnBoundsChanged();
                }
            }
        }

        public SKMatrix Matrix
        {
            get => matrix;
            set
            {
                if (matrix != value)
                {
                    matrix = value;
                    OnBoundsChanged();
                }
            }
        }

        private SKRect CalculateBounds()
        {
            var matrix = Document.Matrix.PostConcat(Matrix);
            return SKRect.Create(matrix.TransX, matrix.TransY, Size.Width * matrix.ScaleX, Size.Height * matrix.ScaleY);
        }

        public Annotation GetAnnotation(string name)
        {
            if (pageAnnotations == null)
            {
                GetAnnotations();
            }
            return pageAnnotations[name];
        }

        public IEnumerable<Annotation> GetAnnotations() => pageAnnotations ??= Page.Annotations;

        public bool Draw(PdfViewState state)
        {
            state.PageView = this;

            state.Canvas.Save();

            state.Canvas.SetMatrix(state.PageViewMatrix);
            var pageRect = SKRect.Create(Size);
            //state.Canvas.ClipRect(pageRect);
            state.Canvas.DrawRect(pageRect, Document.PageBackgroundPaint);

            SKPicture picture = GetPicture(state.Viewer);
            if (picture != null)
            {
                state.Canvas.DrawPicture(picture, Document.PageForegroundPaint);
                if (state.Viewer.ShowMarkup && GetAnnotations().Any())
                {
                    OnPaintAnnotations(state);
                }

                if (state.Viewer.ShowCharBound)
                {
                    DrawCharBounds(state);
                }
            }

            state.Canvas.Restore();
            return picture != null;
        }

        private void DrawCharBounds(PdfViewState state)
        {
            try
            {
                foreach (var textString in state.PageView.GetStrings())
                {
                    foreach (var textChar in textString.TextChars)
                    {
                        state.Canvas.DrawPoints(SKPointMode.Polygon, textChar.Quad.GetPoints(), DefaultSKStyles.PaintRed);
                    }
                }
            }
            catch { }
        }

        private void OnPaintAnnotations(PdfViewState state)
        {
            var self = state.Page.RotateMatrix;
            state.Canvas.Save();
            state.Canvas.Concat(ref self);
            foreach (var annotation in GetAnnotations())
            {
                if (annotation != null && annotation.Visible)
                {
                    state.DrawAnnotation = annotation;
                    OnPainAnnotation(state);
                }
            }
            state.Canvas.Restore();
        }

        private void OnPainAnnotation(PdfViewState state)
        {
            try
            {
                if (state.DrawAnnotation.IsNew)
                {
                    return;
                }
                state.DrawAnnotationBound = state.DrawAnnotation.Draw(state.Canvas);
                if (state.Viewer.SelectedAnnotation is Annotation selectedAnnotation
                    && selectedAnnotation == state.DrawAnnotation
                    && selectedAnnotation.Page == state.Page)
                {
                    OnPaintSelectedAnnotation(state);
                }
            }
            catch { }
        }

        private void OnPaintSelectedAnnotation(PdfViewState state)
        {
            var selectedPoint = state.Viewer.SelectedPoint;
            if (state.Viewer.Operations.Current == OperationType.None
            || state.DrawAnnotation != selectedPoint?.Annotation)
            {
                var bound = state.DrawAnnotationBound;
                bound.Inflate(1, 1);
                state.Canvas.DrawRect(bound, DefaultSKStyles.PaintBorderSelection);
            }
            if (!state.Viewer.IsReadOnly)
            {
                foreach (var controlPoint in state.DrawAnnotation.GetControlPoints())
                {
                    var bound = controlPoint.GetBounds(state.PageViewMatrix);
                    state.Canvas.DrawOval(bound, DefaultSKStyles.PaintPointFill);
                    state.Canvas.DrawOval(bound, controlPoint == selectedPoint
                        ? DefaultSKStyles.PaintBorderSelection
                        : DefaultSKStyles.PaintBorderDefault);
                }
            }
        }

        public SKPicture GetPicture(IPdfView canvasView)
        {
            if (picture == null && Document.LockObject.IsSet)
            {
                Document.LockObject.Reset();
                var task = new Task(state => Paint((IPdfView)state), canvasView);
                task.Start();
            }
            return picture;
        }

        public SKImage GetImage(IPdfView canvasView, float scaleX, float scaleY)
        {
            var picture = GetPicture(canvasView);
            if (picture == null)
            {
                return null;
            }
            if (scaleX != imageScale || image == null)
            {
                imageScale = scaleX;
                image?.Dispose();
                var imageSize = new SKSizeI((int)(Size.Width * scaleX), (int)(Size.Height * scaleY));
                var matrix = SKMatrix.Identity;
                if (imageScale < 1F)
                {
                    //matrix = SKMatrix.CreateScale(scaleX, scaleY);
                }
                image = SKImage.FromPicture(picture, imageSize, matrix);//, Matrix, 
            }
            return image;
        }

        private void Paint(IPdfView canvasView)
        {
            try
            {
                var rect = SKRect.Create(Size);
                using (var recorder = new SKPictureRecorder())
                using (var canvas = recorder.BeginRecording(rect))
                {
                    try
                    {
                        Page.Render(canvas, rect);
                    }
                    catch (Exception ex)
                    {
                        using (var paint = new SKPaint { Color = SKColors.DarkRed })
                        {
                            canvas.Save();
                            if (canvas.TotalMatrix.ScaleY < 0)
                            {
                                var matrix = SKMatrix.CreateScale(1, -1);
                                canvas.Concat(ref matrix);
                            }
                            canvas.DrawText(ex.Message, 0, 0, paint);
                            canvas.Restore();
                        }
                    }
                    picture = recorder.EndRecording();
                }
                //text
                var positionComparator = new TextStringPositionComparer<ITextString>();
                Page.Strings.Sort(positionComparator);

                canvasView.InvalidatePaint();
                //Envir.MainContext.Send((s) => ((IPdfView)s).InvalidateSurface(), canvasView);
            }
            finally
            {
                Document.LockObject.Set();
            }
        }

        public IEnumerable<ITextString> GetStrings() => Page.Strings;

        public SKMatrix GetViewMatrix(PdfViewState state)
        {
            return state.ViewMatrix.PreConcat(Document.Matrix.PreConcat(Matrix));
        }

        public void Dispose()
        {
            image?.Dispose();
            picture?.Dispose();
        }

        public void Touch(PdfViewState state)
        {
            state.PageView = this;
            state.PagePointerLocation = state.InvertPageViewMatrix.MapPoint(state.PointerLocation);
            //SKMatrix.PreConcat(ref state.PageMatrix, state.PageView.InitialMatrix);

            switch (state.Viewer.Operations.Current)
            {
                case OperationType.PointMove:
                    OnTouchPointMove(state);
                    return;
                case OperationType.PointAdd:
                    OnTouchPointAdd(state);
                    return;
                case OperationType.AnnotationSize:
                    OnTouchSized(state);
                    return;
                case OperationType.AnnotationDrag:
                    OnTouchDragged(state);
                    return;
                default:
                    break;
            }

            if (OnTouchAnnotations(state))
            {
                return;
            }
            if (OnTouchText(state))
            {
                return;
            }
            if (state.TouchAction == TouchAction.Released)
            {
                state.Viewer.SelectedAnnotation = null;
            }
            else if (state.TouchAction == TouchAction.Pressed)
            {
                state.Viewer.TextSelection.Clear();                
            }
            state.Viewer.Cursor = CursorType.Arrow;
            state.Annotation = null;
            state.ToolTipAnnotation = null;
            state.Viewer.HoverPoint = null;
        }

        private void OnTouchPointMove(PdfViewState state)
        {
            if (state.TouchAction == TouchAction.Moved)
            {
                if (state.TouchButton == MouseButton.Left)
                {
                    state.Viewer.SelectedPoint.MappedPoint = state.PagePointerLocation;
                }
            }
            else if (state.TouchAction == TouchAction.Released)
            {
                //CurrentPoint.Point = state.PagePointerLocation;

                state.Viewer.Operations.Current = OperationType.None;
            }

            state.Viewer.InvalidatePaint();
        }

        private void OnTouchPointAdd(PdfViewState state)
        {
            var selectedAnnotation = state.Viewer.SelectedAnnotation;
            var vertexShape = selectedAnnotation as VertexShape;
            if (state.TouchAction == TouchAction.Moved)
            {
                state.Viewer.SelectedPoint.MappedPoint = state.PagePointerLocation;
            }
            else if (state.TouchAction == TouchAction.Released)
            {
                var rect = new SKRect(vertexShape.FirstPoint.X - 5, vertexShape.FirstPoint.Y - 5, vertexShape.FirstPoint.X + 5, vertexShape.FirstPoint.Y + 5);
                if (vertexShape.Points.Length > 2 && rect.Contains(vertexShape.LastPoint))
                {
                    //EndOperation(operationLink.Value);
                    //BeginOperation(annotation, OperationType.PointRemove, CurrentPoint);
                    //annotation.RemovePoint(annotation.Points.Length - 1);
                    state.Viewer.Operations.CloseVertextShape(vertexShape);
                }
                else
                {
                    state.Viewer.Operations.EndOperation();
                    var point = state.Viewer.SelectedPoint = vertexShape.AddPoint(state.Page.InvertRotateMatrix.MapPoint(state.PagePointerLocation));
                    state.Viewer.Operations.BeginOperation(vertexShape, OperationType.PointAdd, point);
                    return;
                }

                state.Viewer.Operations.Current = OperationType.None;
            }

            state.Viewer.InvalidatePaint();
        }

        private void OnTouchSized(PdfViewState state)
        {
            if (state.PressedLocation == null)
                return;
            var selectedAnnotation = state.Viewer.SelectedAnnotation;
            if (state.TouchAction == TouchAction.Moved)
            {
                var bound = selectedAnnotation.GetViewBounds(state.PageViewMatrix);
                bound.Size += new SKSize(state.PointerLocation - state.PressedLocation.Value);

                selectedAnnotation.SetBounds(state.InvertPageViewMatrix.MapRect(bound));

                state.PressedLocation = state.PointerLocation;
            }
            else if (state.TouchAction == TouchAction.Released)
            {
                state.Viewer.Operations.Current = OperationType.None;
            }
            state.Viewer.InvalidatePaint();
        }

        private void OnTouchDragged(PdfViewState state)
        {
            var selectedAnnotation = state.Viewer.SelectedAnnotation;
            if (state.TouchAction == TouchAction.Moved)
            {
                if (state.Page != selectedAnnotation.Page)
                {
                    if (!selectedAnnotation.IsNew)
                    {
                        state.Viewer.Operations.EndOperation();
                        state.Viewer.Operations.BeginOperation(selectedAnnotation, OperationType.AnnotationRePage, nameof(Annotation.Page), selectedAnnotation.Page.Index, state.PageView.Index);
                    }
                    selectedAnnotation.Page = state.Page;
                    if (!selectedAnnotation.IsNew)
                    {
                        state.Viewer.Operations.BeginOperation(selectedAnnotation, OperationType.AnnotationDrag, nameof(Annotation.Box));
                        state.PressedLocation = null;
                    }
                }
                var bound = selectedAnnotation.GetViewBounds(state.PageViewMatrix);
                if (bound.Width == 0)
                    bound.Right = bound.Left + 1;
                if (bound.Height == 0)
                    bound.Bottom = bound.Top + 1;
                if (state.PressedLocation == null || state.TouchButton == MouseButton.Unknown)
                {
                    bound.Location = state.PointerLocation;
                }
                else
                {
                    bound.Location += state.PointerLocation - state.PressedLocation.Value;
                    state.PressedLocation = state.PointerLocation;
                }
                selectedAnnotation.SetBounds(state.InvertPageViewMatrix.MapRect(bound));

                state.Viewer.InvalidatePaint();
            }
            else if (state.TouchAction == TouchAction.Pressed)
            {
                state.PressedLocation = state.PointerLocation;
                if (selectedAnnotation.IsNew)
                {
                    state.Viewer.Operations.AddAnnotation(selectedAnnotation);
                    if (selectedAnnotation is StickyNote sticky)
                    {
                        state.Viewer.Operations.Current = OperationType.None;
                    }
                    else if (selectedAnnotation is Line line)
                    {
                        line.StartPoint = state.InvertPageMatrix.MapPoint(state.PointerLocation);
                        state.Viewer.SelectedPoint = line.GetControlPoints().OfType<LineEndControlPoint>().FirstOrDefault();
                        state.Viewer.Operations.Current = OperationType.PointMove;
                    }
                    else if (selectedAnnotation is VertexShape vertexShape)
                    {
                        vertexShape.FirstPoint = state.InvertPageMatrix.MapPoint(state.PointerLocation);
                        state.Viewer.SelectedPoint = vertexShape.FirstControlPoint;
                        state.Viewer.Operations.Current = OperationType.PointAdd;
                    }
                    else if (selectedAnnotation is Shape shape)
                    {
                        state.Viewer.Operations.Current = OperationType.AnnotationSize;
                    }
                    else if (selectedAnnotation is FreeText freeText)
                    {
                        if (freeText.Line == null)
                        {
                            state.Viewer.Operations.Current = OperationType.AnnotationSize;
                        }
                        else
                        {
                            freeText.Line.Start = state.InvertPageMatrix.MapPoint(state.PointerLocation);
                            state.Viewer.SelectedPoint = selectedAnnotation.GetControlPoints().OfType<TextMidControlPoint>().FirstOrDefault();
                            state.Viewer.Operations.Current = OperationType.PointMove;
                        }
                    }
                    else
                    {
                        var controlPoint = selectedAnnotation.GetControlPoints().OfType<BottomRightControlPoint>().FirstOrDefault();
                        if (controlPoint != null)
                        {
                            state.Viewer.SelectedPoint = controlPoint;
                            state.Viewer.Operations.Current = OperationType.PointMove;
                        }
                        else
                        {
                            state.Viewer.Operations.Current = OperationType.AnnotationSize;
                        }
                    }
                }
            }
            else if (state.TouchAction == TouchAction.Released)
            {
                state.PressedLocation = null;
                state.Viewer.Operations.Current = OperationType.None;
            }
        }

        private bool OnTouchAnnotations(PdfViewState state)
        {
            state.Annotation = null;
            var selectedAnnotation = state.Viewer.SelectedAnnotation;
            if (selectedAnnotation != null && selectedAnnotation.Page == state.Page)
            {
                var bounds = selectedAnnotation.GetViewBounds(state.PageViewMatrix);
                bounds.Inflate(2, 2);
                if (bounds.Contains(state.PointerLocation))
                {
                    state.Annotation = selectedAnnotation;
                    state.AnnotationBound = bounds;
                    OnTouchAnnotation(state);
                    return true;
                }
            }
            foreach (var annotation in state.PageView.GetAnnotations())
            {
                if (annotation == null || !annotation.Visible)
                    continue;
                var bounds = annotation.GetViewBounds(state.PageViewMatrix);
                bounds.Inflate(2, 2);
                if (bounds.Contains(state.PointerLocation))
                {
                    if (state.Annotation != null
                    && bounds.Contains(state.AnnotationBound))
                    {
                        continue;
                    }
                    state.Annotation = annotation;
                    state.AnnotationBound = bounds;
                }
            }

            if (state.Annotation != null)
            {
                OnTouchAnnotation(state);
                return true;
            }
            return false;
        }

        private void OnTouchAnnotation(PdfViewState state)
        {
            var selectedAnnotation = state.Viewer.SelectedAnnotation;
            if (state.TouchAction == TouchAction.Moved)
            {
                state.ToolTipAnnotation = state.Annotation;
                if (state.TouchButton == MouseButton.Left)
                {
                    if (!state.Viewer.IsReadOnly
                        && selectedAnnotation != null
                        && state.PressedLocation != null
                        && state.Viewer.Cursor == CursorType.Hand
                        && !(state.Annotation is Widget))
                    {
                        if (state.Viewer.Operations.Current == OperationType.None)
                        {
                            var dif = SKPoint.Distance(state.PointerLocation, state.PressedLocation.Value);
                            if (Math.Abs(dif) > 5)
                            {
                                state.Viewer.Operations.Current = OperationType.AnnotationDrag;
                            }
                        }
                    }
                }
                else if (state.TouchButton == MouseButton.Unknown)
                {
                    if (state.Annotation == selectedAnnotation)
                    {
                        foreach (var controlPoint in state.Annotation.GetControlPoints())
                        {
                            if (controlPoint.GetViewBounds(state.PageViewMatrix).Contains(state.PointerLocation))
                            {
                                state.Viewer.HoverPoint = controlPoint;
                                return;
                            }
                        }
                    }
                    state.Viewer.HoverPoint = null;
                    if (!state.Viewer.IsReadOnly)
                    {
                        var rect = new SKRect(
                            state.AnnotationBound.Right - 10,
                            state.AnnotationBound.Bottom - 10,
                            state.AnnotationBound.Right,
                            state.AnnotationBound.Bottom);
                        if (rect.Contains(state.PointerLocation)
                           && state.Annotation is Markup markup
                           && markup.AllowSize)
                        {
                            state.Viewer.Cursor = CursorType.SizeNWSE;
                        }
                        else
                        {
                            state.Viewer.Cursor = CursorType.Hand;
                        }
                    }
                }
            }
            else if (state.TouchAction == TouchAction.Pressed)
            {
                if (state.TouchButton == MouseButton.Left)
                {
                    state.PressedLocation = state.PointerLocation;
                    state.Viewer.SelectedAnnotation = state.Annotation;
                    if (state.Annotation == selectedAnnotation && state.Viewer.HoverPoint != null && !state.Viewer.IsReadOnly)
                    {
                        state.Viewer.SelectedPoint = state.Viewer.HoverPoint;
                        state.Viewer.Operations.Current = OperationType.PointMove;
                    }
                    else if (state.Viewer.Cursor == CursorType.SizeNWSE && !state.Viewer.IsReadOnly)
                    {
                        state.Viewer.Operations.Current = OperationType.AnnotationSize;
                    }
                }
            }
            else if (state.TouchAction == TouchAction.Released)
            {
                if (state.TouchButton == MouseButton.Left)
                {
                    state.PressedLocation = null;
                    if (state.Viewer.SelectedAnnotation is Link link)
                    {
                        if (link.Target is GoToLocal goToLocal)
                        {
                            if (goToLocal.Destination is LocalDestination localDestination
                                && localDestination.Page is PdfPage goToPage)
                            {
                                state.Viewer.ScrollTo(goToPage);
                            }
                        }
                        else if (link.Target is GoToURI toToUri)
                        {
                            var uri = toToUri.URI;
                            Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
                        }
                        else if (link.Target is LocalDestination localDestination
                            && localDestination.Page is PdfPage goToPage)
                        {
                            state.Viewer.ScrollTo(goToPage);
                        }

                    }
                }
            }
        }

        private bool OnTouchText(PdfViewState state)
        {
            var textSelection = state.Viewer.TextSelection;
            var textSelectionChanged = false;
            var page = state.Page;
            foreach (var textString in page.Strings)
            {
                if (textString.Quad.IsEmpty)
                    continue;
                if (textString.Quad.Contains(state.PagePointerLocation))
                {
                    foreach (var textChar in textString.TextChars)
                    {
                        if (textChar.Quad.Contains(state.PagePointerLocation))
                        {
                            state.Viewer.Cursor = CursorType.IBeam;
                            if (state.TouchAction == TouchAction.Pressed
                                && state.TouchButton == MouseButton.Left)
                            {
                                textSelection.StartChar = textChar;
                            }
                            textSelectionChanged = true;
                            break;
                        }
                    }
                }
            }
            var startChar = textSelection.StartChar;
            if (state.TouchAction == TouchAction.Moved
                && state.TouchButton == MouseButton.Left
                && startChar != null)
            {
                var firstString = startChar.TextString;
                var firstCharIndex = firstString.TextChars.IndexOf(startChar);
                var firstStringIndex = page.Strings.IndexOf(firstString);
                var firstMiddle = startChar.Quad.Middle.Value;
                var selectionRect = new Quad(new SKRect(firstMiddle.X, firstMiddle.Y, state.PagePointerLocation.X, state.PagePointerLocation.Y));
                textSelection.Clear();

                for (int i = firstStringIndex < 1 ? 0 : firstStringIndex - 1; i < page.Strings.Count; i++)
                {
                    var textString = page.Strings[i];
                    if (textString.Quad.IsEmpty
                        || !selectionRect.ContainsOrIntersect(textString.Quad))
                        continue;
                    var isFirstLine = textString.Quad.Top <= firstString.Quad.Top
                                    && textString.Quad.Bottom >= firstString.Quad.Bottom;
                    var intersect = -1;
                    var endIntersect = -1;
                    for (int j = 0; j < textString.TextChars.Count; j++)
                    {
                        var textChar = textString.TextChars[j];
                        if (textChar == startChar
                            || selectionRect.IntersectsWith(textChar.Quad)
                            || selectionRect.Contains(textChar.Quad))
                        {
                            if (intersect == -1)
                            {
                                intersect = j;
                                if (!isFirstLine)
                                    break;
                            }
                        }
                        else if (intersect >= 0)
                        {
                            endIntersect = j;
                            break;
                        }
                    }
                    if (intersect >= 0)
                    {
                        if (isFirstLine)
                        {
                            var chars = textString.TextChars
                                .Skip(intersect);
                            if (selectionRect.Bottom <= textString.Quad.Bottom
                                && selectionRect.Top >= textString.Quad.Top)
                                chars = chars.Take(endIntersect > -1
                                    ? endIntersect - intersect
                                    : textString.TextChars.Count - intersect);

                            textSelection.AddRange(chars);
                        }
                        else if (selectionRect.Bottom > textString.Quad.Bottom
                            && selectionRect.Top < textString.Quad.Top)
                        {
                            textSelection.AddRange(textString.TextChars);
                        }
                        else
                        {
                            textSelection.AddRange(textString.TextChars
                                .Take(intersect));
                        }
                    }
                }
                textSelectionChanged = true;
            }
            if (state.TouchAction == TouchAction.Released)
            {
               textSelection.StartChar = null;
            }

            return textSelectionChanged;
        }

        public PdfPage GetPage(PdfViewState state) => page;

        private void OnBoundsChanged()
        {
            bounds = null;
            BoundsChanged?.Invoke(this, EventArgs.Empty);
        }


    }

}

