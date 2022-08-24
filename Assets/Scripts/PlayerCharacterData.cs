using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

namespace SurvivalEngine
{

    [System.Serializable]
    public class TimedBonusData
    {
        public BonusType bonus;
        public float time;
        public float value;
    }

    [System.Serializable]
    public class PlayerPetData
    {
        public string pet_id;
        public string uid;
    }

    [System.Serializable]
    public class PlayerCharacterData
    {
        public int player_id;

        public Vector3Data position;
        public int xp = 0;
        public int gold = 0;

        public Dictionary<AttributeType, float> attributes = new Dictionary<AttributeType, float>();
        public Dictionary<BonusType, TimedBonusData> timed_bonus_effects = new Dictionary<BonusType, TimedBonusData>();
        public Dictionary<string, int> crafted_count = new Dictionary<string, int>();
        public Dictionary<string, int> kill_count = new Dictionary<string, int>();
        public Dictionary<string, bool> unlocked_ids = new Dictionary<string, bool>();
        public Dictionary<string, PlayerPetData> pets = new Dictionary<string, PlayerPetData>();

        public PlayerCharacterData(int id) { player_id = id; }
        public void LoadSaveData(Dictionary<string, object> data)
        {
            this.player_id = int.Parse(data["player_id"].ToString());
            this.position.LoadSaveData(data["position"].ToString());
            Debug.LogError(this.position);
            this.xp = int.Parse(data["xp"].ToString());
            this.gold = int.Parse(data["gold"].ToString());

            Dictionary<string, object> attData = data["attributes"] as Dictionary<string, object>;
            this.attributes = new Dictionary<AttributeType, float>();
            foreach(string type in attData.Keys)
            {
                this.attributes[(AttributeType)int.Parse(type)] = float.Parse(attData[type].ToString());
            }
            
            this.timed_bonus_effects = new Dictionary<BonusType, TimedBonusData>();
            Dictionary<string, object> timeBonusData = data["timed_bonus_effects"] as Dictionary<string, object>;
            foreach(string type in timeBonusData.Keys)
            {
                TimedBonusData tmp = new TimedBonusData();
                List<object> listData = timeBonusData[type] as List<object>;
                tmp.bonus = (BonusType)int.Parse(listData[0].ToString());
                tmp.time = float.Parse(listData[1].ToString());
                tmp.value = float.Parse(listData[2].ToString());
                this.timed_bonus_effects[(BonusType)int.Parse(type)] = tmp;
            }

            Dictionary<string, object> craftedData = data["crafted_count"] as Dictionary<string, object>;
            this.crafted_count = new Dictionary<string, int>();
            foreach(string id in craftedData.Keys)
            {
                this.crafted_count[id] = int.Parse(craftedData[id].ToString());
            }

            Dictionary<string, object> killData = data["kill_count"] as Dictionary<string, object>;
            this.kill_count = new Dictionary<string, int>();
            foreach(string id in killData.Keys)
            {
                this.kill_count[id] = int.Parse(killData[id].ToString());
            }

            Dictionary<string, object> unlockedData = data["unlocked_ids"] as Dictionary<string, object>;
            this.unlocked_ids = new Dictionary<string, bool>();
            foreach(string id in unlockedData.Keys)
            {
                this.unlocked_ids[id] = bool.Parse(unlockedData[id].ToString());
            }
            
            this.pets = new Dictionary<string, PlayerPetData>();
            Dictionary<string, object> petData = data["pets"] as Dictionary<string, object>;
            foreach(string type in petData.Keys)
            {
                PlayerPetData tmp = new PlayerPetData();
                List<object> listData = petData[type] as List<object>;
                tmp.pet_id = listData[0].ToString();
                tmp.uid = listData[1].ToString();
                this.pets[type] = tmp;
            }
        }
        public object GetSaveData()
        {
            Dictionary<string, object> sendData = new Dictionary<string, object>();
            sendData["player_id"] = this.player_id;
            // List<float> posList = new List<float>();
            // posList.Add(this.position.x);
            // posList.Add(this.position.y);
            // posList.Add(this.position.z);
            sendData["position"] = position.ToString();
            sendData["xp"] = this.xp;
            sendData["gold"] = this.gold;
            Dictionary<int, object> attData = new Dictionary<int, object>();
            foreach(AttributeType type in this.attributes.Keys) {
                attData[(int)type] = this.attributes[type];
            }
            sendData["attributes"] = attData;
            Dictionary<int, object> timeBonusData = new Dictionary<int, object>();
            foreach (BonusType type in this.timed_bonus_effects.Keys)
            {
                List<object> listData = new List<object>();
                listData.Add((int)this.timed_bonus_effects[type].bonus);
                listData.Add(this.timed_bonus_effects[type].time);
                listData.Add(this.timed_bonus_effects[type].value);
                timeBonusData[(int)type] = listData;
            }
            sendData["timed_bonus_effects"] = timeBonusData;
            sendData["crafted_count"] = crafted_count;
            sendData["kill_count"] = kill_count;
            sendData["unlocked_ids"] = unlocked_ids;

