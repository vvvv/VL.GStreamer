# VL.GStreamer
A set of nodes to use the multimedia framework GStreamer inside vvvv beta and gamma

## Installation
Install [GStreamer 1.16.2](https://gstreamer.freedesktop.org/data/pkg/windows/1.16.2/gstreamer-1.0-mingw-x86_64-1.16.2.msi) to default location (`C:\gstreamer`) using the __Complete__ option in the installer

### vvvv beta
- Install [vvvv beta](https://vvvv.org/downloads) (minimum required version is beta38.1)
- Inside from the VL patch editor open the nuget command line and type `nuget install VL.GStreamer -prerelease -version 1.0.18-gadcd7f95e5` - this is the latest version working with vvvv beta

### vvvv gamma
- Install [vvvv gamma](https://vvvv.org/blog/vvvv-gamma-2019.2-preview) (minimum required version is 2019.2-0374)
- Inside from the VL patch editor open the nuget command line and type `nuget install VL.GStreamer -prerelease`

## Building
Note that this is only for developers, to use the nuget you only need the installation steps above.
- Install [Visual Studio 2019](https://www.visualstudio.com/downloads)
- Open `src/VL.GStreamer.sln` and build

## Example
For now there's only one example found in `lib\packs\VL.GStreamer\Overview.v4p`
