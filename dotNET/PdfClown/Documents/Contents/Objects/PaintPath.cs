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
using PdfClown.Documents.Contents;
using PdfClown.Util;

using System.Collections.Generic;
using SkiaSharp;

namespace PdfClown.Documents.Contents.Objects
{
    /**
      <summary>Path-painting operation [PDF:1.6:4.4.2].</summary>
    */
    [PDF(VersionEnum.PDF10)]
    public abstract class PaintPath : Operation
    {
        public const string CloseFillStrokeEvenOddOperatorKeyword = "b*";
        public const string CloseFillStrokeOperatorKeyword = "b";
        public const string CloseStrokeOperatorKeyword = "s";
        public const string EndPathNoOpOperatorKeyword = "n";
        public const string FillEvenOddOperatorKeyword = "f*";
        public const string FillCompatibleOperatorKeyword = "F";
        public const string FillOperatorKeyword = "f";
        public const string FillStrokeEvenOddOperatorKeyword = "B*";
        public const string FillStrokeOperatorKeyword = "B";
        public const string StrokeOperatorKeyword = "S";

        /**
          <summary>'Close, fill, and then stroke the path, using the nonzero winding number rule to determine
          the region to fill' operation.</summary>
        */
        public static readonly PaintPath CloseFillStroke = new PaintCloseFillStrokePath();
        /**
          <summary>'Close, fill, and then stroke the path, using the even-odd rule to determine the region
          to fill' operation.</summary>
        */
        public static readonly PaintPath CloseFillStrokeEvenOdd = new PaintCloseFillStrokeEvenOddPath();
        /**
          <summary>'Close and stroke the path' operation.</summary>
        */
        public static readonly PaintPath CloseStroke = new PaintCloseStrokePath();
        /**
          <summary>'End the path object without filling or stroking it' operation.</summary>
        */
        public static readonly PaintPath EndPathNoOp = new PaintEndPathNoOp();
        /**
          <summary>'Fill the path, using the nonzero winding number rule to determine the region to fill' operation.</summary>
        */
        public static readonly PaintPath Fill = new PaintFillPath();
        /**
          <summary>'Fill the path, using the even-odd rule to determine the region to fill' operation.</summary>
        */
        public static readonly PaintPath FillEvenOdd = new PaintFillEvenOddPath();
        /**
          <summary>'Fill and then stroke the path, using the nonzero winding number rule to determine the region to
          fill' operation.</summary>
        */
        public static readonly PaintPath FillStroke = new PaintFillStrokePath();
        /**
          <summary>'Fill and then stroke the path, using the even-odd rule to determine the region to fill' operation.</summary>
        */
        public static readonly PaintPath FillStrokeEvenOdd = new PaintFillStrokeEvenOddPath();
        /**
          <summary>'Stroke the path' operation.</summary>
        */
        public static readonly PaintPath Stroke = new PaintStrokePath();

        public PaintPath(string @operator) : base(@operator)
        { }

        public override void Scan(GraphicsState state)
        {
            state.ModifyClipPath?.Scan(state);
            state.ModifyClipPath = null;
        }
    }

    public sealed class PaintCloseFillStrokePath : PaintPath
    {
        public PaintCloseFillStrokePath() : base(PaintPath.CloseFillStrokeOperatorKeyword)
        {
        }

        public override void Scan(GraphicsState state)
        {
            var scanner = state.Scanner;
            if (scanner.Canvas is SKCanvas canvas)
            {
                var pathObject = scanner.Path;
                pathObject.Close();
                using (var paint = state.CreateFillPaint())
                {
                    canvas.DrawPath(pathObject, paint);
                }

                using (var paint = state.CreateStrokePaint())
                {
                    canvas.DrawPath(pathObject, paint);
                }
            }
            base.Scan(state);
        }
    }

    public sealed class PaintCloseFillStrokeEvenOddPath : PaintPath
    {
        public PaintCloseFillStrokeEvenOddPath() : base(PaintPath.CloseFillStrokeEvenOddOperatorKeyword)
        {
        }

