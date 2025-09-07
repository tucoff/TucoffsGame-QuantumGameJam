# InsiDiamond: Quantum Echo Mission 💎🚀

Hello! This is my submission for the **Quantum Game Jam 2025**! The theme was **ECHO** and I used the fascinating concept of quantum echoes in diamond crystals with nitrogen-vacancy (NV) centers - a real quantum physics phenomenon that I've gamified into an epic particle battle adventure!

## 🎮 The Game Concept

You're a teenage nerd who uses a special laser to enter a diamond crystal. Your mission is to command a squadron of electrons (the "Quantum Starships") and manipulate them to collect information from NV centers. Using microwave pulses, you perform precision maneuvers like the "Pi Maneuver" (π/2 pulse) and the "Pi/2 Maneuver" (π pulse) to reverse particle chaos and generate a quantum echo!

### Real Physics Behind the Game 🔬

The game is based on the **Hahn Echo Pulse Sequence** used in quantum sensing with diamond NV centers:

1. **Optical Pumping**: Laser initialization puts all electron spins in the same state (ms=0)
2. **π/2 Pulse**: First microwave pulse creates quantum superposition - electrons are in two states simultaneously
3. **Free Evolution**: Electron spins start to dephase due to environmental magnetic fields
4. **π Pulse**: Second microwave pulse flips the spins, reversing the dephasing process
5. **Quantum Echo**: All spins realign simultaneously, creating a detectable signal burst

In the game, you're performing these pulse sequences on chaotic electrons to "convert" them and extract quantum information!

## 🎯 Gameplay Mechanics

- **3D Survival Adventure**: Navigate through a microscopic diamond crystal world
- **Quantum Beam Combat**: Use laser beams to perform quantum manipulation on enemy electrons
- **Progressive Difficulty**: Spawn rates and enemy speeds increase every 30 seconds
- **Dark Mode Crisis**: After 2 minutes, lights shut off and chaos intensifies dramatically
- **Conversion System**: Hit enemies enter a 3-second quantum superposition before joining your side
- **Survival Scoring**: Score based on survival time with persistent high score tracking

## 🛠️ Installation & Setup

### Prerequisites
- Unity 2022.3 LTS or newer
- Git (for cloning)
- Optional: Python 3.8+ (for quantum simulation integration)

### Installation Steps

1. **Clone the Repository**
```bash
git clone https://github.com/tucoff/TucoffsGame-QuantumGameJam.git
cd TucoffsGame-QuantumGameJam
```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Open" and select the project folder
   - Unity will automatically import all assets and dependencies

3. **Configure Input System**
   - The game uses Unity's new Input System
   - Input actions are configured in `Assets/InputSystem_Actions.inputactions`
   - Controls: Mouse to aim/rotate camera, Left Click to shoot, ESC to toggle cursor lock

4. **Build the Game**
   - File → Build Settings
   - Add scenes: `Menu.unity` (Scene 0) and `Main.unity` (Scene 1)
   - Select your target platform and build

## 🏗️ Code Architecture

The game follows a modular architecture with clear separation of concerns:

### Core Game Systems

#### 1. **GameManager.cs** - Central Game State Controller
```csharp
public class GameManager : MonoBehaviour
{
    // Singleton pattern for global access
    public static GameManager Instance { get; private set; }
    
    // Handles:
    // - Game state management (start, game over, restart)
    // - Score calculation and high score persistence
    // - Scene transitions
    // - Time-based score multiplier system
}
```

**Key Features:**
- Singleton pattern ensures single instance across scenes
- Real-time score calculation based on survival time
- PlayerPrefs integration for persistent high scores
- Automatic scene loading on game over

#### 2. **PlayerFollowController.cs** - Player Input & Camera System
```csharp
public class PlayerFollowController : MonoBehaviour
{
    // Features:
    // - Third-person camera with smooth following
    // - Mouse-based camera rotation (horizontal only for simplicity)
    // - Quantum beam shooting with cooldown system
    // - Input System integration
}
```

