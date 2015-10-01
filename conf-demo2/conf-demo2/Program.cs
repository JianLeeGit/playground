using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Net;
using System.IO;
using System.Threading;
using Splunk.Logging;

namespace conf_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            EnableSelfSignedCertificates();

            Uri uri = new Uri("https://localhost:8088");
            string token = "BEC47D17-AC4A-49ED-834B-969745D24550";

            var trace = new TraceSource("conf-demo");
            trace.Switch.Level = SourceLevels.All;
            var listener = new HttpEventCollectorTraceListener(uri, token);
            trace.Listeners.Add(listener);

            HashSet<string> files = new HashSet<string>();

            while (true)
            {
                string[] currentFiles = Directory.GetFiles(args[0]);
                foreach (string s in currentFiles)
                {
                    if (!files.Contains(s))
                    {
                        files.Add(s);
                        string ascii = ToAscii(s.Substring(s.LastIndexOf('\\') + 1), new Bitmap(s, true));

                        trace.TraceInformation(ascii);
                        trace.Flush();
                    }
                }
                Thread.Sleep(200);
            }
        }

        private static string ToAscii(string s, Bitmap image)
        {
            Boolean toggle = false;
            StringBuilder sb = new StringBuilder();
            sb.Append(s);
            sb.Append("\n");

            image = GetReSizedImage(image, 254);

            for (int h = 0; h < image.Height; h++)
            {
                for (int w = 0; w < image.Width; w++)
                {
                    Color pixelColor = image.GetPixel(w, h);
                    int red = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int green = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int blue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    Color gray = Color.FromArgb(red, green, blue);

                    if (!toggle)
                    {
                        int index = (gray.R * 10) / 255;
                        sb.Append(_Chars[index]);
                    }
                }

                if (!toggle)
                {
                    sb.Append("\n");
                }

                toggle = !toggle;
            }

            return sb.ToString();
        }


        private static Bitmap GetReSizedImage(Bitmap inputBitmap, int asciiWidth)
        {
            int asciiHeight = (int)Math.Ceiling((double)inputBitmap.Height * asciiWidth / inputBitmap.Width);
            Bitmap result = new Bitmap(asciiWidth, asciiHeight);
            Graphics g = Graphics.FromImage((Image)result);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(inputBitmap, 0, 0, asciiWidth, asciiHeight);
            g.Dispose();
            return result;
        }

        private static void EnableSelfSignedCertificates()
        {
            // Enable self signed certificates
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                    System.Security.Cryptography.X509Certificates.X509Chain chain,
                    System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true;
                };
        }

        static private string[] _Chars = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };
    }
}
