/*
  Copyright 2012 Stefano Chizzolini. http://www.pdfclown.org

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

using PdfClown.Documents.Contents.ColorSpaces;
using PdfClown.Objects;
using SkiaSharp;

namespace PdfClown.Documents.Multimedia
{
    /// <summary>Media screen parameters [PDF:1.7:9.1.5].</summary>
    [PDF(VersionEnum.PDF15)]
    public sealed class MediaScreenParameters : PdfObjectWrapper<PdfDictionary>
    {
        /// <summary>Media screen parameters viability.</summary>
        public class Viability : PdfObjectWrapper2<PdfDictionary>
        {
            public class FloatingWindowParametersObject : PdfObjectWrapper<PdfDictionary>
            {
                public enum LocationEnum
                {
                    ///<summary>Upper-left corner.</summary>
                    UpperLeft,
                    ///<summary>Upper center.</summary>
                    UpperCenter,
                    ///<summary>Upper-right corner.</summary>
                    UpperRight,
                    ///<summary>Center left.</summary>
                    CenterLeft,
                    ///<summary>Center.</summary>
                    Center,
                    ///<summary>Center right.</summary>
                    CenterRight,
                    ///<summary>Lower-left corner.</summary>
                    LowerLeft,
                    ///<summary>Lower center.</summary>
                    LowerCenter,
                    ///<summary>Lower-right corner.</summary>
                    LowerRight
                }

                public enum OffscreenBehaviorEnum
                {
                    ///<summary>Take no special action.</summary>
                    None,
                    ///<summary>Move and/or resize the window so that it is on-screen.</summary>
                    Adapt,
                    ///<summary>Consider the object to be non-viable.</summary>
                    NonViable
                }

                public enum RelatedWindowEnum
                {
                    ///<summary>The document window.</summary>
                    Document,
                    ///<summary>The application window.</summary>
                    Application,
                    ///<summary>The full virtual desktop.</summary>
                    Desktop,
                    ///<summary>The monitor specified by <see cref="MediaScreenParameters.Viability.MonitorSpecifier"/>.</summary>
                    Custom
                }

                public enum ResizeBehaviorEnum
                {
                    ///<summary>Not resizable.</summary>
                    None,
                    ///<summary>Resizable preserving its aspect ratio.</summary>
                    AspectRatioLocked,
                    ///<summary>Resizable without preserving its aspect ratio.</summary>
                    Free
                }

                public FloatingWindowParametersObject(SKSize size)
                    : base(new PdfDictionary(7) { { PdfName.Type, PdfName.FWParams } })
                { this.Size = size; }

                public FloatingWindowParametersObject(PdfDirectObject baseObject)
                    : base(baseObject)
                { }

                /// <summary>Gets/Sets the location where the floating window should be positioned relative to
                /// the related window.</summary>
                public LocationEnum? Location
                {
                    get => (LocationEnum?)BaseDataObject.GetNInt(PdfName.P);
                    set => BaseDataObject.Set(PdfName.P, (int?)value);
                }

                /// <summary>Gets/Sets what should occur if the floating window is positioned totally or
                /// partially offscreen (that is, not visible on any physical monitor).</summary>
                public OffscreenBehaviorEnum? OffscreenBehavior
                {
                    get => (OffscreenBehaviorEnum?)BaseDataObject.GetNInt(PdfName.O);
                    set => BaseDataObject.Set(PdfName.O, (int?)value);
                }

                /// <summary>Gets/Sets the window relative to which the floating window should be positioned.
                /// </summary>
                public RelatedWindowEnum? RelatedWindow
                {
                    get => (RelatedWindowEnum?)BaseDataObject.GetNInt(PdfName.RT);
                    set => BaseDataObject.Set(PdfName.RT, (int?)value);
                }

                /// <summary>Gets/Sets how the floating window may be resized by a user.</summary>
                public ResizeBehaviorEnum? ResizeBehavior
                {
                    get => (ResizeBehaviorEnum?)BaseDataObject.GetNInt(PdfName.R);
                    set => BaseDataObject.Set(PdfName.R, (int?)value);
                }

                /// <summary>Gets/Sets the floating window's width and height, in pixels.</summary>
                /// <remarks>These values correspond to the dimensions of the rectangle in which the media
                /// will play, not including such items as title bar and resizing handles.</remarks>
                public SKSize Size
                {
                    get
                    {
                        var sizeObject = BaseDataObject.Get<PdfArray>(PdfName.D);
                        return new SKSize(sizeObject.GetInt(0), sizeObject.GetInt(1));
                    }
                    set => BaseDataObject[PdfName.D] = new PdfArray(2) { (int)value.Width, (int)value.Height };
                }

                /// <summary>Gets/Sets whether the floating window should include user interface elements that
                /// allow a user to close it.</summary>
                /// <remarks>Meaningful only if <see cref="TitleBarVisible"/> is true.</remarks>
                public bool Closeable
                {
                    get => BaseDataObject.GetBool(PdfName.UC, true);
                    set => BaseDataObject.Set(PdfName.UC, value);
                }

                /// <summary>Gets/Sets whether the floating window should have a title bar.</summary>
                public bool TitleBarVisible
                {
                    get => BaseDataObject.GetBool(PdfName.T, true);
                    set => BaseDataObject.Set(PdfName.T, value);
                }

                //TODO: TT entry!
            }

            public enum WindowTypeEnum
            {
                ///<summary>A floating window.</summary>
                Floating,
                ///<summary>A full-screen window that obscures all other windows.</summary>
                FullScreen,
                ///<summary>A hidden window.</summary>
                Hidden,
                ///<summary>The rectangle occupied by the {@link Screen screen annotation} associated with
                ///the media rendition.</summary>
                Annotation
            }

            public Viability(PdfDirectObject baseObject) : base(baseObject)
            { }

            /// <summary>Gets/Sets the background color for the rectangle in which the media is being played.
            /// </summary>
            /// <remarks>This color is used if the media object does not entirely cover the rectangle or if
            /// it has transparent sections.</remarks>
            public DeviceRGBColor BackgroundColor
            {
                get => (DeviceRGBColor)DeviceRGBColorSpace.Default.GetColor(BaseDataObject.Get<PdfArray>(PdfName.B), null);
                set => BaseDataObject[PdfName.B] = GetBaseObject(value);
            }

            /// <summary>Gets/Sets the opacity of the background color.</summary>
            /// <returns>A number in the range 0 to 1, where 0 means full transparency and 1 full opacity.
            /// </returns>
            public double BackgroundOpacity
            {
                get => BaseDataObject.GetDouble(PdfName.O, 1d);
                set
                {
                    if (value < 0)
                    { value = 0; }
                    else if (value > 1)
                    { value = 1; }
                    BaseDataObject.Set(PdfName.O, value);
                }
            }

            /// <summary>Gets/Sets the options used in displaying floating windows.</summary>
            public FloatingWindowParametersObject FloatingWindowParameters
            {
                get => new FloatingWindowParametersObject(BaseDataObject.GetOrCreate<PdfDictionary>(PdfName.F));
                set => BaseDataObject[PdfName.F] = PdfObjectWrapper.GetBaseObject(value);
            }

            /// <summary>Gets/Sets which monitor in a multi-monitor system a floating or full-screen window
            /// should appear on.</summary>
            public MonitorSpecifierEnum? MonitorSpecifier
            {
                get => (MonitorSpecifierEnum?)BaseDataObject.GetNInt(PdfName.M);
                set => BaseDataObject.Set(PdfName.M, (int?)value);
            }

            /// <summary>Gets/Sets the type of window that the media object should play in.</summary>
            public WindowTypeEnum? WindowType
            {
                get => (WindowTypeEnum?)BaseDataObject.GetNInt(PdfName.W);
                set => BaseDataObject.Set(PdfName.W, (int?)value);
            }
        }

        public MediaScreenParameters(PdfDocument context)
            : base(context, new PdfDictionary { { PdfName.Type, PdfName.MediaScreenParams } })
        { }

        public MediaScreenParameters(PdfDirectObject baseObject) : base(baseObject)
        { }

        /// <summary>Gets/Sets the preferred options the renderer should attempt to honor without affecting
        /// its viability.</summary>
        public Viability Preferences
        {
            get => Wrap2<Viability>(BaseDataObject.GetOrCreate<PdfDictionary>(PdfName.BE));
            set => BaseDataObject[PdfName.BE] = PdfObjectWrapper.GetBaseObject(value);
        }

        /// <summary>Gets/Sets the minimum requirements the renderer must honor in order to be considered
        /// viable.</summary>
        public Viability Requirements
        {
            get => Wrap2<Viability>(BaseDataObject.GetOrCreate<PdfDictionary>(PdfName.MH));
            set => BaseDataObject[PdfName.MH] = PdfObjectWrapper.GetBaseObject(value);
        }
    }


}