using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class Program
{
    // === Player Stats ===
    static int pLevel = 1, pXp = 0, pXpToLevel = 50;
    static int pHp = 100, pMaxHp = 100, pMoney = 0;
    static int pDef = 10; // Armor adds to this
    static int baseDamage = 20; // Base weaponless damage

    // Player equipment: currently equipped sword and armor
    static Sword equippedSword = null;
    static Armor equippedArmor = null;

    // Inventory: item name => count
    static Dictionary<string, int> inventory = new Dictionary<string, int>()
    {
        { "HP Potion", 1 } // start with 1 potion
    };

    // List of enemies
    static string[] enemyTypes = { "Goblin", "Orc", "Troll", "Dragon" };

    // RNG
    static Random rng = new();

    // Save file path
    static string saveFilePath = "rpg_save.json";

    static void Main()
    {
        Console.WriteLine("=== Simple RPG ===");

        LoadGame();

        while (true)
        {
            ShowStatus();

            Console.WriteLine("Choose an action:");
            Console.WriteLine("1) Combat");
            Console.WriteLine("2) Heal (10 money)");
            Console.WriteLine("3) Shop");
            Console.WriteLine("4) Inventory");
            Console.WriteLine("5) Save Game");
            Console.WriteLine("6) Load Game");
            Console.WriteLine("7) Reset Progress");
            Console.WriteLine("0) Exit");

            string input = Console.ReadLine();
            switch (input)
            {
                case "1": Combat(); break;
                case "2": Heal(); break;
                case "3": Shop(); break;
                case "4": InventoryMenu(); break;
                case "5": SaveGame(); break;
                case "6": LoadGame(); break;
                case "7": ResetProgress(); break;
                case "0": return;
                default: Console.WriteLine("Invalid input."); break;
            }
        }
    }

    static void ShowStatus()
    {
        Console.WriteLine("\n=== Player Status ===");
        Console.WriteLine($"Level: {pLevel}  XP: {pXp}/{pXpToLevel}  Money: {pMoney}");
        Console.WriteLine($"HP: {pHp}/{pMaxHp}  Defense: {GetTotalDefense()}");
        Console.WriteLine($"Equipped Sword: {(equippedSword != null ? equippedSword.Name + (equippedSword.Enchant != null ? $" ({equippedSword.Enchant})" : "") : "None")}");
        Console.WriteLine($"Equipped Armor: {(equippedArmor != null ? equippedArmor.Name : "None")}");
    }

    static int GetTotalDamage()
    {
        // Total damage = base + sword damage
        return baseDamage + (equippedSword?.Damage ?? 0);
    }

    static int GetTotalDefense()
    {
        return pDef + (equippedArmor?.Defense ?? 0);
    }

    // === Combat ===
    static void Combat()
    {
        var enemy = CreateEnemy();

        Console.WriteLine($"\nYou encountered a Level {enemy.Level} {enemy.Name}!");
        Console.WriteLine($"Enemy HP: {enemy.HP}  Attack: {enemy.Attack}");

        bool heavyAttackCharging = false; // to track 2-turn heavy attack

        while (pHp > 0 && enemy.HP > 0)
        {
            Console.WriteLine("\nChoose your action:");
            Console.WriteLine("1) Normal Attack (5% miss chance)");
            Console.WriteLine("2) Heavy Attack (2-turn, 2x damage, 10% miss chance)");
            Console.WriteLine("3) Use Item");
            Console.WriteLine("4) Run");

            string action = Console.ReadLine();

            if (heavyAttackCharging)
            {
                Console.WriteLine("You release your Heavy Attack!");
                if (!TryMiss(10))
                {
                    int dmg = GetTotalDamage() * 2;
                    Console.WriteLine($"Heavy Attack deals {dmg} damage!");
                    enemy.HP -= dmg;
                }
                else
                {
                    Console.WriteLine("But you missed!");
                }
                heavyAttackCharging = false;
            }
            else
            {
                switch (action)
                {
                    case "1": // Normal Attack
                        if (!TryMiss(5))
                        {
                            int dmg = GetTotalDamage();
                            Console.WriteLine($"You hit the {enemy.Name} for {dmg} damage!");
                            enemy.HP -= dmg;
                        }
                        else Console.WriteLine("You missed!");
                        break;

                    case "2": // Start heavy attack charge
                        Console.WriteLine("You start charging your Heavy Attack. You must wait one turn to release it.");
                        heavyAttackCharging = true;
                        break;

                    case "3": // Use item
                        UseItem();
                        continue; // skip enemy attack to allow item use
                    case "4": // Run
                        if (rng.Next(100) < 75)
                        {
                            Console.WriteLine("You ran away safely!");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("You failed to run away!");
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid action.");
                        continue;
                }
            }

            if (enemy.HP <= 0) break;

            EnemyAttack(enemy);
        }

        if (enemy.HP <= 0)
        {
            Console.WriteLine($"You defeated the {enemy.Name}!");
            pMoney += enemy.MoneyReward;
            Console.WriteLine($"+{enemy.MoneyReward} money");
            pXp += enemy.XpReward;
            Console.WriteLine($"+{enemy.XpReward} XP");

            HandleLoot(enemy);
            TryLevelUp();
        }
    }

    static void EnemyAttack(Enemy enemy)
    {
        Console.WriteLine($"\n{enemy.Name}'s turn!");

        if (TryMiss(5))
        {
            Console.WriteLine($"{enemy.Name} missed!");
            return;
        }

        int rawDamage = rng.Next(enemy.Attack - 2, enemy.Attack + 3);
        int damage = rawDamage - GetTotalDefense();
        if (damage < 0) damage = 0;

        pHp -= damage;
        Console.WriteLine($"{enemy.Name} hits you for {damage} damage! Your HP: {Math.Max(pHp, 0)}");

        if (pHp <= 0)
        {
            Console.WriteLine("You died! Game Over.");
            Environment.Exit(0);
        }
    }

    // Returns true if attack misses
    static bool TryMiss(int chancePercent)
    {
        return rng.Next(100) < chancePercent;
    }

    static Enemy CreateEnemy()
    {
        string name = enemyTypes[rng.Next(enemyTypes.Length)];
        int level = pLevel;

        // Easier starting enemies: scale stronger with level^1.5
        double scale = Math.Pow(level, 1.5);

        return name switch
        {
            "Goblin" => new Enemy(name,
                rng.Next(10 + (int)(scale * 2), 20 + (int)(scale * 3)),
                rng.Next(5 + (int)(scale), 10 + (int)(scale * 2)),
                5 + level * 2,
                5 + level * 3,
                level),

            "Orc" => new Enemy(name,
                rng.Next(20 + (int)(scale * 3), 40 + (int)(scale * 4)),
                rng.Next(10 + (int)(scale * 2), 20 + (int)(scale * 3)),
                10 + level * 4,
                10 + level * 5,
                level),

            "Troll" => new Enemy(name,
                rng.Next(40 + (int)(scale * 5), 80 + (int)(scale * 6)),
                rng.Next(15 + (int)(scale * 3), 25 + (int)(scale * 4)),
                20 + level * 8,
                20 + level * 10,
                level),

            "Dragon" => new Enemy(name,
                rng.Next(150 + (int)(scale * 8), 300 + (int)(scale * 10)),
                rng.Next(40 + (int)(scale * 6), 70 + (int)(scale * 8)),
                100 + level * 20,
                100 + level * 30,
                level),

            _ => new Enemy(name, 10, 5, 5, 5, level),
        };
    }

    static void HandleLoot(Enemy enemy)
    {
        // 60% chance enemy drops an HP Potion
        if (rng.Next(100) < 60)
        {
            AddToInventory("HP Potion", 1);
            Console.WriteLine("Enemy dropped: HP Potion!");
        }

        // 30% chance enemy drops armor
        if (rng.Next(100) < 30)
        {
            Armor armorDrop = Armor.RandomArmor(pLevel);
            Console.WriteLine($"Enemy dropped armor: {armorDrop.Name} (+{armorDrop.Defense} DEF)");
            // Auto-add to inventory as string (use name)
            AddToInventory(armorDrop.Name, 1);

            // Auto-equip if better than current
            if (equippedArmor == null || armorDrop.Defense > equippedArmor.Defense)
            {
                equippedArmor = armorDrop;
                Console.WriteLine($"You equipped the new armor!");
            }
        }

        // 25% chance enemy drops sword
        if (rng.Next(100) < 25)
        {
            Sword swordDrop = Sword.RandomSword(pLevel);
            Console.WriteLine($"Enemy dropped sword: {swordDrop.Name} (+{swordDrop.Damage} DMG{(swordDrop.Enchant != null ? ", Enchant: " + swordDrop.Enchant : "")})");
            AddToInventory(swordDrop.Name, 1);

            if (equippedSword == null)
            {
                equippedSword = swordDrop;
                Console.WriteLine("You equipped the new sword!");
            }
            else if (swordDrop.Damage > equippedSword.Damage)
            {
                Console.WriteLine("You can sell this sword or equip it.");
                Console.WriteLine("1) Equip new sword");
                Console.WriteLine("2) Sell new sword for money");
                string choice = Console.ReadLine();
                if (choice == "1")
                {
                    equippedSword = swordDrop;
                    Console.WriteLine("You equipped the new sword.");
                }
                else
                {
                    pMoney += swordDrop.GetSellPrice();
                    Console.WriteLine($"You sold the sword for {swordDrop.GetSellPrice()} money.");
                }
            }
        }
    }

    static void AddToInventory(string item, int count)
    {
        if (inventory.ContainsKey(item))
            inventory[item] += count;
        else
            inventory[item] = count;
    }

    static void TryLevelUp()
    {
        while (pXp >= pXpToLevel)
        {
            pXp -= pXpToLevel;
            pLevel++;
            pXpToLevel = (int)(pXpToLevel * 1.8); // Faster scaling

            pMaxHp += 25;
            pHp = pMaxHp; // heal to full on level up
            baseDamage += 5;
            pDef += 3;

            Console.WriteLine($"\n🎉 LEVEL UP! You are now level {pLevel}!");
            Console.WriteLine($"Max HP: {pMaxHp}, Base Damage: {baseDamage}, Defense: {pDef}");
        }
    }

    // === Heal ===
    static void Heal()
    {
        const int cost = 10;
        if (pMoney >= cost)
        {
            pMoney -= cost;
            pHp = Math.Min(pHp + 30, pMaxHp);
            Console.WriteLine($"You healed 30 HP. Current HP: {pHp}");
        }
        else Console.WriteLine("Not enough money to heal!");
    }

    // === Shop ===
    static void Shop()
    {
        Console.WriteLine("\nWelcome to the shop!");
        Console.WriteLine("1) HP Potion (5 money)");
        Console.WriteLine("2) Buy Armor (+5 DEF) (50 money)");
        Console.WriteLine("3) Buy Sword (+5 DMG) (50 money)");
        Console.WriteLine("4) Exit shop");

        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                if (pMoney >= 5)
                {
                    AddToInventory("HP Potion", 1);
                    pMoney -= 5;
                    Console.WriteLine("Bought HP Potion.");
                }
                else Console.WriteLine("Not enough money.");
                break;

            case "2":
                if (pMoney >= 50)
                {
                    Armor armor = new Armor("Basic Armor", 5);
                    AddToInventory(armor.Name, 1);
                    pMoney -= 50;
                    if (equippedArmor == null || armor.Defense > equippedArmor.Defense)
                    {
                        equippedArmor = armor;
                        Console.WriteLine("Bought and equipped Basic Armor!");
                    }
                    else Console.WriteLine("Bought Basic Armor and added to inventory.");
                }
                else Console.WriteLine("Not enough money.");
                break;

            case "3":
                if (pMoney >= 50)
                {
                    Sword sword = new Sword("Basic Sword", 5, null);
                    AddToInventory(sword.Name, 1);
                    pMoney -= 50;
                    if (equippedSword == null || sword.Damage > equippedSword.Damage)
                    {
                        equippedSword = sword;
                        Console.WriteLine("Bought and equipped Basic Sword!");
                    }
                    else Console.WriteLine("Bought Basic Sword and added to inventory.");
                }
                else Console.WriteLine("Not enough money.");
                break;

            case "4":
                Console.WriteLine("Leaving shop.");
                break;

            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
    }

    // === Inventory Menu ===
    static void InventoryMenu()
    {
        Console.WriteLine("\nYour inventory:");
        foreach (var item in inventory)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }

        Console.WriteLine("Do you want to equip a sword or armor? (yes/no)");
        string input = Console.ReadLine().ToLower();

        if (input == "yes")
        {
            Console.WriteLine("Type item name to equip:");
            string itemName = Console.ReadLine();

            if (!inventory.ContainsKey(itemName) || inventory[itemName] == 0)
            {
                Console.WriteLine("You don't have that item.");
                return;
            }

            // Equip sword
            if (Sword.IsSword(itemName))
            {
                Sword swordToEquip = Sword.FromName(itemName);
                if (swordToEquip == null)
                {
                    Console.WriteLine("Cannot equip this sword.");
                    return;
                }

                equippedSword = swordToEquip;
                Console.WriteLine($"Equipped sword: {equippedSword.Name}");
            }
            // Equip armor
            else if (Armor.IsArmor(itemName))
            {
                Armor armorToEquip = Armor.FromName(itemName);
                if (armorToEquip == null)
                {
                    Console.WriteLine("Cannot equip this armor.");
                    return;
                }

                equippedArmor = armorToEquip;
                Console.WriteLine($"Equipped armor: {equippedArmor.Name}");
            }
            else
            {
                Console.WriteLine("You can't equip that item.");
            }
        }
    }

    // === Use Item ===
    static void UseItem()
    {
        Console.WriteLine("Type the item name to use or empty to cancel:");
        string itemName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(itemName))
            return;

        if (itemName == "HP Potion" && inventory.ContainsKey("HP Potion") && inventory["HP Potion"] > 0)
        {
            pHp = Math.Min(pHp + 50, pMaxHp);
            inventory["HP Potion"]--;
            Console.WriteLine($"You used an HP Potion. Current HP: {pHp}");
        }
        else
        {
            Console.WriteLine("You don't have that item or can't use it.");
        }
    }

    // === Save/Load/Reset ===
    static void SaveGame()
    {
        var saveData = new SaveData
        {
            pLevel = pLevel,
            pXp = pXp,
            pXpToLevel = pXpToLevel,
            pHp = pHp,
            pMaxHp = pMaxHp,
            pMoney = pMoney,
            baseDamage = baseDamage,
            pDef = pDef,
            equippedSwordName = equippedSword?.Name,
            equippedSwordEnchant = equippedSword?.Enchant,
            equippedSwordDamage = equippedSword?.Damage ?? 0,
            equippedArmorName = equippedArmor?.Name,
            equippedArmorDef = equippedArmor?.Defense ?? 0,
            inventory = inventory
        };

        string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(saveFilePath, json);

        Console.WriteLine("Game saved.");
    }

    static void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Console.WriteLine("No save file found. Starting new game.");
            return;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            var saveData = JsonSerializer.Deserialize<SaveData>(json);

            if (saveData != null)
            {
                pLevel = saveData.pLevel;
                pXp = saveData.pXp;
                pXpToLevel = saveData.pXpToLevel;
                pHp = saveData.pHp;
                pMaxHp = saveData.pMaxHp;
                pMoney = saveData.pMoney;
                baseDamage = saveData.baseDamage;
                pDef = saveData.pDef;

                inventory = saveData.inventory ?? new Dictionary<string, int>();

                // Load equipped sword
                if (!string.IsNullOrEmpty(saveData.equippedSwordName))
                    equippedSword = new Sword(saveData.equippedSwordName, saveData.equippedSwordDamage, saveData.equippedSwordEnchant);
                else
                    equippedSword = null;

                // Load equipped armor
                if (!string.IsNullOrEmpty(saveData.equippedArmorName))
                    equippedArmor = new Armor(saveData.equippedArmorName, saveData.equippedArmorDef);
                else
                    equippedArmor = null;

                Console.WriteLine("Game loaded.");
            }
        }
        catch
        {
            Console.WriteLine("Failed to load save. Starting new game.");
        }
    }

    static void ResetProgress()
    {
        Console.WriteLine("Are you sure you want to reset your progress? (yes/no)");
        if (Console.ReadLine().ToLower() == "yes")
        {
            if (File.Exists(saveFilePath))
                File.Delete(saveFilePath);

            pLevel = 1;
            pXp = 0;
            pXpToLevel = 50;
            pHp = 100;
            pMaxHp = 100;
            pMoney = 0;
            baseDamage = 20;
            pDef = 10;

            equippedSword = null;
            equippedArmor = null;
            inventory = new Dictionary<string, int> { { "HP Potion", 1 } };

            Console.WriteLine("Progress reset.");
        }
    }

    // === Classes for Enemies, Swords, Armor, and SaveData ===

    class Enemy
    {
        public string Name;
        public int HP;
        public int Attack;
        public int XpReward;
        public int MoneyReward;
        public int Level;

        public Enemy(string name, int hp, int attack, int xpReward, int moneyReward, int level)
        {
            Name = name;
            HP = hp;
            Attack = attack;
            XpReward = xpReward;
            MoneyReward = moneyReward;
            Level = level;
        }
    }

    class Sword
    {
        public string Name;
        public int Damage;
        public string Enchant; // e.g., "Fire"

        public Sword(string name, int damage, string enchant)
        {
            Name = name;
            Damage = damage;
            Enchant = enchant;
        }

        // Random sword generator for enemy drops
        public static Sword RandomSword(int playerLevel)
        {
            string[] swordNames = { "Short Sword", "Long Sword", "Claymore", "Katana" };
            string[] enchants = { null, "Fire" };

            var rng = new Random();

            string name = swordNames[rng.Next(swordNames.Length)];
            int damage = 5 + playerLevel * 3 + rng.Next(0, 6);
            string enchant = enchants[rng.Next(enchants.Length)];

            return new Sword(name, damage, enchant);
        }

        public int GetSellPrice()
        {
            return Damage * 10 + (Enchant != null ? 50 : 0);
        }

        public static bool IsSword(string name)
        {
            // Simplified check
            string[] knownSwords = { "Short Sword", "Long Sword", "Claymore", "Katana", "Basic Sword" };
            foreach (var s in knownSwords)
                if (s == name) return true;
            return false;
        }

        public static Sword FromName(string name)
        {
            // For simplicity, assume no enchant when equipping from inventory
            switch (name)
            {
                case "Short Sword": return new Sword("Short Sword", 10, null);
                case "Long Sword": return new Sword("Long Sword", 15, null);
                case "Claymore": return new Sword("Claymore", 20, null);
                case "Katana": return new Sword("Katana", 25, null);
                case "Basic Sword": return new Sword("Basic Sword", 5, null);
                default: return null;
            }
        }
    }

    class Armor
    {
        public string Name;
        public int Defense;

        public Armor(string name, int defense)
        {
            Name = name;
            Defense = defense;
        }

        public static Armor RandomArmor(int playerLevel)
        {
            string[] armorNames = { "Leather Armor", "Chainmail", "Plate Armor" };
            var rng = new Random();

            string name = armorNames[rng.Next(armorNames.Length)];
            int def = 3 + playerLevel * 2 + rng.Next(0, 4);

            return new Armor(name, def);
        }

        public static bool IsArmor(string name)
        {
            string[] knownArmor = { "Leather Armor", "Chainmail", "Plate Armor", "Basic Armor" };
            foreach (var a in knownArmor)
                if (a == name) return true;
            return false;
        }

        public static Armor FromName(string name)
        {
            return name switch
            {
                "Leather Armor" => new Armor("Leather Armor", 7),
                "Chainmail" => new Armor("Chainmail", 12),
                "Plate Armor" => new Armor("Plate Armor", 18),
                "Basic Armor" => new Armor("Basic Armor", 5),
                _ => null,
            };
        }
    }

    class SaveData
    {
        public int pLevel { get; set; }
        public int pXp { get; set; }
        public int pXpToLevel { get; set; }
        public int pHp { get; set; }
        public int pMaxHp { get; set; }
        public int pMoney { get; set; }
        public int baseDamage { get; set; }
        public int pDef { get; set; }
        public string equippedSwordName { get; set; }
        public int equippedSwordDamage { get; set; }
        public string equippedSwordEnchant { get; set; }
        public string equippedArmorName { get; set; }
        public int equippedArmorDef { get; set; }
        public Dictionary<string, int> inventory { get; set; }
    }
}

