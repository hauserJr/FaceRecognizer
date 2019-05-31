using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using FaceRecognize_Wpf.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Emgu.CV.Face.FaceRecognizer;

namespace FaceRecognize_Wpf.Services
{
    public class FacesRepositores
    {
        private FaceRecognizer faceRecognizer;
        private JsonFileApp configureApp = new JsonFileApp();
        private readonly string FilePath = $"{PathArgs.dataDomain}{PathArgs.facePicturePath}";
        private readonly string TrainingDataPath = $"{PathArgs.dataDomain}{PathArgs.faceDataPath}{PathArgs.faceDateName}";
        public FacesRepositores()
        {
            faceRecognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);
        }

        public int FaceTraining()
        {
            var facesMat = new List<Mat>();
            var labels = new List<int>();
            

            //取得路徑下所有照片
            int PictureCount = 0;

            foreach (var dirItem in Directory.GetDirectories(FilePath))
            {
                //移除路徑及檔名
                var getUserDetail = dirItem.Replace(FilePath, "");
                var getEmployeeNum = getUserDetail.Split('.')[0];
                var getUserName = getUserDetail.Split('.')[1];

                foreach (var fileName in Directory.GetFiles(dirItem)
                .Select((value, index) => new { index, value }))
                {
                    //照片灰階處理
                    facesMat.Add(new Mat($"{fileName.value}").ToImage<Gray, byte>().Mat);
                    //照片Tag
                    labels.Add(int.Parse(getEmployeeNum));
                    PictureCount += 1;
                }

                //進行訓練
                faceRecognizer.Train(facesMat.ToArray(), labels.ToArray());
                faceRecognizer.Write(TrainingDataPath);
            }
            return PictureCount;
        }

        public (double, int, bool) FacesRecognize(Mat face)
        {
      
            //取得基數設定
            var getData = configureApp.GetConfigureFileData();

            //基準數
            double faceBase = getData.faceBaseScore;

            //偵測是否為人臉
            var getFaceData = new FaceTrace().TraceFace(face);

            faceRecognizer.Read(TrainingDataPath);
            FileStream fileStream = new FileStream(TrainingDataPath, FileMode.Open, FileAccess.Read);
            if (fileStream.Length > 0)
            {
                //如果是人臉進行比對
                foreach (var item in getFaceData.Faces)
                {
                    //影格圖片灰階化 並設定大小100
                    var ToGrayFace = face
                        .ToImage<Gray, byte>()
                        .GetSubRect(item)
                        .Resize(100, 100, Inter.Cubic);

                    if (fileStream.Length != 0)
                    {
                        var predictResult = faceRecognizer.Predict(ToGrayFace);

                        //當有一筆資料吻合 其他則不再辨識
                        if (predictResult.Distance <= faceBase)
                        {
                            return (predictResult.Distance, predictResult.Label, true);
                        }
                    }
                }
            }
            return (0, 0, false);
        }

        public int GetFileCount(string filePath)
        {
            return Directory.GetFiles(filePath).Length;
        }
    }
}
