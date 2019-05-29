using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace GeneticLab {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private Model m;
        private int   iterationsCounter = 0;

        public MainWindow() {
            InitializeComponent();
            GeneticPlotView.Model = new PlotModel();
            GeneticPlotView.Model.Axes.Add(new LinearAxis() {
                                                                Position           = AxisPosition.Bottom,
                                                                MajorGridlineStyle = LineStyle.Solid,
                                                                MinorGridlineStyle = LineStyle.Dot
                                                            });
            GeneticPlotView.Model.Axes.Add(new LinearAxis() {
                                                                Position           = AxisPosition.Left,
                                                                MajorGridlineStyle = LineStyle.Solid,
                                                                MinorGridlineStyle = LineStyle.Dot
                                                            });

/*
            m = new Model {
                              dT           = Double.Parse(dTText.Text.Replace(".", ",")), GeneLength = 5, MaxPopulation = Int32.Parse(MaxPopulationText.Text.Replace(".", ",")),
                              GoodRate     = Double.Parse(GoodRateText.Text.Replace(".", ",")),
                              MutationRate = Double.Parse(MutRateText.Text.Replace(".", ","))
                          };
            m.Circles.Add(new Circle(new BinaryVector(0.3, 0.6), 0.2));
            m.Circles.Add(new Circle(new BinaryVector(0.6, 0.4), 0.15));
            m.Initialize(128);
            DrawPlot();*/
        }

        private void DrawPlot(int count = 0) {
            GeneticPlotView.Model.Series.Clear();
            var curCount = 0;
            foreach(var gene in m.Population) {
                if(curCount >= count && count != 0)
                    break;
                var path = m.CreatePath(gene);
                var ls   = new LineSeries();
                foreach(var point in path) {
                    ls.Points.Add(new DataPoint(point.X, point.Y));
                }

                ls.Title = $"{gene.Penalty(m).ToString()}";
                GeneticPlotView.Model.Series.Add(ls);
                curCount++;
            }

            foreach(var circle in m.Circles) {
                var path = circle.ToPath(0.1);
                var ls   = new LineSeries {Color = OxyColor.FromRgb(0, 0, 0), LineStyle = LineStyle.Solid};
                
                foreach(var point in path) {
                    ls.Points.Add(new DataPoint(point.X, point.Y));
                }

                GeneticPlotView.Model.Series.Add(ls);
            }

            // box
            var box = new LineSeries();
            box.Points.Add(new DataPoint(0, 0));
            box.Points.Add(new DataPoint(0, 1));
            box.Points.Add(new DataPoint(1, 1));
            box.Points.Add(new DataPoint(1, 0));
            box.Points.Add(new DataPoint(0, 0));
            box.Color     = OxyColors.Black;
            box.LineStyle = LineStyle.Solid;

            GeneticPlotView.Model.Series.Add(box);
            GeneticPlotView.InvalidatePlot();
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            try {
                m.dT            = Double.Parse(dTText.Text.Replace(".", ","));
                m.MaxPopulation = Int32.Parse(MaxPopulationText.Text.Replace(".", ","));
                m.GoodRate      = Double.Parse(GoodRateText.Text.Replace(".", ","));
                m.MutationRate  = Double.Parse(MutRateText.Text.Replace(".", ","));
                m.GeneLength = Int32.Parse(GeneLengthText.Text);
            }
            catch(Exception ex) {
                return;
            }

            StartButton.IsEnabled = false;
            int iterations = 1;
            if(Int32.TryParse(ItCounter.Text, out iterations)) {
                Bar.Maximum = iterations;
                Thread t = new Thread(new ThreadStart(delegate {
                                                          for(int i = 0; i < iterations; i++) {
                                                              m.Iteration(this);
                                                              this.Dispatcher.Invoke(delegate {
//                                                                                     lock() {
//                                                                                       int il = i;  
//                                                                                     }
//                                                                                     
                                                                                         Bar.Value = i;
                                                                                         iterationsCounter++;
                                                                                         IterationsLabel.Content =
                                                                                             iterationsCounter
                                                                                                 .ToString();
                                                                                         PopSize.Content =
                                                                                             m.Population.Count;
                                                                                         if(i % 250 == 0) {
                                                                                             DrawPlot();
                                                                                         }
                                                                                     });
                                                          }

                                                          this.Dispatcher.Invoke(delegate {
//                                                                                     lock() {
//                                                                                       int il = i;  
//                                                                                     }
//                                                                                     
                                                                                     Bar.Value = 0;
                                                                                     DrawPlot();
                                                                                     StartButton.IsEnabled = true;
                                                                                 });
                                                      }));
                t.Start();
            }
        }

        private void ShowFirst_OnClick(object sender, RoutedEventArgs e) {
            if(Int32.TryParse(FirstCount.Text, out int count)) {
                DrawPlot(count);
            }
        }

        private void Reset_OnClick(object sender, RoutedEventArgs e) {
            m = new Model {
                              dT            = Double.Parse(dTText.Text.Replace(".", ",")),
                              GeneLength    = Int32.Parse(GeneLengthText.Text),
                              MaxPopulation = Int32.Parse(MaxPopulationText.Text),
                              GoodRate      = Double.Parse(GoodRateText.Text.Replace(".", ",")),
                              MutationRate  = Double.Parse(MutRateText.Text.Replace(".", ","))
                          };
            FileStream   fs           = new FileStream(@"mess.txt", FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fs);

            string circle;
            while((circle = streamReader.ReadLine()) != null) {
                var sr  = new StringReader(circle);
                var par = circle.Split(' ');
                m.Circles.Add(new
                                  Circle(new BinaryVector(Double.Parse(par[0], NumberStyles.Any), Double.Parse(par[1], NumberStyles.Any)),
                                         Double.Parse(par[2], NumberStyles.Any)));
            }


           // m.Circles.Add(new Circle(new BinaryVector(0.6, 0.4), 0.15));
           // m.Circles.Add(new Circle(new BinaryVector(0.9, 0.9), 0.05));
            m.Initialize(m.MaxPopulation);
            iterationsCounter       = 0;
            IterationsLabel.Content = 0;
            DrawPlot();
        }
    }
}