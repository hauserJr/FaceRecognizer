using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.Structure;
using FaceRecognize_Wpf.Emun;
using FaceRecognize_Wpf.Helper;
using FaceRecognize_Wpf.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace FaceRecognize_Wpf.UCWindows
{
    /// <summary>
    /// FaceRecognize.xaml 的互動邏輯
    /// </summary>
    public partial class FaceRecognize : UserControl
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

        //Handlers
        MessageShoeDelegate messageShoeDelegate
                = new MessageShoeDelegate(handlerCollention.MessageShow);

        LabelContentDelegate labelContentDelegate
                = new LabelContentDelegate(handlerCollention.SetContent);

        public FaceRecognize()
        {
            InitializeComponent();

            //繫結Combo Box資料
            ComboItmeOfWebCam();

        }


        #region Handlers
        public delegate void FaceResultDelegate(string Content, Brush brush);
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
                                //不顯示找無人臉的資訊
                                //this.Dispatcher.Invoke(faceResultDelegate, "NONE", Brushes.Black);
                            }
                            else
                            {
                                var faceRectColor = System.Drawing.Color.Red;
                                //判斷人臉數
                                foreach (var faceItem in getFaceData.Faces)
                                {
                                    //判斷人臉是否存在於資料庫
                                    var facePass = facesRepo.FacesRecognize(camCapture.QueryFrame());
                                    if (facePass.Item3)
                                    {
                                        faceRectColor = System.Drawing.Color.Green;
                                        var userData = NameCollection.UserTable.Where(o => o.Key == facePass.Item2.ToString()).FirstOrDefault();

                                        //臉部分數
                                        var userName = @"Unkonw";
                                        if (userData != null)
                                        {
                                            userName = userData.Name;
                                        }

                                        this.Dispatcher.Invoke(labelContentDelegate, this.FaceRecognizeScore, Brushes.Black, facePass.Item1.ToString("f2"));
                                        this.Dispatcher.Invoke(faceResultDelegate,  DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+ "\r\n" + userName + "\r\nPASS", Brushes.Green);
                                    }
                                    else
                                    {
                                        //不顯示REJECT資訊
                                        //this.Dispatcher.Invoke(faceResultDelegate, "REJECT", Brushes.Red);
                                    }

                                    //繪製人臉框
                                    CvInvoke.Rectangle(camMat, faceItem, new Bgr(faceRectColor).MCvScalar, 2);
                                }
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
            CamStatus camStatus;

            //文字及顏色初始化
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
                        //停用Camera
                        IsStopTask = true;
                        camStatus = CamStatus.Stop;
                    }
                    else
                    {
                        //恢復預設值
                        IsStopTask = false;

                        //Web Camera狀態,文字輸出
                        camStatus = CamStatus.Stop;
                        this.CameraSetupBtn.Content = "啟用";

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

                    //Web Camera狀態,文字輸出
                    camStatus = CamStatus.NotFound;
                }
            }
            catch (Exception ex)
            {
                //停止Task動作
                IsStopTask = true;

                //相機狀態,文字輸出
                camStatus = CamStatus.Exception;
            }

            //輸出Web Camera狀態
            this.Dispatcher.Invoke(returnMeg, this.WebCamContent, camStatus);
        }


        #endregion
    }
}
