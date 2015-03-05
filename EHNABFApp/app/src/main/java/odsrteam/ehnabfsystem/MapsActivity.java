package odsrteam.ehnabfsystem;

import android.content.Context;
import android.content.Intent;
import android.graphics.Color;
import android.location.Location;
//import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.support.v4.app.FragmentActivity;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.Switch;
import android.widget.TextView;
import android.widget.Toast;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.api.GoogleApiClient;
import com.google.android.gms.common.api.GoogleApiClient.ConnectionCallbacks;
import com.google.android.gms.common.api.GoogleApiClient.OnConnectionFailedListener;
import com.google.android.gms.location.LocationListener;
import com.google.android.gms.location.LocationRequest;
import com.google.android.gms.location.LocationServices;
import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.SupportMapFragment;
import com.google.android.gms.maps.model.BitmapDescriptorFactory;
import com.google.android.gms.maps.model.CameraPosition;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.LatLngBounds;
import com.google.android.gms.maps.model.Marker;
import com.google.android.gms.maps.model.MarkerOptions;

import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;


public class MapsActivity extends FragmentActivity implements
        ConnectionCallbacks, OnConnectionFailedListener, LocationListener, GoogleMap.OnInfoWindowClickListener {

    protected static final String TAG = "mainsystem";


    /**
     * The desired interval for location updates. Inexact. Updates may be more or less frequent.
     */
    public static final long UPDATE_INTERVAL_IN_MILLISECONDS = 300000;

    /**
     * The fastest rate for active location updates. Exact. Updates will never be more frequent
     * than this value.
     */
    public static final long FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS =
            UPDATE_INTERVAL_IN_MILLISECONDS / 2;

    // Keys for storing activity state in the Bundle.
    protected final static String REQUESTING_LOCATION_UPDATES_KEY = "requesting-location-updates-key";
    protected final static String LOCATION_KEY = "location-key";
    protected final static String LAST_UPDATED_TIME_STRING_KEY = "last-updated-time-string-key";
    public static final int ZOOM=12;




    private GoogleMap mMap=null; // Might be null if Google Play services APK is not available.

    /**
     * Provides the entry point to Google Play services.
     */
    protected GoogleApiClient mGoogleApiClient=null;

    /**
     * Stores parameters for requests to the FusedLocationProviderApi.
     */
    protected LocationRequest mLocationRequest=null;

    /**
     * Represents a geographical location.
     */
    protected Location mCurrentLocation=null;


    /**
     * Tracks the status of the location updates request. Value changes when the user presses the
     * Start Updates and Stop Updates buttons.
     */
    protected Boolean mRequestingLocationUpdates;

    /**
     * Time when the location was updated represented as a String.
     */
    protected String mLastUpdateTime;

    /**
     * Represents a geographical location.
     */
    protected Location mLastLocation=null;


    protected String Resultstring;
    InfluxDBShop Forcast;

    //Marker WinMark;
    protected Long[] minDura=null;
    protected int[] minIdx=null;
    protected Marker[] hMarker=null;
    protected Long[] MPduration=null;

    // UI Widgets.
    protected Button mStartUpdatesButton;
    protected Button mStopUpdatesButton;
    protected TextView mLatitudeText=null;
    protected TextView mLongitudeText=null;
    protected TextView mResultText=null;
    protected Button[] mWinButton=null;

    protected boolean locationEnabled;


    //double Winlat;
    //double Winlon;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_maps);
        // Locate the UI widgets.
        mStartUpdatesButton = (Button) findViewById(R.id.start_updates_button);
        //mStopUpdatesButton = (Button) findViewById(R.id.stop_updates_button);
        mLatitudeText = (TextView) findViewById((R.id.latitude_text));
        mLongitudeText = (TextView) findViewById((R.id.longitude_text));

        mResultText=(TextView) findViewById((R.id.testTXT));
        mWinButton=new Button[3];
        mWinButton[0]=(Button) findViewById((R.id.buttonWin1));
        mWinButton[1]=(Button) findViewById((R.id.buttonWin2));
        mWinButton[2]=(Button) findViewById((R.id.buttonWin3));


        //minDura =new Long[3];
        //minIdx=new int[3];
        //hMarker=new Marker[12];
        // Kick off the process of building a GoogleApiClient and requesting the LocationServices
        // API.
        //Log.d(TAG, " １");
        //Log.d(TAG, "11");
        buildGoogleApiClient();
        //Log.d(TAG, "12");
        //Log.d(TAG, " ２");
        setUpMapIfNeeded();
        //Log.d(TAG, "13");
        //Log.d(TAG, " ３");
        Forcast =new InfluxDBShop();
        mRequestingLocationUpdates = false;
        mLastUpdateTime = "";
        minDura=new Long[3];
        minIdx=new int[3];
        hMarker=new Marker[12];
        MPduration=new Long[12];

        // Update values using data stored in the Bundle.
        //Log.d(TAG, " ４");
        //Log.d(TAG, "14");
        updateValuesFromBundle(savedInstanceState);


        //initInfluxDBConnect();

        //Forcast.getForcastResults(handler,"H1");
        //Log.i(TAG, "Get:"+Resultstring);
        mStartUpdatesButton.setEnabled(true);

        //mStopUpdatesButton.setEnabled(false);

    }

    Handler handler = new Handler(){
        @Override
        public void handleMessage(Message msg) {
            super.handleMessage(msg);
            Bundle data = msg.getData();
            Resultstring = data.getString("total");
            //Resultstring=val;

            SimpleDateFormat sdf = new SimpleDateFormat("HH:mm"); // the format of your date


            int mID= data.getInt("mID");

            String HPtitle=data.getString("name");
            Long HPduration=data.getLong("duration"); //seconds
            String HPdistance=data.getString("distance");


            String ArriveTime= sdf.format(new Date(System.currentTimeMillis()+HPduration*1000));


            String HPsnippet="";
            HPsnippet+="\n預測時間  普通病床  加護病床  急診推床";

            double[] tmp=data.getDoubleArray("20.0");
            String formattedDate = sdf.format(new Date((long)tmp[3]));
            HPsnippet+=String.format("\n%10s  %10.2f  %10.2f  %10.2f", formattedDate, tmp[0], tmp[1], tmp[2]);

            tmp=data.getDoubleArray("40.0");
            formattedDate = sdf.format(new Date((long)tmp[3]));
            HPsnippet+=String.format("\n%10s  %10.2f  %10.2f  %10.2f", formattedDate, tmp[0], tmp[1], tmp[2]);

            tmp=data.getDoubleArray("60.0");
            formattedDate = sdf.format(new Date((long)tmp[3]));
            HPsnippet+=String.format("\n%10s  %10.2f  %10.2f  %10.2f", formattedDate, tmp[0], tmp[1], tmp[2]);

            //long arriaveTime =System.currentTimeMillis()+;
            MPduration[mID]=HPduration*1000;




            //Marker tmpMark=mMap.addMarker(new MarkerOptions()
            hMarker[mID]=mMap.addMarker(new MarkerOptions()
                    .position(new LatLng(data.getDouble("latitude"), data.getDouble("longitude")))
                    .title(HPtitle + " (" + HPdistance + ")")
                    .snippet(HPsnippet));

            //Log.i(TAG, "ｓｍａｌｌ:" + minDura+" "+HPduration);

            if(HPduration<minDura[0])  {
                minDura[2]=minDura[1];
                minDura[1]=minDura[0];
                minDura[0]=HPduration;
                minIdx[2]=minIdx[1];
                minIdx[1]=minIdx[0];
                minIdx[0]=mID;

                //showMarker(mID);
                hMarker[mID].showInfoWindow();
                if(mLastLocation!=null)
                    moveBound(hMarker[mID].getPosition().longitude, hMarker[mID].getPosition().latitude,
                        mLastLocation.getLongitude(), mLastLocation.getLatitude());

                //String name=hMarker[minIdx[0]].getTitle().substring(0,hMarker[minIdx[0]].getTitle().indexOf('('));

                mWinButton[0].setText(HPtitle);
                mResultText.setText(HPtitle+"  預估行駛時間: "+
                        sdf.format(new Date(HPduration*1000))+
                        "  預估到院時間: "+ArriveTime );

            }else if(HPduration<minDura[1]) {
                minDura[2] = minDura[1];
                minDura[1] = HPduration;
                minIdx[2] = minIdx[1];
                minIdx[1] = mID;
                mWinButton[1].setText(HPtitle);
            }else if(HPduration<minDura[2]){
                minDura[2] = HPduration;
                minIdx[2] = mID;
                mWinButton[2].setText(HPtitle);
            }





        }
    };




    /**
     * Updates fields based on data stored in the bundle.
     *
     * @param savedInstanceState The activity state saved in the Bundle.
     */
    private void updateValuesFromBundle(Bundle savedInstanceState) {
        Log.i(TAG, "Updating values from bundle");
        if (savedInstanceState != null) {
            // Update the value of mRequestingLocationUpdates from the Bundle, and make sure that
            // the Start Updates and Stop Updates buttons are correctly enabled or disabled.
            if (savedInstanceState.keySet().contains(REQUESTING_LOCATION_UPDATES_KEY)) {
                mRequestingLocationUpdates = savedInstanceState.getBoolean(
                        REQUESTING_LOCATION_UPDATES_KEY);
                setButtonsText();
            }

            // Update the value of mCurrentLocation from the Bundle and update the UI to show the
            // correct latitude and longitude.
            if (savedInstanceState.keySet().contains(LOCATION_KEY)) {
                // Since LOCATION_KEY was found in the Bundle, we can be sure that mCurrentLocation
                // is not null.
                mCurrentLocation = savedInstanceState.getParcelable(LOCATION_KEY);
            }

            // Update the value of mLastUpdateTime from the Bundle and update the UI.
            if (savedInstanceState.keySet().contains(LAST_UPDATED_TIME_STRING_KEY)) {
                mLastUpdateTime = savedInstanceState.getString(LAST_UPDATED_TIME_STRING_KEY);
            }

            updateUI();
        }
    }



    /**
     * Sets up the map if it is possible to do so (i.e., the Google Play services APK is correctly
     * installed) and the map has not already been instantiated.. This will ensure that we only ever
     * call {@link #setUpMap()} once when {@link #mMap} is not null.
     * <p/>
     * If it isn't installed {@link SupportMapFragment} (and
     * {@link com.google.android.gms.maps.MapView MapView}) will show a prompt for the user to
     * install/update the Google Play services APK on their device.
     * <p/>
     * A user can return to this FragmentActivity after following the prompt and correctly
     * installing/updating/enabling the Google Play services. Since the FragmentActivity may not
     * have been completely destroyed during this process (it is likely that it would only be
     * stopped or paused), {@link #onCreate(Bundle)} may not be called again so we should call this
     * method in {@link #onResume()} to guarantee that it will be called.
     */
    private void setUpMapIfNeeded() {
        // Do a null check to confirm that we have not already instantiated the map.
        if (mMap == null) {
            // Try to obtain the map from the SupportMapFragment.
            mMap = ((SupportMapFragment) getSupportFragmentManager().findFragmentById(R.id.map))
                    .getMap();
            mMap.setInfoWindowAdapter(new PopupAdapter(getLayoutInflater()));
            mMap.setOnInfoWindowClickListener(this);
            setupMarkerListener();
            // Check if we were successful in obtaining the map.
            if (mMap != null) {
                setUpMap();
            }
        }
    }



    /**
     * This is where we can add markers or lines, add listeners or move the camera. In this case, we
     * just add a marker near Africa.
     * <p/>
     * This should only be called once and when we are sure that {@link #mMap} is not null.
     */
    private void setUpMap() {

        if (mLastLocation != null){

            moveCamera(mLastLocation, ZOOM);
            LatLng tmp=new LatLng(mLastLocation.getLatitude(),mLastLocation.getLongitude());
            Marker tmpM=mMap.addMarker(new MarkerOptions().position(tmp).title("目前位置"));
            tmpM.showInfoWindow();

        }else{
            //mMap.addMarker(new MarkerOptions().position(new LatLng(24.183744, 120.60371)).title("台中榮民總醫院"));
            mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(new LatLng(24.183744, 120.60371), 7));
        }
    }



    private void moveCamera(Location mTargetLocation, float zoom){
        LatLng tmp=new LatLng(mTargetLocation.getLatitude(),mTargetLocation.getLongitude());
        //mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(tmp, zoom));
        CameraPosition cameraPosition = new CameraPosition.Builder()
                .target(tmp)      // Sets the center of the map to Mountain View
                .zoom(zoom)                   // Sets the zoom
                //.bearing(90)                // Sets the orientation of the camera to east
                .tilt(40)                   // Sets the tilt of the camera to 30 degrees
                .build();                   // Creates a CameraPosition from the builder
        mMap.animateCamera(CameraUpdateFactory.newCameraPosition(cameraPosition));

    }


    /**
     * Builds a GoogleApiClient. Uses the {@code #addApi} method to request the
     * LocationServices API.
     */
    protected synchronized void buildGoogleApiClient() {
        Log.i(TAG, "Building GoogleApiClient");
        mGoogleApiClient = new GoogleApiClient.Builder(this)
                .addConnectionCallbacks(this)
                .addOnConnectionFailedListener(this)
                .addApi(LocationServices.API)
                .build();
        createLocationRequest();

    }

    /**
     * Sets up the location request. Android has two location request settings:
     * {@code ACCESS_COARSE_LOCATION} and {@code ACCESS_FINE_LOCATION}. These settings control
     * the accuracy of the current location. This sample uses ACCESS_FINE_LOCATION, as defined in
     * the AndroidManifest.xml.
     * <p/>
     * When the ACCESS_FINE_LOCATION setting is specified, combined with a fast update
     * interval (5 seconds), the Fused Location Provider API returns location updates that are
     * accurate to within a few feet.
     * <p/>
     * These settings are appropriate for mapping applications that show real-time location
     * updates.
     */
    protected void createLocationRequest() {
        mLocationRequest = new LocationRequest();

        // Sets the desired interval for active location updates. This interval is
        // inexact. You may not receive updates at all if no location sources are available, or
        // you may receive them slower than requested. You may also receive updates faster than
        // requested if other applications are requesting location at a faster interval.
        mLocationRequest.setInterval(UPDATE_INTERVAL_IN_MILLISECONDS);

        // Sets the fastest rate for active location updates. This interval is exact, and your
        // application will never receive updates faster than this value.
        mLocationRequest.setFastestInterval(FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS);

        mLocationRequest.setPriority(LocationRequest.PRIORITY_HIGH_ACCURACY);
    }

    /**
     * Handles the Start Updates button and requests start of location updates. Does nothing if
     * updates have already been requested.
     */
    public void startUpdatesButtonHandler(View view) {
        if (!mRequestingLocationUpdates) {
            mRequestingLocationUpdates = true;
            startLocationUpdates();
        }else{
            mRequestingLocationUpdates = false;
            stopLocationUpdates();
        }
        setButtonsText();
    }

    /**
     * Handles the Stop Updates button, and requests removal of location updates. Does nothing if
     * updates were not previously requested.
     */
    public void stopUpdatesButtonHandler(View view) {
        if (mRequestingLocationUpdates) {
            mRequestingLocationUpdates = false;
            setButtonsEnabledState();
            stopLocationUpdates();
        }
    }

    /**
     * Requests location updates from the FusedLocationApi.
     */
    protected void startLocationUpdates() {
        // The final argument to {@code requestLocationUpdates()} is a LocationListener
        // (http://developer.android.com/reference/com/google/android/gms/location/LocationListener.html).
        LocationServices.FusedLocationApi.requestLocationUpdates(
                mGoogleApiClient, mLocationRequest, this);
    }

    /**
     * Ensures that only one button is enabled at any time. The Start Updates button is enabled
     * if the user is not requesting location updates. The Stop Updates button is enabled if the
     * user is requesting location updates.
     */
    private void setButtonsEnabledState() {
        if (mRequestingLocationUpdates) {
            mStartUpdatesButton.setEnabled(false);
            mStopUpdatesButton.setEnabled(true);
        } else {
            mStartUpdatesButton.setEnabled(true);
            mStopUpdatesButton.setEnabled(false);
        }
    }

    private void setButtonsText() {
        if (mRequestingLocationUpdates) {
            //mStartUpdatesButton.setEnabled(false);
            mStartUpdatesButton.setText("停止更新");
            //mStopUpdatesButton.setEnabled(true);
        } else {
            //mStartUpdatesButton.setEnabled(true);
            mStartUpdatesButton.setText("自動更新");
            //mStopUpdatesButton.setEnabled(false);
        }
    }

    /**
     * Updates the latitude, the longitude, and the last location time in the UI.
     */
    private void updateUI() {
        mLatitudeText.setText(String.valueOf(mCurrentLocation.getLatitude()));
        mLongitudeText.setText(String.valueOf(mCurrentLocation.getLongitude()));
        moveCamera(mCurrentLocation,ZOOM);

        mMap.clear();

        //LatLng tmp=new LatLng(mCurrentLocation.getLatitude(),mCurrentLocation.getLongitude());

        for(int i=0;i<3;i++)
            minDura[i]=100000000L;

        mMap.addMarker(new MarkerOptions()
                  .position(new LatLng(mCurrentLocation.getLatitude(), mCurrentLocation.getLongitude())).title("目前位置")
                .icon(BitmapDescriptorFactory.fromResource(R.drawable.ambunance1)));


        int [] candlist=selectCandicateHosptial(mCurrentLocation);

        for(int i=0;i<10;i++) {
            String ID="H"+candlist[i];
            Forcast.getForcastResults(mCurrentLocation, handler, ID);
            //Log.d(TAG, " "+candlist[i]);
        }
        mWinButton[0].setTextColor(Color.rgb(0, 150, 150));


    }

    protected int[] selectCandicateHosptial(Location CurrentLocation) {
        Map<String, HPInfo> RealHPname =Forcast.RealHPname;
        HashMap<String, Double> hospDist= new HashMap<String, Double>();

        for (int i = 0; i < RealHPname.size(); i++) {
            String hName="H"+(i+1);
            LatLng branch = new LatLng(RealHPname.get(hName).getlatitude(),RealHPname.get(hName).getlongitude());
            double distance = distHaversine(CurrentLocation.getLatitude(),CurrentLocation.getLongitude() , branch);
            //var distance = CurrentPosition.distanceFrom(branch);
            hospDist.put(""+(i+1),distance);

        }
        //依距離進行排序
        List<Map.Entry<String, Double>> list_Data =
                new ArrayList<Map.Entry<String, Double>>(hospDist.entrySet());
        Collections.sort(list_Data, new Comparator<Map.Entry<String, Double>>() {
            public int compare(Map.Entry<String, Double> entry1,
                               Map.Entry<String, Double> entry2) {
                return (int)(entry1.getValue() - entry2.getValue());
            }
        });

        //var teststr = "";
        int[] candlist =new int[RealHPname.size()];
       int i=0;
        for (Map.Entry<String, Double> entry:list_Data) {
            candlist[i]=Integer.parseInt(entry.getKey());
            i++;
        }
        return candlist;
    }

    protected double distHaversine(double lat, double lng, LatLng tMarker) {

        int R = 6371; // earth's mean radius in km
        double dLat = rad(tMarker.latitude - lat);
        double dLong = rad(tMarker.longitude - lng);

        double a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
                Math.cos(rad(lat)) * Math.cos(rad(tMarker.latitude))
                        * Math.sin(dLong / 2) * Math.sin(dLong / 2);
        double c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
        double d = R * c;

        return d;
    }

    protected double rad(double x)
    {
        return x * Math.PI / 180;
    }



    /**
     * Removes location updates from the FusedLocationApi.
     */
    protected void stopLocationUpdates() {
        // It is a good practice to remove location requests when the activity is in a paused or
        // stopped state. Doing so helps battery performance and is especially
        // recommended in applications that request frequent location updates.

        // The final argument to {@code requestLocationUpdates()} is a LocationListener
        // (http://developer.android.com/reference/com/google/android/gms/location/LocationListener.html).
        LocationServices.FusedLocationApi.removeLocationUpdates(mGoogleApiClient, this);
    }

   public void win1buttonClick(View view){showMarker(minIdx[0]);}
    public void win2buttonClick(View view){
        showMarker(minIdx[1]);
    }
    public void win3buttonClick(View view){
        showMarker(minIdx[2]);
    }

    protected void showMarker(int mIndex){
        Marker marker=hMarker[mIndex];
        SimpleDateFormat sdf = new SimpleDateFormat("HH:mm");
        String name=marker.getTitle().substring(0,marker.getTitle().indexOf('(')-1);
        mResultText.setText(name+"  預估行駛: "+
                    (MPduration[mIndex]/60000)+"分"+(MPduration[mIndex]%60000)/1000+"秒"+
                    "  預估到院時間: "+sdf.format(new Date(System.currentTimeMillis()+MPduration[mIndex])) );

        for(int i=0;i<3;i++){
            if(minIdx[i]==mIndex){
                mWinButton[i].setTextColor(Color.rgb(0, 150, 150));
            }else{
                mWinButton[i].setTextColor(Color.BLACK);
            }
        }

        if( marker.isInfoWindowShown() ) {
            marker.hideInfoWindow();
        } else {
            marker.showInfoWindow();
        }

        moveBound(marker);


    }



    @Override
    protected void onResume() {
        super.onResume();
        //buildGoogleApiClient();

        LocationManager manager = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
        if (!manager.isProviderEnabled(LocationManager.GPS_PROVIDER) && !manager.isProviderEnabled(LocationManager.NETWORK_PROVIDER)) {
            locationEnabled = false;
            Toast.makeText(this, "請開啟定位服務", Toast.LENGTH_SHORT).show();
            mResultText.setText("請開啟定位服務");

        }
        else locationEnabled = true;

        Log.i(TAG, " "+mCurrentLocation);
        setUpMapIfNeeded();
        setButtonsText();


        if (mGoogleApiClient.isConnected() && mRequestingLocationUpdates) {
            startLocationUpdates();
           // updateUI();
        }else if(mGoogleApiClient.isConnected()==false){
            mGoogleApiClient.connect();
//            startLocationUpdates();
        }




    }

    @Override
    protected void onStart() {
        super.onStart();
        //Log.d(TAG, "1");
        mGoogleApiClient.connect();
        //Log.d(TAG, "2");
    }

    @Override
    protected void onStop() {
        super.onStop();
        if (mGoogleApiClient.isConnected()) {
            mGoogleApiClient.disconnect();
        }
    }
    @Override
    protected void onPause() {
        super.onPause();
        // Stop location updates to save battery, but don't disconnect the GoogleApiClient object.
        if (mGoogleApiClient.isConnected()) {
            stopLocationUpdates();
        }
    }

    /**
     * Callback that fires when the location changes.
     */
    @Override
    public void onLocationChanged(Location location) {
        mCurrentLocation = location;
        mLastUpdateTime = DateFormat.getTimeInstance().format(new Date());
        updateUI();
        //Toast.makeText(this, getResources().getString(R.string.location_updated_message),
        //        Toast.LENGTH_SHORT).show();
    }



    @Override
    public void onConnected(Bundle connectionHint) {
        Log.i(TAG, "Connected to GoogleApiClient");



        mLastLocation = LocationServices.FusedLocationApi.getLastLocation(
                mGoogleApiClient);



        if (mLastLocation != null) {

            mLatitudeText.setText(String.valueOf(mLastLocation.getLatitude()));
            mLongitudeText.setText(String.valueOf(mLastLocation.getLongitude()));
        }

        if (mCurrentLocation == null && locationEnabled) {
            mCurrentLocation = LocationServices.FusedLocationApi.getLastLocation(mGoogleApiClient);
            mLastUpdateTime = DateFormat.getTimeInstance().format(new Date());
            updateUI();
        }

        // If the user presses the Start Updates button before GoogleApiClient connects, we set
        // mRequestingLocationUpdates to true (see startUpdatesButtonHandler()). Here, we check
        // the value of mRequestingLocationUpdates and if it is true, we start location updates.
        if (mRequestingLocationUpdates && locationEnabled) {
            startLocationUpdates();
        }
    }

    @Override
    public void onConnectionFailed(ConnectionResult result) {
        // Refer to the javadoc for ConnectionResult to see what error codes might be returned in
        // onConnectionFailed.
        Log.i("run", "Connection failed: ConnectionResult.getErrorCode() = " + result.getErrorCode());
    }


    @Override
    public void onConnectionSuspended(int cause) {
        // The connection to Google Play services was lost for some reason. We call connect() to
        // attempt to re-establish the connection.
        Log.i("run", "Connection suspended");
        mGoogleApiClient.connect();
    }

    /**
     * Stores activity data in the Bundle.
     */
    public void onSaveInstanceState(Bundle savedInstanceState) {
        savedInstanceState.putBoolean(REQUESTING_LOCATION_UPDATES_KEY, mRequestingLocationUpdates);
        savedInstanceState.putParcelable(LOCATION_KEY, mCurrentLocation);
        savedInstanceState.putString(LAST_UPDATED_TIME_STRING_KEY, mLastUpdateTime);
        super.onSaveInstanceState(savedInstanceState);
    }

    @Override
    public void onInfoWindowClick(Marker marker) {
        //Toast.makeText(this, marker.getTitle(), Toast.LENGTH_LONG).show();

        //Log.d("EHNAF", "Clickinfo!!.");


    }

    private void setupMarkerListener() {
        mMap.setOnMarkerClickListener( new GoogleMap.OnMarkerClickListener() {
            @Override
            public boolean onMarkerClick( Marker marker ) {
                //moveCamera();
                //Log.d("EHNAF", "Click1!!.");
                SimpleDateFormat sdf = new SimpleDateFormat("HH:mm");
                for(int i=0;i<hMarker.length;i++)
                {
                    if(marker.equals(hMarker[i])) {
                        //String ArriveTime= sdf.format(new Date(System.currentTimeMillis()+MPduration[i]));
                        /*String name=marker.getTitle().substring(0,marker.getTitle().indexOf('(')-1);
                        mResultText.setText(name+"  預估行駛: "+
                                (MPduration[i]/60000)+"分"+(MPduration[i]%60000)/1000+"秒"+
                                "  預估到院時間: "+sdf.format(new Date(System.currentTimeMillis()+MPduration[i])) );*/

                        showMarker(i);

                    }
                }

                /*if( marker.isInfoWindowShown() ) {
                    marker.hideInfoWindow();
                } else {
                    marker.showInfoWindow();
                }

                moveBound(marker);*/

                return true;
            }
        });
    }

    void moveBound(Marker m1) {

        if(mLastLocation != null)
         moveBound(m1.getPosition().longitude, m1.getPosition().latitude,
                mLastLocation.getLongitude(), mLastLocation.getLatitude());

    }

    void moveBound(double lo1, double la1, double lo2, double la2){
        double L,R,U,D;
        if(la1 < la2) {
            L = la1;
            R = la2;
        }else{
            R = la1;
            L = la2;
        }

        if(lo1 < lo2) {
            U =lo1;
            D = lo2;
        }else{
            D = lo1;
            U = lo2;
        }

        LatLngBounds Mybounds = new LatLngBounds(
                new LatLng(L, U),
                new LatLng(R, D));
        mMap.moveCamera(CameraUpdateFactory.newLatLngBounds(Mybounds, 150));
    }

}

