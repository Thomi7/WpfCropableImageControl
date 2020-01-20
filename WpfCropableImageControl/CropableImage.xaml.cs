using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfCropableImageControl
{
    public partial class CropableImage : UserControl, INotifyPropertyChanged
    {
        #region Properties

        #region Bitmaps

        public static readonly DependencyProperty BitmapsProperty = DependencyProperty.Register(nameof(Bitmaps), typeof(Array), typeof(CropableImage), new PropertyMetadata(Bitmaps_Changed));
        public Array Bitmaps
        {
            get => (BitmapSource[])GetValue(BitmapsProperty);
            set => SetValue(BitmapsProperty, value);
        }
        private static void Bitmaps_Changed(object sender, DependencyPropertyChangedEventArgs e)
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
                cropableImage.CropHeight_Verify();
            }
        }

        #endregion

        #region CropHeight

        // the desired CropHeight (from User)
        public static DependencyProperty CropHeightProperty = DependencyProperty.Register(nameof(CropHeight), typeof(int), typeof(CropableImage), new PropertyMetadata(-1, CropHeight_Changed));
        public int CropHeight
        {
            get => (int)GetValue(CropHeightProperty);
            set => SetValue(CropHeightProperty, value);
        }
        private static void CropHeight_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CropableImage cropableImage && e.OldValue != e.NewValue)
                cropableImage.CropHeight_Verify();
        }

        // the actual verified CropHeight
        private int _actualCropHeight;
        public int ActualCropHeight
        {
            get => _actualCropHeight;
            set
            {
                if (value != _actualCropHeight)
                {
                    _actualCropHeight = value;
                    OnPropertyChanged("ActualCropHeight");
                }
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
                cropableImage.CropWidth_Verify();
            }
        }

        #endregion

        #region CropWidth

        // the desired CropWidth (from User)
        public static DependencyProperty CropWidthProperty = DependencyProperty.Register(nameof(CropWidth), typeof(int), typeof(CropableImage), new PropertyMetadata(-1, CropWidth_Changed));
        public int CropWidth
        {
            get => (int)GetValue(CropWidthProperty);
            set => SetValue(CropWidthProperty, value);
        }
        private static void CropWidth_Changed(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CropableImage cropableImage && e.OldValue != e.NewValue)
                cropableImage.CropWidth_Verify();
        }

        // the actual verified CropHeight
        private int _actualCropWidth;
        public int ActualCropWidth
        {
            get => _actualCropWidth;
            set
            {
                if (value != _actualCropWidth)
                {
                    _actualCropWidth = value;
                    OnPropertyChanged("ActualCropWidth");
                }
            }
        }

        #endregion

        #endregion

        private int BigImageHeight;

        private int _bigImageWidth;
        public int BigImageWidth
        {
            get => _bigImageWidth;
            set
            {
                if (value != _bigImageWidth)
                {
                    _bigImageWidth = value;
                    OnPropertyChanged("BigImageWidth");
                }
            }
        }

        private void CropHeight_Verify()
        {
            if (CropHeight != -1)
            {
                if (CropHeight > BigImageHeight - ShiftY)
                    ActualCropHeight = BigImageHeight - ShiftY;
                else if (CropHeight < 0)
                    ActualCropHeight = 0;
                else
                    ActualCropHeight = CropHeight;
            }
            else
                ActualCropHeight = BigImageHeight - ShiftY;
        }

        private void CropWidth_Verify()
        {
            if (CropWidth != -1)
            {
                if (CropWidth > BigImageWidth - ShiftX)
                    ActualCropWidth = BigImageWidth - ShiftX;
                else if (CropWidth < 0)
                    ActualCropWidth = 0;
                else
                    ActualCropWidth = CropWidth;
            }
            else
                ActualCropWidth = BigImageWidth - ShiftX;
        }

        public CropableImage()
        {
            DataContext = this;
            InitializeComponent();

            MainGrid.RenderTransform = new TranslateTransform();
        }

        private void UpdateGrid()
        {
            if (Bitmaps != null)
            {
                MainGrid.Children.Clear();
                MainGrid.RowDefinitions.Clear();
                MainGrid.ColumnDefinitions.Clear();

                var pixelHeight = 0;
                var pixelWidth = 0;
                foreach (var b in Bitmaps)
                {
                    var image = b as BitmapImage;
                    pixelHeight += image.PixelWidth;
                    if (pixelWidth < image.PixelWidth)
                        pixelWidth = image.PixelWidth;
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

                var endHeight = 0;
                foreach (var r in horizontalImageParts[0])
                    endHeight += r.Height;

                var endWidth = 0;
                foreach (var c in horizontalImageParts)
                    endWidth += c[0].Width;

                BigImageHeight = endHeight;
                if (CropHeight == -1)
                    ActualCropHeight = BigImageHeight;

                BigImageWidth = endWidth;
                if (CropWidth == -1)
                    ActualCropWidth = BigImageWidth;

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
            }
        }

        private void InitializeBigImage(int row, int column, ImagePart imagePart)
        {
            var s = new StackPanel() { Width = imagePart.Width };

            //var st = new UserControl() {  };

            var shiftStackPanel = new StackPanel();
            foreach (var b in Bitmaps)
                shiftStackPanel.Children.Add(new Image() { Source = b as BitmapImage });

            var sizeStackPanel = new StackPanel() { Height = imagePart.Height, Width = 100 };
            sizeStackPanel.Children.Add(shiftStackPanel);

            //st.Child = sizeStackPanel;
            //st.Content = sizeStackPanel;
            s.Children.Add(sizeStackPanel);
            //s.Content = st;

            MainGrid.Children.Add(s);
            Grid.SetRow(s, row);
            Grid.SetColumn(s, column);

            shiftStackPanel.RenderTransform = new TranslateTransform() { Y = -imagePart.Y, X = -imagePart.X };
        }

        private class ImagePart
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }

        private int GridColumns
        {
            get => MainGrid.ColumnDefinitions.Count - 1;
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
