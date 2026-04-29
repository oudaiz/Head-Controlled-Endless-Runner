# Head-Controlled Endless Runner (Unity + Computer Vision)

## 🚀 Playable Version

You can download and run the full game here:

👉 https://drive.google.com/drive/folders/1xkh_snuHhj8RDKKuy-5aQTemty20O31o?usp=drive_link

**Steps:**

1. Download the project
2. Extract the files
3. Run:

   ```bash
   ./HeadRunner.x86_64
   ```
4. Choose input mode:

   * Keyboard
   * Camera (Head Control)

---

## Overview

This project is an Endless Runner game developed in Unity, where the player can be controlled either using the keyboard or using head movements via a webcam.

The system integrates:

* Unity game engine
* Computer vision using MediaPipe
* Python backend (Flask + WebSocket)
* Real-time communication between Python and Unity

---

## Features

* 3-lane endless runner gameplay
* Jump / Slide / Left / Right controls
* Real-time head tracking using webcam
* Smooth command detection and filtering
* Score system, coins, obstacles, and trains
* Dual input modes:

  * Keyboard
  * Camera (AI-based)

---

## System Architecture

Camera → Python (MediaPipe) → Command Detection → WebSocket → Unity Game

---

## How It Works

1. The Python system captures webcam input
2. Face landmarks are extracted using MediaPipe
3. Head movement is converted into commands:

   * LEFT
   * RIGHT
   * JUMP
   * SLIDE
4. Commands are sent to Unity via WebSocket
5. Unity executes the corresponding player actions

---

## Run the Project

### Prebuilt Version (Recommended)

The project includes a ready-to-run build.

Steps:

1. Open the `Build` folder
2. Run:

   ```bash
   ./HeadRunner.x86_64
   ```
3. Choose:

   * Keyboard mode
   * Camera mode

No installation required.

---

## Project Structure

```
python_controller/   → Python AI + API + detection
unity_scripts/       → Unity C# scripts
Build/               → Ready-to-run game
record/              → Game visuals
```

---

## Technologies Used

* Unity (C#)
* Python
* MediaPipe
* OpenCV
* Flask
* WebSockets

---

## Notes

* The camera system is precompiled using PyInstaller (`vision_runtime`)
* No Python installation is required for the prebuilt version
* Ensure webcam access is allowed

---

## Author

Oday Zidan
Computer Engineering Student
Pamukkale University

---
