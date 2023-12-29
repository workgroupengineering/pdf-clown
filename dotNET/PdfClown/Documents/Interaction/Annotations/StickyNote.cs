/*
  Copyright 2008-2015 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library" (the
  Program): see the accompanying README files for more info.

  This Program is free software; you can redistribute it and/or modify it under the terms
  of the GNU Lesser General Public License as published by the Free Software Foundation;
  either version 3 of the License, or (at your option) any later version.

  This Program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY,
  either expressed or implied; without even the implied warranty of MERCHANTABILITY or
  FITNESS FOR A PARTICULAR PURPOSE. See the License for more details.

  You should have received a copy of the GNU Lesser General Public License along with this
  Program (see README files); if not, go to the GNU website (http://www.gnu.org/licenses/).

  Redistribution and use, with or without modification, are permitted provided that such
  redistributions retain the above copyright notice, license and disclaimer, along with
  this list of conditions.
*/

using PdfClown.Bytes;
using PdfClown.Documents;
using PdfClown.Objects;

using System;
using System.Collections.Generic;
using SkiaSharp;
using PdfClown.Tools;
using PdfClown.Documents.Contents.ColorSpaces;
using PdfClown.Util;
using PdfClown.Documents.Interaction.Annotations.ControlPoints;
using PdfClown.Documents.Contents.Composition;
using PdfClown.Documents.Contents.XObjects;
using PdfClown.Util.Math.Geom;

namespace PdfClown.Documents.Interaction.Annotations
{
    /**
      <summary>Text annotation [PDF:1.6:8.4.5].</summary>
      <remarks>It represents a sticky note attached to a point in the PDF document.</remarks>
    */
    [PDF(VersionEnum.PDF10)]
    public sealed class StickyNote : Markup
    {
        public const int size = 28;

        private static readonly ImageNameEnum DefaultIconType = ImageNameEnum.Note;
        private static readonly bool DefaultOpen = false;
        private static readonly Dictionary<ImageNameEnum, PdfName> IconTypeEnumCodes;

        static StickyNote()
        {
            IconTypeEnumCodes = new Dictionary<ImageNameEnum, PdfName>
            {
                [ImageNameEnum.Comment] = PdfName.Comment,
                [ImageNameEnum.Help] = PdfName.Help,
                [ImageNameEnum.Insert] = PdfName.Insert,
                [ImageNameEnum.Key] = PdfName.Key,
                [ImageNameEnum.NewParagraph] = PdfName.NewParagraph,
                [ImageNameEnum.Note] = PdfName.Note,
                [ImageNameEnum.Paragraph] = PdfName.Paragraph
            };
        }

        /**
          <summary>Gets the code corresponding to the given value.</summary>
        */
        private static PdfName ToCode(ImageNameEnum value)
        {
            return IconTypeEnumCodes[value];
        }

        /**
          <summary>Gets the icon type corresponding to the given value.</summary>
        */
        private static ImageNameEnum ToIconTypeEnum(IPdfString value)
        {
            if (value == null)
                return DefaultIconType;
            foreach (KeyValuePair<ImageNameEnum, PdfName> iconType in IconTypeEnumCodes)
            {
                if (string.Equals(iconType.Value.StringValue, value.StringValue, StringComparison.Ordinal))
                    return iconType.Key;
            }
            return DefaultIconType;
        }

        public StickyNote(PdfPage page, SKPoint location, string text)
            : base(page, PdfName.Text, SKRect.Create(location.X, location.Y, 0, 0), text)
        {
        }

        public StickyNote(PdfDirectObject baseObject) : base(baseObject)
        { }

        /**
          <summary>Gets/Sets the icon to be used in displaying the annotation.</summary>
        */
        public ImageNameEnum ImageName
        {
            get => ToIconTypeEnum((IPdfString)BaseDataObject[PdfName.Name]);
            set
            {
                var oldValue = ImageName;
                if (oldValue != value)
                {
                    BaseDataObject[PdfName.Name] = (value != DefaultIconType ? ToCode(value) : null);
                    GenerateAppearance();
                    OnPropertyChanged(oldValue, value);
                }
            }
        }

        /**
          <summary>Gets/Sets whether the annotation should initially be displayed open.</summary>
        */
        public bool IsOpen
        {
            get => BaseDataObject.GetBool(PdfName.Open, DefaultOpen);
            set
            {
                var oldValue = IsOpen;
                if (oldValue != value)
                {
                    BaseDataObject[PdfName.Open] = (value != DefaultOpen ? PdfBoolean.Get(value) : null);
                    OnPropertyChanged(oldValue, value);
                }
            }
        }

        [PDF(VersionEnum.PDF15)]
        public MarkupState? State
        {
            get => MarkupStateExtension.Get(BaseDataObject.GetString(PdfName.State));
            set
            {
                var oldValue = State;
                if (oldValue != value)
                {
                    BaseDataObject.SetName(PdfName.State, MarkupStateExtension.GetCode(value));
                    OnPropertyChanged(oldValue, value);
                }
            }
        }

