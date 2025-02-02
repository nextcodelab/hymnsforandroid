package com.lemuelinchrist.android.hymns.utils.Networks;

import static android.content.ContentValues.TAG;

import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.util.Log;
import android.view.View;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;

import androidx.cardview.widget.CardView;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.lemuelinchrist.android.hymns.entities.Hymn;
import com.lemuelinchrist.android.hymns.utils.Networks.Servers.ConstantHelper;


import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLConnection;
import java.nio.charset.StandardCharsets;
import java.util.Scanner;

public class JsonFetch extends AsyncTask<Hymn, Void, HymnYT> {
    public WebView webView;
    public View cardView;
    public boolean refresh = false;
    @Override
    protected HymnYT doInBackground(Hymn... params) {
        Hymn hymn = null;
        if (params != null && params.length > 0) {
            hymn = params[0];
        }

        if (refresh == false){
            if (NetworkCache.hymnTunes != null) {
                if (hymn != null) {
                    return NetworkCache.GetHymnTune(hymn);
                }
                return null;
            }
        }

        Exception exception = null;

        URL url = null;
        URLConnection urlConnection = null;
        try {

            url = new URL(ConstantHelper.SCRIPT_URL);
            urlConnection = url.openConnection();
            urlConnection.setConnectTimeout(1000);

            InputStream inputStream = urlConnection.getInputStream();
            if (inputStream == null) {
                return null;
            }
            Scanner s = new Scanner(inputStream).useDelimiter("\\A");
            String result = s.hasNext() ? s.next() : "";
            final ObjectMapper objectMapper = new ObjectMapper();
            if (result != null && result != "") {
                NetworkCache.hymnTunes = objectMapper.readValue(result, HymnYT[].class);
                SharedPreferences.Editor editor = NetworkCache.Preferences.edit();
                editor.putString(NetworkCache.HYMN_JSON_FILE, result);
                editor.apply();
                if (hymn != null) {
                    return NetworkCache.GetHymnTune(hymn);
                }
            }
            return null;
        } catch (IOException e) {
            Log.e(TAG, "IO Exception", e);
            exception = e;
            return null;
        } finally {


        }
    }

    @Override
    protected void onPostExecute(HymnYT response) {
        if (response != null) {
            if (cardView != null) {
                cardView.setVisibility(View.VISIBLE);
            }
            if (webView != null) {
                HymnYT tune = response;
                String youtubeId = NetworkCache.extractYTId(tune.youtube_link);
                webView.setWebViewClient(new WebViewClient() {
                    @Override
                    public boolean shouldOverrideUrlLoading(WebView view, String url) {
                        return false;
                    }
                });

                WebSettings webSettings = webView.getSettings();
                webSettings.setJavaScriptEnabled(true);
                webSettings.setLoadWithOverviewMode(true);
                webSettings.setUseWideViewPort(true);
                webView.loadUrl("https://www.youtube.com/embed/" + youtubeId);
            }
        }

    }
}
