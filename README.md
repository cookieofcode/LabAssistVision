# Lab Vision

Proof of concept of a mixed reality application for the Microsoft HoloLens 2 integrating object recognition using cloud computing and real-time on-device markerless object tracking. The augmented objects provide interaction using hand input, eye-gaze, and voice commands to visually verify the tracking result.

This work is part of the thesis "Mixed Reality Task Assistance for Pharmaceutical Laboratories using Markerless Object Tracking and Cloud Services" submitted to the [University of Applied Sciences Northwestern Switzerland](https://www.fhnw.ch).

## Overview

*Lab Vision* provides a showcase for markerless tracking in pharmaceutical laboratories using the [Microsoft HoloLens 2](https://www.microsoft.com/de-de/hololens/hardware).

[![A demonstration video is available at: https://youtu.be/ru2a367seSQ](./teaser.gif)](https://youtu.be/ru2a367seSQ)
*A demonstration video in full length is available at: https://youtu.be/ru2a367seSQ*

### Key Features

- Provides a camera service using the [color camera](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/platform-capabilities-and-apis/locatable-camera#hololens-2) of the Microsoft HoloLens 2 in Unity without additional plugins
  - The video format can be switched during runtime
  - Provides camera extrinsic and intrinsic
  - Supports grayscale images (extracting luminance from NV12)
  - Supports NV12 to RGB conversion using a fragment shader
  - Camera simulation using the WebCam in Unity Editor
- Object Detection using [Custom Vision](https://customvision.ai) as Cloud Service
  - Provides tracker initialization for markerless tracking
  - *Experimental:* Run on repeat to provide tracking by detection
  - *Experimental:* Run object detection network (ONNX) locally on device using [Barracuda](https://docs.unity3d.com/Packages/com.unity.barracuda@1.0/manual/index.html)
- Markerless object tracking using [OpenCV Tracking API](https://docs.opencv.org/4.5.0/d9/df8/group__tracking.html)
  - Implemented Trackers: MOSSE, TLD, Boosting, Median Flow, MIL, CSRT, KCF
  - *Real-time tracking (30 FPS) is achieved using MOSSE at a resolution of 760x428 @ 30 FPS in grayscale and synchronous mode.*
- Interaction using Hand Input, eye gaze combined with voice commands (e.g. "*Detect*", "*Okay*") provided by [MRTK](https://github.com/microsoft/MixedRealityToolkit-Unity).
- Developer Console to change settings (e.g. video profile, color format, tracker, scenario) at runtime.
- Video display of the camera stream including a debug view of the bounding boxes of tracked objects.

## Documentation

### Setup

Lab Vision requires [OpenCVForUnity](https://assetstore.unity.com/packages/tools/integration/opencv-for-unity-21088) and a pretrained [Custom Vision](https://www.customvision.ai) prediction API. The following steps are required after cloning to setup the project:

1. Open the project (e.g. in the Unity Hub) with Unity Version 2019.4.15f1. *Note: Another Unity version may requires adjustments or API updates*

2. Import [OpenCVForUnity](https://assetstore.unity.com/packages/tools/integration/opencv-for-unity-21088) version 2.4.1 (e.g. using the Unity Package Manager). Only the the OpenCVForUnity Wrapper API and prebuilt OpenCV for Windows and UWP is required.

3. Open the LabVision scene.

4. Change the following in Build Settings (File > Build Settings):
   - Platform: Universal Windows Platform
   - Target Device: HoloLens

5. Verify the project settings (see Project Configuration)

6. Add the Custom Vision prediction key and prediction endpoint obtained from the portal to the [CustomVisionObjectDetectionService](Assets/MixedRealityToolkit.Generated/Extensions/Detection/CustomVisionObjectDetectionService.cs) profile. This setting can be found in the Game Object "MixedRealityToolkit" under the tab Extensions > Configurations > CustomVisionObjectDetectionService > DefaultObjectDetectionServiceProfile. *Note: If no profile is assigned, assign the default or create a new*.

### Dependencies

The following table contains dependencies required in this project:

| Dependency | Version | Resolvment | Remark |
| ------------- |:-------------:| :-----| :---|
| [OpenCVForUnity](https://assetstore.unity.com/packages/tools/integration/opencv-for-unity-21088) | 2.4.1 | Import manually (see [Setup](#setup)) | Paid Unity asset with precompiled [OpenCV](https://opencv.org) for [UWP](https://docs.microsoft.com/en-us/windows/uwp/) |
| [NuGet for Unity](https://github.com/GlitchEnzo/NuGetForUnity) | 2.0.1 | Included in repository |  Licensed under the MIT License |
| [DotNetWinRT](https://www.nuget.org/packages/Microsoft.Windows.MixedReality.DotNetWinRT) | 0.5.1049 | Included in repository, resolved by NuGet for Unity | NuGet Package |
| [Mixed Reality Toolkit for Unity](https://github.com/microsoft/MixedRealityToolkit-Unity) | 2.5.1 | Resolved by Unity Package Manager | Licensed under the MIT License |
| [Barracuda](https://docs.unity3d.com/Packages/com.unity.barracuda@1.0/manual/index.html) | 1.0.4 | Resolved by Unity Package Manager | Required to run the Custom Vision network on the device (experimental feature) |

**A pretrained Custom Vision network including a prediction endpoint and key is required.**

### Project Configuration

Ensure that the following settings are configured in Unity:

- Build Settings
  - Platform: *Universal Windows Platform*
  - Target Device: *HoloLens*
- Project Settings
  - Player
    - Scripting Define Symbols: *`OPENCV_USE_UNSAFE_CODE;DOTNETWINRT_PRESENT`*
    - Allow 'unsafe' code: *true*
    - Capabilities: *InternetClient, InternetClientServer, PrivateNetworkClientServer, Webcam, Microphone, HumanInterfaceDevice, Spatial Perception, Gaze Input*
    - XR Settings
      - Virtual Reality Supported: *true*
      - Depth Format: *16-bit*

**Ensure that each extension service (MixedRealityToolkit GameObject > Extension Services) has a profile assigned.**

### Build

- Build the Project using ARM64 (Debug/Release). *Note that running in Debug mode has high impact on performance*.

### Development

- It is possible to run the application using the Unity Editor and the Play Mode to get fast feedback during development. While object detection and tracking is supported, mapping to 3D is partially possible using Holographic Remoting due to missing intrinsic and extrinsic.
  - The [MonoCamera](Assets/LabVision/Camera/MonoCamera.cs) simulates the [Locatable Camera](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/platform-capabilities-and-apis/locatable-camera) of the device in the Unity Editor. Using [PhotoCapture](https://docs.unity3d.com/ScriptReference/Windows.WebCam.PhotoCapture.html) on repeat delivers the WebCam image in NV12 format (at a low framerate). The camera extrinsic and intrinsic required for mapping the 3D position are ignored.
  - In Holographic Remoting, the WebCam of the computer is used. A tracked object is then mapped to the collision point of the gaze of the main camera and the [Spatial Mesh](https://docs.microsoft.com/en-us/windows/mixed-reality/design/spatial-mesh-ux). *Experimental: Using [StreamCamera](Assets/LabVision/Camera/StreamCamera.cs), the camera stream of the device obtained by the [Windows Device Portal](https://docs.microsoft.com/en-us/windows/uwp/debug-test-perf/device-portal) can be used in the Unity Editor. Note that a proxy is required to bypass the authentication (does also not provide extrinsic and intrinsic).*
- The video profile, color format, and tracker can be switched during runtime. *Real-time tracking (30 FPS) is achieved using MOSSE at a resolution of 760x428 @ 30 FPS in grayscale and synchronous mode.*

### License

Lab Vision is open for use in compliance with MIT License. The grayscale shader for the video display is adapted from [HoloLensARTookit](https://github.com/qian256/HoloLensARToolKit), which is licensed under GNU Lesser General Public License v3.0.
