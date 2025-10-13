# Stream Starting countdown timer
Allows you to show an accurate countdown at the start of stream, and fire events at specified times.  
Events can be VNyan websockets, MixItUp commands or external executables  
* Support for image fonts and Spout2 output (or just capture the window and greenscreen it)
* 100% Free and Open Source  
* No ads, no premium version, no subscription, no monetization of any sort  
* Lightweight portable executable
* No registry settings, just config.json files in the exe directory
<img width="874" height="455" alt="image" src="https://github.com/user-attachments/assets/034b483b-61ff-4c9b-a49a-f72c68ad6dea" /> 

## Quick Start
1. Requires [.net 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0). This should already be installed on most modern versions of Windows
2. Download the .zip file from the [latest release](https://github.com/LumKitty/StreamStartingTimer/releases) and extract to wherever you keep your streamer tools
3. Run StreamStartingTimer.exe
4. Add a window capture in OBS capturing this application, put this on your "Stream Starting" scene
5. Add a crop filter to remove the top toolbar and the bottom status bar
6. Change the capture properties:
   - Window Match Priority: "Match title, otherwise find window of same type"
   - Capture Cursor: Disabled
8. Add a Chroma key filter to the capture, leave it on the default greenscreen
9. Click the Config button and set your desired font and size
10. If you do not use either VNyan or MixItUp, click the config button and disable them
11. Close the application
12. Run StreamStartingTimer.exe -m 5 for a five minute countdown

<img width="490" height="493" alt="image" src="https://github.com/user-attachments/assets/c46e796b-46bb-4ed6-8952-b148c14294ae" />

## Usage
This application is designed to be started from the commandline or launched from a Stream Deck.  
Commandline options:  
```-m xx``` Start a timer for xx minutes  
```-s xx``` Add xx seconds to the timer (can be used in conjunction with -m)  
```-p xx``` Start a timer that targets xx minutes past the hour (e.g. if the time is 6:42 -p 45 will start a 3 min timer (finishing at 6:45:00). -p 0 will start an 18 min (finishing at at 7:00:00)  
```-c FileName``` Load a different configuration file instead of DefaultConfig.json  
```-e Filename``` Load a different events file instead of DefaultEvents.json (see the events section for more details)  

If the application was started with ```-m```, ```-s``` or ```-p``` it will automatically close once the timer expired. If it is manually started it will remain open.

## Adjusting the timer at runtime
While the timer is running, you can use the +30s and +1m buttons to add time to the timer, e.g. if you are going to be running late
The timer can also be adjusted from the commandline with the ```-a``` parameter. This alters the behaviour of ```-m``` and ```-s``` which will now make adjustments to the current time. These can be negative. ```-p``` can also be used but this will force the time to xx minutes past the hour, ignoring the current time.  

Examples:  
```StreamStartingTimer.exe -a -m 5``` - Add five minutes to an already running timer  
```StreamStartingTimer.exe -a -s -30``` - Remove 30 seconds from an already running timer  

## Usage from StreamDeck
1. Install [BarRaider's Advanced Launcher](https://marketplace.elgato.com/product/advanced-launcher-d9a289e4-9f61-4613-9f86-0069f5897125)
2. Add the Advanced launcher to a button on your stream deck
3. Click "Choose File" and point to StreamStartingTimer.exe
4. Add commandline arguments as described above

## Image Font (Spout2) Configuration
1. The best way to use this timer is with an "Image Font". Create a folder named "DefaultFont", and place 11 PNG files, named 0.png, 1.png .. 9.png and colon.png.
2. Every PNG must be the exact same width and height, with the exception of colon.png which can be a different width.
3. Click the config button and enable spout2 output. If necessary change the Font Dir setting to point to the folder with your PNGs in it
4. If you aren't already using it, install the [OBS-Spout plugin](https://github.com/Off-World-Live/obs-spout2-plugin)
5. Start the timer
6. Add a new Spout2 capture source in OBS, ensure that the Spout Sender is set to "StreamStartingTimer"

An example font is included on the release page (DemoFont.zip) but ideally you should create your own that matches your personal aesthetic

## Events
<img width="785" height="336" alt="image" src="https://github.com/user-attachments/assets/9ddb4107-d371-4d79-b012-9fcf57742f56" />

Clicking "Edit Events" will take you to the event editor. This allows you to automate e.g. running Twitch ads before you go live  
There are three types of event: VNyan, MixItUp and EXE. They all have the same properties:  
```Enabled``` This event will actually fire  
```EventType``` Either VNyan, MixItUp or EXE  
```Payload``` What to actually run (see the individual event descriptions below)  
```Refire``` If you add to the timer after this event has fired, should it run a second time  
```Time``` When the timer reaches this value, the event will fire  
If you save your events as DefaultEvents.json they will be loaded automatically at startup, otherwise you must specify them on the commandline with ```-e```
Some event types will have additional properties

### VNyan event
Sends the payload as a websocket message to VNyan
### MixItUp event
Calls the specified MixItUp command (which must exist). If you specify any arguments they will be available as $AllArgs in MixItUp
### EXE event
Will be run exactly as if you'd typed it at a command prompt (e.g. powershell.exe). Arguments may be specified, and you can set the window to e.g. Minimised (some apps may override this)

## Testing
Running the application without commandline parameters will open it in setup/test mode. Immediately to the right of the +1m button you can set a time, in seconds for the timer to start at. The application will not quit when the timer expires  

## StatusBar indicators
When the application starts, the indicators in the bottom left corner will be yellow and we will attempt to connect to VNyan and MixItUp  
When they turn green we have successfully connected
If they turn red, connection has timed out, but we will keep retrying. This is a visual indicator to actually start your streaming software! :D

## Advanced usage
Special use cases that need a bit more work:
### VNyan or MixItUp are on a different machine
Edit DefaultConfig.json and change the VNyanURL or MixItUp URL, replacing localhost with the IP address or name of the machine
### Different clock font needed for certain streams
Save your config file as something other than DefaultConfig.json and launch with e.g. ```-s SpecialConfig.json```
### Different startup events needed e.g. streaming on a different platform
Save your events as e.g. Twitch.json and Youtube.json and launch with e.g ```-e YouTube.json```
