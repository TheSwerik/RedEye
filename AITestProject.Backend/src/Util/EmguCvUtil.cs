using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.Util;
using Emgu.Util.TypeEnum;

namespace AITestProject.Util
{
    /// <summary>
    ///     Provide extension method to convert IInputArray to and from Bitmap
    /// </summary>
    public static class EmguCvUtil
    {
        /// <summary>
        ///     Convert raw data to bitmap
        /// </summary>
        /// <param name="scan0">The pointer to the raw data</param>
        /// <param name="step">The step</param>
        /// <param name="size">The size of the image</param>
        /// <param name="srcColorType">The source image color type</param>
        /// <param name="numberOfChannels">The number of channels</param>
        /// <param name="srcDepthType">The source image depth type</param>
        /// <param name="tryDataSharing">Try to create Bitmap that shares the data with the image</param>
        /// <returns>The Bitmap</returns>
        public static Bitmap RawDataToBitmap(IntPtr scan0, int step, Size size, Type srcColorType, int numberOfChannels,
            Type srcDepthType, bool tryDataSharing = false)
        {
            if (tryDataSharing)
            {
                if (srcColorType == typeof(Gray) && srcDepthType == typeof(byte))
                {
                    //Grayscale of Bytes
                    var bmpGray = new Bitmap(
                        size.Width,
                        size.Height,
                        step,
                        PixelFormat.Format8bppIndexed,
                        scan0
                    );

                    bmpGray.Palette = GrayscalePalette;

                    return bmpGray;
                }
                // Mono in Linux doesn't support scan0 constructor with Format24bppRgb, use ToBitmap instead
                // See https://bugzilla.novell.com/show_bug.cgi?id=363431

                if (
                        Platform.OperationSystem == OS.Windows &&
                        Platform.ClrType == ClrType.DotNet &&
                        srcColorType == typeof(Bgr) && srcDepthType == typeof(byte)
                        && (step & 3) == 0)
                    //Bgr byte    
                    return new Bitmap(
                        size.Width,
                        size.Height,
                        step,
                        PixelFormat.Format24bppRgb,
                        scan0);
                if (srcColorType == typeof(Bgra) && srcDepthType == typeof(byte))
                    //Bgra byte
                    return new Bitmap(
                        size.Width,
                        size.Height,
                        step,
                        PixelFormat.Format32bppArgb,
                        scan0);

                //PixelFormat.Format16bppGrayScale is not supported in .NET
                //else if (typeof(TColor) == typeof(Gray) && typeof(TDepth) == typeof(UInt16))
                //{
                //   return new Bitmap(
                //      size.width,
                //      size.height,
                //      step,
                //      PixelFormat.Format16bppGrayScale;
                //      scan0);
                //}
            }

            PixelFormat format; //= System.Drawing.Imaging.PixelFormat.Undefined;

            if (srcColorType == typeof(Gray)) // if this is a gray scale image
                format = PixelFormat.Format8bppIndexed;
            else if (srcColorType == typeof(Bgra)) //if this is Bgra image
                format = PixelFormat.Format32bppArgb;
            else if (srcColorType == typeof(Bgr)) //if this is a Bgr Byte image
                format = PixelFormat.Format24bppRgb;
            else
                using (var m = new Mat(size.Height, size.Width, CvInvoke.GetDepthType(srcDepthType), numberOfChannels,
                    scan0, step))
                using (var m2 = new Mat())
                {
                    CvInvoke.CvtColor(m, m2, srcColorType, typeof(Bgr));
                    return RawDataToBitmap(m2.DataPointer, m2.Step, m2.Size, typeof(Bgr), 3, srcDepthType);
                }

            var bmp = new Bitmap(size.Width, size.Height, format);
            var data = bmp.LockBits(
                new Rectangle(Point.Empty, size),
                ImageLockMode.WriteOnly,
                format);
            using (var bmpMat = new Mat(size.Height, size.Width, DepthType.Cv8U, numberOfChannels, data.Scan0,
                data.Stride))
            using (var dataMat = new Mat(size.Height, size.Width, CvInvoke.GetDepthType(srcDepthType), numberOfChannels,
                scan0, step))
            {
                if (srcDepthType == typeof(byte))
                {
                    dataMat.CopyTo(bmpMat);
                }
                else
                {
                    double scale = 1.0, shift = 0.0;
                    var range = dataMat.GetValueRange();
                    if (range.Max > 255.0 || range.Min < 0)
                    {
                        scale = range.Max.Equals(range.Min) ? 0.0 : 255.0 / (range.Max - range.Min);
                        shift = scale.Equals(0) ? range.Min : -range.Min * scale;
                    }

                    CvInvoke.ConvertScaleAbs(dataMat, bmpMat, scale, shift);
                }
            }

            bmp.UnlockBits(data);

            if (format == PixelFormat.Format8bppIndexed)
                bmp.Palette = GrayscalePalette;
            return bmp;
        }

