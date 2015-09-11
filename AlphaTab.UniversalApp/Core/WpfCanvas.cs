using AlphaTab;
using AlphaTab.Platform;
using AlphaTab.Platform.Model;
using AlphaTab.Rendering;
using AlphaTab.Rendering.Glyphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace AlphaTab.Platform.CSharp
{
    public class WpfCanvas : ICanvas, IPathCanvas
    {
        private Windows.UI.Color _color;

        private FontFamily _fontFamily;
        private float _fontSize;

        private TextBaseline _textVBaseline;
        private TextAlign _textHAlign;

        private Windows.UI.Xaml.HorizontalAlignment _textHAlignment;
        private Windows.UI.Xaml.VerticalAlignment _textVAlignment;

        private Windows.UI.Xaml.Controls.Canvas _canvas;
        private Windows.UI.Xaml.Shapes.Path _currentPath;

        private float _currentX;
        private float _currentY;
        private int _currentFigureIndex = -1;

        private Windows.UI.Xaml.Media.SolidColorBrush _foregroundBrush = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Black);

        public Color Color
        {
            get
            {
                return new Color(_color.R, _color.G, _color.B, _color.A);
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _color = Windows.UI.Color.FromArgb(value.A, value.R, value.G, value.B);
                RecreateBrush();
            }
        }

        public Font Font
        {
            get
            {
                FontStyle fs = FontStyle.Plain;
                //TODO: if (_font) fs |= FontStyle.Bold;
                //if (_font.Italic) fs |= FontStyle.Italic;
                return new Font(_fontFamily.ToString(), _fontSize, fs);
            }
            set
            {
                //TODO:
                /*var fontStyle = GdiFontStyle.Regular;
                if (value.IsBold) fontStyle |= GdiFontStyle.Bold;
                if (value.IsItalic) fontStyle = GdiFontStyle.Italic;
                */

                _fontFamily = new Windows.UI.Xaml.Media.FontFamily(value.Family);
                _fontSize = value.Size;
            }
        }

        public TextAlign TextAlign
        {
            get { return _textHAlign; }
            set
            {
                _textHAlign = value;
                switch (value)
                {
                    case TextAlign.Left:
                        _textHAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
                        break;

                    case TextAlign.Center:
                        _textHAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                        break;

                    case TextAlign.Right:
                        _textHAlignment = Windows.UI.Xaml.HorizontalAlignment.Right;
                        break;
                }
            }
        }

        public float LineWidth
        {
            get
            {
                System.Diagnostics.Debug.WriteLine("Unimplemented: LineWidth");
                return 1;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public RenderingResources Resources { get; set; }

        public TextBaseline TextBaseline
        {
            get { return _textVBaseline; }
            set
            {
                _textVBaseline = value;
            }
        }

        public WpfCanvas()
        {
            _canvas = new Windows.UI.Xaml.Controls.Canvas();
            _currentPath = CreateNewPath();
        }

        public void BeginRender(float width, float height)
        {
            _canvas = new Windows.UI.Xaml.Controls.Canvas();

            _canvas.Width = width;
            _canvas.Height = height;
        }

        public object EndRender()
        {
            return _canvas;
        }

        private void RecreateBrush()
        {
            _foregroundBrush = new Windows.UI.Xaml.Media.SolidColorBrush(_color);
        }

        private Windows.UI.Xaml.Shapes.Path CreateNewPath()
        {
            _currentPath = new Windows.UI.Xaml.Shapes.Path();
            _currentPath.Stroke = _foregroundBrush;

            _currentPath.StrokeThickness = 1;
            _currentPath.Data = new PathGeometry();
            ((PathGeometry)_currentPath.Data).Figures = new PathFigureCollection();
            _currentFigureIndex = -1;
            return _currentPath;
        }

        public void StrokeRect(float x, float y, float w, float h)
        {
            var rect = new Windows.UI.Xaml.Shapes.Rectangle();
            rect.Width = w;
            rect.Height = h;

            rect.Stroke = _foregroundBrush;

            Windows.UI.Xaml.Controls.Canvas.SetTop(rect, y);
            Windows.UI.Xaml.Controls.Canvas.SetLeft(rect, x);

            _canvas.Children.Add(rect);
        }

        public void FillRect(float x, float y, float w, float h)
        {
            var rect = new Windows.UI.Xaml.Shapes.Rectangle();
            rect.Width = w;
            rect.Height = h;

            rect.Fill = _foregroundBrush;
            rect.Stroke = _foregroundBrush;

            Windows.UI.Xaml.Controls.Canvas.SetTop(rect, y);
            Windows.UI.Xaml.Controls.Canvas.SetLeft(rect, x);

            _canvas.Children.Add(rect);
        }

        public void BeginPath()
        {
            ((PathGeometry)_currentPath.Data).Figures.Add(new PathFigure { });
            _currentFigureIndex++;
        }

        public void ClosePath()
        {
            if (_currentPath != null)
            {
                ((PathGeometry)_currentPath.Data).Figures[_currentFigureIndex].IsClosed = true;
            }
            else
            {
                //invalid path
            }
        }

        public void MoveTo(float x, float y)
        {
            if (_currentPath != null)
            {
                ((PathGeometry)_currentPath.Data).Figures[0].StartPoint = new Point(x, y);
            }

            _currentX = x;
            _currentY = y;
        }

        public void LineTo(float x, float y)
        {
            ((PathGeometry)_currentPath.Data).Figures[_currentFigureIndex].Segments.Add(new LineSegment() { Point = new Point(x, y) });

            _currentX = x;
            _currentY = y;
        }

        public void QuadraticCurveTo(float cpx, float cpy, float x, float y)
        {
            var pathFigure = ((PathGeometry)_currentPath.Data).Figures[_currentFigureIndex];

            pathFigure.Segments.Add(new QuadraticBezierSegment()
            {
                Point1 = new Point(cpx, cpy),
                Point2 = new Point(x, y),
            });
            _currentX = x;
            _currentY = y;
        }

        public void BezierCurveTo(float cp1x, float cp1y, float cp2x, float cp2y, float x, float y)
        {
            var pathFigure = ((PathGeometry)_currentPath.Data).Figures[_currentFigureIndex];
            //pathFigure.StartPoint = new Point(x1, y1);

            pathFigure.Segments.Add(new BezierSegment()
            {
                Point1 = new Point(cp1x, cp1y),
                Point2 = new Point(cp2x, cp2y),
                Point3 = new Point(x, y)
            });

            _currentX = x;
            _currentY = y;
        }

        private void AddBezier(float x1, float y1, float cp1x, float cp1y, float cp2x, float cp2y, float x2, float y2)
        {
            var pathFigure = ((PathGeometry)_currentPath.Data).Figures[_currentFigureIndex];
            pathFigure.StartPoint = new Point(x1, y1);

            pathFigure.Segments.Add(new BezierSegment()
            {
                Point1 = new Point(cp1x, cp1y),
                Point2 = new Point(cp2x, cp2y),
                Point3 = new Point(x2, y2)
            });
        }

        public void FillCircle(float x, float y, float radius)
        {
            var e = new Windows.UI.Xaml.Shapes.Ellipse();

            e.Width = radius;
            e.Height = radius; //*2?

            e.Fill = _foregroundBrush;
            e.Stroke = _foregroundBrush;

            Windows.UI.Xaml.Controls.Canvas.SetTop(e, y);
            Windows.UI.Xaml.Controls.Canvas.SetLeft(e, x);

            _canvas.Children.Add(e);

            _currentX = x;
            _currentY = y;
        }

        public void Fill()
        {
            _currentPath.Fill = _foregroundBrush;
            ((PathGeometry)_currentPath.Data).Figures[_currentFigureIndex].IsFilled = true;
            ClosePath();
            _canvas.Children.Add(_currentPath);

            _currentPath = CreateNewPath();
        }

        public void Stroke()
        {
            _canvas.Children.Add(_currentPath);
            _currentPath = CreateNewPath();
        }

        public void FillText(string text, float x, float y)
        {
            var t = new Windows.UI.Xaml.Controls.TextBlock();

            t.Name = Guid.NewGuid().ToString();

            t.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
            t.FontSize = _fontSize;
            t.Text = text;
            t.Foreground = _foregroundBrush;
            t.FontFamily = _fontFamily;
            t.HorizontalAlignment = _textHAlignment;

            var posX = x;
            var posY = y;

            //approx measurement of text in order to offset for center/middle
            if (_textVBaseline == TextBaseline.Middle)
            {
                t.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                var approxSize = MeasureText("X");
                posY = posY - approxSize / 2;
            }
            if (_textVBaseline == TextBaseline.Top)
            {
                t.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            }
            if (_textVBaseline == TextBaseline.Bottom)
            {
                t.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom;
            }

            if (_textHAlignment == Windows.UI.Xaml.HorizontalAlignment.Center)
            {
                var width = MeasureText(text);
                posX = posX - (width / 4);
            }

            Windows.UI.Xaml.Controls.Canvas.SetTop(t, posY);
            Windows.UI.Xaml.Controls.Canvas.SetLeft(t, posX);

            _canvas.Children.Add(t);
        }

        public float MeasureText(string text)
        {
            //TODO; proper method
            return _fontSize * text.Length;
            /*var t = new Windows.UI.Xaml.Controls.TextBlock();
            t.Text = text;
            t.FontFamily = _fontFamily;
            t.FontSize = _fontSize;
            t.UpdateLayout();
            return (int)Math.Ceiling(t.ActualWidth);*/
        }

        public void FillMusicFontSymbol(float x, float y, float scale, MusicFontSymbol symbol)
        {
            if (symbol == MusicFontSymbol.None)
            {
                return;
            }

            var glyph = new Rendering.Utils.SvgRenderer(MusicFont.SymbolLookup[symbol], scale, scale);
            glyph.Paint(x, y, this);
        }
    }
}