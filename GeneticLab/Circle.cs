using System;
using System.Collections.Generic;

namespace GeneticLab {
    public class Circle {
        public Circle(BinaryVector position, double radius) {
            Position = position;
            Radius = radius;
        }

        public bool CheckInside(BinaryVector point) {
            return (point - Position).Module() <= Radius;
        }

        public BinaryVector[] ToPath(double dT) {
            var    result = new List<BinaryVector>();
            
            for(double time   = 0; time <= 1; time = Math.Round(time + dT, 7)) {
                result.Add(new BinaryVector(Radius*Math.Sin(2*Math.PI*time), Radius*Math.Cos(2*Math.PI*time))+Position);
            }
            return result.ToArray();
        }
        
        public BinaryVector Position;
        public double Radius;
    }
}