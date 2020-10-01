# AR Unity Examples

Example projects that use [*AR Foundation 3.1*](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@3.1/manual/index.html) and [*AR Foundation 4.1*](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.1/manual/index.html) its functionality with sample assets and components.

# Examples

## ARCompass

This is a good starting sample that enables point cloud visualization and plane detection. There are buttons on screen that let you pause, resume, reset, and reload the ARSession.

When a plane is detected, you can tap on the detected plane to place a cube on it. This uses the `ARRaycastManager` to perform a raycast against the plane.

| Action | Meaning |
| ------ | ------- |
|   <img src="https://storage.googleapis.com/fns-blog/public/frontend/assets/images/project/ar-compass/pointerMarker.png" width="100%" alt="Compass Camera Marker"></img>     | North Direction Calculation For the value in the [pointer marker](https://storage.googleapis.com/fns-blog/public/frontend/assets/images/project/ar-compass/pointerMarker.png) property, the pointer is always measured relative to the rotation of the marker at its current position.|
|<img src="https://storage.googleapis.com/fns-blog/public/frontend/assets/images/project/ar-compass/cameraMarker.png" width="100%" alt="Compass Pointer Marker"></img>|North Direction For the value in the [camera marker](https://storage.googleapis.com/fns-blog/public/frontend/assets/images/project/ar-compass/cameraMarker.png) property, the pointer is always measured relative to the top of the screen at its current position. |
| Tracking State | If the focus of the AR camera is the marker, the tracking status return as the marker is scanned. When the AR camera moves away, the marker returns not tracked when the marker leaves the frame. |

| Camera Marker Test | Pointer Marker Test |
| ------ | ------- |
| ![](https://storage.googleapis.com/fns-blog/public/frontend/assets/images/project/ar-compass/cameraMarker_Test1.gif) | ![](https://storage.googleapis.com/fns-blog/public/frontend/assets/images/project/ar-compass/pointerMarker_Test1.gif) | 
