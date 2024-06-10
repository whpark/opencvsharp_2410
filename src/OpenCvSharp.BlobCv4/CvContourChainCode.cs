﻿// Copyright (C) 2007 by Cristóbal Carnero Liñán
// grendel.ccl@gmail.com
//
// This file is part of cvBlob.
//
// cvBlob is free software: you can redistribute it and/or modify
// it under the terms of the Lesser GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// cvBlob is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// Lesser GNU General Public License for more details.
//
// You should have received a copy of the Lesser GNU General Public License
// along with cvBlob.  If not, see <http://www.gnu.org/licenses/>.

//=========================================
// 2024-06-07 PWH.
//  ported to OpenCvSharp 4
//  original - https://github.com/shimat/opencvsharp_2410
//=========================================

using System;
using System.Collections.Generic;

using CvPoint = OpenCvSharp.Point;
using CvSize = OpenCvSharp.Size;
using CvRect = OpenCvSharp.Rect;
using CvScalar = OpenCvSharp.Scalar;

namespace OpenCvSharp.BlobCv4
{
    /// <summary>
    /// 
    /// </summary>
    public class CvContourChainCode : ICloneable
    {
        /// <summary>
        /// Point where contour begin.
        /// </summary>
        public CvPoint StartingPoint { get; set; }

        /// <summary>
        /// Polygon description based on chain codes.
        /// </summary>
        public List<CvChainCode> ChainCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public CvContourChainCode()
        {
            StartingPoint = default(CvPoint);
            ChainCode = new List<CvChainCode>();
        }

        #region ConvertToPolygon

        /// <summary>
        /// Convert a chain code contour to a polygon.
        /// </summary>
        /// <returns>A polygon.</returns>
        public CvContourPolygon ConvertToPolygon()
        {
            CvContourPolygon contour = new CvContourPolygon();

            int x = StartingPoint.X;
            int y = StartingPoint.Y;
            contour.Add(new CvPoint(x, y));

            if (ChainCode.Count > 0)
            {
                CvChainCode lastCode = ChainCode[0];
                x += CvBlobConst.ChainCodeMoves[(int)ChainCode[0]][0];
                y += CvBlobConst.ChainCodeMoves[(int)ChainCode[0]][1];
                for (int i = 1; i < ChainCode.Count; i++)
                {
                    if (lastCode != ChainCode[i])
                    {
                        contour.Add(new CvPoint(x, y));
                        lastCode = ChainCode[i];
                    }
                    x += CvBlobConst.ChainCodeMoves[(int)ChainCode[i]][0];
                    y += CvBlobConst.ChainCodeMoves[(int)ChainCode[i]][1];
                }
            }

            return contour;
        }

        #endregion
        #region Perimeter
        /// <summary>
        /// Calculates perimeter of a polygonal contour.
        /// </summary>
        /// <returns>Perimeter of the contour.</returns>
        public double Perimeter()
        {
            double perimeter = 0.0;
            foreach (CvChainCode cc in ChainCode)
            {
                int type = (int)cc;
                if (type % 2 != 0)
                    perimeter += Math.Sqrt(1.0 + 1.0);
                else
                    perimeter += 1.0;
            }
            return perimeter;
        }
        #endregion
        #region Render
        /// <summary>
        /// Draw a contour.
        /// </summary>
        /// <param name="img">Image to draw on.</param>
        public void Render(Mat img)
        {
            Render(img, new CvScalar(255, 255, 255));
        }
        /// <summary>
        /// Draw a contour.
        /// </summary>
        /// <param name="img">Image to draw on.</param>
        /// <param name="color">Color to draw (default, white).</param>
        public void Render(Mat img, CvScalar color)
        {
            if (img == null)
                throw new ArgumentNullException(nameof(img));
            if (img.Depth() != MatType.CV_8U || img.Channels() != 3)
                throw new ArgumentException("Invalid img format (U8 3-channels)");

            int step = (int)img.Step();
            CvRect roi = new CvRect(0, 0, img.Cols, img.Rows);
            int width = roi.Width;
            int height = roi.Height;
            int offset = (3 * roi.X) + (roi.Y * step);

            unsafe
            {
                byte *imgData = img.DataPointer + offset;
                int x = StartingPoint.X;
                int y = StartingPoint.Y;
                foreach (CvChainCode cc in ChainCode)
	            {
	                imgData[3*x + step*y + 0] = (byte)(color.Val0);
	                imgData[3*x + step*y + 1] = (byte)(color.Val1); 
	                imgData[3*x + step*y + 2] = (byte)(color.Val2); 
	                x += CvBlobConst.ChainCodeMoves[(int)cc][0];
	                y += CvBlobConst.ChainCodeMoves[(int)cc][1];
                }
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public CvContourChainCode Clone()
        {
            return new CvContourChainCode
            {
                ChainCode = new List<CvChainCode>(ChainCode),
                StartingPoint = StartingPoint,
            };
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