        public override void Scan(GraphicsState state)
        {
            var scanner = state.Scanner;
            if (scanner.Canvas is SKCanvas canvas)
            {
                var pathObject = scanner.Path;
                pathObject.Close();
                pathObject.FillType = SKPathFillType.EvenOdd;
                using (var paint = state.CreateFillPaint())
                {
                    canvas.DrawPath(pathObject, paint);
                }

                using (var paint = state.CreateStrokePaint())
                {
                    canvas.DrawPath(pathObject, paint);
                }
            }
            base.Scan(state);
        }
    }

    public sealed class PaintCloseStrokePath : PaintPath
    {
        public PaintCloseStrokePath() : base(PaintPath.CloseStrokeOperatorKeyword)
        {
        }

        public override void Scan(GraphicsState state)
        {
            var scanner = state.Scanner;
            if (scanner.Canvas is SKCanvas canvas)
            {
                var pathObject = scanner.Path;
                pathObject.Close();

                using (var paint = state.CreateStrokePaint())
                {
                    canvas.DrawPath(pathObject, paint);
                }
            }
            base.Scan(state);
        }
    }

    public sealed class PaintEndPathNoOp : PaintPath
    {
        public PaintEndPathNoOp() : base(PaintPath.EndPathNoOpOperatorKeyword)
        {
        }

        public override void Scan(GraphicsState state)
        {
            base.Scan(state);
        }
    }

    public sealed class PaintFillPath : PaintPath
    {
        public PaintFillPath() : base(PaintPath.FillOperatorKeyword)
        {
        }

        public override void Scan(GraphicsState state)
        {
            var scanner = state.Scanner;
            if (scanner.Canvas is SKCanvas canvas)
            {
                var pathObject = scanner.Path;
                using (var paint = state.CreateFillPaint())
                {
                    canvas.DrawPath(pathObject, paint);
                }
            }
            base.Scan(state);
        }
    }

    public sealed class PaintFillEvenOddPath : PaintPath
    {
        public PaintFillEvenOddPath() : base(PaintPath.FillEvenOddOperatorKeyword)
        {
        }

        public override void Scan(GraphicsState state)
        {
            var scanner = state.Scanner;
            if (scanner.Canvas is SKCanvas canvas)
            {
                var pathObject = scanner.Path;
                pathObject.FillType = SKPathFillType.EvenOdd;
                using (var paint = state.CreateFillPaint())
                {
                    canvas.DrawPath(pathObject, paint);
                }
            }
            base.Scan(state);
        }
    }

    public sealed class PaintFillStrokePath : PaintPath
    {
        public PaintFillStrokePath() : base(PaintPath.FillStrokeOperatorKeyword)
        {
        }

        public override void Scan(GraphicsState state)
        {
            var scanner = state.Scanner;
            if (scanner.Canvas is SKCanvas canvas)
            {
                var pathObject = scanner.Path;
                using (var paint = state.CreateFillPaint())
                {
                    canvas.DrawPath(pathObject, paint);
                }

                using (var paint = state.CreateStrokePaint())
                {
                    canvas.DrawPath(pathObject, paint);
                }
            }
            base.Scan(state);
        }
    }

    public sealed class PaintFillStrokeEvenOddPath : PaintPath
    {
        public PaintFillStrokeEvenOddPath() : base(PaintPath.FillStrokeEvenOddOperatorKeyword)
        {
        }

        public override void Scan(GraphicsState state)
        {
            var scanner = state.Scanner;
            if (scanner.Canvas is SKCanvas canvas)
            {
                var pathObject = scanner.Path;
                pathObject.FillType = SKPathFillType.EvenOdd;
                using (var paint = state.CreateFillPaint())
                {
                    canvas.DrawPath(pathObject, paint);
                }

                using (var paint = state.CreateStrokePaint())
                {
                    canvas.DrawPath(pathObject, paint);
                }
            }
            base.Scan(state);
        }
    }

    public sealed class PaintStrokePath : PaintPath
    {
        public PaintStrokePath() : base(PaintPath.StrokeOperatorKeyword)
        {
        }

        public override void Scan(GraphicsState state)
        {
            var scanner = state.Scanner;
            if (scanner.Canvas is SKCanvas canvas)
            {
                var pathObject = scanner.Path;
                using (var paint = state.CreateStrokePaint())
                {
                    canvas.DrawPath(pathObject, paint);
                }
            }
            base.Scan(state);
        }
    }
}