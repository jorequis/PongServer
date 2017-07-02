using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PongServer
{
    class Utils
    {
        public static Vector2 RotateVector2d(Vector2 vec, float degrees)
        {
            Vector2 result = new Vector2();
            float radians = Deg2Rad * degrees;

            result.x = (float)(vec.x * Math.Cos(radians) - vec.y * Math.Sin(radians));
            result.y = (float)(vec.x * Math.Sin(radians) + vec.y * Math.Cos(radians));

            return result;
        }

        public const float Deg2Rad = 0.0174532924F;
    }
}