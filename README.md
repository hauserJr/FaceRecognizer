
# 臉部辨識系統說明 #
1. 自動臉部追蹤
2. 自動臉部辨識
3. 支援一鍵資料訓練
4. 支援離線辨識
5. 可自選Camera
6. 辨識度高(資料訓練充足可達95%以上)

# 額外功能說明 #
1. 支援多臉追蹤
2. 支援多臉辨識(目前設定僅辨識最近的)

# 開發環境 #
1. .NET Framewokr 4.7+
2. .NET CORE 2.1+

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
3. 拍照
4. 訓練

結果：
> PASS
![image](https://i.imgur.com/4NCkUAZ.png)

> REJECT
![image](https://i.imgur.com/A6RAY7o.png)

注意：
1. Distance分數越低越符合訓練結果，但實際數字仍須自行調整
2. 訓練圖片需要有2張以上(含兩張)，才可進行比對
