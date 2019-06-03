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
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FaceRecognize_Wpf
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        private string cameraPath = $"{PathArgs.dataDomain}{PathArgs.facePicturePath}";
        private string faceSourcePath = $"{PathArgs.dataDomain}{PathArgs.faceDataPath}";
        private string faceSourcePath_Backup = $"{PathArgs.dataDomain}{PathArgs.faceDataBackupPath}";
        private string faceConfigurePath = $"{PathArgs.dataDomain}";
        private string facedatPath = $"{PathArgs.dataDomain}{PathArgs.faceDataPath}{PathArgs.faceDateName}";
        private string facedatPath_Bckup = $"{PathArgs.dataDomain}{PathArgs.faceDataBackupPath}{PathArgs.faceDateName}";
        private string configureFilePath = $"{PathArgs.dataDomain}/{PathArgs.configureFile}";

        protected override void OnStartup(StartupEventArgs e)
        {
            Task.Run(() => NameCollection.UserTable);

            //檢查Configure資料夾是否存在
            if (!Directory.Exists(faceConfigurePath))
            {
                Directory.CreateDirectory(faceConfigurePath);
            }

            //檢查圖片資料夾是否存在
            if (!Directory.Exists(cameraPath))
            {
                Directory.CreateDirectory(cameraPath);
            }

            //檢查臉部資料庫資料夾是否存在
            if (!Directory.Exists(faceSourcePath))
            {
                Directory.CreateDirectory(faceSourcePath);
            }
            //檢查臉部資料庫資料備份夾是否存在
            if (!Directory.Exists(faceSourcePath_Backup))
            {
                Directory.CreateDirectory(faceSourcePath_Backup);
            }

            //判斷資料庫是否存在,不存在則不進入人臉辨識
            if (!File.Exists(facedatPath))
            {
                File.Create(facedatPath).Dispose();
            }

            //判斷備份資料庫是否存在,不存在則不進入人臉辨識
            if (!File.Exists(facedatPath_Bckup))
            {
                File.Create(facedatPath_Bckup).Dispose();
            }

            //Merge data檔案資料
            var bcakaupFileInfo = new FileInfo(facedatPath_Bckup);
            var fileInfo = new FileInfo(facedatPath);
            if (bcakaupFileInfo.Length > fileInfo.Length)
            {
                File.Copy(facedatPath_Bckup, facedatPath, true); 
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
