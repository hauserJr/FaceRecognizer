using FaceRecognize_Wpf.Model.Configure;
using FaceRecognize_Wpf.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FaceRecognize_Wpf
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        private string cameraPath = $"{PathArgs.dataDomain}{PathArgs.facePicturePath}";
        private string faceSourcePath = $"{PathArgs.dataDomain}{PathArgs.faceDataPath}";
        private string faceConfigurePath = $"{PathArgs.dataDomain}";
        private string facedatPath = $"{PathArgs.dataDomain}{PathArgs.faceDataPath}{PathArgs.faceDateName}";
        private string configureFilePath = $"{PathArgs.dataDomain}/{PathArgs.configureFile}";

        protected override void OnStartup(StartupEventArgs e)
        {
            Task.Run(() => NameCollection.UserTable);

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

            //判斷資料庫是否存在,不存在則不進入人臉辨識
            if (!File.Exists(facedatPath))
            {
                File.Create(facedatPath);
            }

            //檢查設定檔是否存在
            if (!File.Exists(configureFilePath))
            {
                FileStream fileStream = new FileStream(configureFilePath, FileMode.Create);
                fileStream.Close();

                new JsonFileApp().UpdateConfigureFile(new ConfigureModel());
            }
        }
    }
}
