using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using FaceRecognize_Wpf.Emun;
using FaceRecognize_Wpf.Helper;
using FaceRecognize_Wpf.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static FaceRecognize_Wpf.UCWindows.FaceControl;

namespace FaceRecognize_Wpf.UCWindows
{
    /// <summary>
    /// FaceAutoLearning.xaml 的互動邏輯
    /// </summary>
    public partial class FaceAutoLearning : Page
    {
        //初始化Camera
        public VideoCapture camCapture;

        //是否停止子執行緒 false = 不停 ,true = 停止
        public static bool IsStopTask = false;

        // 臉部辨識相關功能
        private FacesRepositores facesRepo = new FacesRepositores();

        //圖片存放位置
        private string cameraPath = $"{PathArgs.dataDomain}{PathArgs.facePicturePath}";

        //Handlers Method Collention
        private static HandlerCollention handlerCollention = new HandlerCollention();

        //MessageBox Handlers
        MessageShoeDelegate messageShoeDelegate
                = new MessageShoeDelegate(handlerCollention.MessageShow);

        //Label Handler
        LabelContentDelegate labelContentDelegate
                = new LabelContentDelegate(handlerCollention.SetContent);

        public FaceAutoLearning()
        {
            InitializeComponent();

            ComboItmeOfWebCam();

        }

        #region Handler
        public delegate void FaceResultDelegate(string Content, Brush brush);

        public delegate void TakeShotToMLDelegate(Mat CamImg, string _emplyeeNum, string _userName);

        /// <summary>
        /// 臉部辨識結果
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="brush"></param>
        public void FaceResult(string Content, Brush brush)
        {
            this.FaceResultText.Content = Content;
            this.FaceResultText.Foreground = brush;
        }

        /// <summary>
        /// 拍照存放圖片
        /// </summary>
        public void TakeShotToML(Mat CamImg, string _emplyeeNum, string _userName)
        {
            try
            {
                var userName = _userName;
                var employeeNum = _emplyeeNum;

                //取得人臉區域的Rectangle
                var getFacesFeature = new FaceTrace().TraceFace(CamImg);

                //Lower UserName 
                var lowerUserName = userName.ToLower();
                var userPictureDir = $"{ cameraPath }{ employeeNum }.{ lowerUserName }";

                //判斷是否有取得人臉
                foreach (var faceItem in getFacesFeature.Faces)
                {
                    //儲存人臉
                    //1. 進行大小處理 100 * 100
                    //2. 灰階處理
                    //3. 儲存

                    //取得目錄底下的圖片數量
                    var getPictureCount = facesRepo.GetFileCount(userPictureDir);

                    CamImg.ToImage<Emgu.CV.Structure.Gray, byte>()
                        .GetSubRect(faceItem)
                        .Resize(100, 100, Inter.Cubic)
                        .Save($"{userPictureDir}/{ userName }_{ getPictureCount }.jpg");
                }
                var pictureCount = facesRepo.FaceTraining(userPictureDir);

            }
            catch (Exception ex)
            {
                
            }

        }
        #endregion

