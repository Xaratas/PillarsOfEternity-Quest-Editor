using AI.Achievement;
using AI.Plan;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
public static class Scripts
{
	[Serializable]
	public enum BrowserType
	{
		None,
		GlobalVariable,
		Conversation,
		Quest,
		ObjectGuid
	}
	private const string ConsoleNotifyColor = "[00FF00]";
	private static GameObject s_SoulMemoryCamera;
	public static AIPackageController GetAIController(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			AIPackageController component = objectByID.GetComponent<AIPackageController>();
			if (component)
			{
				return component;
			}
			UnityEngine.Debug.LogWarning(objectGuid + " doesn't have a AI Package Controller.", objectByID);
		}
		UnityEngine.Debug.LogWarning(objectGuid + " could not be found when searching for an AI Package Controller.", null);
		return null;
	}
	private static bool IsDead(AIController aiController)
	{
		Health component = aiController.gameObject.GetComponent<Health>();
		return component != null && component.Dead;
	}
	[Script("Set Package", "Scripts\\AI"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("AI Package", "New package for the AI", "")]
	public static void AISetPackage(Guid objectGuid, AIPackageController.PackageType newType)
	{
		AIPackageController aIController = Scripts.GetAIController(objectGuid);
		if (aIController)
		{
			if (Scripts.IsDead(aIController))
			{
				return;
			}
			aIController.ChangeBehavior(newType);
		}
	}
	[Script("Set Patrol State", "Scripts\\AI"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Should Patrol", "The new state of patrolling", "")]
	public static void AISetPatrolling(Guid objectGuid, bool shouldPatrol)
	{
		AIPackageController aIController = Scripts.GetAIController(objectGuid);
		if (aIController)
		{
			if (Scripts.IsDead(aIController))
			{
				return;
			}
			aIController.Patroller = shouldPatrol;
			aIController.InitAI();
		}
	}
	[Script("Set Patrol Point", "Scripts\\AI"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Patrol Point", "New base patrol point", "", Scripts.BrowserType.ObjectGuid)]
	public static void AISetPatrolPoint(Guid objectGuid, Guid waypointGuid)
	{
		AIPackageController aIController = Scripts.GetAIController(objectGuid);
		if (aIController)
		{
			if (Scripts.IsDead(aIController))
			{
				return;
			}
			aIController.Patroller = true;
			aIController.PreferredPatrolPoint = InstanceID.GetObjectByID(waypointGuid);
			aIController.InitAI();
		}
	}
	[Script("Move to Point", "Scripts\\AI"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Point", "Object to path to", "", Scripts.BrowserType.ObjectGuid), ScriptParam2("Movement Type", "How to get there", "")]
	public static void AIPathToPoint(Guid objectGuid, Guid waypointGuid, AnimationController.MovementType moveType)
	{
		AIPackageController aIController = Scripts.GetAIController(objectGuid);
		GameObject objectByID = InstanceID.GetObjectByID(waypointGuid);
		if (aIController != null && objectByID != null)
		{
			if (Scripts.IsDead(aIController))
			{
				return;
			}
			Waypoint component = objectByID.GetComponent<Waypoint>();
			if (component)
			{
				Patrol patrol = aIController.StateManager.CurrentState as Patrol;
				if (patrol != null && aIController.CurrentWaypoint == component)
				{
					return;
				}
				aIController.Patroller = true;
				aIController.StateManager.PopAllStates();
				aIController.CurrentWaypoint = null;
				aIController.PrevWaypoint = null;
				aIController.RecordRetreatPosition(component.transform.position);
				Patrol patrol2 = AIStateManager.StatePool.Allocate<Patrol>();
				patrol2.StartPoint = component;
				component.WalkOnly = (moveType == AnimationController.MovementType.Walk);
				patrol2.TargetScanner = aIController.GetTargetScanner();
				aIController.StateManager.PushState(patrol2);
				ScanForTarget scanForTarget = aIController.StateManager.DefaultState as ScanForTarget;
				if (scanForTarget != null)
				{
					patrol2.TargetScanner = scanForTarget.TargetScanner;
				}
				else
				{
					CasterScanForTarget casterScanForTarget = aIController.StateManager.DefaultState as CasterScanForTarget;
					if (casterScanForTarget != null)
					{
						patrol2.TargetScanner = casterScanForTarget.TargetScanner;
					}
				}
			}
			else
			{
				aIController.StateManager.PopAllStates();
				PathToPosition pathToPosition = AIStateManager.StatePool.Allocate<PathToPosition>();
				pathToPosition.Parameters.Destination = objectByID.transform.position;
				pathToPosition.Parameters.MovementType = moveType;
				aIController.StateManager.PushState(pathToPosition);
			}
		}
	}
	[Script("Force Attack", "Scripts\\AI"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Target", "Object to attack", "", Scripts.BrowserType.ObjectGuid)]
	public static void AIForceAttack(Guid objectGuid, Guid targetGuid)
	{
		AIPackageController aIController = Scripts.GetAIController(objectGuid);
		GameObject objectByID = InstanceID.GetObjectByID(targetGuid);
		if (aIController && objectByID)
		{
			if (Scripts.IsDead(aIController))
			{
				return;
			}
			ApproachTarget approachTarget = AIStateManager.StatePool.Allocate<ApproachTarget>();
			aIController.StateManager.PushState(approachTarget);
			approachTarget.TargetScanner = aIController.GetTargetScanner();
			approachTarget.Target = objectByID;
			approachTarget.IsForceAttack = true;
			if (approachTarget.TargetScanner == null)
			{
				approachTarget.Attack = AIController.GetPrimaryAttack(aIController.gameObject);
			}
		}
	}
	[Script("Set Busy State", "Scripts\\AI"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Is Busy", "The new state of busy", "")]
	public static void AISetBusy(Guid objectGuid, bool isBusy)
	{
		AIPackageController aIController = Scripts.GetAIController(objectGuid);
		if (aIController)
		{
			if (Scripts.IsDead(aIController))
			{
				return;
			}
			aIController.IsBusy = isBusy;
		}
	}
	[Script("Knock Down", "Scripts\\AI"), ScriptParam0("Target", "Character to knock down", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Duration", "The time is seconds for the character to lie down", "1.0")]
	public static void KnockDown(Guid targetGuid, float duration)
	{
		GameObject objectByID = InstanceID.GetObjectByID(targetGuid);
		if (objectByID)
		{
			AIController component = objectByID.GetComponent<AIController>();
			if (component == null)
			{
				return;
			}
			if (Scripts.IsDead(component))
			{
				return;
			}
			if (duration < 0f)
			{
				duration = 0.1f;
			}
			KnockedDown knockedDown = component.StateManager.CurrentState as KnockedDown;
			if (knockedDown != null)
			{
				knockedDown.ResetKnockedDown(duration);
				return;
			}
			knockedDown = AIStateManager.StatePool.Allocate<KnockedDown>();
			component.StateManager.PushState(knockedDown);
			knockedDown.SetKnockdownTime(duration);
		}
	}
	[Script("Player Safe Mode", "Scripts\\AI"), ScriptParam0("Enabled", "True to disable party character AIs and Input, false to restore", "true")]
	public static void PlayerSafeMode(bool enabled)
	{
		if (enabled && TimeController.Instance)
		{
			TimeController.Instance.SafePaused = false;
		}
		GameState.PlayerSafeMode = enabled;
		GameInput.DisableInput = enabled;
		UICamera.DisableSelectionInput = enabled;
		if (enabled)
		{
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			for (int i = 0; i < partyMembers.Length; i++)
			{
				PartyMemberAI partyMemberAI = partyMembers[i];
				if (!(partyMemberAI == null))
				{
					GameObject gameObject = partyMemberAI.gameObject;
					AIController component = gameObject.GetComponent<AIController>();
					if (component != null && !(component.StateManager.CurrentState is Unconscious))
					{
						component.InterruptAnimationForCutscene();
						component.StateManager.PopAllStates();
					}
				}
			}
			if (UIWindowManager.Instance)
			{
				UIWindowManager.Instance.CloseAllWindows();
			}
		}
	}
	[Script("Force In Combat Idle", "Scripts\\AI"), ScriptParam0("NPC", "Set the combat idle stance for this character.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("In Combat Idle", "Whether the character should be a combat idle stance.", "true")]
	public static void ForceInCombatIdle(Guid objectGuid, bool inCombatIdle)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		AnimationController component = objectByID.GetComponent<AnimationController>();
		if (component == null)
		{
			return;
		}
		component.ForceCombatIdle = inCombatIdle;
	}
	[Script("Force Combat Pathing", "Scripts\\AI"), ScriptParam0("NPC", "Set combat pathing for this character.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Use Combat Pathing", "Whether the character should be forced to use combat pathing.", "true")]
	public static void ForceCombatPathing(Guid objectGuid, bool useCombatPathing)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		Mover component = objectByID.GetComponent<Mover>();
		if (component == null)
		{
			return;
		}
		component.ForceCombatPathing = useCombatPathing;
	}
	private static AudioBank GetAudioBank(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			return objectByID.GetComponent<AudioBank>();
		}
		UnityEngine.Debug.LogWarning(objectGuid + " could not be found when searching for an AudioBank!", null);
		return null;
	}
	private static AudioSource GetAudioSource(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			return objectByID.GetComponent<AudioSource>();
		}
		UnityEngine.Debug.LogWarning(objectGuid + " could not be found when searching for an AudioSource!", null);
		return null;
	}
	private static SoundSetComponent GetSoundSet(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			return objectByID.GetComponent<SoundSetComponent>();
		}
		UnityEngine.Debug.LogWarning(objectGuid + " could not be found when searching for an SoundSetComponent!", null);
		return null;
	}
	private static DeveloperCommentary GetCommentary(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			return objectByID.GetComponent<DeveloperCommentary>();
		}
		UnityEngine.Debug.LogWarning(objectGuid + " could not be found when searching for an DeveloperCommentary!", null);
		return null;
	}
	[Script("Play Sound From Bank", "Scripts\\Audio"), ScriptParam0("Object", "Object with audiobank", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Sound Cue", "The name of the cue in the audio bank.", "")]
	public static void PlaySound(Guid objectGuid, string audioCue)
	{
		AudioBank audioBank = Scripts.GetAudioBank(objectGuid);
		if (audioBank)
		{
			audioBank.PlayFrom(audioCue);
		}
	}
	[Script("Play Sound From Source", "Scripts\\Audio"), ScriptParam0("Object", "Object with AudioSource", "", Scripts.BrowserType.ObjectGuid)]
	public static void PlaySound(Guid objectGuid)
	{
		AudioSource audioSource = Scripts.GetAudioSource(objectGuid);
		if (audioSource)
		{
			GlobalAudioPlayer.Play(audioSource);
		}
	}
	[Script("Play Sound From Sound Set", "Scripts\\Audio"), ScriptParam0("Object", "Object with Sound Set", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Sound Action", "Sound To Play", "Invalid"), ScriptParam2("Sound Variation", "Variation to play", "-1")]
	public static void PlaySoundFromSoundSet(Guid objectGuid, SoundSet.SoundAction action, int variation)
	{
		SoundSetComponent soundSet = Scripts.GetSoundSet(objectGuid);
		if (soundSet && soundSet.SoundSet)
		{
			soundSet.SoundSet.PlaySound(action, variation, false);
		}
	}
	[Script("Play Commentary", "Scripts\\Audio"), ScriptParam0("Object", "Object with a developer commentary component", "", Scripts.BrowserType.ObjectGuid)]
	public static void PlayCommentary(Guid objectGuid)
	{
		DeveloperCommentary commentary = Scripts.GetCommentary(objectGuid);
		if (commentary)
		{
			commentary.Play();
		}
	}
	[Script("Play Music", "Scripts\\Audio"), ScriptParam0("Audio Clip Filename", "AudioClip path relative to the Resources folder.", "")]
	public static void PlayMusic(string filename)
	{
		AudioClip audioClip = Resources.Load<AudioClip>(filename);
		if (audioClip == null)
		{
			UnityEngine.Debug.LogError("PlayMusic failed, file not found or is not 44100hz: " + filename);
			return;
		}
		MusicManager.FadeParams fadeParams = new MusicManager.FadeParams();
		MusicManager.Instance.PlayMusic(audioClip, fadeParams);
	}
	[Script("Play Scripted Music", "Scripts\\Audio"), ScriptParam0("Clip Name", "The scripted clip name specified in Area Music", ""), ScriptParam1("Block Combat Music", "Prevents game from playing normal combat music", "false"), ScriptParam2("Fade Type", "The type of fade to use when transitioning to new music.", "FadeOutPauseFadeIn"), ScriptParam3("Fade Out Duration", "The length in seconds to fade out new music.", "0.5"), ScriptParam4("Fade In Duration", "The length in seconds to fade in new music.", "0.5"), ScriptParam5("Pause Duration", "The length in seconds to fade in new music.", "0.5"), ScriptParam6("Loop", "Designates if the music should loop.", "false")]
	public static void PlayScriptedMusic(string filename, bool blockCombatMusic, MusicManager.FadeType fadeType, float fadeOutDuration, float fadeInDuration, float pauseDuration, bool loop)
	{
		MusicManager.Instance.PlayScriptedMusic(filename, blockCombatMusic, fadeType, fadeOutDuration, fadeInDuration, pauseDuration, loop);
	}
	[Script("End Scripted Music", "Scripts\\Audio")]
	public static void EndScriptedMusic()
	{
		MusicManager.Instance.EndScriptedMusic();
	}
	[Script("Resume Area Music", "Scripts\\Audio")]
	public static void ResumeAreaMusic()
	{
		MusicManager.Instance.ResumeScriptedOrNormalMusic(true);
	}
	[Script("Fade Out Area Music", "Scripts\\Audio")]
	public static void FadeOutAreaMusic()
	{
		MusicManager.Instance.FadeOutAreaMusic(false);
	}
	[Script("Enable Music Loop Cooldown", "Scripts\\Audio")]
	public static void EnableMusicLoopCooldown()
	{
		MusicManager.Instance.EnableLoopCooldown();
	}
	[Script("Disable Music Loop Cooldown", "Scripts\\Audio")]
	public static void DisableMusicLoopCooldown()
	{
		MusicManager.Instance.DisableLoopCooldown();
	}
	[Script("Fade Out Audio", "Scripts\\Audio"), ScriptParam0("Parent Object", "The parent object with the audio sources", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Fade Time", "How long to fade the audio in seconds.", "1.0")]
	public static void FadeOutAudio(Guid parentObject, float fadeTime)
	{
		GameObject objectByID = InstanceID.GetObjectByID(parentObject);
		if (objectByID)
		{
			MusicManager.Instance.FadeAllAudioSourcesOut(objectByID, fadeTime);
		}
	}
	[Script("Fade In Audio", "Scripts\\Audio"), ScriptParam0("Parent Object", "The parent object with the audio sources", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Fade Time", "How long to fade the audio in seconds.", "1.0")]
	public static void FadeInAudio(Guid parentObject, float fadeTime)
	{
		GameObject objectByID = InstanceID.GetObjectByID(parentObject);
		if (objectByID)
		{
			MusicManager.Instance.FadeAllAudioSourcesIn(objectByID, fadeTime);
		}
	}
	[Script("Override Fatigue Whispers", "Scripts\\Audio"), ScriptParam0("New Volume", "The new volume to force the fatigue whispers to.", "0.0")]
	public static void OverrideFatigueWhispers(float newVolume)
	{
		if (FatigueWhispers.Instance)
		{
			FatigueWhispers.Instance.SetVolumeOverride(newVolume);
		}
	}
	[Script("Release Fatigue Whisper Override", "Scripts\\Audio")]
	public static void ReleaseFatigueWhisperOverride()
	{
		if (FatigueWhispers.Instance)
		{
			FatigueWhispers.Instance.ReleaseVolumeOverride();
		}
	}
	public static Faction GetFactionComponent(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			Faction component = objectByID.GetComponent<Faction>();
			if (component)
			{
				return component;
			}
			UnityEngine.Debug.LogWarning(objectGuid + " doesn't have a faction component.", objectByID);
		}
		UnityEngine.Debug.LogWarning(objectGuid + " could not be found when searching for faction component.", null);
		return null;
	}
	[Script("Set Is Hostile", "Scripts\\Faction"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Is Hostile", "Hostile state towards the player", "true")]
	public static void SetIsHostile(Guid objectGuid, bool isHostile)
	{
		Faction factionComponent = Scripts.GetFactionComponent(objectGuid);
		if (factionComponent)
		{
			factionComponent.RelationshipToPlayer = ((!isHostile) ? Faction.Relationship.Neutral : Faction.Relationship.Hostile);
		}
	}
	[Script("Set Team Relationship", "Scripts\\Faction"), ScriptParam0("Team A", "The first team to change", ""), ScriptParam1("Team B", "The second team to change", ""), ScriptParam2("Relationship", "How team A and B will relate to each other", "")]
	public static void SetTeamRelationship(string teamA, string teamB, Faction.Relationship newRelationship)
	{
		Team teamByTag = Team.GetTeamByTag(teamA);
		if (teamByTag == null)
		{
			UnityEngine.Debug.LogError("SetTeamRelationship has an error. " + teamA + " does not exist. Make sure you match up the script tag.");
			return;
		}
		Team teamByTag2 = Team.GetTeamByTag(teamB);
		if (teamByTag2 == null)
		{
			UnityEngine.Debug.LogError("SetTeamRelationship has an error. " + teamB + " does not exist. Make sure you match up the script tag.");
			return;
		}
		teamByTag.SetRelationship(teamByTag2, newRelationship, true);
		teamByTag2.SetRelationship(teamByTag, newRelationship, true);
	}
	[AdjustStat("axis", "strength", "id"), Script("Reputation Add Points By Tag", "Scripts\\Faction"), ScriptParam0("Faction Name", "Faction tag to modify", ""), ScriptParam1("Axis", "Good vs. Bad action", "Positive"), ScriptParam2("Strength", "Severity of the change", "Minor")]
	public static void ReputationAddPoints(FactionName id, Reputation.Axis axis, Reputation.ChangeStrength strength)
	{
		Reputation reputation = ReputationManager.Instance.GetReputation(id);
		if (reputation != null)
		{
			reputation.AddReputation(axis, strength);
			UnityEngine.Debug.Log(string.Concat(new string[]
			{
				id.ToString(),
				" reputation changed on the ",
				axis.ToString(),
				" axis (",
				strength.ToString(),
				")."
			}));
		}
		else
		{
			UnityEngine.Debug.LogError("Faction " + id.ToString() + " is not setup!");
		}
	}
	[AdjustStat("axis", "strength"), Script("Disposition Add Points", "Scripts\\Disposition"), ScriptParam0("Axis", "Action type", "Aggressive"), ScriptParam1("Strength", "Severity of the change", "Minor")]
	public static void DispositionAddPoints(Disposition.Axis axis, Disposition.Strength strength)
	{
		ReputationManager.Instance.PlayerDisposition.ChangeDisposition(axis, strength);
		UnityEngine.Debug.Log(string.Concat(new string[]
		{
			"Disposition change: ",
			axis.ToString(),
			" ",
			strength.ToString(),
			" change."
		}));
	}
	private static bool IsDead(GameObject gameObject)
	{
		Health component = gameObject.GetComponent<Health>();
		return component != null && component.Dead;
	}
	private static bool IsDeadOrMaimed(GameObject gameObject)
	{
		Health component = gameObject.GetComponent<Health>();
		return component != null && component.Dead && !component.MaimAvailable();
	}
	[Script("Print String", "Scripts\\General"), ScriptParam0("Text", "string to print to the console", "")]
	public static void PrintString(string text)
	{
		global::Console.AddMessage(text);
	}
	[Script("Activate Object", "Scripts\\General"), ScriptParam0("Object", "Object to activate.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Active", "Active state to set the object to.", "true")]
	public static void ActivateObject(Guid objectGuid, bool activate)
	{
		if (!activate)
		{
			GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
			if (objectByID)
			{
				objectByID.SetActive(false);
				Persistence component = objectByID.GetComponent<Persistence>();
				if (component)
				{
					component.SaveObject();
				}
				ConditionalToggle component2 = objectByID.GetComponent<ConditionalToggle>();
				if (component2)
				{
					component2.ActivateOnlyThroughScript = true;
					component2.StartActivated = false;
					InstanceID component3 = objectByID.GetComponent<InstanceID>();
					if (component3 != null)
					{
						component3.Load();
						ConditionalToggleManager.Instance.AddToScriptInactiveList(component2);
					}
				}
			}
		}
		else
		{
			List<ConditionalToggle> scriptInactiveList = ConditionalToggleManager.Instance.ScriptInactiveList;
			GameObject gameObject = null;
			foreach (ConditionalToggle current in scriptInactiveList)
			{
				if (!(current == null))
				{
					InstanceID component4 = current.GetComponent<InstanceID>();
					if (component4 && component4.Guid == objectGuid)
					{
						gameObject = component4.gameObject;
						break;
					}
				}
			}
			if (gameObject)
			{
				gameObject.SetActive(true);
				Persistence component5 = gameObject.GetComponent<Persistence>();
				if (component5)
				{
					component5.SaveObject();
				}
				ConditionalToggle component6 = gameObject.GetComponent<ConditionalToggle>();
				component6.ForceActivate();
				ConditionalToggleManager.Instance.ScriptInactiveList.Remove(component6);
			}
		}
	}
	[Script("Start Conversation", "Scripts\\Conversation"), ScriptParam0("Object", "Speaker Object", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Conversation", "Conversation File", "", Scripts.BrowserType.Conversation), ScriptParam2("Conversation Node ID", "Conversation Node ID", "0")]
	public static void StartConversation(Guid objectGuid, string conversation, int nodeID)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			if (Scripts.IsDeadOrMaimed(objectByID))
			{
				return;
			}
			ConversationManager.Instance.StartConversation(conversation, nodeID, objectByID, FlowChartPlayer.DisplayMode.Standard, false);
		}
	}
	[Script("Start Scripted Interaction", "Scripts\\Interaction"), ScriptParam0("Object", "Scripted Interaction Object", "", Scripts.BrowserType.ObjectGuid)]
	public static void StartScriptedInteraction(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			if (Scripts.IsDeadOrMaimed(objectByID))
			{
				return;
			}
			ScriptedInteraction component = objectByID.GetComponent<ScriptedInteraction>();
			if (component && GameState.CutsceneAllowed)
			{
				ConversationManager.Instance.KillAllBarkStrings();
				component.StartConversation();
			}
		}
	}
	[Script("Start Cutscene", "Scripts\\Cutscene"), ScriptParam0("Object", "Cutscene Object", "", Scripts.BrowserType.ObjectGuid)]
	public static void StartCutscene(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			Cutscene component = objectByID.GetComponent<Cutscene>();
			if (component)
			{
				component.StartCutscene();
			}
		}
	}
	[Script("Area Transition", "Scripts\\Area"), ScriptParam0("Area Name", "The name of the area", ""), ScriptParam1("Start Point Name", "The name of the start point", "")]
	public static void AreaTransition(MapType areaName, StartPoint.PointLocation startPoint)
	{
		GameState.s_playerCharacter.StartPointLink = startPoint;
		GameState.LoadedGame = false;
		GameState.ChangeLevel(areaName);
	}
	[Script("Add Experience", "Scripts\\Quest"), ScriptParam0("Experience", "Amount of XP to reward", "50")]
	public static void AddExperience(int XP)
	{
		foreach (PartyMemberAI current in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			CharacterStats component = current.GetComponent<CharacterStats>();
			if (component)
			{
				component.AddExperience(XP);
			}
		}
		global::Console.AddMessage(global::Console.Format(GUIUtils.GetTextWithLinks(1016), new object[]
		{
			XP
		}), Color.green);
	}
	[Script("Add Experience To Level", "Scripts\\Quest"), ScriptParam0("Level", "The level you want to level to", "12")]
	public static void AddExperienceToLevel(int level)
	{
		foreach (PartyMemberAI current in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			CharacterStats component = current.GetComponent<CharacterStats>();
			if (component && component.Level < level)
			{
				component.Experience = CharacterStats.ExperienceNeededForLevel(level);
			}
		}
	}
	[Script("Add Experience Player", "Scripts\\Quest"), ScriptParam0("Experience", "Amount of XP to reward", "50")]
	public static void AddExperiencePlayer(int XP)
	{
		if (GameState.s_playerCharacter == null)
		{
			return;
		}
		PartyMemberAI component = GameState.s_playerCharacter.GetComponent<PartyMemberAI>();
		if (component == null)
		{
			return;
		}
		if (!component.Secondary)
		{
			CharacterStats component2 = component.GetComponent<CharacterStats>();
			if (component2)
			{
				component2.AddExperience(XP);
			}
		}
		global::Console.AddMessage(global::Console.Format(GUIUtils.GetTextWithLinks(1016), new object[]
		{
			XP
		}), Color.green);
	}
	[Script("Set Interaction Image", "Scripts\\Interaction"), ScriptParam0("Interaction Image", "The interaction image index", "0")]
	public static void SetInteractionImage(int index)
	{
		if (ScriptedInteraction.ActiveInteraction)
		{
			ScriptedInteraction.ActiveInteraction.SetState(index);
		}
	}
	[Script("Flip Tile", "Scripts\\Tile"), ScriptParam0("Object", "Flip tile object", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Frame", "The interaction image index", "0")]
	public static void FlipTile(Guid objectGuid, int frame)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		TileFlipper component = objectByID.GetComponent<TileFlipper>();
		if (component)
		{
			component.Flip(frame);
		}
	}
	[Script("Add to Party", "Scripts\\Party"), ScriptParam0("Tag", "Companion to Add", "", Scripts.BrowserType.ObjectGuid)]
	public static void AddToParty(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			UnityEngine.Debug.LogWarning("Could not find object to add to party.");
			return;
		}
		for (int i = 0; i < PartyMemberAI.PartyMembers.Length; i++)
		{
			if (PartyMemberAI.PartyMembers[i] != null && PartyMemberAI.PartyMembers[i].gameObject == objectByID)
			{
				return;
			}
		}
		PartyMemberAI component = objectByID.GetComponent<PartyMemberAI>();
		if (component)
		{
			component.AssignedSlot = PartyMemberAI.NextAvailablePrimarySlot;
		}
		PartyMemberAI.AddToActiveParty(objectByID, true);
	}
	[Script("Remove from Party", "Scripts\\Party"), ScriptParam0("Tag", "Companion to Remove", "", Scripts.BrowserType.ObjectGuid)]
	public static void RemoveFromParty(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			UnityEngine.Debug.LogWarning("Could not find object to remove party.");
			return;
		}
		PartyMemberAI component = objectByID.GetComponent<PartyMemberAI>();
		if (component == null)
		{
			UnityEngine.Debug.LogWarning("Object " + objectByID.name + " doesn't have a PartyMemberAI. Cannot remove from party.");
			return;
		}
		PartyMemberAI.RemoveFromActiveParty(component, true);
	}
	[Script("Teleport Party To Location", "Scripts\\Movement"), ScriptParam0("Tag", "Location to teleport to ", "", Scripts.BrowserType.ObjectGuid)]
	public static void TeleportPartyToLocation(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		PartyWaypoint component = objectByID.GetComponent<PartyWaypoint>();
		if (component)
		{
			component.TeleportPartyToLocation();
		}
		else
		{
			int num = 0;
			PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
			for (int i = 0; i < partyMembers.Length; i++)
			{
				PartyMemberAI partyMemberAI = partyMembers[i];
				if (partyMemberAI != null)
				{
					partyMemberAI.transform.position = objectByID.transform.position;
					partyMemberAI.transform.rotation = objectByID.transform.rotation;
				}
				num++;
			}
		}
		CameraControl.Instance.FocusOnPoint(objectByID.transform.position);
	}
	[Script("Teleport Player To Location", "Scripts\\Movement"), ScriptParam0("Tag", "Location to teleport to ", "", Scripts.BrowserType.ObjectGuid)]
	public static void TeleportPlayerToLocation(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		if (GameState.s_playerCharacter == null)
		{
			return;
		}
		GameState.s_playerCharacter.transform.position = objectByID.transform.position;
		GameState.s_playerCharacter.transform.rotation = objectByID.transform.rotation;
		CameraControl.Instance.FocusOnPoint(objectByID.transform.position);
	}
	[Script("Teleport Object To Location", "Scripts\\Movement"), ScriptParam0("Object", "The object to teleport", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Tag", "Location to teleport to.", "", Scripts.BrowserType.ObjectGuid)]
	public static void TeleportObjectToLocation(Guid objectGuid, Guid targetGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		GameObject objectByID2 = InstanceID.GetObjectByID(targetGuid);
		if (objectByID2 == null || objectByID == null)
		{
			return;
		}
		AIController component = objectByID.GetComponent<AIController>();
		if (component != null)
		{
			component.RecordRetreatPosition(objectByID2.transform.position);
		}
		objectByID.transform.position = objectByID2.transform.position;
		objectByID.transform.rotation = objectByID2.transform.rotation;
	}
	[Script("Start Timer", "Scripts\\Timer"), ScriptParam0("Object", "The timer object", "", Scripts.BrowserType.ObjectGuid)]
	public static void StartTimer(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		Timer component = objectByID.GetComponent<Timer>();
		if (component)
		{
			component.StartTimer();
		}
	}
	[Script("Stop Timer", "Scripts\\Timer"), ScriptParam0("Object", "The timer object", "", Scripts.BrowserType.ObjectGuid)]
	public static void StopTimer(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		Timer component = objectByID.GetComponent<Timer>();
		if (component)
		{
			component.StopTimer();
		}
	}
	[Script("Set Timer", "Scripts\\Timer"), ScriptParam0("Object", "The timer object", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Time", "New delay time for timer", "1")]
	public static void SetTimer(Guid objectGuid, float time)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		Timer component = objectByID.GetComponent<Timer>();
		if (component)
		{
			component.Delay = time;
		}
	}
	[Script("Rest", "Scripts\\Time")]
	public static void Rest()
	{
		RestZone.ShowRestUI(UIRestMovieManager.Mode.CAMP);
	}
	[Script("RestWithMovieID", "Scripts\\Time"), ScriptParam0("MovieType", "This is the movie enum to display", "0")]
	public static void RestWithMovieID(UIRestMovieManager.Mode movie)
	{
		RestZone.ShowRestUI(movie);
	}
	[Script("AdvanceTimeByHours", "Scripts\\Time"), ScriptParam0("Hours", "Hours to advance", "8")]
	public static void AdvanceTimeByHours(int hours)
	{
		WorldTime.Instance.AdvanceTimeByHours(hours, false);
		RestZone.TriggerOnResting();
	}
	[Script("AdvanceTimeToHour", "Scripts\\Time"), ScriptParam0("Hour", "Hour to advance to", "0")]
	public static void AdvanceTimeToHour(int hour)
	{
		WorldTime.Instance.AdvanceTimeToHour(hour);
		RestZone.TriggerOnResting();
	}
	[Script("Change Water Level", "Scripts\\Water"), ScriptParam0("Object", "The water plane", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Water Level", "New Water Level", "0"), ScriptParam2("Timer", "Time to move to new level", "1")]
	public static void ChangeWaterLevel(Guid objectGuid, float newLevel, float time)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		WaterLevelController component = objectByID.GetComponent<WaterLevelController>();
		if (component != null)
		{
			component.ChangeWaterLevel(newLevel, time);
		}
	}
	[Script("Set Trigger Enabled", "Scripts\\Trigger"), ScriptParam0("Object", "The trigger", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("isEnabled", "New Enabled Value", "true")]
	public static void SetTriggerEnabled(Guid objectGuid, bool isEnabled)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		Trigger component = objectByID.GetComponent<Trigger>();
		if (component != null)
		{
			component.IsEnabled = isEnabled;
		}
	}
	[Script("Set Switch Enabled", "Scripts\\Switch"), ScriptParam0("Object", "The switch", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("isEnabled", "New Enabled Value", "true")]
	public static void SetSwitchEnabled(Guid objectGuid, bool isEnabled)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		global::Switch component = objectByID.GetComponent<global::Switch>();
		if (component != null)
		{
			component.Enabled = isEnabled;
		}
	}
	[Script("World Map Set Visibility", "Scripts\\Trigger"), ScriptParam0("Map", "Map to change status of", "Map"), ScriptParam1("Visibility", "New visibility status", "Locked")]
	public static void WorldMapSetVisibility(MapType map, MapData.VisibilityType visibility)
	{
		WorldMap.Instance.SetVisibility(map, visibility);
	}
	[Script("Fade To Black", "Scripts\\Fade"), ScriptParam0("Fade Time", "Fade time", "2.0"), ScriptParam1("Fade Music", "Fade music", "true"), ScriptParam2("Fade Ambient Audio", "Fade all ambient audio", "true")]
	public static void FadeToBlack(float time, bool music, bool audio)
	{
		FadeManager.Instance.FadeMusic = music;
		FadeManager.Instance.FadeAudio = audio;
		FadeManager.Instance.FadeToBlack(FadeManager.FadeType.Script, time);
	}
	[Script("Fade From Black", "Scripts\\Fade"), ScriptParam0("Fade Time", "Fade time", "2.0"), ScriptParam1("Fade Music", "Fade music", "true"), ScriptParam2("Fade Ambient Audio", "Fade all ambient audio", "true")]
	public static void FadeFromBlack(float time, bool music, bool audio)
	{
		FadeManager.Instance.FadeMusic = music;
		FadeManager.Instance.FadeAudio = audio;
		FadeManager.Instance.FadeFromBlack(FadeManager.FadeType.Script, time);
	}
	[Script("Encounter Spawn", "Scripts\\Encounter"), ScriptParam0("Object", "The Encounter", "", Scripts.BrowserType.ObjectGuid)]
	public static void EncounterSpawn(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		Encounter component = objectByID.GetComponent<Encounter>();
		if (component != null)
		{
			component.ForceSpawn();
		}
	}
	[Script("Encounter Despawn", "Scripts\\Encounter"), ScriptParam0("Object", "The Encounter", "", Scripts.BrowserType.ObjectGuid)]
	public static void EncounterDespawn(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		Encounter component = objectByID.GetComponent<Encounter>();
		if (component != null)
		{
			component.DeSpawn();
		}
	}
	[Script("Encounter Set Combat End When All Are Dead Flag", "Scripts\\Encounter"), ScriptParam0("Object", "The Encounter", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("CombatEndWhenAllAreDeadFlag", "Set this value", "true")]
	public static void EncounterSetCombatEndWhenAllAreDeadFlag(Guid objectGuid, bool boolValue)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		Encounter component = objectByID.GetComponent<Encounter>();
		if (component != null)
		{
			component.SetCombatEndsWhenAllAreDeadAll(boolValue);
		}
	}
	[Script("SetNavMeshObstacleActivated", "Scripts\\NavObstacle"), ScriptParam0("Is Active", "Sets if the obstacle is activated or not.", "true")]
	public static void SetNavMeshObstacleActivated(Guid objectGuid, bool isActive)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		NavMeshObstacle component = objectByID.GetComponent<NavMeshObstacle>();
		if (component != null)
		{
			component.carving = isActive;
		}
	}
	[Script("Set Background", "Scripts\\Background"), ScriptParam0("Object", "The character to set background on", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Background", "The background value", "0")]
	public static void SetBackground(Guid objectGuid, CharacterStats.Background background)
	{
		CharacterStats characterStats = Conditionals.GetCharacterStats(objectGuid);
		if (characterStats)
		{
			characterStats.CharacterBackground = background;
		}
	}
	[Script("Set Player Background", "Scripts\\PlayerBackground"), ScriptParam0("Background", "The background value", "0")]
	public static void SetPlayerBackground(CharacterStats.Background background)
	{
		CharacterStats component = GameState.s_playerCharacter.GetComponent<CharacterStats>();
		if (component)
		{
			component.CharacterBackground = background;
		}
	}
	[Script("Unlock Present Story Item", "Scripts\\PlayerBackground"), ScriptParam0("Key", "The string key of the item to unlock", "")]
	public static void UnlockPresentStoryItem(string key)
	{
		if (JournalBiographyManager.Instance)
		{
			JournalBiographyManager.Instance.UnlockPresentStory(key);
		}
		else
		{
			UnityEngine.Debug.LogError("JournalBiography manager not initialized in UnlockPresentStoryItem.");
		}
	}
	[Script("Unlock Past Story Item", "Scripts\\PlayerBackground"), ScriptParam0("Key", "The string key of the item to unlock", "")]
	public static void UnlockPastStoryItem(string key)
	{
		if (JournalBiographyManager.Instance)
		{
			JournalBiographyManager.Instance.UnlockPastStory(key);
		}
		else
		{
			UnityEngine.Debug.LogError("JournalBiography manager not initialized in UnlockPastStoryItem.");
		}
	}
	[Script("End Game", "Scripts\\General")]
	public static void EndGame()
	{
		UIEndGameSlidesManager.Instance.ShowWindow();
	}
	[Script("Autosave", "Scripts\\General")]
	public static void Autosave()
	{
		GameState.Autosave();
	}
	[Script("Screen Shake", "Scripts\\Camera"), ScriptParam0("Duration", "Duration of the shake", "0"), ScriptParam1("Strength", "Strength of the shake", "0")]
	public static void ScreenShake(float duration, float strength)
	{
		CameraControl.Instance.ScreenShake(duration, strength);
	}
	[Script("Increment Tracked Achievement Stat", "Scripts\\General"), ScriptParam0("Tracked Stat", "The tracked achievement stat we are incrementing.", "TrackedStat")]
	public static void IncrementTrackedAchievementStat(AchievementTracker.TrackedAchievementStat stat)
	{
		if (AchievementTracker.Instance)
		{
			AchievementTracker.Instance.IncrementTrackedStat(stat);
		}
	}
	[Script("Decrement Tracked Achievement Stat", "Scripts\\General"), ScriptParam0("Tracked Stat", "The tracked achievement stat we are incrementing.", "TrackedStat")]
	public static void DecrementTrackedAchievementStat(AchievementTracker.TrackedAchievementStat stat)
	{
		if (AchievementTracker.Instance)
		{
			AchievementTracker.Instance.DecrementTrackedStat(stat);
		}
	}
	[Script("Set Tracked Achievement Stat", "Scripts\\General"), ScriptParam0("Tracked Stat", "The tracked achievement stat we are incrementing.", "TrackedStat"), ScriptParam1("New Value", "The new value we are setting the tracked stat to.", "0")]
	public static void IncrementTrackedAchievementStat(AchievementTracker.TrackedAchievementStat stat, int value)
	{
		if (AchievementTracker.Instance)
		{
			AchievementTracker.Instance.ForceSetTrackedStat(stat, value);
		}
	}
	[Script("Select Weapon Set", "Scripts\\General"), ScriptParam0("Object", "Character", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Weapon Set ID", "The index of the weapon set to apply.", "0")]
	public static void SelectWeaponSet(Guid objectGuid, int weaponSet)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			if (Scripts.IsDead(objectByID))
			{
				return;
			}
			Equipment component = objectByID.GetComponent<Equipment>();
			if (component)
			{
				component.SelectWeaponSet(weaponSet, false);
			}
		}
	}
	[Script("Soul Memory Camera Enable", "Scripts\\Camera"), ScriptParam0("Enabled", "Whether the camera should be enabled or disabled", "true")]
	public static void SoulMemoryCameraEnable(bool enabled)
	{
		if (enabled)
		{
			MusicManager.Instance.FadeAmbientAudioOut(0.3f);
			MusicManager.Instance.FadeOutAreaMusic(false);
			if (Scripts.s_SoulMemoryCamera == null)
			{
				GameObject gameObject = GameResources.LoadPrefab<GameObject>("SoulMemoryCamera", true);
				if (gameObject)
				{
					Persistence component = gameObject.GetComponent<Persistence>();
					if (component)
					{
						GameUtilities.Destroy(component);
					}
					gameObject.transform.parent = Camera.main.gameObject.transform;
					Scripts.s_SoulMemoryCamera = gameObject;
				}
			}
			FullscreenCameraEffect component2 = Scripts.s_SoulMemoryCamera.GetComponent<FullscreenCameraEffect>();
			if (component2)
			{
				component2.FadeIn();
			}
		}
		else
		{
			MusicManager.Instance.FadeAmbientAudioIn(0.3f);
			MusicManager.Instance.ResumeScriptedOrNormalMusic(true);
			if (Scripts.s_SoulMemoryCamera != null)
			{
				FullscreenCameraEffect component3 = Scripts.s_SoulMemoryCamera.GetComponent<FullscreenCameraEffect>();
				if (component3)
				{
					component3.FadeOut();
				}
			}
		}
	}
	[Script("Disable Fog Of War", "Scripts\\Fog of War")]
	public static void DisableFogOfWar()
	{
		if (FogOfWar.Instance != null)
		{
			FogOfWar.Instance.QueueDisable();
		}
	}
	[Script("Fog Of War Reveal All", "Scripts\\Fog of War")]
	public static void RevealAllFogOfWar()
	{
		if (FogOfWar.Instance != null)
		{
			FogOfWar.Instance.QueueRevealAll();
		}
	}
	[Script("Point Of No Return Save", "Scripts\\General")]
	public static void PointOfNoReturnSave()
	{
		if (!GameState.Mode.TrialOfIron)
		{
			GameResources.SaveGame(UISaveLoadManager.GetPointOfNoReturnSaveFileName());
		}
	}
	[Script("Game Complete Save", "Scripts\\General")]
	public static void GameCompleteSave()
	{
		try
		{
			GameState.GameComplete = true;
			GameResources.SaveGame(UISaveLoadManager.GetGameCompleteSaveFileName());
		}
		finally
		{
			GameState.GameComplete = false;
		}
	}
	[Script("Set Global", "Scripts\\Globals"), ScriptParam0("Name", "Global Name", "GlobalName", Scripts.BrowserType.GlobalVariable), ScriptParam1("Value", "Global Value", "0")]
	public static void SetGlobalValue(string name, int globalValue)
	{
		GlobalVariables.Instance.SetVariable(name, globalValue);
	}
	[Script("Increment Global", "Scripts\\Globals"), ScriptParam0("Name", "Global Name", "GlobalName", Scripts.BrowserType.GlobalVariable), ScriptParam1("Value", "Increase Global by Value", "1")]
	public static void IncrementGlobalValue(string name, int globalValue)
	{
		int variable = GlobalVariables.Instance.GetVariable(name);
		GlobalVariables.Instance.SetVariable(name, variable + globalValue);
	}
	[Script("Randomize Global", "Scripts\\Globals"), ScriptParam0("Name", "Global Name", "GlobalName", Scripts.BrowserType.GlobalVariable), ScriptParam1("MinValue", "Minimum Value for Global", "1"), ScriptParam2("MaxValue", "Maximum Value for Global", "2")]
	public static void RandomizeGlobalValue(string name, int minValue, int maxValue)
	{
		int val = UnityEngine.Random.Range(minValue, maxValue + 1);
		GlobalVariables.Instance.SetVariable(name, val);
	}
	[Script("Randomize Global With Global", "Scripts\\Globals"), ScriptParam0("Name", "Global Name", "GlobalName", Scripts.BrowserType.GlobalVariable), ScriptParam1("MinValueGlobal", "Minimum Value for Global", "GlobalName", Scripts.BrowserType.GlobalVariable), ScriptParam2("MaxValueGlobal", "Maximum Value for Global", "GlobalName", Scripts.BrowserType.GlobalVariable)]
	public static void RandomizeGlobalValueWithGlobal(string name, string minValue, string maxValue)
	{
		int val = UnityEngine.Random.Range(GlobalVariables.Instance.GetVariable(minValue), GlobalVariables.Instance.GetVariable(maxValue) + 1);
		GlobalVariables.Instance.SetVariable(name, val);
	}
	public static Health GetHealthComponent(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			Health component = objectByID.GetComponent<Health>();
			if (component)
			{
				return component;
			}
			UnityEngine.Debug.LogWarning(objectGuid + " doesn't have a health component.", objectByID);
		}
		UnityEngine.Debug.LogWarning(objectGuid + " could not be found when searching for health component.", null);
		return null;
	}
	public static CharacterStats GetCharacterStatsComponent(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			CharacterStats component = objectByID.GetComponent<CharacterStats>();
			if (component)
			{
				return component;
			}
			UnityEngine.Debug.LogWarning(objectGuid + " doesn't have a characterstats component.", objectByID);
		}
		UnityEngine.Debug.LogWarning(objectGuid + " could not be found when searching for characterstats component.", null);
		return null;
	}
	[Script("Set Health", "Scripts\\Health"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Health Value", "Set current health to this value", "100.0")]
	public static void SetHealth(Guid objectGuid, float healthValue)
	{
		Health healthComponent = Scripts.GetHealthComponent(objectGuid);
		if (healthComponent)
		{
			if (healthValue > healthComponent.MaxHealth)
			{
				healthValue = healthComponent.MaxHealth;
			}
			healthComponent.AddHealth(healthValue - healthComponent.CurrentHealth);
		}
	}
	[Script("Set Stamina", "Scripts\\Health"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Stamina Value", "Set current stamina to this value", "100.0")]
	public static void SetStamina(Guid objectGuid, float staminaValue)
	{
		Health healthComponent = Scripts.GetHealthComponent(objectGuid);
		if (healthComponent)
		{
			if (staminaValue > healthComponent.MaxStamina)
			{
				staminaValue = healthComponent.MaxStamina;
			}
			healthComponent.AddStamina(staminaValue - healthComponent.CurrentStamina);
		}
	}
	[Script("Set Fatigue", "Scripts\\Health"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Fatigue Value", "Set current fatigue time to this value (in hours)", "24.0")]
	public static void SetFatigue(Guid objectGuid, float fatigueValue)
	{
		CharacterStats characterStatsComponent = Scripts.GetCharacterStatsComponent(objectGuid);
		if (characterStatsComponent)
		{
			characterStatsComponent.Fatigue = fatigueValue * 60f * 60f;
		}
	}
	[Script("Add Fatigue", "Scripts\\Health"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Fatigue Value", "Add this value to current fatigue time (in hours)", "24.0")]
	public static void AddFatigue(Guid objectGuid, float fatigueValue)
	{
		CharacterStats characterStatsComponent = Scripts.GetCharacterStatsComponent(objectGuid);
		if (characterStatsComponent)
		{
			characterStatsComponent.Fatigue += fatigueValue * 60f * 60f;
		}
	}
	[Script("Add Fatigue To Party", "Scripts\\Health"), ScriptParam0("Fatigue Value", "Add this value to current fatigue time (in hours)", "24.0")]
	public static void AddFatigueToParty(float fatigueValue)
	{
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		for (int i = 0; i < partyMembers.Length; i++)
		{
			PartyMemberAI partyMemberAI = partyMembers[i];
			if (!(partyMemberAI == null))
			{
				CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
				if (!(component == null))
				{
					component.Fatigue += fatigueValue * 60f * 60f;
				}
			}
		}
	}
	[Script("Add Fatigue To Slot", "Scripts\\Health"), ScriptParam0("Fatigue Value", "Add this value to current fatigue time (in hours)", "24.0")]
	public static void AddFatigueToSlot(float fatigueValue, int slot)
	{
		PartyMemberAI partyMemberAtFormationIndex = PartyMemberAI.GetPartyMemberAtFormationIndex(slot);
		if (partyMemberAtFormationIndex)
		{
			CharacterStats component = partyMemberAtFormationIndex.GetComponent<CharacterStats>();
			if (component == null)
			{
				return;
			}
			component.Fatigue += fatigueValue * 60f * 60f;
		}
		else
		{
			UnityEngine.Debug.LogError("AddFatigueToSlot Scripting Error: No party member found at slot " + slot);
		}
	}
	[Script("Add Fatigue To Party With Skill Check", "Scripts\\Health"), ScriptParam0("Fatigue Value", "Add this value to current fatigue time (in hours)", "24.0"), ScriptParam1("Skill Type", "Skill to test.", "Stealth"), ScriptParam2("Skill Operator", "Comparison operator.", "EqualTo"), ScriptParam3("Skill Value", "Compare the object's skill against this value.", "0")]
	public static void AddFatigueToPartyWithSkillCheck(float fatigueValue, CharacterStats.SkillType skillType, Conditionals.Operator comparisonOperator, int skillValue)
	{
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		for (int i = 0; i < partyMembers.Length; i++)
		{
			PartyMemberAI partyMemberAI = partyMembers[i];
			if (!(partyMemberAI == null) && !partyMemberAI.Secondary)
			{
				CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
				if (!(component == null))
				{
					if (Conditionals.CompareInt(component.CalculateSkill(skillType), skillValue, comparisonOperator))
					{
						component.Fatigue += fatigueValue * 60f * 60f;
					}
				}
			}
		}
	}
	[Script("Heal Party", "Scripts\\Health")]
	public static void HealParty()
	{
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		for (int i = 0; i < partyMembers.Length; i++)
		{
			PartyMemberAI partyMemberAI = partyMembers[i];
			if (!(partyMemberAI == null))
			{
				Health component = partyMemberAI.GetComponent<Health>();
				if (component)
				{
					if (component.Unconscious)
					{
						component.OnRevive();
					}
					component.AddHealth(component.MaxHealth - component.CurrentHealth);
					component.AddStamina(component.MaxStamina - component.CurrentStamina);
				}
			}
		}
	}
	[Script("Set Invunerable", "Scripts\\Health"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Invunerable", "Sets the venerability of the object", "true")]
	public static void SetInvunerable(Guid objectGuid, bool invunerable)
	{
		Health healthComponent = Scripts.GetHealthComponent(objectGuid);
		if (healthComponent)
		{
			healthComponent.TakesDamage = invunerable;
		}
	}
	[Script("Set Prevent Death", "Scripts\\Health"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Prevent Death", "Sets the death prevention of the object", "true")]
	public static void SetPreventDeath(Guid objectGuid, bool preventDeath)
	{
		CharacterStats characterStatsComponent = Scripts.GetCharacterStatsComponent(objectGuid);
		if (characterStatsComponent)
		{
			if (preventDeath)
			{
				characterStatsComponent.DeathPrevented++;
			}
			else
			{
				characterStatsComponent.DeathPrevented--;
			}
		}
	}
	[Script("Kill", "Scripts\\Health"), ScriptParam0("Object", "Object to kill.", "", Scripts.BrowserType.ObjectGuid)]
	public static void Kill(Guid objectGuid)
	{
		Health healthComponent = Scripts.GetHealthComponent(objectGuid);
		CharacterStats characterStatsComponent = Scripts.GetCharacterStatsComponent(objectGuid);
		if (healthComponent)
		{
			if (characterStatsComponent)
			{
				characterStatsComponent.ApplyAffliction(AfflictionData.Maimed);
			}
			healthComponent.CanBeTargeted = true;
			healthComponent.ShouldDecay = true;
			healthComponent.ApplyHealthChangeDirectly(-healthComponent.CurrentHealth * 10f, false);
			healthComponent.ApplyDamageDirectly(healthComponent.MaxStamina * 100f);
		}
	}
	[Script("Deal Damage", "Scripts\\Health"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Damage", "Deals this amount of damage to the object (armor/defense is applied to calculation)", "10.0")]
	public static void DealDamage(Guid objectGuid, float damage)
	{
		Health healthComponent = Scripts.GetHealthComponent(objectGuid);
		if (healthComponent)
		{
			bool flag = !healthComponent.CanBeTargeted;
			healthComponent.CanBeTargeted = true;
			healthComponent.ApplyDamageDirectly(damage);
			if (flag)
			{
				healthComponent.CanBeTargeted = false;
			}
		}
	}
	[Script("Apply Affliction", "Scripts\\Health"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Affliction", "Applies this affliction to the object", "")]
	public static void ApplyAffliction(Guid objectGuid, string affliction)
	{
		CharacterStats characterStatsComponent = Scripts.GetCharacterStatsComponent(objectGuid);
		if (characterStatsComponent != null)
		{
			Affliction affliction2 = AfflictionData.FindAfflictionForScript(affliction);
			if (affliction2 != null)
			{
				characterStatsComponent.ApplyAffliction(affliction2);
			}
		}
	}
	[Script("Apply Affliciton To Party Member Slot #", "Scripts\\Health"), ScriptParam0("Affliction", "Applies this affliction to the object", ""), ScriptParam1("Slot #", "Party member slot # to apply affliction to", "")]
	public static void ApplyAfflictionToPartyMember(string affliction, int index)
	{
		Affliction affliction2 = AfflictionData.FindAfflictionForScript(affliction);
		if (affliction2 == null)
		{
			UnityEngine.Debug.LogWarning(affliction + " could not be found.", null);
			return;
		}
		if (index < PartyMemberAI.PartyMembers.Length)
		{
			CharacterStats component = PartyMemberAI.PartyMembers[index].GetComponent<CharacterStats>();
			if (component == null)
			{
				UnityEngine.Debug.LogWarning("Party member index could not be found");
			}
			else
			{
				component.ApplyAffliction(affliction2);
			}
		}
	}
	[Script("Remove Affliction", "Scripts\\Health"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Affliction", "Removes this affliction from the object", "")]
	public static void RemoveAffliction(Guid objectGuid, string affliction)
	{
		CharacterStats characterStatsComponent = Scripts.GetCharacterStatsComponent(objectGuid);
		if (characterStatsComponent != null)
		{
			Affliction affliction2 = AfflictionData.FindAfflictionForScript(affliction);
			if (affliction2 != null)
			{
				characterStatsComponent.ClearEffectFromAffliction(affliction2);
			}
		}
	}
	[Script("Apply Affliction To Party With Skill Check", "Scripts\\Health"), ScriptParam0("Affliction", "Applies this affliction to the object", ""), ScriptParam1("Skill Type", "Skill to test.", "Stealth"), ScriptParam2("Skill Operator", "Comparison operator.", "EqualTo"), ScriptParam3("Skill Value", "Compare the object's skill against this value.", "0")]
	public static void ApplyAfflictionToPartyWithSkillCheck(string affliction, CharacterStats.SkillType skillType, Conditionals.Operator comparisonOperator, int skillValue)
	{
		Affliction affliction2 = AfflictionData.FindAfflictionForScript(affliction);
		if (affliction2 == null)
		{
			UnityEngine.Debug.LogWarning(affliction + " could not be found.", null);
			return;
		}
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		for (int i = 0; i < partyMembers.Length; i++)
		{
			PartyMemberAI partyMemberAI = partyMembers[i];
			if (!(partyMemberAI == null) && !partyMemberAI.Secondary)
			{
				CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
				if (!(component == null))
				{
					if (Conditionals.CompareInt(component.CalculateSkill(skillType), skillValue, comparisonOperator))
					{
						component.ApplyAffliction(affliction2);
					}
				}
			}
		}
	}
	[Script("Apply Affliction To Worst Party Member", "Scripts\\Health"), ScriptParam0("Affliction", "Applies this affliction to the object", ""), ScriptParam1("Skill Type", "Skill to test.", "Stealth")]
	public static void ApplyAfflictionToWorstPartyMember(string affliction, CharacterStats.SkillType skillType)
	{
		Affliction affliction2 = AfflictionData.FindAfflictionForScript(affliction);
		if (affliction2 == null)
		{
			UnityEngine.Debug.LogWarning(affliction + " could not be found.", null);
			return;
		}
		int num = 2147483647;
		CharacterStats characterStats = null;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		for (int i = 0; i < partyMembers.Length; i++)
		{
			PartyMemberAI partyMemberAI = partyMembers[i];
			if (!(partyMemberAI == null) && !partyMemberAI.Secondary)
			{
				CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
				if (!(component == null))
				{
					int num2 = component.CalculateSkill(skillType);
					if (num2 < num)
					{
						characterStats = component;
						num = num2;
					}
				}
			}
		}
		if (characterStats != null)
		{
			characterStats.ApplyAffliction(affliction2);
		}
	}
	[Script("Set Affliction To Best Party Member", "Scripts\\Health"), ScriptParam0("Affliction", "Applies this affliction to the object", ""), ScriptParam1("Skill Type", "Skill to test.", "Stealth")]
	public static void ApplyAfflictionToBestPartyMember(string affliction, CharacterStats.SkillType skillType)
	{
		Affliction affliction2 = AfflictionData.FindAfflictionForScript(affliction);
		if (affliction2 == null)
		{
			UnityEngine.Debug.LogWarning(affliction + " could not be found.", null);
			return;
		}
		int num = -2147483648;
		CharacterStats characterStats = null;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		for (int i = 0; i < partyMembers.Length; i++)
		{
			PartyMemberAI partyMemberAI = partyMembers[i];
			if (!(partyMemberAI == null) && !partyMemberAI.Secondary)
			{
				CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
				if (!(component == null))
				{
					int num2 = component.CalculateSkill(skillType);
					if (num2 > num)
					{
						characterStats = component;
						num = num2;
					}
				}
			}
		}
		if (characterStats != null)
		{
			characterStats.ApplyAffliction(affliction2);
		}
	}
	[Script("Set Skill Check Token", "Scripts\\Health"), ScriptParam0("Skill Type", "Skill to test.", "Stealth"), ScriptParam1("Skill Operator", "Comparison operator.", "EqualTo"), ScriptParam2("Skill Value", "Compare the object's skill against this value.", "0")]
	public static void SetSkillCheckToken(CharacterStats.SkillType skillType, Conditionals.Operator comparisonOperator, int skillValue)
	{
		int num = 0;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		for (int i = 0; i < partyMembers.Length; i++)
		{
			PartyMemberAI partyMemberAI = partyMembers[i];
			if (!(partyMemberAI == null) && !partyMemberAI.Secondary)
			{
				CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
				if (!(component == null))
				{
					if (Conditionals.CompareInt(component.CalculateSkill(skillType), skillValue, comparisonOperator))
					{
						PartyMemberAI.SkillCheckTokens[num] = partyMemberAI;
						num++;
						if (num >= 30)
						{
							return;
						}
					}
				}
			}
		}
	}
	[Script("Set Skill Check Token With Worst Check", "Scripts\\Health"), ScriptParam0("Skill Type", "Skill to test.", "Stealth")]
	public static void SetSkillCheckTokenWorst(CharacterStats.SkillType skillType)
	{
		int num = 2147483647;
		PartyMemberAI partyMemberAI = null;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		for (int i = 0; i < partyMembers.Length; i++)
		{
			PartyMemberAI partyMemberAI2 = partyMembers[i];
			if (!(partyMemberAI2 == null) && !partyMemberAI2.Secondary)
			{
				CharacterStats component = partyMemberAI2.GetComponent<CharacterStats>();
				if (!(component == null))
				{
					int num2 = component.CalculateSkill(skillType);
					if (num2 < num)
					{
						partyMemberAI = partyMemberAI2;
						num = num2;
					}
				}
			}
		}
		if (partyMemberAI != null)
		{
			PartyMemberAI.SkillCheckTokens[0] = partyMemberAI;
		}
	}
	[Script("Set Skill Check Token With Best Check", "Scripts\\Health"), ScriptParam0("Skill Type", "Skill to test.", "Stealth")]
	public static void SetSkillCheckTokenBest(CharacterStats.SkillType skillType)
	{
		int num = -2147483648;
		PartyMemberAI partyMemberAI = null;
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		for (int i = 0; i < partyMembers.Length; i++)
		{
			PartyMemberAI partyMemberAI2 = partyMembers[i];
			if (!(partyMemberAI2 == null) && !partyMemberAI2.Secondary)
			{
				CharacterStats component = partyMemberAI2.GetComponent<CharacterStats>();
				if (!(component == null))
				{
					int num2 = component.CalculateSkill(skillType);
					if (num2 > num)
					{
						partyMemberAI = partyMemberAI2;
						num = num2;
					}
				}
			}
		}
		if (partyMemberAI != null)
		{
			PartyMemberAI.SkillCheckTokens[0] = partyMemberAI;
		}
	}
	[Script("Remove Item", "Scripts\\Items"), ScriptParam0("Item Name", "The name of the item to remove", "ItemName")]
	public static void RemoveItem(string itemName)
	{
		Item item = GameResources.LoadPrefab<Item>(itemName, false);
		string name = item.Name;
		foreach (PartyMemberAI current in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			Inventory inventory = current.Inventory;
			if (inventory != null && inventory.DestroyItem(itemName, 1) == 0)
			{
				Scripts.LogItemRemove(name, 1);
				return;
			}
		}
		UnityEngine.Debug.LogWarning("Remove Item script call: tried to remove " + itemName + " from party but removal failed.");
	}
	[Script("Remove Item from NPC", "Scripts\\Items"), ScriptParam0("NPC", "Remove item from this NPC.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Item Name", "The name of the item to remove", "ItemName")]
	public static void RemoveItemFromNPC(Guid objectGuid, string itemName)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		Equipment component = objectByID.GetComponent<Equipment>();
		if (component)
		{
			Equippable.EquipmentSlot equipmentSlot = component.CurrentItems.FindSlot(itemName);
			if (equipmentSlot != Equippable.EquipmentSlot.None)
			{
				Scripts.RemoveItemInSlot(objectGuid, equipmentSlot);
			}
		}
		Inventory component2 = objectByID.GetComponent<Inventory>();
		if (component2)
		{
			int num = component2.DestroyItem(itemName, 1);
			if (num > 0)
			{
				UnityEngine.Debug.LogWarning(string.Concat(new string[]
				{
					"Remove Item from NPC script call: tried to remove ",
					itemName,
					" from ",
					objectByID.name,
					" but removal failed."
				}));
			}
		}
	}
	[Script("Give Item By Name", "Scripts\\Items"), ScriptParam0("Item Name", "The name of the item to give", "ItemName"), ScriptParam1("Count", "The number of items to add", "1")]
	public static void GiveItem(string itemName, int count)
	{
		PlayerInventory component = GameState.s_playerCharacter.GetComponent<PlayerInventory>();
		if (component)
		{
			Item item = GameResources.LoadPrefab<Item>(itemName, false);
			if (item)
			{
				component.AddItemAndLog(item, count, null);
			}
			else
			{
				UnityEngine.Debug.LogError(string.Concat(new object[]
				{
					"Give Item By Name script call: Tried to give ",
					count,
					" of item '",
					itemName,
					"' but couldn't find that item."
				}));
			}
		}
		else
		{
			UnityEngine.Debug.LogError(string.Concat(new object[]
			{
				"Give Item By Name script call: Tried to give ",
				count,
				" of item ",
				itemName,
				" but couldn't find player inventory."
			}));
		}
	}
	[Script("Give Item And Equip", "Scripts\\Items"), ScriptParam0("NPC", "NPC to give the item to.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Item Name", "The name of the item to give", "ItemName"), ScriptParam2("Primary", "Equip the item in the primary slot (left hand).", "false")]
	public static void GiveItemAndEquip(Guid npcGuid, string itemName, bool primary)
	{
		GameObject objectByID = InstanceID.GetObjectByID(npcGuid);
		if (objectByID == null)
		{
			UnityEngine.Debug.LogError("GiveItemAndEquip Failed! Unable to find the specified guid!");
			return;
		}
		Equipment component = objectByID.GetComponent<Equipment>();
		if (component)
		{
			Equippable equippable = GameResources.LoadPrefab<Equippable>(itemName, true);
			if (equippable)
			{
				Equippable equippable2 = GameResources.Instantiate<Equippable>(equippable);
				equippable2.Prefab = equippable;
				component.Equip(equippable2, (!primary) ? Equippable.EquipmentSlot.SecondaryWeapon : Equippable.EquipmentSlot.PrimaryWeapon, false);
			}
			else
			{
				UnityEngine.Debug.LogError("GiveItemAndEquip: Tried to give '" + itemName + "' but couldn't find that item OR the item wasn't equippable.");
			}
		}
		else
		{
			UnityEngine.Debug.LogError("GiveItemAndEquip: Tried to give '" + itemName + " but couldn't find player inventory.");
		}
	}
	[Script("Give Item to NPC", "Scripts\\Items"), ScriptParam0("NPC", "NPC to give the item to.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Item Name", "The name of the item to give", "ItemName"), ScriptParam2("Count", "The number of items to add", "1")]
	public static void GiveItemToNPC(Guid objectGuid, string itemName, int count)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		Inventory component = objectByID.GetComponent<Inventory>();
		if (component)
		{
			Item item = GameResources.LoadPrefab<Item>(itemName, false);
			if (item)
			{
				component.AddItem(item, count);
			}
			else
			{
				UnityEngine.Debug.LogError(string.Concat(new object[]
				{
					"GiveItemToNPC script call: Tried to give ",
					count,
					" of item '",
					itemName,
					"' but couldn't find that item."
				}));
			}
		}
		else
		{
			UnityEngine.Debug.LogError(string.Concat(new object[]
			{
				"GiveItemToNPC script call: Tried to give ",
				count,
				" of item ",
				itemName,
				" but couldn't find the object's inventory."
			}));
		}
	}
	[Script("Remove Item Stack", "Scripts\\Items"), ScriptParam0("Item Name", "The name of the item to remove", "ItemName"), ScriptParam1("Count", "The number of items to remove", "1")]
	public static void RemoveItemStack(string itemName, int count)
	{
		Item item = GameResources.LoadPrefab<Item>(itemName, false);
		string name = item.Name;
		int num = count;
		foreach (PartyMemberAI current in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			Inventory inventory = current.Inventory;
			if (inventory != null && (num = inventory.DestroyItem(itemName, num)) == 0)
			{
				Scripts.LogItemRemove(name, count);
				return;
			}
		}
		UnityEngine.Debug.LogWarning(string.Concat(new object[]
		{
			"Remove Item Stack script call: tried to remove ",
			count,
			" of ",
			itemName,
			" but only found ",
			count - num,
			" of those."
		}));
		Scripts.LogItemRemove(name, count - num);
	}
	[Script("Remove Item Stack From NPC", "Scripts\\Items"), ScriptParam0("NPC", "NPC to lose item.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Item Name", "The name of the item to remove", "ItemName"), ScriptParam2("Count", "The number of items to remove", "1")]
	public static void RemoveItemStackFromNPC(Guid objectGuid, string itemName, int count)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			UnityEngine.Debug.LogError(string.Concat(new object[]
			{
				"Remove Item Stack From NPC script call: Tried to remove ",
				count,
				" of item ",
				itemName,
				" but couldn't find NPC."
			}));
			return;
		}
		Inventory component = objectByID.GetComponent<Inventory>();
		Item x = GameResources.LoadPrefab<Item>(itemName, false);
		int num = count;
		if (component != null && x != null && (num = component.DestroyItem(itemName, num)) == 0)
		{
			return;
		}
		UnityEngine.Debug.LogWarning(string.Concat(new object[]
		{
			"Remove Item Stack script call: tried to remove ",
			count,
			" of ",
			itemName,
			" from ",
			objectByID.name,
			" but only found ",
			count - num,
			" of those."
		}));
	}
	[Script("Unequip Item in Slot", "Scripts\\Items"), ScriptParam0("Character", "Character to unequip.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Slot", "Equipment Slot to unequip", "None")]
	public static void UnequipItemInSlot(Guid objectGuid, Equippable.EquipmentSlot slot)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		Equipment component = objectByID.GetComponent<Equipment>();
		if (component == null)
		{
			return;
		}
		Equippable itemInSlot = component.CurrentItems.GetItemInSlot(slot);
		if (itemInSlot == null)
		{
			return;
		}
		Equippable equippable = component.UnEquip(itemInSlot);
		if (equippable)
		{
			PartyMemberAI component2 = objectByID.GetComponent<PartyMemberAI>();
			if (component2 != null && component2.gameObject.activeInHierarchy)
			{
				PlayerInventory component3 = GameState.s_playerCharacter.GetComponent<PlayerInventory>();
				if (component3 != null && component3.AddItem(equippable, 1) != 0)
				{
					UnityEngine.Debug.LogWarning("Inventory Item Stack Space is incorrect. We couldn't add back an item we just removed.");
				}
			}
			else
			{
				Inventory component4 = objectByID.GetComponent<Inventory>();
				if (component4 != null && component4.AddItem(equippable, 1) != 0)
				{
					UnityEngine.Debug.LogWarning("Inventory Item Stack Space is incorrect. We couldn't add back an item we just removed.");
				}
			}
		}
		NPCAppearance component5 = objectByID.GetComponent<NPCAppearance>();
		if (component5)
		{
			component5.Generate();
		}
	}
	[Script("Get Player Money", "Scripts\\Items")]
	public static int GetPlayerMoney()
	{
		PlayerInventory component = GameState.s_playerCharacter.GetComponent<PlayerInventory>();
		if (component)
		{
			return (int)component.currencyTotalValue.v;
		}
		return 0;
	}
	[Script("Give Player Money", "Scripts\\Items"), ScriptParam0("Amount", "The amount of money to add to the player", "1")]
	public static void GivePlayerMoney(int amount)
	{
		PlayerInventory component = GameState.s_playerCharacter.GetComponent<PlayerInventory>();
		if (component)
		{
			component.currencyTotalValue.v += (float)amount;
			global::Console.AddMessage("[00FF00]" + GUIUtils.FormatWithLinks(295, new object[]
			{
				amount
			}), global::Console.ConsoleState.BOTH);
			if (GlobalAudioPlayer.Instance)
			{
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ReceiveGold);
			}
		}
	}
	[Script("Remove Player Money", "Scripts\\Items"), ScriptParam0("Amount", "The amount of money to remove from the player", "1")]
	public static void RemovePlayerMoney(int amount)
	{
		PlayerInventory component = GameState.s_playerCharacter.GetComponent<PlayerInventory>();
		if (component)
		{
			if (component.currencyTotalValue.v >= (float)amount)
			{
				global::Console.AddMessage("[00FF00]" + GUIUtils.FormatWithLinks(296, new object[]
				{
					amount
				}), global::Console.ConsoleState.BOTH);
				component.currencyTotalValue.v -= (float)amount;
			}
			else
			{
				global::Console.AddMessage("[00FF00]" + GUIUtils.FormatWithLinks(296, new object[]
				{
					component.currencyTotalValue.v
				}), global::Console.ConsoleState.BOTH);
				component.currencyTotalValue.v = 0f;
			}
		}
	}
	public static void LogItemGet(Item item, int quantity, bool stashed)
	{
		if (!item.IsQuestItem && !item.IsRedirectIngredient)
		{
			if (quantity != 1)
			{
				global::Console.AddMessage("[00FF00]" + GUIUtils.FormatWithLinks(299, new object[]
				{
					item.Name,
					quantity
				}), global::Console.ConsoleState.BOTH);
			}
			else
			{
				global::Console.AddMessage("[00FF00]" + GUIUtils.FormatWithLinks(297, new object[]
				{
					item.Name
				}), global::Console.ConsoleState.BOTH);
			}
		}
		if (stashed)
		{
			global::Console.AddMessage("[00FF00]" + GUIUtils.GetTextWithLinks(1327), global::Console.ConsoleState.BOTH);
		}
	}
	public static void LogItemRemove(string itemDisplayName, int quantity)
	{
		if (quantity != 1)
		{
			global::Console.AddMessage("[00FF00]" + GUIUtils.FormatWithLinks(300, new object[]
			{
				itemDisplayName,
				quantity
			}), global::Console.ConsoleState.BOTH);
		}
		else
		{
			global::Console.AddMessage("[00FF00]" + GUIUtils.FormatWithLinks(298, new object[]
			{
				itemDisplayName
			}), global::Console.ConsoleState.BOTH);
		}
	}
	[Script("Log Given Recipe", "Scripts\\Items"), ScriptParam0("Recipe Name", "The name of the recipe learned", "RecipeName")]
	public static void LogRecipeGet(string recipeName)
	{
		Recipe recipe = GameResources.LoadPrefab<Recipe>(recipeName, false);
		if (recipe)
		{
			global::Console.AddMessage("[00FF00]" + GUIUtils.FormatWithLinks(1660, new object[]
			{
				recipe.DisplayName
			}), global::Console.ConsoleState.BOTH);
		}
	}
	[Script("Remove Item in Slot", "Scripts\\Items"), ScriptParam0("Character", "Remove the item from this character.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Slot", "Equipment Slot to remove", "None")]
	public static void RemoveItemInSlot(Guid objectGuid, Equippable.EquipmentSlot slot)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID == null)
		{
			return;
		}
		Equipment component = objectByID.GetComponent<Equipment>();
		if (component == null)
		{
			return;
		}
		Equippable itemInSlot = component.CurrentItems.GetItemInSlot(slot);
		if (itemInSlot == null)
		{
			return;
		}
		Equippable equippable = component.UnEquip(itemInSlot);
		if (equippable == null)
		{
			return;
		}
		PersistenceManager.RemoveObject(equippable.GetComponent<Persistence>());
		GameUtilities.Destroy(equippable.gameObject);
		NPCAppearance component2 = objectByID.GetComponent<NPCAppearance>();
		if (component2)
		{
			component2.Generate();
		}
	}
	[Script("Lock Equipment Slot", "Scripts\\Testing"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Slot", "Equipment Slot to lock", "None")]
	public static void LockEquipmentSlot(Guid objectGuid, Equippable.EquipmentSlot slot)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!objectByID)
		{
			return;
		}
		Equipment component = objectByID.GetComponent<Equipment>();
		if (!component)
		{
			return;
		}
		component.LockSlot(slot);
	}
	[Script("Unlock Equipment Slot", "Scripts\\Testing"), ScriptParam0("Object", "Object to modify.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Slot", "Equipment Slot to unlock", "None")]
	public static void UnlockEquipmentSlot(Guid objectGuid, Equippable.EquipmentSlot slot)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (!objectByID)
		{
			return;
		}
		Equipment component = objectByID.GetComponent<Equipment>();
		if (!component)
		{
			return;
		}
		component.UnlockSlot(slot);
	}
	public static OCL GetOCLComponent(Guid objectGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			OCL component = objectByID.GetComponent<OCL>();
			if (component)
			{
				return component;
			}
			UnityEngine.Debug.LogWarning(objectGuid + " doesn't have a ocl component.", objectByID);
		}
		UnityEngine.Debug.LogWarning(objectGuid + " could not be found when searching for ocl component.", null);
		return null;
	}
	[Script("Open", "Scripts\\OCL"), ScriptParam0("Object", "Object to open.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Ignore Lock", "Should we ignore the lock to open", "false")]
	public static void Open(Guid objectGuid, bool ignoreLock)
	{
		OCL oCLComponent = Scripts.GetOCLComponent(objectGuid);
		if (oCLComponent)
		{
			oCLComponent.Open(null, ignoreLock);
		}
	}
	[Script("Seal Open", "Scripts\\OCL"), ScriptParam0("Object", "Object to open.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Ignore Lock", "Should we ignore the lock to open", "false")]
	public static void SealOpen(Guid objectGuid, bool ignoreLock)
	{
		OCL oCLComponent = Scripts.GetOCLComponent(objectGuid);
		if (oCLComponent)
		{
			oCLComponent.Open(null, ignoreLock);
			oCLComponent.SealOpen();
		}
	}
	[Script("Close", "Scripts\\OCL"), ScriptParam0("Object", "Object to close.", "", Scripts.BrowserType.ObjectGuid)]
	public static void Close(Guid objectGuid)
	{
		OCL oCLComponent = Scripts.GetOCLComponent(objectGuid);
		if (oCLComponent)
		{
			oCLComponent.Close(null);
		}
	}
	[Script("Lock", "Scripts\\OCL"), ScriptParam0("Object", "Name of the object", "", Scripts.BrowserType.ObjectGuid)]
	public static void Lock(Guid objectGuid)
	{
		OCL oCLComponent = Scripts.GetOCLComponent(objectGuid);
		if (oCLComponent)
		{
			oCLComponent.Lock(null);
		}
	}
	[Script("UnLock", "Scripts\\OCL"), ScriptParam0("Object", "Object to unlock.", "", Scripts.BrowserType.ObjectGuid)]
	public static void Unlock(Guid objectGuid)
	{
		OCL oCLComponent = Scripts.GetOCLComponent(objectGuid);
		if (oCLComponent)
		{
			oCLComponent.Unlock(null);
		}
	}
	[Script("Toggle Open", "Scripts\\OCL"), ScriptParam0("Object", "Object to toggle.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Ignore Lock", "Should we ignore the lock to open", "false")]
	public static void ToggleOpen(Guid objectGuid, bool ignoreLock)
	{
		OCL oCLComponent = Scripts.GetOCLComponent(objectGuid);
		if (oCLComponent)
		{
			oCLComponent.Toggle(null, ignoreLock);
		}
	}
	[Script("Toggle Lock", "Scripts\\OCL"), ScriptParam0("Object", "Object to toggle.", "", Scripts.BrowserType.ObjectGuid)]
	public static void ToggleLock(Guid objectGuid)
	{
		OCL oCLComponent = Scripts.GetOCLComponent(objectGuid);
		if (oCLComponent)
		{
			oCLComponent.ToggleLock(null);
		}
	}
	[Script("Start Quest", "Scripts\\Quest"), ScriptParam0("Quest Name", "The name of the quest.", "", Scripts.BrowserType.Quest)]
	public static void StartQuest(string questName)
	{
		QuestManager.Instance.StartQuest(questName, null);
	}
	[Script("Advance Quest", "Scripts\\Quest"), ScriptParam0("Quest Name", "The name of the quest.", "", Scripts.BrowserType.Quest)]
	public static void AdvanceQuest(string questName)
	{
		QuestManager.Instance.AdvanceQuest(questName);
	}
	[Script("Debug Advance Quest", "Scripts\\Quest"), ScriptParam0("Quest Name", "The name of the quest.", "", Scripts.BrowserType.Quest)]
	public static void DebugAdvanceQuest(string questName)
	{
		QuestManager.Instance.AdvanceQuest(questName, true);
	}
	[Script("Trigger Quest Addendum", "Scripts\\Quest"), ScriptParam0("Quest Name", "The name of the quest.", "", Scripts.BrowserType.Quest), ScriptParam1("Addendum ID", "The ID of the addendum to set.", "0")]
	public static void TriggerQuestAddendum(string questName, int addendumID)
	{
		QuestManager.Instance.TriggerQuestAddendum(questName, addendumID);
	}
	[Script("Trigger Quest End State", "Scripts\\Quest"), ScriptParam0("Quest Name", "The name of the quest.", "", Scripts.BrowserType.Quest), ScriptParam1("End State ID", "The ID of the end state to set.", "0")]
	public static void TriggerQuestEndState(string questName, int endStateID)
	{
		QuestManager.Instance.TriggerQuestEndState(questName, endStateID, false);
	}
	[Script("Trigger Quest Fail State", "Scripts\\Quest"), ScriptParam0("Quest Name", "The name of the quest.", "", Scripts.BrowserType.Quest), ScriptParam1("End State ID", "The ID of the end state to set.", "0")]
	public static void TriggerQuestFailState(string questName, int endStateID)
	{
		QuestManager.Instance.TriggerQuestEndState(questName, endStateID, true);
	}
	[Script("Add Talent", "Scripts\\RPG"), ScriptParam0("Talent Name", "The name of the talent to add to the player. Should be faster when provided an objectGuid.", "TalentName")]
	public static void AddTalent(string talentName)
	{
		CommandLine.AddAbility("player", talentName);
	}
	[Script("Add Talent", "Scripts\\RPG"), ScriptParam0("Target character", "The character desired to add talent/ability", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Talent Name", "The name of the talent to add to the character.", "TalentName")]
	public static void AddTalent(Guid objectGuid, string talentName)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		CommandLine.AddAbility(objectByID.GetComponent<CharacterStats>(), talentName);
	}
	[Script("Remove Talent", "Scripts\\RPG"), ScriptParam0("Target character", "The character desired to remove talent/ability", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Talent Name", "The name of the talent to remove from the character. Will also remove abilities.", "TalentName")]
	public static void RemoveTalent(Guid objectGuid, string talentName)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		CommandLine.RemoveAbility(objectByID, talentName);
	}
	[Script("Set Wants To Talk", "Scripts\\RPG"), ScriptParam0("Target Party Member", "The party member desired to show the flag", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Wants To Talk", "The new state for the conversation flag", "false")]
	public static void SetWantsToTalk(Guid objectGuid, bool wantsToTalk)
	{
		GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
		if (objectByID)
		{
			NPCDialogue component = objectByID.GetComponent<NPCDialogue>();
			if (component)
			{
				component.wantsToTalk = wantsToTalk;
			}
			else
			{
				UnityEngine.Debug.LogWarning(objectGuid + " doesn't have a PartyMemberAI component.", objectByID);
			}
		}
		else
		{
			UnityEngine.Debug.LogWarning(objectGuid + " could not be found!");
		}
	}
	[Script("Mark Conversation Node As Read", "Scripts\\Conversation"), ScriptParam0("Conversation", "Name of the conversation.", "", Scripts.BrowserType.Conversation), ScriptParam1("Conversation Node ID", "Conversation node ID.", "0")]
	public static void MarkConversationNodeAsRead(string conversation, int nodeID)
	{
		ConversationManager.Instance.SetMarkedAsRead(conversation, nodeID);
	}
	[Script("Clear Conversation Node As Read", "Scripts\\Conversation"), ScriptParam0("Conversation", "Name of the conversation.", "", Scripts.BrowserType.Conversation), ScriptParam1("Conversation Node ID", "Conversation node ID.", "0")]
	public static void ClearConversationNodeAsRead(string conversation, int nodeID)
	{
		ConversationManager.Instance.ClearMarkedAsRead(conversation, nodeID);
	}
	public static Vendor GetVendorComponent(Guid storeGuid)
	{
		GameObject objectByID = InstanceID.GetObjectByID(storeGuid);
		if (objectByID)
		{
			Vendor component = objectByID.GetComponent<Vendor>();
			if (component)
			{
				return component;
			}
			UnityEngine.Debug.LogWarning(storeGuid + " doesn't have a vendor component.", objectByID);
		}
		UnityEngine.Debug.LogWarning(storeGuid + " could not be found when searching for vendor component.", null);
		return null;
	}
	[Script("Open Store", "Scripts\\Stores"), ScriptParam0("Vendor", "Vendor object to open.", "", Scripts.BrowserType.ObjectGuid)]
	public static void OpenStore(Guid storeGuid)
	{
		Vendor vendorComponent = Scripts.GetVendorComponent(storeGuid);
		if (vendorComponent)
		{
			vendorComponent.OpenStore();
		}
	}
	[Script("Open Store with Rates", "Scripts\\Stores"), ScriptParam0("Vendor", "Vendor object to open.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Buy Rate", "Buy Rate (default 0.2)", "0.2"), ScriptParam2("Sell Rate", "Sell Rate (default 1.5)", "1.5")]
	public static void OpenStoreWithRates(Guid storeGuid, float buyRate, float sellRate)
	{
		Vendor vendorComponent = Scripts.GetVendorComponent(storeGuid);
		if (vendorComponent)
		{
			vendorComponent.OpenStore(buyRate, sellRate);
		}
	}
	[Script("Open Inn", "Scripts\\Stores"), ScriptParam0("Vendor", "Vendor object to open.", "", Scripts.BrowserType.ObjectGuid)]
	public static void OpenInn(Guid innGuid)
	{
		Vendor vendorComponent = Scripts.GetVendorComponent(innGuid);
		if (vendorComponent)
		{
			vendorComponent.OpenInn();
		}
	}
	[Script("Open Inn", "Scripts\\Stores"), ScriptParam0("Vendor", "Vendor object to open.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Rate", "Rate multiplier (default 1.0)", "1.0")]
	public static void OpenInnWithRate(Guid innGuid, float rate)
	{
		Vendor vendorComponent = Scripts.GetVendorComponent(innGuid);
		if (vendorComponent)
		{
			vendorComponent.OpenInn(rate);
		}
	}
	[Script("Set Vendor Rates", "Scripts\\Stores"), ScriptParam0("Vendor", "Vendor object to set.", "", Scripts.BrowserType.ObjectGuid), ScriptParam1("Buy Rate", "Buy Rate (default 0.2)", "0.2"), ScriptParam2("Sell Rate", "Sell Rate (default 1.5)", "1.5"), ScriptParam3("Inn Rate", "Inn Rate (default 1.0)", "1.0")]
	public static void SetVendorRates(Guid vendorGuid, float buyRate, float sellRate, float innRate)
	{
		Vendor vendorComponent = Scripts.GetVendorComponent(vendorGuid);
		if (vendorComponent)
		{
			Store component = vendorComponent.GetComponent<Store>();
			if (component)
			{
				component.buyMultiplier = buyRate;
				component.sellMultiplier = sellRate;
			}
			Inn component2 = vendorComponent.GetComponent<Inn>();
			if (component2)
			{
				component2.multiplier = innRate;
			}
		}
	}
	[Script("Open Recruitment", "Scripts\\Stores"), ScriptParam0("Vendor", "Vendor object to open.", "", Scripts.BrowserType.ObjectGuid)]
	public static void OpenRecruitment(Guid storeGuid)
	{
		Vendor vendorComponent = Scripts.GetVendorComponent(storeGuid);
		if (vendorComponent)
		{
			vendorComponent.OpenRecruitment();
		}
	}
	[Script("Activate Stronghold", "Scripts\\Stronghold")]
	public static void ActivateStronghold()
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null && !stronghold.Activated)
		{
			stronghold.ActivateStronghold(false);
		}
	}
	[Script("Disable Stronghold", "Scripts\\Stronghold")]
	public static void DisableStronghold()
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			stronghold.Disabled = true;
		}
	}
	[Script("Show Stronghold UI", "Scripts\\Stronghold"), ScriptParam0("Window", "Window To Show", "Status")]
	public static void ShowStrongholdUI(Stronghold.WindowPane window)
	{
		UIWindowManager.Instance.SuspendFor(UIStrongholdManager.Instance);
		UIStrongholdManager.Instance.ShowForPane = window;
		UIStrongholdManager.Instance.ShowWindow();
	}
	[Script("Add Prisoner", "Scripts\\Stronghold"), ScriptParam0("Object", "Prisoner object to add to the stronghold prison", "", Scripts.BrowserType.ObjectGuid)]
	public static void AddPrisoner(Guid objectGuid)
	{
		if (GameState.s_playerCharacter != null)
		{
			Stronghold stronghold = GameState.Stronghold;
			if (stronghold != null)
			{
				GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
				if (objectByID != null)
				{
					stronghold.AddPrisoner(objectByID);
				}
			}
		}
	}
	[Script("Remove Prisoner", "Scripts\\Stronghold"), ScriptParam0("Object", "Prisoner object to remove from the stronghold prison", "", Scripts.BrowserType.ObjectGuid)]
	public static void RemovePrisoner(Guid objectGuid)
	{
		if (GameState.s_playerCharacter != null)
		{
			Stronghold stronghold = GameState.Stronghold;
			if (stronghold != null)
			{
				GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
				if (objectByID != null)
				{
					stronghold.RemovePrisoner(objectByID);
				}
			}
		}
	}
	[Script("On Prisoner Death", "Scripts\\Stronghold"), ScriptParam0("Object", "Prisoner object who has been killed", "", Scripts.BrowserType.ObjectGuid)]
	public static void OnPrisonerDeath(Guid objectGuid)
	{
		Scripts.RemovePrisoner(objectGuid);
	}
	[Script("Show Prisoners", "Scripts\\Stronghold")]
	public static void ShowPrisoners()
	{
		if (GameState.s_playerCharacter != null)
		{
			Stronghold stronghold = GameState.Stronghold;
			if (stronghold != null)
			{
				int num = stronghold.PrisonerCount();
				global::Console.AddMessage("Stronghold prisoner count: " + num);
				for (int i = 0; i < num; i++)
				{
					global::Console.AddMessage(string.Concat(new object[]
					{
						"   ",
						num,
						". ",
						stronghold.PrisonerName(i)
					}));
				}
			}
		}
	}
	[Script("On Hireling Death", "Scripts\\Stronghold"), ScriptParam0("Object", "Hireling object who has been killed", "", Scripts.BrowserType.ObjectGuid)]
	public static void OnHirelingDeath(Guid objectGuid)
	{
		if (GameState.s_playerCharacter != null)
		{
			Stronghold stronghold = GameState.Stronghold;
			if (stronghold != null)
			{
				GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
				if (objectByID != null)
				{
					CharacterStats component = objectByID.GetComponent<CharacterStats>();
					if (component != null)
					{
						string name = component.Name();
						StrongholdHireling strongholdHireling = stronghold.FindHireling(name);
						if (strongholdHireling != null)
						{
							stronghold.OnHirelingDeath(strongholdHireling);
						}
					}
					Persistence component2 = objectByID.GetComponent<Persistence>();
					if (component2)
					{
						PersistenceManager.RemoveObject(component2);
						GameUtilities.DestroyComponentImmediate(component2);
					}
				}
			}
		}
	}
	[Script("On Visitor Death", "Scripts\\Stronghold"), ScriptParam0("Object", "Visitor object who has been killed", "", Scripts.BrowserType.ObjectGuid)]
	public static void OnVisitorDeath(Guid objectGuid)
	{
		if (GameState.s_playerCharacter != null)
		{
			Stronghold stronghold = GameState.Stronghold;
			if (stronghold != null)
			{
				GameObject objectByID = InstanceID.GetObjectByID(objectGuid);
				if (objectByID != null)
				{
					CharacterStats component = objectByID.GetComponent<CharacterStats>();
					if (component != null)
					{
						string name = component.Name();
						StrongholdVisitor strongholdVisitor = stronghold.FindVisitor(name);
						if (strongholdVisitor != null)
						{
							stronghold.OnVisitorDeath(strongholdVisitor);
						}
					}
				}
			}
		}
	}
	[Script("Adjust Security", "Scripts\\Stronghold"), ScriptParam0("Integer", "Amount to adjust stronghold Security", "0")]
	public static void AdjustSecurity(int adj)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			stronghold.Security += adj;
		}
	}
	[Script("Adjust Prestige", "Scripts\\Stronghold"), ScriptParam0("Integer", "Amount to adjust stronghold Prestige", "0")]
	public static void AdjustPrestige(int adj)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			stronghold.Prestige += adj;
		}
	}
	[Script("Add Turns", "Scripts\\Stronghold"), ScriptParam0("Integer", "Number of turns to add", "1")]
	public static void AddTurns(int amount)
	{
		Stronghold stronghold = GameState.Stronghold;
		if (stronghold != null)
		{
			stronghold.AddTurns(amount);
		}
	}
	[Script("Open Character Creation Screen", "Scripts\\UI")]
	public static void OpenCharacterCreation()
	{
		int num = 1;
		UICharacterCreationManager.Instance.OpenCharacterCreation(UICharacterCreationManager.CharacterCreationType.NewPlayer, GameState.s_playerCharacter.gameObject, 0, num, CharacterStats.ExperienceNeededForLevel(num));
	}
	[Script("Open Character Creation New Companion Screen", "Scripts\\UI"), ScriptParam0("Player Cost", "How much it will cost if the player completes this companion.", "0"), ScriptParam1("Ending Level", "What level the character will be.", "1")]
	public static void OpenCharacterCreationNewCompanion(int playerCost, int endingLevel)
	{
		GameObject file = GameResources.LoadPrefab<GameObject>(UICharacterCreationManager.Instance.NewCompanionPrefabString, false);
		GameObject gameObject = GameResources.Instantiate<GameObject>(file, GameState.s_playerCharacter.rigidbody.position, GameState.s_playerCharacter.rigidbody.rotation);
		PartyMemberAI component = gameObject.GetComponent<PartyMemberAI>();
		if (component)
		{
			component.enabled = false;
		}
		UICharacterCreationManager.Instance.OpenCharacterCreation(UICharacterCreationManager.CharacterCreationType.NewCompanion, gameObject, playerCost, 1, CharacterStats.ExperienceNeededForLevel(endingLevel));
		CharacterStats component2 = gameObject.GetComponent<CharacterStats>();
		component2.Experience = CharacterStats.ExperienceNeededForLevel(endingLevel);
	}
	[Script("Play Interstitial", "Scripts\\UI"), ScriptParam0("Index", "The index of an interstitial to play (see InterstitialMaster list).", "0")]
	public static void PlayInterstitial(int index)
	{
		if (UIInterstitialManager.Instance == null)
		{
			UnityEngine.Debug.LogError("PlayInterstitial: UI hasn't been given a chance to initialize yet.");
			return;
		}
		UIInterstitialManager.ForChapter = index;
		UIInterstitialManager.Instance.ShowWindow();
	}
	[Script("Open Crafting Window", "Scripts\\UI"), ScriptParam0("Crafting Object Type", "A string identifying a unique crafting location to show (Recipe.CraftingLocation)", "")]
	public static void OpenCrafting(string location)
	{
		if (UICraftingManager.Instance == null)
		{
			UnityEngine.Debug.LogError("OpenCrafting: UI hasn't been given a chance to initialize yet.");
			return;
		}
		if (!GameState.InCombat)
		{
			UIWindowManager.Instance.SuspendFor(UICraftingManager.Instance);
			UICraftingManager.Instance.EnchantMode = false;
			UICraftingManager.Instance.ForLocation = location;
			UICraftingManager.Instance.ShowWindow();
		}
	}
	[Script("Open Enchanting Window", "Scripts\\UI"), ScriptParam0("Crafting Object Type", "A string identifying a unique crafting location to show (Recipe.CraftingLocation)", ""), ScriptParam1("Target Item Name", "The name of the item to enchant.", "")]
	public static void OpenEnchanting(string location, string itemTarget)
	{
		if (UICraftingManager.Instance == null)
		{
			UnityEngine.Debug.LogError("OpenEnchanting: UI hasn't been given a chance to initialize yet.");
			return;
		}
		Item item = null;
		foreach (PartyMemberAI current in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (current)
			{
				BaseInventory[] components = current.GetComponents<BaseInventory>();
				BaseInventory[] array = components;
				for (int i = 0; i < array.Length; i++)
				{
					BaseInventory baseInventory = array[i];
					InventoryItem inventoryItem = baseInventory.ItemList.FirstOrDefault((InventoryItem ii) => ii.NameEquals(itemTarget));
					if (inventoryItem != null)
					{
						item = inventoryItem.baseItem;
					}
					if (item)
					{
						break;
					}
				}
				Equipment component = current.GetComponent<Equipment>();
				if (component)
				{
					Ref<Equippable> @ref = component.CurrentItems.Slots.FirstOrDefault((Ref<Equippable> re) => re.Val && re.Val.Prefab.name.Equals(itemTarget));
					if (@ref != null)
					{
						item = @ref.Val;
					}
					if (item)
					{
						break;
					}
					WeaponSet weaponSet = component.WeaponSets.FirstOrDefault((WeaponSet ws) => ws.PrimaryWeapon && ws.PrimaryWeapon.Prefab.name.Equals(itemTarget));
					if (weaponSet != null)
					{
						item = weaponSet.PrimaryWeapon;
					}
					if (item)
					{
						break;
					}
					weaponSet = component.WeaponSets.FirstOrDefault((WeaponSet ws) => ws.SecondaryWeapon && ws.SecondaryWeapon.Prefab.name.Equals(itemTarget));
					if (weaponSet != null)
					{
						item = weaponSet.SecondaryWeapon;
					}
					if (item)
					{
						break;
					}
				}
			}
		}
		if (item)
		{
			UIWindowManager.Instance.SuspendFor(UICraftingManager.Instance);
			UICraftingManager.Instance.EnchantMode = true;
			UICraftingManager.Instance.ForLocation = location;
			UICraftingManager.Instance.EnchantTarget = item;
			UICraftingManager.Instance.ShowWindow();
		}
	}
	[Script("Return To Main Menu", "Scripts\\UI")]
	public static void ReturnToMainMenu()
	{
		Screen.showCursor = false;
		InGameUILayout inGameUILayout = UnityEngine.Object.FindObjectOfType<InGameUILayout>();
		InGameHUD.Instance.ShowHUD = false;
		inGameUILayout.StartCoroutine(Scripts.EndDemo());
	}
	[DebuggerHidden]
	public static IEnumerator EndDemo()
	{
		return new Scripts.<EndDemo>c__Iterator55();
	}
	[Script("Trigger Tutorial", "Scripts\\UI"), ScriptParam0("Tutorial Index", "The index in the TutorialMaster list of the tutorial to play.", "0")]
	public static void TriggerTutorial(int index)
	{
		TutorialManager.STriggerTutorial(index);
	}
	[Script("Set End Game Slide", "Scripts\\UI"), ScriptParam0("Image Index", "The index in the EndGameSlidesImages array.", "0")]
	public static void SetEndGameSlide(int imageIndex)
	{
		UIEndGameSlidesManager.Instance.SetImage(imageIndex);
	}
	[Script("Begin Watcher Movie", "Scripts\\UI")]
	public static void BeginWatcherMovie()
	{
		UIConversationWatcherMovie.Instance.Show();
	}
	[Script("End Watcher Movie", "Scripts\\UI")]
	public static void EndWatcherMovie()
	{
		UIConversationWatcherMovie.Instance.Hide();
	}
}
