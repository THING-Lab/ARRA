# VR Expert Application
This application is built for the Oculus Rift with Unity. Make sure you have Unity Hub installed already before you begin. Also please ensure you have the Oculus app and your headset properly setup.

## Setup
- Open Unity Hub and add the application by selecting the project folder.
- Install Unity Version 2019.1.11f1 (Unity Hub should prompt you to install this).
- Open the project from Unity Hub.
- From here you should be able to run the project and enter VR.
- If you are connecting across different network, your router may block the ports we are using. We recommend forwarding ports `4444` and `8888`

## Features/Controls
Once you are connected to a technician you should have the following features available to you.
- *Technician Avatar*: You will see a digital representation of the Technician's avatar in space, acompanied with a Video Feed. This appears when they successfully connect.
- *Video Feed*: There is a live stream feed of what the Technician is seeing from their webcam, you can swap it's position by pressing a button labeled 'Toggle Video View' on the left controller between being in the Technician's view cone or attached to the left hand controller.
- *Environment Scan*: Once the Technician performs a scan of the environment, it will be sent to the Expert app and you will see a pale mesh scan populate the scene.
- *Teleporting*: To traverse the scan you can teleport by pointing the left controller at the ground and squeezing the middle finger trigger. When you let go, you will jump to that position.
- *Highlight*: By pressing the button labeled 'Highlight' on the right controller you can point at a part of the scan mesh to send a highlight to the Technician. This is sent when you let go of the button.
- *Annotation*: By squeezing the trigger on the right hand you can draw annotations in 3D space. These are sent to the technician when you let go of the button. If you wish to clear the drawings, press the button labeled 'Clear' on the right controller.
