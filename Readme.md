
[TOC]

# Welcome to the Scripting Document

Hello, this is the scripting document, you can query these API, then use them in lua.



### Quick Start to build the mods from this project

[https://battletalent.github.io/community-docs/docs/tutorials/modtoolkit-overview](https://battletalent.github.io/community-docs/docs/getting-started/modtoolkit-overview)

if you just want to try play this mod, you can download it from [HERE](https://github.com/fonzieyang/BTModToolkit/releases/download/V0.0.1/ModProj.zip)


### Commonly used classes


#### Basic Classes

* [Scheduler](class_cross_link_1_1_scheduler.html)
* [LanguageMgr](class_cross_link_1_1_language_mgr.html)
* [EffectMgr](class_cross_link_1_1_language_mgr.html)
* [AudioMgr](class_cross_link_1_1_audio_mgr.html)
* [ResourceMgr](class_cross_link_1_1_resource_mgr.html)
* [GameDataMgr](class_cross_link_1_1_game_data_mgr.html)
* [XLuaMgr](class_cross_link_1_1_x_lua_mgr.html)


#### GamePlay Related Classes

* [HitInfoConfig](class_cross_link_1_1_hit_info_config.html)
* [UnlockContentConfig](class_cross_link_1_1_unlock_content_config.html)
* [AIProxy](class_cross_link_1_1_a_i_proxy.html)
* [CharacterCombatAttr](class_cross_link_1_1_character_combat_attr.html)
* [FullBodyMuscleState](class_cross_link_1_1_full_body_muscle_state.html)
* [BuffMgr](class_cross_link_1_1_buffer_mgr.html)
* [InteractBase](class_cross_link_1_1_interact_base.html)
* [HUDMgr](class_cross_link_1_1_hud_mgr.html)
* [PlayerDataMgr](class_cross_link_1_1_player_data_mgr.html)
* [LevelDataMgr](class_cross_link_1_1_level_data_mgr.html)



#### Lua Host Classes


* [LuaBehaviour](class_cross_link_1_1_lua_behaviour.html)
* [InteractTriggerX](class_cross_link_1_1_interact_trigger_x.html)
* [FlyObjectX](class_cross_link_1_1_fly_object_x.html)

For lua host script, check the LuaFunction in each class, that's the event they will receive.

For example, InteractTriggerX has luaCloseSkill, then it'll recive CloseSkill Event


exceptional case:

LuaFunction name| actual event name
:---:|:---:
AwakeInit | Awake
StartInit | Start

For example, InteractTriggerX has luaAwakeInit, but it'll receive Awake Event instead of AwakeInit






#### Commonly used components

| Basic Components                   | Function                                                     | More Details                                                 |
| ---------------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------ |
| InteractWeapon                     | represent the entity of weapon                               |                                                              |
| InteractTrigger / InteractTriggerX | define how weapon works when player pressing trigger button  | https://battletalent.github.io/community-docs/docs/details/skill-system |
| StabObj                            | define how your weapon penetrate others                      |                                                              |
| RagdollHitInfoObj                  | define how to calculate damage                               | https://battletalent.github.io/community-docs/docs/details/hitinfo-and-collisioneffect |
| RagdollHitInfoRef                  | it'll define a group of RagdollHitInfoObj as a whole, so that they won't hit multiple times |                                                              |
| RigidbodyInit                      | define intertia tensor and center mass, if it's zero, then it'll not take effect on the rigidbody |                                                              |
| CollisionEffect                    | simulate physics collision effect                            | https://battletalent.github.io/community-docs/docs/details/hitinfo-and-collisioneffect |
| AttachLine / AttachPoint           | define how player grabs it                                   |                                                              |
| Mount Point                        | define how player put it in the pocket                       |                                                              |
| FlyObj / FlyObjX                   | attached on Fly Object, such as bullet, magic                | https://battletalent.github.io/community-docs/docs/details/flyobject |






#### Damage Pipeline

https://battletalent.github.io/community-docs/docs/details/damage-pipeline





#### Background Knowledge

| Term                  | Explaination                                                 |
| --------------------- | ------------------------------------------------------------ |
| addressables          | by pressing the addressable toggle box to build resource into addressable resource, then rename the path to make it get loaded correctly in game.  once you filled in correct addressable resources, then the game will load it on start |
| resources path        | Weapon Path: Weapon/<br>        Script Path: LuaScript/<br>        ICon Path:ICon/<br>        Effect Path:Effect/<br>        Audio Path: Audio/Sound/<br>        FlyObj Path: FlyObj/<br>        <br />    any resource's addressable name in the path above, can be loaded by the system without path included. for example, if an effect's addressable name is Effect/explosion, can be create by EffectMgr.Instance:PlayEffect("explosion") |
| entry point           | any lua script's addressable name is Entry, will become the entry point of this mod |
| components end with X | means it's scriptable, such as FlyObjectX, InteractTriggerX  |
| coordinate            | z is forward, x is right, y is up                            |




#### DebugTools

| Dev Environment                             | Pros                                                         | Cons                                                |
| ------------------------------------------- | ------------------------------------------------------------ | --------------------------------------------------- |
| edit in Editor + debug in Windows Simulator | 1. dosen't need a gaming PC<br> 2. iterate very fast, because you don't need to put on headset<br> 3. you can make use of shortcut to debug faster | 1. the feeling is different from in VR              |
| edit in Editor + debug in SteamVR           | 1. you can check the real size in headset<br> 2. you can make use of shortcut to debug faster | 1. need to install mod and put on headset everytime |
| edit in Editor + debug in Quest             | 1. you can check the real size in headset                    | 1. need to install mod and put on headset everytime |

#### Shortcut

| Shortcut        | Function at Body Mode | Function at Hand Mode    |
| -------------- | --------------------- | ------------------------ |
| WASD           | walk                  | walk                     |
| Shift          | run                   | hand rotation            |
| Alt            | switch to Hand Mode   | switch to Body Mode      |
| Tab            |                       | switch between your hand |
| Mouse Movement | body rotation         | hand movement            |
| Ctrl           |                       | hand movement on Z axis  |

| Shortcut  | Function                         |
| -------- | -------------------------------- |
| F12      | open or close MainMenu           |
| F11      | open or close CheatMenu          |
| F10      | reload mods(not working for now) |
| F9       | remove mods(not working for now) |
| F8       | jump to test scene               |
| F4       | hide mouse                       |
| PageDown | select next target               |
| U        | kill target                      |
| Y        | stun target                      |
| PadNum   | target attacks                   |
| Home     | enable target's AI               |
| End      | disable target's AI              |
| K        | kill all                         |
| Space    | jump                             |
| G        | jump(no cooldown)                |
| V        | dash(no cooldown)                |

#### Log Path

| Log     | Log Path                                                     |
| ------- | ------------------------------------------------------------ |
| Windows | https://docs.unity3d.com/Manual/LogFiles.html                |
| Quest   | read it on **in-game console** or using **adb log command** or using **sidequest's adb window** |


### Attributions

* Song used in the song mod: Spinning Monkeys - Kevin Macleod under license: https://creativecommons.org/licenses/by/3.0/
