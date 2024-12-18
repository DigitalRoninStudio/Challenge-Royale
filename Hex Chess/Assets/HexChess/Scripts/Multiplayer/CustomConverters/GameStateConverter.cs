using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;


public static class GameStateConverter
{
    public class EntityDataListJsonConverter : JsonConverter<List<EntityData>>
    {
        public override List<EntityData> ReadJson(JsonReader reader, Type objectType, List<EntityData> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = new List<EntityData>();

            JArray jsonArray = JArray.Load(reader);

            foreach (JObject jObject in jsonArray.Children<JObject>())
            {
                var type = jObject.GetValue("Type").ToString();
                var value = jObject.GetValue("Value").CreateReader();
                // var obj = serializer.Deserialize(value, Type.GetType($"GameStateData.{type}")) as EntityData;
                var obj = serializer.Deserialize(value, Type.GetType(type)) as EntityData;
                list.Add(obj);
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, List<EntityData> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            foreach (EntityData obj in value)
            {
                JObject jo = new JObject();
                jo.Add("Type", obj.GetType().Name);
                jo.Add("Value", JToken.FromObject(obj, serializer));

                jo.WriteTo(writer);
            }

            writer.WriteEndArray();
        }
    }

    public class BehaviourListJsonConverter : JsonConverter<List<BehaviourData>>
    {
        public override List<BehaviourData> ReadJson(JsonReader reader, Type objectType, List<BehaviourData> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = new List<BehaviourData>();

            JArray jsonArray = JArray.Load(reader);

            foreach (JObject jObject in jsonArray.Children<JObject>())
            {
                var type = jObject.GetValue("Type").ToString();
                var value = jObject.GetValue("Value").CreateReader();
                // var obj = serializer.Deserialize(value, Type.GetType($"GameStateData.{type}")) as BehaviourData;
                var obj = serializer.Deserialize(value, Type.GetType(type)) as BehaviourData;
                list.Add(obj);
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, List<BehaviourData> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            foreach (BehaviourData obj in value)
            {
                JObject jo = new JObject();
                jo.Add("Type", obj.GetType().Name);
                jo.Add("Value", JToken.FromObject(obj, serializer));

                jo.WriteTo(writer);
            }

            writer.WriteEndArray();
        }
    }

    public class StatusEffectListJsonConverter : JsonConverter<List<StatusEffectData>>
    {
        public override List<StatusEffectData> ReadJson(JsonReader reader, Type objectType, List<StatusEffectData> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = new List<StatusEffectData>();

            JArray jsonArray = JArray.Load(reader);

            foreach (JObject jObject in jsonArray.Children<JObject>())
            {
                var type = jObject.GetValue("Type").ToString();
                var value = jObject.GetValue("Value").CreateReader();
                var obj = serializer.Deserialize(value, Type.GetType(type)) as StatusEffectData;
                list.Add(obj);
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, List<StatusEffectData> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            foreach (StatusEffectData obj in value)
            {
                JObject jo = new JObject();
                jo.Add("Type", obj.GetType().Name);
                jo.Add("Value", JToken.FromObject(obj, serializer));

                jo.WriteTo(writer);
            }

            writer.WriteEndArray();
        }
    }

    public class ModifierListJsonConverter : JsonConverter<List<Modifier>>
    {
        public override List<Modifier> ReadJson(JsonReader reader, Type objectType, List<Modifier> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var list = new List<Modifier>();

            JArray jsonArray = JArray.Load(reader);

            foreach (JObject jObject in jsonArray.Children<JObject>())
            {
                var type = jObject.GetValue("Type").ToString();
                var value = jObject.GetValue("Value").CreateReader();
                var obj = serializer.Deserialize(value, Type.GetType(type)) as Modifier;
                list.Add(obj);
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, List<Modifier> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            foreach (Modifier obj in value)
            {
                JObject jo = new JObject();
                jo.Add("Type", obj.GetType().Name);
                jo.Add("Value", JToken.FromObject(obj, serializer));

                jo.WriteTo(writer);
            }

            writer.WriteEndArray();
        }
    }

    public class EntityCoordinateConvertor : JsonConverter<Dictionary<Vector2Int, List<string>>>
    {
        public override Dictionary<Vector2Int, List<string>> ReadJson(JsonReader reader, Type objectType, Dictionary<Vector2Int, List<string>> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var result = new Dictionary<Vector2Int, List<string>>();

            foreach (var property in jsonObject.Properties())
            {
                var vectorKey = ParseVector2Int(property.Name);
                var listValues = property.Value.ToObject<List<string>>();
                result[vectorKey] = listValues;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, Dictionary<Vector2Int, List<string>> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                string key = FormatVector2Int(kvp.Key);
                writer.WritePropertyName(key);
                serializer.Serialize(writer, kvp.Value);
            }

            writer.WriteEndObject();
        }

        private Vector2Int ParseVector2Int(string str)
        {
            var trimmed = str.Trim('(', ')');
            var parts = trimmed.Split(',');

            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);