        /// <summary>
        ///     Convert the mat into Bitmap, the pixel values are copied over to the Bitmap
        /// </summary>
        public static Bitmap ToBitmap(this Mat mat)
        {
            if (mat.Dims > 3)
                return null;
            var channels = mat.NumberOfChannels;
            var s = mat.Size;
            Type colorType;
            switch (channels)
            {
                case 1:
                    colorType = typeof(Gray);

                    if (s.Equals(Size.Empty))
                        return null;
                    if ((s.Width | 3) != 0) //handle the special case where width is not a multiple of 4
                    {
                        var bmp = new Bitmap(s.Width, s.Height, PixelFormat.Format8bppIndexed);
                        bmp.Palette = GrayscalePalette;
                        var bitmapData = bmp.LockBits(new Rectangle(Point.Empty, s), ImageLockMode.WriteOnly,
                            PixelFormat.Format8bppIndexed);
                        using (var m = new Mat(s.Height, s.Width, DepthType.Cv8U, 1, bitmapData.Scan0,
                            bitmapData.Stride))
                        {
                            mat.CopyTo(m);
                        }

                        bmp.UnlockBits(bitmapData);
                        return bmp;
                    }

                    break;
                case 3:
                    colorType = typeof(Bgr);
                    break;
                case 4:
                    colorType = typeof(Bgra);
                    break;
                default:
                    throw new Exception("Unknown color type");
            }

            return RawDataToBitmap(mat.DataPointer, mat.Step, s, colorType, mat.NumberOfChannels,
                CvInvoke.GetDepthType(mat.Depth), true);
        }


        /// <summary>
        ///     Convert the umat into Bitmap, the pixel values are copied over to the Bitmap
        /// </summary>
        public static Bitmap ToBitmap(this UMat umat)
        {
            using (var tmp = umat.GetMat(AccessType.Read))
            {
                return tmp.ToBitmap();
            }
        }

        /// <summary>
        ///     Convert the gpuMat into Bitmap, the pixel values are copied over to the Bitmap
        /// </summary>
        public static Bitmap ToBitmap(this GpuMat gpuMat)
        {
            using (var tmp = new Mat())
            {
                gpuMat.Download(tmp);
                return tmp.ToBitmap();
            }
        }

