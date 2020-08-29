//modified code from Lander-Verhack
//https://blogs.u2u.be/lander/post/2018/01/23/Creating-a-PDF-Viewer-in-WPF-using-Windows-10-APIs

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Xml;
using Windows.ApplicationModel.Chat;
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
        private List<PdfPageImage> pdfPageImages = new List<PdfPageImage>();
        private PdfDocument pdfDocument;

        private MainWindow mainWindow;

        //vars used for displaying the pdfpage
        //index of the next page to load
        private uint nextPdfPageIndex = 0;
        //position of the bottom with margin of the lastest page from the top
        private double currentPdfPageTotalHeight = 0;


        public PdfViewer()
        {
            InitializeComponent();
            pdfPath = @"C:/testpdf.pdf";
        }

        public void StartRender(MainWindow mainWindow)
        {
            pdfWidth = (uint)this.PagesContainer.ActualWidth;
            this.mainWindow = mainWindow;
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

            if (pdfDoc == null) return;
            PdfPageImage pdfimage = null;
            for (int i = 0; i < pdfDoc.PageCount; i++)
            {
                pdfimage = new PdfPageImage((uint)i, pdfDoc.GetPage((uint)i), pdfWidth);
                pdfimage.PreGenerateImage();
                pdfPageImages.Add(pdfimage);
                items.Add(pdfimage.ResultImage);
            }
            pdfDocument = pdfDoc;

            Image pdfImage = pdfPageImages[0].ResultImage;
            pdfimage = pdfPageImages[0];
            currentlyLoadedImages.Add(pdfimage);
            nextPdfPageIndex++;
            currentPdfPageTotalHeight += pdfImage.Height + pdfImage.Margin.Top + pdfImage.Margin.Bottom;
            mainWindow.txtInfo.Text = nextPdfPageIndex + "|" + pdfDocument.PageCount;
            pdfimage.GenerateImage();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(pdfDocument != null)
            {
                if(e.VerticalChange > 0)
                {
                    if (pdfDocument.PageCount > nextPdfPageIndex)
                    {
                        Image pdfImage = pdfPageImages[(int)nextPdfPageIndex].ResultImage;
                        if (e.VerticalOffset + this.ActualHeight >= currentPdfPageTotalHeight)
                        {
                            PdfPageImage pdfimage = pdfPageImages[(int)nextPdfPageIndex];
                            if (!pdfimage.AsImage)
                            {
                                currentlyLoadedImages.Add(pdfimage);
                                nextPdfPageIndex++;
                                currentPdfPageTotalHeight += pdfImage.Height + pdfImage.Margin.Top + pdfImage.Margin.Bottom;
                                mainWindow.txtInfo.Text = nextPdfPageIndex + "|" + pdfDocument.PageCount/* + "\n" + e.VerticalOffset + "|" + currentPdfPageTotalHeight + "\n" +
                                    pdfImage.Height + "|" + pdfImage.Margin.Top + "|" + pdfImage.Margin.Bottom*/;
                                pdfimage.GenerateImage();
                            }
                        }
                    }

                    double currentHeight = currentPdfPageTotalHeight;
                    for (int i = currentlyLoadedImages.Count - 1; i > 0; i--)
                    {
                        Image pdfImage = pdfPageImages[i].ResultImage;
                        currentHeight -= pdfImage.Height + pdfImage.Margin.Top + pdfImage.Margin.Bottom;
                        if (e.VerticalOffset >= currentHeight)
                        {
                            if(i-1 >=0 )
                            {
                                pdfPageImages[i-1].UnloadImage();
                                break;
                            }
                        }
                    }
                }else
                {
                }
            }
        }
    }
}
