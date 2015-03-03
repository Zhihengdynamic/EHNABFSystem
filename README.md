# 重度級急救醫院網絡空床數預測系統 (EHNABF System)
為提高醫療網救護之品質及安全，本系統考慮系統動態流程的觀念，快速地即時反應網絡內各醫院之病患相互轉院影響之動態病床使用資訊。網絡醫院病床預測資訊己考慮時間的概念，將可提高病床資訊的精準度，使病患到院後確切有病床的機率提高，也更有效率的資源分配規畫。本系統有以下特色：

1. 可預測及顯示醫院各類空床數量資訊在未來20, 40, 60分鐘等區間。
2. 提高資訊的精準度→醫院資訊考慮”時間”的概念，病患到院後確切有空床的機率提高。
3. 更有效率的資源規畫→若傷病情況超乎該院醫療資源或能力可以處置，則可盡早轉介至其他醫療院所或醫學中心，以節省時間。
4. 系統動態流程的觀念→快速地反應網絡內各醫院之病患相互轉院影響之空床數量資訊變化。

## Website
URL: http://ehnabf.azurewebsites.net/

備用: http://140.128.135.12/ERFWeb/

## 原始資料來源
[衛服部網頁](http://www.mohw.gov.tw/CHT/DOMA/DM1_P.aspx?f_list_no=608&fod_list_no=4680&doc_no=43081)上記載之各醫院網頁急診即時資訊。

## 原始資料擷取腳本來源
感謝[全國重度級急救責任醫院急診即時訊息](http://er.mohw.g0v.tw/#/dashboard/file/default.json)網站工作小組所公開的 web scraper script。

URL: https://ethercalc.org/twer

## API
本專案建立有Web API，提供查詢當下某時段後系統預測之醫院的空床數。

* URL: http://erbedserver.cloudapp.net/ERFSystem/ERFSystem.svc/Forcasting/time/ + 預測分鐘數

  METHOD: GET

  Return:
  
  
          {
              "Hospital_ID": 醫院代號 (string),       
              "Basetime": 預測時當下時間 (timestamp),    
              "Forecast_Time": 預測多久後的空床數，以分鐘為單位 (int),       
              "HpF_BED": [lower, mean, upper ]  預測之推床空床數 (double),
              "HpF_ICU": [lower, mean, upper ]  預測之加護病房空床數 (double),
              "HpF_WARN": [lower, mean, upper ] 預測之普通病房空床數 (double)
          }




使用範例：

* 預測20分鐘後之醫院床數

  URL: http://erbedserver.cloudapp.net/ERFSystem/ERFSystem.svc/Forcasting/time/20

* 預測35分鐘後之醫院床數

  URL: http://erbedserver.cloudapp.net/ERFSystem/ERFSystem.svc/Forcasting/time/35
  
##Mobile app
URL: https://www.dropbox.com/s/n294v48lq6hgspc/EHNABF-app.apk?dl=0





 

