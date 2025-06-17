"# Unity_Avatar" 
A Unity desktop application for Windows that lets users interact with an avatar using text or voice input (via microphone). The avatar responds using the Gemini API through a proxy server.

Features-
Voice Input: Press the Spacebar or click "Start Recording" to use the microphone.
Text Input: Type messages to interact with the avatar.
Avatar Response: The avatar speaks responses using text-to-speech.

Clone - https://github.com/anomalisfree/Unity-Text-to-Speech-using-Google-Cloud
https://github.com/UnityGameStudio/Gemini-Unity-Google-Cloud

Build for Windows:
In Unity, go to File > Build Settings, select PC, Mac & Linux Standalone, Target Platform: Windows.
Build to Builds/Windows as Avatar_SampleScene.exe.


Run the Application:
Double-click Avatar_SampleScene.exe.
Ensure microphone permissions are enabled (Windows Settings > Privacy > Microphone).
Press Spacebar or click "Start Recording" to use the microphone, or type a message to interact with the avatar.



Scripts:
MicrophoneManager.cs: Handles microphone recording (Spacebar or UI button).
MicrophoneTrigger.cs: UI button to toggle recording.
UnityAndGeminiV3.cs: Sends user input to the Gemini API via a proxy server.
