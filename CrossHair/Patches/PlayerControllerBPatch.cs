using UnityEngine;
using HarmonyLib;
using GameNetcodeStuff;
using System.Collections.Generic;

using CrossHair.Utilities;

namespace CrossHair.Patches {
	public class TargetFieldHook {
		public string fieldName; //? The name of the field to hook
		public object targetValue; //? The value "value" should be set to start fade

		private float fadeValue; //? The value "value" should be set to end fade
		private float fadeOutDuration; //? The duration of the fade
		private float fadeInDuration; //? The duration of the fade

		public PrivateFieldAccessor<PlayerControllerB> field;

		private bool isFading = false;
		public float currentFade = 1f;
		private float fadeTimer = 0f;

		public TargetFieldHook(PlayerControllerB playerInstance, string fieldName, object targetValue, float fadeValue = 0.2f, float fadeOutDuration = 0.2f, float fadeInDuration = 0.5f) {
			this.fieldName = fieldName;
			this.targetValue = targetValue;
			this.fadeValue = fadeValue;
			this.fadeOutDuration = fadeOutDuration;
			this.fadeInDuration = fadeInDuration;

			field = new PrivateFieldAccessor<PlayerControllerB>(playerInstance, fieldName);
		}

		//? Does the field match the target value?
		public bool GetState() {
			object newValue = field.Get<object>();
			try {
				return targetValue switch {
					InteractTrigger	target => newValue != null 	== target,
					bool 			target => (bool)newValue 	== target,
					float 			target => (float)newValue 	== target,
					int 			target => (int)newValue 	== target,
					string 			target => (string)newValue 	== target,
					_ => throw new System.TypeAccessException($"TargetFieldHook.GetState() - Unknown type: {targetValue.GetType()}"),
				};
			} 
			catch (System.TypeAccessException e) {
				Console.LogError(e.ToString());
			}
			
			Console.LogDebug($"{fieldName} - Unknown type: {targetValue.GetType()}");
			return false;
		}

		public void Update() {
			if (isFading) {
				if (!GetState()) {
					isFading = false;
					fadeTimer = currentFade;
					// Console.LogDebug($"{fieldName} - Un-Matched target value !({targetValue})");
				}
				else if (currentFade > fadeValue) {
					currentFade = Mathf.Lerp(1f, fadeValue, fadeTimer);
					fadeTimer += Time.deltaTime / fadeOutDuration;
				}
			}
			else {
				if (GetState()) {
					isFading = true;
					fadeTimer = 1f - currentFade;
					// Console.LogDebug($"{fieldName} - Matched Target value ({targetValue})");
				}
				else if (currentFade < 1f) {
					currentFade = Mathf.Lerp(fadeValue, 1f, fadeTimer);
					fadeTimer += Time.deltaTime / fadeInDuration;
				}
			}
			currentFade = Mathf.Clamp(currentFade, 0, 1);
			fadeTimer = Mathf.Clamp(fadeTimer, 0, 1);
		}

		public void LogValues() {
			Console.LogDebug($"{fieldName}\nValue: {field.Get<object>()} - Target: {targetValue}\nFade: {fadeValue} - Duration: {fadeOutDuration} - {fadeInDuration}");
		}
	}

	[HarmonyPatch(typeof(PlayerControllerB))]
	internal static class PlayerControllerBPatch {
		private static List<TargetFieldHook> targetFields = new List<TargetFieldHook>();
		// static PrivateFieldAccessor<PlayerControllerB> testField;

