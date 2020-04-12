# VR Expert Application
This application is built for the Oculus Rift with Unity. Make sure you have Unity Hub installed already before you begin. Also please ensure you have the Oculus app and your headset properly setup.

## Before Getting Started
Please make sure you have done the following before you begin setting up the project.
- Install [Unity Hub](https://unity3d.com/get-unity/download)
- Install [Oculus App](https://www.oculus.com/rift/setup/?locale=en_US)

## Setup
- Open Unity Hub and add the application by selecting the project folder named “VR Expert App”.
- Install Unity Version 2019.1.11f1 (Unity Hub should prompt you to install this).
- Open the project from Unity Hub.
- Open the scene labeled 'Main' in `Assets/Scenes`
- From here you should be able to run the project and enter VR.
- If you are connecting across different networks, your router may block the ports we are using for communication between the two applications. You will need to forward ports 4444 and 8888.

## Features/Controls
Once you are connected to a technician you will have the following features available to you.
- *Technician Avatar*: You will see a digital representation of the Technician's avatar in virtual space, accompanied with a Video Feed. This appears when they successfully connect.
- *Video Feed*: There is a live stream feed of what the Technician is seeing from their webcam. You can change the location of this video by pressing a button labeled 'Change Video View' on the left controller, which will toggle location between Technician's view cone and the left hand controller.
- *Environment Scan*: Once the Technician performs a scan of the real world environment, the virtual models will be sent to the Expert app and you will see a pale mesh scan populate the scene.
- *Teleporting*: To traverse the scan you can teleport by pointing the left controller at the ground and squeezing the middle finger trigger. When you let go, you will jump to that position.
- *Highlight*:  By pressing the button labeled 'Highlight' on the right controller you can point at a part of the scan mesh in 3-dimensions to send a highlight to the Technician. This is sent when you let go of the button.
- *Annotation*: By squeezing the trigger on the right hand you can draw annotations in 3D space. These are sent to the technician when you let go of the button. If you wish to clear the drawings, press the button labeled 'Clear' on the right controller. The clearing of the drawings will also update in the technician’s view.

## Previous Scanning Method
During the development of these applications, we investigated two methods of scanning the physical environment: (1) using the built-in Hololens scanning hardware and software, and (2) using a separate Kinect with Skanect Pro software. Our current version relies exclusively on the Hololens scanning method.

In previous versions of this application we used a scan of the room that was created manually using a kinect, follow [this guide](https://github.com/THING-Lab/ARRA/blob/master/KinectScanning.md) if you would like to generate one of those scans. Our current application does not support importing manually generated scans, so you will have to edit the Unity project and add the scan to the scene yourself to use it.
