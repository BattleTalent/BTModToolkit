# BattleTalent Mod Toolkit - Project Prompt

You are working on the **BattleTalent Mod Toolkit**, the official Unity-based modding framework for the BattleTalent VR game developed by CrossLink. This toolkit empowers creators to build custom content including weapons, spells, items, scenes, characters, and game mechanics.

## Project Context

**Game**: BattleTalent - A physics-based VR combat game
**Platform**: PC VR (Steam) and Meta Quest (Android)
**Engine**: Unity 2020.3.48f1 LTS
**Company**: CrossLink (CyDream)
**Repository**: BTModToolkit by BattleTalent organization

## Technical Stack

### Core Technologies
- **Unity 2020.3.48f1** - Game engine with URP rendering
- **C# .NET 4.x** - Primary scripting language
- **XLua** - Lua scripting integration for mod logic
- **Unity Addressables** - Asset management and loading system
- **Mirror Networking** - Multiplayer synchronization
- **Universal Render Pipeline (URP)** - Rendering pipeline

### VR Integration
- OpenXR/XR Toolkit support
- Hand tracking and gesture recognition
- Physics-based interactions
- Haptic feedback systems

## Project Architecture

### Directory Structure
```
Assets/
├── Build/              # Mod examples and templates
│   ├── Common/         # Shared utilities and scripts
│   ├── Weapons/        # Weapon mods (swords, bows, guns, etc.)
│   ├── Spells/         # Magic spells and effects
│   ├── Scenes/         # Custom game environments
│   └── Characters/     # Avatar and character mods
├── Toolkit/            # Core modding framework
│   ├── ModImporter/    # Runtime mod loading system
│   ├── AvatarBuilder/  # Character creation tools
│   ├── HandPoseHelper/ # VR hand gesture tools
│   └── TemplateWizard/ # Mod creation wizard
├── Editor/             # Unity editor extensions
└── Mirror/             # Networking framework
```

### Key Systems

1. **Mod Loading System**
   - Addressable-based asset bundles
   - Runtime mod discovery and loading
   - Hot-swapping capabilities
   - Cross-platform compatibility

2. **Lua Scripting Framework**
   - XLua integration with Unity
   - Game API exposure through CL namespace
   - Event-driven architecture
   - Component lifecycle management

3. **VR Interaction System**
   - Physics-based weapon handling
   - Hand pose definitions
   - Haptic feedback integration
   - Spatial audio support

4. **Networking Architecture**
   - Mirror-based multiplayer
   - Client-server synchronization
   - Cross-platform play support
   - Mod compatibility validation

## Development Workflows

### Creating New Mods
1. Use TemplateWizard for mod scaffolding
2. Define assets and dependencies
3. Implement Lua behavior scripts
4. Configure addressable assets
5. Test in editor with ModImporter
6. Build for target platforms

### Lua Scripting Patterns
```lua
-- Standard mod structure
local ModName = {
    -- Component properties
}

function ModName:Start()
    -- Initialization logic
    -- Access game APIs via CL.*
end

function ModName:OnDestroy()
    -- Cleanup resources
    -- Restore game state
end

return Class(nil, nil, ModName)
```

### C# Development
- Use CrossLink namespace for framework code
- Implement Unity component patterns
- Handle addressable asset operations
- Support both editor and runtime contexts

## Mod Categories

### Weapons
- **Melee**: Swords, daggers, maces, spears
- **Ranged**: Bows, crossbows, firearms
- **Magic**: Wands, staves, crystal weapons
- Features: Physics interactions, hand poses, visual effects

### Spells
- **Elemental**: Fire, ice, thunder, wind
- **Utility**: Telekinesis, teleportation, shields
- **Combat**: Direct damage, area effects, debuffs
- Features: Particle systems, sound effects, gameplay mechanics

### Environments
- **Arenas**: Combat areas with unique mechanics
- **Game Modes**: Custom rules and objectives
- **Interactive Objects**: Destructible elements, triggers
- Features: Lighting, audio, physics interactions

### Characters
- **Avatars**: Player character customization
- **NPCs**: AI-controlled entities
- **Skins**: Visual variations and themes
- Features: Animation, rigging, equipment compatibility

## Technical Considerations

### Performance Optimization
- Efficient asset loading and unloading
- LOD systems for complex models
- Texture streaming for memory management
- Physics optimization for VR

### Cross-Platform Support
- Asset bundle compatibility
- Platform-specific optimizations
- Input system abstraction
- Performance scaling

### Quality Assurance
- Automated build validation
- Runtime error handling
- Mod compatibility testing
- Version control integration

## API Integration

### Game APIs (CL Namespace)
- **CL.ModManager**: Mod lifecycle management
- **CL.SettingMgr**: Game settings access
- **CL.UnlockContentConfig**: Feature unlocking
- **CL.DynamicModifierConfig**: Gameplay modifiers

### Unity Systems
- **Addressables**: Asset management
- **XR Toolkit**: VR interactions
- **Physics**: Collision and rigidbody systems
- **Audio**: 3D spatial sound

## Common Development Tasks

1. **Creating Weapon Mods**
   - Model and texture assets
   - Physics collision setup
   - Hand pose definitions
   - Lua behavior implementation

2. **Implementing Spells**
   - Particle effect systems
   - Target selection mechanics
   - Damage calculation
   - Visual feedback

3. **Building Scenes**
   - Environment art creation
   - Lighting and atmosphere
   - Interactive elements
   - Multiplayer considerations

4. **Character Development**
   - Rigging and animation
   - Equipment attachment points
   - Customization options
   - Performance optimization

## Best Practices

### Code Quality
- Follow Unity naming conventions
- Implement proper error handling
- Use async/await for addressable operations
- Document public APIs thoroughly

### Asset Management
- Optimize texture sizes and formats
- Use appropriate compression settings
- Implement proper LOD systems
- Consider mobile platform limitations

### User Experience
- Provide clear mod descriptions
- Include preview images/videos
- Test on target VR hardware
- Ensure accessibility compliance

### Community Standards
- Follow modding guidelines
- Respect intellectual property
- Maintain compatibility with base game
- Support community feedback

This toolkit represents a comprehensive modding solution for VR game development, enabling creators to extend BattleTalent with custom content while maintaining performance and compatibility standards.
