using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Documents.Interaction.Annotations.ControlPoints;
using PdfClown.Util.Invokers;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PdfClown.UI.Operations
{
    public class EditOperationList : IEnumerable<EditOperation>
    {
        private LinkedList<EditOperation> operations = new();
        private LinkedListNode<EditOperation> lastLink;
        private OperationType currentType;
        private Markup selectedMarkup;
        private Annotation selectedAnnotation;
        private Annotation hoverAnnotation;
        private ControlPoint selectedPoint;
        private ControlPoint hoverPoint;
        private bool handlePropertyChanged = true;
        private IPdfDocumentViewModel document;

        public int Count => operations.Count;

        public IPdfView Viewer { get; set; }

        public bool HashOperations => lastLink != null;

        public bool CanRedo => (lastLink == null ? operations.First : lastLink?.Next) != null;

        public bool CanUndo => lastLink != null;

        public OperationType Current
        {
            get => currentType;
            set
            {
                if (currentType != value)
                {
                    var oldValue = currentType;
                    currentType = value;
                    OnCurrentOperationChanged(oldValue, value);
                }
            }
        }

        public LinkedList<EditOperation> Items
        {
            get => operations;
            set => operations = value;
        }

        public ControlPoint SelectedPoint
        {
            get => selectedPoint;
            set
            {
                if (selectedPoint != value)
                {
                    selectedPoint = value;
                    if (selectedPoint != null)
                    {
                        SelectedAnnotation = selectedPoint.Annotation;
                    }
                    else
                    {
                        Current = OperationType.None;
                    }
                }
            }
        }

        public ControlPoint HoverPoint
        {
            get => hoverPoint;
            set
            {
                if (hoverPoint != value)
                {
                    hoverPoint = value;
                    if (hoverPoint != null)
                    {
                        Viewer.Cursor = CursorType.Cross;
                    }
                    else
                    {
                        Viewer.Cursor = CursorType.Arrow;
                    }
                }
            }
        }

        public Annotation HoverAnnotation
        {
            get => hoverAnnotation;
            set
            {
                if (hoverAnnotation != value)
                {
                    hoverAnnotation = value;
                    Viewer.InvalidatePaint();
                }
            }
        }

        public Annotation SelectedAnnotation
        {
            get => selectedAnnotation;
            set
            {
                if (selectedAnnotation != value)
                {
                    var oldValue = selectedAnnotation;
                    selectedAnnotation = value;
                    SelectedPoint = null;                    
                    SelectedMarkup = value as Markup;
                    Current = OperationType.None;
                    if (oldValue != null)
                    {
                        SuspendAnnotationPropertyHandler(oldValue);
                    }
                    if (value != null)
                    {
                        ResumeAnnotationPropertyHandler(value);
                        if (value.IsNew)
                        {
                            if (value is TextMarkup
                                || (value is StickyNote note
                                && note.ReplyTo != null))
                            {
                                AddAnnotation(value).EndOperation();
                            }
                            else
                            {
                                Current = OperationType.AnnotationDrag;
                            }
                        }
                    }
                    SelectedAnnotationChanged?.Invoke(new PdfAnnotationEventArgs(value));
                    Viewer.InvalidatePaint();
                }
            }
        }

        public Markup SelectedMarkup
        {
            get => selectedMarkup;
            set
            {
                if (selectedMarkup != value)
                {
                    selectedMarkup = value;
                    if (value != null)
                    {
                        SelectedAnnotation = value;
                    }
                }
            }
        }

        public IPdfDocumentViewModel Document
        {
            get => document;
            set
            {
                if (document != value)
                {
                    var oldValue = document;
                    document = value;
                    if (oldValue != null)
                    {
                        oldValue.AnnotationAdded -= OnDocumentAnnotationAdded;
                        oldValue.AnnotationRemoved -= OnDocumentAnnotationRemoved;
                    }
                    SelectedAnnotation = null;
                    SelectedPoint = null;
                    HoverAnnotation = null;
                    HoverPoint = null;
                    if (document != null)
                    {
                        document.AnnotationAdded += OnDocumentAnnotationAdded;
                        document.AnnotationRemoved += OnDocumentAnnotationRemoved;
                    }
                }
            }
        }

        public event EventHandler<DetailedPropertyChangedEventArgs> AnnotationPropertyChanged;

        public event PdfAnnotationEventHandler AnnotationAdded;

        public event PdfAnnotationEventHandler AnnotationRemoved;

        public event PdfAnnotationEventHandler SelectedAnnotationChanged;

        public event EventHandler Changed;

        public event PdfAnnotationEventHandler CheckCanRemove;

        public event OperationEventHandler FinishOperation;

        private void OnDocumentAnnotationAdded(PdfAnnotationEventArgs e)
        {
            AnnotationAdded?.Invoke(e);
            Viewer.InvalidatePaint();
        }

        private void OnDocumentAnnotationRemoved(PdfAnnotationEventArgs e)
        {
            if (e.Annotation == SelectedAnnotation)
                SelectedAnnotation = null;
            AnnotationRemoved?.Invoke(e);
            Viewer.InvalidatePaint();
        }

        private void SuspendAnnotationPropertyHandler(Annotation annotation)
        {
            if (annotation != null)
            {
                annotation.PropertyChanged -= OnAnnotationPropertyChanged;
            }
        }

        private void ResumeAnnotationPropertyHandler(Annotation annotation)
        {
            if (annotation != null)
            {
                annotation.PropertyChanged += OnAnnotationPropertyChanged;
            }
        }

        private void UpdateModificationDate(Annotation annotation)
        {
            try
            {
                handlePropertyChanged = false;
                annotation.ModificationDate = DateTime.UtcNow;
            }
            finally
            {
                handlePropertyChanged = true;
            }
        }        

        public void CloseVertextShape(VertexShape vertexShape)
        {
            vertexShape.QueueRefreshAppearance();
            SelectedPoint = null;
            Current = OperationType.None;
        }

        private void OnAnnotationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!handlePropertyChanged)
                return;

            var annotation = (Annotation)sender;
            if (!string.Equals(e.PropertyName, nameof(Markup.ModificationDate), StringComparison.Ordinal))
            {
                UpdateModificationDate(annotation);
            }
            var details = (DetailedPropertyChangedEventArgs)e;
            var invoker = Invoker.GetPropertyInvoker(annotation.GetType(), e.PropertyName);
            switch (e.PropertyName)
            {
                case nameof(Markup.SKColor):
                    var colorDetails = (DetailedPropertyChangedEventArgs<SKColor>)details;
                    BeginOperation(annotation, OperationType.AnnotationProperty, invoker, colorDetails.OldValue, colorDetails.NewValue);
                    break;
                case nameof(Markup.Contents):
                case nameof(Markup.Subject):
                case nameof(Markup.RichContents):
                case nameof(Markup.DefaultStyle):
                    var stringDetails = (DetailedPropertyChangedEventArgs<string>)details;
                    BeginOperation(annotation, OperationType.AnnotationProperty, invoker, stringDetails.OldValue, stringDetails.NewValue);
                    break;
                case nameof(Markup.Border):
                    var borderDetails = (DetailedPropertyChangedEventArgs<Border>)details;
                    BeginOperation(annotation, OperationType.AnnotationProperty, invoker, borderDetails.OldValue, borderDetails.NewValue);
                    break;
                case nameof(Markup.Popup):
                    var popupDetails = (DetailedPropertyChangedEventArgs<Popup>)details;
                    BeginOperation(annotation, OperationType.AnnotationProperty, invoker, popupDetails.OldValue, popupDetails.NewValue);
                    break;
                case nameof(Markup.ReplyTo):
                    var annotationDetails = (DetailedPropertyChangedEventArgs<Annotation>)details;
                    BeginOperation(annotation, OperationType.AnnotationProperty, invoker, annotationDetails.OldValue, annotationDetails.NewValue);
                    break;
                case nameof(Markup.ReplyType):
                    var replyTypeDetails = (DetailedPropertyChangedEventArgs<ReplyTypeEnum?>)details;
                    BeginOperation(annotation, OperationType.AnnotationProperty, invoker, replyTypeDetails.OldValue, replyTypeDetails.NewValue);
                    break;
                case nameof(Markup.BorderEffect):
                    var borderEffectDetails = (DetailedPropertyChangedEventArgs<BorderEffect>)details;
                    BeginOperation(annotation, OperationType.AnnotationProperty, invoker, borderEffectDetails.OldValue, borderEffectDetails.NewValue);
                    break;
                case nameof(Line.StartStyle):
                case nameof(Line.EndStyle):
                    var lineEndDetails = (DetailedPropertyChangedEventArgs<LineEndStyleEnum>)details;
                    BeginOperation(annotation, OperationType.AnnotationProperty, invoker, lineEndDetails.OldValue, lineEndDetails.NewValue);
                    break;
            }
            AnnotationPropertyChanged?.Invoke(annotation, details);
            Viewer.InvalidatePaint();
        }

        public bool Undo()
        {
            if (lastLink != null)
            {
                var operation = lastLink.Value;
                lastLink = lastLink.Previous;
                try
                {
                    handlePropertyChanged = false;                    
                    operation.Undo();                    
                }
                finally
                {
                    handlePropertyChanged = true;
                }
                OnChanged();
                Viewer.InvalidatePaint();
                return true;
            }
            return false;
        }

        public bool Redo()
        {
            var operationLink = lastLink == null ? operations.First : lastLink?.Next;
            if (operationLink != null)
            {
                lastLink = operationLink;
                var operation = lastLink.Value;
                try
                {
                    handlePropertyChanged = false;                    
                    operation.Redo();                    
                }
                finally
                {
                    handlePropertyChanged = true;
                }
                OnChanged();
                Viewer.InvalidatePaint();
                return true;
            }
            return false;
        }

        public void EndOperation()
        {
            lastLink.Value.EndOperation();
        }

        public void CancalOperation(EditOperation operation)
        {
            lastLink = operations.Find(operation);
            Undo();
        }

        public void RejectAll()
        {
            while (Undo()) { };
        }

        public IEnumerable<EditOperation> GetOperations()
        {
            return operations.Select(x => x);
        }

        public void ClearOperations()
        {
            operations.Clear();
            lastLink = null;
            SelectedAnnotation = null;
            OnChanged();
        }

        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public AnnotationOperation AddAnnotation(Annotation newValue)
        {
            var operation = BeginOperation(newValue, OperationType.AnnotationAdd);
            newValue.IsNew = false;
            return operation;
        }

        public IEnumerable<Annotation> RemoveAnnotation(Annotation annotation)
        {
            if (!OnCheckCanRemove(annotation))
            {
                return null;
            }
            var operation = BeginOperation(annotation, OperationType.AnnotationRemove);
            if (annotation == SelectedAnnotation)
            {
                SelectedAnnotation = null;
            }

            var list = operation.EndOperation() as List<Annotation>;

            if (list.Contains(SelectedAnnotation))
                SelectedAnnotation = null;

            AnnotationRemoved?.Invoke(new PdfAnnotationEventArgs(annotation));
            Viewer.InvalidatePaint();
            return list;
        }

        private bool OnCheckCanRemove(Annotation annotation)
        {
            if (CheckCanRemove != null)
            {
                var args = new PdfAnnotationEventArgs(annotation);
                CheckCanRemove(args);
                return !args.Cancel;
            }
            return true;
        }

        public void OnEndOperation(EditOperation operation, object result)
        {
            FinishOperation?.Invoke(new OperationEventArgs(operation, result));
            OnChanged();
        }

        public AnnotationOperation BeginOperation(Annotation annotation, OperationType type, object property = null, object begin = null, object end = null)
        {
            var operation = new AnnotationOperation
            {
                OperationList = this,
                Document = Viewer.Document.GetDocumentView(annotation.Document),
                Annotation = annotation,
                Type = type,
                Property = property,
                OldValue = begin,
                NewValue = end
            };
            if (type == OperationType.AnnotationDrag
                || type == OperationType.AnnotationSize)
            {
                operation.OldValue = annotation.GetViewBounds();
            }
            if (property is ControlPoint controlPoint)
            {
                operation.OldValue = controlPoint.MappedPoint;
            }
            EqueuOperation(operation);
            if (end != null)
            {
                OnChanged();
            }
            return operation;
        }

        private void EqueuOperation(AnnotationOperation operation)
        {
            if (lastLink == null)
            {
                operations.Clear();
                lastLink = operations.AddFirst(operation);
            }
            else
            {
                var next = lastLink;
                while (next?.Next != null)
                {
                    next = next.Next;
                }
                while (next != lastLink)
                {
                    next = next.Previous;
                    operations.Remove(next.Next);
                }
                lastLink = operations.AddAfter(lastLink, operation);
            }
        }

        private void OnCurrentOperationChanged(OperationType oldValue, OperationType newValue)
        {
            var selectedAnnotation = SelectedAnnotation;
            var lastOperation = lastLink?.Value;
            if (selectedAnnotation != null
                && lastOperation is AnnotationOperation annotationOperation
                && annotationOperation.Annotation == selectedAnnotation)
            {
                switch (oldValue)
                {
                    case OperationType.AnnotationAdd:
                        lastOperation.EndOperation();
                        break;
                    case OperationType.AnnotationDrag:
                        lastOperation.EndOperation();
                        break;
                    case OperationType.AnnotationSize:
                        lastOperation.EndOperation();
                        break;
                    case OperationType.PointMove:
                    case OperationType.PointAdd:
                    case OperationType.PointRemove:
                        lastOperation.EndOperation();
                        break;
                }
            }
            if (newValue != OperationType.None)
            {
                if (selectedAnnotation == null)
                {
                    throw new InvalidOperationException("SelectedAnnotation is not specified!");
                }
            }
            switch (newValue)
            {
                case OperationType.AnnotationDrag:
                    if (!selectedAnnotation.IsNew)
                    {
                        BeginOperation(selectedAnnotation, newValue, "Box");
                    }
                    else
                    {
                        Viewer.Cursor = CursorType.Cross;
                    }
                    break;
                case OperationType.AnnotationSize:
                    BeginOperation(selectedAnnotation, newValue, "Box");
                    Viewer.Cursor = CursorType.SizeNWSE;
                    break;
                case OperationType.PointMove:
                case OperationType.PointAdd:
                case OperationType.PointRemove:
                    if (SelectedPoint == null)
                    {
                        throw new InvalidOperationException("SelectedPoint is not specified!");
                    }
                    BeginOperation(selectedAnnotation, newValue, SelectedPoint);
                    Viewer.Cursor = CursorType.Cross;
                    break;
                case OperationType.None:
                    Viewer.Cursor = CursorType.Arrow;
                    break;
            }
        }

        public bool OnKeyDown(string keyName, KeyModifiers modifiers)
        {
            if (string.Equals(keyName, "Delete", StringComparison.OrdinalIgnoreCase))
            {
                if (!Viewer.IsReadOnly)
                {
                    if (SelectedPoint is IndexControlPoint indexControlPoint)
                    {
                        BeginOperation(indexControlPoint.Annotation, OperationType.PointRemove, indexControlPoint, indexControlPoint.MappedPoint, indexControlPoint.MappedPoint);
                        ((VertexShape)indexControlPoint.Annotation).RemovePoint(indexControlPoint.Index);
                        return true;
                    }
                    else if (SelectedAnnotation is Annotation annotation)
                    {
                        RemoveAnnotation(annotation);
                        return true;
                    }
                }
            }
            else if (string.Equals(keyName, "Escape", StringComparison.OrdinalIgnoreCase))
            {
                if (SelectedPoint != null
                    && SelectedAnnotation is VertexShape vertexShape
                    && Current == OperationType.PointAdd)
                {
                    CloseVertextShape(vertexShape);
                    return true;
                }
            }
            else if (string.Equals(keyName, "Z", StringComparison.OrdinalIgnoreCase))
            {
                if (modifiers == KeyModifiers.Ctrl)
                {
                    Undo();
                    return true;
                }
                else if (modifiers == (KeyModifiers.Ctrl | KeyModifiers.Shift))
                {
                    Redo();
                    return true;
                }
            }
            return false;
        }

        public void MoveToLast()
        {
            if (operations.Any())
            {
                lastLink = operations.Last;
            }
        }

        public LinkedList<EditOperation>.Enumerator GetEnumerator() => operations.GetEnumerator();

        IEnumerator<EditOperation> IEnumerable<EditOperation>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