        #region Action
        /// <summary>
        /// Web Cam畫面輸出
        /// </summary>
        public void ShowCamera()
        {
            //Web Camera開啟後的動作
            //進入無窮迴圈
            FaceResultDelegate faceResultDelegate = new FaceResultDelegate(FaceResult);
            while (true)
            {
                try
                {
                    //當收到Task停止指示時
                    if (IsStopTask)
                    {
                        camCapture.Stop();
                        camCapture.Dispose();
                        throw new Exception();
                    }
                    else
                    {
                        // 取得串流Frame
                        using (Mat camMat = camCapture.QueryFrame())
                        {
                            //從Camera取得Mat Data
                            camCapture.Retrieve(camMat);

                            //將影格進行人臉追蹤處理
                            var getFaceData = new FaceTrace().TraceFace(camMat);
                            
                            if (getFaceData.Faces.Count() == 0)
                            {
                                this.Dispatcher.Invoke(faceResultDelegate, "NO Face", Brushes.Black);
                            }
                            else
                            {
                                //判斷人臉數
                                foreach (var faceItem in getFaceData.Faces)
                                {
                                    //繪製人臉框
                                    CvInvoke.Rectangle(camMat, faceItem, new Bgr(System.Drawing.Color.Green).MCvScalar, 2);

                                    //判斷人臉是否存在於資料庫
                                    var trainMat = camCapture.QueryFrame();
                                    var facePass = facesRepo.FacesRecognize(trainMat);
                                    if (facePass.Item3)
                                    {
                                        var userData = NameCollection.UserTable.Where(o => o.Key == facePass.Item2.ToString()).FirstOrDefault();

                                        this.Dispatcher.Invoke(faceResultDelegate, $"{getFaceData.Faces.Count()} Face", Brushes.Green);

                                        //進入訓練
                                        TakeShotToMLDelegate takeShotToMLDelegate = new TakeShotToMLDelegate(TakeShotToML);
                                        this.Dispatcher.Invoke(takeShotToMLDelegate, trainMat, userData.Key, userData.Name);

                                    }
                                    else
                                    {
                                        this.Dispatcher.Invoke(faceResultDelegate, "0 Face", Brushes.Red);
                                    }
                                }
                            }


                            //透過Invoke將資料傳遞至主UI上
                            this.Dispatcher.Invoke(() =>
                            {
                                this.CamView.Source = camMat.Bitmap.BitmapToBitmapImage();
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(labelContentDelegate, this.TrainTip, Brushes.Black, "等候訓練啟動中 ...");
                    this.Dispatcher.Invoke(faceResultDelegate, $"0 Face", Brushes.Black);

                    //當發生Catch或者Task停止時
                    this.Dispatcher.Invoke(() =>
                    {
                        //Combo Box重新啟用
                        this.WebCamCombo.IsEnabled = true;
                        //按鈕文字變更 停用 => 啟用
                        this.CameraSetupBtn.Content = "啟用";
                        //Web Camera狀態變更 啟用 => 停用
                        this.WebCamContent.Content = "停用";
                        //狀態文字顏色設定為紅色
                        this.WebCamContent.Foreground = Brushes.Red;
                    });
                    //將Stop Task Flag恢復預設
                    IsStopTask = false;
                    break;
                }
            }
        }

        public void ComboItmeOfWebCam()
        {
            //偵測可用Web Camera
            FilterInfoCollection webCamItem = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            List<string> comboList = new List<string>();

            //將可用的Web Camera Insert入陣列內
            for (int i = 0; i < webCamItem.Count; i++)
            {
                comboList.Add(webCamItem[i].Name);
            }

            //Combo Box初始文字設定
            var firstItem = "無法偵測相機";
            if (comboList.Count() > 0)
            {
                firstItem = "請選擇相機";
            }
            comboList.Insert(0, firstItem);

            this.Dispatcher.Invoke(() =>
            {
                //將List 繫結至Combo Box
                this.WebCamCombo.ItemsSource = comboList;
                //預設選項
                this.WebCamCombo.SelectedIndex = 0;
                //文字置中
                this.WebCamCombo.VerticalContentAlignment = VerticalAlignment.Center;
            });
        }
        #endregion

        #region
        /// <summary>
        /// 啟動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetupButton_Click(object sender, RoutedEventArgs e)
        {
            FaceResultDelegate faceResultDelegate = new FaceResultDelegate(FaceResult);
            var webcamIndex = this.WebCamCombo.SelectedIndex - 1;

            //文字及顏色初始化
            CamStatus camStatus;

            //
            var returnMeg = new CamMegDelegate(handlerCollention.ReturnCamMeg);

            try
            {
                //賦予Camera串流資訊
                camCapture = new VideoCapture(webcamIndex, VideoCapture.API.Any);
                if (webcamIndex >= 0 && camCapture.IsOpened)
                {
                    //判斷是否停用相機
                    if (this.CameraSetupBtn.Content.Equals("停用"))
                    {
                        camCapture.Dispose();

                        //停用Camera
                        IsStopTask = true;

                        //Work
                        camStatus = CamStatus.Stop;
                        this.CameraSetupBtn.Content = "啟用";

                        this.Dispatcher.Invoke(faceResultDelegate, $"0 Face", Brushes.Black);
                        this.Dispatcher.Invoke(labelContentDelegate, this.TrainTip, Brushes.Black, "等候訓練啟動中 ...");
                    }
                    else
                    {
                        //恢復預設值
                        IsStopTask = false;

                        //Web Camera狀態,文字輸出
                        camStatus = CamStatus.Work;

                        //鎖定Combo Box以及Btn,避免重複操作
                        this.WebCamCombo.IsEnabled = false;

                        //
                        this.CameraSetupBtn.Content = "停用";

                        this.Dispatcher.Invoke(labelContentDelegate, this.TrainTip, Brushes.Green, "訓練中 ...");

                        //開啟子執行緒,執行Camera串流輸出
                        Task.Run(() => ShowCamera());
                    }
                }
                else
                {
                    //停止Task動作
                    IsStopTask = true;

                    //相機狀態,文字輸出
                    camStatus = CamStatus.NotFound;
                }
            }
            catch (Exception ex)
            {
                //停止Task動作
                IsStopTask = true;

                //Web Camera狀態,文字輸出
                camStatus = CamStatus.Exception;
            }

            //輸出Web Camera狀態
            this.Dispatcher.Invoke(returnMeg, this.WebCamContent, camStatus);
        }
        #endregion
    }
}
