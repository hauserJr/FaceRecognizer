using FaceRecognize_Wpf.Model.Configure;
using FaceRecognize_Wpf.Services;
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

namespace FaceRecognize_Wpf.UCWindows
{
    /// <summary>
    /// FaceConfigure.xaml 的互動邏輯
    /// </summary>
    public partial class FaceConfigure : Page
    {
        private JsonFileApp configureApp = new JsonFileApp();
        public FaceConfigure()
        {
            InitializeComponent();

            //預設值
            SetValueToUI();


            this.BaseScoreDesc.Content = $"臉部辨識分數越接近0辨識率越高" +
                $"\r\n在不同光源下分數都不一樣" +
                $"\r\n黃光約2500;白光約200" +
                $"\r\n以上分數皆為參考值，最終仍須實際情況調整";
        }

        /// <summary>
        /// 預設值
        /// </summary>
        public void SetValueToUI()
        {
            var appData = configureApp.GetConfigureFileData();
            this.BaseScore.Text = appData.faceBaseScore.ToString();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            var getData = configureApp.GetConfigureFileData();
            getData.faceBaseScore = double.Parse(this.BaseScore.Text);

            configureApp.UpdateConfigureFile(getData);
            SetValueToUI();

            MessageBox.Show("更新成功","完成");
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            configureApp.UpdateConfigureFile(new ConfigureModel());
            SetValueToUI();
            MessageBox.Show("回復預設成功", "完成");
        }
    }
}
