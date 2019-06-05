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
using FaceRecognize_Wpf.Emun;

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

        private DateTime? takeFeatures;

        //建議分數
        private static List<double> scoreAvg = new List<double>();

        //Handlers Method Collention
        private static HandlerCollention handlerCollention = new HandlerCollention();

        //MessageBox Handlers
        MessageShoeDelegate messageShoeDelegate
                = new MessageShoeDelegate(handlerCollention.MessageShow);

        //Label Handler
        LabelContentDelegate labelContentDelegate
                = new LabelContentDelegate(handlerCollention.SetContent);

        public FaceControl()
        {
            InitializeComponent();

            //繫結Combo Box資料
            ComboItmeOfWebCam();

            this.AutoShot.IsEnabled = false;
        }

        #region Handlers
        public delegate void TakeShotDelegate();
        public delegate void FaceResultDelegate(string Content, Brush brush);
        /// <summary>
        /// 拍照存放圖片
        /// </summary>
        public void TakeShot()
        {
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
                takeFeatures = (DateTime?)null;
                messageShoeDelegate.Invoke("員工編號需要為數字或編號已存在", "注意");
            }
            else if (string.IsNullOrEmpty(userName) || userName.ToLower().Equals("unknow"))
            {
                takeFeatures = (DateTime?)null;
                messageShoeDelegate.Invoke("使用者名稱未輸入或出現不合法名稱", "注意");
            }
            else if (!camCapture.IsOpened)
            {
                takeFeatures = (DateTime?)null;
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
                    //Lower UserName 
                    var lowerUserName = userName.ToLower();
                    var userPictureDir = $"{ cameraPath }{ employeeNum }.{ lowerUserName }";
                    facesRepo.SavePicture(camMat, getFacesFeature, userPictureDir, lowerUserName);
                }
                else
                {
                    //messageShoeDelegate.Invoke("未偵測到人臉", "注意");
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
                            if (getFaceData.Faces.Count() == 0)
                            {
                                this.Dispatcher.Invoke(faceResultDelegate, "NONE", Brushes.Black);
                            }
                            else
                            {
                                if (takeFeatures.HasValue && takeFeatures >= DateTime.Now)
                                {   
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        var showSecond = (takeFeatures.Value.Second - DateTime.Now.Second);
                                        if (showSecond <= 0 || !takeFeatures.HasValue)
                                        {
                                            takeFeatures = (DateTime?)null;
                                            this.Seconds.Text = "0";
                                        }
                                        else
                                        {
                                            this.Seconds.Text = (takeFeatures.Value.Second - DateTime.Now.Second).ToString();
                                        }
                                    });
                                    this.Dispatcher.Invoke(TakeShot);
                                }

                                //判斷人臉數
                                foreach (var faceItem in getFaceData.Faces)
                                {
                                    //繪製人臉框
                                    CvInvoke.Rectangle(camMat, faceItem, new Bgr(System.Drawing.Color.Yellow).MCvScalar, 2);

                                    //開始計時
                                    Stopwatch stopWatch = new Stopwatch();
                                    stopWatch.Start();
                                    //判斷人臉是否存在於資料庫
                                    var facePass = facesRepo.FacesRecognize(camCapture.QueryFrame());
                                    //計時停止
                                    stopWatch.Stop();
                                    string stopWatchResult = stopWatch.Elapsed.TotalSeconds.ToString();

                                    if (facePass.Item3)
                                    {
                                        var userData = NameCollection.UserTable.Where(o => o.Key == facePass.Item2.ToString()).FirstOrDefault();

                                        this.Dispatcher.Invoke(labelContentDelegate, this.FaceRecognizeScore, Brushes.Black, facePass.Item1.ToString("f2"));

                                        var userName = "Unknow";
                                        if (userData != null)
                                        {
                                            userName = userData.Name;
                                        }
                                        
                                        this.Dispatcher.Invoke(faceResultDelegate, userName + "\r\nPASS", Brushes.Green);

                                        //臉部分數及平均分數
                                        FaceScoreAvg(facePass.Item1);
                                    }
                                    else
                                    {
                                        this.Dispatcher.Invoke(faceResultDelegate, "REJECT", Brushes.Red);
                                    }
                                    this.Dispatcher.Invoke(labelContentDelegate, this.IDSpeed, Brushes.Black, stopWatchResult);
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
                    //MessageBox.Show("發生錯誤，請重啟應用程式","注意");
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

        #endregion

        #region Click Action

        /// <summary>
        /// 啟動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetupButton_Click(object sender, RoutedEventArgs e)
        {
            var webcamIndex = this.WebCamCombo.SelectedIndex - 1;

            //文字及顏色初始化
            CamStatus camStatus ;
            
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
                        //啟用訓練
                        EnableTrainBtn();

                        //停用Camera
                        IsStopTask = true;

                        //停用連拍
                        this.AutoShot.IsEnabled = false;
                        //Work
                        camStatus = CamStatus.Stop;
                        this.CameraSetupBtn.Content = "啟用";

                    }
                    else
                    {
                        //停用訓練
                        this.TrainingBtn.IsEnabled = false;
                        //啟用連拍
                        this.AutoShot.IsEnabled = true;
                        //恢復預設值
                        IsStopTask = false;

                        //Web Camera狀態,文字輸出
                        camStatus = CamStatus.Work;

                        //鎖定Combo Box以及Btn,避免重複操作
                        this.WebCamCombo.IsEnabled = false;

                        //
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
                    camStatus = CamStatus.NotFound;

                    //停用連拍
                    this.AutoShot.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                //停止Task動作
                IsStopTask = true;

                //停用連拍
                this.AutoShot.IsEnabled = false;

                //Web Camera狀態,文字輸出
                camStatus = CamStatus.Exception;
            }

            //輸出Web Camera狀態
            this.Dispatcher.Invoke(returnMeg,this.WebCamContent, camStatus);
        }

        /// <summary>
        /// 拍照
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TakeShotButton_Click(object sender, RoutedEventArgs e)
        {
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
        private async void TrainingButton_Click(object sender, RoutedEventArgs e)
        {
            //訓練資料

            //訓練時 所有按鈕關閉
            this.TrainingBtn.IsEnabled = false;
            this.CameraSetupBtn.IsEnabled = false;
            this.TakeShotBtn.IsEnabled = false;
            this.UseBase.IsEnabled = false;

            this.FaceResultText.Content = "訓練中 ...";
            var asyncTrain = Task.Run(() => TrainingData());
            
            //等候訓練
            await asyncTrain;

            //訓練完成
            this.TrainingBtn.IsEnabled = true;
            this.CameraSetupBtn.IsEnabled = true;
            this.TakeShotBtn.IsEnabled = true;
            this.UseBase.IsEnabled = true;

            this.FaceResultText.Content = "訓練完成 ...";

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
            dataApp.faceBaseScore = scoreAvg.Average().ToString();
            configureApp.UpdateConfigureFile(dataApp);

            MessageBox.Show("變更成功，請重啟系統。","成功");
           
           
        }

        /// <summary>
        /// 定時連拍
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoShot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var IsIntSeconds = int.TryParse(this.Seconds.Text, out var outSeconds);
                if (IsIntSeconds)
                {
                    takeFeatures = DateTime.Now.AddSeconds(outSeconds);
                    //this.AutoShot.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("秒數為空或非數字。", "警告");
            }
        }
        #endregion

        #region Other Methon
        public async void EnableTrainBtn()
        {
            Thread.Sleep(500);
            this.TrainingBtn.IsEnabled = true;
        }

        public async Task TrainingData()
        {
            UpdateNameJsonFileDelegate updateNameJsonFileDelegate
                = new UpdateNameJsonFileDelegate(handlerCollention.UpdateNameJsonFile);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //開始訓練
            var pictureCount = facesRepo.MultipleFaceTraining();

            //計時停止
            stopWatch.Stop();
            string stopWatchResult = stopWatch.Elapsed.TotalSeconds.ToString();

            //更新人名清單
            this.Dispatcher.Invoke(updateNameJsonFileDelegate);

            //顯示資訊
            messageShoeDelegate.Invoke($"訓練完成" +
                $"\r\n已訓練樣本數：{pictureCount}" +
                $"\r\n耗時：{stopWatchResult}" +
                $"\r\n資料於重啟系統後生效。", "成功");
        }

        /// <summary>
        /// 臉部辨識分數及建議基數計算
        /// </summary>
        /// <param name="Score"></param>
        private void FaceScoreAvg(double Score)
        {
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

                this.Dispatcher.Invoke(() =>
                {
                    this.BaseScoreAvg.Content = scoreAvg.Average().ToString("f2");
                });
            }
        }
        #endregion
    }
}
