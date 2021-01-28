using OpenCVForUnity.CoreModule;
using System;
using JetBrains.Annotations;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;

namespace LabAssistVision
{
    public class CameraFrame : IDisposable
    {
        /// <summary>
        /// Contains the image data as OpenCV <seealso cref="Mat"/>.
        /// </summary>
        [NotNull] public readonly Mat Mat;

        /// <summary>
        /// Contains information on camera intrinsic parameters.
        /// </summary>
        [NotNull] public readonly CameraIntrinsic Intrinsic;

        /// <summary>
        /// Contains information on the extrinsic of the camera.
        /// </summary>
        [NotNull] public readonly CameraExtrinsic Extrinsic;

        /// <summary>
        /// The width of the initialized camera profile, padded to 64 bits.
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// The height of the initialized camera profile.
        /// </summary>
        public readonly int Height;

        /// <summary>
        /// This counter is incremented when the frame is updated.
        /// </summary>
        public uint FrameCount;

        /// <summary>
        /// Determines the color format of the camera frame.
        /// </summary>
        public ColorFormat Format;

        public CameraFrame([NotNull] Mat mat, [NotNull] CameraIntrinsic intrinsic, [NotNull] CameraExtrinsic extrinsic, int width, int height, uint frameCount, ColorFormat format)
        {
            if (mat == null) throw new ArgumentNullException(nameof(mat));
            if (intrinsic == null) throw new ArgumentNullException(nameof(intrinsic));
            if (extrinsic == null) throw new ArgumentNullException(nameof(extrinsic));
            Mat = mat;
            Intrinsic = intrinsic;
            Extrinsic = extrinsic;
            Width = width;
            Height = height;
            FrameCount = frameCount;
            Format = format;
        }

        public void Dispose()
        {
            Mat?.Dispose();
        }

        /// <summary>
        /// Encodes the image of the camera frame into a memory buffer using OpenCV.
        /// See the <see href="https://docs.opencv.org/4.5.0/d4/da8/group__imgcodecs.html#ga288b8b3da0892bd651fce07b3bbd3a56">OpenCV documentation</see> for the list of supported file formats.
        /// </summary>
        /// <param name="ext">The extension of the file format supported by OpenCV</param>
        /// <returns>A resized buffer to fit the compressed image</returns>
        [NotNull]
        public byte[] EncodeImage([NotNull] string ext = ".jpg")
        {
            if (ext == null) throw new ArgumentNullException(nameof(ext));
            MatOfByte buffer = new MatOfByte();
            return Encode(ext, buffer);
        }

        [NotNull]
        private byte[] Encode([NotNull] string ext, [NotNull] MatOfByte buffer)
        {
            if (ext == null) throw new ArgumentNullException(nameof(ext));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            switch (Format)
            {
                case ColorFormat.RGB:
                {
                    Mat bgr = new Mat(Height, Width, CvType.CV_8UC3);
                    Imgproc.cvtColor(Mat, bgr, Imgproc.COLOR_RGB2BGR); // OpenCV uses BGR, Mat is RGB
                    Imgcodecs.imencode(ext, bgr, buffer);
                    break;
                }
                case ColorFormat.Grayscale:
                    Imgcodecs.imencode(ext, Mat, buffer);
                    break;
                default:
                    throw new NotSupportedException($"Image encoding for {Format} not supported");
            }
            return buffer.toArray();
        }
    }

    public enum ColorFormat
    {
        Grayscale,
        RGB,
        Unknown
    }
}
