package com.lemuelinchrist.android.hymns;

import static com.lemuelinchrist.android.hymns.content.ContentArea.HISTORY_LOGBOOK_FILE;

import android.content.ActivityNotFoundException;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.ApplicationInfo;
import android.content.res.ColorStateList;
import android.content.res.Configuration;
import android.graphics.Color;
import android.net.Uri;
import android.os.Bundle;
import android.os.StrictMode;
import android.util.Log;
import android.view.*;
import android.widget.AdapterView;
import android.widget.EditText;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.Toast;

import androidx.appcompat.app.ActionBar;
import androidx.appcompat.app.ActionBarDrawerToggle;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.core.app.ComponentActivity;
import androidx.core.content.FileProvider;
import androidx.core.view.GravityCompat;
import androidx.core.view.ViewCompat;
import androidx.drawerlayout.widget.DrawerLayout;
import androidx.preference.PreferenceManager;
import androidx.viewpager.widget.ViewPager;

import com.lemuelinchrist.android.hymns.content.OnLyricVisibleListener;
import com.lemuelinchrist.android.hymns.dao.HymnsDao;
import com.lemuelinchrist.android.hymns.entities.Hymn;
import com.lemuelinchrist.android.hymns.logbook.LogBook;
import com.lemuelinchrist.android.hymns.search.SearchActivity;
import com.lemuelinchrist.android.hymns.settings.SettingsActivity;
import com.lemuelinchrist.android.hymns.style.Theme;
import com.lemuelinchrist.android.hymns.utils.Networks.NetworkCache;
import com.lemuelinchrist.android.hymns.utils.Networks.NetworkHelper;
import com.lemuelinchrist.android.hymns.utils.Networks.Servers.SheetItemPusher;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;


/**
 * Created by lemuelcantos on 17/7/13.
 */
