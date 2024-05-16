/*
  Copyright 2006-2015 Stefano Chizzolini. http://www.pdfclown.org

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
using tokens = PdfClown.Tokens;

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq;

namespace PdfClown.Objects
{
    /**
      <summary>PDF name object [PDF:1.6:3.2.4].</summary>
    */
    public sealed class PdfName : PdfSimpleObject<string>, IPdfString, IEquatable<PdfName>
    {
        /*
          NOTE: As name objects are simple symbols uniquely defined by sequences of characters,
          the bytes making up the name are never treated as text, always keeping them escaped.
        */
        /*
          NOTE: Name lexical conventions prescribe that the following reserved characters
          are to be escaped when placed inside names' character sequences:
            - delimiters;
            - whitespaces;
            - '#' (number sign character).
        */
        private static readonly Regex EscapedPattern = new("#([\\da-fA-F]{2})");
        private static readonly Regex UnescapedPattern = new("[\\s\\(\\)<>\\[\\]{}/%#]");

        private static readonly ConcurrentDictionary<string, PdfName> names = new(StringComparer.Ordinal);

#pragma warning disable 0108
        public static readonly PdfName A = new("A");
        public static readonly PdfName a = new("a");
        public static readonly PdfName A85 = new("A85");
        public static readonly PdfName AA = new("AA");
        public static readonly PdfName AC = new("AC");
        public static readonly PdfName Accepted = new("Accepted");
        public static readonly PdfName Action = new("Action");
        public static readonly PdfName AcroForm = new("AcroForm");
        public static readonly PdfName AdobePPKLite = new("Adobe.PPKLite");
        public static readonly PdfName AESV2 = new("AESV2");
        public static readonly PdfName AESV3 = new("AESV3");
        public static readonly PdfName AHx = new("AHx");
        public static readonly PdfName AIS = new("AIS");
        public static readonly PdfName All = new("All");
        public static readonly PdfName AllOff = new("AllOff");
        public static readonly PdfName AllOn = new("AllOn");
        public static readonly PdfName AllPages = new("AllPages");
        public static readonly PdfName Alternate = new("Alternate");
        public static readonly PdfName AN = new("AN");
        public static readonly PdfName And = new("And");
        public static readonly PdfName Annot = new("Annot");
        public static readonly PdfName Annotation = new("Annotation");
        public static readonly PdfName Annots = new("Annots");
        public static readonly PdfName AnyOff = new("AnyOff");
        public static readonly PdfName AnyOn = new("AnyOn");
        public static readonly PdfName AntiAlias = new("AntiAlias");
        public static readonly PdfName AP = new("AP");
        public static readonly PdfName App = new("App");
        public static readonly PdfName AppDefault = new("AppDefault");
        public static readonly PdfName Approved = new("Approved");
        public static readonly PdfName ArtBox = new("ArtBox");
        public static readonly PdfName AS = new("AS");
        public static readonly PdfName Ascent = new("Ascent");
        public static readonly PdfName ASCII85Decode = new("ASCII85Decode");
        public static readonly PdfName ASCIIHexDecode = new("ASCIIHexDecode");
        public static readonly PdfName AsIs = new("AsIs");
        public static readonly PdfName Author = new("Author");
        public static readonly PdfName AvgWidth = new("AvgWidth");
        public static readonly PdfName B = new("B");
        public static readonly PdfName Background = new("Background");
        public static readonly PdfName BaseEncoding = new("BaseEncoding");
        public static readonly PdfName BaseFont = new("BaseFont");
        public static readonly PdfName BaseState = new("BaseState");
        public static readonly PdfName BBox = new("BBox");
        public static readonly PdfName BC = new("BC");
        public static readonly PdfName BE = new("BE");
        public static readonly PdfName Bead = new("Bead");
        public static readonly PdfName BG = new("BG");
        public static readonly PdfName BG2 = new("BG2");
        public static readonly PdfName BitsPerComponent = new("BitsPerComponent");
        public static readonly PdfName BitsPerCoordinate = new("BitsPerCoordinate");
        public static readonly PdfName BitsPerFlag = new("BitsPerFlag");
        public static readonly PdfName BitsPerSample = new("BitsPerSample");
        public static readonly PdfName Bl = new("Bl");
        public static readonly PdfName BlackPoint = new("BlackPoint");
        public static readonly PdfName BlackIs1 = new("BlackIs1");
        public static readonly PdfName BleedBox = new("BleedBox");
        public static readonly PdfName Blinds = new("Blinds");
        public static readonly PdfName BM = new("BM");
        public static readonly PdfName Border = new("Border");
        public static readonly PdfName Bounds = new("Bounds");
        public static readonly PdfName Box = new("Box");
        public static readonly PdfName BPC = new("BPC");
        public static readonly PdfName BS = new("BS");
        public static readonly PdfName Btn = new("Btn");
        public static readonly PdfName BU = new("BU");
        public static readonly PdfName Butt = new("Butt");
        public static readonly PdfName ByteRange = new("ByteRange");
        public static readonly PdfName C = new("C");
        public static readonly PdfName C0 = new("C0");
        public static readonly PdfName C1 = new("C1");
        public static readonly PdfName CA = new("CA");
        public static readonly PdfName ca = new("ca");
        public static readonly PdfName CalGray = new("CalGray");
        public static readonly PdfName CalRGB = new("CalRGB");
        public static readonly PdfName Cancelled = new("Cancelled");
        public static readonly PdfName Cap = new("Cap");
        public static readonly PdfName CapHeight = new("CapHeight");
        public static readonly PdfName Caret = new("Caret");
        public static readonly PdfName Catalog = new("Catalog");
        public static readonly PdfName Category = new("Category");
        public static readonly PdfName CCF = new("CCF");
        public static readonly PdfName CCITTFaxDecode = new("CCITTFaxDecode");
        public static readonly PdfName CenterWindow = new("CenterWindow");
        public static readonly PdfName Cert = new("Cert");
        public static readonly PdfName CF = new("CF");
        public static readonly PdfName CFM = new("CFM");
        public static readonly PdfName CMYK = new("CMYK");
        public static readonly PdfName Ch = new("Ch");
        public static readonly PdfName CharSet = new("CharSet");
        public static readonly PdfName CIDFontType0 = new("CIDFontType0");
        public static readonly PdfName CIDFontType2 = new("CIDFontType2");
        public static readonly PdfName CIDSet = new("CIDSet");
        public static readonly PdfName CIDSystemInfo = new("CIDSystemInfo");
        public static readonly PdfName CIDToGIDMap = new("CIDToGIDMap");
        public static readonly PdfName Circle = new("Circle");
        public static readonly PdfName CL = new("CL");
        public static readonly PdfName ClosedArrow = new("ClosedArrow");
        public static readonly PdfName CMap = new("CMap");
        public static readonly PdfName CMapName = new("CMapName");
        public static readonly PdfName CMapType = new("CMapType");
        public static readonly PdfName CO = new("CO");
        public static readonly PdfName Color = new("Color");
        public static readonly PdfName ColorBurn = new("ColorBurn");
        public static readonly PdfName ColorDodge = new("ColorDodge");
        public static readonly PdfName Colors = new("Colors");
        public static readonly PdfName ColorSpace = new("ColorSpace");
        public static readonly PdfName ColorTransform = new("ColorTransform");
        public static readonly PdfName Columns = new("Columns");
        public static readonly PdfName Compatible = new("Compatible");
        public static readonly PdfName Completed = new("Completed");
        public static readonly PdfName Comment = new("Comment");
        public static readonly PdfName Confidential = new("Confidential");
        public static readonly PdfName Configs = new("Configs");
        public static readonly PdfName Contents = new("Contents");
        public static readonly PdfName Coords = new("Coords");
        public static readonly PdfName Count = new("Count");
        public static readonly PdfName Cover = new("Cover");
        public static readonly PdfName CP = new("CP");
        public static readonly PdfName Changes = new("Changes");
        public static readonly PdfName CharProcs = new("CharProcs");
        public static readonly PdfName CreationDate = new("CreationDate");
        public static readonly PdfName Creator = new("Creator");
        public static readonly PdfName CreatorInfo = new("CreatorInfo");
        public static readonly PdfName CropBox = new("CropBox");
        public static readonly PdfName Crypt = new("Crypt");
        public static readonly PdfName CS = new("CS");
        public static readonly PdfName CT = new("CT");
        public static readonly PdfName D = new("D");
        public static readonly PdfName DA = new("DA");
        public static readonly PdfName Date = new("Date");
        public static readonly PdfName Darken = new("Darken");
        public static readonly PdfName DC = new("DC");
        public static readonly PdfName DCT = new("DCT");
        public static readonly PdfName DCTDecode = new("DCTDecode");
        public static readonly PdfName Decode = new("Decode");
        public static readonly PdfName DecodeParms = new("DecodeParms");
        public static readonly PdfName DefaultCryptFilter = new("DefaultCryptFilter");
        public static readonly PdfName Departmental = new("Departmental");
        public static readonly PdfName Desc = new("Desc");
        public static readonly PdfName DescendantFonts = new("DescendantFonts");
        public static readonly PdfName Descent = new("Descent");
        public static readonly PdfName Design = new("Design");
        public static readonly PdfName Dest = new("Dest");
        public static readonly PdfName Dests = new("Dests");
        public static readonly PdfName DeviceCMYK = new("DeviceCMYK");
        public static readonly PdfName DeviceGray = new("DeviceGray");
        public static readonly PdfName DeviceRGB = new("DeviceRGB");
        public static readonly PdfName DeviceN = new("DeviceN");
        public static readonly PdfName Di = new("Di");
        public static readonly PdfName Diamond = new("Diamond");
        public static readonly PdfName Difference = new("Difference");
        public static readonly PdfName Differences = new("Differences");
        public static readonly PdfName DigestMethod = new("DigestMethod");
        public static readonly PdfName Direction = new("Direction");
        public static readonly PdfName DisplayDocTitle = new("DisplayDocTitle");
        public static readonly PdfName Dissolve = new("Dissolve");
        public static readonly PdfName Dm = new("Dm");
        public static readonly PdfName DocMDP = new("DocMDP");
        public static readonly PdfName DocTimeStamp = new("DocTimeStamp");
        public static readonly PdfName Domain = new("Domain");
        public static readonly PdfName DOS = new("DOS");
        public static readonly PdfName DP = new("DP");
        public static readonly PdfName DR = new("DR");
        public static readonly PdfName Draft = new("Draft");
        public static readonly PdfName DS = new("DS");
        public static readonly PdfName DSS = new("DSS");
        public static readonly PdfName Duplex = new("Duplex");
        public static readonly PdfName DuplexFlipLongEdge = new("DuplexFlipLongEdge");
        public static readonly PdfName DuplexFlipShortEdge = new("DuplexFlipShortEdge");
        public static readonly PdfName Dur = new("Dur");
        public static readonly PdfName DV = new("DV");
        public static readonly PdfName DW = new("DW");
        public static readonly PdfName DW2 = new("DW2");
        public static readonly PdfName E = new("E");
        public static readonly PdfName EarlyChange = new("EarlyChange");
        public static readonly PdfName EF = new("EF");
        public static readonly PdfName EmbeddedFile = new("EmbeddedFile");
        public static readonly PdfName EmbeddedFiles = new("EmbeddedFiles");
        public static readonly PdfName Encode = new("Encode");
        public static readonly PdfName EncodedByteAlign = new("EncodedByteAlign");
        public static readonly PdfName Encoding = new("Encoding");
        public static readonly PdfName EndOfBlock = new("EndOfBlock");
        public static readonly PdfName EndOfLine = new("EndOfLine");
        public static readonly PdfName Encrypt = new("Encrypt");
        public static readonly PdfName EncryptMetadata = new("EncryptMetadata");
        public static readonly PdfName Event = new("Event");
        public static readonly PdfName Exclusion = new("Exclusion");
        public static readonly PdfName Experimental = new("Experimental");
        public static readonly PdfName Expired = new("Expired");
        public static readonly PdfName Export = new("Export");
        public static readonly PdfName ExportState = new("ExportState");
        public static readonly PdfName Extend = new("Extend");
        public static readonly PdfName Extends = new("Extends");
        public static readonly PdfName ExtGState = new("ExtGState");
        public static readonly PdfName F = new("F");
        public static readonly PdfName Fade = new("Fade");
        public static readonly PdfName FB = new("FB");
        public static readonly PdfName FD = new("FD");
        public static readonly PdfName FDecodeParms = new("FDecodeParms");
        public static readonly PdfName Ff = new("Ff");
        public static readonly PdfName FFilter = new("FFilter");
        public static readonly PdfName FG = new("FG");
        public static readonly PdfName Fields = new("Fields");
        public static readonly PdfName FileAttachment = new("FileAttachment");
        public static readonly PdfName Filespec = new("Filespec");
        public static readonly PdfName Filter = new("Filter");
        public static readonly PdfName Final = new("Final");
        public static readonly PdfName First = new("First");
        public static readonly PdfName FirstChar = new("FirstChar");
        public static readonly PdfName FirstPage = new("FirstPage");
        public static readonly PdfName Fit = new("Fit");
        public static readonly PdfName FitB = new("FitB");
        public static readonly PdfName FitBH = new("FitBH");
        public static readonly PdfName FitBV = new("FitBV");
        public static readonly PdfName FitH = new("FitH");
        public static readonly PdfName FitR = new("FitR");
        public static readonly PdfName FitV = new("FitV");
        public static readonly PdfName FitWindow = new("FitWindow");
        public static readonly PdfName Fl = new("Fl");
        public static readonly PdfName Flags = new("Flags");
        public static readonly PdfName FlateDecode = new("FlateDecode");
        public static readonly PdfName Fly = new("Fly");
        public static readonly PdfName Fo = new("Fo");
        public static readonly PdfName Font = new("Font");
        public static readonly PdfName FontBBox = new("FontBBox");
        public static readonly PdfName FontDescriptor = new("FontDescriptor");
        public static readonly PdfName FontFile = new("FontFile");
        public static readonly PdfName FontFile2 = new("FontFile2");
        public static readonly PdfName FontFile3 = new("FontFile3");
        public static readonly PdfName FontMatrix = new("FontMatrix");
        public static readonly PdfName FontName = new("FontName");
        public static readonly PdfName FontFamily = new("FontFamily");
        public static readonly PdfName FontStretch = new("FontStretch");
        public static readonly PdfName ForComment = new("ForComment");
        public static readonly PdfName Form = new("Form");
        public static readonly PdfName ForPublicRelease = new("ForPublicRelease");
        public static readonly PdfName FreeText = new("FreeText");
        public static readonly PdfName FS = new("FS");
        public static readonly PdfName FT = new("FT");
        public static readonly PdfName FreeTextCallout = new("FreeTextCallout");
        public static readonly PdfName FreeTextTypeWriter = new("FreeTextTypeWriter");
        public static readonly PdfName FullScreen = new("FullScreen");
        public static readonly PdfName Function = new("Function");
        public static readonly PdfName Functions = new("Functions");
        public static readonly PdfName FunctionType = new("FunctionType");
        public static readonly PdfName FWParams = new("FWParams");
        public static readonly PdfName G = new("G");
        public static readonly PdfName Gamma = new("Gamma");
        public static readonly PdfName Glitter = new("Glitter");
        public static readonly PdfName GoTo = new("GoTo");
        public static readonly PdfName GoTo3DView = new("GoTo3DView");
        public static readonly PdfName GoToAction = new("GoToAction");
        public static readonly PdfName GoToE = new("GoToE");
        public static readonly PdfName GoToR = new("GoToR");
        public static readonly PdfName Graph = new("Graph");
        public static readonly PdfName Group = new("Group");
        public static readonly PdfName H = new("H");
        public static readonly PdfName HardLight = new("HardLight");
        public static readonly PdfName Height = new("Height");
        public static readonly PdfName Help = new("Help");
        public static readonly PdfName HF = new("HF");
        public static readonly PdfName HI = new("HI");
        public static readonly PdfName Hide = new("Hide");
        public static readonly PdfName HideMenubar = new("HideMenubar");
        public static readonly PdfName HideToolbar = new("HideToolbar");
        public static readonly PdfName HideWindowUI = new("HideWindowUI");
        public static readonly PdfName Highlight = new("Highlight");
        public static readonly PdfName highlight = new("highlight");
        public static readonly PdfName Hue = new("Hue");
        public static readonly PdfName I = new("I");
        public static readonly PdfName IC = new("IC");
        public static readonly PdfName ICCBased = new("ICCBased");
        public static readonly PdfName ID = new("ID");
        public static readonly PdfName Identity = new("Identity");
        public static readonly PdfName IdentityH = new("Identity-H");
        public static readonly PdfName IdentityV = new("Identity-V");
        public static readonly PdfName IF = new("IF");
        public static readonly PdfName IM = new("IM");
        public static readonly PdfName Image = new("Image");
        public static readonly PdfName ImageMask = new("ImageMask");
        public static readonly PdfName ImportData = new("ImportData");
        public static readonly PdfName Ind = new("Ind");
        public static readonly PdfName Index = new("Index");
        public static readonly PdfName Indexed = new("Indexed");
        public static readonly PdfName Info = new("Info");
        public static readonly PdfName Inline = new("Inline");
        public static readonly PdfName Ink = new("Ink");
        public static readonly PdfName InkList = new("InkList");
        public static readonly PdfName Insert = new("Insert");
        public static readonly PdfName Interpolate = new("Interpolate");
        public static readonly PdfName Intent = new("Intent");
        public static readonly PdfName IRT = new("IRT");
        public static readonly PdfName IT = new("IT");
        public static readonly PdfName ItalicAngle = new("ItalicAngle");
        public static readonly PdfName IX = new("IX");
        public static readonly PdfName JavaScript = new("JavaScript");
        public static readonly PdfName JBIG2Decode = new("JBIG2Decode");
        public static readonly PdfName JBIG2Globals = new("JBIG2Globals");
        public static readonly PdfName JPXDecode = new("JPXDecode");
        public static readonly PdfName JS = new("JS");
        public static readonly PdfName K = new("K");
        public static readonly PdfName Key = new("Key");
        public static readonly PdfName Keywords = new("Keywords");
        public static readonly PdfName Kids = new("Kids");
        public static readonly PdfName L = new("L");
        public static readonly PdfName L2R = new("L2R");
        public static readonly PdfName Lab = new("Lab");
        public static readonly PdfName Lang = new("Lang");
        public static readonly PdfName Language = new("Language");
        public static readonly PdfName Last = new("Last");
        public static readonly PdfName LastChar = new("LastChar");
        public static readonly PdfName LastModified = new("LastModified");
        public static readonly PdfName LastPage = new("LastPage");
        public static readonly PdfName Launch = new("Launch");
        public static readonly PdfName LC = new("LC");
        public static readonly PdfName LE = new("LE");
        public static readonly PdfName Leading = new("Leading");
        public static readonly PdfName Length = new("Length");
        public static readonly PdfName Length1 = new("Length1");
        public static readonly PdfName Length2 = new("Length2");
        public static readonly PdfName Length3 = new("Length3");
        public static readonly PdfName LI = new("LI");
        public static readonly PdfName Lighten = new("Lighten");
        public static readonly PdfName Limits = new("Limits");
        public static readonly PdfName Line = new("Line");
        public static readonly PdfName LineArrow = new("LineArrow");
        public static readonly PdfName Linearized = new("Linearized");
        public static readonly PdfName LineDimension = new("LineDimension");
        public static readonly PdfName Link = new("Link");
        public static readonly PdfName ListMode = new("ListMode");
        public static readonly PdfName LJ = new("LJ");
        public static readonly PdfName LL = new("LL");
        public static readonly PdfName LLE = new("LLE");
        public static readonly PdfName LLO = new("LLO");
        public static readonly PdfName Location = new("Location");
        public static readonly PdfName Locked = new("Locked");
        public static readonly PdfName Luminosity = new("Luminosity");
        public static readonly PdfName LW = new("LW");
        public static readonly PdfName LZW = new("LZW");
        public static readonly PdfName LZWDecode = new("LZWDecode");
        public static readonly PdfName M = new("M");
        public static readonly PdfName Mac = new("Mac");
        public static readonly PdfName MacRomanEncoding = new("MacRomanEncoding");
        public static readonly PdfName MacExpertEncoding = new("MacExpertEncoding");
        public static readonly PdfName Matrix = new("Matrix");
        public static readonly PdfName Matte = new("Matte");
        public static readonly PdfName max = new("max");
        public static readonly PdfName MaxLen = new("MaxLen");
        public static readonly PdfName MaxWidth = new("MaxWidth");
        public static readonly PdfName MCD = new("MCD");
        public static readonly PdfName MCS = new("MCS");
        public static readonly PdfName MediaBox = new("MediaBox");
        public static readonly PdfName MediaClip = new("MediaClip");
        public static readonly PdfName MediaDuration = new("MediaDuration");
        public static readonly PdfName MediaOffset = new("MediaOffset");
        public static readonly PdfName MediaPlayerInfo = new("MediaPlayerInfo");
        public static readonly PdfName MediaPlayParams = new("MediaPlayParams");
        public static readonly PdfName MediaScreenParams = new("MediaScreenParams");
        public static readonly PdfName Marked = new("Marked");
        public static readonly PdfName Mask = new("Mask");
        public static readonly PdfName Metadata = new("Metadata");
        public static readonly PdfName MH = new("MH");
        public static readonly PdfName Mic = new("Mic");
        public static readonly PdfName min = new("min");
        public static readonly PdfName MissingWidth = new("MissingWidth");
        public static readonly PdfName MK = new("MK");
        public static readonly PdfName ML = new("ML");
        public static readonly PdfName MMType1 = new("MMType1");
        public static readonly PdfName ModDate = new("ModDate");
        public static readonly PdfName Movie = new("Movie");
        public static readonly PdfName MR = new("MR");
        public static readonly PdfName MU = new("MU");
        public static readonly PdfName Multiply = new("Multiply");
        public static readonly PdfName N = new("N");
        public static readonly PdfName Name = new("Name");
        public static readonly PdfName Named = new("Named");
        public static readonly PdfName Names = new("Names");
        public static readonly PdfName NewParagraph = new("NewParagraph");
        public static readonly PdfName NewWindow = new("NewWindow");
        public static readonly PdfName Next = new("Next");
        public static readonly PdfName NextPage = new("NextPage");
        public static readonly PdfName NM = new("NM");
        public static readonly PdfName None = new("None");
        public static readonly PdfName NonEFontNoWarn = new("NonEFontNoWarn");
        public static readonly PdfName NonFullScreenPageMode = new("NonFullScreenPageMode");
        public static readonly PdfName Normal = new("Normal");
        public static readonly PdfName Not = new("Not");
        public static readonly PdfName NotApproved = new("NotApproved");
        public static readonly PdfName Note = new("Note");
        public static readonly PdfName NotForPublicRelease = new("NotForPublicRelease");
        public static readonly PdfName NU = new("NU");
        public static readonly PdfName NumCopies = new("NumCopies");
        public static readonly PdfName Nums = new("Nums");
        public static readonly PdfName O = new("O");
        public static readonly PdfName ObjStm = new("ObjStm");
        public static readonly PdfName OC = new("OC");
        public static readonly PdfName OE = new("OE");
        public static readonly PdfName OCG = new("OCG");
        public static readonly PdfName OCGs = new("OCGs");
        public static readonly PdfName OCMD = new("OCMD");
        public static readonly PdfName OCProperties = new("OCProperties");
        public static readonly PdfName OFF = new("OFF");
        public static readonly PdfName Off = new("Off");
        public static readonly PdfName ON = new("ON");
        public static readonly PdfName OneColumn = new("OneColumn");
        public static readonly PdfName OP = new("OP");
        public static readonly PdfName op = new("op");
        public static readonly PdfName Open = new("Open");
        public static readonly PdfName OpenAction = new("OpenAction");
        public static readonly PdfName OpenArrow = new("OpenArrow");
        public static readonly PdfName OpenType = new("OpenType");
        public static readonly PdfName OPM = new("OPM");
        public static readonly PdfName Opt = new("Opt");
        public static readonly PdfName Or = new("Or");
        public static readonly PdfName Order = new("Order");
        public static readonly PdfName Ordering = new("Ordering");
        public static readonly PdfName Org = new("Org");
        public static readonly PdfName OS = new("OS");
        public static readonly PdfName Outlines = new("Outlines");
        public static readonly PdfName Overlay = new("Overlay");
        public static readonly PdfName P = new("P");
        public static readonly PdfName Panose = new("Panose");
        public static readonly PdfName Page = new("Page");
        public static readonly PdfName PageElement = new("PageElement");
        public static readonly PdfName PageLabel = new("PageLabel");
        public static readonly PdfName PageLabels = new("PageLabels");
        public static readonly PdfName PageLayout = new("PageLayout");
        public static readonly PdfName PageMode = new("PageMode");
        public static readonly PdfName Pages = new("Pages");
        public static readonly PdfName PaintType = new("PaintType");
        public static readonly PdfName Paperclip = new("Paperclip");
        public static readonly PdfName Paragraph = new("Paragraph");
        public static readonly PdfName Params = new("Params");
        public static readonly PdfName Parent = new("Parent");
        public static readonly PdfName Pattern = new("Pattern");
        public static readonly PdfName PatternType = new("PatternType");
        public static readonly PdfName PC = new("PC");
        public static readonly PdfName PDFDocEncoding = new("PdfDocEncoding");
        public static readonly PdfName Perms = new("Perms");
        public static readonly PdfName PI = new("PI");
        public static readonly PdfName PickTrayByPDFSize = new("PickTrayByPDFSize");
        public static readonly PdfName PID = new("PID");
        public static readonly PdfName PieceInfo = new("PieceInfo");
        public static readonly PdfName PL = new("PL");
        public static readonly PdfName PO = new("PO");
        public static readonly PdfName Polygon = new("Polygon");
        public static readonly PdfName PolygonCloud = new("PolygonCloud");
        public static readonly PdfName PolygonDimension = new("PolygonDimension");
        public static readonly PdfName PolyLine = new("PolyLine");
        public static readonly PdfName PolyLineDimension = new("PolyLineDimension");
        public static readonly PdfName Popup = new("Popup");
        public static readonly PdfName Predictor = new("Predictor");
        public static readonly PdfName Preferred = new("Preferred");
        public static readonly PdfName PreRelease = new("PreRelease");
        public static readonly PdfName Prev = new("Prev");
        public static readonly PdfName PrevPage = new("PrevPage");
        public static readonly PdfName Print = new("Print");
        public static readonly PdfName PrintPageRange = new("PrintPageRange");
        public static readonly PdfName PrintScaling = new("PrintScaling");
        public static readonly PdfName PrintState = new("PrintState");
        public static readonly PdfName Private = new("Private");
        public static readonly PdfName Producer = new("Producer");
        public static readonly PdfName Prop_Build = new("Prop_Build");
        public static readonly PdfName Properties = new("Properties");
        public static readonly PdfName PubSec = new("PubSec");
        public static readonly PdfName Push = new("Push");
        public static readonly PdfName PushPin = new("PushPin");
        public static readonly PdfName PV = new("PV");
        public static readonly PdfName Q = new("Q");
        public static readonly PdfName QuadPoints = new("QuadPoints");
        public static readonly PdfName R = new("R");
        public static readonly PdfName r = new("r");
        public static readonly PdfName R2L = new("R2L");
        public static readonly PdfName Range = new("Range");
        public static readonly PdfName Rejected = new("Rejected");
        public static readonly PdfName RBGroups = new("RBGroups");
        public static readonly PdfName RC = new("RC");
        public static readonly PdfName RClosedArrow = new("RClosedArrow");
        public static readonly PdfName RD = new("RD");
        public static readonly PdfName Reason = new("Reason");
        public static readonly PdfName Recipients = new("Recipients");
        public static readonly PdfName Rect = new("Rect");
        public static readonly PdfName Reference = new("Reference");
        public static readonly PdfName Review = new("Review");
        public static readonly PdfName Registry = new("Registry");
        public static readonly PdfName Rendition = new("Rendition");
        public static readonly PdfName Renditions = new("Renditions");
        public static readonly PdfName ResetForm = new("ResetForm");
        public static readonly PdfName Resources = new("Resources");
        public static readonly PdfName REx = new("REx");
        public static readonly PdfName RF = new("RF");
        public static readonly PdfName RGB = new("RGB");
        public static readonly PdfName RI = new("RI");
        public static readonly PdfName RL = new("RL");
        public static readonly PdfName Root = new("Root");
        public static readonly PdfName ROpenArrow = new("ROpenArrow");
        public static readonly PdfName Rotate = new("Rotate");
        public static readonly PdfName Rows = new("Rows");
        public static readonly PdfName RT = new("RT");
        public static readonly PdfName RunLengthDecode = new("RunLengthDecode");
        public static readonly PdfName S = new("S");
        public static readonly PdfName SA = new("SA");
        public static readonly PdfName Saturation = new("Saturation");
        public static readonly PdfName SBApproved = new("SBApproved");
        public static readonly PdfName SBCompleted = new("SBCompleted");
        public static readonly PdfName SBConfidential = new("SBConfidential");
        public static readonly PdfName SBDraft = new("SBDraft");
        public static readonly PdfName SBFinal = new("SBFinal");
        public static readonly PdfName SBForComment = new("SBForComment");
        public static readonly PdfName SBForPublicRelease = new("SBForPublicRelease");
        public static readonly PdfName SBInformationOnly = new("SBInformationOnly");
        public static readonly PdfName SBNotApproved = new("SBNotApproved");
        public static readonly PdfName SBNotForPublicRelease = new("SBNotForPublicRelease");
        public static readonly PdfName SBPreliminaryResults = new("SBPreliminaryResults");
        public static readonly PdfName SBRejected = new("SBRejected");
        public static readonly PdfName SBVoid = new("SBVoid");
        public static readonly PdfName Screen = new("Screen");
        public static readonly PdfName Separation = new("Separation");
        public static readonly PdfName SetOCGState = new("SetOCGState");
        public static readonly PdfName SHAccepted = new("SHAccepted");
        public static readonly PdfName Shading = new("Shading");
        public static readonly PdfName ShadingType = new("ShadingType");
        public static readonly PdfName SHInitialHere = new("SHInitialHere");
        public static readonly PdfName SHSignHere = new("SHSignHere");
        public static readonly PdfName SHWitness = new("SHWitness");
        public static readonly PdfName Sig = new("Sig");
        public static readonly PdfName SigRef = new("SigRef");
        public static readonly PdfName Simplex = new("Simplex");
        public static readonly PdfName SinglePage = new("SinglePage");
        public static readonly PdfName Size = new("Size");
        public static readonly PdfName Slash = new("Slash");
        public static readonly PdfName SMask = new("SMask");
        public static readonly PdfName SoftLight = new("SoftLight");
        public static readonly PdfName Sold = new("Sold");
        public static readonly PdfName Sound = new("Sound");
        public static readonly PdfName SP = new("SP");
        public static readonly PdfName Speaker = new("Speaker");
        public static readonly PdfName Split = new("Split");
        public static readonly PdfName Square = new("Square");
        public static readonly PdfName Squiggly = new("Squiggly");
        public static readonly PdfName SR = new("SR");
        public static readonly PdfName SS = new("SS");
        public static readonly PdfName St = new("St");
        public static readonly PdfName Stamp = new("Stamp");
        public static readonly PdfName StandardEncoding = new("StandardEncoding");
        public static readonly PdfName State = new("State");
        public static readonly PdfName StateModel = new("StateModel");
        public static readonly PdfName StdCF = new("StdCF");
        public static readonly PdfName StemV = new("StemV");
        public static readonly PdfName StemH = new("StemH");
        public static readonly PdfName StrikeOut = new("StrikeOut");
        public static readonly PdfName StructParent = new("StructParent");
        public static readonly PdfName StrF = new("StrF");
        public static readonly PdfName StmF = new("StmF");
        public static readonly PdfName Style = new("Style");
        public static readonly PdfName SubFilter = new("SubFilter");
        public static readonly PdfName Subj = new("Subj");
        public static readonly PdfName Subject = new("Subject");
        public static readonly PdfName SubmitForm = new("SubmitForm");
        public static readonly PdfName Subtype = new("Subtype");
        public static readonly PdfName Supplement = new("Supplement");
        public static readonly PdfName SW = new("SW");
        public static readonly PdfName Sy = new("Sy");
        public static readonly PdfName Symbol = new("Symbol");
        public static readonly PdfName T = new("T");
        public static readonly PdfName Tabs = new("Tabs");
        public static readonly PdfName Tag = new("Tag");
        public static readonly PdfName Text = new("Text");
        public static readonly PdfName TF = new("TF");
        public static readonly PdfName Thread = new("Thread");
        public static readonly PdfName Threads = new("Threads");
        public static readonly PdfName TilingType = new("TilingType");
        public static readonly PdfName Timespan = new("Timespan");
        public static readonly PdfName Title = new("Title");
        public static readonly PdfName TK = new("TK");
        public static readonly PdfName Toggle = new("Toggle");
        public static readonly PdfName Top = new("Top");
        public static readonly PdfName TopSecret = new("TopSecret");
        public static readonly PdfName ToUnicode = new("ToUnicode");
        public static readonly PdfName TP = new("TP");
        public static readonly PdfName TR = new("TR");
        public static readonly PdfName Trans = new("Trans");
        public static readonly PdfName TransformMethod = new("TransformMethod");
        public static readonly PdfName TransformParams = new("TransformParams");
        public static readonly PdfName Transparency = new("Transparency");
        public static readonly PdfName TrimBox = new("TrimBox");
        public static readonly PdfName TrueType = new("TrueType");
        public static readonly PdfName TrustedMode = new("TrustedMode");
        public static readonly PdfName Ttl = new("Ttl");
        public static readonly PdfName TwoColumnLeft = new("TwoColumnLeft");
        public static readonly PdfName TwoColumnRight = new("TwoColumnRight");
        public static readonly PdfName TwoPageLeft = new("TwoPageLeft");
        public static readonly PdfName TwoPageRight = new("TwoPageRight");
        public static readonly PdfName Tx = new("Tx");
        public static readonly PdfName Type = new("Type");
        public static readonly PdfName Type0 = new("Type0");
        public static readonly PdfName Type1 = new("Type1");
        public static readonly PdfName Type1C = new("Type1C");
        public static readonly PdfName Type3 = new("Type3");
        public static readonly PdfName U = new("U");
        public static readonly PdfName UC = new("UC");
        public static readonly PdfName UE = new("UE");
        public static readonly PdfName Unchanged = new("Unchanged");
        public static readonly PdfName Uncover = new("Uncover");
        public static readonly PdfName Underline = new("Underline");
        public static readonly PdfName Unix = new("Unix");
        public static readonly PdfName Unmarked = new("Unix");
        public static readonly PdfName URI = new("URI");
        public static readonly PdfName URL = new("URL");
        public static readonly PdfName Usage = new("Usage");
        public static readonly PdfName UseAttachments = new("UseAttachments");
        public static readonly PdfName UseCMap = new("UseCMap");
        public static readonly PdfName UseNone = new("UseNone");
        public static readonly PdfName UseOC = new("UseOC");
        public static readonly PdfName UseOutlines = new("UseOutlines");
        public static readonly PdfName User = new("User");
        public static readonly PdfName UseThumbs = new("UseThumbs");
        public static readonly PdfName V = new("V");
        public static readonly PdfName VE = new("VE");
        public static readonly PdfName Version = new("Version");
        public static readonly PdfName Vertices = new("Vertices");
        public static readonly PdfName VerticesPerRow = new("VerticesPerRow");
        public static readonly PdfName View = new("View");
        public static readonly PdfName ViewerPreferences = new("ViewerPreferences");
        public static readonly PdfName ViewState = new("ViewState");
        public static readonly PdfName VisiblePages = new("VisiblePages");
        public static readonly PdfName W = new("W");
        public static readonly PdfName W2 = new("W2");
        public static readonly PdfName FontWeight = new("FontWeight");
        public static readonly PdfName WhitePoint = new("WhitePoint");
        public static readonly PdfName Widget = new("Widget");
        public static readonly PdfName Width = new("Width");
        public static readonly PdfName Widths = new("Widths");
        public static readonly PdfName Win = new("Win");
        public static readonly PdfName WinAnsiEncoding = new("WinAnsiEncoding");
        public static readonly PdfName Wipe = new("Wipe");
        public static readonly PdfName WMode = new("WMode");
        public static readonly PdfName WP = new("WP");
        public static readonly PdfName WS = new("WS");
        public static readonly PdfName X = new("X");
        public static readonly PdfName XML = new("XML");
        public static readonly PdfName XObject = new("XObject");
        public static readonly PdfName XRef = new("XRef");
        public static readonly PdfName XRefStm = new("XRefStm");
        public static readonly PdfName XHeight = new("XHeight");
        public static readonly PdfName XStep = new("XStep");
        public static readonly PdfName XYZ = new("XYZ");
        public static readonly PdfName Yes = new("Yes");
        public static readonly PdfName YStep = new("YStep");
        public static readonly PdfName Z = new("Z");
        public static readonly PdfName ZapfDingbats = new("ZapfDingbats");
        public static readonly PdfName Zoom = new("Zoom");
#pragma warning restore 0108

        private static readonly byte[] NamePrefixChunk = tokens::Encoding.Pdf.Encode(tokens.Keyword.NamePrefix);

        static PdfName()
        {
            var flags = BindingFlags.Public | BindingFlags.Static;
            var query = typeof(PdfName)
                    .GetFields(flags)
                    .Where(fieldInfo => fieldInfo.FieldType == typeof(PdfName))
                    .Select(fieldInfo => (PdfName)fieldInfo.GetValue(null));
            foreach (var item in query)
            {
                names[item.RawValue] = item;
            }
        }

        /**
          <summary>Gets the object equivalent to the given value.</summary>
        */
        public static PdfName Get(object value, bool escaped = false) => Get(value?.ToString(), escaped);

        /**
          <summary>Gets the object equivalent to the given value.</summary>
        */
        public static PdfName Get(string value, bool escaped = false)
        {
            return value == null ? null : names.GetOrAdd(value, (v) => new PdfName(v, escaped));
        }

        public PdfName(string value)
            : this(value, true)
        { }

        internal PdfName(string value, bool escaped)
        {
            /*
              NOTE: To avoid ambiguities due to the presence of '#' characters,
              it's necessary to explicitly state when a name value has already been escaped.
              This is tipically the case of names parsed from a previously-serialized PDF file.
            */
            if (escaped)
            {
                RawValue = value;
            }
            else
            {
                if (!UnescapedPattern.IsMatch(value))
                    RawValue = value;
                else
                    Value = value;
            }
        }

        public string StringValue => (string)Value;

        public override object Value
        {
            get => base.Value;
            protected set
            {
                /*
                  NOTE: Before being accepted, any character sequence identifying a name MUST be normalized
                  escaping reserved characters.
                */
                var buffer = new StringBuilder();
                {
                    string stringValue = (string)value;
                    int index = 0;
                    Match unescapedMatch = UnescapedPattern.Match(stringValue);
                    while (unescapedMatch.Success)
                    {
                        int start = unescapedMatch.Index;
                        if (start > index)
                        {
                            buffer.Append(stringValue.Substring(index, start - index));
                        }

                        buffer.Append('#' + String.Format("{0:x}", (int)unescapedMatch.Groups[0].Value[0]));

                        index = start + unescapedMatch.Length;
                        unescapedMatch = unescapedMatch.NextMatch();
                    }
                    if (index < stringValue.Length)
                    {
                        buffer.Append(index == 0 ? stringValue : stringValue.Substring(index));
                    }
                }
                RawValue = buffer.ToString();
            }
        }

        public override PdfObject Accept(IVisitor visitor, object data) => visitor.Visit(this, data);

        public override int CompareTo(PdfDirectObject obj)
        {
            if (obj is not PdfName objName)
                throw new ArgumentException("Object MUST be a PdfName");

            return string.Compare(RawValue, objName.RawValue, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (obj is PdfName objName)
            {
                return Equals(objName);
            }
            else if (obj is IPdfString pdfString)
            {
                return Equals(pdfString);
            }
            else if (obj is string objString)
            {
                return Equals(objString);
            }

            return base.Equals(obj);
        }

        public bool Equals(IPdfString pdfString)
        {
            return string.Equals(RawValue, pdfString.StringValue, StringComparison.Ordinal);
        }

        public bool Equals(string objString)
        {
            return string.Equals(RawValue, objString, StringComparison.Ordinal);
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            /*
              NOTE: The textual representation of a name concerns unescaping reserved characters.
            */
            string value = RawValue;
            var buffer = new StringBuilder();
            int index = 0;
            Match escapedMatch = EscapedPattern.Match(value);
            while (escapedMatch.Success)
            {
                int start = escapedMatch.Index;
                if (start > index)
                {
                    buffer.Append(value.Substring(index, start - index));
                }

                buffer.Append((char)Int32.Parse(escapedMatch.Groups[1].Value, NumberStyles.HexNumber));

                index = start + escapedMatch.Length;
                escapedMatch = escapedMatch.NextMatch();
            }
            if (index < value.Length)
            {
                buffer.Append(value.Substring(index));
            }

            return buffer.ToString();
        }

        public override void WriteTo(IOutputStream stream, PdfFile context)
        {
            stream.Write(NamePrefixChunk);
            stream.Write(RawValue);
        }

        public bool Equals(PdfName other)
        {
            return string.Equals(RawValue, other?.RawValue, StringComparison.Ordinal);
        }
    }
}