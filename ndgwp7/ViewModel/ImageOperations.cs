/* 
    Copyright (C) 2011  Comarch
  
    NDG for WP7 is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either 
    version 2.1 of the License, or (at your option) any later version.
  
    NDG is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.
  
    You should have received a copy of the GNU Lesser General Public
    License along with NDG.  If not, see <http://www.gnu.org/licenses/
*/
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace com.comarch.mobile.ndg.ViewModel
{
    /// <summary>
    /// Class stores methods used during operations of image.
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Converts image form Bitmap to bytes.
        /// </summary>
        /// <param name="bitmapImage">Original image as BitmapImage</param>
        /// <returns>Return array of byte conatain converted image.</returns>
        public byte[] ConvertImageToBytes(BitmapImage bitmapImage)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                WriteableBitmap writeable = new WriteableBitmap(bitmapImage);
                writeable.SaveJpeg(stream, writeable.PixelWidth, writeable.PixelHeight, 0, 100);
                stream.Seek(0, SeekOrigin.Begin);
                return stream.GetBuffer();
            }
        }

        /// <summary>
        /// Resizes image to resolution agreed with resolution set in application settings.
        /// </summary>
        /// <param name="image">Original image as BitmapImage.</param>
        /// <param name="pixelWidth">New image width (in pixels).</param>
        /// <param name="pixelHeight">New image height (in pixels).</param>
        /// <param name="quality">Compression quality.</param>
        /// <param name="keepAspectRatio">Information about keeping original width/height ratio or set width and height from input.</param>
        /// <returns>Return resized image as BitmapImage</returns>
        public BitmapImage ResizeImage(BitmapImage image, int pixelWidth, int pixelHeight, int quality, bool keepAspectRatio)
        //with keepAspectRatio == true orginal image's aspect ratio will be preserved and image will be scaled to fit pixelHeight x pixelWidth block 
        {
            WriteableBitmap writeableBitmap = new WriteableBitmap(image);   
            MemoryStream stream = new MemoryStream();
            int resizedHeight = pixelHeight;
            int resizedWidth = pixelWidth;
            if (keepAspectRatio)
            {
                double imageAspectRatio = (double)image.PixelWidth / (double)image.PixelHeight;
                if (Math.Abs(image.PixelWidth - pixelWidth) < Math.Abs(image.PixelHeight - pixelHeight))
                {
                    resizedHeight = (int)(pixelWidth / imageAspectRatio);
                }
                else
                {
                    resizedWidth = (int)(pixelHeight * imageAspectRatio);
                }
            }
            writeableBitmap.SaveJpeg(stream, resizedWidth, resizedHeight, 0, quality);
            stream.Seek(0, SeekOrigin.Begin);
            BitmapImage output = new BitmapImage();
            output.SetSource(stream);
            return output;
        }
    }
}
