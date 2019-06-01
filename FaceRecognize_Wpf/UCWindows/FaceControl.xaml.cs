using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Emgu.CV;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FaceRecognize_Wpf.Services;
using Emgu.CV.CvEnum;
using AForge.Video.DirectShow;
using Emgu.CV.Structure;
using FaceRecognize_Wpf.Helper;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

namespace FaceRecognize_Wpf.UCWindows
{
    /// <summary>
    /// FaceControl.xaml 的互動邏輯
    /// </summary>
    public partial class FaceControl : Page
    {
        //初始化Camera
        public VideoCapture camCapture;

        //是否停止子執行緒 false = 不停 ,true = 停止
        public static bool IsStopTask = false;

        //是否進行拍照 false = 不拍 ,true = 拍
        public static bool IsTakeShot = false;

        // 臉部辨識相關功能
        private FacesRepositores facesRepo = new FacesRepositores();

        //圖片存放位置
        private string cameraPath = $"{PathArgs.dataDomain}{PathArgs.facePicturePath}";

        //建議分數
        private static List<double> scoreAvg = new List<double>();

        public FaceControl()
        {
            InitializeComponent();

            //繫結Combo Box資料
            ComboItmeOfWebCam();

            //更新時間排程
            Task.Run(() => UpdateDate());
        }


        #region Handlers
        public delegate void MessageShoeDelegate(string Content, string Title);
        public delegate void CameraStatusDelegate(string Content, Brush brush);
        public delegate void TakeShotDelegate();
        public delegate void FaceResultDelegate(string Content, Brush brush);
        public delegate void FaceScoreDelegate(double Score);
        public delegate void UpdateCollentionDelegate();

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

        /// <summary>
        /// 拍照存放圖片
        /// </summary>
        public void TakeShot()
        {
            MessageShoeDelegate messageShoeDelegate = new MessageShoeDelegate(MessageShow);
            var userName = this.UserName.Text;
            var employeeNum = this.EmployeeNum.Text;


            var IsUser = NameCollection.UserTable.Where(o => o.Key == employeeNum).FirstOrDefault();
            //編號不可有以下情況
            //1. 空值
            //2. 非數字
            //3. 已存在
            if (string.IsNullOrEmpty(employeeNum) 
                || Regex.IsMatch(employeeNum, @"^[+-]?/d*$")
                || (IsUser != null && IsUser.Name != userName))
            {
                messageShoeDelegate.Invoke("員工編號需要為數字或編號已存在", "注意");
            }
            else if (string.IsNullOrEmpty(userName) || userName.ToLower().Equals("unknow"))
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
                    var userPictureDir = $"{ cameraPath }{employeeNum}.{ userName }";
                    //判斷該照片資料夾是否存在
                    if (!File.Exists(userPictureDir))
                    {
                        Directory.CreateDirectory(userPictureDir);
                    }
                    //判斷是否有取得人臉
                    foreach (var faceItem in getFacesFeature.Faces)
                    {
                        //儲存人臉
                        //1. 進行大小處理 100 * 100
                        //2. 灰階處理
                        //3. 儲存

                        //取得目錄底下的圖片數量
                        var getPictureCount = facesRepo.GetFileCount(userPictureDir);

                        camMat.ToImage<Emgu.CV.Structure.Gray, byte>()
                            .GetSubRect(faceItem)
                            .Resize(100, 100, Inter.Cubic)
                            .Save($"{userPictureDir}/{ this.UserName.Text }_{ getPictureCount }.jpg");
                    }
                    messageShoeDelegate.Invoke("拍照完成請進行訓練", "成功");
                }
                else
                {
                    messageShoeDelegate.Invoke("未偵測到人臉", "注意");
                }

            }
        }

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
        /// Message Box Show
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="Title"></param>
        public void MessageShow(string Content, string Title)
        {
            MessageBox.Show(Content, Title);
        }

        /// <summary>
        /// 臉部辨識分數及建議基數計算
        /// </summary>
        /// <param name="Score"></param>
        public void FaceScore(double Score)
        {
            this.FaceRecognizeScore.Content = Score.ToString("f2");

            //臉部基數設定
            //只取100筆做平均計算
            if (Score >= 1)
            {
                scoreAvg.Add(Score);
                if (scoreAvg.Count > 100)
                {
                    //超過100筆,移除第1筆
                    scoreAvg.RemoveAt(0);
                }
                this.BaseScoreAvg.Content = scoreAvg.Average().ToString("f2");
            }
        }

        /// <summary>
        /// 更新人名清冊
        /// </summary>
        public void UpdateCollention()
        {
            NameCollection.Update();
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
                //每80ms執行一次即可
                Thread.Sleep(80);
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

                                    //判斷人臉是否存在於資料庫
                                    var facePass = facesRepo.FacesRecognize(camCapture.QueryFrame());
                                    if (facePass.Item3)
                                    {
                                        var userData = NameCollection.UserTable.Where(o => o.Key == facePass.Item2.ToString()).FirstOrDefault();
                                        this.Dispatcher.Invoke(faceScoreDelegate, facePass.Item1);
                                        this.Dispatcher.Invoke(faceResultDelegate, userData.Name + "\r\nPASS", Brushes.Green);
                                    }
                                    else
                                    {
                                        this.Dispatcher.Invoke(faceResultDelegate, "REJECT", Brushes.Red);
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

                        //啟用訓練
                        this.TrainingBtn.IsEnabled = true;
                    }
                    else
                    {
                        //停用訓練
                        this.TrainingBtn.IsEnabled = false;

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
            catch (Exception ex)
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
            UpdateCollentionDelegate updateCollentionDelegate = new UpdateCollentionDelegate(UpdateCollention);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            
            //開始訓練
            var pictureCount = facesRepo.FaceTraining();

            //計時停止
            stopWatch.Stop();
            string stopWatchResult = stopWatch.Elapsed.TotalSeconds.ToString();

            //更新人名清單
            this.Dispatcher.Invoke(updateCollentionDelegate);
            
            //顯示資訊
            messageShoeDelegate.Invoke($"訓練完成" +
                $"\r\n已訓練樣本數：{pictureCount}" +
                $"\r\n耗時：{stopWatchResult}", "成功");
        }
        
        /// <summary>
        /// 使用按鈕設定臉部分數基數
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UseBase_Click(object sender, RoutedEventArgs e)
        {
            JsonFileApp configureApp = new JsonFileApp();
            var dataApp = configureApp.GetConfigureFileData();
            dataApp.faceBaseScore = scoreAvg.Average();
            configureApp.UpdateConfigureFile(dataApp);

            MessageBox.Show("變更成功，請重啟系統。","成功");
           
           
        }
        #endregion
    }
}
