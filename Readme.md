
[TOC]

# Welcome to the Scripting Document

Hello, this is the scripting document, you can query these API, then use them in lua.
Looking forward to what everyone will create. We will open more systems in the future, currently we only export weapon related tools(in C#). 


#### Quick Start

1. install Unity 2019.4.12f(download [UnityHub](https://unity3d.com/get-unity/download), then download Engine via Hub)
2. download [ModProj](https://github.com/fonzieyang/BTModToolkit)
3. open the project, then click BuildTools/BuildAllBundles
4. copy the mod files to the corresponding path:
	* Windows: C:\Users\[username]\AppData\LocalLow\CrossLink\BattleTalent\Mods
	* Quest:      /sdcard/Android/data/com.CrossLink.SAO/files/Mods/
5. Done! (if something goes wrong, please open your cheat menu then tell us with your error messages)

if you just want to try play this mod, you can download it from [HERE](https://github.com/fonzieyang/BTModToolkit/releases/download/V0.0.1/ModProj.zip)

#### Make your first weapon with BTModToolkit in 10 mintues

please check out this [Weapon Modding Video](https://youtu.be/IqPl5KRgZ8Y)


#### Mod Host Community

We host most of our mods on [Mod.io](https://battletalent.mod.io/). There are so many resources out there.

We'll keep updating the free demo. Feel free to use Battle Talent as your VR coding playground and ask question on our [discord](https://discord.gg/f4dmSG9).


#### Before release your own mod

please make sure you've **cleared the project** and **renamed the product**

two ways to clear the example project:
* remove the pre-added addressables(window->asset management->addressables->group)
* delete the Examples folder(Assets/Examples)

the way to change product name:
* Edit -> Project Settings -> Modify the Project Name



#### Workflow

 <img src="workflow.png">



#### Tips for scripting in lua:

* namespace of CShape code is CS in lua
* namespace CrossLink is CL in lua, CL == CS.CrossLink
* namespace UnityEngine is UE in lua, UE == CS.UnityEngine
* calling a static function should use '.' instead of ':' in lua
* recommend using [ZeroBrane Studio](https://studio.zerobrane.com/) as lua editor
* use IsNullOrDestroyed for null checking

#### ZeroBrane IDE auto-complete


1. copy syntax.lua(ModProj/Assets/Resources/Tools/ZeroBraneAutoCompleteTool) to path ZeroBraneStudio/api/lua.
2. open ZeroBrane Studio and click Edit->Preferances->Settings User to open user.lua.
3. write "table.insert(api, 'syntax')" in user.lua.





#### Related Manual

* [Unity manual](https://docs.unity3d.com/2019.4/Documentation/Manual/)
* [Lua manual](https://www.lua.org/manual/5.1/)
* [Xlua](https://github.com/Tencent/xLua/blob/master/README_EN.md)
* [EasySave3](https://docs.moodkie.com/easy-save-3/es3-api/es3-class/)


#### TODO

* player modding
* level modding
* npc modding



---





# Basic Concept

Battle Talent is fully driven by physics, so every collider in this game, we can find the logic object they belonged to(except environment).

#### check what we got by a collider or rigidbody

	local pu = CL.PhysicsUnit.GetPhysicsUnit(collider)
	if (pu == nil || pu:IsScene()) then
		print("Env")
	else
		print(pu.unitType)
	end
	
	-- by default, we use Bottom Up Method, which means, we may get the sub object of another object
	-- if we want to get the root object, then we should use GetPhysicsUnitTopDown instead
	local puRB = CL.PhysicsUnit.GetPhysicsUnitTopDown(rigidbody.transform)
	if (puRB == nil || puRB:IsScene()) then
		print("Env")
	else
		print(puRB.unitType)
	end

#### here's the enum and the class

enum name| class name
:---|:---
FlyObject | [FlyObject](class_cross_link_1_1_fly_object.html)
SceneObject | [SceneObject](class_cross_link_1_1_scene_object.html)
InteractRole | [FullCharacterControl](class_cross_link_1_1_full_character_control.html)
InteractWeapon | [InteractWeapon](class_cross_link_1_1_interact_weapon.html)
PlayerRole | [PlayerUnit](class_cross_link_1_1_player_unit.html)
PlayerHand | [PhysicsHand](class_cross_link_1_1_physics_hand.html)


#### layer settings

check out [LayerDefine](class_cross_link_1_1_layer_define.html) to find all the layers we predefined

layer name| physics object associated
:---|:---
BodyMask | Npc&Player's Bodypart
EnvLayerMask | Environment
InteractLayerMask | Weapon, Item, Part of BodyPart

---

# Commonly used classes


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

| Basic Components                   | Function                                                     |
| ---------------------------------- | ------------------------------------------------------------ |
| InteractWeapon                     | represent the entity of weapon                               |
| InteractTrigger / InteractTriggerX | define how weapon works when player pressing trigger button  |
| StabObj                            | define how your weapon penetrate others                      |
| RagdollHitInfoObj                  | define how to calculate damage                               |
| RagdollHitInfoRef                  | it'll define a group of RagdollHitInfoObj as a whole, so that they won't hit multiple times |
| RagdollInit                        | define intertia tensor and center mass, if it's zero, then it'll not take effect on the rigidbody |
| CollisionEffect                    | simulate physics collision effect                            |
| AttachLine / AttachPoint           | define how player grabs it                                   |
| Mount Point                        | define how player put it in the pocket                       |
| FlyObj / FlyObjX                   | attached on Fly Object, such as bullet, magic                |




 
#### InteractTrigger component


Here shows: 
* What will happen when you toggled those paramters on InteractTrigger component.
* How's the mana cost system work.
* What's the callback function we'll call when we pull the trigger.

For more examples, please check out the Mod Toolkit


| manaCost                |                                                             |                            |                                                             |                                                             |                                                             |
| ----------------------- | ------------------------------------------------------------ | --------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------ | ------------------------------------------------------------ |
| instantSkill            |                                                              | √                           | √                                                            |                                                              |                                                              |
| skillChargeEndTime      |                                                              |                             | √                                                            | √                                                            | √                                                            |
| activateTime            |                                                              |                             |                                                              | √                                                            |                                                              |
|                         |                                                              |                             |                                                              |                                                              |                                                              |
| example                 | Continuous updated skill, such as telekinesis, sprint spear, slash katana | one shot skill, such as gun | can be shot and charged at the same time, such as storm pistol | can be charged into special state, such as thunder spear     | Charged for an one shot release, such as fireball spell      |
| Real mana in calcuation | manaCostOnCharge                                             | manaCost                    | manaCost     manaCostOnCharge                                | manaCostOnCharge                                             | manaCostOnCharge                                             |
| callback                | OpenSkill<br />UpdateSkill<br />CloseSkill                   | UpdateSkill                 | UpdateSkill    <br />OnChargeBegin<br />OnChargeReady<br />OnChargeUpdate<br />OnChargeRelease    <br />OnChargeCancel | OnActivateBegin  <br />OnActivateEnd  <br />OnActivateCancel | OnChargeBegin->OnChargeReady->OnChargeUpdate->OnChargeRelease |


#### CollisionEffect component

BuiltIn collision materials for weapon here, paste the name into CollisionEffect component, then the colliders under this will be identified as this material.

| Collision Material Name | Used For                     |
| ----------------------- | ---------------------------- |
| Weapon                  | Blade                        |
| WeaponBlunt             | Hammer, Metal, Handle        |
| Metal                   | Environment                  |
| Brick                   | Environment                  |
| Wood                    | Environment or Wooden Weapon |




#### HitInfo component

Builtin hit type, paste the name into RagdollHitInfoObj component, then the colliders under this will be identified as this hit type

Notice that you may need to use RagdollHitInfoRef component to bind multiple RagdollHitInfoObj as one hit identity, otherwise it will cause multiple hits at a time.



| Name            | VelocityMlp | DamageMlp | DamageThrough | DamageCrit | StabMlp | HitMlp | HitRandom | StabDamage | BreakDefenceMlp | HitBackMlp | KnockoutFactor | DizzyFactor | StiffValue |
| --------------- | ----------- | --------- | ------------- | ---------- | ------- | ------ | --------- | ---------- | --------------- | ---------- | -------------- | ----------- | ---------- |
| Sword           | 1.2         | 10        | 0.5           | 1.5        | 1.5     | 115    | 0.2       | 120        | 1               | 90         | 0.1            | 0           | 2.8        |
| LightSaber      | 1.2         | 35        | 0.5           | 1.5        | 1.5     | 120    | 0.2       | 120        | 1               | 90         | 0.1            | 0           | 2.8        |
| Wood            | 1.2         | 5         | 1.5           | 1.5        | 1       | 125    | 1         | 0          | 1.2             | 90         | 0.5            | 0.5         | 2.8        |
| Brick           | 1.2         | 5         | 1.5           | 1.5        | 1       | 130    | 1         | 0          | 1.2             | 90         | 0.5            | 0.5         | 2.8        |
| Metal           | 1.2         | 7         | 1.2           | 1.5        | 1       | 125    | 1         | 0          | 1.3             | 112.5      | 0.3            | 0.4         | 2.8        |
| Fist            | 1.2         | 4         | 1.2           | 1.5        | 1       | 125    | 1         | 0          | 1               | 90         | 0.3            | 0.4         | 2.8        |
| Hammer          | 1.2         | 6         | 1.5           | 1.5        | 1       | 130    | 0.1       | 0          | 2               | 112.5      | 0.4            | 0.4         | 3.6        |
| SingleHammer    | 1.2         | 8         | 2             | 1.5        | 1       | 125    | 0.1       | 0          | 2               | 112.5      | 0.45           | 0.45        | 3.6        |
| Axe             | 1.2         | 12        | 1             | 1.5        | 1.8     | 120    | 0.1       | 120        | 2               | 90         | 0.1            | 0           | 3.6        |
| Dagger          | 1.3         | 5         | 0.5           | 2          | 4.5     | 115    | 0.2       | 120        | 0.8             | 90         | 0.1            | 0           | 1.6        |
| Katana          | 1.2         | 12        | 0.1           | 1.3        | 1       | 115    | 0.2       | 120        | 1               | 90         | 0.1            | 0           | 1.6        |
| Rapier          | 1.2         | 8         | 0.2           | 1.5        | 3       | 115    | 0.1       | 120        | 1.5             | 90         | 0.1            | 0           | 2.8        |
| Shield          | 1.2         | 8         | 1.2           | 1.5        | 1       | 125    | 0.1       | 0          | 2               | 90         | 0.3            | 0.4         | 2.8        |
| Spear           | 1.3         | 7         | 0.8           | 1.8        | 2.5     | 120    | 0.1       | 120        | 1.4             | 112.5      | 0.1            | 0           | 2.8        |
| Stick           | 1.2         | 6         | 1             | 2          | 1       | 125    | 1         | 0          | 1.5             | 90         | 0.5            | 0.6         | 2.8        |
| SwordWind       | 1.2         | 12        | 0.5           | 1.5        | 1.1     | 115    | 0.1       | 120        | 1               | 90         | 0.1            | 0           | 2.8        |
| SwordWind_Slash | 1.2         | 20        | 0.5           | 1.5        | 1.1     | 115    | 0.5       | 0          | 1               | 112.5      | 0.5            | 0           | 2.8        |
| Wand            | 1.2         | 6         | 1             | 2          | 1       | 120    | 0.8       | 0          | 1.2             | 90         | 0.5            | 0.6         | 2.8        |
| Bullet          | 1.2         | 3         | 0.4           | 4.5        | 1       | 115    | 0.1       | 0          | 1               | 90         | 0.1            | 0           | 2.8        |
| Explode         | 1.2         | 40        | 1             | 1          | 1       | 115    | 0.1       | 0          | 1               | 400        | 1              | 0           | 2.8        |
| Arrow           | 1.2         | 6         | 0.5           | 6          | 2       | 120    | 0.5       | 120        | 1               | 90         | 0.1            | 0           | 2.8        |
| MagicBall       | 1.2         | 12        | 0.5           | 4          | 1       | 115    | 0.1       | 0          | 1               | 90         | 0.3            | 0           | 2.8        |
| Flame           | 1.2         | 5         | 0.6           | 2          | 1       | 120    | 0.1       | 0          | 1               | 112.5      | 0.3            | 0           | 2.8        |
| IceSword        | 1.2         | 20        | 0.5           | 1.5        | 1.5     | 115    | 0.2       | 120        | 1               | 90         | 0.1            | 0           | 2.8        |
| Laser           | 1.2         | 15        | 0.6           | 2          | 1.5     | 115    | 0.1       | 0          | 1               | 90         | 0.3            | 0           | 2.8        |
| FlySlash        | 1.2         | 15        | 0.5           | 1          | 1.3     | 115    | 0.1       | 0          | 1               | 90         | 0.3            | 0           | 2.8        |
| FlyString       | 1.2         | 15        | 0.5           | 1          | 1.3     | 115    | 0.1       | 0          | 1               | 90         | 0.3            | 0           | 2.8        |
| FlyThunder      | 1.2         | 3         | 0.5           | 1          | 1.3     | 115    | 0.1       | 0          | 1               | 140        | 0.1            | 0           | 2.8        |
| FlyOriFire      | 1.2         | 15        | 0.5           | 4          | 1       | 115    | 0.1       | 0          | 1               | 90         | 0.3            | 0           | 2.8        |
| TrackBall       | 2.2         | 10        | 1             | 1.5        | 1.3     | 115    | 0.1       | 0          | 1               | 90         | 0.6            | 0           | 2.8        |
| Spike           | 1.2         | 5         | 1             | 1.5        | 2       | 115    | 0.1       | 120        | 1               | 112.5      | 0.3            | 0           | 2.8        |
| SpikeSmall      | 1.2         | 3         | 1             | 1.5        | 2       | 115    | 0.1       | 120        | 1               | 112.5      | 0.3            | 0           | 2.8        |
| Drone           | 1.2         | 22.5      | 1             | 1          | 1       | 115    | 0.1       | 0          | 1               | 150        | 0.3            | 0           | 2.8        |
| DropObj         | 1.2         | 50        | 1             | 2          | 1       | 115    | 2         | 120        | 1               | 150        | 1              | 0           | 2.8        |
| KO              | 1.2         | 20        | 1             | 2          | 1       | 115    | 2         | 0          | 1               | 120        | 1              | 0           | 2.8        |






---

#### FlyObject component


##### Life Cycle

Once collision count meet with collisionCount or maxFlyTime is over, then flyobject will enter finish state

 <img src="FlyObjLifeCycle.png">




##### Fly Logic

In fly state, FlyObject can play trail and keep track of the trajectory

Once collision count meet with collisionFlyCount, then flyobject will enter flystop state

 <img src="FlyState.png">



##### Collision Ignore Settings

You can setup the flyobject ignore specific type of objects.

ignoreDamageList: you can ignore collision with specific type objs 

ignoreHolder: you can also ignore the role spawn this flyobj



##### Collision Callback Functions:

Note: collision event will override the original functions in lua, so you need to call the OnCollisionUpdate manually to maintain the life cycle, please check out the FlySpellBaseScript.txt for example.

|            | Collision                 |                          |
| ---------- | ------------------------- | ------------------------ |
| PlayerHand | OnCollisionWithPlayerHand | OnTriggerWithPlayerHand  |
| Player     | OnCollisionWithPlayer     | OnTriggerWithPlayer      |
| HitScan    | OnCollisionWithHitScan    |                          |
| Scene      | OnCollisionWithScene      | OnTriggerWithStaticScene |
| Role       | OnCollisionWithRole       | OnTriggerWithRole        |
| default    | OnCollision               | OnTrigger                |


---




#### Damage Pipeline

from now on, we can use modifier to modify each hit our weapon caused.

here's the damage pipeline, and we'll inject our code to modify the data though to pipeline to get the result we want

 <img src="DamagePipeline.png">


##### How we register the phase event

for local event, such as for specific hit, you can get the RagdollHitInfo structure, then register the functions below

        OnInteractRoleHitPhase1Event // used for hit detetion, built-in function will fill those data here
        OnInteractRoleHitPhase2Event // you can modify the damage effect here
        OnInteractRoleHitPhase3Event // we can do extra stuff, such as recover hp from attack


for global event, it's not opened to modder yet, we'll introduce it later


[DamageBasicData](class_cross_link_1_1_damage_basic_data.html)

Basic collision data, contains collider, rigidbody, impact...




[DamageHitData](class_cross_link_1_1_damage_hit_data.html)

Determines the damage calculation



[DamageEffectData](class_cross_link_1_1_damage_effect_data.html)

Behaviour by this damage, such as knockdown, dizzy, floating...







---

# Tricks

#### A easier way to config addressables.

1. put you mod folder under the path: "Asset/Build". for anything doesn't need to be addressable, please put outside of this folder.

 <img src="addressable_1.png">

2. put you resources into the corresponding folder, such as Weapon, Audio, ICon

3. add WeaponPaths in addressableConfig(Assets->Resources->AddressableConfig).

4. define you prefix.

5. click - **Create And Refresh Addressable Name**.

 <img src="addressable_3.png">

6. fill the old and new prefix, click invoke to modify. it will modify all perfabs and scripts at WeaponPaths.

 <img src="addressable_4.png">

#### For HandAttach component, use HandPoseHelper to adject hand's position and rotation.

1. add HandPoseHelper("Asset/Resources/Tools/HandPoseHelper") to you scene.

2. select **HandAttach**.

 <img src="HandPoseHelper.png">

3. click AddDrawTool to draw hand.

4. adject node **Bip002 R Hand** and **Bip002 L Hand** according to the hand.

5. click RemoveDrawTool to remove tool, save your weapon's perfab, done.



#### Background Knowledge

| Term                  | Explaination                                                 |
| --------------------- | ------------------------------------------------------------ |
| addressables          | by pressing the addressable toggle box to build resource into addressable resource, then rename the path to make it get loaded correctly in game.  once you filled in correct addressable resources, then the game will load it on start |
| resources path        | Weapon Path: Weapon/<br>        Script Path: LuaScript/<br>        ICon Path:ICon/<br>        Effect Path:Effect/<br>        Audio Path: Audio/Sound/<br>        FlyObj Path: FlyObj/<br>                           any resource's addressable name in the path above, can be loaded by the system without path included. for example, if an effect's addressable name is Effect/explosion, can be create by EffectMgr.Instance:PlayEffect("explosion") |
| entry point           | any lua script's addressable name is Entry, will become the entry point of this mod |
| components end with X | means it's scriptable, such as FlyObjectX, InteractTriggerX  |
| coordinate            | z is forward, x is right, y is up                            |




#### DebugTools

| Dev Environment                             | Pros                                                         | Cons                                                |
| ------------------------------------------- | ------------------------------------------------------------ | --------------------------------------------------- |
| edit in Editor + debug in Windows Simulator | 1. dosen't need a gaming PC<br> 2. iterate very fast, because you don't need to put on headset<br> 3. you can make use of shotcut to debug faster | 1. the feeling is different from in VR              |
| edit in Editor + debug in SteamVR           | 1. you can check the real size in headset<br> 2. you can make use of shotcut to debug faster | 1. need to install mod and put on headset everytime |
| edit in Editor + debug in Quest             | 1. you can check the real size in headset                    | 1. need to install mod and put on headset everytime |

#### Shotcut

| Shotcut        | Function at Body Mode | Function at Hand Mode    |
| -------------- | --------------------- | ------------------------ |
| WASD           | walk                  | walk                     |
| Shift          | run                   | hand rotation            |
| Alt            | switch to Hand Mode   | switch to Body Mode      |
| Tab            |                       | switch between your hand |
| Mouse Movement | body rotation         | hand movement            |
| Ctrl           |                       | hand movement on Z axis  |

| Shotcut  | Function                         |
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