            return new Vector2Int(x, y);
        }
        private string FormatVector2Int(Vector2Int vector)
        {
            return $"({vector.x},{vector.y})";
        }
    }

    public static string Serialize<T>(T obj)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            StringEscapeHandling = StringEscapeHandling.Default,
            Converters = new List<JsonConverter>
        {
            new EntityDataListJsonConverter(),
            new BehaviourListJsonConverter(),
            new StatusEffectListJsonConverter(),
            new ModifierListJsonConverter(),
            new EntityCoordinateConvertor()
        }
        };
        return JsonConvert.SerializeObject(obj, settings);
    }

    public static T Deserialize<T>(string data)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            StringEscapeHandling = StringEscapeHandling.Default,
            Converters = new List<JsonConverter>
        {
            new EntityDataListJsonConverter(),
            new BehaviourListJsonConverter(),
            new StatusEffectListJsonConverter(),
            new ModifierListJsonConverter(),
            new EntityCoordinateConvertor()
        }

        };
        return JsonConvert.DeserializeObject<T>(data, settings);
    }
}
public class GameData
{
    public string GUID;
    public MapData MapData;
    public List<PlayerData> PlayersData;
    public RandomGeneratorState RandomState;
    public RoundData RoundData;

    public GameData()
    {
        PlayersData = new List<PlayerData>();
        RandomState = new RandomGeneratorState();
        RoundData = new RoundData();
    }

    public GameData(Game game)
    {
        GUID = game.GUID;
        RandomState = game.randomGenerator.GetState();
        RoundData = game.roundController.GetRoundData();
        MapData = GameFactory.CreateMapData(game);
        PlayersData = new List<PlayerData>();

        foreach (var player in game.players)
            PlayersData.Add(player.GetPlayerData());
    }
}
public class RandomGeneratorState
{
    public uint InitialSeed { get; set; }
    public long CurrentPosition { get; set; }
}
public class RoundData
{
    public int Round;
    public Team StartRoundTeam;
    public Team TeamWithInitiation;
}
public class MapData
{
    public string Id;
    public Dictionary<Vector2Int, List<string>> entityPositions;

    public MapData()
    {
        entityPositions = new Dictionary<Vector2Int, List<string>>();
    }
}
public class PlayerData
{
    public string Id;
    public Team Team;
    public PlayerState PlayerState;
    public EnergyData EnergyData;
    public List<EntityData> EntityData;

    public PlayerData()
    {
        EnergyData = new EnergyData();
        EntityData = new List<EntityData>();
    }

    public PlayerData(Player player)
    {
        Id = player.clientId;
        Team = player.team;
        PlayerState = player.playerState;
        EnergyData = player.energyController.GetEnergyData();
        EntityData = new List<EntityData>();

        foreach (var entity in player.entities)
            EntityData.Add(entity.GetEntityData());        
    }
}

public class EnergyData
{
    public int energy;
    public int stash;
}

public abstract class EntityData
{
    public string GUID;
    public string Id;
    public Visibility Visibility;
    public Direction Direction;
    public bool IsBlockingMovemenet;

    public List<BehaviourData> BehaviourDatas;
    public List<StatusEffectData> StatusEffectDatas;
    public List<ModifierSource> ModifierSourceDatas;

    public EntityData()
    {
        BehaviourDatas = new List<BehaviourData>();
        StatusEffectDatas = new List<StatusEffectData>();
        ModifierSourceDatas = new List<ModifierSource>();
    }

    public EntityData(Entity entity)
    {
        GUID = entity.guid;
        Id = entity.EntityBlueprint.Id;
        Visibility = entity.visibility;
        Direction = entity.direction;
        IsBlockingMovemenet = entity.isBlockingMovement;

        BehaviourDatas = new List<BehaviourData>();
        foreach (var behaviour in entity.Behaviours) 
            BehaviourDatas.Add(behaviour.GetBehaviourData());

        StatusEffectDatas = new List<StatusEffectData>();
        foreach (var statusEffect in entity.StatusEffectController.StatusEffects)
            StatusEffectDatas.Add(statusEffect.GetStatusEffectData());

        ModifierSourceDatas = new List<ModifierSource>();
        foreach (var modifier in entity.ModifierController.Modifiers)
            ModifierSourceDatas.Add(modifier.GetModifierSource());
    }
}

public class FigureData : EntityData
{
    public FigureData() : base() { }

    public FigureData(Figure figure) : base(figure) { }
}

#region BehaviourData
public class BehaviourData
{
    public string GUID;
    public string Id;

    public BehaviourData() { }

    public BehaviourData(Behaviour behaviour)
    {
        GUID = behaviour.guid;
        Id = behaviour.BehaviourBlueprint.Id;
    }
}
#region MovementBehaviourData
public abstract class MovementBehaviourData : BehaviourData
{
    public MovementBehaviourData() : base() { }
    public MovementBehaviourData(MovementBehaviour behaviour) : base(behaviour) { }
}


