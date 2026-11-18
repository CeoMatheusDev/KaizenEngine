# Protect Kaizen Engine Core
-keep class com.kaizen.** { *; }
-keep class org.kaizenengine.** { *; }
-keep class ** extends org.kaizenengine.kaizen.plugin.KaizenPlugin { *; }

# Protect methods exposed to Kaizen via @UsedByKaizen
-keepattributes *Annotation*
-keepclassmembers class * {
    @org.kaizenengine.kaizen.plugin.UsedByKaizen *;
}

# Protect JNI bindings
-keepclasseswithmembernames class * {
    native <methods>;
}
-keep public class * extends android.app.Activity
-keep public class * extends android.app.Application
-keep public class * extends android.app.Service