		[HarmonyPatch("ConnectClientToPlayerObject"), HarmonyPostfix]
		private static void ConnectClientToPlayerObject(ref PlayerControllerB __instance) {
			PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
			if (__instance != localPlayer) return;
			Console.LogInfo($"PlayerControllerB.ConnectClientToPlayerObject() called");

			if (Plugin.CrossHairFading.Value != true) return;

			targetFields = new List<TargetFieldHook>() {
				new(__instance, "isWalking", true, 0.5f, 1f, 0.25f),
				new(__instance, "isSprinting", true, 0.25f, 0.5f, 0.25f),
				new(__instance, "isJumping", true, 0.1f, 0.05f, 1f),
				new(__instance, "isFallingFromJump", true, 0.1f, 0.01f, 1f),
				new(__instance, "isFallingNoJump", true, 0.1f, 0.01f, 1f),
				new(__instance, "isCrouching", true, 0.5f, 0.5f, 1f),
				new(__instance, "isClimbingLadder", true, 0.01f, 0.25f, 2f),
				new(__instance, "twoHanded", true, 0.1f, 0.01f, 0.5f),
				new(__instance, "performingEmote", true, 0.05f, 0.2f, 2f),
				new(__instance, "isUnderwater", true, 0.025f, 0.1f, 1f),
				new(__instance, "inTerminalMenu", true, 0f, 0.2f, 1.5f),
				new(__instance, "isPlayerDead", true, 0.05f, 2.5f, 1f),
				new(__instance, "hasBegunSpectating", true, 0f, 5f, 50f),
				new(__instance, "isHoldingInteract", true, 0.1f, 0.5f, 0.5f),
			};

			Console.LogDebug($"PlayerControllerB.targetFields.Count: {targetFields.Count}");

			// foreach (TargetFieldHook targetField in targetFields) {
			// 	targetField.LogValues();
			// }

			// testField = new(__instance, "isHoldingInteract");
		}

		// static bool isHoldingTest = false;
		// static int testCount = 0;
		[HarmonyPatch("Update"), HarmonyPostfix]
		private static void Update(ref PlayerControllerB __instance) {
			if (__instance != GameNetworkManager.Instance.localPlayerController) return;

			float lowestFade = 1f;
			string lowestFieldName = "";
			foreach (TargetFieldHook targetField in targetFields) {
				targetField.Update();
				if (targetField.currentFade < lowestFade) {
					lowestFade = targetField.currentFade;
					lowestFieldName = targetField.fieldName;
				}
			}

			float aprox = Mathf.Abs(HUDManagerPatch.CrossHairTMP.alpha - lowestFade * (HUDManagerPatch.CrossHairAlpha / 255f));

			// //!! Debug Only
			// bool testState = testField.Get<bool>();
			// if (testState && !isHoldingTest) {
			// 	Console.LogDebug($"{lowestFieldName}: {HUDManagerPatch.CrossHairTMP.alpha} != ({lowestFade} * (({HUDManagerPatch.CrossHairAlpha} / 255)={HUDManagerPatch.CrossHairAlpha / 255}) = {lowestFade * HUDManagerPatch.CrossHairAlpha / 255}) = {aprox > 0.0001f}");
			// 	Console.LogDebug($"Current Aplha: {lowestFade * (HUDManagerPatch.CrossHairAlpha / 255)}");
			// 	Console.LogDebug($"Aprox: {aprox}");
			// 	isHoldingTest = true;
			// }
			// else if (!testState && isHoldingTest) {
			// 	isHoldingTest = false;
			// }
			
			if (aprox > 0.0001f) {
				HUDManagerPatch.SetCrossHairAlphaPercent(lowestFade);
			}
		}
	}
}



