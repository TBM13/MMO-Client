using System;
using System.IO;
using System.Globalization;
using System.ComponentModel;

using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;

using SharpVectors.Runtime;
using SharpVectors.Dom.Svg;
using SharpVectors.Converters;

namespace MMO_Client.Client.Assets.Controls
{
    /// <summary>
    /// This is a modified version of <see cref="SvgViewbox"/>.
    /// </summary>
    /// <remarks>
    /// It wraps the drawing canvas, <see cref="SvgDrawingCanvas"/>, instead of
    /// image control, therefore any interactivity support implemented in the
    /// drawing canvas will be available in the <see cref="Viewbox"/>.
    /// </remarks>
    public class CustomSvgViewbox : Viewbox, ISvgControl, IUriContext
    {
        #region Public Fields

        /// <summary>
        /// Identifies the <see cref="XamlSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XamlSourceProperty =
            DependencyProperty.Register("XamlSource", typeof(string), typeof(CustomSvgViewbox),
                new FrameworkPropertyMetadata(null, OnXamlSourceChanged));

        #endregion

        #region Private Fields

        private const string DefaultTitle = "CustomSvgViewbox";

        private bool _isAutoSized;
        private bool _autoSize;
        private bool _textAsGeometry;
        private bool _includeRuntime;
        private bool _optimizePath;

        private bool _ensureViewboxPosition;
        private bool _ensureViewboxSize;
        private bool _ignoreRootViewbox;

        private DrawingGroup _svgDrawing;

        private string _appTitle;
        private CultureInfo _culture;

        private Uri _baseUri;
        private string _sourceXaml;

        private CustomSvgDrawingCanvas _drawingCanvas;

        private SvgInteractiveModes _interactiveMode;

        private event EventHandler<SvgAlertArgs> _svgAlerts;
        private event EventHandler<SvgErrorArgs> _svgErrors;

        #endregion

        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgViewbox"/> class.
        /// </summary>
        public CustomSvgViewbox()
        {
            _textAsGeometry = false;
            _includeRuntime = true;
            _optimizePath = true;
            _drawingCanvas = new CustomSvgDrawingCanvas();

            _appTitle = DefaultTitle;

            this.Child = _drawingCanvas;
        }

