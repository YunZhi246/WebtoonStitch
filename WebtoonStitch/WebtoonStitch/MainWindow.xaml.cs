using System;
using System.Collections.Generic;
using System.Drawing;
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

        public MainWindow()
        {
            InitializeComponent();
            resetStart();
        }

        public string Directory { get; private set; }

        private void StartWorking()
        {
            List<string> fileNames = new List<string>();
            this.Directory = ChooseFolder();

            if (this.Directory != string.Empty)
            {
                string[] files = System.IO.Directory.GetFiles(this.Directory, "*.jpg");
                fileNames = files.ToList<string>();
            }

            masterFileNames = new SortedList<string, string>();
            foreach (string fileName in fileNames)
            {
                int lastIndex = fileName.LastIndexOf(@"\");
                masterFileNames.Add(fileName.Substring(lastIndex + 1), fileName);
            }

            this.FilesListBox.ItemsSource = masterFileNames.Keys.ToList();

            //visibilities
            this.StartButton.Visibility = Visibility.Hidden;
            this.FilesListBox.Visibility = Visibility.Visible;
            this.StitchButton.Visibility = Visibility.Visible;
            this.CancelButton.Visibility = Visibility.Visible;

        }
        private string ChooseFolder()
        {
            string path = string.Empty;
            FolderBrowserDialog chooseFolder = new FolderBrowserDialog();
            chooseFolder.ShowNewFolderButton = false;
            chooseFolder.RootFolder = Environment.SpecialFolder.MyComputer;
            chooseFolder.SelectedPath = @"Z:\new";
            DialogResult result = chooseFolder.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                path = chooseFolder.SelectedPath;
            }

            return path;
        }

        private int StitchImages()
        {
            List<string> keys = (List<string>)FilesListBox.ItemsSource;
            List<System.Drawing.Image> images = new List<System.Drawing.Image>();
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
                    totalHeight += img.Height;
                    if (img.Width > finalWidth) { finalWidth = img.Width; }
                }
            }
            Bitmap bitmap = new Bitmap(finalWidth, totalHeight);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                int yPos = 0;
                foreach(System.Drawing.Image img in images)
                {
                    g.DrawImage(img, 0, yPos);
                    yPos += img.Height;
                }
            }
            finalImage = bitmap;
            return totalHeight;
         //   bitmap.Save(this.Directory+"\\FINALIMAGE.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);


            /*
            Bitmap bitmap = new Bitmap(image1.Width + image2.Width, Math.Max(image1.Height, image2.Height));
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(image1, 0, 0);
                g.DrawImage(image2, image1.Width, 0);
            }
            */
        }


        //*********BUTTON CLICKS*********
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            this.StartWorking();
        }
        private void StitchButton_Click(object sender, RoutedEventArgs e)
        {
            int height = StitchImages();

            MemoryStream ms = new MemoryStream();
            finalImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            BitmapImage bImg = new BitmapImage();
            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(ms.ToArray());
            bImg.EndInit();
            PreviewImage.Source = bImg;
            PreviewImage.Height = height;
            
            FilesListBox.Visibility = Visibility.Hidden;
            StitchButton.Visibility = Visibility.Hidden;
            CancelButton.Visibility = Visibility.Hidden;
            PreviewViewbox.Visibility = Visibility.Visible;
            PreviewScrollViewer.Visibility = Visibility.Visible;
            SaveButton.Visibility = Visibility.Visible;
            PreviewImage.Visibility = Visibility.Visible;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            resetStart();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            finalImage.Save(this.Directory + "\\FINALIMAGE.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            resetStart();
        }

        private void resetStart()
        {
            this.StartButton.Visibility = Visibility.Visible;
            this.FilesListBox.Visibility = Visibility.Hidden;
            this.StitchButton.Visibility = Visibility.Hidden;
            this.CancelButton.Visibility = Visibility.Hidden;
            PreviewViewbox.Visibility = Visibility.Hidden;
            PreviewScrollViewer.Visibility = Visibility.Hidden;
            SaveButton.Visibility = Visibility.Hidden;
            PreviewImage.Visibility = Visibility.Hidden;
        }
        
    }
}
