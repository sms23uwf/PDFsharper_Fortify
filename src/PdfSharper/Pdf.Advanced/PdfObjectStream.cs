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
using System.Diagnostics;
using System.IO;
using PdfSharper.Pdf.IO;
using System.Linq;
using PdfSharper.Pdf.Internal;

namespace PdfSharper.Pdf.Advanced
{
    /// <summary>
    /// Represents an object stream that contains compressed objects.
    /// PDF 1.5.
    /// </summary>
    public class PdfObjectStream : PdfDictionary
    {
        // Reference: 3.4.6  Object Streams / Page 100

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfObjectStream"/> class.
        /// </summary>
        public PdfObjectStream(PdfDocument document)
            : base(document)
        {
#if DEBUG && CORE
            if (Internal.PdfDiagnostics.TraceObjectStreams)
            {
                Debug.WriteLine("PdfObjectStream(document) created.");
            }
#endif
        }

        /// <summary>
        /// Initializes a new instance from an existing dictionary. Used for object type transformation.
        /// </summary>
        internal PdfObjectStream(PdfDictionary dict)
            : base(dict)
        {
            int n = Elements.GetInteger(Keys.N);
            int first = Elements.GetInteger(Keys.First);
            Stream.TryUnfilter();

            Parser parser = new Parser(null, new MemoryStream(Stream.Value));
            _header = parser.ReadObjectStreamHeader(n, first);

#if DEBUG && CORE
            if (Internal.PdfDiagnostics.TraceObjectStreams)
            {
                Debug.WriteLine(String.Format("PdfObjectStream(document) created. Header item count: {0}", _header.GetLength(0)));
            }
#endif
        }

        /// <summary>
        /// Reads the compressed object with the specified index.
        /// </summary>
        internal void ReadReferences(PdfCrossReferenceTable xrefTable)
        {
            ////// Create parser for stream.
            ////Parser parser = new Parser(_document, new MemoryStream(Stream.Value));
            for (int idx = 0; idx < _header.Length; idx++)
            {
                int objectNumber = _header[idx][0];
                int offset = _header[idx][1];

                PdfObjectID objectID = new PdfObjectID(objectNumber);

                PdfReference iref = new PdfReference(objectID, this.ObjectID, idx);
                if (!xrefTable.Contains(iref.ObjectID))
                {
                    xrefTable.Add(iref);
                }
                else
                {
                    GetType();
                }

                if (!_document._irefTable.Contains(iref.ObjectID))
                {
                    _document._irefTable.Add(iref);
                }

                iref.Document = _document;
            }
        }

        /// <summary>
        /// Reads the compressed object with the specified index.
        /// </summary>
        internal PdfReference ReadCompressedObject(int index, PdfCrossReferenceTable xRefTable)
        {
            Parser parser = new Parser(_document, new MemoryStream(Stream.Value));
            int objectNumber = _header[index][0];
            int offset = _header[index][1];
            return parser.ReadCompressedObject(objectNumber, offset, xRefTable);
        }

        /// <summary>
        /// N pairs of integers.
        /// The first integer represents the object number of the compressed object.
        /// The second integer represents the absolute offset of that object in the decoded stream,
        /// i.e. the byte offset plus First entry.
        /// </summary>
        private readonly int[][] _header;  // Reference: Page 102

        protected override void WriteObject(PdfWriter writer)
        {
            //setup our stream             
            using (MemoryStream msObjects = new MemoryStream())
            using (MemoryStream fullOutput = new MemoryStream())
            {
                PdfWriter objStreamWriter = new PdfWriter(msObjects, _document.SecurityHandler, true);
                for (int i = 0; i < _header.Length; i++)
                {
                    int objectNumber = _header[i][0];
                    //non-offset position 
                    _header[i][1] = objStreamWriter.Position;

                    //TODO: get the object from the correct trailer, it's not always the most recent version that
                    //should be written here
                    _document._irefTable[new PdfObjectID(objectNumber)].Value.Write(objStreamWriter);
                }

                string objectStreamHeader = string.Join(" ", _header.Select(h => $"{h[0]} {h[1]}")) + " ";


                Elements.SetInteger(Keys.First, objectStreamHeader.Length);

                var rawHeader = new RawEncoding().GetBytes(objectStreamHeader);
                fullOutput.Write(rawHeader, 0, rawHeader.Length);

                msObjects.Seek(0, SeekOrigin.Begin);
                msObjects.CopyTo(fullOutput);

                Stream = new PdfStream(fullOutput.ToArray(), this, Stream.Trailer);
                Stream.Zip();
            }
            base.WriteObject(writer);
        }

        /// <summary>
        /// Predefined keys common to all font dictionaries.
        /// </summary>
        public class Keys : PdfStream.Keys
        {
            // Reference: TABLE 3.14  Additional entries specific to an object stream dictionary / Page 101

            /// <summary>
            /// (Required) The type of PDF object that this dictionary describes;
            /// must be ObjStmfor an object stream.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "ObjStm")]
            public const string Type = "/Type";

            /// <summary>
            /// (Required) The number of compressed objects in the stream.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string N = "/N";

            /// <summary>
            /// (Required) The byte offset (in the decoded stream) of the first
            /// compressed object.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string First = "/First";

            /// <summary>
            /// (Optional) A reference to an object stream, of which the current object
            /// stream is considered an extension. Both streams are considered part of
            /// a collection of object streams (see below). A given collection consists
            /// of a set of streams whose Extendslinks form a directed acyclic graph.
            /// </summary>
            [KeyInfo(KeyType.Stream | KeyType.Optional)]
            public const string Extends = "/Extends";
        }
    }

#if DEBUG && CORE
    static class ObjectStreamDiagnostics
    {
        public static void AddObjectStreamXRef()
        { }
    }
#endif
}
