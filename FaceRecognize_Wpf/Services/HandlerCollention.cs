using FaceRecognize_Wpf.Emun;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System;
using System.Threading;

namespace FaceRecognize_Wpf.Services
{
    /// <summary>
    /// Message Box
    /// </summary>
    /// <param name="Content"></param>
    /// <param name="Title"></param>
    public delegate void MessageShoeDelegate(string Content, string Title);

    /// <summary>
    /// Camera狀態
    /// </summary>
    /// <param name="cameraLabel"></param>
    /// <param name="camStatus"></param>
    public delegate void CamMegDelegate(Label cameraLabel, CamStatus camStatus);
    
    /// <summary>
    /// Label內容及文字
    /// </summary>
    /// <param name="textLabel"></param>
    /// <param name="solidColorBrush"></param>
    /// <param name="Message"></param>
    public delegate void LabelContentDelegate(Label textLabel, SolidColorBrush solidColorBrush, string Message);

    /// <summary>
    /// 拍照
    /// </summary>
    public delegate void TakeShotDelegate();

    /// <summary>
    /// 更新Name Json File
    /// </summary>
    public delegate void UpdateNameJsonFileDelegate();


    public class HandlerCollention
    {
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
        /// Camera 狀態
        /// </summary>
        /// <param name="cameraLabel"></param>
        /// <param name="camStatus"></param>
        public void ReturnCamMeg(Label cameraLabel, CamStatus camStatus)
        {
            string returnMsg = @"狀態無法評估";
            var brushesColor = Brushes.Red;
            switch (camStatus)
            {
                case CamStatus.Work:
                    returnMsg = @"啟用";
                    brushesColor = Brushes.Green;
                    break;
                case CamStatus.Stop:
                    returnMsg = @"停用";
                    break;
                case CamStatus.NotFound:
                    returnMsg = @"找不到Camera";
                    break;

                //Exception
                case CamStatus.Exception:
                    returnMsg = @"異常，請重啟應用程式。";
                    break;
            }

            SetContent(cameraLabel, brushesColor, returnMsg);
        }

        /// <summary>
        /// 設定Label文字顏色
        /// </summary>
        /// <param name="textLabel"></param>
        /// <param name="solidColorBrush"></param>
        /// <param name="Message"></param>
        public void SetContent(Label textLabel, SolidColorBrush solidColorBrush, string Message)
        {
            textLabel.Content = Message;
            textLabel.Foreground = solidColorBrush;
        }

        /// <summary>
        /// 更新人名清冊
        /// </summary>
        public void UpdateNameJsonFile()
        {
            NameCollection.Update();
        }


    }
}
