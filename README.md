# SnapClicker
![GitHub Release](https://img.shields.io/github/v/release/p6laris/SnapClicker)
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/p6laris/SnapClicker/.github%2Fworkflows%2Frelease.yml?branch=release)

<img width="400" alt="SnapClickerAppScreenshot" src="https://github.com/p6laris/SnapClicker/blob/main/Assets/SnapClickerApp.png">

**SnapClicker** is a versatile automation tool that lets you record and replay mouse clicks and keyboard inputs with pinpoint accuracy. 
Whether you're automating repetitive tasks, building complex input sequences, or crafting custom workflows — SnapClicker makes it easy. 
With a modern, user-friendly interface, you can save recordings as presets, manage them locally, and run them anytime with just a few clicks.

## ✨ Features
1. **Fast and Lightweight**
Utilizes the efficient `SendInput` API for simulating input — significantly faster and more reliable than older methods like `keybd_event` or `mouse_event`.
2. **Smart Recording**
    * **Full Recording**: Record any sequence of mouse and keyboard inputs for playback.
    * **Manual Recording**: Capture an individual action with precise timestamps—ideal for games and long-duration automation.
3. **PreciseDelays Mode**
   Enable high-precision timing for minimal delays between actions (microsecond-level control).
   
   > ⚠️This mode significantly increases CPU usage and system load. Only enable when absolutely necessary (e.g., competitive gaming, bypassing strict anti-bot checks).
   > For most tasks, standard delays are sufficient and far more efficient.

4. **Preset System**
Record your actions and save them as named presets. Easily organize and replay them whenever you need.

5. **Modern UI**
Sleek, intuitive interface that makes creating, managing, and running automations effortless.

6. **Wide Input Support**
Supports a variety of mouse actions — including **left click**, **right click**, **middle click**, and **mouse movement**. For keyboards, it handles **nearly all keys**, allowing you to build robust, flexible workflows.

7. **Customizable Hotkeys**
Start and stop recording or playback using keyboard shortcuts. You can personalize these hotkeys to fit your workflow and preferences.

8. **Adjustable Action Intervals**
Customize the delay between each recorded action to control the speed and rhythm of playback. This feature is especially useful for bypassing anti-auto-clicking detection in certain applications or games.

## 💿 Installation
### 1. Download from GitHub Releases
Go to the Latest [Release page](https://github.com/p6laris/SnapClicker/releases).
Download the setup file:
* SnapClicker-Setup.exe (installer)
* or SnapClicker-Portable.zip (no installation required)

Run the installer or extract the portable ZIP.

## 📖 Usage Manual

#### Standard Automation
For everyday tasks like data entry or repetitive clicking:
1. Keep `PreciseDelay` disabled
2. Set action interval to `0` for maximum speed
3. Record full automation sequences when possible

#### Precision Automation
For applications requiring exact timing:
1. Switch to manual recording mode
2. Set up individual actions
3. Configure custom intervals between actions
4. For `PreciseDelay` mode:
   - Enable only when absolutely necessary
   - Use minimum 5ms intervals
   - Monitor system resources
5. Test sequences in controlled environment first

### Recommended Settings
| Use Case          | Mode          | Interval | Notes                  |
|-------------------|---------------|----------|------------------------|
| General automation| Standard      | 0-1ms   | Balanced performance   |
| Critical timing  | PreciseDelay  | +5ms    | Short sessions only    |

## 🧪 Performance Tests

### Default Mode (Without PreciseDelays)
- **Max CPS (Clicks Per Second):** 228
- **Test Duration:** 1 second
- **Action Interval:** 0ms
  
<img width="400" alt="SnapClicker default performance test" src="https://github.com/p6laris/SnapClicker/blob/main/SnapClickerDefaultPerf.png">

### PreciseDelays
- **Max CPS (Clicks Per Second):** 3000
- **Test Duration:** 1 second
- **Action Interval:** 0ms
  
<img width="400" alt="SnapClicker default performance test" src="https://github.com/p6laris/SnapClicker/blob/main/SnapClickerPrecisePerf.png">

## ⚠️ Important Caution Notice
**Disclaimer of Liability:**
- This software is provided "as-is" without warranties
- We are **not responsible** for any:
  - System instability or hardware issues
  - Account bans from games/software
  - Data loss or unintended actions
  - Consequences of improper usage
- Using `PreciseDelay` mode or extreme settings may:
  - Trigger anti-cheat systems
  - Cause high CPU temperatures
  - Lead to unpredictable behavior
    
## 📜 License
SnapClicker is released under the [**MIT License**](https://github.com/p6laris/SnapClicker/blob/main/LICENSE).
