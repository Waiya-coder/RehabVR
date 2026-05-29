# VR Rowing — Upper Body Rehabilitation Game

A Meta Quest VR experience that gamifies upper body physical therapy exercises. 
The player sits in a rowing boat, grabs two oars, and rows toward a distant 
glowing lamppost — each stroke replicating the Standing Row exercise used in 
shoulder rehabilitation.

## Demo : apk + video

APK FILE :  https://drive.google.com/file/d/1XJPJA2J8wDOoMMYVyEROFDKAuogonLN7/view?usp=drive_link


 DEMO  VIDEO : https://drive.google.com/file/d/1tXvTg3DP1clS340stQ9LVrIhBlmZUW1r/view?usp=sharing


## What it does

- Full VR rowing interaction using Meta Quest hand tracking and grab mechanics
- Physics-based boat that responds to oar stroke velocity and direction
- Tutorial system that teaches the player how to grab and pull correctly 
  before the session begins
- Real-time form feedback , detects uneven strokes and flags them as 
  "Uneven" or "Too fast" so the player corrects their technique
- Glowing lamppost goal that brightens as you approach — physical arrival 
  triggers a celebration
- Spatial audio — water splash on each stroke, ambient boat sound fades in 
  with speed

## Motivation

I wanted to explore how VR could make repetitive therapeutic exercises more 
engaging. Research shows up to 70% of patients don't complete physiotherapy 
programs , largely due to boredom and low motivation. By wrapping the standing 
row exercise inside a rowing game, the movement becomes purposeful: you're not 
just pulling your elbow back, you're propelling a boat toward a light.

## Potential impact

For patients recovering from shoulder injuries or surgery, adherence to home 
exercise is critical but notoriously difficult to maintain. A game-based system 
removes the clinical feeling of repetition and replaces it with intrinsic 
motivation , you want to reach the light. Real-time form feedback means the 
patient isn't just completing reps, they're completing them correctly.

## Tech stack

- Unity 6000.3.12f(URP)
- Meta XR SDK / Oculus Interaction Toolkit
- C# scripting
- Meta Quest 3

## What I learned

I had used Unity before but never built a physics-based VR interaction system 
from scratch. Key new things I learned:

- How grab interactables work in the Meta XR SDK (Grabbable, 
  OneGrabRotateTransformer, HandGrabInteractable)
- Translating real physical movement (oar rotation) into meaningful physics 
  forces on a Rigidbody without causing instability
- Designing VR UI that doesn't cause motion sickness :  world-space canvases, 
  smooth force application, angular velocity clamping
- How to structure a tutorial as a state machine in code

## Challenges

The hardest problem was making the physics feel right. Too much force and the 
boat launched off the water. Too little and nothing moved. 
Motion sickness was another challenge. VR comfort requires smooth, predictable 
motion. I added force smoothing, angular velocity caps, and a steering dead zone 
so that rowing both oars evenly never caused accidental rotation.

## Running locally

1. Install Unity6000.3.12f LTS with Android Build Support
2. Install Meta XR SDK via Package Manager
3. Clone this repo and open in Unity Hub
4. Build Settings → Android → Meta Quest target
5. Build and run, or sideload the APK directly

## Scripts overview

| Script | Purpose |
|--------|---------|
| `BoatController.cs` | Physics — force, drag, steering, audio |
| `Oar.cs` | Stroke velocity detection, splash audio |
| `TutorialManager.cs` | Step-by-step onboarding state machine |
| `GoalManager.cs` | Rep tracking, lamp intensity, session state |
| `GoalTrigger.cs` | Trigger collider fires goal completion |
| `FormTracker.cs` | Per-rep stroke quality rating |
| `GoalCelebration.cs` | Arrival celebration — particles, light pulse, audio |

## Assets used

- Boat model: [[Old Wooden Rowboat — Unity Asset Store]](https://assetstore.unity.com/packages/3d/vehicles/sea/old-rowboat-31917)
- Lamppost model: [credit asset source]
- Water splash sound: freesound.org (freesounds123-small-water-splash-374843)
- Boat ambient sound: freesound.org (freesound_community-thai-motor-boat-64851)
- Victory sound: freesound.org (eaglaxle-gaming-victory-464016)
- Rain VFX: https://assetstore.unity.com/packages/vfx/particles/environment/rain-system-vfx-2d-3d-325936
