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

import java.util.HashMap;
import java.util.Map;

public class SheetItemPusher implements Response.Listener<String>, Response.ErrorListener {
    Context context;
    boolean processing = false;

    public SheetItemPusher(Context _context) {
        context = _context;

    }

    public void pushItem(final String hymnId, final String link) {
        if (processing)
            return;
        StringRequest stringRequest = new StringRequest(Request.Method.POST, ConstantHelper.SCRIPT_URL, this, this) {
            @Override
            protected Map<String, String> getParams() {
                Map<String, String> parmas = new HashMap<>();

                //here we pass params

                parmas.put("hymnid", hymnId);
                parmas.put("link", link);

                return parmas;
            }
        };

        int socketTimeOut = 10000;// u can change this .. here it is 50 seconds

        RetryPolicy retryPolicy = new DefaultRetryPolicy(socketTimeOut, 0, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT);
        stringRequest.setRetryPolicy(retryPolicy);

        RequestQueue queue = Volley.newRequestQueue(context);

        queue.add(stringRequest);
        processing = true;
    }

    @Override
    public void onResponse(String response) {
        processing = false;
        Toast.makeText(context, response, Toast.LENGTH_LONG).show();
    }

    @Override
    public void onErrorResponse(VolleyError error) {
        processing = false;
    }
}
