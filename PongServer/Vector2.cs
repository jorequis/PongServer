namespace PongServer
{
    public struct Vector2
    {
        public Vector2(float x, float y) { this.x = x; this.y = y; }

        public float x;
        public float y;
        
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            a.x += b.x;
            a.y += b.y;
            return a;
        }

        public static Vector2 operator *(float d, Vector2 a)
        {
            a.x *= d;
            a.y *= d;
            return a;
        }

        public static Vector2 operator *(Vector2 a, float d)
        {
            a.x *= d;
            a.y *= d;
            return a;
        }
    }
}