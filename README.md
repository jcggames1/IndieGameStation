# IndieGameStation
Source code for Indie Game Station

Here is the source code for my launcher app, Indie Game Station. It's made using the monogame framework and my own personal library, which I've included in the source. You'll have to add it as a reference to make it work.

If anyone wants to improve the code or add any features, go right ahead. It's not a complicated program so it shouldn't be difficult to figure out.

Here is the description text from the original post:


How To Use
Here, you will find instructions on how to use the various features.

Setting up your games directory

In the main folder of the app, you should see a file called "directory.txt". This is what tells the app where to look for games. Simply enter the path to your games folder (for example, "C:\Games") and save. When the app starts, this directory and any sub folders will be scanned for executables.

Note: The scanner will only scan one folder deep from the entered directory, so if your game is nested within two or more folders, it won't be picked up.

Modifying which kinds of games to look for

In the app folder, there should be a file called "extensions.txt". This file tells which extensions to look for when scanning for games. If your game has an extension that is not in the list, simply add it and save. Note that extensions are separated by line and are not case sensitive.

Adding custom thumbnails

If you want to add a custom thumbnail for a game, simply add an image to the same folder as the executable and give it the same name as the executable. For example, if your game is called "My Game.exe" then name the image "My Game.png". Acceptable image formats are PNG, JPG, BMP and GIF.

Adding a custom background and/or default thumbnail

If you want to change the background on the main menu, simply replace "bg.png" in the app's main folder with the image of your choice. If you want to replace the default thumbnail (the thumbnail that displays if there is none found for a particular game) then simply replace "d_icon.png" in the app folder. The dimensions of the images don't really matter, they will be automatically stretched within the app.

Disabling the fullscreenizer

By default, the fullscreenizer is enabled on startup. If you want to universally disable it, enter the settings menu by pressing Y on the controller and set the Fullscreenizer option to OFF.

Disabling the fullscreenizer for specific games

In some cases, you may want to disable the fullscreenizer only for certain games. In the app's folder, there should be a file called "fullscreen_exempt.txt". Add a new line to this file that is the exact name as the game's executable, not including the extension. For example, if your game is called "My Game.exe", then add the line "My Game" to fullscreen_exempt.txt. Note that games should be separated by line.

Setting up joystick to keyboard support

Included in this download is AntiMicro v2.23, an open-source joystick to keyboard mapping program. It is located in the AntiMicro folder within the main app folder. Once you've run it and have created a controller profile, click "Save As" and save the profile in the same folder as the game's executable, with the same name as the executable. For example, if your game is called "My Game.exe", you should save the profile in the same folder as "My Game.amgp". Note that it doesn't matter if it's called "My Game.controller.amgp" or something similar, it will still be detected. All that matters is the game name and the .amgp extension.

Exiting a game while running

If you want to quickly exit whichever game is currently running, or there is no way to quit in-game, there is an "emergency exit" button combination that will terminate the game running and return to the menu. On an XBox controller, hold down both thumbstick buttons and both shoulder buttons (four buttons total) to instantly exit the currently running game.
