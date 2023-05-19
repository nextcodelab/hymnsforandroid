package com.lemuelinchrist.android.hymns.utils.Networks;


import android.content.Context;
import android.content.SharedPreferences;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;

import androidx.preference.PreferenceManager;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;
import com.lemuelinchrist.android.hymns.content.YoutubeButton;
import com.lemuelinchrist.android.hymns.entities.Hymn;

import java.util.regex.Matcher;
import java.util.regex.Pattern;
//import com.fasterxml.jackson.core.JsonParser;

public class NetworkCache {
    public static boolean refreshTunes = false;
    public static SharedPreferences Preferences;
    public static boolean hasInternet = false;
    public static HymnYT[] hymnTunes = null;
    public static String HYMN_JSON_FILE = "hymn_tunes.json";

    public static void LoadHymnTunes(Context context) {
        if (Preferences == null) {
            Preferences = PreferenceManager.getDefaultSharedPreferences(context);
        }
        String json = Preferences.getString(HYMN_JSON_FILE, null);
        if (json == null) {
            if (isNetworkAvailable(context)) {
                JsonFetch jsonFetch = new JsonFetch();
                jsonFetch.execute();
            }
        } else {
            if (refreshTunes) {
                if (isNetworkAvailable(context)) {
                    JsonFetch jsonFetch = new JsonFetch();
                    jsonFetch.execute();
                } else {
                    jsonToList(json);
                }
            } else {
                jsonToList(json);
            }
        }


    }

    public static HymnYT GetHymnTune(Hymn hymn) {
        String hymnId = hymn.getHymnId();
        String url = Preferences.getString(hymnId, null);
        if (url != null) {
            HymnYT yt = new HymnYT();
            yt.unique_id = hymnId;
            yt.youtube_link = url;
            return yt;
        }
        if (hymnTunes != null) {
            for (HymnYT hm : hymnTunes) {
                if (hm.unique_id.equals(hymnId)) {
                    SharedPreferences.Editor editor = Preferences.edit();
                    editor.putString(hymnId, hm.youtube_link);
                    editor.apply();
                    return hm;
                }
            }
        }
        return null;
    }


    public static String extractYTId(String ytUrl) {
        String pattern = "(?<=youtu.be/|watch\\?v=|/videos/|embed\\/)[^#\\&\\?]*";
        Pattern compiledPattern = Pattern.compile(pattern);
        Matcher matcher = compiledPattern.matcher(ytUrl);
        if (matcher.find()) {
            return matcher.group();
        } else {
            return null;
        }
    }

    public static void jsonToList(String json) {
        String result = json;
        final ObjectMapper objectMapper = new ObjectMapper();
        if (result != null && result != "") {
            try {
                NetworkCache.hymnTunes = objectMapper.readValue(result, HymnYT[].class);
            } catch (JsonProcessingException e) {
                throw new RuntimeException(e);
            }

        }
    }

    public void SerializeTunes() {
        if (hymnTunes == null)
            return;
        if (hymnTunes.length == 0)
            return;
        ObjectMapper mapper = new ObjectMapper();
        mapper.enable(SerializationFeature.INDENT_OUTPUT);
        try {
            String json = mapper.writeValueAsString(hymnTunes);
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
    }

    public static boolean isNetworkAvailable(Context context) {
        ConnectivityManager connectivityManager
                = (ConnectivityManager) context.getSystemService(Context.CONNECTIVITY_SERVICE);
        NetworkInfo activeNetworkInfo = connectivityManager != null ? connectivityManager.getActiveNetworkInfo() : null;
        boolean hasNetwork = activeNetworkInfo != null && activeNetworkInfo.isConnected();
        hasInternet = hasNetwork;
        return hasInternet;
    }
}



