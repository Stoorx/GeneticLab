using System;

namespace GeneticLab {
    public class BinaryVector {
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;

        public BinaryVector() {}
            
        public BinaryVector(double x, double y) {
            X = x;
            Y = y;
        }
        public double Module() {
            return Math.Sqrt(X * X + Y * Y);
        }

        public static BinaryVector operator+(BinaryVector left, BinaryVector right) {
            return new BinaryVector {X = left.X + right.X, Y = left.Y + right.Y};
        }
        public static BinaryVector operator-(BinaryVector left, BinaryVector right) {
            return new BinaryVector {X = left.X - right.X, Y = left.Y - right.Y};
        }
        public static BinaryVector operator-(BinaryVector val) {
            return new BinaryVector {X = -val.X, Y = -val.Y};
        }
        public static BinaryVector operator*(BinaryVector left, double right) {
            return new BinaryVector {X = left.X * right, Y = left.Y * right};
        }
        public static BinaryVector operator/(BinaryVector left, double right) {
            return new BinaryVector {X = left.X / right, Y = left.Y / right};
        }
    }
}