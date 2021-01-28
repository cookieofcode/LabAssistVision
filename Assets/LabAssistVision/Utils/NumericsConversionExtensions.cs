using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// from https://docs.microsoft.com/en-us/windows/mixed-reality/develop/unity/unity-xrdevice-advanced
// Unity uses a left-handed coordinate system, while the Windows Perception APIs use right-handed coordinate systems.
// To convert between these two conventions, you can use this helper.
public static class NumericsConversionExtensions
{
    public static UnityEngine.Vector2 ToUnity(this System.Numerics.Vector2 v) => new UnityEngine.Vector2(v.X, v.Y);
    public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 v) => new UnityEngine.Vector3(v.X, v.Y, -v.Z);
    public static UnityEngine.Quaternion ToUnity(this System.Numerics.Quaternion q) => new UnityEngine.Quaternion(-q.X, -q.Y, q.Z, q.W);
    public static UnityEngine.Matrix4x4 ToUnity(this System.Numerics.Matrix4x4 m) => new UnityEngine.Matrix4x4(
        new UnityEngine.Vector4(m.M11, m.M12, -m.M13, m.M14),
        new UnityEngine.Vector4(m.M21, m.M22, -m.M23, m.M24),
        new UnityEngine.Vector4(-m.M31, -m.M32, m.M33, -m.M34),
        new UnityEngine.Vector4(m.M41, m.M42, -m.M43, m.M44));

    public static System.Numerics.Vector3 ToSystem(this UnityEngine.Vector3 v) => new System.Numerics.Vector3(v.x, v.y, -v.z);
    public static System.Numerics.Quaternion ToSystem(this UnityEngine.Quaternion q) => new System.Numerics.Quaternion(-q.x, -q.y, q.z, q.w);
    public static System.Numerics.Matrix4x4 ToSystem(this UnityEngine.Matrix4x4 m) => new System.Numerics.Matrix4x4(
        m.m00, m.m10, -m.m20, m.m30,
        m.m01, m.m11, -m.m21, m.m31,
       -m.m02, -m.m12, m.m22, -m.m32,
        m.m03, m.m13, -m.m23, m.m33);

}