using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace GeneticLab {
    public class Model {
        public Model() {}

        public void Initialize(int startPopulationSize) {
            Population = new List<Gene>();
            for(int i = 0; i < startPopulationSize; i++) {
                Population.Add(new Gene(GeneLength, false));
            }
        }

        public BinaryVector[] CreatePath(Gene gene) {
            var result = new List<BinaryVector>();

            for(double time = 0; time <= 1; time = Math.Round(time + dT, 7)) {
                result.Add(gene.CalculatePosition(time));
            }

            return result.ToArray();
        }


        public Gene Crossingover(Gene left, Gene right) {
            Debug.Assert(left.Length == right.Length);

            var newGene = new Gene(left.Length, true);
            var    firstCut  = r.Next(left.Length);
            var    secondCut = r.Next(left.Length);

            var crossType = r.NextDouble();
            if(crossType > 0.5) {
                for(int pos = 0; pos < newGene.Length; pos++) {
                    newGene.X.Amplitudes[pos] = (left.X.Amplitudes[pos] + right.X.Amplitudes[pos]) / 2;
                    newGene.X.Phases[pos]     = (left.X.Phases[pos] + right.X.Phases[pos]) / 2;
                    newGene.Y.Amplitudes[pos] = (left.Y.Amplitudes[pos] + right.Y.Amplitudes[pos]) / 2;
                    newGene.Y.Phases[pos]     = (left.Y.Phases[pos] + right.Y.Phases[pos]) / 2;
                }
            }
            else {

                int position = 0;
                if(firstCut < secondCut) {
                    for(; position < firstCut; position++) {
                        newGene.X.Amplitudes[position] = left.X.Amplitudes[position];
                        newGene.X.Phases[position]     = left.X.Phases[position];
                        newGene.Y.Amplitudes[position] = left.Y.Amplitudes[position];
                        newGene.Y.Phases[position]     = left.Y.Phases[position];
                    }

                    for(; position < secondCut; position++) {
                        newGene.X.Amplitudes[position] = right.X.Amplitudes[position];
                        newGene.X.Phases[position]     = right.X.Phases[position];
                        newGene.Y.Amplitudes[position] = right.Y.Amplitudes[position];
                        newGene.Y.Phases[position]     = right.Y.Phases[position];
                    }

                    for(; position < newGene.Length; position++) {
                        newGene.X.Amplitudes[position] = left.X.Amplitudes[position];
                        newGene.X.Phases[position]     = left.X.Phases[position];
                        newGene.Y.Amplitudes[position] = left.Y.Amplitudes[position];
                        newGene.Y.Phases[position]     = left.Y.Phases[position];
                    }
                }
                else {
                    for(; position < secondCut; position++) {
                        newGene.X.Amplitudes[position] = right.X.Amplitudes[position];
                        newGene.X.Phases[position]     = right.X.Phases[position];
                        newGene.Y.Amplitudes[position] = right.Y.Amplitudes[position];
                        newGene.Y.Phases[position]     = right.Y.Phases[position];
                    }

                    for(; position < secondCut; position++) {
                        newGene.X.Amplitudes[position] = left.X.Amplitudes[position];
                        newGene.X.Phases[position]     = left.X.Phases[position];
                        newGene.Y.Amplitudes[position] = left.Y.Amplitudes[position];
                        newGene.Y.Phases[position]     = left.Y.Phases[position];
                    }

                    for(; position < newGene.Length; position++) {
                        newGene.X.Amplitudes[position] = right.X.Amplitudes[position];
                        newGene.X.Phases[position]     = right.X.Phases[position];
                        newGene.Y.Amplitudes[position] = right.Y.Amplitudes[position];
                        newGene.Y.Phases[position]     = right.Y.Phases[position];
                    }
                }
            }

            return newGene;
        }

        public void Mutation(Gene gene) {
            for(int i = 0; i < gene.Length; i++) {
                if(r.NextDouble() < MutationRate) {
                    gene.X.Amplitudes[i] += (2 * r.NextDouble() - 1) *  (1/((double)i+1));
                    gene.FlushPenalty();
                }

                if(r.NextDouble() < MutationRate) {
                    gene.X.Phases[i] += Math.PI * (2 * r.NextDouble() - 1) * (1/((double)i+1));
                    gene.FlushPenalty();
                }

                if(r.NextDouble() < MutationRate) {
                    gene.Y.Amplitudes[i] += (2 * r.NextDouble() - 1) * (1/((double)i+1));
                    gene.FlushPenalty();
                }

                if(r.NextDouble() < MutationRate) {
                    gene.Y.Phases[i] += Math.PI * (2 * r.NextDouble() - 1) *(1/((double)i+1));
                    gene.FlushPenalty();
                }
            }
        }

        public void Iteration(MainWindow wnd) {
            //wnd.Dispatcher.Invoke(delegate { wnd.Bar.Maximum = Population.Count; });
            var oldPopulationSize = Population.Count;

            for(int i = 0; i < oldPopulationSize; i++) {
                var newGene = Crossingover(Population[r.Next(Population.Count)], Population[r.Next(Population.Count)]);
                Mutation(newGene);
                newGene.X.Adjust();
                newGene.Y.Adjust();
                Population.Add(newGene);
                //wnd.Dispatcher.Invoke(delegate { wnd.Bar.Value = i; });
                //Console.WriteLine($@"{i} {Population.Count}");
            }

            if(Population.Count > MaxPopulation) {
                Population = new List<Gene>(Population.OrderBy(gene => gene.Penalty(this))
                                                      .Take((int) (Population.Count * GoodRate)));
            }
            else {
                Population = new List<Gene>(Population.OrderBy(gene => gene.Penalty(this)));
            }
        }

        public  List<Gene>   Population    { get; set; } = null;
        public  int          MaxPopulation { get; set; } = 128;
        public  double       MutationRate  { get; set; } = 0.10;
        public  double       GoodRate      { get; set; } = 0.3;
        public  double       dT            { get; set; } = 0.010;

        private int _geneLen = 4;
        public int GeneLength {
            get { return _geneLen; }
            set {
                if(GeneLength == value) {
                    return;
                }
                if(Population != null)
                foreach(var gene in Population) {
                    gene.ChangeLength(value);
                    
                }
                _geneLen = value;
            }
        }

        public  double       MaxStepForce  { get; set; } = 0.1;
        public  List<Circle> Circles = new List<Circle>();
        private Random       r       = new Random((int) DateTime.Now.Ticks);
    }
}