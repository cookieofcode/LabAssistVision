using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays the FPS measured by <see cref="FPSUtils"/> for debugging purposes. Adapted from https://github.com/qian256/HoloLensARToolKit/blob/master/HoloLensARToolKit/Assets/ARToolKitUWP/Scripts/ARUWPController.cs.
/// </summary>
[DisallowMultipleComponent]
public class FPSDisplayManager : MonoBehaviour
{
    #region Member Variables
    /// <summary>
    /// Update Render FPS text only if enabled.
    /// </summary>
    private bool _displayRenderFPS;

    /// <summary>
    /// Update Video FPS text only if enabled.
    /// </summary>
    private bool _displayVideoFPS;

    /// <summary>
    /// Update Track FPS text only if enabled.
    /// </summary>
    private bool _displayTrackFPS;

    /// <summary>
    /// Displays Render FPS.
    /// </summary>
    public TextMeshProUGUI renderFPS;

    /// <summary>
    /// Displays Video FPS.
    /// </summary>
    public TextMeshProUGUI videoFPS;

    /// <summary>
    /// Displays Track FPS.
    /// </summary>
    public TextMeshProUGUI trackFPS;

    /// <summary>
    /// Average render frame period in millisecond for previous 50 frames provided by <see cref="FPSUtils"/>.
    /// </summary>
    private float _renderDeltaTime;

    /// <summary>
    /// Average video frame period in millisecond for previous 50 frames provided by <see cref="FPSUtils"/>.
    /// necessary.
    /// </summary>
    private float _videoDeltaTime;

    /// <summary>
    /// Average track frame period in millisecond for previous 50 frames provided by <see cref="FPSUtils"/>.
    /// necessary.
    /// </summary>
    private float _trackDeltaTime;
    #endregion // Member Variables

    #region Internal Methods

    /// <summary>
    /// Retrieve the Render FPS.
    /// </summary>
    /// <returns>Number of frames per second</returns>
    private float GetRenderFPS()
    {
        return 1000.0f / _renderDeltaTime;
    }

    /// <summary>
    /// Retrieve the Video FPS.
    /// </summary>
    /// <returns>Number of frames per second</returns>
    private float GetVideoFPS()
    {
        return 1000.0f / _videoDeltaTime;
    }

    /// <summary>
    /// Retrieve the Track FPS.
    /// </summary>
    /// <returns>Number of frames per second</returns>
    private float GetTrackFPS()
    {
        return 1000.0f / _trackDeltaTime;
    }
    #endregion // Internal Methods

    #region Unity Overrides

    private void Start()
    {
        _displayTrackFPS = renderFPS != null;
        _displayRenderFPS = trackFPS != null;
        _displayVideoFPS = videoFPS != null;
    }

    private void LateUpdate()
    {
        _renderDeltaTime = FPSUtils.GetRenderDeltaTime();
        _trackDeltaTime = FPSUtils.GetTrackDeltaTime();
        _videoDeltaTime = FPSUtils.GetVideoDeltaTime();

        if (_displayRenderFPS)
        {
            renderFPS.text = $"Render: {_renderDeltaTime:0.0} ms ({GetRenderFPS():0.} fps)";
        }

        if (_displayVideoFPS)
        {
            videoFPS.text = $"Video:   {_videoDeltaTime:0.0} ms ({GetVideoFPS():0.} fps)";
        }

        if (_displayTrackFPS)
        {
            trackFPS.text = $"Track:   {_trackDeltaTime:0.0} ms ({GetTrackFPS():0.} fps)";
        }

    }
    #endregion // Unity Overrides
}