        /// <summary>
        ///     Create an Image &lt; TColor, TDepth &gt; from Bitmap
        /// </summary>
        public static Image<TColor, TDepth> ToImage<TColor, TDepth>(this Bitmap bitmap) where
            TColor : struct, IColor
            where TDepth : new()
        {
            var size = bitmap.Size;
            var image = new Image<TColor, TDepth>(size);

            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format32bppRgb:
                    if (typeof(TColor) == typeof(Bgr) && typeof(TDepth) == typeof(byte))
                    {
                        var data = bitmap.LockBits(
                            new Rectangle(Point.Empty, size),
                            ImageLockMode.ReadOnly,
                            bitmap.PixelFormat);

                        using (var mat =
                            new Image<Bgra, byte>(size.Width, size.Height, data.Stride, data.Scan0))
                        {
                            CvInvoke.MixChannels(mat, image, new[] {0, 0, 1, 1, 2, 2});
                        }

                        bitmap.UnlockBits(data);
                    }
                    else
                    {
                        using (var tmp = bitmap.ToImage<Bgr, byte>())
                        {
                            image.ConvertFrom((IInputArray) tmp);
                        }
                    }

                    break;
                case PixelFormat.Format32bppArgb:
                    if (typeof(TColor) == typeof(Bgra) && typeof(TDepth) == typeof(byte))
                    {
                        image.CopyFromBitmap(bitmap);
                    }
                    else
                    {
                        var data = bitmap.LockBits(
                            new Rectangle(Point.Empty, size),
                            ImageLockMode.ReadOnly,
                            bitmap.PixelFormat);
                        using (var tmp =
                            new Image<Bgra, byte>(size.Width, size.Height, data.Stride, data.Scan0))
                        {
                            image.ConvertFrom(tmp);
                        }

                        bitmap.UnlockBits(data);
                    }

                    break;
                case PixelFormat.Format8bppIndexed:
                    if (typeof(TColor) == typeof(Bgra) && typeof(TDepth) == typeof(byte))
                    {
                        Matrix<byte> bTable, gTable, rTable, aTable;
                        ColorPaletteToLookupTable(bitmap.Palette, out bTable, out gTable, out rTable, out aTable);
                        var data = bitmap.LockBits(
                            new Rectangle(Point.Empty, size),
                            ImageLockMode.ReadOnly,
                            bitmap.PixelFormat);
                        using (var indexValue =
                            new Image<Gray, byte>(size.Width, size.Height, data.Stride, data.Scan0))
                        {
                            using (var b = new Mat())
                            using (var g = new Mat())
                            using (var r = new Mat())
                            using (var a = new Mat())
                            {
                                CvInvoke.LUT(indexValue, bTable, b);
                                CvInvoke.LUT(indexValue, gTable, g);
                                CvInvoke.LUT(indexValue, rTable, r);
                                CvInvoke.LUT(indexValue, aTable, a);
                                using (var mv = new VectorOfMat(b, g, r, a))
                                {
                                    CvInvoke.Merge(mv, image);
                                }
                            }
                        }

                        bitmap.UnlockBits(data);
                        bTable.Dispose();
                        gTable.Dispose();
                        rTable.Dispose();
                        aTable.Dispose();
                    }
                    else
                    {
                        using (var tmp = bitmap.ToImage<Bgra, byte>())
                        {
                            image.ConvertFrom(tmp);
                        }
                    }

                    break;
                case PixelFormat.Format24bppRgb:
                    if (typeof(TColor) == typeof(Bgr) && typeof(TDepth) == typeof(byte))
                    {
                        image.CopyFromBitmap(bitmap);
                    }
                    else
                    {
                        var data = bitmap.LockBits(
                            new Rectangle(Point.Empty, size),
                            ImageLockMode.ReadOnly,
                            bitmap.PixelFormat);
                        using (var tmp =
                            new Image<Bgr, byte>(size.Width, size.Height, data.Stride, data.Scan0))
                        {
                            image.ConvertFrom(tmp);
                        }

                        bitmap.UnlockBits(data);
                    }

                    break;
                case PixelFormat.Format1bppIndexed:
                    if (typeof(TColor) == typeof(Gray) && typeof(TDepth) == typeof(byte))
                    {
                        var rows = size.Height;
                        var cols = size.Width;
                        var data = bitmap.LockBits(
                            new Rectangle(Point.Empty, size),
                            ImageLockMode.ReadOnly,
                            bitmap.PixelFormat);

                        var fullByteCount = cols >> 3;
                        var partialBitCount = cols & 7;

                        var mask = 1 << 7;

                        var srcAddress = data.Scan0.ToInt64();
                        var imagedata = image.Data as byte[,,];

                        var row = new byte[fullByteCount + (partialBitCount == 0 ? 0 : 1)];

                        var v = 0;
                        for (var i = 0; i < rows; i++, srcAddress += data.Stride)
                        {
                            Marshal.Copy((IntPtr) srcAddress, row, 0, row.Length);

                            for (var j = 0; j < cols; j++, v <<= 1)
                            {
                                if ((j & 7) == 0)
                                    //fetch the next byte 
                                    v = row[j >> 3];

                                imagedata[i, j, 0] = (v & mask) == 0 ? (byte) 0 : (byte) 255;
                            }
                        }
                    }
                    else
                    {
                        using (var tmp = bitmap.ToImage<Gray, byte>())
                        {
                            image.ConvertFrom(tmp);
                        }
                    }

                    break;
                default:

                    #region Handle other image type

                    //         Bitmap bgraImage = new Bitmap(value.Width, value.Height, PixelFormat.Format32bppArgb);
                    //         using (Graphics g = Graphics.FromImage(bgraImage))
                    //         {
                    //            g.DrawImageUnscaled(value, 0, 0, value.Width, value.Height);
                    //         }
                    //         Bitmap = bgraImage;
                    using (var tmp1 = new Image<Bgra, byte>(size))
                    {
                        var data = tmp1.Data;
                        for (var i = 0; i < size.Width; i++)
                        for (var j = 0; j < size.Height; j++)
                        {
                            var color = bitmap.GetPixel(i, j);
                            data[j, i, 0] = color.B;
                            data[j, i, 1] = color.G;
                            data[j, i, 2] = color.R;
                            data[j, i, 3] = color.A;
                        }

                        image.ConvertFrom(tmp1);
                    }

                    #endregion

                    break;
            }

