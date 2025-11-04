namespace ReCap.Parser.Core.Core;

public sealed partial class Catalog
{
    partial void Register_Noun()
    {
        RegisterFileType(".Noun", new[] { "Noun" }, 480);

        var noun = AddStruct("Noun");
        noun.Add("nounType", "enum", 0);
        noun.Add("clientOnly", "bool", 4);
        noun.Add("isFixed", "bool", 5);
        noun.Add("isSelfPowered", "bool", 6);
        noun.Add("gfxPickMethod", "enum", 8);
        noun.Add("lifetime", "float", 12);
        noun.Add("graphicsScale", "float", 20);
        noun.Add("prefab", "key", 16);
        noun.Add("modelKey", "key", 36);
        noun.Add("levelEditorModelKey", "key", 52);

        var cSPBoundingBox = AddStruct("cSPBoundingBox");
        cSPBoundingBox.Add("min", "cSPVector3", 32);
        cSPBoundingBox.Add("max", "cSPVector3", 44);
        noun.Add(cSPBoundingBox, 24);

        noun.Add("presetExtents", "enum", 80);
        noun.Add("voice", "key", 96);
        noun.Add("foot", "key", 112);
        noun.Add("flightSound", "key", 128);

        var gfxStates = AddStruct("gfxStates", 8);
        var gfxStateData = AddStruct("state", 56);
        gfxStateData.Add("name", "key", 12);
        gfxStateData.Add("model", "key", 28);
        gfxStateData.Add("animation", "key", 44);
        gfxStateData.Add("prefab", "asset", 48);
        gfxStateData.Add("animationLoops", "bool", 52);
        gfxStates.AddStructArray("state", gfxStateData, 0);
        RegisterNullableType("gfxStates");
        noun.Add("gfxStates", "nullable", gfxStates, 132);

        var cNewGfxState = AddStruct("cNewGfxState", 40);
        cNewGfxState.Add("prefab", "asset", 0);
        cNewGfxState.Add("model", "key", 16);
        cNewGfxState.Add("animation", "key", 32);
        RegisterNullableType("cNewGfxState");

        var doorDef = AddStruct("doorDef", 24);
        doorDef.Add("graphicsState_open", "nullable", cNewGfxState, 0);
        doorDef.Add("graphicsState_opening", "nullable", cNewGfxState, 4);
        doorDef.Add("graphicsState_closed", "nullable", cNewGfxState, 8);
        doorDef.Add("graphicsState_closing", "nullable", cNewGfxState, 12);
        doorDef.Add("clickToOpen", "bool", 16);
        doorDef.Add("clickToClose", "bool", 17);
        doorDef.Add("initialState", "enum", 20);
        RegisterNullableType("doorDef");
        noun.Add("doorDef", "nullable", doorDef, 136);

        var switchDef = AddStruct("switchDef", 12);
        switchDef.Add("graphicsState_unpressed", "nullable", cNewGfxState, 0);
        switchDef.Add("graphicsState_pressing", "nullable", cNewGfxState, 4);
        switchDef.Add("graphicsState_pressed", "nullable", cNewGfxState, 8);
        RegisterNullableType("switchDef");
        noun.Add("switchDef", "nullable", switchDef, 140);

        var pressureSwitchDef = AddStruct("pressureSwitchDef", 40);
        pressureSwitchDef.Add("graphicsState_unpressed", "nullable", cNewGfxState, 0);
        pressureSwitchDef.Add("graphicsState_pressing", "nullable", cNewGfxState, 4);
        pressureSwitchDef.Add("graphicsState_pressed", "nullable", cNewGfxState, 8);
        var cVolumeDef = AddStruct("cVolumeDef");
        cVolumeDef.Add("shape", "enum", 0);
        cVolumeDef.Add("boxWidth", "float", 4);
        cVolumeDef.Add("boxLength", "float", 8);
        cVolumeDef.Add("boxHeight", "float", 12);
        cVolumeDef.Add("sphereRadius", "float", 16);
        cVolumeDef.Add("capsuleHeight", "float", 20);
        cVolumeDef.Add("capsuleRadius", "float", 24);
        pressureSwitchDef.Add(cVolumeDef, 28);
        RegisterNullableType("pressureSwitchDef");
        noun.Add("pressureSwitchDef", "nullable", pressureSwitchDef, 144);

        var crystalDef = AddStruct("crystalDef", 24);
        crystalDef.Add("modifier", "key", 0);
        crystalDef.Add("type", "enum", 4);
        crystalDef.Add("rarity", "enum", 16);
        RegisterNullableType("crystalDef");
        noun.Add("crystalDef", "nullable", crystalDef, 148);

        noun.Add("assetId", "uint64_t", 152);
        noun.Add("npcClassData", "asset", 160);
        noun.Add("playerClassData", "asset", 164);
        noun.Add("characterAnimationData", "asset", 168);

        var creatureThumbnailData = AddStruct("creatureThumbnailData", 108);
        creatureThumbnailData.Add("fovY", "float", 0);
        creatureThumbnailData.Add("nearPlane", "float", 4);
        creatureThumbnailData.Add("farPlane", "float", 8);
        creatureThumbnailData.Add("cameraPosition", "cSPVector3", 12);
        creatureThumbnailData.Add("cameraScale", "float", 24);
        creatureThumbnailData.Add("cameraRotation_0", "cSPVector3", 28);
        creatureThumbnailData.Add("cameraRotation_1", "cSPVector3", 40);
        creatureThumbnailData.Add("cameraRotation_2", "cSPVector3", 52);
        creatureThumbnailData.Add("mouseCameraDataValid", "bool", 64);
        creatureThumbnailData.Add("mouseCameraOffset", "cSPVector3", 68);
        creatureThumbnailData.Add("mouseCameraSubjectPosition", "cSPVector3", 80);
        creatureThumbnailData.Add("mouseCameraTheta", "float", 92);
        creatureThumbnailData.Add("mouseCameraPhi", "float", 96);
        creatureThumbnailData.Add("mouseCameraRoll", "float", 100);
        creatureThumbnailData.Add("poseAnimID", "uint32_t", 104);
        RegisterNullableType("creatureThumbnailData");
        noun.Add("creatureThumbnailData", "nullable", creatureThumbnailData, 172);

        noun.AddArray("eliteAssetIds", "uint64_t", 172);

        noun.Add("physicsType", "enum", 184);
        noun.Add("density", "float", 188);
        noun.Add("physicsKey", "key", 204);
        noun.Add("affectsNavMesh", "bool", 208);
        noun.Add("dynamicWall", "bool", 209);

        var events = AddStruct("events", 32);
        events.Add("onEnterEvent", "key", 12);
        events.Add("onExitEvent", "key", 28);
        RegisterNullableType("events");

        var triggerVolume = AddStruct("triggerVolume", 136);
        triggerVolume.Add("onEnter", "key", 12);
        triggerVolume.Add("onExit", "key", 28);
        triggerVolume.Add("onStay", "key", 44);
        triggerVolume.Add("events", "nullable", events, 48);
        triggerVolume.Add("useGameObjectDimensions", "bool", 52);
        triggerVolume.Add("isKinematic", "bool", 53);
        triggerVolume.Add("shape", "enum", 56);
        triggerVolume.Add("offset", "cSPVector3", 60);
        triggerVolume.Add("timeToActivate", "float", 72);
        triggerVolume.Add("persistentTimer", "bool", 76);
        triggerVolume.Add("triggerOnceOnly", "bool", 77);
        triggerVolume.Add("triggerIfNotBeaten", "bool", 78);
        triggerVolume.Add("triggerActivationType", "enum", 80);
        triggerVolume.Add("luaCallbackOnEnter", "char*", 84);
        triggerVolume.Add("luaCallbackOnExit", "char*", 88);
        triggerVolume.Add("luaCallbackOnStay", "char*", 92);
        triggerVolume.Add("boxWidth", "float", 96);
        triggerVolume.Add("boxLength", "float", 100);
        triggerVolume.Add("boxHeight", "float", 104);
        triggerVolume.Add("sphereRadius", "float", 108);
        triggerVolume.Add("capsuleHeight", "float", 112);
        triggerVolume.Add("capsuleRadius", "float", 116);
        triggerVolume.Add("serverOnly", "bool", 120);
        RegisterNullableType("triggerVolume");
        noun.Add("triggerVolume", "nullable", triggerVolume, 292);

        var collisionVolume = AddStruct("creatureCollisionVolume", 20);
        collisionVolume.Add("shape", "enum", 0);
        collisionVolume.Add("boxWidth", "float", 4);
        collisionVolume.Add("boxLength", "float", 8);
        collisionVolume.Add("boxHeight", "float", 12);
        collisionVolume.Add("sphereRadius", "float", 16);
        RegisterNullableType("creatureCollisionVolume");

        var projectile = AddStruct("projectile", 12);
        projectile.Add("creatureCollisionVolume", "nullable", collisionVolume, 0);
        projectile.Add("otherCollisionVolume", "nullable", collisionVolume, 4);
        projectile.Add("targetType", "enum", 8);
        RegisterNullableType("projectile");
        noun.Add("projectile", "nullable", projectile, 296);

        var orbit = AddStruct("orbit", 12);
        orbit.Add("orbitHeight", "float", 0);
        orbit.Add("orbitRadius", "float", 4);
        orbit.Add("orbitSpeed", "float", 8);
        RegisterNullableType("orbit");
        noun.Add("orbit", "nullable", orbit, 300);

        var locomotionTuning = AddStruct("locomotionTuning", 12);
        locomotionTuning.Add("acceleration", "float", 0);
        locomotionTuning.Add("deceleration", "float", 4);
        locomotionTuning.Add("turnRate", "float", 8);
        RegisterNullableType("locomotionTuning");
        noun.Add("locomotionTuning", "nullable", locomotionTuning, 304);

        noun.Add("gravityData", "asset", 308);

        var sharedComponentData = AddStruct("SharedComponentData", 40);

        var audioTrigger = AddStruct("audioTrigger", 32);
        audioTrigger.Add("type", "enum", 0);
        audioTrigger.Add("sound", "key", 16);
        audioTrigger.Add("is3D", "bool", 20);
        audioTrigger.Add("retrigger", "bool", 21);
        audioTrigger.Add("hardStop", "bool", 22);
        audioTrigger.Add("isVoiceover", "bool", 23);
        audioTrigger.Add("voiceLifetime", "float", 24);
        audioTrigger.Add("triggerVolume", "nullable", triggerVolume, 28);
        RegisterNullableType("audioTrigger");

        var teleporter = AddStruct("teleporter", 12);
        teleporter.Add("destinationMarkerId", "uint32_t", 0);
        teleporter.Add("triggerVolume", "nullable", triggerVolume, 4);
        teleporter.Add("deferTriggerCreation", "bool", 8);
        RegisterNullableType("teleporter");

        var eventListenerDef = AddStruct("eventListenerDef", 8);
        var listener = AddStruct("listener", 40);
        listener.Add("event", "key", 0);
        listener.Add("callback", "key", 28);
        listener.Add("luaCallback", "char*", 36);
        eventListenerDef.AddStructArray("listener", listener, 0);
        RegisterNullableType("eventListenerDef");

        var spawnPointDef = AddStruct("spawnPointDef", 8);
        spawnPointDef.Add("sectionType", "enum", 0);
        spawnPointDef.Add("activatesSpike", "bool", 4);
        RegisterNullableType("spawnPointDef");

        var spawnTrigger = AddStruct("spawnTrigger", 28);
        spawnTrigger.Add("triggerVolume", "nullable", triggerVolume, 0);
        spawnTrigger.Add("deathEvent", "key", 16);
        spawnTrigger.Add("challengeOverride", "uint32_t", 20);
        spawnTrigger.Add("waveOverride", "uint32_t", 24);
        RegisterNullableType("spawnTrigger");

        var interactable = AddStruct("interactable", 72);
        interactable.Add("numUsesAllowed", "uint32_t", 0);
        interactable.Add("interactableAbility", "key", 16);
        interactable.Add("startInteractEvent", "key", 32);
        interactable.Add("endInteractEvent", "key", 48);
        interactable.Add("optionalInteractEvent", "key", 64);
        interactable.Add("challengeValue", "uint32_t", 68);
        RegisterNullableType("interactable");

        var defaultGfxState = AddStruct("defaultGfxState", 24);
        defaultGfxState.Add("name", "key", 12);
        defaultGfxState.Add("animationStartTime", "float", 16);
        defaultGfxState.Add("animationRate", "float", 20);
        RegisterNullableType("defaultGfxState");

        var combatant = AddStruct("combatant", 16);
        combatant.Add("deathEvent", "key", 12);
        RegisterNullableType("combatant");

        var triggerComponent = AddStruct("triggerComponent", 4);
        triggerComponent.Add("triggerVolume", "nullable", triggerVolume, 0);
        RegisterNullableType("triggerComponent");

        var spaceshipSpawnPoint = AddStruct("spaceshipSpawnPoint", 4);
        spaceshipSpawnPoint.Add("index", "uint32_t", 0);
        RegisterNullableType("spaceshipSpawnPoint");

        sharedComponentData.Add("audioTrigger", "nullable", audioTrigger, 0);
        sharedComponentData.Add("teleporter", "nullable", teleporter, 4);
        sharedComponentData.Add("eventListenerDef", "nullable", eventListenerDef, 8);
        sharedComponentData.Add("spawnPointDef", "nullable", spawnPointDef, 16);
        sharedComponentData.Add("spawnTrigger", "nullable", spawnTrigger, 12);
        sharedComponentData.Add("interactable", "nullable", interactable, 20);
        sharedComponentData.Add("defaultGfxState", "nullable", defaultGfxState, 24);
        sharedComponentData.Add("combatant", "nullable", combatant, 28);
        sharedComponentData.Add("triggerComponent", "nullable", triggerComponent, 32);
        sharedComponentData.Add("spaceshipSpawnPoint", "nullable", spaceshipSpawnPoint, 36);
        noun.Add(sharedComponentData, 252);

        noun.Add("isFlora", "bool", 328);
        noun.Add("isMineral", "bool", 329);
        noun.Add("isCreature", "bool", 330);
        noun.Add("isPlayer", "bool", 331);
        noun.Add("isSpawned", "bool", 332);

        noun.Add("toonType", "key", 324);
        noun.Add("modelEffect", "key", 348);
        noun.Add("removalEffect", "key", 364);
        noun.Add("meleeDeathEffect", "key", 396);
        noun.Add("meleeCritEffect", "key", 412);
        noun.Add("energyDeathEffect", "key", 428);
        noun.Add("energyCritEffect", "key", 444);
        noun.Add("plasmaDeathEffect", "key", 460);
        noun.Add("plasmaCritEffect", "key", 476);
    }
}
