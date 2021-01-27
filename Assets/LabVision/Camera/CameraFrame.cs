using OpenCVForUnity.CoreModule;
using System;
using JetBrains.Annotations;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;

namespace LabVision
{
    public class CameraFrame : IDisposable
    {
        [NotNull] public readonly Mat Mat;
        [NotNull] public readonly CameraIntrinsic Intrinsic;
        [NotNull] public readonly CameraExtrinsic Extrinsic;
        public readonly int Width;
        public readonly int Height;

        /// <summary>
        /// This counter is incremented when the frame is updated.
        /// </summary>
        public uint FrameCount;

        public ColorFormat Format;

        public CameraFrame([NotNull] Mat mat, [NotNull] CameraIntrinsic intrinsic, [NotNull] CameraExtrinsic extrinsic, int width, int height, uint frameCount, ColorFormat format)
        {
            if (mat == null) throw new ArgumentNullException(nameof(mat));
            Intrinsic = intrinsic ?? throw new ArgumentNullException(nameof(intrinsic));
            Extrinsic = extrinsic ?? throw new ArgumentNullException(nameof(extrinsic));
            Mat = mat;
            Width = width;
            Height = height;
            FrameCount = frameCount;
            Format = format;
        }

        public void Dispose()
        {
            Mat?.Dispose();
        }

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
            if (Format != ColorFormat.Grayscale)
            {
                Mat bgr = new Mat(Height, Width, CvType.CV_8UC3);
                Imgproc.cvtColor(Mat, bgr, Imgproc.COLOR_RGB2BGR); // OpenCV uses BGR, Mat is RGB
                Imgcodecs.imencode(ext, bgr, buffer);
            }
            else
            {
                Imgcodecs.imencode(ext, Mat, buffer);
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
