# ACM Multimedia Open Source Competition

## Prerequisites

This document will help you get set up with VR2Gather. In order to launch
VR2Gather, we first have to make sure you have a couple of things installed:

- *Docker*: For running the VR2Gather Orchestrator.
  Get it here: https://www.docker.com/products/docker-desktop/
- *Unity Hub*: For launching the VR2Gather demo application.
  Get it here: https://unity.com/download
- *cwipc*: A open-source point cloud compression library.
  Get it here: https://github.com/cwi-dis/cwipc/releases/tag/v7.5.3

Hardware requirements:

- A capable machine with a modern graphics card
- A VR headset, preferably an Meta Quest 2 or Meta Quest Pro (optional but
  encouraged)
- A depth camera, either a Azure Kinect or Intel Realsense (optional)

In order to launch a shared experience over the network, a second setup like
this is required. Though single-user experiences work just as well.

Note that while downloading and installing `cwipc` is optional and VR2Gather
will work without it, we do encourage you to do so, as it demonstrates the
point cloud streaming capabilities of the platform, even if you don't own a
depth camera through synthetic point clouds.

Further, though VR2Gather is cross-platform compatible, it is easiest to set up
and use on Windows. Some of the functionality may even not be available on
other platforms, e.g. support for VR headsets or point cloud acquisition.

## Setup

### Orchestrator

Unzip the archive `VR2Gather-Orchestrator.zip` and using a terminal
application, enter the extracted folder and run the following command to launch
a new container using Docker:

    docker compose up

This will launch the VR2Gather orchestrator on port 8090, ready to accept
connections.

### `cwipc`

Download the latest installer for `cwipc` from Github at this address:
https://github.com/cwi-dis/cwipc/releases/tag/v7.5.3

Open the installer, follow the instructions and wait for the installation to
complete. Note that this is optional, if you either don't have a depth camera
or simply don't need point cloud support. VR2Gather will work without it.

### VR2Gather

The following paragraphs will guide you through setting up a quick demo
experience complete with point cloud rendering (if available). The application
that you are going to be opening, `VRTApp-Sample` represents a minimum viable
setup to build a VR2Gather application. The source code for the VR2Gather
Unity package itself can be found in the folder `nl.cwi.dis.vr2gather/`.

If available, connect your depth camera and VR headset to your computer and
make sure they are running correctly.

Extract the archive `VR2Gather.zip` and open *Unity Hub*. Click *Add*, followed
by *Add project from disk*. Navigate to the extracted *VR2Gather* folder,
select *VRTApp-Sample* and click *Add Project*. Wait for Unity Hub to
potentially finish installing the editor and then open the project. On the
first launch, this will take a short while as Unity installs all dependencies.

Once opened, in the project window, select *All Scenes* and double-click
`VRTLoginManager` to open to login manager scene. Hit the play button to launch
the experience.

If you want to have a Social VR experience with a second participant, these
steps need to be repeated on a second machine as well. Also make sure that on
the second machine, once you have opened the `VRTLoginManager` scene, select
the `VRTInitializer` GameObject and update the field *Orchestrator URL* to a
hostname/IP address under which the orchestrator is reachable over the network,
i.e. the IP address of the machine the orchestrator is running on.

## Navigating the Experience

First enter your username and confirm. The application will contact the
orchestrator and fetch some basic information. If everything went well, your
will be presented with a start menu. Hit *Play* to start a new experience.

*Note:* If you are not using a headset, it may happen that all you see is a
black screen. Try scrolling up on your scroll wheel to reveal the scene.

If you share the experience with another participant:

- On the first machine:
  - Click *Create*, update session settings if necessary and confirm
  - Then wait on the subsequent screen until your partners username shows up
    in the list titled *Connected Users*
  - Click *Start* and the experience will be launched
- On the second machine:
  - Click *Join* and wait for a session to appear in the dropdown menu titled
    *Select session to join*
  - Select the session and hit *Join* and the experience will be launched

If you want to experience the scene alone:

- Click *Create*, update session settings if necessary and confirm
- Click *Start* and the experience will be launched

On a keyboard, use WASD to navigate and the mouse's scroll wheel to adjust your
vertical viewpoint. Hold ALT, use the mouse cursor and left click to interact
with objects. Hold ALT, use the mouse cursor and right click to teleport.

Hit ESC and click *Leave Session* to leave the session and return to the main
menu.

Using a VR headset, use the D-pad on your controller to navigate. Hold and
release the trigger button to teleport. Extend your index finger and move
towards interactable objects to interact with them.

Hit the Oculus button on your controller and select *Leave Session* on the menu
that pops up to leave the session and return to the main menu.
