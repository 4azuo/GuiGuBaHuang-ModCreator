using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper class for BitmapImage operations
    /// </summary>
    public static class BitmapHelper
    {
        /// <summary>
        /// Load BitmapImage from file path without locking the file.
        /// Returns null if file doesn't exist or cannot be loaded.
        /// </summary>
        /// <param name="filePath">Absolute path to the image file</param>
        /// <returns>BitmapImage or null</returns>
        public static BitmapImage LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load into memory, release file handle
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze(); // Make it cross-thread accessible and ensure file is released
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Load BitmapImage from file path with custom decode pixel width for thumbnails.
        /// This is useful for displaying large images in smaller controls to save memory.
        /// Returns null if file doesn't exist or cannot be loaded.
        /// </summary>
        /// <param name="filePath">Absolute path to the image file</param>
        /// <param name="decodePixelWidth">Width in pixels to decode (height will be calculated proportionally)</param>
        /// <returns>BitmapImage or null</returns>
        public static BitmapImage LoadFromFileWithSize(string filePath, int decodePixelWidth)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = decodePixelWidth; // Decode to smaller size
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Check if a file is a valid image file by attempting to load it.
        /// </summary>
        /// <param name="filePath">Absolute path to the file</param>
        /// <returns>True if file is a valid image, false otherwise</returns>
        public static bool IsValidImageFile(string filePath)
        {
            return LoadFromFile(filePath) != null;
        }

        /// <summary>
        /// Converts a <see cref="BitmapImage"/> to its equivalent Base64 string representation.
        /// </summary>
        /// <remarks>This method encodes the image using the PNG format. To use a different format, modify
        /// the encoder as needed.</remarks>
        /// <param name="bitmapImage">The <see cref="BitmapImage"/> to convert. Must not be null.</param>
        /// <returns>A Base64-encoded string representing the image data of the provided <see cref="BitmapImage"/>.</returns>
        public static string BitmapImageToBase64(BitmapImage bitmapImage)
        {
            BitmapEncoder encoder = new PngBitmapEncoder(); // or JpegBitmapEncoder
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(ms);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// Converts a Base64-encoded string to a <see cref="BitmapImage"/>.
        /// </summary>
        /// <remarks>The resulting <see cref="BitmapImage"/> is frozen to make it thread-safe and
        /// immutable.</remarks>
        /// <param name="base64">The Base64-encoded string representing image data.</param>
        /// <returns>A <see cref="BitmapImage"/> created from the provided Base64 string.</returns>
        public static BitmapImage Base64ToBitmapImage(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
        }
    }
}
