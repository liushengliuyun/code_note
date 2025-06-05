package com.liusheng.x;

import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.text.TextUtils;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.Display;
import android.view.Gravity;
import android.view.KeyEvent;
import android.view.WindowManager;
import android.widget.FrameLayout;
import android.widget.ImageView;

import com.google.android.gms.ads.identifier.AdvertisingIdClient;
import com.google.android.gms.common.GooglePlayServicesNotAvailableException;
import com.google.android.gms.common.GooglePlayServicesRepairableException;
import com.unity3d.player.UnityPlayerActivity;

import java.io.IOException;
import java.util.concurrent.Executors;

public class XRuntimeActivity extends UnityPlayerActivity {

    public static final String LogTag = "-slotx-";
    public static XRuntimeActivity Instance;

    private boolean canUseBack = false;

    private ImageView bgView = null;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Log.d(XRuntimeActivity.LogTag, "onCreate XRuntimeActivity");
        if (mUnityPlayer == null) return;

        Instance = this;
        Log.d(XRuntimeActivity.LogTag, "XRuntimeActivity");

        //设置全屏
        ShowSplash();

        getWindow().addFlags(WindowManager.LayoutParams.FLAG_TRANSLUCENT_STATUS);
    }

    public void TryGetGoogleAdId(){
        Context context = getApplicationContext();
        Executors.newCachedThreadPool().execute(() -> {
            try {
                AdvertisingIdClient.Info idInfo = AdvertisingIdClient.getAdvertisingIdInfo(context);
                XUnityAgent.ExecuteUnityMethod(XUnityAgent.Method_GetGAID, idInfo.getId());
            } catch (IOException | GooglePlayServicesNotAvailableException |
                     GooglePlayServicesRepairableException e) {
                    XUnityAgent.ExecuteUnityMethod(XUnityAgent.Method_GetGAID, e.toString());
//                throw new RuntimeException(e);
            }
        });
    }


//    Vibrator vibrator;
//    ///震动
//    public void Vibrate(long[] pattern, int[] amplitudes, int index) {
//        if (vibrator == null) {
//            vibrator = (Vibrator) getSystemService(Context.VIBRATOR_SERVICE);
//        }
//
//        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
//            VibrationEffect vibrationEffect =  VibrationEffect.createWaveform(pattern, amplitudes, index);
//            vibrator.vibrate(vibrationEffect);
//        }
//        else
//        {
//            vibrator.vibrate(pattern, index);
//        }
//    }


    /// 联系我们
    public void SendEmail(String myEmail, String title, String mailContent, String choose) {
        Instance.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                Intent intent = new Intent(Intent.ACTION_SEND);
                intent.putExtra(Intent.EXTRA_EMAIL, new String[]{myEmail});
                intent.putExtra(Intent.EXTRA_SUBJECT, title);
                intent.setType("message/rfc822");
                Intent.createChooser(intent, choose);
                if (!TextUtils.isEmpty(mailContent)) {
                    intent.putExtra(Intent.EXTRA_TEXT, mailContent);
                }
                try {
                    Instance.startActivity(intent);
                } catch (Throwable e){
                    e.printStackTrace();
                }
            }
        });
    }

    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        if (keyCode == KeyEvent.KEYCODE_BACK && !canUseBack) {
            // 不执行任何操作，禁用返回键
            return true;
        }
        Log.d(XRuntimeActivity.LogTag, "onKeyDown :" + String.valueOf(keyCode));
        return super.onKeyDown(keyCode, event);
    }

    @Override
    public boolean onKeyUp(int keyCode, KeyEvent event) {
        if (keyCode == KeyEvent.KEYCODE_BACK && !canUseBack) {
            // 不执行任何操作，禁用返回键
            return true;
        }
        Log.d(XRuntimeActivity.LogTag, "onKeyUp :" + String.valueOf(keyCode));
        return super.onKeyUp(keyCode, event);
    }

    public void ShowSplash() {
        try {
            if (bgView != null){
                HideSplash();
            }
            bgView = new ImageView(this);
            bgView.setBackgroundResource(R.drawable.img_gamebg);
            bgView.setScaleType(ImageView.ScaleType.CENTER_CROP);

            Context context = getApplicationContext();
            WindowManager windowManager = (WindowManager) context.getSystemService(Context.WINDOW_SERVICE);
            Display display = windowManager.getDefaultDisplay();
            DisplayMetrics displayMetrics = new DisplayMetrics();
            display.getRealMetrics(displayMetrics);

            int realWidth = (int) (displayMetrics.heightPixels * (1700f / 750f));

            int realHeight = displayMetrics.heightPixels;
            // 创建 FrameLayout 的布局参数
            FrameLayout.LayoutParams layoutParams = new FrameLayout.LayoutParams(realWidth, realHeight);
// 设置视图在 FrameLayout 中居中
            layoutParams.gravity = Gravity.CENTER;

// 添加视图到 mUnityPlayer 并应用布局参数
            mUnityPlayer.addView(bgView, layoutParams);

            bgView.bringToFront();

//            mLVBlock = new LVBlock(this);
//            mLVBlock.setViewColor(Color.rgb(245, 209, 22));
//            mLVBlock.setShadowColor(Color.BLACK);
//            mLVBlock.startAnim();
//
//            FrameLayout.LayoutParams viewParams = new FrameLayout.LayoutParams(400, 400);
//            viewParams.topMargin = getResources().getDisplayMetrics().heightPixels / 2 + 200;
//            viewParams.leftMargin = getResources().getDisplayMetrics().widthPixels / 2 - 200;
//            mLVBlock.setLayoutParams(viewParams);
//
//            mUnityPlayer.addView(mLVBlock);
        } catch (Exception e) {
            // error("[onShowSplash]" + e.toString());
        }
    }

    public void HideSplash() {
        try {
            if (bgView == null)
                return;

            runOnUiThread(new Runnable() {
                public void run() {
                    mUnityPlayer.removeView(bgView);
                    bgView = null;
                    canUseBack = true;

//                    mLVBlock.stopAnim();
//                    mUnityPlayer.removeView(mLVBlock);
//                    mLVBlock = null;
                }
            });
        } catch (Exception e) {
            // error("[onHideSplash]" + e.toString());
        }
    }
}
