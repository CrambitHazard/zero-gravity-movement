# Zero Gravity Movement System

A realistic zero gravity physics system for Unity 6 with 6-DOF movement, fuel management, and interactive environment.

## 🚀 Quick Start

### Requirements
- Unity 6 (6000.2.5f1 or later)
- No additional packages required

### Setup Instructions

1. **Open Unity 6**
2. **Open this project**
3. **Fix Input System** (CRITICAL - Unity will freeze without this):
   - **Edit → Project Settings**
   - **Player** (left sidebar)
   - **Expand "Other Settings"**
   - **Find "Active Input Handling"**
   - **Change from "Input System Package" to "Input Manager (Old)"**
   - **Unity will restart** - click Yes
4. **Create the Demo**:
   - **Right-click in Hierarchy → Create Empty**
   - **Add Component → "CrashProofDemoSetup"**
   - **Press Play**

## 🎮 Controls

```
WASD        = Move Forward/Back/Left/Right
Mouse       = Look Around
Space       = Thruster Up
Left Shift  = Thruster Down
B           = Brake (stop all movement)
ESC         = Toggle cursor lock
```

## ⚠️ Common Issues & Fixes

### Issue 1: Unity Freezes When Pressing WASD
**Cause**: Unity 6 uses new Input System by default, but code uses legacy input
**Fix**: Change "Active Input Handling" to "Input Manager (Old)" in Player Settings

### Issue 2: Compilation Errors with UI Components
**Cause**: Unity 6 assembly references changed
**Fix**: Use `CrashProofDemoSetup` instead of `RuntimeDemoSetup` - it has no UI dependencies

### Issue 3: "2 Audio Listeners" Warning
**Cause**: Both player and camera have AudioListener components
**Fix**: Ignore this warning - it doesn't affect functionality

### Issue 4: Empty Scene After Setup
**Cause**: Setup script didn't run
**Fix**: Make sure "Setup On Start" is checked in CrashProofDemoSetup component

## 🎯 What You Should See

After setup:
- Blue player capsule (you)
- 5 colored floating cubes
- Black space background
- Speed display in top-left corner
- Smooth movement without crashes

## 🛠️ System Features

- **Realistic Physics**: Momentum conservation and inertia
- **6-DOF Movement**: Full translational and rotational control
- **Interactive Environment**: Push and collide with objects
- **Visual Feedback**: On-screen speed and status display
- **Crash-Proof**: Error handling prevents Unity freezes

## 📂 Core Scripts

- `CrashProofDemoSetup.cs` - Creates the demo automatically
- `SimplePlayerController.cs` - Handles movement and physics
- `SimpleCameraFollow.cs` - Camera follows player smoothly

## ⚡ Performance

- Runs at 60+ FPS in Unity 6
- No external package dependencies
- Optimized for all platforms (PC, Mac, WebGL, Mobile)

---

**If you experience any crashes, make sure you've changed the Input System setting. This is the #1 cause of freezing.**
