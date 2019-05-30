using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Emgu.CV.ML;
using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Threading;
using System.ComponentModel;
using static FaceRecognize_Wpf.Helper.ConvertType;
using FaceRecognize_Wpf.Model;
using System.Windows.Threading;
using System.IO;
using AForge.Video.DirectShow;
using FaceDetection;
using FaceRecognize_Wpf.Services;
namespace FaceRecognize_Wpf
{
    /*
     *  Y 執行緒結構
     *  Main--
     *       |-> Sub
     *       |-> Sub
     *       |-> Sub  
     *       
     *  *******************************
     *  
     *  N 執行緒結構(不要出現遞迴結構)
     *  Main--
     *       |-> Sub --
     *                |-> Sub
     *       |-> Sub --
     *                |-> Sub
     */
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        //初始化Camera
        public VideoCapture camCapture;

        //是否停止子執行緒 false = 不停 ,true = 停止
        public static bool IsStopTask = false;

        //是否進行拍照 false = 不拍 ,true = 拍
        public static bool IsTakeShot = false;

        private readonly SynchronizationContext syncContext = SynchronizationContext.Current;

        // 臉部辨識相關功能
        private FacesRepositores facesRepo = new FacesRepositores();

        private string cameraPath = $"{PathArgs.dataDomain}{PathArgs.facePicturePath}";
        private string faceSourcePath = $"{PathArgs.dataDomain}{PathArgs.faceDataPath}";
        private string faceConfigurePath = $"{PathArgs.dataDomain}";
        private string facedatPath = $"{PathArgs.dataDomain}{PathArgs.faceDataPath}{PathArgs.faceDateName}";

        public MainWindow()
        {
            InitializeComponent();

            //繫結Combo Box資料
            ComboItmeOfWebCam();

            //更新時間排程
            Task.Run(() => UpdateDate());

            //檢查Configure資料夾是否存在
            if (!File.Exists(faceConfigurePath))
            {
                Directory.CreateDirectory(faceConfigurePath);
            }

            //檢查圖片資料夾是否存在
            if (!File.Exists(cameraPath))
            {
                Directory.CreateDirectory(cameraPath);
            }

            //檢查臉部資料庫資料夾是否存在
            if (!File.Exists(faceSourcePath))
            {
                Directory.CreateDirectory(faceSourcePath);
            }

        }

