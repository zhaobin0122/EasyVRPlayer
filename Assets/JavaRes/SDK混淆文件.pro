
-keep class com.EasyMovieTexture.** { *; }
-keep class com.meelive.vr.** { *; }

-keep class bitter.jnibridge.**{ *; }
-keep class com.unity3d.**{ *; }
-keep class org.fmod.**{ *; }

-keep class com.huawei.automation.**{ *; }
-keep class com.huawei.dfx.**{ *; }
-keep class com.huawei.hvr.**{ *; }
-keep class com.huawei.vrlab.**{ *; }
-keep class com.huawei.hvr.**{ *; }

-keep class com.aw.vrsdk.**{ *; }
-keep class com.picovr.**{ *; }
-keep class com.psmart.**{ *; }
-keep class com.pvr.**{ *; }
-keep class com.unity3d.player.**{ *; }
-keep class com.pico.loginpaysdk.** { *; }

-keep class com.htc.** { *; }
-keep class com.qualcomm.** { *; }
-keep class com.google.flatbuffers.** { *; }
-keep class vive.wave.** { *; }

-keep class com.nolovr.** { *; }
-keep class com.nibiru.** { *; }
-keep class ruiyue.controller.** { *; }
-keep class ruiyue.controllersdklib.** { *; }
-keep class com.dlodlo.** {*;}
-keep class com.qualcomm.** { *; }
-keep class com.sixdof.** { *; }
-keep class ruiyue.gesture.** { *; }
-keep class ruiyue.sixdof.** { *; }
-keep class x.core.ui.view.** {*;}

-keep class * extends android.app.Application
-keep class * extends android.app.Service
-keep class * extends android.content.ContentProvider
-keep class * extends android.content.BroadcastReceiver

-keep class * extends android.app.Activity {
    public protected <fields>;
    public protected <methods>;
}

-keep class * extends android.opengl.GLSurfaceView {
    public protected <fields>;
    public protected <methods>;
}

-keep class * extends android.view.SurfaceView {
    public protected <fields>;
    public protected <methods>;
}

-keep class android.app.INibiruVRManager{
   public <fields>;
   public <methods>;
}

-keepclassmembers class * extends android.os.Parcelable {
    static android.os.Parcelable$Creator CREATOR;
}

-keep class com.google.vrtoolkit.cardboard.GLSurfaceView2{
   public <fields>;
   public <methods>;
   private <methods>;
   native <methods>;
}