/*
	> public bool performingEmote;
	> private bool isWalking;
	> public bool isSprinting;
	> private bool isJumping;
	> private bool isFallingFromJump;
	> public bool isCrouching;
	> private bool isFallingNoJump;
	> public bool isUnderwater;
	> private float cameraUp;
	> public bool isPlayerDead;
	> public bool inTerminalMenu;
	> public bool twoHanded;
	> public bool isClimbingLadder;
	> public bool hasBegunSpectating;

	public bool isTestingPlayer;
	[Header(MODELS / ANIMATIONS)]
	public Transform[] bodyParts;
	public Transform thisPlayerBody;
	public SkinnedMeshRenderer thisPlayerModel;
	public SkinnedMeshRenderer thisPlayerModelLOD1;
	public SkinnedMeshRenderer thisPlayerModelLOD2;
	public SkinnedMeshRenderer thisPlayerModelArms;
	public Transform playerGlobalHead;
	public Transform playerModelArmsMetarig;
	public Transform localArmsRotationTarget;
	public Transform meshContainer;
	public Transform lowerSpine;
	public Camera gameplayCamera;
	public Transform cameraContainerTransform;
	public Transform playerEye;
	public float targetFOV = 66f;
	public Camera visorCamera;
	public CharacterController thisController;
	public Animator playerBodyAnimator;
	public MeshFilter playerBadgeMesh;
	public MeshRenderer playerBetaBadgeMesh;
	public int playerLevelNumber;
	public Transform localVisor;
	public Transform localVisorTargetPoint;
	private bool isSidling;
	private bool wasMovingForward;
	public MultiRotationConstraint cameraLookRig1;
	public MultiRotationConstraint cameraLookRig2;
	public Transform playerHudUIContainer;
	public Transform playerHudBaseRotation;
	public ChainIKConstraint rightArmNormalRig;
	public ChainIKConstraint rightArmProceduralRig;
	public Transform rightArmProceduralTarget;
	private Vector3 rightArmProceduralTargetBasePosition;
	public Light nightVision;
	public int currentSuitID;
	public bool performingEmote; //!!
	public float emoteLayerWeight;
	public float timeSinceStartingEmote;
	public ParticleSystem beamUpParticle;
	public ParticleSystem beamOutParticle;
	public ParticleSystem beamOutBuildupParticle;
	public bool localArmsMatchCamera;
	public Transform localArmsTransform;
	[Header("AUDIOS")]
	public AudioSource movementAudio;
	public AudioSource itemAudio;
	public AudioSource statusEffectAudio;
	public AudioSource waterBubblesAudio;
	public int currentFootstepSurfaceIndex;
	private int previousFootstepClip;
	[HideInInspector]
	public Dictionary<AudioSource, AudioReverbTrigger> audioCoroutines = new Dictionary<AudioSource, AudioReverbTrigger>();
	[HideInInspector]
	public Dictionary<AudioSource, IEnumerator> audioCoroutines2 = new Dictionary<AudioSource, IEnumerator>();
	[HideInInspector]
	public AudioReverbTrigger currentAudioTrigger;
	public AudioReverbTrigger currentAudioTriggerB;
	public float targetDryLevel;
	public float targetRoom;
	public float targetHighFreq;
	public float targetLowFreq;
	public float targetDecayTime;
	public ReverbPreset reverbPreset;
	public AudioListener activeAudioListener;
	public AudioReverbFilter activeAudioReverbFilter;
	public ParticleSystem bloodParticle;
	public bool playingQuickSpecialAnimation;
	private Coroutine quickSpecialAnimationCoroutine;
	[Header("INPUT / MOVEMENT")]
	public float movementSpeed = 0.5f;
	public PlayerActions playerActions;
	private bool isWalking; //!!
	private bool movingForward;
	public Vector2 moveInputVector;
	public Vector3 velocityLastFrame;
	private float sprintMultiplier = 1f;
	public bool isSprinting; //!!
	public float sprintTime = 5f;
	public Image sprintMeterUI;
	[HideInInspector]
	public float sprintMeter;
	[HideInInspector]
	public bool isExhausted;
	private float exhaustionEffectLerp; //?? 
	public float jumpForce = 5f;
	private bool isJumping; //!!
	private bool isFallingFromJump; //!!
	private Coroutine jumpCoroutine;
	public float fallValue;
	public bool isGroundedOnServer;
	public bool isPlayerSliding;
	private float playerSlidingTimer;
	public Vector3 playerGroundNormal;
	public float maxSlideFriction;
	private float slideFriction;
	public float fallValueUncapped;
	public bool takingFallDamage;
	public float minVelocityToTakeDamage;
	public bool isCrouching; //!!
	private bool isFallingNoJump; //!!
	public int isMovementHindered;
	private int movementHinderedPrev;
	public float hinderedMultiplier = 1f;
	public int sourcesCausingSinking;
	public bool isSinking;
	public bool isUnderwater; //!!
	private float syncUnderwaterInterval;
	private bool isFaceUnderwaterOnServer;
	public Collider underwaterCollider;
	private bool wasUnderwaterLastFrame;
	public float sinkingValue;
	public float sinkingSpeedMultiplier;
	public int statusEffectAudioIndex;
	private float cameraUp; // //!!
	public float lookSensitivity = 0.4f;
	private float oldLookRot; 
	private float targetLookRot; 
	private float previousYRot;
	private float targetYRot;
	public Vector3 syncFullRotation;
	private Vector3 walkForce;
	public Vector3 externalForces;
	private Vector3 movementForcesLastFrame;
	public Rigidbody playerRigidbody;
	public float averageVelocity;
	public int velocityMovingAverageLength = 20;
	public int velocityAverageCount;
	public float getAverageVelocityInterval;
	public bool jetpackControls;
	public bool disablingJetpackControls;
	public Transform jetpackTurnCompass;
	private bool startedJetpackControls;
	private float previousFrameDeltaTime;
	private Collider[] nearByPlayers = new Collider[4];
	private bool teleportingThisFrame;
	public bool teleportedLastFrame;
	[Header("LOCATION")]
	public bool isInElevator;
	public bool isInHangarShipRoom;
	public bool isInsideFactory;
	[Space(5f)]
	public bool wasInElevatorLastFrame;
	public Vector3 previousElevatorPosition;
	[Header("CONTROL / NETWORKING")]
	public ulong playerClientId;
	public string playerUsername = "Player";
	public ulong playerSteamId;
	public ulong actualClientId;
	public bool isPlayerControlled;
	public bool justConnected = true;
	public bool disconnectedMidGame;
	[Space(5f)]
	private bool isCameraDisabled;
	public StartOfRound playersManager;
	public bool isHostPlayerObject;
	private Vector3 oldPlayerPosition;
	private int previousAnimationState;
	public Vector3 serverPlayerPosition;
	public bool snapToServerPosition;
	private float oldCameraUp;
	public float ladderCameraHorizontal;
	private float updatePlayerAnimationsInterval;
	private float updatePlayerLookInterval;
	private List<int> currentAnimationStateHash = new List<int>();
	private List<int> previousAnimationStateHash = new List<int>();
	private float currentAnimationSpeed;
	private float previousAnimationSpeed;
	private int previousAnimationServer;
	private int oldConnectedPlayersAmount;
	private int playerMask = 8;
	public RawImage playerScreen;
	public VoicePlayerState voicePlayerState;
	public AudioSource currentVoiceChatAudioSource;
	public PlayerVoiceIngameSettings currentVoiceChatIngameSettings;
	private float voiceChatUpdateInterval;
	public bool isTypingChat;
	[Header("DEATH")]
	public int health;
	public float healthRegenerateTimer;
	public bool criticallyInjured;
	public bool hasBeenCriticallyInjured;
	private float limpMultiplier = 0.2f;
	public CauseOfDeath causeOfDeath;
	public bool isPlayerDead; //!!
	[HideInInspector]
	public bool setPositionOfDeadPlayer;
	[HideInInspector]
	public Vector3 placeOfDeath;
	public Transform spectateCameraPivot;
	public PlayerControllerB spectatedPlayerScript;
	public DeadBodyInfo deadBody;
	public GameObject[] bodyBloodDecals;
	private int currentBloodIndex;
	public List<GameObject> playerBloodPooledObjects = new List<GameObject>();
	public bool bleedingHeavily;
	private float bloodDropTimer;
	private bool alternatePlaceFootprints;
	public EnemyAI inAnimationWithEnemy;
	[Header("UI/MENU")]
	public bool inTerminalMenu; //!!
	public QuickMenuManager quickMenuManager;
	public TextMeshProUGUI usernameBillboardText;
	public Transform usernameBillboard;
	public CanvasGroup usernameAlpha;
	public Canvas usernameCanvas;
	private Vector3 tempVelocity;
	[Header("ITEM INTERACTION")]
	public float grabDistance = 5f;
	public float throwPower = 17f;
	public bool isHoldingObject;
	private bool hasThrownObject;
	public bool twoHanded; //!!
	public bool twoHandedAnimation;
	public float carryWeight = 1f;
	public bool isGrabbingObjectAnimation;
	public bool activatingItem;
	public float grabObjectAnimationTime;
	public Transform localItemHolder;
	public Transform serverItemHolder;
	public Transform propThrowPosition;
	public GrabbableObject currentlyHeldObject;
	private GrabbableObject currentlyGrabbingObject;
	public GrabbableObject currentlyHeldObjectServer;
	public GameObject heldObjectServerCopy;
	private Coroutine grabObjectCoroutine;
	private Ray interactRay;
	private int grabbableObjectsMask = 64;
	private int interactableObjectsMask = 832;
	private int walkableSurfacesMask = 268437769;
	private int walkableSurfacesNoPlayersMask = 268437761;
	private RaycastHit hit;
	private float upperBodyAnimationsWeight;
	public float doingUpperBodyEmote;
	private float handsOnWallWeight;
	public Light helmetLight;
	public Light[] allHelmetLights;
	private bool grabbedObjectValidated;
	private bool grabInvalidated;
	private bool throwingObject;
	[Space(5f)]
	public GrabbableObject[] ItemSlots;
	public int currentItemSlot;
	private MeshRenderer[] itemRenderers;
	private float timeSinceSwitchingSlots;
	[HideInInspector]
	public bool grabSetParentServer;
	[Header("TRIGGERS AND SPECIAL")]
	public Image cursorIcon;
	public TextMeshProUGUI cursorTip;
	public Sprite grabItemIcon;
	private bool hoveringOverItem;
	public InteractTrigger hoveringOverTrigger;
	public InteractTrigger previousHoveringOverTrigger;
	public InteractTrigger currentTriggerInAnimationWith;
	public bool isHoldingInteract;
	public bool inSpecialInteractAnimation;
	public float specialAnimationWeight;
	public bool isClimbingLadder; //!!
	public bool enteringSpecialAnimation;
	public float climbSpeed = 4f;
	public Vector3 clampCameraRotation;
	public Transform lineOfSightCube;
	public bool voiceMuffledByEnemy;
	[Header("SPECIAL ITEMS")]
	public int shipTeleporterId;
	public MeshRenderer mapRadarDirectionIndicator;
	public Animator mapRadarDotAnimator;
	public bool equippedUsableItemQE;
	public bool IsInspectingItem;
	public bool isFirstFrameLateUpdate = true;
	public GrabbableObject pocketedFlashlight;
	public bool isFreeCamera;
	public bool isSpeedCheating;
	public bool inShockingMinigame;
	public Transform shockingTarget;
	public Transform turnCompass;
	public Transform smoothLookTurnCompass;
	public float smoothLookMultiplier = 25f;
	private bool smoothLookEnabledLastFrame;
	public Camera turnCompassCamera;
	[HideInInspector]
	public Vector3 targetScreenPos;
	[HideInInspector]
	public float shockMinigamePullPosition;
	[Space(5f)]
	public bool speakingToWalkieTalkie;
	public bool holdingWalkieTalkie;
	public float isInGameOverAnimation;
	[HideInInspector]
	public bool hasBegunSpectating; //!!
	private Coroutine timeSpecialAnimationCoroutine;
	private float spectatedPlayerDeadTimer;
	public float insanityLevel;
	public float maxInsanityLevel = 50f;
	public float insanitySpeedMultiplier = 1f;
	public bool isPlayerAlone;
	public float timeSincePlayerMoving;
	public Scrollbar terminalScrollVertical;
	private bool updatePositionForNewlyJoinedClient;
	private float timeSinceTakingGravityDamage;
	[Space(5f)]
	public float drunkness;
	public float drunknessInertia = 1f;
	public float drunknessSpeed;
	public bool increasingDrunknessThisFrame;
"
*/