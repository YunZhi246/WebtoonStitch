using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebtoonStitch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SortedList<string, string> masterFileNames;
        private Bitmap finalImage;
        private String view = "START";

        public MainWindow()
        {
            InitializeComponent();
            updateView();
        }

        public string Directory { get; private set; }

        private bool StartWorking()
        {
            List<string> fileNames = new List<string>();
            this.Directory = ChooseFolder();

            if (this.Directory == string.Empty)
            {
               // System.Windows.MessageBox.Show("Error please try again!");
                return false;
            }

            //FIX THIS LATER, SKETCHY WAY
            string[] files = System.IO.Directory.GetFiles(this.Directory, "*.jpg");
            fileNames = files.ToList<string>();
            masterFileNames = new SortedList<string, string>();
            foreach (string fileName in fileNames)
            {
                int lastIndex = fileName.LastIndexOf(@"\");
                masterFileNames.Add(fileName.Substring(lastIndex + 1), fileName);
            }
            files = System.IO.Directory.GetFiles(this.Directory, "*.png");
            fileNames = files.ToList<string>();
            foreach (string fileName in fileNames)
            {
                int lastIndex = fileName.LastIndexOf(@"\");
                masterFileNames.Add(fileName.Substring(lastIndex + 1), fileName);
            }

            List<String> sortedNames = masterFileNames.Keys.ToList();
            sortedNames.Sort();
            this.FilesListBox.ItemsSource = sortedNames;

            return true;
        }
        private string ChooseFolder()
        {
            string path = string.Empty;
            FolderBrowserDialog chooseFolder = new FolderBrowserDialog();
            chooseFolder.ShowNewFolderButton = false;
            chooseFolder.RootFolder = Environment.SpecialFolder.MyComputer;
            chooseFolder.SelectedPath = ""/*@"Z:\new"*/;
            DialogResult result = chooseFolder.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                path = chooseFolder.SelectedPath;
            }

            return path;
        }

        private void StitchImages()
        {
            List<string> keys = (List<string>)FilesListBox.ItemsSource;
            List<System.Drawing.Image> images = new List<System.Drawing.Image>();
            List<System.Drawing.Image> resizedImages = new List<System.Drawing.Image>();
            int totalHeight = 0;
            int finalWidth = 0;
            foreach(string key in keys)
            {
                string filename = string.Empty;
                bool isThere = masterFileNames.TryGetValue(key, out filename);
                if (isThere)
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(filename);
                    images.Add(img);
                    if (img.Width > finalWidth) { finalWidth = img.Width; }
                }
            }
            if ((bool)CustomWidthCheckBox.IsChecked)
            {
                int temp;
                bool isInt = int.TryParse(CustomWidthTextBox.Text, out temp);
                if (isInt)
                {
                    finalWidth = temp;
                }
                else
                {
                    System.Windows.MessageBox.Show("Width entered not an integer, will default to largest width.");
                }
            }

            foreach (System.Drawing.Image img in images)
            {
                System.Drawing.Image rImg = ResizeImage(img, finalWidth);
                totalHeight += rImg.Height;
                resizedImages.Add(rImg);
            }
            
            Bitmap bitmap = new Bitmap(finalWidth, totalHeight);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                int yPos = 0;
                foreach(System.Drawing.Image img in resizedImages)
                {
                    g.DrawImage(img, 0, yPos);
                    yPos += img.Height;
                }
            }
            finalImage = bitmap;
        }

        public static System.Drawing.Image ResizeImage(System.Drawing.Image image, int width)
        {
            float fhei = ((float)image.Height * ((float)width / (float)image.Width));
            int height = (int)fhei;
            if (image.Height == height && image.Width == width) return image;

            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

    //        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void displayPreview()
        {
            System.Drawing.Image displayImage = ResizeImage(finalImage, 700);
            MemoryStream ms = new MemoryStream();
            displayImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            BitmapImage bImg = new BitmapImage();
            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(ms.ToArray());
            bImg.EndInit();
            PreviewImage.Source = bImg;
            PreviewImage.Height = displayImage.Height;
      //      PreviewImage.Width = finalImage.Width - 50;
        }

        private void updateView()
        {
            StartGrid.Visibility = Visibility.Hidden;
            ShowFilesGrid.Visibility = Visibility.Hidden;
            PreviewGrid.Visibility = Visibility.Hidden;

            if (view == "START")
            {
                StartGrid.Visibility = Visibility.Visible;
            }
            else if (view == "FILES")
            {
                ShowFilesGrid.Visibility = Visibility.Visible;
            }
            else if (view == "PREVIEW")
            {
                PreviewGrid.Visibility = Visibility.Visible;
            }
        }


        //*********BUTTON CLICKS*********
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.StartWorking())
            {
                view = "FILES";
                updateView();
            }
        }
        private void StitchButton_Click(object sender, RoutedEventArgs e)
        {
            StitchImages();
            displayPreview();
            view = "PREVIEW";
            updateView();
        }

        private void FilesCancelButton_Click(object sender, RoutedEventArgs e)
        {
            view = "START";
            updateView();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            finalImage.Save(this.Directory + "\\FINALIMAGE.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            view = "START";
            updateView();
        }
        private void PreviewCancelButton_Click(object sender, RoutedEventArgs e)
        {
            view = "START";
            updateView();
        }

        private void CustomWidthCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)CustomWidthCheckBox.IsChecked)
            {
                CustomWidthTextBox.IsEnabled = true;
            }
            else
            {
                CustomWidthTextBox.IsEnabled = false;
            }
        }
    }
}