        public MarkupStateModel? StateModel
        {
            get => MarkupStateModelExtension.Get(BaseDataObject.GetString(PdfName.StateModel));
            set
            {
                var oldValue = StateModel;
                if (oldValue != value)
                {
                    BaseDataObject.SetName(PdfName.StateModel, MarkupStateModelExtension.GetCode(value));
                    OnPropertyChanged(oldValue, value);
                }
            }
        }

        protected override FormXObject GenerateAppearance()
        {
            SKRect bound = GetBound();
            var normalAppearance = ResetAppearance(bound, out var zeroMatrix);
            bound = zeroMatrix.MapRect(bound);
            var svg = SvgImage.GetCache(ImageName.ToString());
            var pathMatrix = SvgImage.GetMatrix(svg, bound, 2);
            using var tempPath = new SKPath();
            svg.Path.Transform(pathMatrix, tempPath);
            //var pathBounds = pathMatrix.MapRect(svg.Path.Bounds);
            //pathMatrix = pathMatrix.PostConcat(SKMatrix.CreateTranslation(0, bound.Height));
            var composer = new PrimitiveComposer(normalAppearance);
            composer.SetLineWidth(1);
            composer.SetStrokeColor(DeviceRGBColor.Default);
            composer.SetFillColor(Color ?? DeviceRGBColor.White);
            composer.DrawPath(tempPath);
            composer.FillStroke();
            composer.Flush();
            return normalAppearance;
        }

        protected override SKRect GetDrawBox()
        {
            return GetBound();
        }

        private SKRect GetBound()
        {
            if (Box.Width == 0 || Box.Height == 0)
            {
                SKPoint location = Box.Location;
                return SKRect.Create(location + new SKPoint(0, -size), new SKSize(size, size));
            }
            return Box;
        }

        public override void SetBounds(SKRect value)
        {
            base.SetBounds(SKRect.Create(value.Left, value.Top, 0, 0));
        }

        public override SKRect GetViewBounds(SKMatrix viewMatrix)
        {
            base.GetViewBounds();
            var bound = GetBound();
            return viewMatrix.PreConcat(PageMatrix).MapRect(bound);
            //return SKRect.Create(bound.Left, bound.Top, size / Math.Abs(viewMatrix.ScaleX), size / Math.Abs(viewMatrix.ScaleY));
        }

        public override IEnumerable<ControlPoint> GetControlPoints()
        {
            yield break;
        }
    }

    public enum MarkupState
    {
        None,
        Unmarked,
        Accepted,
        Rejected,
        Cancelled,
        Completed,
    }

    public enum MarkupStateModel
    {
        Marked,
        Review
    }
    internal static class MarkupStateExtension
    {
        private static readonly BiDictionary<MarkupState, string> codes;

        static MarkupStateExtension()
        {
            codes = new BiDictionary<MarkupState, string>
            {
                [MarkupState.None] = PdfName.None.StringValue,
                [MarkupState.Unmarked] = PdfName.Unmarked.StringValue,
                [MarkupState.Accepted] = PdfName.Accepted.StringValue,
                [MarkupState.Rejected] = PdfName.Rejected.StringValue,
                [MarkupState.Cancelled] = PdfName.Cancelled.StringValue,
                [MarkupState.Completed] = PdfName.Completed.StringValue,
            };
        }

        public static MarkupState? Get(IPdfString name)
        {
            if (name == null)
                return null;

            return Get(name.StringValue);
        }

        public static MarkupState? Get(string name)
        {
            if (name == null)
                return null;

            return codes.GetKey(name);
        }

        public static string GetCode(this MarkupState? intent)
        {
            return intent == null ? null : codes[intent.Value];
        }
    }

    internal static class MarkupStateModelExtension
    {
        private static readonly BiDictionary<MarkupStateModel, string> codes;

        static MarkupStateModelExtension()
        {
            codes = new BiDictionary<MarkupStateModel, string>
            {
                [MarkupStateModel.Marked] = PdfName.Marked.StringValue,
                [MarkupStateModel.Review] = PdfName.Review.StringValue
            };
        }

        public static MarkupStateModel? Get(IPdfString name)
        {
            if (name == null)
                return null;

            return Get(name.StringValue);
        }

        public static MarkupStateModel? Get(string name)
        {
            if (name == null)
                return null;

            return codes.GetKey(name);
        }

        public static string GetCode(this MarkupStateModel? intent)
        {
            return intent == null ? null : codes[intent.Value];
        }
    }
    /**
         <summary>Icon to be used in displaying the annotation [PDF:1.6:8.4.5].</summary>
    */
    public enum ImageNameEnum
    {
        /**
          <summary>Comment.</summary>
        */
        Comment,
        /**
          <summary>Help.</summary>
        */
        Help,
        /**
          <summary>Insert.</summary>
        */
        Insert,
        /**
          <summary>Key.</summary>
        */
        Key,
        /**
          <summary>New paragraph.</summary>
        */
        NewParagraph,
        /**
          <summary>Note.</summary>
        */
        Note,
        /**
          <summary>Paragraph.</summary>
        */
        Paragraph
    };

}