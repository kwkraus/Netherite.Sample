// create an azure function that accepts an image and creates a thumbprint
// and stores it in a blob storage
// 2019-01-10   PV
// 2021-04-04   PV      Azure Storage v12

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;

namespace MyFunctionApp
{
    public static class myfunction
    {
        [FunctionName("myfunction")]
        public static async Task Run([BlobTrigger("mycontainer/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, [Blob("mycontainer/{name}-thumb", FileAccess.Write, Connection = "AzureWebJobsStorage")]CloudBlockBlob outputBlob)
        {
            // Load image
            using (var image = Image.Load(myBlob))
            {
                // Resize
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Min,
                    Size = new Size(100, 100)
                }));

                // Save to memory
                using (var ms = new MemoryStream())
                {
                    image.SaveAsJpeg(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    // Save to blob storage
                    await outputBlob.UploadFromStreamAsync(ms);
                }
            }
        }
    }
}