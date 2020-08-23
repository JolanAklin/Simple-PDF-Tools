//modified code from Lander-Verhack
//https://blogs.u2u.be/lander/post/2018/01/23/Creating-a-PDF-Viewer-in-WPF-using-Windows-10-APIs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private List<PdfPageImage> currentlyLoadedImages = new List<PdfPageImage>();
        private PdfDocument pdfDocument;
        private uint imageIndex = 0;


        public PdfViewer()
        {
            InitializeComponent();
            pdfPath = @"C:/testpdf.pdf";
            
        }

        public void StartRender()
        {
            pdfWidth = (uint)this.ActualWidth;
            RenderPdf();
        }

        private void RenderPdf()
        {
            if (!string.IsNullOrEmpty(this.pdfPath))
            {
                //making sure it's an absolute path
                var path = System.IO.Path.GetFullPath(this.pdfPath);

                StorageFile.GetFileFromPathAsync(path).AsTask()
                  //load pdf document on background thread
                  .ContinueWith(t => PdfDocument.LoadFromFileAsync(t.Result).AsTask()).Unwrap()
                  //display on UI Thread
                  .ContinueWith(t2 => PdfToImages(this, t2.Result), TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private async Task PdfToImages(PdfViewer pdfViewer, PdfDocument pdfDoc)
        {
            var items = pdfViewer.PagesContainer.Items;
            items.Clear();
            pdfDocument = pdfDoc;

            if (pdfDoc == null) return;

            PdfPageImage pdfimage = new PdfPageImage(imageIndex, pdfDocument.GetPage(imageIndex), pdfWidth);
            imageIndex++;
            await pdfimage.GenerateImage();
            items.Add(pdfimage.ResultImage);
            currentlyLoadedImages.Add(pdfimage);
            Console.WriteLine(pdfDocument.GetPage(0).Size.Width + " x " + pdfDocument.GetPage(0).Size.Height);
            Console.WriteLine(pdfimage.BmpImage.Width + " x " + pdfimage.BmpImage.Height);
        }

        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            uint scrollHeightViewport = (uint)this.ActualHeight;
            Console.WriteLine(e.VerticalOffset);
            uint pagesTotalHeight = 0;
            foreach (PdfPageImage image in currentlyLoadedImages)
            {
                Image pdfImage = image.ResultImage;
                pagesTotalHeight += (uint)(pdfImage.ActualHeight + pdfImage.Margin.Top + pdfImage.Margin.Bottom);
            }
            if (e.VerticalOffset + scrollHeightViewport > pagesTotalHeight)
            {
                if (pdfDocument != null)
                {
                    if(imageIndex < pdfDocument.PageCount)
                    {
                        PdfPageImage pdfimage = new PdfPageImage(imageIndex, pdfDocument.GetPage(imageIndex), pdfWidth);
                        imageIndex++;
                        await pdfimage.GenerateImage();
                        var items = this.PagesContainer.Items;
                        items.Add(pdfimage.ResultImage);
                        currentlyLoadedImages.Add(pdfimage);
                    }

                }
            }
        }
    }
}
