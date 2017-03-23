﻿#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Stefan Lange
//
// Copyright (c) 2005-2016 empira Software GmbH, Cologne Area (Germany)
//
// http://www.PdfSharper.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;

namespace PdfSharper.Drawing
{
    /// <summary>
    /// Specifies style information applied to text.
    /// </summary>
    [Flags]
    public enum XFontStyle  // Same values as System.Drawing.FontStyle.
    {
        /// <summary>
        /// Normal text.
        /// </summary>
        Regular = XGdiFontStyle.Regular,

        /// <summary>
        /// Bold text.
        /// </summary>
        Bold = XGdiFontStyle.Bold,

        /// <summary>
        /// Italic text.
        /// </summary>
        Italic = XGdiFontStyle.Italic,

        /// <summary>
        /// Bold and italic text. 
        /// </summary>
        BoldItalic = XGdiFontStyle.BoldItalic,

        /// <summary>
        /// Underlined text.
        /// </summary>
        Underline = XGdiFontStyle.Underline,

        /// <summary>
        /// Text with a line through the middle.
        /// </summary>
        Strikeout = XGdiFontStyle.Strikeout,

        // Additional flags:
        // BoldSimulation
        // ItalicSimulation // It is not ObliqueSimulation, because oblique is what is what you get and this simulates italic.
    }
}