**Advanced Features:**
- Smooth camera interpolation using `Vector3.Lerp()`
- Dynamic cursor lock/unlock with ESC key
- Shooting cooldown reset on successful enemy hits (rewards accuracy)
- Configurable camera distance, height, and sensitivity

#### 3. **EnemySpawn.cs** - Dynamic Enemy Management System
```csharp
public class EnemySpawn : MonoBehaviour
{
    // Sophisticated spawning system with:
    // - Progressive difficulty scaling
    // - Dark mode intensity mechanics
    // - Circular spawn pattern around player
    // - Dynamic enemy speed adjustment
}
```

**Spawning Algorithm:**
- **Circular Distribution**: Enemies spawn in a circle around the player using polar coordinates
- **Progressive Scaling**: Every 30 seconds, spawn interval decreases by 20% (minimum 0.5s)
- **Speed Ramping**: Enemy movement speed increases every 20 seconds
- **Dark Mode Escalation**: When lights go out, spawn distance reduces to 85% and max enemies increase every 10 seconds

#### 4. **EnemyBeamRotation.cs** - Quantum State Simulation
```csharp
public class EnemyBeamRotation : MonoBehaviour
{
    // Three-phase enemy lifecycle:
    // 1. Alive: Normal movement toward player
    // 2. Defeated: 3-second rotation (quantum superposition simulation)
    // 3. Converted: Complete stop, tag changes to "Player"
}
```

**Quantum Physics Simulation:**
- **Phase 1 (Alive)**: Enemies follow the player, representing chaotic electron movement
- **Phase 2 (Hit by Beam)**: 3-second rotation around a random axis - simulates quantum superposition
- **Phase 3 (Converted)**: All movement stops, enemy becomes ally - represents successful echo

#### 5. **Beam.cs** - Quantum Pulse Implementation
```csharp
public class Beam : MonoBehaviour
{
    // Simulates microwave pulse interaction:
    // - Collision detection with enemy electrons
    // - Upward force application (levitation effect)
    // - Quantum state transition triggering
}
```

**Physics Integration:**
- Disables gravity on hit enemies (quantum levitation effect)
- Applies upward impulse force for visual feedback
- Immunity system prevents multiple hits on same enemy
- Automatic cooldown reset on successful hits

#### 6. **LightingManager.cs** - Dramatic Environmental Control
```csharp
public class LightingManager : MonoBehaviour
{
    // Advanced lighting system featuring:
    // - Smooth light transitions
    // - Dark mode with survival lighting
    // - Atmospheric effects and fog
    // - Enhanced beam visibility in darkness
}
```

**Environmental Features:**
- **Normal Mode**: Full lighting with standard visibility
- **Dark Mode Transition**: 2-minute timer triggers dramatic darkness
- **Survival Lighting**: Minimal ambient light for navigation
- **Enhanced Visuals**: Increased emissive materials and particle effects in darkness

### Scene Management

#### Menu System (`Menu.unity`)
- **Story Integration**: Contains the full quantum physics explanation text
- **UI Navigation**: Clean menu interface with play/quit options
- **Educational Content**: Detailed explanation of Hahn echo sequences

#### Main Game (`Main.unity`)
- **3D Environment**: Diamond crystal-inspired world design
- **Player Setup**: Sphere player with camera rig
- **Enemy Prefabs**: Dynamic enemy spawning system
- **Lighting Rig**: Comprehensive lighting setup for day/night transitions

### Data Persistence
- **High Scores**: Stored in `PlayerPrefs` for cross-session persistence
- **Game State**: Temporary state management during gameplay
- **Settings**: Input mappings and configuration stored in Unity asset files

## 🐍 Python Integration Guide

Want to integrate real quantum simulations? Here's how to connect Python quantum libraries:

### Option 1: Unity-Python Communication via TCP

