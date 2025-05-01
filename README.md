# Bullet-Hell RL Agent

**Authors:** Moses Chen, Patrick Soong

## Overview
This project implements a 2D top-down bullet-hell dodging game in Unity, with an RL agent trained using Unity ML-Agents and the Proximal Policy Optimization (PPO) algorithm. The agent learns to survive incoming projectiles, collect a timed key objective, and maximize its survival time. Key features include customizable bullet spawners, a dodge-radius sensing system, a heart-based life system, and a scoring UI.

## Repository Structure
```
/Assets                ← Unity project files
/ProjectSettings       ← Unity project settings
/config                ← ML-Agents YAML configuration files
/.gitignore            ← Unity & ML-Agents ignore rules
/README.md             ← Project overview and setup instructions
```

## Getting Started
### Prerequisites
- Unity version **6000.0.44f1**
- Unity ML-Agents package compatible with Unity 6000.0.44f1
- Conda (Miniconda or Anaconda) installed
- Python 3.8+
- Git and Git LFS (for managing large assets)

### Setup
1. Clone this repository:
   ```bash
   git clone https://github.com/<YourUsername>/bullet.git
   cd bullet
   ```
2. Open the Unity project:
   - Launch Unity Hub and add the project folder.
   - Open the scene located in `Assets/Scenes`.
3. Create and activate a Conda environment for ML-Agents:
   ```bash
   conda create -n mlagents python=3.8
   conda activate mlagents
   ```
4. Install Python dependencies in your Conda environment:
   ```bash
   pip install mlagents
   ```

## Training the Agent
1. Configure training in `config/ppo_config.yaml` (adjust hyperparameters as desired).
2. Start training:
   ```bash
   mlagents-learn config/ppo_config.yaml --run-id=run1
   ```
3. Press the Play button in Unity to begin training episodes.
4. Monitor progress with TensorBoard:
   ```bash
   tensorboard --logdir results
   ```