public class NormalMovementData : MovementBehaviourData
{
    public NormalMovementData() : base() { }
    public NormalMovementData(NormalMovementBehaviour behaviour) : base(behaviour) { }
}

public class KnightMovementData : MovementBehaviourData
{
    public KnightMovementData() : base() { }
    public KnightMovementData(KnightMovementBehaviour behaviour) : base(behaviour) { }
}

public class TeleportMovementData : MovementBehaviourData
{
    public TeleportMovementData() : base() { }
    public TeleportMovementData(TeleportMovementBehaviour behaviour) : base(behaviour) { }
}
public class DirectionMovementData : MovementBehaviourData
{
    public DirectionMovementData() : base() { }
    public DirectionMovementData(DirectionMovementBehaviour behaviour) : base(behaviour) { }
}
#endregion
#region AttackBehaviourData
public abstract class AttackBehaviourData : BehaviourData
{
    public AttackBehaviourData() : base() { }
    public AttackBehaviourData(AttackBehaviour behaviour) : base(behaviour) { }
}
public class MeleeAttackData : AttackBehaviourData
{
    public MeleeAttackData() : base() { }
    public MeleeAttackData(MeleeAttackBehaviour behaviour) : base(behaviour) { }
}

public class RangedAttackData : AttackBehaviourData
{
    public RangedAttackData() : base() { }
    public RangedAttackData(RangedAttackBehaviour behaviour) : base(behaviour) { }

}
#endregion
#region DamageableBehaviourData
public class DamageableBehaviourData : BehaviourData
{
    public int CurrentHealth;
    public DamageableBehaviourData() : base() { }
    public DamageableBehaviourData(DamageableBehaviour behaviour) : base(behaviour) 
    {
        CurrentHealth = behaviour.CurrentHealth;
    }
}
#endregion
#region AbilityBehaviourData
public class AbilityBehaviourData : BehaviourData
{
    public AbilityBehaviourData() : base() { }
    public AbilityBehaviourData(AbilityBehaviour ability) : base(ability) { }
}
public class ActiveAbilityBehaviourData : AbilityBehaviourData
{
    public int CurrentCooldown;
    public ActiveAbilityBehaviourData() : base() { }
    public ActiveAbilityBehaviourData(ActiveAbility activeAbility) : base(activeAbility) 
    {
        CurrentCooldown = activeAbility.CurrentCooldow;
    }
}
public class SwordsmanSpecialData : ActiveAbilityBehaviourData
{
    public bool Toggle;
    public ModifierData HealthModifierData;
    public SwordsmanSpecialData() : base() { }
    public SwordsmanSpecialData(SwordsmanSpecial special) : base(special) 
    {
        Toggle = special.IsToogle;
        HealthModifierData = special.healthModifier.GetModifierData();
    }

}

public class PassiveAbilityBehaviourData : AbilityBehaviourData
{
    public PassiveAbilityBehaviourData() : base() { }
    public PassiveAbilityBehaviourData(PassiveAbility passiveAbility) : base(passiveAbility) { }
}
#endregion
#endregion

public class ModifierData
{
    public string Guid;
    //public ModifierSource ModifierSource;
    public ModifierData() { }

    public ModifierData(Modifier modifier)
    {
        Guid = modifier.guid;
       // ModifierSource = modifier.modifierSource.GetModifierSource();
    }
}

public class HealthModifierData : ModifierData
{
    public HealthModifierData() : base() { }
    public HealthModifierData(HealthModifier modifier) : base(modifier) { }
}
public abstract class StatusEffectData
{
    public string Guid;
    public string BehaviourGuid;
    public string EntityGuid;
    public string BlueprintId;
    public int Duration;

    public StatusEffectData() { }

    public StatusEffectData(StatusEffect statusEffect)
    {
        Guid = statusEffect.guid;
        BehaviourGuid = statusEffect.owner.guid;
        EntityGuid = statusEffect.owner.Owner.guid;
        BlueprintId = statusEffect.StatusEffectBlueprint.Id;
        Duration = statusEffect.duration;
    }
}

public class StunData : StatusEffectData
{
    public StunData() : base() { }
    public StunData(Stun stun) : base(stun) { }
}
public class DisarmData : StatusEffectData
{
    public DisarmData() : base() { }
    public DisarmData(Disarm disarm) : base(disarm) { }
}
public class RootData : StatusEffectData
{
    public RootData() : base() { }
    public RootData(Root root) : base(root) { }
}

public class DamageImmuneData : StatusEffectData
{
    public DamageImmuneData() : base() { }
    public DamageImmuneData(DamageImmune damageImmune) : base(damageImmune) { }
}
public class DamageReturnData : StatusEffectData
{
    public DamageReturnData() : base() { }
    public DamageReturnData(DamageReturn damageReturn) : base(damageReturn) { }
}
public class ShieldData : StatusEffectData
{
    public int CurrentHealth;
    public ShieldData() : base() { }
    public ShieldData(Shield shield) : base(shield) 
    {
        CurrentHealth = shield.CurrentHealth;
    }
}




