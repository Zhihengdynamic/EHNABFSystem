<%@ Page Title="EHNABF System" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="ERFWebApplication._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">

</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
<div id="abouttext">    
<table cellspacing="0" cellpadding="5" border="0" style="background-color: transparent;">
<tr>
  <td style="width:70%; vertical-align:top;">
  　為提高醫療網救護之品質及安全， 本系統考慮系統動態流程的觀念，快速地即時反應網絡內各醫院之病患相互轉院影響之動態病床使用資訊。網絡醫院病床預測資訊己考慮時間的概念，將可提高病床資訊的精準度，使病患到院後確切有病床的機率提高，也更有效率的資源分配規畫。本系統有以下特色：
<ul style="list-style-type:circle">
<li>可預測及顯示醫院各類空床數量資訊在未來20, 40, 60分鐘等區間。</li>
<li>提高資訊的精準度→醫院資訊考慮”時間”的概念，病患到院後確切有空床的機率提高。</li>
<li>更有效率的資源規畫→若傷病情況超乎該院醫療資源或能力可以處置，則可盡早轉介至其他醫療院所或醫學中心，以節省時間。</li>
<li>系統動態流程的觀念→快速地反應網絡內各醫院之病患相互轉院影響之空床數量資訊變化。</li>
</ul>


<p style="text-align:left;"> 累計瀏覽人數：<img src="http://counter1.fc2.com/counter_img.php?id=27501949&main=1" alt="odsm logo" /></p>

</td>
<td style="width:30%; background-color:#fff; text-align:left; vertical-align:bottom; color:#000;" >
<img src="img/logo2.jpg" width="75%" alt="logo" /><br/>
最佳決策系統研究團隊
<ul style="list-style-type:circle">
<li>東海大學工業工程與經營資訊系 <a href="http://web.thu.edu.tw/sjweng/www/" target="_blank" style="color: #CC0000">翁紹仁</a>老師</li>
<li>弘光科技大學資訊工程系 <a href="http://csie.hk.edu.tw/info/73" target="_blank" style="color: #CC0000">徐永煜</a>老師</li>
<li>台中榮民總醫院嘉義分院 <a href="http://www.vhcy.gov.tw/vhcy/index.php?module=CurerInfo&action=user_detail&sn=693" target="_blank" style="color: #CC0000">王立敏</a>副院長（前中區緊急醫療應變中心執行長）</li>
</ul>

</td>

</tr>
</table>
</div>

<div id="css_table" style="width:100%" >
    <div class="css_tr">
        <div class="css_td" style="text-align:left;vertical-align:bottom;width:50%;">
            <h2>
                請輸入救護車位置
            </h2>
   
           <p><input type="text" size="60" name="address" value="台中" /><input type="submit" value="Go!" /></p>
    
        </div>   
        
        <div class="css_td" style="text-align:left;vertical-align:bottom;width:50%;">
           <div id="suggesttext" style="display: none;font-size: 1.5em; font-weight: 600;">建議醫院：</div> 
             
           <button type="button" id="Button0" style="display: none;" onclick='showinfoButtonclick(infowindow[0],hospMarker[0],event)' >台北榮民總醫院</button>
           <button type="button" id="Button1" style="display: none;" onclick='showinfoButtonclick(infowindow[1],hospMarker[1],event)'>林口長庚醫院</button>
           <button type="button" id="Button2" style="display: none;" onclick='showinfoButtonclick(infowindow[2],hospMarker[2],event)'>台中榮民總醫院</button>
           <button type="button" id="Button3" style="display: none;" onclick='showinfoButtonclick(infowindow[3],hospMarker[3],event)'>台北市立萬芳醫院</button>
           <button type="button" id="Button4" style="display: none;" onclick='showinfoButtonclick(infowindow[4],hospMarker[4],event)'>國立成功大學醫學院附設醫院</button>
           <button type="button" id="Button5" style="display: none;" onclick='showinfoButtonclick(infowindow[5],hospMarker[5],event)'>花蓮慈濟醫院</button>
           <button type="button" id="Button6" style="display: none;" onclick='showinfoButtonclick(infowindow[6],hospMarker[6],event)'>三軍總醫院附設民眾診療服務處</button>
           <button type="button" id="Button7" style="display: none;" onclick='showinfoButtonclick(infowindow[7],hospMarker[7],event)'>秀傳紀念醫院</button>
           <button type="button" id="Button8" style="display: none;" onclick='showinfoButtonclick(infowindow[8],hospMarker[8],event)'>彰化基督教醫院</button>
           <button type="button" id="Button9" style="display: none;" onclick='showinfoButtonclick(infowindow[9],hospMarker[9],event)'>高雄榮民總醫院</button>
           <button type="button" id="Button10" style="display: none;" onclick='showinfoButtonclick(infowindow[10],hospMarker[10],event)'>高雄醫學大學附設中和紀念醫院</button>
           <button type="button" id="Button11" style="display: none;" onclick='showinfoButtonclick(infowindow[11],hospMarker[11],event)'>童綜合醫院</button>
           
         </div>
            
         <div class="css_td" style="text-align:right;vertical-align:bottom">
            <div id="test"></div>
            
         </div>

    </div>
        
</div>
<div id="warnings_panel" style="background-color: #000; color:#7FFF00;font-size: 1.2em; font-weight: 400;">
           </div>
       <asp:Literal ID="js" runat="server"></asp:Literal><br />
       <div id="map_canvas" style="width: 100%; height: 500px"> </div>
</asp:Content>

