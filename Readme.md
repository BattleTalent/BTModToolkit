# Welcome to the Scripting Document

Hello, this is scripting document, you can query these API, then use them in lua, looking forward to what will everyone create. We will open more system in the future, currently we only export weapon related tools(in C#). 

##### Quick Start

1. install Unity 2019.4.12f(download [UnityHub](https://unity3d.com/get-unity/download), then download Engine via Hub)
2. download [ModProj](https://github.com/fonzieyang/BTModToolkit)
3. open the project, then click BuildTools/BuildAllBundles
4. copy the mod files to the corresponding path:
	* Windows: C:\Users\[username]\AppData\LocalLow\CrossLink\BattleTalent\Mods
	* Quest:      /sdcard/Android/data/com.CrossLink.SAO/files/Mods/
5. Done! (if something goes wrong, please open your cheat menu then tell us about your error messages)

if you just want to try play this mod, you can download it from [HERE](https://github.com/fonzieyang/BTModToolkit/releases/download/V0.0.1/ModProj.zip)

##### Make your first weapon with BTModToolkit in 10 mintues

please check out this [Weapon Modding Video](https://youtu.be/IqPl5KRgZ8Y), for more details please check out this [Weapon Modding Doc](https://docs.google.com/spreadsheets/d/1z3dAbbIpCERFYRC-NOEZxo7R3kv008184Jws9MFz2NI/edit?usp=sharing)


##### Before release your own mod

please make sure you've *cleared the project* and *renamed the product*

two ways to clear the example project:
* remove the pre-added addressables(window->asset management->addressables->group)
* delete the Examples folder(Assets/Examples)

the way to change product name:
* Edit -> Project Settings -> Modify the Project Name

##### Tips for scripting in lua:

* namespace of CShape code is CS in lua
* namespace CrossLink is CL in lua, CL == CS.CrossLink
* namespace UnityEngine is UE in lua, UE == CS.UnityEngine
* calling a static function should use '.' instead of ':' in lua
* recommend using [ZeroBrane Studio](https://studio.zerobrane.com/) as lua editor

##### TODO

* lua grammar completion(ZeroBrane based)
* lua break point debug(ZeroBrane based)
* level modding
* npc modding
* player modding




##### Related Manual

* [Unity manual](https://docs.unity3d.com/2019.4/Documentation/Manual/)
* [Lua manual](https://www.lua.org/manual/5.1/)
* [Xlua](https://github.com/Tencent/xLua/blob/master/README_EN.md)
* [EasySave3](https://docs.moodkie.com/easy-save-3/es3-api/es3-class/)


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

	local puRB = CL.PhysicsUnit.GetPhysicsUnit(rigidbody.transform)
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




---


# Coding Examples



#### tick a function

	local scheId = CL.Scheduler.Create(target,
	function(sche, t, s)
		local progress = t/s
		print("updating:" .. progress)
		if (t>=s) then
			print("finished succ")
		end
	end
	, 0.1, 10, 0.2)
	:SetUpdateChannel(CL.Scheduler.UpdateChannel.FixedUpdate)
	:IgnoreTimeScale(true)
	:SetOnStop(function(sche)
		print("on stop")
	end).actionId
	CL.Scheduler.RemoveScheduler(scheId)

#### add store item

	storeItem = CL.UnlockContentItem()
	storeItem.name = weaponName
	storeItem.dependItemName = dependWeapon
	storeItem.iconName = weaponName
	storeItem.contentType = CL.UnlockContentConfig.UnlockContentType.Weapon
	storeItem.unlockRequireCoinNum = 1
	CL.UnlockContentConfig.AddItem(storeItem)

#### add a new hitinfo to the config file

	HitInfoConfig = CL.GameDataMgr.GetData(typeof(CL.HitInfoConfig))
	local lightBladeHitInfo = CL.HitInfoConfig.HitInfoConfigItem()
	lightBladeHitInfo.Name = "WMD_LightBlade"
	lightBladeHitInfo.VelocityMlp = 1.5
	lightBladeHitInfo.DamageMlp = 30
	lightBladeHitInfo.DamageThrough = 1.5
	lightBladeHitInfo.DamageCrit = 1.5
	lightBladeHitInfo.StabMlp = 1.5
	lightBladeHitInfo.HitMlp = 120
	lightBladeHitInfo.HitRandom = 0.2
	lightBladeHitInfo.StabDamage = 120
	lightBladeHitInfo.BreakDefenceMlp = 1
	lightBladeHitInfo.HitBackMlp = 90
	lightBladeHitInfo.KnockoutFactor = 0.1
	lightBladeHitInfo.DizzyFactor = 0
	lightBladeHitInfo.StiffValue = 2.5
	HitInfoConfig:AddData(lightBladeHitInfo)


#### check npc's hp

	if (pu.unitType == CL.Tagger.Tag.InteractRole) then
		local fc = pu
		print(fc.attr.HpBase)
		// even change camp(from enemy to friend)
		if (fc.attr.camp == "Bad") then
			fc.attr:ChangeCamp()
		end		
	end


#### apply hit scan(in C#), raycast with the weapon's direction then apply hitscan dmg

	if (Physics.SphereCast(interact.trans.position +
			interact.trans.forward * slashDetectOffset, 0.2f,
			interact.trans.forward,
			out hitInfo, slashDis, slashLayer))
	{
			InteractTrigger.SetOverrideHitInfo(rgHitInfo);
			InteractTrigger.BeginScanDmg();
			InteractTrigger.AddHitScanProtect();
			InteractTrigger.ApplyHitScanDamage(hitInfo.collider, interact,
					interact.rb, hitCol, interact.rb.velocity.normalized,
					holdingCharacter.aiProxy.GetCamp());
			InteractTrigger.EndHitScan();
			InteractTrigger.SetOverrideHitInfo(null);
			if (InteractTrigger.ScanHited)
			{
					windCutSound.Play(hitInfo.point);
			}                
	}


#### search enemies around you(in C#), the value of paramter camp is only "Good" and "Bad"

	static public AIProxy GetCloestTarget(Transform self, string camp, bool goodRelate = true, bool includeDown = false)
	{
			AIProxy target = null;
			float minDis = 10000;

			var allChar = AIProxy.GetAroundAITarget();
			var charIte = allChar.GetEnumerator();

			while (charIte.MoveNext()){
					var proxy = charIte.Current.Value;

					if (includeDown == false && proxy.IsDown())
							continue;

					if (proxy.GetTransform() == self || proxy.gameObject.activeInHierarchy == false)
							continue;

					var rela = SocietyMgr.GetSocietyInfo().GetCampRelationTo(camp, proxy.GetCamp());
					if ((goodRelate && rela > 0)||
							(!goodRelate && rela < 0)){
							var dis = (proxy.GetTransform().position - self.position).sqrMagnitude;
							if (dis < minDis){
									minDis = dis;
									target = proxy;
							}
					}
			}
			return target;
	}

#### xlua tips

	// cast & typeof example
	cast(castObj, typeof(CL.TestCastClass))

	// the way we implemented oop
	function Clone(t, cpFunc)
		cpFunc = cpFunc or true
		if type(t) ~= 'table' then return t end
		local meta = getmetatable(t)
		local target = {}
		for k, v in pairs(t) do
				if type(v) == 'table' then
						target[k] = Clone(v, cpFunc)
				else
						if not cpFunc and type(v) == 'function' then
						else
								target[k] = v
						end            
				end
		end
		setmetatable(target, meta)
		return target
	end

	local getmetatable = getmetatable
	function Class(base,static,instance)

		local mt = nil
		if (base ~= nil) then
				mt = getmetatable(base)
		end

		local class = static or {}

		local callfunc
		callfunc = function(cls, ...)
								local r = nil
								if (mt ~= nil) then
										r = mt.__call(cls, ...)
								end
								local ret
								if instance ~= nil then
										ret = Clone(instance, false)
								else
										ret = {}
								end

								local ins_ret = setmetatable(
										{
												__base = r,
										},

										{
												__index = function(t, k)
														local ret_field
														ret_field = rawget(t,k)
														if nil ~= ret_field then
																return ret_field
														end

														ret_field = ret[k]
														if nil == ret_field and r ~= nil then
																ret_field = r[k]
														end
														return ret_field
												end,

												__newindex = function(t,k,v)
														if r == nil or r[k] == nil then
																rawset(t,k,v)
														else
																r[k] = v
														end
												end,
										})

								if ret.ctor then
										ins_ret:ctor(...)
								end

								return ins_ret
						end

		setmetatable(class,
				{
						__index = function(t, k)
												if k == 'CreateFromClass' then
														return callfunc
												end

												if base ~= nil and base[k] ~= nil then
														return base[k]
												end

												if instance ~= nil and instance[k] ~= nil then
														return instance[k]
												end                    
											end,

						__call = callfunc,
				}
		)
		return class
	end
	";

---
