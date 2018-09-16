using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using openalprnet;
using System.Configuration;

namespace LPRConsole
{
    class LPR
    {
        List<VehicleResult> vehicleResult;
        List<Image> images;
        

        public Rectangle BoundingRectangle(List<Point> points)
        {
            int minX = points.Min(x=>x.X);
            int minY = points.Min(x=>x.Y);
            int maxX = points.Max(x=>x.X);
            int maxY = points.Max(x => x.Y);

            return new Rectangle(new Point(minX,minY), new Size(maxX - minX, maxY - minY));            
        }

        private Bitmap CropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        private Bitmap CombineImages(List<Image> images)
        {
            
            Bitmap finalImage = null;
            try
            {
                int width = 0;
                int height = 0;

                foreach (Image bmp in images)
                {
                    width += bmp.Width;
                    if (bmp.Height > height)
                    {
                        height = bmp.Height;
                    }
                }

                finalImage = new Bitmap(width, height);

                using (Graphics g = Graphics.FromImage(finalImage))
                {
                    g.Clear(Color.Black);

                    int offset = 0;

                    foreach (Bitmap image in images)
                    {
                        g.DrawImage(image, new Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width;
                    }
                }

                return finalImage;
            }
            catch (Exception ex)
            {
                if (finalImage != null)
                {
                    finalImage.Dispose();
                }
                throw ex;
            }
            finally
            {
                foreach (Image image in images)
                {
                    image.Dispose();
                }
            }
        }

        public Bitmap ResizeImage(Image image)
        {
            Int16 BOXHEIGHT = 30;
            Int16 BOXWIDTH = 135;

            double scaleHeight = (double)BOXHEIGHT / (double)image.Height;

            double scaleWitdh = (double)BOXWIDTH / (double)image.Height;

            double scale = Math.Min(scaleHeight,scaleWitdh);

            Bitmap resizedImage = new Bitmap(image, Convert.ToInt32(image.Width * scale), Convert.ToInt32(image.Height * scale));

            return resizedImage;
        }

        public Boolean RecognizeNumber(string path)
        {
            try
            {

                double maxVehicle = Convert.ToDouble(ConfigurationManager.AppSettings["MaxVehicle"]);
                int maxResult = Convert.ToInt32(ConfigurationManager.AppSettings["MaxResult"]);


                string confPath = AppDomain.CurrentDomain.BaseDirectory + @"\openalpr.conf";
                string runtimePath = AppDomain.CurrentDomain.BaseDirectory + @"\runtime_date";

            AlprNet alpr = new AlprNet("eu", confPath, runtimePath);

            if (!alpr.IsLoaded())
            {
                throw new System.Exception("Error loading openalpr library");
            }

            alpr.DetectRegion = true;
            alpr.TopN = maxResult;

            AlprResultsNet results = alpr.Recognize(path);

            if (images!=null)
            {
                images = null;
            }

            if (vehicleResult !=null)
            {
                vehicleResult = null;
            }

            images = new List<Image>(results.Plates.Count);
            vehicleResult = new List<VehicleResult>();

            

            Int16 count = 1;
            Int16 listIndex = 0;

            if (results.Plates.Count>0)
            {
                foreach (AlprPlateResultNet result in results.Plates)
                {
                    if (count>maxVehicle)
                    {
                        break;
                    }

                    Rectangle rect = BoundingRectangle(result.PlatePoints);
                    Image img = Image.FromFile(path);
                    Image cropped = CropImage(img, rect);
                    cropped = ResizeImage(cropped);
                    images.Add(cropped);

                    string filePath = ConfigurationManager.AppSettings["CSVPath"];                    
                    

                    foreach (AlprPlateNet plate in result.TopNPlates)
                    {
                        vehicleResult.Add(new VehicleResult(plate.Characters,listIndex,count--));

                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath, true))
                        {
                            sw.WriteLine(string.Format(string.Join(",", plate.Characters,DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))));
                        }

                        //Console.WriteLine(plate.Characters);

                        listIndex++;
                    }

                 count++;   
                }

                return true;
            }
            else{
                return false;
            }
           //System.IO.File.Delete(path); 
            }
            catch(Exception ex){
                throw ex;
            }

            return false;
        }
    }


}