        /// <summary>
        /// Static constructor to define metadata for the control (and link it to the style in Generic.xaml).
        /// </summary>
        static CustomSvgViewbox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomSvgViewbox),
                new FrameworkPropertyMetadata(typeof(CustomSvgViewbox)));
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

        /// <summary>
        /// Gets or sets the path to the XAML source to load into this <see cref="Canvas"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> specifying the path to the XAML source.
        /// Settings this to <see langword="null"/> will close any opened diagram.
        /// </value>
        /// <seealso cref="UriSource"/>
        /// <seealso cref="StreamSource"/>
        public string XamlSource
        {
            get
            {
                return (string)GetValue(XamlSourceProperty);
            }
            set
            {
                _sourceXaml = value;
                this.SetValue(XamlSourceProperty, value);
            }
        }

        /// <summary>
        /// Gets the drawing canvas, which is the child of this <see cref="Viewbox"/>.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="SvgDrawingCanvas"/> specifying the child
        /// of this <see cref="Viewbox"/>, which handles the rendering.
        /// </value>
        public CustomSvgDrawingCanvas DrawingCanvas
        {
            get
            {
                return _drawingCanvas;
            }
        }

        /// <summary>
        /// Gets the drawing from the SVG file conversion.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="DrawingGroup"/> specifying the converted drawings
        /// which is rendered in the canvas and displayed in the this viewbox.
        public DrawingGroup Drawings
        {
            get
            {
                return _svgDrawing;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically resize this
        /// <see cref="Viewbox"/> based on the size of the loaded drawing.
        /// </summary>
        /// <value>
        /// This is <see langword="true"/> if this <see cref="Viewbox"/> is
        /// automatically resized based on the size of the loaded drawing;
        /// otherwise, it is <see langword="false"/>. The default is 
        /// <see langword="false"/>, and the user-defined size or the parent assigned
        /// layout size is used.
        /// </value>
        public bool AutoSize
        {
            get
            {
                return _autoSize;
            }
            set
            {
                _autoSize = value;

                this.OnAutoSizeChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the path geometry is 
        /// optimized using the <see cref="StreamGeometry"/>.
        /// </summary>
        /// <value>
        /// This is <see langword="true"/> if the path geometry is optimized
        /// using the <see cref="StreamGeometry"/>; otherwise, it is 
        /// <see langword="false"/>. The default is <see langword="true"/>.
        /// </value>
        public bool OptimizePath
        {
            get
            {
                return _optimizePath;
            }
            set
            {
                _optimizePath = value;

                this.OnSettingsChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the texts are rendered as
        /// path geometry.
        /// </summary>
        /// <value>
        /// This is <see langword="true"/> if texts are rendered as path 
        /// geometries; otherwise, this is <see langword="false"/>. The default
        /// is <see langword="false"/>.
        /// </value>
        public bool TextAsGeometry
        {
            get
            {
                return _textAsGeometry;
            }
            set
            {
                _textAsGeometry = value;

                this.OnSettingsChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <c>SharpVectors.Runtime.dll</c>
        /// classes are used in the generated output.
        /// </summary>
        /// <value>
        /// This is <see langword="true"/> if the <c>SharpVectors.Runtime.dll</c>
        /// classes and types are used in the generated output; otherwise, it is 
        /// <see langword="false"/>. The default is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// The use of the <c>SharpVectors.Runtime.dll</c> prevents the hard-coded
        /// font path generated by the <see cref="FormattedText"/> class, support
        /// for embedded images etc.
        /// </remarks>
        public bool IncludeRuntime
        {
            get
            {
                return _includeRuntime;
            }
            set
            {
                _includeRuntime = value;

                this.OnSettingsChanged();
            }
        }

        /// <summary>
        /// Gets or sets the main culture information used for rendering texts.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="CultureInfo"/> specifying the main
        /// culture information for texts. The default is the English culture.
        /// </value>
        /// <remarks>
        /// <para>
        /// This is the culture information passed to the <see cref="FormattedText"/>
        /// class instance for the text rendering.
        /// </para>
        /// <para>
        /// The library does not currently provide any means of splitting texts
        /// into its multi-language parts.
        /// </para>
        /// </remarks>
        public CultureInfo CultureInfo
        {
            get
            {
                return _culture;
            }
            set
            {
                if (value != null)
                {
                    _culture = value;

                    this.OnSettingsChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value to indicate turning off viewbox at the root of the drawing.
        /// </summary>
        /// <value>
        /// For image outputs, this will force the original size to be saved.
        /// <para>
        /// The default value is <see langword="false"/>.
        /// </para>
        /// </value>
        /// <remarks>
        /// There are reported cases where are diagrams displayed in Inkscape program, but will not
        /// show when converted. These are diagrams on the drawing canvas of Inkspace but outside 
        /// the svg viewbox. 
        /// <para>
        /// When converted the drawings are also converted but not displayed due to
        /// clipping. Setting this property to <see langword="true"/> will clear the clipping region
        /// on conversion.
        /// </para>
        /// </remarks>
        public bool IgnoreRootViewbox
        {
            get
            {
                return _ignoreRootViewbox;
            }
            set
            {
                _ignoreRootViewbox = value;

                this.OnAutoSizeChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value to indicate preserving the original viewbox size when saving images.
        /// </summary>
        /// <value>
        /// For image outputs, this will force the original size to be saved.
        /// <para>
        /// The default value is <see langword="false"/>. However, the ImageSvgConverter converted
        /// sets this to <see langword="true"/> by default.
        /// </para>
        /// </value>
        /// <remarks>
        /// Setting this to <see langword="true"/> will cause the rendering process to draw a transparent
        /// box around the output, if a viewbox is defined. This will ensure that the original image
        /// size is saved.
        /// </remarks>
        public bool EnsureViewboxSize
        {
            get
            {
                return _ensureViewboxSize;
            }
            set
            {
                _ensureViewboxSize = value;

                this.OnAutoSizeChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value to indicate applying a translate transform to the viewbox to ensure
        /// it is visible when rendered.
        /// </summary>
        /// <value>
        /// This determines whether a transformation is applied to the rendered drawing. For drawings
        /// where the top-left position of the viewbox is off the screen, due to negative values, this
        /// will ensure the drawing is visible.
        /// <para>
        /// The default value is <see langword="true"/>. Set this value to <see langword="false"/> if
        /// you wish to apply your own transformations to the drawings.
        /// </para>
        /// </value>
        public bool EnsureViewboxPosition
        {
            get
            {
                return _ensureViewboxPosition;
            }
            set
            {
                _ensureViewboxPosition = value;

                this.OnAutoSizeChanged();
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

        #region Public Methods
        public void AddDrawing(DrawingGroup drawing, Rect maxBounds) =>
            this.OnLoadDrawing(drawing, maxBounds);
        #endregion

        #region Protected Methods

        /// <summary>
        /// Raises the Initialized event. This method is invoked whenever IsInitialized is set to true.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
        protected override void OnInitialized(EventArgs e)
        {
            if (this.Child != _drawingCanvas)
            {
                this.Child = _drawingCanvas;
            }

            base.OnInitialized(e);

            /*if (_sourceUri != null || _sourceStream != null || !string.IsNullOrWhiteSpace(_sourceSvg))
            {
                if (_svgDrawing == null)
                {
                    DrawingGroup drawing = this.CreateDrawing();
                    if (drawing != null)
                    {
                        this.OnLoadDrawing(drawing);
                    }
                }
            }*/
        }

        /// <summary>
        /// This handles changes in the rendering settings of this control.
        /// </summary>
        protected virtual void OnSettingsChanged()
        {
            if (!this.IsInitialized || string.IsNullOrWhiteSpace(_sourceXaml))
                return;

            string source = _sourceXaml;

#if DEBUG
            if (DesignMode)
            {
                if (source.StartsWith("."))
                    source = File.ReadAllText(@".\assetsDesignPath.txt") + source.Remove(0, 1);
            }
#endif

            using StreamReader streamReader = new(source);
            DrawingGroup drawing = (DrawingGroup)XamlReader.Load(streamReader.BaseStream);

            if (drawing != null)
                this.OnLoadDrawing(drawing, drawing.Bounds);
        }

        /// <summary>
        /// This handles changes in the automatic resizing property of this control.
        /// </summary>
        protected virtual void OnAutoSizeChanged()
        {
            if (_autoSize)
            {
                if (this.IsInitialized && _svgDrawing != null)
                {
                    Rect rectDrawing = _svgDrawing.Bounds;
                    if (!rectDrawing.IsEmpty)
                    {
                        this.Width = rectDrawing.Width;
                        this.Height = rectDrawing.Height;

                        _isAutoSized = true;
                    }
                }
            }
            else
            {
                if (_isAutoSized)
                {
                    this.Width = double.NaN;
                    this.Height = double.NaN;
                }
            }
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

        private void OnLoadDrawing(DrawingGroup drawing, Rect maxBounds)
        {
            try
            {
                if (drawing == null)
                    return;

                // Clear the current drawing
                this.OnUnloadDiagram();

                // Allow the contained canvas to render the object.
                _drawingCanvas.RenderDiagrams(drawing, maxBounds);

                // Pass any tooltip object to the contained canvas
                _drawingCanvas.ToolTip = this.ToolTip;

                // Keep an instance of the current drawing
                _svgDrawing = drawing;

                // Finally, force a resize of the viewbox
                this.OnAutoSizeChanged();
            }
            catch (Exception ex)
            {
                this.OnHandleError(null, ex);
            }
        }

        private void OnUnloadDiagram()
        {
            try
            {
                if (_drawingCanvas != null)
                {
                    _drawingCanvas.UnloadDiagrams();

                    if (_isAutoSized)
                    {
                        this.Width = double.NaN;
                        this.Height = double.NaN;
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnHandleError(null, ex);
            }
        }

        private static void OnXamlSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is not CustomSvgViewbox viewbox)
                return;

            viewbox._sourceXaml = (string)args.NewValue;
            if (string.IsNullOrWhiteSpace(viewbox._sourceXaml))
                viewbox.OnUnloadDiagram();
            else
                viewbox.OnSettingsChanged();
        }

        #endregion

        #region IUriContext Members

        /// <summary>
        /// Gets or sets the base URI of the current application context.
        /// </summary>
        /// <value>
        /// The base URI of the application context.
        /// </value>
        public Uri BaseUri
        {
            get
            {
                return _baseUri;
            }
            set
            {
                _baseUri = value;
            }
        }

        #endregion

        #region ISvgControl Members

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

        int ISvgControl.Width
        {
            get
            {
                return (int)this.ActualWidth;
            }
        }

        int ISvgControl.Height
        {
            get
            {
                return (int)this.ActualHeight;
            }
        }

        void ISvgControl.HandleAlert(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || this.DesignMode)
            {
                return;
            }
            this.OnHandleAlert(message);
        }

        void ISvgControl.HandleError(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || this.DesignMode)
            {
                return;
            }
            this.OnHandleError(message, null);
        }

        void ISvgControl.HandleError(Exception exception)
        {
            if (exception == null || this.DesignMode)
            {
                return;
            }
            this.OnHandleError(null, exception);
        }

        void ISvgControl.HandleError(string message, Exception exception)
        {
            if ((string.IsNullOrWhiteSpace(message) && exception == null) || this.DesignMode)
            {
                return;
            }
            this.OnHandleError(message, exception);
        }

        #endregion
    }
}