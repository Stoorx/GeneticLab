using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GeneticLab {
    public class Gene {
        private static Random        r = new Random((int) DateTime.Now.Ticks);
        public         FourierSeries X;
        public         FourierSeries Y;

        private double _penalties = -1;
        public  int                       Length => X.Length;

        public void FlushPenalty() {
            _penalties = -1;
        }

        public Gene(int length, bool clear = true) {
            Debug.Assert(length > 0, @"Gene length can not be 0.");

            X = new FourierSeries(length);
            Y = new FourierSeries(length);
            if(clear == false) {
                for(int i = 0; i < X.Length; i++) {
                    X.Amplitudes[i] = (2 * r.NextDouble() - 1)* (1/((double)i+1));
                    X.Phases[i]     = (Math.PI * (2 * r.NextDouble() - 1))* (1/((double)i+1));
                }

                for(int i = 0; i < Y.Length; i++) {
                    Y.Amplitudes[i] = (2 * r.NextDouble() - 1)* (1/((double)i+1));
                    Y.Phases[i]     = (Math.PI * (2 * r.NextDouble() - 1))* (1/((double)i+1));
                }


                X.Adjust();
                Y.Adjust();
            }
        }

        public double Penalty(Model model) {
            if(_penalties != -1) {
                return _penalties;
            }

            double       penalty          = 0;
            BinaryVector previousPosition = CalculatePosition(0);
            for(double time = model.dT; time <= 1; time = Math.Round(time + model.dT, 7)) {
                var currentPosition = CalculatePosition(time);
                if(currentPosition.X > 1 || currentPosition.X < 0
                                         || currentPosition.Y > 1 || currentPosition.Y < 0) {
                    penalty          += (currentPosition - previousPosition).Module();
                    previousPosition =  currentPosition;
                }
                else {
                    foreach(var circle in model.Circles) {
                        if(circle.CheckInside(currentPosition)) {
                            penalty          += (currentPosition - previousPosition).Module();
                            previousPosition =  currentPosition;
                        }
                    }
                }
            }

            var maxForce = ForcesPath(model).OrderByDescending(v => v.Module()).First().Module();
            var rpenalty = 1000*Math.Sqrt(penalty) + maxForce;
            _penalties=rpenalty;
            return rpenalty;
        }

        public BinaryVector[] ForcesPath(Model m) {
            var result = new List<BinaryVector>();

            for(double time = 0; time <= 1; time = Math.Round(time + m.dT, 7)) {
                result.Add(CalculateForce(time));
            }

            
            return result.ToArray();
        }
        
        public BinaryVector CalculatePosition(double time) {
            return new BinaryVector(Math.Round(X.Calculate(time) + time, 6), Math.Round(Y.Calculate(time) + time, 6));
            //return new BinaryVector(time, time);
        }

        public BinaryVector CalculateForce(double time) {
            Func<double, double> wndFunc     = (double t) => 3 * t * t - 2 * t * t * t;
            Func<double, double> wndFuncDer  = (double t) => 6 * t - 6 * t * t;
            Func<double, double> wndFuncDer2 = (double t) => 6 - 12 * t;

            Func<double, double, double, double, double, double, double, double> member =
                (double t, double amplitude, double k, double phase, double m, double mDer, double mDer2) => {
                    return -amplitude * (
                                            -4 * Math.PI * Math.PI * k * k * mDer * mDer
                                            * Math.Sin(phase - 2 * Math.PI * k * m)
                                            - 2 * Math.PI * k * mDer2
                                            * Math.Cos(phase - 2 * Math.PI * k * m)
                                        );
                };

            Func<FourierSeries, double, double> fourierSum =
                (FourierSeries fs, double t) => {
                    double result = 0;
                    for(int i = 0; i < fs.Length; i++) {
                        result += member(t, fs.Amplitudes[i], i, fs.Phases[i], wndFunc(t), wndFuncDer(t),
                                         wndFuncDer2(t));
                    }

                    return result;
                };
            return new BinaryVector(Math.Round(fourierSum(X, time) + wndFuncDer2(time), 6),
                                    Math.Round(fourierSum(Y, time) + wndFuncDer2(time), 6));
        }

        public void ChangeLength(int newLength) {
            var nX = new FourierSeries(newLength);
            var nY = new FourierSeries(newLength);

            for(int i = 0; i < X.Length && i < nX.Length; i++) {
                nX.Amplitudes[i] = X.Amplitudes[i];
                nX.Phases[i] = X.Phases[i];
            }

            for(int i = 0; i < Y.Length && i < nY.Length; i++) {
                nY.Amplitudes[i] = Y.Amplitudes[i];
                nY.Phases[i]     = Y.Phases[i];
            }

            X = nX;
            Y = nY;
            X.Adjust();
            Y.Adjust();
        }
    }
}