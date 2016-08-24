// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************
// The Control ImageEx is from the project https://github.com/Microsoft/UWPCommunityToolkit/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Casting;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace JacobC.Xiami.Controls
{
    /// <summary>
    /// The ImageEx control extends the default Image platform control improving the performance and responsiveness of your Apps.
    /// Source images are downloaded asynchronously showing a load indicator while in progress.
    /// Once downloaded, the source image is stored in the App local cache to preserve resources and load time next time the image needs to be displayed.
    /// </summary>
    [TemplateVisualState(Name = "Loading", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Loaded", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Unloaded", GroupName = "CommonStates")]
    [TemplatePart(Name = "Image", Type = typeof(Image))]
    [TemplatePart(Name = "PlaceholderImage", Type = typeof(Image))]
    [TemplatePart(Name = "Progress", Type = typeof(ProgressRing))]
    public sealed partial class ImageEx : Control
    {
        private Image _image;
        private Image _placeholderImage;
        private ProgressRing _progress;

        private bool _isInitialized;

        #region Constructor And Overrides

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEx"/> class.
        /// </summary>
        public ImageEx()
        {
            DefaultStyleKey = typeof(ImageEx);
            Loaded += OnLoaded;
        }

        /// <summary>
        /// Update the visual state of the control when its template is changed.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            if (_image != null)
            {
                _image.ImageOpened -= OnImageOpened;
                _image.ImageFailed -= OnImageFailed;
            }

            _image = GetTemplateChild("Image") as Image;
            _placeholderImage = GetTemplateChild("PlaceholderImage") as Image;
            _progress = GetTemplateChild("Progress") as ProgressRing;

            _isInitialized = true;

            SetSource(Source);

            if (_image != null)
            {
                _image.ImageOpened += OnImageOpened;
                _image.ImageFailed += OnImageFailed;
            }

            base.OnApplyTemplate();
        }

        /// <summary>
        /// Measures the size in layout required for child elements and determines a size for the control.
        /// </summary>
        /// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
        /// <returns>The size that this element determines it needs during layout, based on its calculations of child element sizes.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            _progress.Width = _progress.Height = Math.Min(1024, Math.Min(availableSize.Width, availableSize.Height)) / 8.0;
            return base.MeasureOverride(availableSize);
        }

        private void OnImageOpened(object sender, RoutedEventArgs e)
        {
            ImageOpened?.Invoke(this, e);
        }

        private void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ImageFailed?.Invoke(this, e);
            VisualStateManager.GoToState(this, "Failed", true);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_image != null && _image.Source == null)
            {
                RefreshImage();
            }
        }

        private async void RefreshImage()
        {
            await LoadImageAsync();
        }

        #endregion

        #region Source Related
        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(object), typeof(ImageEx), new PropertyMetadata(null, SourceChanged));

        private Uri _uri;
        private bool _isHttpSource;
        private bool _isLoadingImage;

        /// <summary>
        /// Gets or sets get or set the source used by the image
        /// </summary>
        public object Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ImageEx;
            control?.SetSource(e.NewValue);
        }

        private static bool IsHttpUri(Uri uri)
        {
            return uri.IsAbsoluteUri && (uri.Scheme == "http" || uri.Scheme == "https");
        }

        private async void SetSource(object source)
        {
            if (_isInitialized)
            {
                _image.Source = null;

                if (source == null)
                {
                    VisualStateManager.GoToState(this, "Unloaded", true);
                    return;
                }

                VisualStateManager.GoToState(this, "Loading", true);

                if (source is string)
                {
                    string url = source as string;
                    if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _uri))
                    {
                        _isHttpSource = IsHttpUri(_uri);
                        if (!_isHttpSource && !_uri.IsAbsoluteUri)
                        {
                            _uri = new Uri("ms-appx:///" + url.TrimStart('/'));
                        }

                        await LoadImageAsync();
                    }
                }
                else if (source is Uri)
                {
                    _uri = source as Uri;
                    await LoadImageAsync();
                }
                else
                {
                    var sourcelink = source as Uri;
                    if (sourcelink != null)

                        _image.Source = source as ImageSource;
                }

                VisualStateManager.GoToState(this, "Loaded", true);
            }
        }

        private async Task LoadImageAsync()
        {
            if (!_isLoadingImage && _uri != null)
            {
                _isLoadingImage = true;
                if (IsCacheEnabled && _isHttpSource)
                {
                    _image.Source = await ImageCache.GetFromCacheAsync(_uri);
                }
                else
                {
                    _image.Source = new BitmapImage(_uri);
                }

                _isLoadingImage = false;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Identifies the <see cref="PlaceholderSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PlaceholderSourceProperty = DependencyProperty.Register(
            "PlaceholderSource",
            typeof(ImageSource),
            typeof(ImageEx),
            new PropertyMetadata(default(ImageSource)));

        /// <summary>
        /// Identifies the <see cref="PlaceholderStretch"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PlaceholderStretchProperty = DependencyProperty.Register(
            "PlaceholderStretch",
            typeof(Stretch),
            typeof(ImageEx),
            new PropertyMetadata(default(Stretch)));

        /// <summary>
        /// Identifies the <see cref="PlaceholderAnimationDuration"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PlaceholderAnimationDurationProperty =
            DependencyProperty.Register(
                "PlaceholderAnimationDuration",
                typeof(Duration),
                typeof(ImageEx),
                new PropertyMetadata(TimeSpan.Zero));

        /// <summary>
        /// Gets or sets the placeholder source.
        /// </summary>
        /// <value>
        /// The placeholder source.
        /// </value>
        public ImageSource PlaceholderSource
        {
            get { return (ImageSource)GetValue(PlaceholderSourceProperty); }
            set { SetValue(PlaceholderSourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the placeholder stretch.
        /// </summary>
        /// <value>
        /// The placeholder stretch.
        /// </value>
        public Stretch PlaceholderStretch
        {
            get { return (Stretch)GetValue(PlaceholderStretchProperty); }
            set { SetValue(PlaceholderStretchProperty, value); }
        }

        /// <summary>
        /// Gets or sets the placeholder animation duration.
        /// </summary>
        /// <value>
        /// The placeholder animation duration.
        /// </value>
        public Duration PlaceholderAnimationDuration
        {
            get { return (Duration)GetValue(PlaceholderAnimationDurationProperty); }
            set { SetValue(PlaceholderAnimationDurationProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="NineGrid"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NineGridProperty = DependencyProperty.Register("NineGrid", typeof(Thickness), typeof(ImageEx), new PropertyMetadata(default(Thickness)));

        /// <summary>
        /// Identifies the <see cref="Stretch"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(ImageEx), new PropertyMetadata(Stretch.Uniform));

        /// <summary>
        /// Event raised if the image failed loading.
        /// </summary>
        public event ExceptionRoutedEventHandler ImageFailed;

        /// <summary>
        /// Event raised when the image is successfully loaded and opened.
        /// </summary>
        public event RoutedEventHandler ImageOpened;

        /// <summary>
        /// Gets or sets the stretch of the image.
        /// </summary>
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        /// <summary>
        /// Gets or sets the nine-grid used by the image.
        /// </summary>
        public Thickness NineGrid
        {
            get { return (Thickness)GetValue(NineGridProperty); }
            set { SetValue(NineGridProperty, value); }
        }

        /// <summary>
        /// Returns the image as a <see cref="CastingSource"/>.
        /// </summary>
        /// <returns>The image as a <see cref="CastingSource"/>.</returns>
        public CastingSource GetAsCastingSource()
        {
            return _isInitialized ? _image.GetAsCastingSource() : null;
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets cache state
        /// </summary>
        public bool IsCacheEnabled
        {
            get; set;
        }
        #endregion

    }
}