#### Python Quantum Server (quantum_server.py)
```python
import socket
import json
import numpy as np
from qiskit import QuantumCircuit, Aer, execute
from qiskit.visualization import plot_histogram

class QuantumEchoSimulator:
    def __init__(self):
        self.backend = Aer.get_backend('qasm_simulator')
    
    def simulate_hahn_echo(self, dephasing_time=1.0):
        # Create quantum circuit for Hahn echo
        qc = QuantumCircuit(1, 1)
        
        # Initial state |0⟩
        qc.reset(0)
        
        # π/2 pulse (Hadamard gate)
        qc.h(0)
        
        # Free evolution (phase accumulation)
        qc.rz(dephasing_time, 0)
        
        # π pulse (X gate - flip)
        qc.x(0)
        
        # Second free evolution
        qc.rz(dephasing_time, 0)
        
        # Measurement
        qc.measure(0, 0)
        
        # Execute
        job = execute(qc, self.backend, shots=1024)
        result = job.result()
        counts = result.get_counts(qc)
        
        # Calculate echo strength
        echo_strength = counts.get('0', 0) / 1024
        return echo_strength

def start_quantum_server():
    simulator = QuantumEchoSimulator()
    
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind(('localhost', 8888))
        s.listen()
        print("Quantum server listening on port 8888...")
        
        while True:
            conn, addr = s.accept()
            with conn:
                data = conn.recv(1024)
                request = json.loads(data.decode())
                
                if request['action'] == 'simulate_echo':
                    echo_strength = simulator.simulate_hahn_echo(
                        request.get('dephasing_time', 1.0)
                    )
                    
                    response = {
                        'echo_strength': echo_strength,
                        'success': echo_strength > 0.7  # Threshold for successful echo
                    }
                    
                    conn.sendall(json.dumps(response).encode())

if __name__ == "__main__":
    start_quantum_server()
```

#### Unity Integration Script (QuantumBridge.cs)
```csharp
using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

public class QuantumBridge : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    
    [System.Serializable]
    public class QuantumRequest
    {
        public string action;
        public float dephasing_time;
    }
    
    [System.Serializable]
    public class QuantumResponse
    {
        public float echo_strength;
        public bool success;
    }
    
    void Start()
    {
        ConnectToQuantumServer();
    }
    
    void ConnectToQuantumServer()
    {
        try
        {
            client = new TcpClient("localhost", 8888);
            stream = client.GetStream();
            Debug.Log("Connected to Quantum Server!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to Quantum Server: {e.Message}");
        }
    }
    
    public void SimulateQuantumEcho(float dephasingTime, Action<bool> callback)
    {
        if (stream == null) return;
        
        var request = new QuantumRequest
        {
            action = "simulate_echo",
            dephasing_time = dephasingTime
        };
        
        string json = JsonConvert.SerializeObject(request);
        byte[] data = Encoding.UTF8.GetBytes(json);
        
        stream.Write(data, 0, data.Length);
        
        // Read response
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        
        var result = JsonConvert.DeserializeObject<QuantumResponse>(response);
        callback?.Invoke(result.success);
    }
}
```

### Option 2: Direct Python Integration with Unity ML-Agents

For more advanced integration, use Unity ML-Agents:

1. Install ML-Agents in Unity
2. Create Python training environment
3. Use reinforcement learning to optimize quantum pulse sequences
4. Train AI agents to perform optimal echo sequences

### Option 3: Quantum Hardware Integration

For real quantum hardware (IBM Quantum, Rigetti, etc.):

```python
# IBM Quantum integration example
from qiskit import IBMQ, QuantumCircuit, transpile
from qiskit.providers.ibmq import least_busy

# Load IBM Quantum account
IBMQ.load_account()
provider = IBMQ.get_provider(hub='ibm-q')

# Get least busy device
backend = least_busy(provider.backends(simulator=False))

def run_on_real_quantum_computer(circuit):
    transpiled_circuit = transpile(circuit, backend)
    job = backend.run(transpiled_circuit)
    return job.result()
```

## 📁 Project Structure

