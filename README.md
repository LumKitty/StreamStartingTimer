# Stream Starting countdown timer
Allows you to show an accurate countdown at the start of stream, and fire events at specified times.  
Events can be VNyan websockets, MixItUp commands or external executables  
* 100% Free and Open Source  
* No ads, no premium version, no subscription, no monetization of any sort  
* Lightweight portable executable
* No registry settings, just config.json files in the exe directory
<img width="691" height="295" alt="image" src="https://github.com/user-attachments/assets/30d01f4d-a5ca-4be1-ad2f-aa6b84800528" />


## Quick Start
1. Requires [.net 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0). This should already be installed on most modern versions of Windows
2. Download the .zip file from the [latest release](https://github.com/LumKitty/StreamStartingTimer/releases) and extract to wherever you keep your streamer tools
3. Run StreamStartingTimer.exe
4. Add a window capture in OBS capturing this application, put this on your "Stream Starting" scene
5. Add a crop filter to remove the top toolbar and the bottom status bar
6. Add a Chroma key filter to greenscreen the clock
7. Click the Clock Font button and set your desired font and size
8. Close the application
9. Run StreamStartingTimer.exe -m 5 for a five minute countdown

## Usage
This application is designed to be started from the commandline or launched from a Stream Deck. Commandline options:  
```-m xx``` Start a timer for xx minutes  
```-s xx``` Add xx seconds to the timer (can be used in conjunction with -m)  
```-p xx``` Start at xx minutes past the hour (e.g. if the time is 6:42 -p 45 will start the timer at 6:45. -p 0 will start at 7:00)  
```-c FileName``` Load a different configuration file instead of DefaultConfig.json  
```-e Filename``` Load a different events file instead of DefaultEvents.json (see the events section for more details)  

While the timer is running, you can used the +30s and +1m buttons to add time to the timer, e.g. if you are going to be running late

If the application was started with ```-m```, ```-s``` or ```-p``` it will automatically close once the timer expired. If it is manually started it will remain open.

## Usage from StreamDeck
1. Install [BarRaider's Advanced Launcher](https://marketplace.elgato.com/product/advanced-launcher-d9a289e4-9f61-4613-9f86-0069f5897125)
2. Add the Advanced launcher to a button on your stream deck
3. Click "Choose File" and point to StreamStartingTimer.exe
4. Add commandline arguments as described above

## Configuration
The clock font can be customised to match your overall aesthetic. Click the "Clock Font" text to choose your font, change the colour by entering a hex code immediately to right of the font button.  
If your clock colour is green, change the Background colour to a different colour hex code and adjust your OBS chroma key filter to match  
The "Align" setting must match what you have set in your OBS transform. This will allow you to resize the window without the clock being repositioned in your scene  
For automating tasks at specific times, click Edit Events to open the event editor

## Events
Clicking "Edit Events" will take you to the event editor. This allows you to automate e.g. running Twitch ads before you go live  
There are three types of event: VNyan, MixItUp and EXE. They all have the same properties:  
```Enabled``` This event will actually fire  
```EventType``` Either VNyan, MixItUp or EXE  
```Payload``` What to actually run (see the individual event descriptions below)  
```Refire``` If you add to the timer after this event has fired, should it run a second time  
```Time``` When the timer reaches this value, the event will fire  
If you save your events as DefaultEvent.json they will be loaded automatically at startup, otherwise you must specify them on the commandline with ```-e```

### VNyan event
Sends the payload as a websocket message to VNyan
### MixItUp event
Calls the specified MixItUp command (which must exist) any arguments to the command should go after the space character.
WARNING: If your MixItUp command has a space in its name, you must use the command ID instead. You can use the "Show MIU" button to get this
This will be fixed in a future version
### EXE event
Will be run exactly as if you'd typed it at a command prompt (e.g. powershell.exe D:\Twitch\MyStuff.ps1)

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
Copy DefaultConfig.json to e.g. SpecialConfig.json and launch with ```-s SpecialConfig.json```
### Different startup events needed e.g. streaming on a different platform
Save your events as e.g. Twitch.json and Youtube.json and launch with ```-e YouTube.json```
