# Unity-ConsoleTiny
Console Tiny is a powerful replacement for Unity's editor console. (Works with Unity 2022.1.0b16)

## Feature
- Text Search Filter
- Multi-line Display
- Colored Callstacks
- Callstack Navigation
- Custom Filters
- DLL Support
- Lua Support
- Wrapper Support
NOTE: This still lacks a few features that are avaialble in Unity's default console but (hopefully) those are trivial.
To open a file in VS, this plugin uses a binary built using another open source project(by original author) [Visual Studio File Open Tool](https://github.com/akof1314/VisualStudioFileOpenTool)

![](https://github.com/akof1314/Unity-ConsoleTiny/raw/master/DLLTest/screenshot.png)

## Install
- Unity 2018.x (or later)
	- `Packages\manifest.json`

`manifest.json` file add line:

```
"com.wuhuan.consoletiny": "file:../PackagesCustom/com.wuhuan.consoletiny"

```

## Usage
Open window: `Ctrl+Shift+T` (Linux/Windows) or `Cmd+Shift+T` (OS X).

## License
MIT