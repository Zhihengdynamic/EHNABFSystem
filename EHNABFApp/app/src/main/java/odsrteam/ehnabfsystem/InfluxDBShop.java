package odsrteam.ehnabfsystem;

import android.location.Location;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.util.Log;

import com.google.gson.Gson;

import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.client.ClientProtocolException;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.impl.client.DefaultHttpClient;
import org.influxdb.InfluxDB;
import org.influxdb.InfluxDBFactory;
import org.influxdb.dto.Pong;
import org.influxdb.dto.Serie;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.IOException;
import java.io.InputStream;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.TimeUnit;

/**
 * Created by user on 2015/1/29.
 */
public class InfluxDBShop {

    protected static final String TAG = "InfluxDBShop";
    protected final static String HOST="http://grainflux.cloudapp.net:8086";
    protected final static String DBNAME="ERFdb";
    Map<String, HPInfo> RealHPname = null;

    //public String Resultstring=null;

    Handler handler=null;
    String hName=null;
    Location mCurrentLocation=null;
    long time=0;
    String dist=null;

    public InfluxDBShop(){
        RealHPname = new HashMap<String, HPInfo>();
        RealHPname.put("H1", new HPInfo("台北榮民總醫院",25.1201836,121.5201598));
        RealHPname.put("H2", new HPInfo("林口長庚醫院",25.0618495,121.3676923));
        RealHPname.put("H3", new HPInfo("台中榮民總醫院",24.183744, 120.60371));

    }

    public void getForcastResults(Location mCurrentLocation, Handler handlersrc, String hNamesrc) {
        //Gson info=new Gson();
        handler = handlersrc;
        hName = hNamesrc;
        this.mCurrentLocation = mCurrentLocation;

        //Thread T=new Thread(runnable);
        InfluxThread T = new InfluxThread();
        T.start();
        try {
            T.join();
        } catch (InterruptedException e) {
            e.printStackTrace();
            Log.i(TAG, "join:" + e.toString());
        }

    }

