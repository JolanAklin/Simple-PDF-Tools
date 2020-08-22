//modified code from Lander-Verhack
//https://blogs.u2u.be/lander/post/2018/01/23/Creating-a-PDF-Viewer-in-WPF-using-Windows-10-APIs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace simplePDFTools
{
    /// <summary>
    /// Logique d'interaction pour PdfViewer.xaml
    /// </summary>
    public partial class PdfViewer : UserControl
    {

        public string pdfPath;
        public static uint pdfWidth;
        private string path;
        private static PdfDocument pdfDocument;
        private static List<Image> currentlyLoadedImages = new List<Image>();


        public PdfViewer()
        {
            InitializeComponent();
            pdfPath = @"C:/testpdf.pdf";
            
        }

        public void StartRender()
        {
            pdfWidth = (uint)this.ActualWidth;
            //RenderPdf();
            PrepareForPdfLoad();
        }

        private void PrepareForPdfLoad()
        {
            if (!string.IsNullOrEmpty(this.pdfPath))
            {
                //making sure it's an absolute path
                path = System.IO.Path.GetFullPath(this.pdfPath);

                StorageFile.GetFileFromPathAsync(path).AsTask()
                  //load pdf document on background thread
                  .ContinueWith(t => PdfDocument.LoadFromFileAsync(t.Result).AsTask()).Unwrap()
                  //display on UI Thread
                  .ContinueWith(t2 => PrepareImage(this, t2.Result), TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private async static Task PrepareImage(PdfViewer pdfViewer, PdfDocument pdfDoc)
        {
            pdfDocument = pdfDoc;
            var items = pdfViewer.PagesContainer.Items;
            items.Clear();

            if (pdfDoc == null) return;

            for (uint i = 0; i < pdfDoc.PageCount; i++)
            {
                using (var page = pdfDoc.GetPage(i))
                {
                    var image = new Image
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(4, 4, 4, 4),
                        MaxWidth = pdfWidth,
                    };
                    items.Add(image);
                }
            }
        }

        private async static Task ShowPage(uint index, Image imageToRender)
        {
            using (var page = pdfDocument.GetPage(index))
            {
                //BitmapImage bitmap = await PageToBitmapAsync(page);

                BitmapImage image = new BitmapImage();

                using (var stream = new InMemoryRandomAccessStream())
                {
                    PdfPageRenderOptions pdfRenderOption = new PdfPageRenderOptions();
                    pdfRenderOption.DestinationWidth = pdfWidth;
                    await page.RenderToStreamAsync(stream, pdfRenderOption);

                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream.AsStream();
                    image.EndInit();
                }

                imageToRender.Source = image;
                currentlyLoadedImages.Add(imageToRender);
            }
        }

        private static async Task<BitmapImage> PageToBitmapAsync(PdfPage page)
        {
            BitmapImage image = new BitmapImage();

            using (var stream = new InMemoryRandomAccessStream())
            {
                PdfPageRenderOptions pdfRenderOption = new PdfPageRenderOptions();
                pdfRenderOption.DestinationWidth = pdfWidth;
                await page.RenderToStreamAsync(stream,pdfRenderOption);

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream.AsStream();
                image.EndInit();
            }

            return image;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            uint scrollHeightViewport = (uint)this.ActualHeight;
            Console.WriteLine(e.VerticalOffset);
            uint pagesTotalHeight = 0;
            foreach (Image image in currentlyLoadedImages)
            {
                pagesTotalHeight += (uint)(image.ActualHeight + image.Margin.Top + image.Margin.Bottom);
            }
            if (e.VerticalOffset + scrollHeightViewport > pagesTotalHeight)
            {
                if(!this.PagesContainer.Items.IsEmpty)
                {
                    uint imageIndex = (uint)currentlyLoadedImages.Count;
                    Image renderimage = (Image)this.PagesContainer.Items[(int)imageIndex];
                    AsyncShowImage();

                }
            }
        }

        private async void AsyncShowImage()
        {
            uint imageIndex = (uint)currentlyLoadedImages.Count;
            Image renderimage = (Image)this.PagesContainer.Items[(int)imageIndex];
            await ShowPage(imageIndex, renderimage);
        }
    }
}
