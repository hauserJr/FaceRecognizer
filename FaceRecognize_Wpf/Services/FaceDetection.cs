using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using FaceDetection;
using FaceRecognize_Wpf.Model;

namespace FaceRecognize_Wpf.Services
{
    public class FaceTrace
    {
        /// <summary>
        /// 追蹤人臉
        /// </summary>
        public FaceCollention TraceFace(Mat myMat)
        {
            //辨識Model
            var dataResult = new FaceCollention();

            //眼、臉資料初始化，提供演算法存放資料
            List<Rectangle> Faces = new List<Rectangle>();
            List<Rectangle> Eyes = new List<Rectangle>();

            //偵測眼睛
            var eyePath = $"{PathArgs.haarcascade_Eye}";

            //偵測臉部
            var facePath = $"{PathArgs.haarcascade_Face}";

            //利用Open Cv計算眼臉位置
            DetectFace.Detect(
               myMat, facePath, eyePath,
               Faces, Eyes,
               out long detectionTime);

            dataResult = new FaceCollention()
            {
                Faces = Faces,
                Eyes = Eyes,
            };
            return dataResult;
        }
    }
}
