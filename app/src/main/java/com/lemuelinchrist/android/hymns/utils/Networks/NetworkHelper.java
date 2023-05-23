package com.lemuelinchrist.android.hymns.utils.Networks;

import static android.content.ContentValues.TAG;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Debug;
import android.util.Log;
import android.widget.Toast;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.lemuelinchrist.android.hymns.utils.Networks.Servers.SheetItemPusher;

import java.io.Console;
import java.io.IOException;
import java.io.InputStream;
import java.net.MalformedURLException;
import java.net.URL;
import java.net.URLConnection;
import java.util.Scanner;
import java.util.logging.Logger;

public class NetworkHelper {
    public static String getResponse(String urlStr)  {
        Exception exception = null;
        String urlString = urlStr;
        URL url = null;
        URLConnection urlConnection = null;
        try {
            url = new URL(urlString);
            urlConnection = url.openConnection();
            urlConnection.setConnectTimeout(1000);

            InputStream inputStream = null;
            inputStream = urlConnection.getInputStream();
            Scanner s = new Scanner(inputStream).useDelimiter("\\A");
            String result = s.hasNext() ? s.next() : "";
            return result;
        } catch (MalformedURLException e) {
            throw new RuntimeException(e);
        } catch (IOException e) {
            throw new RuntimeException(e);
        }


    }
    public static HymnYT[] jsonToList(String json) {
        String result = json;
        final ObjectMapper objectMapper = new ObjectMapper();
        if (result != null && result != "") {
            try {
               return objectMapper.readValue(result, HymnYT[].class);
            } catch (JsonProcessingException e) {
                throw new RuntimeException(e);
            }

        }
        return null;
    }
    public static void pushAllJson(Context context){
        for (HymnYT hm : NetworkCache.hymnTunes) {
            String id = hm.unique_id;
            String link = NetworkCache.extractYTId(hm.youtube_link);
            SheetItemPusher sp = new SheetItemPusher(context);
            sp.pushItem(id, link);
        }
        Toast.makeText(context, "Sucess All Push", Toast.LENGTH_LONG).show();
    }
}
