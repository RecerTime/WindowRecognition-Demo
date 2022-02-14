using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace WindowRecognition_v._1___Demo
{
    public class Point
    {
        public int x;
        public int y;

        public override string ToString()
        {
            return $"{x}, {y}";
        }
    }

    public class Corners
    {
        public Point upLeft = new Point();
        public Point upRight = new Point();
        public Point downRight = new Point();
        public Point downLeft = new Point();

        public override string ToString()
        {
            return $"{upLeft.ToString()} : {upRight.ToString()} : {downLeft.ToString()} : {downRight.ToString()}";
        }

        public bool PointInside(Point inside)
        {
            return inside.x > upLeft.x && inside.x < downRight.x && inside.y > upLeft.y && inside.y < downRight.y;
        }
    }

    class Program
    {
        static Color[] colors = { Color.FromArgb(255, 0, 0), Color.FromArgb(0, 255, 0), Color.FromArgb(0, 0, 255), Color.FromArgb(255, 0, 255) };

        static double tolerance = 150;

        static int pixelDistance = 8;

        static int squareTolerance = 2;

        Bitmap refImage = new Bitmap("Test1.5.png");

        Bitmap image;

        Point middle = new Point();

        Corners screenSpace = new Corners();

        bool isHittingViewport;

        void Start()
        {
            Console.WriteLine(DateTime.Now.Millisecond);

            image = refImage;

            middle.x = image.Width / 2;
            middle.y = image.Height / 2;

            pixelDistance = (image.Width * image.Height) / 41616;

            CheckEdge();

            isHittingViewport = screenSpace.PointInside(middle);

            Console.WriteLine(screenSpace.ToString());
        }

        double RGBDif(Color a, Color b)
        {
            int R = (int)Math.Pow((a.R - b.R), 2);
            int G = (int)Math.Pow((a.G - b.G), 2);
            int B = (int)Math.Pow((a.B - b.B), 2);
            return Math.Sqrt(R + G + B);
        }

        void CheckEdge()
        {
            List <List<Color>> pixels = new List<List<Color>>();

            for (int y = 0; y < image.Height; y = y + pixelDistance)
            {
                List<Color> colors = new List<Color>();

                for (int x = 0; x < image.Width; x = x + pixelDistance)
                {
                    colors.Add(image.GetPixel(x, y));
                }

                pixels.Add(colors);
            }
            screenSpace = CheckCorner(pixels);

            //SaveToImage(pixels, screenSpace);
        }

        void SaveToImage(List<List<Color>> pixels, Corners screenRect)
        {
            Bitmap bitmap = new Bitmap(pixels.Count, pixels[0].Count);

            for (int x = screenRect.upLeft.x; x < screenRect.upRight.x; x++)
            {
                for (int y = screenRect.upLeft.y; y < screenRect.downLeft.y; y++)
                {
                    bitmap.SetPixel(x, y, pixels[x][y]);
                }
            }

            bitmap.Save("Result", ImageFormat.Png);
        }

        Corners CheckCorner(List<List<Color>> pixels)
        {
            Corners redCorner = CheckRGB(pixels, colors[0]);
            Corners greenCorner = CheckRGB(pixels, colors[1]);
            Corners blueCorner = CheckRGB(pixels, colors[2]);
            Corners purpleCorner = CheckRGB(pixels, colors[3]);

            Corners corners = new Corners();

            corners.upLeft = redCorner.upLeft;
            corners.upRight = greenCorner.upRight;
            corners.downRight = blueCorner.downRight;
            corners.downLeft = purpleCorner.downLeft;

            return corners;
        }

        Corners CheckRGB(List<List<Color>> pixels, Color color)
        {
            int counter = 0;

            int startIndex = -1;

            Corners corners = null;

            for (int y = 0; y < pixels.Count && corners == null; y++)
            {
                List<Color> currentList = pixels[y];
                for (int x = 0; x < currentList.Count && corners == null; x++)
                {
                    Color c = currentList[x];
                    if (c.R > 150 || c.G > 150 || c.B > 150)
                    {
                        if (RGBDif(c, color) < tolerance)
                        {
                            counter++;

                            if (counter == 1)
                            {
                                startIndex = x;
                            }
                        }
                        else
                        {
                            if (counter >= squareTolerance)
                            {
                                int offsetY = 1;

                                while (y + offsetY < pixels.Count && RGBDif(pixels[y + offsetY][startIndex], color) < tolerance)
                                {
                                    offsetY++;
                                }

                                if (offsetY >= squareTolerance)
                                {
                                    if (y + offsetY - 1 < pixels.Count && startIndex + counter - 1 < currentList.Count)
                                    {
                                        //Console.WriteLine((startIndex + counter - 1) + ", " + (y + offsetY - 1));

                                        if (RGBDif(pixels[y + offsetY - 1][startIndex + counter - 1], color) < tolerance)
                                        {
                                            corners = new Corners();

                                            corners.upLeft.x = startIndex;
                                            corners.upLeft.y = y;

                                            corners.upRight.x = startIndex + counter - 1;
                                            corners.upRight.y = y;

                                            corners.downRight.x = startIndex + counter - 1;
                                            corners.downRight.y = y + offsetY - 1;

                                            corners.downLeft.x = startIndex;
                                            corners.downLeft.y = y + offsetY - 1;
                                        }
                                    }
                                }
                            }
                            startIndex = -1;
                            counter = 0;
                        }
                    }
                }
            }
            return corners;
        }

        static void Main(string[] args)
        {
            Program program = new Program();

            program.Start();

            Console.WriteLine(DateTime.Now.Millisecond);

            Console.ReadLine();
        }
    }
}