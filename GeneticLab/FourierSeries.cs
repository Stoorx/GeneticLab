using System;

namespace GeneticLab {
    public class FourierSeries {
        public double[] Amplitudes;
        public double[] Phases;

        public FourierSeries(int length) {
            Amplitudes = new double[length];
            Phases = new double[length];
            for(int i = 0; i < length; i++) {
                Amplitudes[i] = 0;
                Phases[i] = 0;
            }
        }

        public int Length => Amplitudes.Length;

        public double Calculate(double t) {
            double result = Amplitudes[0];
            for(int i = 1; i < Length; ++i) {
                result += Amplitudes[i]*Math.Sin(2 * Math.PI * i * t + Phases[i]);
            }

            return result;
        }

        public void Adjust() {
            var startPoint = Calculate(0);
            Amplitudes[0] -= startPoint;
        }
    }
}