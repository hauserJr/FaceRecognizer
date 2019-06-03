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
using FaceRecognize_Wpf.UCWindows;

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

        public MainWindow()
        {
            InitializeComponent();

            Task.Run(() => UpdateDate());

            FaceControl faceControl = new FaceControl();
            this.FrameContainer.Navigate(faceControl);
        }
        private void FaceInit_Click(object sender, RoutedEventArgs e)
        {
            FaceControl faceControl = new FaceControl();
            this.FrameContainer.Navigate(faceControl);
        }

        private void FaceRecognize_Click(object sender, RoutedEventArgs e)
        {
            FaceRecognize faceRecognize = new FaceRecognize();
            this.FrameContainer.Navigate(faceRecognize);
        }

        private void FaceConfigure_Click(object sender, RoutedEventArgs e)
        {
            FaceConfigure faceConfigure = new FaceConfigure();
            this.FrameContainer.Navigate(faceConfigure);
        }

        private void FaceAutoLearning_Click(object sender, RoutedEventArgs e)
        {
            FaceAutoLearning faceAutoLearning = new FaceAutoLearning();
            this.FrameContainer.Navigate(faceAutoLearning);
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
                    this.NowDate.Content = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}" +
                    $" {DateConvert.TWDayOfWeek(DateTime.Now)}";
                });
                Thread.Sleep(1000);
            }
        }
    }
}