    private InfluxDB initInfluxDBConnect(){
        //Log.i(TAG, "InfluxDB Connect");
        InfluxDB influxDB = InfluxDBFactory.connect(HOST, "guest", "guest");
        boolean influxDBstarted = false;
        do {
            Pong response;
            try {
                response = influxDB.ping();
                if (response.getStatus().equalsIgnoreCase("ok")) {
                    influxDBstarted = true;
                }
            } catch (Exception e) {
                // NOOP intentional
            }
            try {
                Thread.sleep(100L);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        } while (!influxDBstarted);
        return influxDB;
    }

    class InfluxThread extends Thread{
        public void run(){

            Bundle data = new Bundle();
            getDistanceInfo(
                    mCurrentLocation.getLatitude(),
                    mCurrentLocation.getLongitude(),
                    RealHPname.get(hName).getlatitude(),
                    RealHPname.get(hName).getlongitude()) ;
            data.putLong("duration",time);
            data.putString("distance",dist);

            // TODO: http request.
            InfluxDB influxDB=initInfluxDBConnect();
            //List<Serie> result = influxDB.query(DBNAME, "select flownum, HpF_ICU from "+ hName+" where time < now()+2h group by flownum order asc", TimeUnit.MILLISECONDS);
            List<Serie> result = influxDB.query(DBNAME, "select * from "+ hName+" where time < now()+2h group by flownum order asc", TimeUnit.MILLISECONDS);
            //Log.i(TAG, "２");

            Serie tmpSerie=result.get(0);

            Message msg = new Message();


            String Ftime;
            //SimpleDateFormat sdf = new SimpleDateFormat("HH:mm:ss"); // the format of your date

            for(int i=0; i<3; i++){
                double[] value=new double[4];
                Ftime=(tmpSerie.getRows().get(i).get("Forecast_Time")).toString();

                value[0]=(Double)tmpSerie.getRows().get(i).get("HpF_WARN");
                value[1]=(Double)tmpSerie.getRows().get(i).get("HpF_ICU");
                value[2]=(Double)tmpSerie.getRows().get(i).get("HpF_BED");
                value[3]=(Double)tmpSerie.getRows().get(i).get("time");

                //Log.i(TAG, Ftime+" "+value[0]+" "+value[1]+" "+value[2]+" "+value[3]);

                data.putDoubleArray(Ftime, value);

            }

            data.putString("total",tmpSerie.toString());
            data.putString("name",RealHPname.get(hName).getname());
            data.putDouble("latitude", RealHPname.get(hName).getlatitude());
            data.putDouble("longitude", RealHPname.get(hName).getlongitude());




            //Log.i(TAG,data.toString());

            //double[] tmp=data.getDoubleArray("20.0");
            //long unixmilliSeconds = (long)tmp[3];
            //Date date = new Date(unixmilliSeconds); // *1000 is to convert seconds to milliseconds

            //sdf.setTimeZone(TimeZone.getTimeZone("GMT-4")); // give a timezone reference for formating (see comment at the bottom
            /*String formattedDate = sdf.format(new Date((long)tmp[3]));
            Log.i(TAG, String.format("\n%s\t%.2f\t%.2f\t%.2f", formattedDate, tmp[0], tmp[1], tmp[2]));

            tmp=data.getDoubleArray("40.0");
            formattedDate = sdf.format(new Date((long)tmp[3]));
            Log.i(TAG, String.format("\n%s\t%.2f\t%.2f\t%.2f", formattedDate, tmp[0], tmp[1], tmp[2]));

            tmp=data.getDoubleArray("60.0");
            formattedDate = sdf.format(new Date((long)tmp[3]));
            Log.i(TAG, String.format("\n%s\t%.2f\t%.2f\t%.2f", formattedDate, tmp[0], tmp[1], tmp[2]));
*/

            //Log.i(TAG, "４");
            msg.setData(data);
            //Log.i(TAG, "５");
            handler.sendMessage(msg);
        }
    }

    //examples
    //https://maps.googleapis.com/maps/api/distancematrix/json?origins=25.1201836,121.5201598&destinations=25.0618495,121.3676923
    //https://maps.googleapis.com/maps/api/directions/json?origin=25.1201836,121.5201598&destination=25.0618495,121.3676923
    private void getDistanceInfo(double lat1, double lng1, double lat2, double lng2) {
        StringBuilder stringBuilder = new StringBuilder();
        //Double dist = 0.0;
        //String dist=null;
        //String time=null;

        try {

            //destinationAddress = destinationAddress.replaceAll(" ","%20");
            String url = "http://maps.googleapis.com/maps/api/directions/json?origin=" + lat1 + "," + lng1 + "&destination=" + lat2 + "," + lng2 + "&mode=driving&sensor=false";

            HttpPost httppost = new HttpPost(url);

            HttpClient client = new DefaultHttpClient();
            HttpResponse response;
            stringBuilder = new StringBuilder();


            response = client.execute(httppost);
            HttpEntity entity = response.getEntity();
            InputStream stream = entity.getContent();
            int b;
            while ((b = stream.read()) != -1) {
                stringBuilder.append((char) b);
            }
        } catch (ClientProtocolException e) {
        } catch (IOException e) {
        }

        //JSONObject jsonObject = new JSONObject();
        try {

            JSONObject jsonObject = new JSONObject(stringBuilder.toString());

            JSONArray array = jsonObject.getJSONArray("routes");

            JSONObject routes = array.getJSONObject(0);

            JSONArray legs = routes.getJSONArray("legs");

            JSONObject steps = legs.getJSONObject(0);

            JSONObject distance = steps.getJSONObject("distance");

            JSONObject duration = steps.getJSONObject("duration");

            //Log.i("Distance", distance.toString());
            //dist = Double.parseDouble(distance.getString("text").replaceAll("[^\\.0123456789]","") );
            dist = distance.getString("text");
            time= duration.getLong("value");

        } catch (JSONException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
        }

    }
}

class HPInfo {
    public HPInfo(String name, double latitude, double longitude) {
        this.name=name;
        this.latitude=latitude;
        this.longitude=longitude;
    }

    public String getname() { return this.name; }
    public double getlatitude() { return this.latitude; }
    public double getlongitude() { return this.longitude; }

    private String name;
    private double latitude;
    private double longitude;
}


