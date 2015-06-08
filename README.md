# UniversalGalleryDemo
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

At the moment Raspberry Pi 2 doesn't support smooth transitions between images in C#/XAML but I think this might be due graphics drivers or too weak/unstable power source (2.1A).
When animating, CPU is stuck at 100% and FPS is between 0-10 frames per seconds when testingd opacity and scale animations with CompositeTransform.
I was also unable to get mouse and keyboard to work on that device so I didn't implement any interactions.
