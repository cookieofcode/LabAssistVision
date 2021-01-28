/*
*  FPSUtils.cs
*  HoloLensARToolKit
*
*  This file is a part of HoloLensARToolKit.
*
*  HoloLensARToolKit is free software: you can redistribute it and/or modify
*  it under the terms of the GNU Lesser General Public License as published by
*  the Free Software Foundation, either version 3 of the License, or
*  (at your option) any later version.
*
*  HoloLensARToolKit is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU Lesser General Public License for more details.
*
*  You should have received a copy of the GNU Lesser General Public License
*  along with HoloLensARToolKit.  If not, see <http://www.gnu.org/licenses/>.
*
*  Copyright 2020 Long Qian
*
*  Author: Long Qian
*  Contact: lqian8@jhu.edu
*
*/

/*
 * Modified by cookieofcode (cookieofcode@gmail.com)
 */

using System;
using System.Collections.Generic;

/// <summary>
/// FPSUtils class provides utility functions for FPS recording.
/// </summary>
public static class FPSUtils
{
    /// <summary>
    /// The queue to record the timestamps of rendering, used to calculate render FPS. [internal use]
    /// </summary>
    private static Queue<long> qRenderTick = new Queue<long>();

    /// <summary>
    /// The queue to record the timestamps of video frames, used to calculate video FPS. [internal use]
    /// </summary>
    private static Queue<long> qVideoTick = new Queue<long>();

    /// <summary>
    /// The queue to record the timestamps of tracking performed, used to calculate tracking FPS. [internal use]
    /// </summary>
    private static Queue<long> qTrackTick = new Queue<long>();

    /// <summary>
    /// Record the current timestamp for rendering a frame. [internal use]
    /// </summary>
    public static void RenderTick()
    {
        while (qRenderTick.Count > 49)
        {
            qRenderTick.Dequeue();
        }
        qRenderTick.Enqueue(DateTime.Now.Ticks);
    }

    /// <summary>
    /// Get the average rendering period for the previous 50 occurrence. [public use]
    /// </summary>
    /// <returns>Average rendering period in millisecond</returns>
    public static float GetRenderDeltaTime()
    {
        if (qRenderTick.Count == 0)
        {
            return float.PositiveInfinity;
        }
        return (DateTime.Now.Ticks - qRenderTick.Peek()) / 500000.0f;
    }


    /// <summary>
    /// Record the current timestamp for video frame arrival. [internal use]
    /// </summary>
    public static void VideoTick()
    {
        while (qVideoTick.Count > 49)
        {
            qVideoTick.Dequeue();
        }
        qVideoTick.Enqueue(DateTime.Now.Ticks);
    }

    /// <summary>
    /// Get the average video frame period for the previous 50 occurrence. [public use]
    /// </summary>
    /// <returns>Average video frame period in millisecond</returns>
    public static float GetVideoDeltaTime()
    {
        if (qVideoTick.Count == 0)
        {
            return float.PositiveInfinity;
        }
        return (DateTime.Now.Ticks - qVideoTick.Peek()) / 500000.0f;
    }

    /// <summary>
    /// Record the current timestamp for tracking performed. [internal use]
    /// </summary>
    public static void TrackTick()
    {
        while (qTrackTick.Count > 49)
        {
            qTrackTick.Dequeue();
        }
        qTrackTick.Enqueue(DateTime.Now.Ticks);
    }

    /// <summary>
    /// Get the average tracking period for the previous 50 occurrence. [public use]
    /// </summary>
    /// <returns>Average tracking period in millisecond</returns>
    public static float GetTrackDeltaTime()
    {
        if (qTrackTick.Count == 0)
        {
            return float.PositiveInfinity;
        }
        return (DateTime.Now.Ticks - qTrackTick.Peek()) / 500000.0f;
    }

}