            Dictionary<string, object> petData = new Dictionary<string, object>();
            foreach (string type in this.pets.Keys)
            {
                List<object> listData = new List<object>();
                listData.Add(this.pets[type].pet_id);
                listData.Add(this.pets[type].uid);
                petData[type] = listData;
            }
            sendData["pets"] = pets;
            return sendData;
        }

        public void FixData()
        {
            //Fix data to make sure old save files compatible with new game version
            if (attributes == null)
                attributes = new Dictionary<AttributeType, float>();
            if (unlocked_ids == null)
                unlocked_ids = new Dictionary<string, bool>();
            if (timed_bonus_effects == null)
                timed_bonus_effects = new Dictionary<BonusType, TimedBonusData>();

            if (crafted_count == null)
                crafted_count = new Dictionary<string, int>();
            if (kill_count == null)
                kill_count = new Dictionary<string, int>();

            if (pets == null)
                pets = new Dictionary<string, PlayerPetData>();
        }

        //--- Attributes ----

        public bool HasAttribute(AttributeType type)
        {
            return attributes.ContainsKey(type);
        }

        public float GetAttributeValue(AttributeType type)
        {
            if (attributes.ContainsKey(type))
                return attributes[type];
            return 0f;
        }

        public void SetAttributeValue(AttributeType type, float value, float max)
        {
            attributes[type] = Mathf.Clamp(value, 0f, max);
        }

        public void AddAttributeValue(AttributeType type, float value, float max)
        {
            if (!attributes.ContainsKey(type))
                attributes[type] = value;
            else
                attributes[type] += value;

            attributes[type] = Mathf.Clamp(attributes[type], 0f, max);
        }

        public void AddTimedBonus(BonusType type, float value, float duration)
        {
            TimedBonusData new_bonus = new TimedBonusData();
            new_bonus.bonus = type;
            new_bonus.value = value;
            new_bonus.time = duration;

            if (!timed_bonus_effects.ContainsKey(type) || timed_bonus_effects[type].time < duration)
                timed_bonus_effects[type] = new_bonus;
        }

        public void RemoveTimedBonus(BonusType type)
        {
            if (timed_bonus_effects.ContainsKey(type))
                timed_bonus_effects.Remove(type);
        }

        public float GetTotalTimedBonus(BonusType type)
        {
            if (timed_bonus_effects.ContainsKey(type) && timed_bonus_effects[type].time > 0f)
                return timed_bonus_effects[type].value;
            return 0f;
        }

        public void GainXP(int xp)
        {
            this.xp += xp;
        }

        // ---- Unlock groups -----

        public void UnlockID(string id)
        {
            if (!string.IsNullOrEmpty(id))
                unlocked_ids[id] = true;
        }

        public void RemoveUnlockedID(string id)
        {
            if (unlocked_ids.ContainsKey(id))
                unlocked_ids.Remove(id);
        }

        public bool IsIDUnlocked(string id)
        {
            if (unlocked_ids.ContainsKey(id))
                return unlocked_ids[id];
            return false;
        }

        // --- Craftable crafted
        public void AddCraftCount(string craft_id, int value = 1)
        {
            if (!string.IsNullOrEmpty(craft_id))
            {
                if (crafted_count.ContainsKey(craft_id))
                    crafted_count[craft_id] += value;
                else
                    crafted_count[craft_id] = value;

                if(crafted_count[craft_id] <= 0)
                    crafted_count.Remove(craft_id);
            }
        }

        public int GetCraftCount(string craft_id)
        {
            if (crafted_count.ContainsKey(craft_id))
                return crafted_count[craft_id];
            return 0;
        }

        public void ResetCraftCount(string craft_id)
        {
            if (crafted_count.ContainsKey(craft_id))
                crafted_count.Remove(craft_id);
        }

        public void ResetCraftCount()
        {
            crafted_count.Clear();
        }

        // --- Killed things
        public void AddKillCount(string craft_id, int value = 1)
        {
            if (!string.IsNullOrEmpty(craft_id))
            {
                if (kill_count.ContainsKey(craft_id))
                    kill_count[craft_id] += value;
                else
                    kill_count[craft_id] = value;

                if (kill_count[craft_id] <= 0)
                    kill_count.Remove(craft_id);
            }
        }

        public int GetKillCount(string craft_id)
        {
            if (kill_count.ContainsKey(craft_id))
                return kill_count[craft_id];
            return 0;
        }

        public void ResetKillCount(string craft_id)
        {
            if (kill_count.ContainsKey(craft_id))
                kill_count.Remove(craft_id);
        }

        public void ResetKillCount()
        {
            kill_count.Clear();
        }

        //----- Pet owned by player ----
        public void AddPet(string uid, string pet_id)
        {
            PlayerPetData pet = new PlayerPetData();
            pet.pet_id = pet_id;
            pet.uid = uid;
            pets[uid] = pet;
        }

        public void RemovePet(string uid)
        {
            if (pets.ContainsKey(uid))
                pets.Remove(uid);
        }

        public static PlayerCharacterData Get(int player_id)
        {
            return PlayerData.Get().GetPlayerCharacter(player_id);
        }
    }
}