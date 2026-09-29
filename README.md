<div align="center">

# 🎵 Music Background System for Unity

![License](https://img.shields.io/badge/License-MIT-yellow.svg)
![Unity](https://img.shields.io/badge/Unity-6.0%2B-black?logo=unity)
![Code](https://img.shields.io/badge/Code-C%23-blue.svg)

**A professional, highly optimized, and feature-rich Background Music (BGM) management system for Unity.**<br>
Built specifically with Unity 6 in mind, this package leverages modern C# features like `Awaitable` and `CancellationToken` for performant, coroutine-free audio transitions and synchronized lyrics display.

</div>

---

## ✨ Key Features

- **Modern C# Async/Await**: Highly optimized audio transitions using native `Awaitable` and `CancellationToken`—no outdated Coroutines.
- **Customizable Crossfading**: Seamlessly crossfade between tracks with custom `AnimationCurve`s for precise ease-in and ease-out transitions.
- **Synchronized Lyrics System**: Built-in TextMeshPro integration to display synchronized lyrics based on precise timestamps.
- **Romantic Typography Effects**: Smooth, customizable micro-animations (alpha fade and scale push-in) to bring your lyrics to life.
- **Scriptable Object Driven**: Easily create, manage, and configure audio tracks (`SongProfile`) and their lyric data without touching code.
- **Dynamic Triggers**: Drag-and-drop `BGMTrigger` components to dynamically change music using trigger colliders. Integrated with `UnityEvent` for easy hooks into particle effects or cutscenes.
- **Playlist Management**: Support for sequential and looping playlists natively out-of-the-box.

---

## 📁 Installation

> [!TIP]
> **The Fastest Way to Install** Simply drop the `Music Background System` folder directly into your Unity project's `Assets` folder.

---

## 🚀 Setup Guide

### 1. Core Setup (BGMManager)
1. Create a new empty GameObject in your initial scene and name it `BGMManager`.
2. Add the `BGMManager` component to this GameObject.
3. *(Optional)* If you want lyrics, create a **TextMeshProUGUI** object in your Canvas and assign it to the **Lyrics Text** field on the `BGMManager`.
4. Configure the Crossfade Duration, Curves, and Typography scaling in the inspector.
5. Add initial songs to the **Playlist** if you want background music to start immediately.

> [!NOTE]
> The `BGMManager` operates as a Singleton and is marked with `DontDestroyOnLoad`. It will seamlessly persist across scenes.

### 2. Creating a Song Profile
1. Right-click in your Project window.
2. Navigate to **Create > BGM System > Song Profile**.
3. Name your new profile and select it.
4. In the Inspector:
   - Assign your **AudioClip**.
   - Adjust the **Volume** slider for track balancing.
   - Under **Lyrics Configuration**, add any number of `LyricLine`s by specifying the timestamp (seconds), duration, and text.

### 3. Setting Up Triggers in the World
1. Select any GameObject in your scene (or create a new empty one) where you want a music change to happen.
2. Add a `Collider` component (e.g., `BoxCollider`) and check **Is Trigger**.
3. Add the `BGMTrigger` component.
4. Assign the **Song Profile** you created to the `Song To Trigger` field.
5. Set the **Trigger Tag** (e.g., `Player`) to ensure only the right object triggers the music change.
6. *(Optional)* Hook up visual effects or dialogue using the **On Song Triggered** `UnityEvent`.

---

## 📝 License
See the `LICENSE` file in the repository root for details.