            return image;
        }


        /// <summary>
        ///     Utility function for converting Bitmap to Image
        /// </summary>
        /// <param name="bmp">the bitmap to copy data from</param>
        /// <param name="image">The image to copy data to</param>
        private static void CopyFromBitmap<TColor, TDepth>(this Image<TColor, TDepth> image, Bitmap bmp) where
            TColor : struct, IColor
            where TDepth : new()
        {
            var data = bmp.LockBits(
                new Rectangle(Point.Empty, bmp.Size),
                ImageLockMode.ReadOnly,
                bmp.PixelFormat);

            using (var mat =
                new Matrix<TDepth>(bmp.Height, bmp.Width, image.NumberOfChannels, data.Scan0, data.Stride))
            {
                CvInvoke.cvCopy(mat.Ptr, image.Ptr, IntPtr.Zero);
            }

            bmp.UnlockBits(data);
        }

        /// <summary>
        ///     Provide a more efficient way to convert Image&lt;Gray, Byte&gt;, Image&lt;Bgr, Byte&gt; and Image&lt;Bgra, Byte&gt;
        ///     into Bitmap
        ///     such that the image data is <b>shared</b> with Bitmap.
        ///     If you change the pixel value on the Bitmap, you change the pixel values on the Image object as well!
        ///     For other types of image this property has the same effect as ToBitmap()
        ///     <b>Take extra caution not to use the Bitmap after the Image object is disposed</b>
        /// </summary>
        /// <typeparam name="TColor">The color of the image</typeparam>
        /// <typeparam name="TDepth">The depth of the image</typeparam>
        /// <param name="image">The image to create Bitmap from</param>
        /// <returns>
        ///     A bitmap representation of the image. In the cases of Image&lt;Gray, Byte&gt;, Image&lt;Bgr, Byte&gt; and
        ///     Image&lt;Bgra, Byte&gt;, the image data is shared between the Bitmap and the Image object.
        /// </returns>
        public static Bitmap AsBitmap<TColor, TDepth>(this Image<TColor, TDepth> image) where
            TColor : struct, IColor
            where TDepth : new()
        {
            IntPtr scan0;
            int step;
            Size size;
            CvInvoke.cvGetRawData(image.Ptr, out scan0, out step, out size);

            return RawDataToBitmap(scan0, step, size, typeof(TColor), image.NumberOfChannels, typeof(TDepth), true);
        }

        /// <summary>
        ///     Convert this image into Bitmap, the pixel values are copied over to the Bitmap
        /// </summary>
        /// <remarks>
        ///     For better performance on Image&lt;Gray, Byte&gt; and Image&lt;Bgr, Byte&gt;, consider using the Bitmap
        ///     property
        /// </remarks>
        /// <returns> This image in Bitmap format, the pixel data are copied over to the Bitmap</returns>
        public static Bitmap ToBitmap<TColor, TDepth>(this Image<TColor, TDepth> image) where
            TColor : struct, IColor
            where TDepth : new()
        {
            var typeOfColor = typeof(TColor);
            var typeofDepth = typeof(TDepth);

            var format = PixelFormat.Undefined;

            if (typeOfColor == typeof(Gray)) // if this is a gray scale image
                format = PixelFormat.Format8bppIndexed;
            else if (typeOfColor == typeof(Bgra)) //if this is Bgra image
                format = PixelFormat.Format32bppArgb;
            else if (typeOfColor == typeof(Bgr)) //if this is a Bgr Byte image
                format = PixelFormat.Format24bppRgb;
            else
                using (var temp = image.Convert<Bgr, byte>())
                {
                    return ToBitmap(temp);
                }

            if (typeof(TDepth) == typeof(byte))
            {
                var size = image.Size;
                var bmp = new Bitmap(size.Width, size.Height, format);
                var data = bmp.LockBits(
                    new Rectangle(Point.Empty, size),
                    ImageLockMode.WriteOnly,
                    format);
                //using (Matrix<Byte> m = new Matrix<byte>(size.Height, size.Width, data.Scan0, data.Stride))
                using (var mat = new Mat(size.Height, size.Width, DepthType.Cv8U, image.NumberOfChannels,
                    data.Scan0, data.Stride))
                {
                    image.Mat.CopyTo(mat);
                }

                bmp.UnlockBits(data);

                if (format == PixelFormat.Format8bppIndexed)
                    bmp.Palette = GrayscalePalette;
                return bmp;
            }

            using (var temp = image.Convert<TColor, byte>())
            {
                return temp.ToBitmap();
            }
        }

