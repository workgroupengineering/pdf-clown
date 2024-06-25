/*
  Copyright 2009-2015 Stefano Chizzolini. http://www.pdfclown.org

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

using PdfClown.Documents.Contents.Fonts;
using PdfClown.Documents.Contents.XObjects;
using PdfClown.Documents.Functions;
using PdfClown.Objects;

using System;
using System.Collections.Generic;

namespace PdfClown.Documents.Contents
{
    ///<summary>Graphics state parameters [PDF:1.6:4.3.4].</summary>
    [PDF(VersionEnum.PDF12)]
    public sealed class ExtGState : PdfObjectWrapper<PdfDictionary>
    {
        private static readonly Dictionary<PdfName, Action<ExtGState, GraphicsState>> cache = new(24)
        {
            { PdfName.Font, (eState, gState) => gState.SetFont(eState)},
            { PdfName.AIS, (eState, gState) => gState.AlphaIsShape = eState.AlphaShape },
            { PdfName.CA, (eState, gState) => gState.StrokeAlpha = eState.StrokeAlpha },
            { PdfName.ca, (eState, gState) => gState.FillAlpha = eState.FillAlpha },
            { PdfName.SA, (eState, gState) => gState.StrokeAdjustment = eState.StrokeAdjustment },
            { PdfName.OP, (eState, gState) => gState.StrokeOverprint = eState.StrokeOverprint },
            { PdfName.op, (eState, gState) => gState.FillOverprint = eState.FillOverprint },
            { PdfName.OPM, (eState, gState) => gState.OverprintMode = eState.OverprintMode },
            { PdfName.LC, (eState, gState) => gState.LineCap = eState.LineCap ?? LineCapEnum.Butt },
            { PdfName.LJ, (eState, gState) => gState.LineJoin = eState.LineJoin ?? LineJoinEnum.Miter },
            { PdfName.LW, (eState, gState) => gState.LineWidth = eState.LineWidth ?? 0 },
            { PdfName.D, (eState, gState) => gState.LineDash = eState.LineDash },
            { PdfName.ML, (eState, gState) => gState.MiterLimit = eState.MiterLimit ?? 0 },
            { PdfName.BM, (eState, gState) => gState.BlendMode = eState.BlendMode },
            { PdfName.SMask, (eState, gState) => gState.SMask = eState.SMask },
            { PdfName.TK, (eState, gState) => gState.Knockout = eState.Knockout },
            { PdfName.BG, (eState, gState) => gState.Function = eState.BG },
            { PdfName.BG2, (eState, gState) => gState.Function = eState.BG2 },
            //TODO:extend supported parameters!!!
        };
        private LineDash lineDash;

        public ExtGState(PdfDocument context) : base(context, new PdfDictionary())
        { }

        public ExtGState(PdfDirectObject baseObject) : base(baseObject)
        { }

        ///<summary>Gets/Sets whether the current soft mask and alpha constant are to be interpreted as
        ///shape values instead of opacity values.</summary>
        [PDF(VersionEnum.PDF14)]
        public bool AlphaShape
        {
            get => BaseDataObject.GetBool(PdfName.AIS, false);
            set => BaseDataObject.Set(PdfName.AIS, value);
        }

        public void ApplyTo(GraphicsState state)
        {
            foreach (PdfName parameterName in BaseDataObject.Keys)
            {
                if (cache.TryGetValue(parameterName, out var func))
                    func(this, state);
            }
        }

        [PDF(VersionEnum.PDF14)]
        public SoftMask SMask
        {
            get => SoftMask.WrapSoftMask(BaseDataObject[PdfName.SMask]);
            set => BaseDataObject[PdfName.SMask] = value.BaseObject;
        }

        public bool? Knockout
        {
            get => BaseDataObject.GetNBool(PdfName.TK);
            set => BaseDataObject.Set(PdfName.TK, value);
        }

        ///<summary>Gets/Sets the blend mode to be used in the transparent imaging model [PDF:1.7:7.2.4].
        ///</summary>
        [PDF(VersionEnum.PDF14)]
        public BlendModeEnum? BlendMode
        {
            get
            {
                PdfDirectObject blendModeObject = BaseDataObject[PdfName.BM];
                if (blendModeObject == null)
                    return null;

                if (blendModeObject is PdfName name)
                { return BlendModeEnumExtension.Get(name); }
                else // MUST be an array.
                {
                    foreach (PdfName alternateBlendModeObject in (PdfArray)blendModeObject)
                    {
                        if (BlendModeEnumExtension.Get(alternateBlendModeObject) is BlendModeEnum blendMode)
                            return blendMode;
                    }
                }
                return null;
            }
            set
            {
                BaseDataObject[PdfName.BM] = value == null ? null : (PdfDirectObject)value.Value.GetName();
            }
        }

        ///<summary>Gets/Sets the nonstroking alpha constant, specifying the constant shape or constant
        ///opacity value to be used for nonstroking operations in the transparent imaging model
        ///[PDF:1.7:7.2.6].</summary>
        [PDF(VersionEnum.PDF14)]
        public float? FillAlpha
        {
            get => BaseDataObject.GetNFloat(PdfName.ca);
            set => BaseDataObject.Set(PdfName.ca, value);
        }

        ///<summary>Gets/Sets the stroking alpha constant, specifying the constant shape or constant
        ///opacity value to be used for stroking operations in the transparent imaging model
        ///[PDF:1.7:7.2.6].</summary>
        [PDF(VersionEnum.PDF14)]
        public float? StrokeAlpha
        {
            get => BaseDataObject.GetNFloat(PdfName.CA);
            set => BaseDataObject.Set(PdfName.CA, value);
        }

        public bool FillOverprint
        {
            get => BaseDataObject.GetBool(PdfName.op);
            set => BaseDataObject.Set(PdfName.op, value);
        }

        public bool StrokeOverprint
        {
            get => BaseDataObject.GetBool(PdfName.OP);
            set => BaseDataObject.Set(PdfName.OP, value);
        }

        public int OverprintMode
        {
            get => BaseDataObject.GetInt(PdfName.OPM);
            set => BaseDataObject.Set(PdfName.OPM, value);
        }

        public bool StrokeAdjustment
        {
            get => BaseDataObject.GetBool(PdfName.SA);
            set => BaseDataObject.Set(PdfName.SA, value);
        }

        [PDF(VersionEnum.PDF13)]
        public Font Font
        {
            get
            {
                var fontObject = BaseDataObject.Get<PdfArray>(PdfName.Font);
                return Font.Wrap(fontObject?[0]);
            }
            set
            {
                var fontObject = BaseDataObject.Get<PdfArray>(PdfName.Font);
                if (fontObject == null)
                { fontObject = new PdfArray(2) { PdfObjectWrapper.GetBaseObject(value), PdfInteger.Default }; }
                else
                { fontObject[0] = PdfObjectWrapper.GetBaseObject(value); }
                BaseDataObject[PdfName.Font] = fontObject;
            }
        }

        [PDF(VersionEnum.PDF13)]
        public float? FontSize
        {
            get
            {
                var fontObject = BaseDataObject.Get<PdfArray>(PdfName.Font);
                return fontObject?.GetFloat(1);
            }
            set
            {
                var fontObject = BaseDataObject.Get<PdfArray>(PdfName.Font);
                if (fontObject == null)
                { fontObject = new PdfArray(2) { (string)null, value }; }
                else
                { fontObject.Set(1, value); }
                BaseDataObject[PdfName.Font] = fontObject;
            }
        }

        [PDF(VersionEnum.PDF13)]
        public LineCapEnum? LineCap
        {
            get => (LineCapEnum?)BaseDataObject.GetNInt(PdfName.LC);
            set => BaseDataObject.Set(PdfName.LC, value.HasValue ? (int)value.Value : null);
        }

        [PDF(VersionEnum.PDF13)]
        public LineDash LineDash
        {
            get => lineDash ??= BaseDataObject.Resolve(PdfName.D) is PdfArray lineDashObject
                ? LineDash.Get(lineDashObject.Get<PdfArray>(0), lineDashObject.GetNumber(1))
                : null;
            set
            {
                lineDash = value;
                BaseDataObject[PdfName.D] = new PdfArray
                {
                    new PdfArray(value.DashArray),
                    value.DashPhase
                };
            }
        }

        [PDF(VersionEnum.PDF13)]
        public LineJoinEnum? LineJoin
        {
            get => (LineJoinEnum?)BaseDataObject.GetNInt(PdfName.LJ);
            set => BaseDataObject.Set(PdfName.LJ, value.HasValue ? (int)value.Value : null);
        }

        [PDF(VersionEnum.PDF13)]
        public float? LineWidth
        {
            get => BaseDataObject.GetNFloat(PdfName.LW);
            set => BaseDataObject.Set(PdfName.LW, value);
        }

        [PDF(VersionEnum.PDF13)]
        public float? MiterLimit
        {
            get => BaseDataObject.GetNFloat(PdfName.ML);
            set => BaseDataObject.Set(PdfName.ML, value);
        }

        public Function BG
        {
            get => BaseDataObject[PdfName.BG] is PdfName ? null : Function.Wrap(BaseDataObject[PdfName.BG]);
            set => BaseDataObject[PdfName.BG] = value.BaseObject;
        }

        public Function BG2
        {
            get => BaseDataObject[PdfName.BG2] is PdfName ? null : Function.Wrap(BaseDataObject[PdfName.BG2]);
            set => BaseDataObject[PdfName.BG2] = value.BaseObject;
        }
    }
}