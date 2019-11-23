Thank you for buying this plugin.

This plugin does not work in the Unity Editor.

====================================
Setup your project for Google ARCore
====================================

1) Switch platform to Android

2) Download and import Google ARCore 1.10 from http://tinyurl.com/3D4ARCore-ARs

3) In the player settings set:
 -Scripting define symbols to HAS_GOOGLE_ARCORE
 -Minimum API level to Android 7
 -Dynamic batching enabled
 -Multithreaded rendering disabled
 -Google ARCore supported enabled

4) Open ARProvider prefab and link "ARCore Device" to "AR Core Prefab"


======================================
Setup your project for Huawei AREngine
======================================

1) Setup your project for Google ARCore

2) Download and import Huawei AREngine 1.7 from http://tinyurl.com/3D4ARCore-ARs

3) Open Assets/Examples/WorldARSample/WorldARConfig.asset and check "Enable Depth"

4) Open Assets/Examples/WorldARSample/preview.scene and create prefab "HuaweiARDevice" from the Huawei AR Device object

5) Remove from the prefab WorldARController

6) Open Assets/3DR4ARCore/Prefabs/ARProvider.prefab and link prefab "HuaweiARDevice" to "AR Core Prefab"

7) In the folder Assets/HuaweiARUnitySDK/Plugins delete google_ar_optional.aar and google_ar_required.aar

8) Open Assets/Examples/Common/SessionComponent.cs and delete method OnGUI and _DoQuit

9) Delete Assets/3DR4ARCore/Plugins/Android/HUAWEI AR Engine SDK_v1.7.0.aar

10) In the Player settings add into "Scripting define symbols" to the end ";HAS_HUAWEI_ARENGINE"

Note: if you had an ARCore version installed on your device then you will have to delete the app before using AREngine version