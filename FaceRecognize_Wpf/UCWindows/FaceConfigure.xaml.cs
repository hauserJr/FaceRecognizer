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
        }

        /// <summary>
        /// 預設值
        /// </summary>
        public void SetValueToUI()
        {
            var appData = configureApp.GetConfigureFileData();
            this.BaseScore.Text = appData.faceBaseScore;
            this.BaseScoreMin.Text = appData.faceMLMinScore;
            this.BaseScoreMax.Text = appData.faceMLMaxScore;
            this.SamplePictureMax.Text = appData.samplePicutreMax;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            var getData = configureApp.GetConfigureFileData();

            if (Int32.TryParse(this.SamplePictureMax.Text, out var numOutpur))
            {
                var saveValue = numOutpur <= 0 || numOutpur  > 999 ? -1 : numOutpur;
                getData.samplePicutreMax = saveValue.ToString();
            }


            getData.faceBaseScore = this.BaseScore.Text;
            getData.faceMLMinScore = this.BaseScoreMin.Text;
            getData.faceMLMaxScore = this.BaseScoreMax.Text;

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