        /// <summary> Create a Bitmap image of certain size</summary>
        /// <param name="image">The image to be converted to Bitmap</param>
        /// <param name="width">The width of the bitmap</param>
        /// <param name="height"> The height of the bitmap</param>
        /// <returns> This image in Bitmap format of the specific size</returns>
        public static Bitmap ToBitmap<TColor, TDepth>(this Image<TColor, TDepth> image, int width, int height) where
            TColor : struct, IColor
            where TDepth : new()
        {
            using (var scaledImage = image.Resize(width, height, Inter.Linear))
            {
                return scaledImage.ToBitmap();
            }
        }


        /// <summary>
        ///     Convert the CudaImage to its equivalent Bitmap representation
        /// </summary>
        public static Bitmap ToBitmap<TColor, TDepth>(this CudaImage<TColor, TDepth> cudaImage) where
            TColor : struct, IColor
            where TDepth : new()
        {
            if (typeof(TColor) == typeof(Bgr) && typeof(TDepth) == typeof(byte))
            {
                var s = cudaImage.Size;
                var result = new Bitmap(s.Width, s.Height, PixelFormat.Format24bppRgb);
                var data = result.LockBits(new Rectangle(Point.Empty, result.Size),
                    ImageLockMode.WriteOnly, result.PixelFormat);
                using (var tmp = new Image<TColor, TDepth>(s.Width, s.Height, data.Stride, data.Scan0)
                )
                {
                    cudaImage.Download(tmp);
                }

                result.UnlockBits(data);
                return result;
            }

            using (var tmp = cudaImage.ToImage())
            {
                return tmp.ToBitmap();
            }
        }

        #region Color Palette

        /// <summary>
        ///     The ColorPalette of Grayscale for Bitmap Format8bppIndexed
        /// </summary>
        public static readonly ColorPalette GrayscalePalette = GenerateGrayscalePalette();

        private static ColorPalette GenerateGrayscalePalette()
        {
            using (var image = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                var palette = image.Palette;
                for (var i = 0; i < 256; i++) palette.Entries[i] = Color.FromArgb(i, i, i);

                return palette;
            }
        }

        /// <summary>
        ///     Convert the color palette to four lookup tables
        /// </summary>
        /// <param name="palette">The color palette to transform</param>
        /// <param name="bTable">Lookup table for the B channel</param>
        /// <param name="gTable">Lookup table for the G channel</param>
        /// <param name="rTable">Lookup table for the R channel</param>
        /// <param name="aTable">Lookup table for the A channel</param>
        public static void ColorPaletteToLookupTable(ColorPalette palette, out Matrix<byte> bTable,
            out Matrix<byte> gTable, out Matrix<byte> rTable, out Matrix<byte> aTable)
        {
            bTable = new Matrix<byte>(256, 1);
            gTable = new Matrix<byte>(256, 1);
            rTable = new Matrix<byte>(256, 1);
            aTable = new Matrix<byte>(256, 1);
            var bData = bTable.Data;
            var gData = gTable.Data;
            var rData = rTable.Data;
            var aData = aTable.Data;

            var colors = palette.Entries;
            for (var i = 0; i < colors.Length; i++)
            {
                var c = colors[i];
                bData[i, 0] = c.B;
                gData[i, 0] = c.G;
                rData[i, 0] = c.R;
                aData[i, 0] = c.A;
            }
        }

        #endregion
    }
}