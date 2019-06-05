using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using FaceRecognize_Wpf.Helper;
using FaceRecognize_Wpf.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        private string FilePath = $"{PathArgs.dataDomain}{PathArgs.facePicturePath}";
        private readonly string TrainingDataPath = $"{PathArgs.dataDomain}{PathArgs.faceDataPath}{PathArgs.faceDateName}";
        private readonly string TrainingDataPath_Backup = $"{PathArgs.dataDomain}{PathArgs.faceDataBackupPath}{PathArgs.faceDateName}";
        public FacesRepositores()
        {
            faceRecognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);
        }

        /// <summary>
        /// 單次訓練
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public int FaceTraining(string filePath = null)
        {
            var facesMat = new List<Mat>();
            var labels = new List<int>();

            //取得路徑下所有照片
            int PictureCount = 0;
            if (filePath != null)
            {
                //移除路徑及檔名
                var getUserDetail = filePath.Replace(FilePath, "");
                var getEmployeeNum = getUserDetail.Split('.')[0];
                var getUserName = getUserDetail.Split('.')[1];

                foreach (var fileName in Directory.GetFiles(filePath)
                .Select((value, index) => new { index, value }))
                {
                    //照片灰階處理
                    facesMat.Add(new Mat($"{fileName.value}").ToImage<Gray, byte>().Mat);
                    //照片Tag
                    labels.Add(int.Parse(getEmployeeNum));
                    PictureCount += 1;
                }

                //進行訓練
                //var AsyncfaceRecognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);
                faceRecognizer.Train(facesMat.ToArray(), labels.ToArray());
                faceRecognizer.Write(TrainingDataPath_Backup);
            }
            return PictureCount;
        }

        /// <summary>
        /// 批次訓練
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public int MultipleFaceTraining(string filePath = null)
        {
            var facesMat = new List<Mat>();
            var labels = new List<int>();

            if (filePath != null)
            {
                FilePath = filePath;
            }

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
                    //進行訓練
                    
                    PictureCount += 1;
                }

                faceRecognizer.Train(facesMat.ToArray(), labels.ToArray());
                faceRecognizer.Write(TrainingDataPath_Backup);
            }
            return PictureCount;
        }

        /// <summary>
        /// 臉部辨識
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public (double, int, bool) FacesRecognize(Mat face)
        {

            try
            {
                //取得基數設定
                var getData = configureApp.GetConfigureFileData();

                //基準數
                double faceBase = double.Parse(getData.faceBaseScore);

                //偵測是否為人臉
                var getFaceData = new FaceTrace().TraceFace(face);

                FileStream fileStream = new FileStream(TrainingDataPath, FileMode.Open, FileAccess.Read);

                //初始化EigenFaceRecognizer
                //不初始化會導致Emgu CV底層對於記憶體操作發生錯誤
                faceRecognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);
                if (fileStream.Length > 0)
                {
                    faceRecognizer.Read(TrainingDataPath);

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
                            //重新宣告
                            var predictResult = faceRecognizer.Predict(ToGrayFace);

                            //當有一筆資料吻合 其他則不再辨識
                            if (predictResult.Distance <= faceBase)
                            {
                                return (predictResult.Distance, predictResult.Label, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (0, 0, false);
            }
            return (0, 0, false);
        }

        /// <summary>
        /// 臉部自動訓練
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public (double, int, bool) FacesRecognizeML(Mat face)
        {
            var settingApp = new JsonFileApp().GetConfigureFileData();
            try
            {
                //取得基數設定
                var getData = configureApp.GetConfigureFileData();

                //偵測是否為人臉
                var getFaceData = new FaceTrace().TraceFace(face);

                FileStream fileStream = new FileStream(TrainingDataPath, FileMode.Open, FileAccess.Read);

                //初始化EigenFaceRecognizer
                //不初始化會導致Emgu CV底層對於記憶體操作發生錯誤
                faceRecognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);
                if (fileStream.Length > 0)
                {
                    faceRecognizer.Read(TrainingDataPath);

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
                            //重新宣告
                            var predictResult = faceRecognizer.Predict(ToGrayFace);

                            //當有一筆資料吻合 其他則不再辨識
                            if (predictResult.Distance >= double.Parse(settingApp.faceMLMinScore) 
                                && predictResult.Distance <= double.Parse(settingApp.faceMLMaxScore))
                            {
                                return (predictResult.Distance, predictResult.Label, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (0, 0, false);
            }
            return (0, 0, false);
        }

        /// <summary>
        /// 儲存Picture
        /// </summary>
        /// <param name="faceMat"></param>
        /// <param name="faceAngle"></param>
        /// <param name="userPictureDir"></param>
        /// <param name="userName"></param>
        public void SavePicture(Mat faceMat, FaceCollention faceAngle ,string userPictureDir, string userName)
        {
            var pictureLimit = new JsonFileApp().GetConfigureFileData().samplePicutreMax;

            //判斷該照片資料夾是否存在
            if (!File.Exists(userPictureDir))
            {
                Directory.CreateDirectory(userPictureDir);
            }


            //判斷是否有取得人臉
            foreach (var faceItem in faceAngle.Faces)
            {
                //取得目錄底下的圖片數量
                var getPictureCount = GetFileCount(userPictureDir);

                //當上限小於當前數量或-1時才進行儲存
                if (int.Parse(pictureLimit) > getPictureCount || int.Parse(pictureLimit) == -1)
                {
                    //儲存人臉
                    //1. 進行大小處理 100 * 100
                    //2. 灰階處理
                    //3. 儲存
                    faceMat.ToImage<Emgu.CV.Structure.Gray, byte>()
                        .GetSubRect(faceItem)
                        .Resize(100, 100, Inter.Cubic)
                        .Save($"{userPictureDir}/" + $"{ userName }_{ getPictureCount }.jpg");
                }
            }
        }

        /// <summary>
        /// 取得檔案數量
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public int GetFileCount(string filePath)
        {
            return Directory.GetFiles(filePath).Length;
        }
    }
}
