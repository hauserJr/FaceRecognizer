using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognize_Wpf.Services
{
    public class FacesRepositores
    {
        private FaceRecognizer faceRecognizer;
        private readonly string FilePath = $"{PathArgs.dataDomain}{PathArgs.facePicturePath}";
        private readonly string TrainingDataPath = $"{PathArgs.dataDomain}{PathArgs.faceDataPath}{PathArgs.faceDateName}";
        public FacesRepositores()
        {
            faceRecognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);
        }

        public void FaceTraining()
        {
            var facesMat = new List<Mat>();
            var labels = new List<int>();

            //取得路徑下所有照片
            foreach (var FileName in Directory.GetFiles(FilePath)
                .Select((value, index) => new { index, value }))
            {
                //照片灰階處理
                facesMat.Add(new Mat($"{FileName.value}").ToImage<Gray, byte>().Mat);
                //照片Tag
                labels.Add(FileName.index);
            }

            //進行訓練
            faceRecognizer.Train(facesMat.ToArray(), labels.ToArray());
            faceRecognizer.Write(TrainingDataPath);
        }

        public (double,bool) FacesRecognize(Mat face)
        {
            //基準數
            double faceBase = 100.0;

            //偵測是否為人臉
            var getFaceData = new FaceTrace().TraceFace(face);

            //如果是人臉進行比對
            foreach (var item in getFaceData.Faces)
            {
                //影格圖片灰階化 並設定大小100
                var ToGrayFace = face
                    .ToImage<Gray, byte>()
                    .GetSubRect(item)
                    .Resize(100, 100, Inter.Cubic);


                faceRecognizer.Read(TrainingDataPath);

                var predictResult = faceRecognizer.Predict(ToGrayFace);
                return (predictResult.Distance, predictResult.Distance <= faceBase ? true : false);
            }
            return (0.0, false);
        }
    }
}
