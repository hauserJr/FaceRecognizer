using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FaceRecognize_Wpf.Model.Configure;
using Newtonsoft.Json;

namespace FaceRecognize_Wpf.Services
{
    public class JsonFileApp
    {
        private string configureFilePath = $"{PathArgs.dataDomain}/{PathArgs.configureFile}";
        public JsonFileApp()
        {

        }

        /// <summary>
        /// 更新Json檔內容
        /// </summary>
        /// <param name="Data"></param>
        public void UpdateConfigureFile(ConfigureModel Data)
        {
            File.WriteAllText(configureFilePath
                , JsonConvert.SerializeObject(Data, Formatting.Indented));
        }

        /// <summary>
        /// 取得Json檔內容
        /// </summary>
        /// <returns></returns>
        public ConfigureModel GetConfigureFileData()
        {
            var configureModel = new ConfigureModel();
            // Open the stream and read it back.
            using (FileStream fs = File.OpenRead(configureFilePath))
            {
                byte[] b = new byte[1024];
                UTF8Encoding temp = new UTF8Encoding(true);

                while (fs.Read(b, 0, b.Length) > 0)
                {
                    configureModel = JsonConvert.DeserializeObject<ConfigureModel>(temp.GetString(b));
                }
            }
            return configureModel;
        }
    }
}
