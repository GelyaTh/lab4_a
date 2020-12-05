using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4_AOCI
{
    class PrimitivesSearch
    {
        private Image<Bgr, byte> srcImg;
        private int lastContoursCount;

        public int LastContoursCount { get => lastContoursCount; }
        public Image<Bgr, byte> SrcImg { get => srcImg; }

        public PrimitivesSearch(Image<Bgr, byte> image)
        {
            srcImg = image;
            lastContoursCount = 0;
        }

        private VectorOfVectorOfPoint findContours(int trsh, bool colorSearch)
        {
            //var cannyEdges = bluredImage.Canny(1, 250);
            // imageBox2.Image = cannyEdges.Resize(640, 480, Inter.Linear);
            Image<Gray, byte> binarizedImage;
            if (colorSearch)
                binarizedImage = binarizeImageByColorSearch();
            else
                binarizedImage = binarizeImage(trsh);

            var contours = new VectorOfVectorOfPoint(); // контейнер для хранения контуров

            CvInvoke.FindContours(
                binarizedImage, // исходное чёрно-белое изображение
                contours, // найденные контуры
                null, // объект для хранения иерархии контуров (в данном случае не используется)
                RetrType.List, // структура возвращаемых данных (в данном случае список) 
                ChainApproxMethod.ChainApproxSimple);

            var approxContours = new VectorOfVectorOfPoint();

            for (int i = 0; i < contours.Size; i++)
            {
                var approxContour = new VectorOfPoint();
                CvInvoke.ApproxPolyDP(
                    contours[i], // исходный контур
                    approxContour, // контур после аппроксимации
                    CvInvoke.ArcLength(contours[i], true) * 0.05, // точность аппроксимации, прямо
                                                                  //пропорциональная площади контура
                    true); // контур становится закрытым (первая и последняя точки соединяются)
                approxContours.Push(approxContour);
            }
            lastContoursCount = approxContours.Size;
            return approxContours;
        }

        public Image<Bgr, byte> drawContours(int thrs, bool colorSearch)
        {
            var contours = findContours(thrs, colorSearch);
            var contoursImage = srcImg.Copy(); //создание "пустой" копии исходного изображения
            for (int i = 0; i < contours.Size; i++)
            {
                var points = contours[i].ToArray();
                contoursImage.Draw(points, new Bgr(Color.GreenYellow), 2); // отрисовка точек }
            }
            return contoursImage;
        }

        public Image<Bgr, byte> findRectangles(int thrs, int area, bool colorSearch)
        {
            var contours = findContours(thrs, colorSearch);
            lastContoursCount = 0;
            var contoursImage = srcImg.Copy(); //создание "пустой" копии исходного изображения
            for (int i=0; i<contours.Size; i++)
            {
                if (CvInvoke.ContourArea(contours[i], false) > area)
                {
                    if (isRectangle(contours[i].ToArray()))
                    {               
                        contoursImage.Draw(CvInvoke.MinAreaRect(contours[i]),
                        new Bgr(Color.GreenYellow), 2);
                        lastContoursCount++;
                    }
                }
            }
            return contoursImage;
        }

        public Image<Bgr, byte> findTriangles(int thrs, int area, bool colorSearch)
        {
            var contours = findContours(thrs, colorSearch);
            lastContoursCount = 0;
            var contoursImage = srcImg.Copy(); //создание "пустой" копии исходного изображения
            for (int i = 0; i < contours.Size; i++)
            {
                if (CvInvoke.ContourArea(contours[i], false) > area)
                {
                    if (contours[i].Size == 3) // если контур содержит 3 точки, то рисуется треугольник
                    {
                        var points = contours[i].ToArray();
                        contoursImage.Draw(new Triangle2DF(points[0], points[1], points[2]),
                        new Bgr(Color.GreenYellow), 2);
                        lastContoursCount++;
                    }
                }
            }
            return contoursImage;
        }

        public Image<Bgr, byte> findCircles(int thrs, bool colorSearch)
        {
            lastContoursCount = 0;
            Image<Gray, byte> grayImage;
            /*if (colorSearch)
                grayImage = binarizeImageByColorSearch();
            else*/
                grayImage = srcImg.Convert<Gray, byte>();
            var bluredImage = grayImage.SmoothGaussian(9);

            List<CircleF> circles = new List<CircleF>(CvInvoke.HoughCircles(bluredImage,
                        HoughModes.Gradient,
                        1.0,
                        250, //minDistance
                        100,
                        thrs, //acTreshold
                        2, //minRadius
                        500)); //maxRadius

            var resultImage = srcImg.Copy();
            foreach (CircleF circle in circles) resultImage.Draw(circle, new Bgr(Color.GreenYellow), 2);
            lastContoursCount = circles.Count;
            return resultImage;
        }

        private Image<Gray, byte> binarizeImage(int trsh)
        {
            var grayImage = srcImg.Convert<Gray, byte>();
            int kernelSize = 5; // радиус размытия
            var bluredImage = grayImage.SmoothGaussian(kernelSize);

            var threshold = new Gray(trsh); // пороговое значение
            var color = new Gray(255); // этим цветом будут закрашены пиксели, имеющие значение > threshold 
            return bluredImage.ThresholdBinary(threshold, color);
        }

        private Image<Gray, byte> binarizeImageByColorSearch()
        {
            var hsvImage = srcImg.Convert<Hsv, byte>(); // конвертация в HSV 
            var hueChannel = hsvImage.Split()[0]; // выделение канала Hue
            byte color = 30; // соответствует желтому тону в Emgu.CV
            byte rangeDelta = 10; // величина разброса цвета 
            return hueChannel.InRange(new Gray(color - rangeDelta), new Gray(color + rangeDelta)); // выделение 
        }


        private bool isRectangle(Point[] points)
        {
            int delta = 10; // максимальное отклонение от прямого угла
            LineSegment2D[] edges = PointCollection.PolyLine(points, true);

            for (int i = 0; i < edges.Length; i++) // обход всех ребер контура
            {
                double angle = Math.Abs(edges[(i + 1) %
                edges.Length].GetExteriorAngleDegree(edges[i]));
                if (angle < 90 - delta || angle > 90 + delta) // если угол непрямой
                {
                    return false;
                }
            }
            return true;
        }
    }
}
