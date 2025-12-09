using Python.Runtime;
using System;
using System.Drawing;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace FractalApp
{
    public class MainForm : Form
    {
        private PictureBox _pictureBox;

        private double _centerX = -0.5;
        private double _centerY = 0.0;
        private double _scale = 0.005;   // чем меньше, тем сильнее приближение
        private int _maxIter = 200;

        private int _imgWidth = 800;
        private int _imgHeight = 600;

        private Timer _autoZoomTimer;
        private int _autoZoomStepsRemaining;

        // сколько всего шагов авто-зум
        private int _autoZoomTotalSteps = 50;

        // во сколько раз уменьшаем scale на шаг (0.8 = +20% приближение)
        private double _autoZoomFactorPerStep = 0.8;

        // пауза между шагами в миллисекундах
        private int _autoZoomIntervalMs = 300;


        public MainForm()
        {
            Text = "Mandelbrot (C# + pythonnet)";
            ClientSize = new Size(_imgWidth, _imgHeight);

            _pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            Controls.Add(_pictureBox);

            this.MouseWheel += MainForm_MouseWheel;


            _autoZoomTimer = new Timer();
            _autoZoomTimer.Interval = _autoZoomIntervalMs;
            _autoZoomTimer.Tick += AutoZoomTimer_Tick;

            RenderFractal();

            StartAutoZoom();
        }


        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                _scale *= 0.5;  
            else
                _scale *= 2.0;   // отдалить

            if (_scale < 1e-15) _scale = 1e-15;
            if (_scale > 1.0) _scale = 1.0;

            RenderFractal();
        }


        private void StartAutoZoom()
        {
            _autoZoomStepsRemaining = _autoZoomTotalSteps;
            _autoZoomTimer.Interval = _autoZoomIntervalMs;
            _autoZoomTimer.Start();
        }

        private void AutoZoomTimer_Tick(object? sender, EventArgs e)
        {
            if (_autoZoomStepsRemaining <= 0)
            {
                _autoZoomTimer.Stop();
                return;
            }

            _scale *= _autoZoomFactorPerStep;
            if (_scale < 1e-15) _scale = 1e-15;

            RenderFractal();

            _autoZoomStepsRemaining--;
        }

        private void InitializeComponent()
        {

        }


        private void RenderFractal()
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");


                    string pythonDir = @"C:\Users\Ахмед\PycharmProjects\pythonProject6";
                    sys.path.append(pythonDir);


                    dynamic fractal = Py.Import("fractal");


                    dynamic pyData = fractal.mandelbrot(
                        _imgWidth, _imgHeight,
                        _centerX, _centerY,
                        _scale, _maxIter
                    );


                    var bmp = new Bitmap(_imgWidth, _imgHeight);

                    for (int y = 0; y < _imgHeight; y++)
                    {
                        dynamic row = pyData[y];
                        for (int x = 0; x < _imgWidth; x++)
                        {
                            int iter = (int)row[x];

                            double t = (double)iter / _maxIter;
                            int c = (int)(255 * t);
                            if (c < 0) c = 0;
                            if (c > 255) c = 255;

                            bmp.SetPixel(x, y, Color.FromArgb(c, c, c));
                        }
                    }

                    _pictureBox.Image?.Dispose();
                    _pictureBox.Image = bmp;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при построении фрактала:\n" + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}
