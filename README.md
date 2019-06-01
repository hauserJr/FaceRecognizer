
# 臉部辨識系統說明 #
1. 自動臉部追蹤
2. 自動臉部辨識
3. 支援一鍵資料訓練
4. 支援離線辨識
5. 可自選Camera
6. 辨識度高(資料訓練充足可達95%以上)
7. 可自訂臉部基數或使用系統推薦

# 額外功能說明 #
1. 支援多臉追蹤
2. 支援多臉辨識(目前設定僅辨識最近的)
3. 計算臉部基數平均(判斷是否成功的基準)

# 可應用環境 #
1. .NET Framewokr 4.7+
2. .NET Core 2.0+

專案使用：NET Framewokr 4.7

# 使用套件 #
1. Emgu.CV 4.1.0
   - 人臉追蹤、辨識、訓練 
2. AFrog 2.2.5
   - Camera選項、設定

# DEMO #
說明：
計算臉部特徵值來比對達到人臉辨識效果

操作步驟：
1. 啟用Camera 
2. 輸入姓名(英文)
3. 輸入編號(唯一值)
3. 拍照
4. 訓練

結果：

> 訓練前REJECT
![image](https://i.imgur.com/OKHXuGH.png)

> 訓練後PASS
![image](https://i.imgur.com/jzZH8uS.png)

# 注意 #
1. Distance分數越低越符合訓練結果，但實際數字仍須自行調整
2. 需要至少有2筆不同人的訓練資料，才可進行比對
3. 部分功能需自行從[EmguCV Github](https://github.com/emgucv/emgucv)移植
   - 如../EmguCVSourceCode/DetectFace.cs內的功能
4. 要辨識臉及眼的位置需要下列兩種xml，可從[OpenCV Github下載](https://github.com/opencv/opencv/tree/master/data/haarcascades)
   - haarcascade_frontalface_default.xml
   - haarcascade_eye.xml
5. 如果要建立訓練模組可利用[AI人臉自動生成](https://thispersondoesnotexist.com/)來取得資訊

# 建議 #
可以從各種角度拍攝人臉，蒐集的越完整辨識度越高，誤判機率越低。
