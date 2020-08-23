using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

namespace simplePDFTools
{
    class PdfPageImage
    {
        private uint index;
        public uint Index { get => index; set => index = value; }

        private PdfPage pdfPage;
        public PdfPage PdfPage { get => pdfPage; set => pdfPage = value; }

        private BitmapImage bmpImage;
        public BitmapImage BmpImage { get => bmpImage; }

        private uint pdfWidth;
        public uint PdfWidth { get => pdfWidth; set => pdfWidth = value; }

        private Image resultImage;
        public Image ResultImage { get => resultImage;}

        private bool asImage = false;
        public bool AsImage { get => asImage; }



        public PdfPageImage(uint index, PdfPage pdfPage, uint pdfWidth)
        {
            this.Index = index;
            this.PdfPage = pdfPage;
            this.PdfWidth = pdfWidth;
        }

        public void PreGenerateImage()
        {
            double imgHeight = PdfPage.Size.Height * (PdfWidth / PdfPage.Size.Width);
            var image = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(4, 4, 4, 4),
                MaxWidth = pdfWidth,
                Height = imgHeight
            };
            resultImage = image;
        }

        public async Task GenerateImage()
        {
            bmpImage = await PageToBitmapAsync(pdfPage);
            resultImage.Source = bmpImage;
            asImage = true;
        }

        private async Task<BitmapImage> PageToBitmapAsync(PdfPage page)
        {
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

            return image;
        }

        public void UnloadImage()
        {
            resultImage.Source = null;
            bmpImage = null;
            asImage = false;
        }

    }
}
