package com.liusheng.x;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

public class XOpenActivity extends Activity

{
    @Override protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        // GamePlayerActivity launchModle = singleTop
        // 进入游戏后 栈结构为(栈顶 -> 栈底) unity(singleTask) - GamePlayerActivity(singleTop)
        // 当前栈根为GamePlayerActivity，重新进入游戏后会创建 新实例GamePlayerActivity
        // isTaskRoot当前activity是否在栈根节点，如果是新GamePlayerActivity返回false
        Log.d(XRuntimeActivity.LogTag, "onCreate XOpenActivity");
        if(!this.isTaskRoot()){
            // 获得当前的行为
            Intent intent = getIntent();
            if(intent != null){
                String action = intent.getAction();
                // 如果当前行为是点击应用程序启动activity
                if(intent.hasCategory(Intent.CATEGORY_LAUNCHER) && Intent.ACTION_MAIN.equals(action)){
                    // 直接结束
                    finish();
                    return;
                }
            }
        }

        Intent intent = new Intent(XOpenActivity.this, XRuntimeActivity.class);
        startActivity(intent);
    }
}
