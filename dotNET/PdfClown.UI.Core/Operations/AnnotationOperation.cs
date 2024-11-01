﻿using PdfClown.Documents.Interaction.Annotations;
using PdfClown.Documents.Interaction.Annotations.ControlPoints;
using PdfClown.Util.Invokers;
using SkiaSharp;

namespace PdfClown.UI.Operations
{
    public class AnnotationOperation : EditOperation
    {
        private Annotation annotation;

        public Annotation Annotation
        {
            get => annotation;
            set
            {
                annotation = value;
                if (annotation.Page != null)
                {
                    PageIndex = annotation.Page.Index;
                }
            }
        }

        public override EditOperation Clone(PdfDocumentViewModel document)
        {
            var cloned = (AnnotationOperation)base.Clone(document);

            cloned.Annotation = document.FindAnnotation(Annotation.Name)
                ?? (Annotation)Annotation.Clone(document.Document);

            if (cloned.Property is ControlPoint controlPoint)
                cloned.Property = controlPoint.Clone(cloned.Annotation);

            return cloned;
        }

        public override object EndOperation()
        {
            var result = (object)null;
            switch (Type)
            {
                case OperationType.AnnotationDrag:
                case OperationType.AnnotationSize:
                    result =
                        NewValue = Annotation.GetViewBounds();
                    break;
                case OperationType.AnnotationAdd:
                    result =
                        Document.AddAnnotation(Annotation);
                    break;
                case OperationType.AnnotationRemove:
                    result =
                        Document.RemoveAnnotation(Annotation);
                    break;
            }
            if (Property is ControlPoint controlPoint)
            {
                result =
                    NewValue = controlPoint.MappedPoint;
            }
            OperationList?.OnEndOperation(this, result);
            return result;
        }

        public override void Redo()
        {
            if (Annotation.Page?.Annotations.Contains(Annotation) ?? false)
            {
                OperationList.SelectedAnnotation = Annotation;
            }
            switch (Type)
            {
                case OperationType.AnnotationAdd:
                    Document.AddAnnotation(Annotation.Page ?? Document.GetPageView(PageIndex)?.Page, Annotation);
                    break;
                case OperationType.AnnotationRemove:
                    Document.RemoveAnnotation(Annotation);
                    break;
                case OperationType.AnnotationDrag:
                    Annotation.SetBounds((SKRect)NewValue);
                    break;
                case OperationType.AnnotationSize:
                    Annotation.SetBounds((SKRect)NewValue);
                    break;
                case OperationType.AnnotationRePage:
                    Annotation.Page = Document.GetPageView((int)NewValue)?.Page;
                    break;
                case OperationType.AnnotationProperty:
                    var invoker = Property as IInvoker;
                    invoker.SetValue(Annotation, NewValue);
                    break;
                case OperationType.PointMove:
                    {
                        if (Property is ControlPoint controlPoint)
                        {
                            controlPoint.MappedPoint = (SKPoint)NewValue;
                        }
                        break;
                    }
                case OperationType.PointAdd:
                    {
                        if (Property is IndexControlPoint controlPoint)
                        {
                            if (Annotation is VertexShape vertexShape)
                            {
                                vertexShape.InsertPoint(controlPoint.Index, (SKPoint)NewValue);
                            }
                        }
                    }
                    break;
                case OperationType.PointRemove:
                    {
                        if (Property is IndexControlPoint controlPoint)
                        {
                            if (Annotation is VertexShape vertexShape)
                            {
                                vertexShape.RemovePoint(controlPoint.Index);
                            }
                        }
                    }
                    break;
            }
            if (Annotation.Page?.Annotations.Contains(Annotation) ?? false)
            {
                OperationList.SelectedAnnotation = Annotation;
            }
        }

        public override void Undo()
        {
            if (Annotation.Page?.Annotations.Contains(Annotation) ?? false)
            {
                OperationList.SelectedAnnotation = Annotation;
            }
            switch (Type)
            {
                case OperationType.AnnotationAdd:
                    Document.RemoveAnnotation(Annotation);
                    break;
                case OperationType.AnnotationRemove:
                    Document.AddAnnotation(Annotation.Page ?? Document.GetPageView(PageIndex)?.Page, Annotation);
                    break;
                case OperationType.AnnotationDrag:
                    Annotation.SetBounds((SKRect)OldValue);
                    break;
                case OperationType.AnnotationSize:
                    Annotation.SetBounds((SKRect)OldValue);
                    break;
                case OperationType.AnnotationRePage:
                    Annotation.Page = Document.Pages[(int)OldValue];
                    break;
                case OperationType.AnnotationProperty:
                    var invoker = Property as IInvoker;
                    invoker.SetValue(Annotation, OldValue);
                    break;
                case OperationType.PointMove:
                    {
                        if (Property is ControlPoint controlPoint)
                        {
                            controlPoint.MappedPoint = (SKPoint)OldValue;
                        }
                        break;
                    }
                case OperationType.PointAdd:
                    {
                        if (Property is IndexControlPoint controlPoint)
                        {
                            if (Annotation is VertexShape vertexShape)
                            {
                                vertexShape.RemovePoint(controlPoint.Index);
                            }
                        }
                    }
                    break;
                case OperationType.PointRemove:
                    {
                        if (Property is IndexControlPoint controlPoint)
                        {
                            if (Annotation is VertexShape vertexShape)
                            {
                                vertexShape.InsertPoint(controlPoint.Index, (SKPoint)NewValue);
                            }
                        }
                    }
                    break;
            }

            if (Annotation.Page?.Annotations.Contains(Annotation) ?? false)
            {
                OperationList.SelectedAnnotation = Annotation;
            }
        }
    }
}