```
TucoffsGame-QuantumGameJam/
├── Assets/
│   ├── Scenes/
│   │   ├── Menu.unity          # Main menu with story
│   │   └── Main.unity          # Game scene
│   ├── Scripts/
│   │   ├── GameManager.cs      # Core game state
│   │   ├── PlayerFollowController.cs  # Player controls
│   │   ├── EnemySpawn.cs       # Enemy management
│   │   ├── EnemyBeamRotation.cs # Quantum state simulation
│   │   ├── Beam.cs             # Quantum pulse system
│   │   ├── LightingManager.cs  # Environmental control
│   │   └── MenuRenderingDisabler.cs # Performance optimization
│   ├── Prefabs/               # Game object templates
│   ├── Materials/             # Visual materials
│   ├── Models/                # 3D models
│   ├── Sounds/                # Audio assets
│   └── InputSystem_Actions.inputactions # Input configuration
├── ProjectSettings/           # Unity project configuration
├── Packages/                  # Unity packages
└── README.md                 # This file
```

## 🎨 Visual Design

The game features a minimalist, scientific aesthetic:
- **Clean Geometry**: Simple shapes representing quantum states
- **Dynamic Lighting**: Dramatic lighting transitions for atmosphere
- **Particle Effects**: Enhanced visual feedback for quantum interactions
- **Color Coding**: Different materials for different quantum states

## 🚀 Performance Optimizations

- **Object Pooling**: Enemies are recycled to reduce garbage collection
- **LOD System**: Distant objects use simplified rendering
- **Culling**: Only visible objects are rendered
- **Efficient Collisions**: Optimized trigger volumes for beam interactions

## 🔧 Customization & Modding

Want to modify the game? Key parameters to adjust:

### Difficulty Scaling
```csharp
// In EnemySpawn.cs
[SerializeField] private float speedMultiplier = 0.8f;    // Spawn rate increase
[SerializeField] private float minimumSpawnInterval = 0.5f; // Fastest spawn rate
[SerializeField] private float moveSpeedIncreaseAmount = 0.5f; // Speed progression
```

### Quantum Physics Parameters
```csharp
// In EnemyBeamRotation.cs
[SerializeField] private float defeatWaitTime = 3f; // Superposition duration
[SerializeField] private float moveSpeed = 5f;     // Base enemy speed
```

### Visual Effects
```csharp
// In LightingManager.cs
[SerializeField] private float lightSwitchTime = 120f;    // When darkness begins
[SerializeField] private float transitionDuration = 1.5f; // Fade duration
```

## 🏆 Future Enhancements

Potential additions for expanded versions:

1. **Multiple Quantum States**: Different enemy types representing different electron spin states
2. **Complex Pulse Sequences**: More sophisticated quantum operations (CPMG, XY-8)
3. **Multiplayer Quantum Entanglement**: Cooperative quantum state manipulation
4. **Real Quantum Hardware**: Direct integration with quantum computers
5. **Educational Mode**: Interactive quantum physics tutorials
6. **VR Support**: Immersive quantum world exploration

## 🤝 Contributing

Want to contribute to the quantum gaming revolution? Here's how:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/quantum-enhancement`)
3. Commit your changes (`git commit -am 'Add quantum tunneling mechanics'`)
4. Push to the branch (`git push origin feature/quantum-enhancement`)
5. Create a Pull Request

## 📖 Learn More

### Quantum Physics Resources
- [Quantum Computing: An Applied Approach](https://link.springer.com/book/10.1007/978-3-030-50433-5)
- [Diamond NV Centers Research Papers](https://journals.aps.org/search)
- [Qiskit Textbook](https://qiskit.org/textbook/)

### Unity Game Development
- [Unity Learn Platform](https://learn.unity.com/)
- [Unity Input System Documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/index.html)
- [Unity Physics Best Practices](https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity7.html)

## 🙏 Acknowledgments

- **Quantum Game Jam 2025** organizers for the inspiration
- **Diamond NV Center Research Community** for the fascinating physics
- **Unity Technologies** for the amazing game engine
- **Qiskit Community** for quantum computing resources

## 📄 License

This project is open source and available under the MIT License.

---

**Ready to enter the quantum realm? The diamond crystal awaits your command! 💎⚛️🚀**

*May your echoes be strong and your qubits be stable!*
