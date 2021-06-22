using System;
using System.ComponentModel;
using System.Collections.Generic;

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;

using SharpVectors.Runtime;
using SharpVectors;

namespace MMO_Client.Client.Assets.Controls
{
    /// <summary>
    /// This is the main drawing canvas for the rendered SVG diagrams.
    /// </summary>
    public class CustomSvgDrawingCanvas : Canvas
    {
        #region Private Fields

        private const string DefaultTitle = "CustomSvgDrawingCanvas";

        private string _appTitle;
        private bool _drawForInteractivity;

        private Rect _bounds;

        private Transform _displayTransform;

        private double _offsetX;
        private double _offsetY;

        private DrawingGroup _wholeDrawing;
        private DrawingGroup _linksDrawing;

        private DrawingVisual _hostVisual;

        private SvgInteractiveModes _interactiveMode;

        // Create a collection of child visual objects.
        private List<Drawing> _drawObjects;

        // Create a collection of child visual link objects.
        private List<Drawing> _linkObjects;

        private event EventHandler<SvgAlertArgs> _svgAlerts;
        private event EventHandler<SvgErrorArgs> _svgErrors;

        #endregion

        #region Constructors and Destructor

        static CustomSvgDrawingCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomSvgDrawingCanvas),
                new FrameworkPropertyMetadata(typeof(CustomSvgDrawingCanvas)));
        }

        public CustomSvgDrawingCanvas()
        {
            _drawForInteractivity = true;

            _appTitle = DefaultTitle;

            _drawObjects = new List<Drawing>();
            _linkObjects = new List<Drawing>();

            _displayTransform = Transform.Identity;

            this.Background = Brushes.Transparent;

            this.SnapsToDevicePixels = true;
        }

        #endregion

        #region Public Properties

        [DefaultValue(DefaultTitle)]
        [Description("The title of the application, used in displaying error and alert messages.")]
        public string AppTitle
        {
            get
            {
                return _appTitle;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _appTitle = value;
                }
            }
        }

        public bool DesignMode
        {
            get
            {
                if (DesignerProperties.GetIsInDesignMode(new DependencyObject()) ||
                    LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    return true;
                }
                return false;
            }
        }

        public Rect Bounds
        {
            get
            {
                return _bounds;
            }
        }

        public Transform DisplayTransform
        {
            get
            {
                return _displayTransform;
            }
        }

        public Point DisplayOffset
        {
            get
            {
                return new(_offsetX, _offsetY);
            }
        }

        public IList<Drawing> DrawObjects
        {
            get
            {
                return _drawObjects;
            }
        }

        public IList<Drawing> LinkObjects
        {
            get
            {
                return _linkObjects;
            }
        }

        public DrawingVisual HostVisual
        {
            get
            {
                return _hostVisual;
            }
        }

        public SvgInteractiveModes InteractiveMode
        {
            get
            {
                return _interactiveMode;
            }
            set
            {
                _interactiveMode = value;
            }
        }

        #endregion

        #region Public Events

        public event EventHandler<SvgAlertArgs> Alert
        {
            add { _svgAlerts += value; }
            remove { _svgAlerts -= value; }
        }

        public event EventHandler<SvgErrorArgs> Error
        {
            add { _svgErrors += value; }
            remove { _svgErrors -= value; }
        }

        #endregion

        #region Protected Properties

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get
            {
                if (_hostVisual != null)
                {
                    return 1;
                }
                return 0;
            }
        }

        #endregion

        #region Public Methods
        public void LoadDiagrams(DrawingGroup whole, DrawingGroup links, DrawingGroup main, Rect maxBounds)
        {
            if (whole == null)
            {
                return;
            }

            this.UnloadDiagrams();

            this.Draw(whole, main, maxBounds);

            _wholeDrawing = whole;
            _linksDrawing = links;

            this.InvalidateMeasure();
        }

        public void UnloadDiagrams()
        {
            _offsetX = 0;
            _offsetY = 0;
            _bounds = new(0, 0, 1, 1);

            _wholeDrawing = null;

            _displayTransform = Transform.Identity;

            this.ClearVisuals();
            this.ClearDrawings();

            this.InvalidateMeasure();
            //            this.InvalidateVisual();
            this.UpdateLayout();

            //            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
            //            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, EmptyDelegate);
        }

        #region RenderDiagrams Methods

        public void RenderDiagrams(DrawingGroup renderedGroup, Rect maxBounds)
        {
            DrawingCollection drawings = renderedGroup.Children;
            int linkIndex = -1;
            int drawIndex = -1;
            for (int i = 0; i < drawings.Count; i++)
            {
                Drawing drawing = drawings[i];
                //string drawingName = SvgObject.GetName(drawing);
                string drawingName = SvgLink.GetKey(drawing);
                if (!string.IsNullOrWhiteSpace(drawingName) && string.Equals(drawingName, SvgObject.DrawLayer))
                {
                    drawIndex = i;
                }
                else if (!string.IsNullOrWhiteSpace(drawingName) &&
                    string.Equals(drawingName, SvgObject.LinksLayer))
                {
                    linkIndex = i;
                }
            }

            DrawingGroup mainGroups = null;
            if (drawIndex >= 0)
            {
                mainGroups = drawings[drawIndex] as DrawingGroup;
            }
            DrawingGroup linkGroups = null;
            if (linkIndex >= 0)
            {
                linkGroups = drawings[linkIndex] as DrawingGroup;
            }

            this.LoadDiagrams(renderedGroup, linkGroups, mainGroups, maxBounds);

            _bounds = _wholeDrawing.Bounds;

            this.InvalidateMeasure();
            this.InvalidateVisual();
        }

        #endregion

        #endregion

        #region Protected Methods

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (_hostVisual == null)
            {
                return null;
            }

            return _hostVisual;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (_wholeDrawing != null)
            {
                Rect rectBounds = _wholeDrawing.Bounds;
                if (!rectBounds.IsEmpty)
                {
                    // Return the new size
                    double diaWidth = rectBounds.Width;
                    double diaHeight = rectBounds.Height;

                    _bounds = rectBounds;
                    return new(diaWidth, diaHeight);
                }
            }
            else
            {
                double dWidth = this.Width;
                double dHeight = this.Height;
                if ((!double.IsNaN(dWidth) && !double.IsInfinity(dWidth)) &&
                    (!double.IsNaN(dHeight) && !double.IsInfinity(dHeight)))
                {
                    return new(dWidth, dHeight);
                }
            }

            var sizeCtrl = base.MeasureOverride(constraint);
            if ((!double.IsNaN(sizeCtrl.Width) && !double.IsInfinity(sizeCtrl.Width)) &&
                (!double.IsNaN(sizeCtrl.Height) && !double.IsInfinity(sizeCtrl.Height)))
            {
                if (sizeCtrl.Width != 0 && sizeCtrl.Height != 0)
                {
                    return sizeCtrl;
                }
            }

            return new(120, 120);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
        }

        protected virtual void OnHandleAlert(string message)
        {
            if (this.DesignMode)
            {
                return;
            }
            var alertArgs = new SvgAlertArgs(message);
            _svgAlerts?.Invoke(this, alertArgs);
            if (!alertArgs.Handled)
            {
                MessageBox.Show(alertArgs.Message, _appTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        protected virtual void OnHandleError(string message, Exception exception)
        {
            if (this.DesignMode)
            {
                return;
            }
            var errorArgs = new SvgErrorArgs(message, exception);
            _svgErrors?.Invoke(this, errorArgs);
            if (!errorArgs.Handled)
            {
                throw new SvgErrorException(errorArgs);
            }
        }

        #endregion

        #region Private Methods

        #region Visuals Methods

        private void AddVisual(DrawingVisual visual)
        {
            if (visual == null)
            {
                return;
            }

            _hostVisual = visual;

            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }

        private void RemoveVisual(DrawingVisual visual)
        {
            if (visual == null)
            {
                return;
            }

            base.RemoveVisualChild(visual);
            base.RemoveLogicalChild(visual);
        }

        private void ClearVisuals()
        {
            if (_hostVisual != null)
            {
                base.RemoveVisualChild(_hostVisual);
                base.RemoveLogicalChild(_hostVisual);

                _hostVisual = null;
            }
        }

        private void AddDrawing(Drawing visual)
        {
            if (visual == null)
            {
                return;
            }

            _drawObjects.Add(visual);
        }

        private void RemoveDrawing(Drawing visual)
        {
            if (visual == null)
            {
                return;
            }

            _drawObjects.Remove(visual);

            if (_linkObjects != null && _linkObjects.Count != 0)
            {
                _linkObjects.Remove(visual);
            }
        }

        private void ClearDrawings()
        {
            if (_drawObjects != null && _drawObjects.Count != 0)
            {
                _drawObjects.Clear();
            }

            if (_linkObjects != null && _linkObjects.Count != 0)
            {
                _linkObjects.Clear();
            }
        }

        #endregion

        #region Draw Methods

        private void Draw(DrawingGroup group, DrawingGroup main, Rect maxBounds)
        {
            DrawingVisual drawingVisual = new();

            DrawingContext drawingContext = drawingVisual.RenderOpen();

            _offsetX = 0;
            _offsetY = 0;
            _displayTransform = Transform.Identity;

            TranslateTransform offsetTransform = null;
            _offsetX += maxBounds.X;
            _offsetY += maxBounds.Y;

            _bounds = maxBounds;

            if (!_offsetX.Equals(0) || !_offsetY.Equals(0))
            {
                offsetTransform = new TranslateTransform(-_offsetX, -_offsetY);
                _displayTransform = new TranslateTransform(_offsetX, _offsetY); // the inverse...
            }

            //Canvas.SetTop(this, -_offsetX);
            //Canvas.SetLeft(this, -_offsetY);

            if (offsetTransform != null)
            {
                drawingContext.PushTransform(offsetTransform);
            }

            drawingContext.DrawDrawing(group);

            if (offsetTransform != null)
            {
                drawingContext.Pop();
            }

            //            drawingVisual.Opacity = group.Opacity;

            //Transform transform = group.Transform;
            //if (transform == null)
            //{
            //    transform = offsetTransform;
            //}
            //if (offsetTransform != null)
            //{
            //    drawingVisual.Transform = offsetTransform;
            //}
            Geometry clipGeometry = group.ClipGeometry;
            if (clipGeometry != null)
            {
                drawingVisual.Clip = clipGeometry;
            }

            drawingContext.Close();

            this.AddVisual(drawingVisual);

            if (_drawForInteractivity)
            {
                if (main == null)
                {
                    main = group;
                }

                this.EnumerateDrawings(main);
            }
        }

        private void EnumerateDrawings(DrawingGroup group)
        {
            if (group == null || group == _linksDrawing)
            {
                return;
            }

            DrawingCollection drawings = group.Children;
            for (int i = 0; i < drawings.Count; i++)
            {
                Drawing drawing = drawings[i];
                if (drawing is DrawingGroup childGroup)
                {
                    SvgObjectType objectType = SvgObject.GetType(childGroup);
                    if (objectType == SvgObjectType.Link)
                    {
                        InsertLinkDrawing(childGroup);
                    }
                    else if (objectType == SvgObjectType.Text)
                    {
                        InsertTextDrawing(childGroup);
                    }
                    else
                    {
                        EnumerateDrawings(childGroup);
                    }
                }
                else
                {
                    InsertDrawing(drawing);
                }
            }
        }

        private void InsertTextDrawing(DrawingGroup group)
        {
            this.AddDrawing(group);
        }

        private void InsertLinkDrawing(DrawingGroup group)
        {
            this.AddDrawing(group);

            if (_linkObjects != null)
            {
                _linkObjects.Add(group);
            }
        }

        private void InsertDrawing(Drawing drawing)
        {
            this.AddDrawing(drawing);
        }

        #endregion

        #region HitTest Methods

        private Drawing HitTest(Point pt)
        {
            if (_linkObjects == null)
            {
                return null;
            }

            Point ptDisplay = _displayTransform.Transform(pt);

            DrawingGroup groupDrawing = null;
            GlyphRunDrawing glyRunDrawing = null;
            GeometryDrawing geometryDrawing = null;

            Drawing foundDrawing = null;

            //for (int i = 0; i < _linkObjects.Count; i++)
            for (int i = _linkObjects.Count - 1; i >= 0; i--)
            {
                Drawing drawing = _linkObjects[i];
                if (TryCast.Cast(drawing, out geometryDrawing))
                {
                    if (HitTestDrawing(geometryDrawing, ptDisplay))
                    {
                        foundDrawing = drawing;
                        break;
                    }
                }
                else if (TryCast.Cast(drawing, out groupDrawing))
                {
                    if (HitTestDrawing(groupDrawing, ptDisplay))
                    {
                        foundDrawing = drawing;
                        break;
                    }
                    else if (SvgObject.GetType(groupDrawing) == SvgObjectType.Text &&
                        groupDrawing.Bounds.Contains(ptDisplay))
                    {
                        foundDrawing = drawing;
                        break;
                    }
                }
                else if (TryCast.Cast(drawing, out glyRunDrawing))
                {
                    if (HitTestDrawing(glyRunDrawing, ptDisplay))
                    {
                        foundDrawing = drawing;
                        break;
                    }
                }
            }

            return foundDrawing;
        }

        private bool HitTestDrawing(GlyphRunDrawing drawing, Point pt)
        {
            if (drawing != null && drawing.Bounds.Contains(pt))
            {
                return true;
            }

            return false;
        }

        private bool HitTestDrawing(GeometryDrawing drawing, Point pt)
        {
            Pen pen = drawing.Pen;
            Brush brush = drawing.Brush;
            if (pen != null && brush == null)
            {
                if (drawing.Geometry.StrokeContains(pen, pt))
                {
                    return true;
                }
                else
                {
                    Geometry geometry = drawing.Geometry;

                    RectangleGeometry rectangle = null;
                    PathGeometry path = null;
                    if (TryCast.Cast(geometry, out EllipseGeometry ellipse))
                    {
                        if (ellipse.FillContains(pt))
                        {
                            return true;
                        }
                    }
                    else if (TryCast.Cast(geometry, out rectangle))
                    {
                        if (rectangle.FillContains(pt))
                        {
                            return true;
                        }
                    }
                    else if (TryCast.Cast(geometry, out path))
                    {
                        PathFigureCollection pathFigures = path.Figures;
                        int itemCount = pathFigures.Count;
                        if (itemCount == 1)
                        {
                            if (pathFigures[0].IsClosed && path.FillContains(pt))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            for (int f = 0; f < itemCount; f++)
                            {
                                PathFigure pathFigure = pathFigures[f];
                                if (pathFigure.IsClosed)
                                {
                                    PathFigureCollection testFigures = new PathFigureCollection();
                                    testFigures.Add(pathFigure);

                                    PathGeometry testPath = new PathGeometry();
                                    testPath.Figures = testFigures;

                                    if (testPath.FillContains(pt))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (brush != null && drawing.Geometry.FillContains(pt))
            {
                return true;
            }

            return false;
        }

        private bool HitTestDrawing(DrawingGroup group, Point pt)
        {
            if (group.Bounds.Contains(pt))
            {
                DrawingGroup groupDrawing = null;
                GlyphRunDrawing glyRunDrawing = null;
                GeometryDrawing geometryDrawing = null;
                DrawingCollection drawings = group.Children;

                for (int i = 0; i < drawings.Count; i++)
                {
                    Drawing drawing = drawings[i];
                    if (TryCast.Cast(drawing, out geometryDrawing))
                    {
                        if (HitTestDrawing(geometryDrawing, pt))
                        {
                            return true;
                        }
                    }
                    else if (TryCast.Cast(drawing, out groupDrawing))
                    {
                        if (HitTestDrawing(groupDrawing, pt))
                        {
                            return true;
                        }
                        SvgObjectType objectType = SvgObject.GetType(groupDrawing);
                        if (objectType == SvgObjectType.Text && groupDrawing.Bounds.Contains(pt))
                        {
                            return true;
                        }
                    }
                    else if (TryCast.Cast(drawing, out glyRunDrawing))
                    {
                        if (HitTestDrawing(glyRunDrawing, pt))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        private static bool IsValidBounds(Rect rectBounds)
        {
            if (rectBounds.IsEmpty || double.IsNaN(rectBounds.Width) || double.IsNaN(rectBounds.Height)
                || double.IsInfinity(rectBounds.Width) || double.IsInfinity(rectBounds.Height))
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