        #region Handlers
        public delegate void MessageShoeDelegate(string Content, string Title);
        public delegate void CameraStatusDelegate(string Content, Brush brush);
        public delegate void TakeShotDelegate();
        public delegate void FaceResultDelegate(string Content, Brush brush);
        public delegate void FaceScoreDelegate(double Score);
        /// <summary>
        /// 修改Web Camera Status
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="brush"></param>
        public void CameraStatus(string Content, Brush brush)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.WebCamContent.Content = Content;
                this.WebCamContent.Foreground = brush;
            });
        }

        public void TakeShot()
        {
            MessageShoeDelegate messageShoeDelegate = new MessageShoeDelegate(MessageShow);
            var userName = this.UserName.Text;
            if (string.IsNullOrEmpty(userName) || userName.ToLower().Equals("unknow"))
            {
                messageShoeDelegate.Invoke("使用者名稱未輸入或出現不合法名稱", "注意");
            }
            else if (!camCapture.IsOpened)
            {
                messageShoeDelegate.Invoke("Camera尚未啟用或出現異常", "注意");
            }
            else
            {
                //取得目前Camera串流
                var camMat = camCapture.QueryFrame();

                //取得人臉區域的Rectangle
                var getFacesFeature = new FaceTrace().TraceFace(camMat);

                if (getFacesFeature.Faces.Count() > 0)
                {
                    //判斷是否有取得人臉
                    foreach (var faceItem in getFacesFeature.Faces)
                    {
                        //儲存人臉
                        //1. 進行大小處理 100 * 100
                        //2. 灰階處理
                        //3. 儲存
                        camMat.ToImage<Emgu.CV.Structure.Gray, byte>()
                            .GetSubRect(faceItem)
                            .Resize(100, 100, Inter.Cubic)
                            .Save($"{cameraPath}{ this.UserName.Text }.jpg");
                    }
                    messageShoeDelegate.Invoke("拍照完成請進行訓練", "成功");
                }
                else
                {
                    messageShoeDelegate.Invoke("未偵測到人臉", "注意");
                }

            }
        }

        public void FaceResult(string Content, Brush brush)
        {
            this.FaceResultText.Content = Content;
            this.FaceResultText.Foreground = brush;
        }

        public void MessageShow(string Content, string Title)
        {
            MessageBox.Show(Content, Title);
        }

        public void FaceScore(double Score)
        {
            this.FaceRecognizeScore.Content = Score.ToString("f2");
        }
        #endregion

        #region Action

        /// <summary>
        /// 在Combo Box顯示可選的Camera
        /// </summary>
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

        /// <summary>
        /// Web Cam畫面輸出
        /// </summary>
        public void ShowCamera()
        {
            //Web Camera開啟後的動作
            //進入無窮迴圈
            while (true)
            {
                try
                {
                    //當收到Task停止指示時
                    if (IsStopTask)
                    {
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
                            FaceResultDelegate faceResultDelegate = new FaceResultDelegate(FaceResult);
                            FaceScoreDelegate faceScoreDelegate = new FaceScoreDelegate(FaceScore);
                            if (getFaceData.Faces.Count() == 0)
                            {
                                this.Dispatcher.Invoke(faceResultDelegate, "NONE", Brushes.Black);
                            }
                            else
                            {
                                //判斷人臉數
                                foreach (var faceItem in getFaceData.Faces)
                                {
                                    //繪製人臉框
                                    CvInvoke.Rectangle(camMat, faceItem, new Bgr(System.Drawing.Color.Yellow).MCvScalar, 2);

                                    //判斷資料庫是否存在,不存在則不進入人臉辨識
                                    if (File.Exists(facedatPath))
                                    {
                                        //判斷人臉是否存在於資料庫
                                        var facePass = facesRepo.FacesRecognize(camCapture.QueryFrame());
                                        if (facePass.Item2)
                                        {
                                            this.Dispatcher.Invoke(faceResultDelegate, "PASS", Brushes.Green);
                                        }
                                        else
                                        {
                                            this.Dispatcher.Invoke(faceResultDelegate, "REJECT", Brushes.Red);
                                        }
                                        this.Dispatcher.Invoke(faceScoreDelegate, facePass.Item1);
                                    }
                                }
                            }


                            if (IsTakeShot)
                            {
                                IsTakeShot = !IsTakeShot;
                                var TSD = new TakeShotDelegate(TakeShot);
                                this.Dispatcher.Invoke(TSD);
                            }

                            //透過Invoke將資料傳遞至主UI上
                            this.Dispatcher.Invoke(() =>
                            {
                                this.CamView.Source = camMat.Bitmap.BitmapToBitmapImage();
                                this.FaceCount.Content = getFaceData.Faces.Count();
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    camCapture.Stop();
                    camCapture = null;
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

        /// <summary>
        /// 更新時間
        /// </summary>
        public void UpdateDate()
        {
            while (true)
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.NoDate.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                });
                Thread.Sleep(1000);
            }
        }

        #endregion

        #region Click Action

        /// <summary>
        /// 啟動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetupButton_Click(object sender, RoutedEventArgs e)
        {
            CameraStatusDelegate cameraStatusDelegate = new CameraStatusDelegate(CameraStatus);
            var webcamIndex = this.WebCamCombo.SelectedIndex - 1;

            //文字及顏色初始化
            var camContent = "";
            var fontColor = Brushes.Black;

            try
            {
                //賦予Camera串流資訊
                camCapture = new VideoCapture(webcamIndex, VideoCapture.API.Any);
                if (webcamIndex >= 0 && camCapture.IsOpened)
                {
                    //判斷是否停用相機
                    if (this.CameraSetupBtn.Content.Equals("停用"))
                    {
                        //停用Camera
                        IsStopTask = true;
                    }
                    else
                    {
                        //恢復預設值
                        IsStopTask = false;

                        //Web Camera狀態,文字輸出
                        camContent = "啟用";
                        fontColor = Brushes.Green;
                        
                        //鎖定Combo Box以及Btn,避免重複操作
                        this.WebCamCombo.IsEnabled = false;
                        this.CameraSetupBtn.Content = "停用";
                        //開啟子執行緒,執行Camera串流輸出
                        Task.Run(() => ShowCamera());
                    }
                }
                else
                {
                    //停止Task動作
                    IsStopTask = true;

                    //相機狀態,文字輸出
                    camContent = @"未選擇正確相機";
                    fontColor = Brushes.Red;
                }
            }
            catch(Exception ex)
            {
                //停止Task動作
                IsStopTask = true;

                //Web Camera狀態,文字輸出
                camContent = "異常";
                fontColor = Brushes.Red;
            }

            //輸出Web Camera狀態
            cameraStatusDelegate.Invoke(camContent, fontColor);
        }

        /// <summary>
        /// 拍照
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TakeShotButton_Click(object sender, RoutedEventArgs e)
        {
            MessageShoeDelegate messageShoeDelegate = new MessageShoeDelegate(MessageShow);
            if (camCapture == null || !camCapture.IsOpened)
            {
                messageShoeDelegate.Invoke("未偵測到可用WebCam", "注意");
            }
            else
            {
                IsTakeShot = true;
            }
        }

        /// <summary>
        /// 模型訓練
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrainingButton_Click(object sender, RoutedEventArgs e)
        {
            MessageShoeDelegate messageShoeDelegate = new MessageShoeDelegate(MessageShow);
            facesRepo.FaceTraining();
            messageShoeDelegate.Invoke("訓練完成","成功");
        }
        #endregion
    }
}
