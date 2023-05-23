package com.lemuelinchrist.android.hymns.utils.Networks.Servers;

import android.content.Context;
import android.widget.Toast;

import com.android.volley.DefaultRetryPolicy;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.RetryPolicy;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.lemuelinchrist.android.hymns.utils.Networks.HymnYT;
import com.lemuelinchrist.android.hymns.utils.Networks.NetworkHelper;

import java.util.HashMap;
import java.util.Map;

public class SheetItemsServer {
    Context context;

    public SheetItemsServer(Context _context) {
        context = _context;

    }

    public void getItems() {
        String json =  NetworkHelper.getResponse(ConstantHelper.SCRIPT_URL);;
        HymnYT[] list = NetworkHelper.jsonToList(json);

    }


}

