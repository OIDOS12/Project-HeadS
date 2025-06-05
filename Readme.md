# ⚽ 2D Football Arcade Multiplayer Game (Unity + Steam)

This project is a 2D arcade-style football game built with Unity, supporting both local and online multiplayer via Steamworks. The game implements basic mechanics such as player movement, jumping, ball kicking, and goal detection, with server-authoritative network logic using FizzySteamworks transport.

---

## 🎮 Features

- ✅ **2D football gameplay** with dynamic physics  
- ✅ **Local mode**: two players on the same keyboard  
- ✅ **Online mode**: peer-to-peer multiplayer via Steam Lobby  
- ✅ **Server-authoritative architecture**  
- ✅ **Lobby system** with ready state and player list  
- ✅ **Goal detection & score tracking**  
- ✅ **Win condition**: first to 10 goals  
- ✅ **Responsive UI**: main menu, pause menu, in-game HUD  
- ✅ **Settings menu**: resolution, fullscreen, sound volume  
- ✅ **Persistent preferences** using `PlayerPrefs`  

---

## 🔧 Technologies Used

- **Unity 6000.0.33f1 LTS**
- **Steamworks.NET**
- **FizzySteamworks** (for P2P transport)
- **Mirror Networking**
- **NUnit + Unity Test Framework**

---

## 📂 Project Structure (Scenes)

- `MainMenu.unity` – Main entry screen  
- `SettingsMenu.unity` – Resolution and audio settings  
- `HostOrJoinScene.unity` – Online mode selection (Host/Join)  
- `Lobby.unity` – Pre-game lobby with player list  
- `GameScene.unity` – Online multiplayer game scene  
- `LocalScene.unity` – Offline/local match scene  

---

## ▶️ Getting Started

### 1. Requirements
- Unity 6000.0.33f1 LTS or later  
- Steam client running  

### 2. Installation
```bash
git clone https://github.com/OIDOS12/Project-HeadS.git