public class HymnsActivity extends AppCompatActivity implements OnLyricVisibleListener, HymnSwitcher,
        SharedPreferences.OnSharedPreferenceChangeListener {
    protected final int SEARCH_REQUEST = 1;
    protected HymnGroup selectedHymnGroup = HymnGroup.getDefaultHymnGroup();
    private ListView mDrawerList;
    private DrawerLayout mDrawerLayout;
    private ActionBarDrawerToggle mDrawerToggle;

    private ActionBar actionBar;
    public ViewPager viewPager;
    public String currentHymnId;
    private HymnBookCollection hymnBookCollection;
    private Theme theme = Theme.LIGHT;
    private SharedPreferences sharedPreferences;
    private boolean preferenceChanged = true;
    private HymnDrawer hymnDrawer;
    private static HymnSwitcher hymnSwitcher;
    public static HymnsActivity Instance;


    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Instance  = this;
        StrictMode.ThreadPolicy policy = new StrictMode.ThreadPolicy.Builder().permitAll().build();
        StrictMode.setThreadPolicy(policy);
        NetworkCache.LoadHymnTunes(this);
        Log.d(this.getClass().getName(), "start app");

        Log.d(this.getClass().getName(), "start Hymn App... Welcome to Hymns!");
        setContentView(R.layout.main_hymns_activity);

        // set default value of preferences
        PreferenceManager.setDefaultValues(this, R.xml.root_preferences, false);

        sharedPreferences = PreferenceManager.getDefaultSharedPreferences(this);
        sharedPreferences.registerOnSharedPreferenceChangeListener(this);

        // Instantiate a ViewPager and a PagerAdapter.
        this.viewPager = (ViewPager) findViewById(R.id.hymn_fragment_viewpager);

        hymnBookCollection = new HymnBookCollection(this, this.viewPager, theme);

        Toolbar toolbar = findViewById(R.id.toolbar);
        toolbar.setTitleTextColor(Color.WHITE);
        setSupportActionBar(toolbar);
        actionBar = getSupportActionBar();
        actionBar.setDisplayShowTitleEnabled(true);
        actionBar.setDisplayHomeAsUpEnabled(true);

        mDrawerLayout = findViewById(R.id.drawer_layout);
        mDrawerList = findViewById(R.id.left_drawer);
        mDrawerLayout.setDrawerShadow(R.drawable.drawer_shadow, GravityCompat.START);
        mDrawerList.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> adapterView, View view, int position, long l) {
                selectDrawerItem(position);
            }
        });

        refreshHymnDrawer();
        mDrawerToggle = new ActionBarDrawerToggle(
                this,                  /* host Activity */
                mDrawerLayout,         /* DrawerLayout object */
                R.string.drawer_open,
                R.string.drawer_close
        ) {
            public void onDrawerClosed(View view) {
                super.onDrawerClosed(view);
            }

            public void onDrawerOpened(View drawerView) {
                super.onDrawerOpened(drawerView);
            }
        };
        mDrawerLayout.setDrawerListener(mDrawerToggle);
        setDisplayConfig();
        hymnSwitcher = this;
    }
    /**
     * On older and low-end devices, when searching for a hymn in the SearchActivity,
     * the app always resumes in the main activity. In this scenario, the hymnId becomes null.
     * To address this, select the hymn and log it first to ensure that the newly selected hymn is displayed correctly.
     */

    public void  setLog(String hymnId){
        HymnsDao hymnsDao = new HymnsDao(this);
        hymnsDao.open();
        Hymn hymn = hymnsDao.get(hymnId);
        hymnsDao.close();
        LogBook  historyLogBook = new LogBook(this, HISTORY_LOGBOOK_FILE);
        historyLogBook.log(hymn);
    }

    public void UpdateSelection() {
        this.viewPager.getCurrentItem();
    }

    private void refreshHymnDrawer() {
        hymnDrawer = new HymnDrawer(this,
                R.layout.drawer_hymngroup_list);
        mDrawerList.setAdapter(hymnDrawer);
    }

    // Warning! this method is very crucial. Without it you will not have a hamburger icon on your
    // action bar.
    @Override
    protected void onPostCreate(Bundle savedInstanceState) {
        super.onPostCreate(savedInstanceState);
        // Sync the toggle state after onRestoreInstanceState has occurred.
        mDrawerToggle.syncState();
    }

    private void selectDrawerItem(int position) {
        Log.i(HymnsActivity.class.getSimpleName(), "Drawer Item selected: " + position);

        selectedHymnGroup = hymnDrawer.getSelectedHymnGroup(position);
        hymnBookCollection.translateTo(selectedHymnGroup);
        mDrawerLayout.closeDrawer(mDrawerList);
    }


    @Override
    public boolean onCreateOptionsMenu(final Menu menu) {
        MenuInflater inflater = getMenuInflater();
        inflater.inflate(R.menu.main, menu);

        return true;
    }

    public boolean onOptionsItemSelected(MenuItem item) {

        boolean ret = true;
        Log.d(this.getClass().getName(), "Item selected: " + item.getItemId());

        // code for drawer:
        int itemId = item.getItemId();
        if (itemId == android.R.id.home) {
            toggleDrawer();
        } else if (itemId == R.id.action_index) {
            Intent intent = new Intent(getBaseContext(), SearchActivity.class);
            intent.putExtra("selectedHymnGroup", selectedHymnGroup);
            startActivityForResult(intent, SEARCH_REQUEST);
            ret = true;
        } else if (itemId == R.id.action_settings) {
            Intent settingsIntent = new Intent(getBaseContext(), SettingsActivity.class);
            startActivity(settingsIntent);
        } else if (itemId == R.id.action_submit_ty) {
            showSubmitDialog();
        } else if (itemId == R.id.hymnal_net) {
            try {

                String number = currentHymnId.replaceAll("[^0-9]", "");
                String letter = "h";
                if (currentHymnId.startsWith("E")) {
                    letter = "h";
                } else {
                    String letters = currentHymnId.replaceAll("[^a-zA-Z].*", "");
                    if (letters.length() > 1) {
                        letter = letters;
                    }
                }
                letter = letter.toLowerCase();
                String url = "https://www.hymnal.net/en/hymn/" + letter + "/" + number;
                Intent myIntent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
                startActivity(myIntent);
            } catch (ActivityNotFoundException e) {
                Toast.makeText(this, "No application can handle this request."
                        + " Please install a webbrowser", Toast.LENGTH_LONG).show();
                e.printStackTrace();
            }
        } else if (itemId == R.id.refesh_action_menu_ic) {
            NetworkCache.refreshTunes = true;
            NetworkCache.LoadHymnTunes(this);
        } else if (itemId == R.id.share_apk_link) {
            ShareAPKLink();
        } else {
            ret = false;
            Log.w(HymnsActivity.class.getSimpleName(), "Warning!! No Item was selected!!");
        }
        return ret;
    }

    void showSubmitDialog() {
        if (currentHymnId == null || currentHymnId == "") {
            return;
        }
        String saveLabel = "Save";
        if (NetworkCache.hasInternet) {
            saveLabel = "Submit";
        }
        String yt = NetworkCache.Preferences.getString(currentHymnId, "");

        final FrameLayout container = new FrameLayout(this);
        FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
        params.leftMargin = 35;
        params.rightMargin = 35;

        final EditText taskEditText = new EditText(this);
        taskEditText.setText(yt);
        taskEditText.setLayoutParams(params);
        container.addView(taskEditText);

        int color = Color.argb(40, 255, 100, 60);
        ColorStateList colorStateList = ColorStateList.valueOf(color);
        ViewCompat.setBackgroundTintList(taskEditText, colorStateList);
        AlertDialog dialog = new AlertDialog.Builder(this)
                .setTitle("YOUTUBE EMBED")
                .setMessage("Enter youtube link")
                .setView(container)
                .setPositiveButton(saveLabel, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        String task = String.valueOf(taskEditText.getText());

                        if (task.contains("https://youtu.be/") || task.contains("https://www.youtube.com/watch?v=")) {
                            String ytId = NetworkCache.extractYTId(task);
                            if (NetworkCache.hasInternet) {
                                SheetItemPusher sp = new SheetItemPusher(container.getContext());
                                sp.pushItem(currentHymnId, NetworkCache.extractYTId(task));
                                Toast.makeText(taskEditText.getContext(), "Submitted", Toast.LENGTH_SHORT).show();
                            }
                            SharedPreferences.Editor editor = NetworkCache.Preferences.edit();
                            editor.putString(currentHymnId, task);
                            editor.apply();
                            if (NetworkCache.hasInternet == false) {
                                Toast.makeText(taskEditText.getContext(), R.string.saved, Toast.LENGTH_SHORT).show();
                            } else {
                                finish();
                                startActivity(getIntent());
                            }
                        } else {
                            Toast.makeText(taskEditText.getContext(), R.string.invalidUrl, Toast.LENGTH_SHORT).show();
                        }
                    }
                })
                .setNegativeButton("Cancel", null)
                .create();
        dialog.show();
    }

    private void shareApplication() {
        ApplicationInfo api = getApplicationContext().getApplicationInfo();
        String filePath = api.sourceDir;

        Intent intent = new Intent(Intent.ACTION_SEND);
        intent.setType("application/vnd.android.package-archive");

        intent.putExtra(Intent.EXTRA_STREAM,
                Uri.parse(filePath));
        startActivity(Intent.createChooser(intent, "Share Using"));
    }

    private void ShareAPKLink() {
        String link = "https://github.com/nextcodelab/hymnsforandroid/raw/master/app/android-apps/HymnsForAndroid-Guitar.apk";
        Intent sendIntent = new Intent();
        sendIntent.setAction(Intent.ACTION_SEND);
        sendIntent.putExtra(Intent.EXTRA_TEXT, link);
        sendIntent.setType("text/plain");
        startActivity(sendIntent);
    }

    private void changeThemeColor() {
        theme = Theme.isNightModePreferred(sharedPreferences.getBoolean("nightMode", false));
        Log.i(getClass().getSimpleName(), "changeTheme: " + theme.name());
        hymnBookCollection.setTheme(theme);
        actionBar.setBackgroundDrawable(theme.getActionBarColor(selectedHymnGroup));
    }

    private void toggleDrawer() {
        if (mDrawerLayout.isDrawerOpen(mDrawerList)) {
            mDrawerLayout.closeDrawer(mDrawerList);
        } else {
            mDrawerLayout.openDrawer(mDrawerList);
        }
    }

    @Override
    // Get what the user chose from the Index of Hymns and display the Hymn
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        switch (requestCode) {
            case SEARCH_REQUEST:
                if (resultCode == RESULT_OK) {
                    try {
                        String rawData = data.getDataString().trim();
                        selectedHymnGroup = HymnGroup.getHymnGroupFromID(rawData);
                        hymnBookCollection.switchToHymnAndRememberChoice(rawData);
                    } catch (NoSuchHymnGroupException e) {
                        e.printStackTrace();
                    }
                }
                break;
        }
    }

    @Override
    public void onBackPressed() {
        Log.d(HymnsActivity.class.getSimpleName(), "onBackPressed Called");
        hymnBookCollection.goToPreviousHymn();

    }

    @Override
    public void onLyricVisible(String hymnId) {

        if (hymnId == null) hymnId = HymnGroup.DEFAULT_HYMN_NUMBER;

        try {
            selectedHymnGroup = HymnGroup.getHymnGroupFromID(hymnId);
            Log.i(getClass().getSimpleName(), "Page changed. setting title to: " + hymnId);
            currentHymnId = hymnId;
            actionBar.setTitle(hymnId);
            changeThemeColor();

            Log.d(getClass().getSimpleName(), "Done painting title");
        } catch (NoSuchHymnGroupException e) {
            e.printStackTrace();
        }
    }


    @Override
    public void onSharedPreferenceChanged(SharedPreferences sharedPreferences, String key) {
        preferenceChanged = true;
    }

    @Override
    public void onResume() {
        super.onResume();
        if (preferenceChanged) {
            setDisplayConfig();
            changeThemeColor();
            refreshHymnDrawer();
            hymnBookCollection.refresh();
            preferenceChanged = false;
        }
        hymnSwitcher = this;
    }

    private void setDisplayConfig() {
        if (sharedPreferences.getBoolean("keepDisplayOn", false)) {
            getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        } else {
            getWindow().clearFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        }
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        super.onConfigurationChanged(newConfig);
        // refresh screen when screen orientation changes
        hymnBookCollection.refresh();
    }

    @Override
    public void switchToHymn(String hymnId) {
        hymnBookCollection.switchToHymn(hymnId);
    }

    public static HymnSwitcher getHymnSwitcher() {
        return hymnSwitcher;
    }
}

