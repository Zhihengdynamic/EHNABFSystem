import json
import csv
from datetime import datetime
import pytz
import urllib2
import xlsxwriter
import math, sys, getopt
import time

# Function definition is here
def writeARRIVAL( target, plist ):
   "This writeARRIVAL into xls"
   target.write("ARRIVAL\n")
   target.write("TBA, Number\n")
   target.write(str(len(plist)))
   target.write("\n")
   for i in range(0,len(plist)):
     tmp=', '.join(map(str,plist[i]))
     target.write(tmp)
     target.write('\n')
   return;

def writeSERVICE( target, nlist ):
   "This writeSERVICExls into xls"
   target.write("SERVICE\n")
   target.write("TBA, Number, Time\n")
   target.write(str(len(nlist)))
   target.write("\n")
   for i in range(0,len(nlist)):
     tmp=', '.join(map(str,nlist[i])) 
     target.write(tmp)
     target.write('\n')
   return;

def checktime( now, datalist ):
   newdata=[]
   srcIDX=0
   pastdata=[int(now),100000000,0,0,0,0]
   for index in range(78):
      if(srcIDX<len(datalist)):
         tmp=datalist[srcIDX]
         while(now-tmp[0]<0):
            srcIDX=srcIDX+1
            if(srcIDX==len(datalist)):
               break;
            tmp=datalist[srcIDX]
         if(now-tmp[0]>1200000):
            #newdata=newdata+[[int(now),100000000,0,0,0,0]]
            newdata=newdata+[pastdata[:]]
         else:
            if(srcIDX<len(datalist)):
               newdata=newdata+[tmp[:]]
               srcIDX=srcIDX+1
               pastdata=[int(now),100000000]+[tmp[i] for i in range(2,6)]
               
            else:
               #newdata=newdata+[[int(now),100000000,0,0,0,0]]
               newdata=newdata+[pastdata[:]]
      else:
         #newdata=newdata+[[int(now),100000000,0,0,0,0]]
         newdata=newdata+[pastdata[:]]
      
      now=now-1200000
   return newdata;

def main(argv):
   
   try:
      opts, args = getopt.getopt(argv,"ho:",["ofile="])
   except getopt.GetoptError:
      print 'createModelData.py -o <outputpath>'
      sys.exit(2)    
   for opt, arg in opts:
      if opt == '-h':
         print 'createModelData.py -o <outputpath>'
         sys.exit()
      elif opt in ("-o", "--ofile"):
         outputdirectory = arg
         
   hospName={}
   #RootDIR="E:/Project/SpringSim15/ERFWcfService/XML/"
   #XMLDIR="E:/Project/OpenSrcData/ERPService/XML/"
   RootDIR=outputdirectory
   local_tz = pytz.timezone('Asia/Taipei')
   hospitalER="H1,H2,H3,H4,H5,H6,H7,H8,H9,H10,H11,H12"
   hospitalNumber=[1,10,11,12,2,3,4,5,6,7,8,9]
   hour="26h"
   now=time.time()*1000
   Sql="select%20time,pending_doctor,pending_bed,pending_icu,pending_ward%20from%20"+hospitalER+"%20where%20time%3Enow()-"+hour
   MyUrl="http://59.126.164.5:8086/db/twer/series?u=guest&p=guest&q="+Sql
   data = json.load(urllib2.urlopen(MyUrl));

  
   HospitalIdx=0;
   for item in data:
     columns=item['columns']
     del columns[1]
     t1=checktime(now,item['points'])
     
     t1.reverse()
     begin=1
     prepoint=[0,0,0,0,0]
     for point in t1:
       del point[1]
       local_dt = local_tz.localize(datetime.fromtimestamp(point[0]/1000))
       tmp=local_dt.strftime('%Y-%m-%d %H:%M:%S')

     begin=1
     
     avg=[0,0,0,0,0]
     p_idx=[0,0,0,0]
     n_idx=[0,0,0,0]
     
     p_list=[[],[],[],[]]
     n_list=[[],[],[],[]]
     p_tmp=[0,0,0,0];
     n_tmp=[0,0,0,0];


     ptmp=[20,1] 
     ntmp=[20,1,20]

     totalArriveTime=[0,0,0,0]
   
     for index in range(78):
       #p1=t1[index]
       #p2=t1[index+72]
       #p3=t1[index+144]
       #avg[0]=20*index
       
       #for i in range(1,5):
       #  avg[i]=(int(p1[i])+int(p2[i])+int(p3[i]))/3.0;

       p1=t1[index]
       avg[0]=20*index
       
       for i in range(1,5):
         avg[i]=int(p1[i]);
         

       if(index==0):
         for i in range(len(avg)):
           prepoint[i]=avg[i];
       else:
         diff=[0,0,0,0,0]
         for i in range(len(avg)):
           diff[i]=avg[i]-prepoint[i]
           prepoint[i]=avg[i]
           #print i
           if(i>0):
              p_tmp[i-1]=[" "," "] 
              n_tmp[i-1]=[" "," "," "] 
              if(diff[i]>0):
                diffINT=math.ceil(diff[i])
                difidx=(index-p_idx[i-1])*20; 
                p_tmp[i-1]=[difidx,diffINT]
                p_idx[i-1]=index
                p_list[i-1]=p_list[i-1]+[p_tmp[i-1]]
                totalArriveTime[i-1]=totalArriveTime[i-1]+difidx;
               
              if(diff[i]<0):
                difidx=(index-n_idx[i-1])*20;
                diffINT=math.ceil(-diff[i]) 
                n_tmp[i-1]=[difidx,diffINT,float(difidx)/diffINT] 
                n_idx[i-1]=index
                n_list[i-1]=n_list[i-1]+[n_tmp[i-1]]
        
     
     classname=["B", "D", "I", "W"]
     #classname=["D", "B", "I", "W"]  #B I changed 
  
     for i in range(0,4):
       if(i!=1):
         filename=RootDIR+'/ssH'+classname[i]+' '+str(hospitalNumber[HospitalIdx])+'.csv'
         target = open(filename, 'w')
         target.truncate()
         while(totalArriveTime[i]<1560): #2160
            p_list[i]=p_list[i]+[ptmp]
            n_list[i]=n_list[i]+[ntmp]
            totalArriveTime[i]=totalArriveTime[i]+20
         
         if(len(p_list[i])>len(n_list[i])):
            target.write(str(len(p_list[i])+3))
         else:
            target.write(str(len(n_list[i])+3))   
         target.write("\n")
         target.write('hospital_name\n')
         writeARRIVAL( target, p_list[i]);
         writeSERVICE( target, n_list[i]);
         target.close()
     HospitalIdx=HospitalIdx+1   
if __name__ == "__main__":
   main(sys.argv[1:])
