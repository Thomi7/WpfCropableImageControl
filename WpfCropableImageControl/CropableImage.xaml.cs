using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfCropableImageControl
{
    public partial class CropableImage : UserControl, INotifyPropertyChanged
    {
        #region Properties

        #region Images

        public static readonly DependencyProperty ImagesProperty = DependencyProperty.Register(nameof(Images), typeof(Array), typeof(CropableImage), new PropertyMetadata(Images_Changed));
        public Array Images
        {
            get => (Array)GetValue(ImagesProperty);
            set => SetValue(ImagesProperty, value);
        }
        private static void Images_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CropableImage cropableImage && e.OldValue != e.NewValue)
                cropableImage.UpdateGrid();
        }

        #endregion

        #region Skips

        public static readonly DependencyProperty SkipsProperty = DependencyProperty.Register(nameof(Skips), typeof(ArrayList), typeof(CropableImage), new PropertyMetadata(Skips_Changed));
        public ArrayList Skips
        {
            get => (ArrayList)GetValue(SkipsProperty);
            set => SetValue(SkipsProperty, value);
        }
        private static void Skips_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CropableImage cropableImage && e.OldValue != e.NewValue)
                cropableImage.UpdateGrid();
        }

        #endregion

        #region CropHeight

        // the desired CropHeight (from User)
        public static DependencyProperty CropHeightProperty = DependencyProperty.Register(nameof(CropHeight), typeof(int?), typeof(CropableImage), new PropertyMetadata(CropHeight_Changed));
        public int? CropHeight
        {
            get => (int?)GetValue(CropHeightProperty);
            set => SetValue(CropHeightProperty, value);
        }
        private static void CropHeight_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CropableImage cropableImage && e.OldValue != e.NewValue)
                cropableImage.UpdateHeight();
        }

        private void UpdateHeight()
        {
            if (CropHeight.HasValue)
            {
                if (CropHeight.Value > BigImageHeight - ShiftY)
                {
                    if (BigImageHeight - ShiftY >= 0)
                        ActualCropHeight = BigImageHeight - ShiftY;
                    else
                        ActualCropHeight = 0;
                }
                else if (CropHeight.Value < 0)
                    ActualCropHeight = 0;
                else
                    ActualCropHeight = CropHeight.Value;
            }
            else
            {
                if (BigImageHeight - ShiftY >= 0)
                    ActualCropHeight = BigImageHeight - ShiftY;
                else
                    ActualCropHeight = 0;
            }
        }

        // the actual verified CropHeight
        private int _actualCropHeight;
        public int ActualCropHeight
        {
            get => _actualCropHeight;
            private set
            {
                if (value != _actualCropHeight)
                {
                    _actualCropHeight = value;
                    OnPropertyChanged("ActualCropHeight");
                }
            }
        }

        #endregion

        #region CropWidth

        // the desired CropWidth (from User)
        public static DependencyProperty CropWidthProperty = DependencyProperty.Register(nameof(CropWidth), typeof(int?), typeof(CropableImage), new PropertyMetadata(CropWidth_Changed));
        public int? CropWidth
        {
            get => (int?)GetValue(CropWidthProperty);
            set => SetValue(CropWidthProperty, value);
        }
        private static void CropWidth_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CropableImage cropableImage && e.OldValue != e.NewValue)
                cropableImage.UpdateWidth();
        }

        private void UpdateWidth()
        {
            if (CropWidth.HasValue)
            {
                if (CropWidth.Value > BigImageWidth - ShiftX)
                {
                    if (BigImageWidth - ShiftX >= 0)
                        ActualCropWidth = BigImageWidth - ShiftX;
                    else
                        ActualCropWidth = 0;
                }
                else if (CropWidth.Value < 0)
                    ActualCropWidth = 0;
                else
                    ActualCropWidth = CropWidth.Value;
            }
            else
            {
                if (BigImageWidth - ShiftX >= 0)
                    ActualCropWidth = BigImageWidth - ShiftX;
                else
                    ActualCropWidth = 0;
            }
        }

        // the actual verified CropWidth
        private int _actualCropWidth;
        public int ActualCropWidth
        {
            get => _actualCropWidth;
            private set
            {
                if (value != _actualCropWidth)
                {
                    _actualCropWidth = value;
                    OnPropertyChanged("ActualCropWidth");
                }
            }
        }

        #endregion

        #region ShiftY

        public static DependencyProperty ShiftYProperty = DependencyProperty.Register(nameof(ShiftY), typeof(int), typeof(CropableImage), new PropertyMetadata(0, ShiftY_Changed));
        public int ShiftY
        {
            get => (int)GetValue(ShiftYProperty);
            set => SetValue(ShiftYProperty, value);
        }
        private static void ShiftY_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CropableImage cropableImage && e.OldValue != e.NewValue)
            {
                (cropableImage.MainGrid.RenderTransform as TranslateTransform).Y = -(int)e.NewValue;
                cropableImage.UpdateHeight();
            }
        }

        #endregion

        #region ShiftX

        public static DependencyProperty ShiftXProperty = DependencyProperty.Register(nameof(ShiftX), typeof(int), typeof(CropableImage), new PropertyMetadata(0, ShiftX_Changed));
        public int ShiftX
        {
            get => (int)GetValue(ShiftXProperty);
            set => SetValue(ShiftXProperty, value);
        }
        private static void ShiftX_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CropableImage cropableImage && e.OldValue != e.NewValue)
            {
                (cropableImage.MainGrid.RenderTransform as TranslateTransform).X = -(int)e.NewValue;
                cropableImage.UpdateWidth();
            }
        }

        #endregion

        private int _bigImageHeight;
        public int BigImageHeight
        {
            get => _bigImageHeight;
            private set
            {
                if (value != _bigImageHeight)
                {
                    _bigImageHeight = value;
                    OnPropertyChanged("BigImageHeight");
                }
            }
        }

        private int _bigImageWidth;
        public int BigImageWidth
        {
            get => _bigImageWidth;
            private set
            {
                if (value != _bigImageWidth)
                {
                    _bigImageWidth = value;
                    OnPropertyChanged("BigImageWidth");
                }
            }
        }

        private int GridColumns
        {
            get => MainGrid.ColumnDefinitions.Count - 1;
        }

        #endregion

        public CropableImage()
        {
            InitializeComponent();
            MainViewBox.DataContext = this;

            MainGrid.RenderTransform = new TranslateTransform();
        }

        private void UpdateGrid()
        {
            if (Images != null)
            {
                MainGrid.Children.Clear();
                MainGrid.RowDefinitions.Clear();
                MainGrid.ColumnDefinitions.Clear();

                var pixelHeight = 0;
                var pixelWidth = 0;
                foreach (var b in Images)
                {
                    var image = b as BitmapSource;
                    pixelHeight += image.PixelHeight;
                    var width = image.PixelWidth;
                    if (pixelWidth < width)
                        pixelWidth = width;
                }

                var verticalImageParts = new List<ImagePart>();
                verticalImageParts.Add(new ImagePart() { X = 0, Y = 0, Height = pixelHeight, Width = pixelWidth });
                var horizontalImageParts = new List<List<ImagePart>>();
                horizontalImageParts.Add(verticalImageParts);
                if (Skips != null)
                {
                    // process vertical Splits
                    var skipCountY = 0;
                    int previousSkipEndY = 0;
                    foreach (Skip skip in Skips)
                    {
                        if (skip.SkipType == SkipType.Y)
                        {
                            var topImagePart = verticalImageParts[skipCountY];
                            if (previousSkipEndY != skip.SkipStart)
                            {
                                topImagePart.Height = skip.SkipStart - previousSkipEndY;

                                var imagePart = new ImagePart() { X = 0, Y = skip.SkipEnd, Height = pixelHeight - skip.SkipEnd, Width = pixelWidth };
                                skipCountY++;
                                verticalImageParts.Add(imagePart);
                                previousSkipEndY = imagePart.Y;
                            }
                            else
                            {
                                previousSkipEndY = skip.SkipEnd;
                                topImagePart.Y = skip.SkipEnd;
                            }
                        }
                    }

                    // process horizontal Splits
                    var skipCountX = 0;
                    var previousSkipEndX = 0;
                    foreach (Skip skip in Skips)
                    {
                        if (skip.SkipType == SkipType.X)
                        {
                            if (previousSkipEndX != skip.SkipStart)
                            {
                                var vList = new List<ImagePart>();
                                foreach (var verticalImagePart in horizontalImageParts[skipCountX])
                                {
                                    verticalImagePart.Width = skip.SkipStart - previousSkipEndX;

                                    var imagePart = new ImagePart() { X = skip.SkipEnd, Y = verticalImagePart.Y, Height = verticalImagePart.Height, Width = pixelWidth - skip.SkipEnd };
                                    vList.Add(imagePart);
                                }
                                previousSkipEndX = vList[0].X;
                                horizontalImageParts.Add(vList);
                                skipCountX++;
                            }
                            else
                            {
                                previousSkipEndX = skip.SkipEnd;
                                foreach (var verticalImagePart in horizontalImageParts[skipCountX])
                                    verticalImagePart.X = skip.SkipEnd;
                            }
                        }
                    }
                }

                // build final image as imagegrid
                foreach (var column in horizontalImageParts)
                {
                    MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                    var row = 0;
                    foreach (var imagePart in column)
                    {
                        MainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                        InitializeBigImage(row, GridColumns, imagePart);
                        row++;
                    }
                }

                var endHeight = 0;
                foreach (var r in horizontalImageParts[0])
                    endHeight += r.Height;

                var endWidth = 0;
                foreach (var c in horizontalImageParts)
                    endWidth += c[0].Width;

                // set height/width if no cropheight/cropwidth is specified
                BigImageHeight = endHeight;
                UpdateHeight();
                BigImageWidth = endWidth;
                UpdateWidth();
            }
        }

        private void InitializeBigImage(int row, int column, ImagePart imagePart)
        {
            var viewBox = new Viewbox { Width = imagePart.Width, Height = imagePart.Height, Stretch = Stretch.None, HorizontalAlignment = HorizontalAlignment.Left };

            var stackPanel = new StackPanel();
            foreach (var b in Images as BitmapSource[])
                stackPanel.Children.Add(new Image() { Source = b as BitmapSource, Width= b.PixelWidth, Height=b.PixelHeight });
            stackPanel.RenderTransform = new TranslateTransform() { Y = -imagePart.Y, X = -imagePart.X };

            viewBox.Child = stackPanel;
            MainGrid.Children.Add(viewBox);
            Grid.SetRow(viewBox, row);
            Grid.SetColumn(viewBox, column);
        }

        private class ImagePart
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }
    }

    public struct Skip
    {
        public SkipType _skipType;
        public SkipType SkipType
        {
            get
            {
                return _skipType;
            }
            set
            {
                if (_skipType != value)
                    _skipType = value;
            }
        }

        private int _skipStart;
        public int SkipStart
        {
            get
            {
                return _skipStart;
            }
            set
            {
                if (_skipStart != value)
                    _skipStart = value;
            }
        }

        private int _skipEnd;
        public int SkipEnd
        {
            get
            {
                return _skipEnd;
            }
            set
            {
                if (_skipEnd != value)
                    _skipEnd = value;
            }
        }
    }

    public enum SkipType
    {
        X, Y
    }
}
