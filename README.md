# UniversalGalleryDemo

[![Build status](https://ci.appveyor.com/api/projects/status/6x7q45xsi2q8xi8t/branch/master?svg=true)](https://ci.appveyor.com/project/jernejk/universalgallerydemo/branch/master) (Unit tests are probably not executed)

This is a demo app for very simple gallery slide show which will work on Windows 10 devices including IoT and Xbox One.
You can use this app as a digital frame or advertisement or similar.

I developed and tested this for Raspberry Pi 2 with Windows 10 IoT core OS but it also works on other devices.

This is what app currently supports:
- Basic generic image provider (only WallDash.net is currently supported)
- Basic slide show (no animations)
- No interactions
- Raspberry Pi 2 support

Possible future features:
- HTTP server for remote controlling slideshow (no mouse and keyboard required on the device)
  - branch server_demo contains a naive implementation of an server on IoT device
- Animations (when I managed to figure out how to do that with 30+ fps)
- Mouse/touch/gamepad interactions
- Multiple sources

Update: Issues bellow are known issues for RaspBerry Pi 2. Read release notes for more details:
http://ms-iot.github.io/content/en-US/win10/ReleaseNotes.htm

At the moment Raspberry Pi 2 on build 10240 (tested on 8.8.2015) doesn't support smooth transitions between images in C#/XAML but I think this might be due graphics drivers or too weak/unstable power source (2.1A).
When animating, CPU is stuck at 100% and FPS is between 0-10 frames per seconds when testingd opacity and scale animations with CompositeTransform.
I also was unable to get mouse and keyboard to work on that device so I didn't implement any interactions.
