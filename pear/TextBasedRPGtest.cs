using System;
using System.Collections.Generic;

Random rng = new Random();

// Player Stats
int pLevel = 1, pXp = 0, pXpToLevel = 50;
int pHp = 100, pMaxHp = 100, pDmg = 20, pDef = 10;
int pMoney = 0;

// Inventory
Dictionary<string, int> inventory = new() {
    { "HP Potion", 0 },
    { "Sword", 0 },
    { "Armor", 0 }
};

string[] enemies = { "Goblin", "Orc", "Troll", "Dragon" };

while (true)
{
    Console.WriteLine($"\n🧍 Level: {pLevel} | XP: {pXp}/{pXpToLevel} | 💰 {pMoney} money");
    Console.WriteLine($"HP: {pHp}/{pMaxHp} | DMG: {pDmg} | DEF: {pDef}");
    Console.WriteLine("1 - Combat   |   2 - Heal (10 money)   |   3 - Shop   |   4 - Inventory");
    string input = Console.ReadLine();

    switch (input)
    {
        case "1": Combat(); break;
        case "2": Heal(); break;
        case "3": Shop(); break;
        case "4": InventoryMenu(); break;
        default: Console.WriteLine("Invalid input."); break;
    }
}

void Combat()
{
    int index = rng.Next(enemies.Length);
    Enemy enemy = CreateEnemy(enemies[index], pLevel);
    Console.WriteLine($"\n⚔️ You encountered a level {enemy.Level} {enemy.Name} (HP: {enemy.HP}, ATK: {enemy.Attack})");

    while (pHp > 0 && enemy.HP > 0)
    {
        Console.WriteLine("\n1 - Attack   |   2 - Run   |   3 - Use Item");
        string action = Console.ReadLine();

        switch (action)
        {
            case "1":
                PlayerAttack(enemy);
                if (enemy.HP > 0) EnemyAttack(enemy);
                break;
            case "2":
                Console.WriteLine("🏃 You ran away!");
                return;
            case "3":
                UseItem();
                break;
            default:
                Console.WriteLine("Invalid action.");
                break;
        }
    }

    if (enemy.HP <= 0)
    {
        Console.WriteLine($"✅ You defeated the {enemy.Name}!");
        pMoney += enemy.MoneyReward;
        pXp += enemy.XpReward;
        Console.WriteLine($"+{enemy.MoneyReward} 💰, +{enemy.XpReward} XP");

        if (rng.Next(100) < 50)
        {
            inventory["HP Potion"]++;
            Console.WriteLine("🎁 Loot drop: HP Potion!");
        }

        if (pXp >= pXpToLevel)
        {
            pXp -= pXpToLevel;
            pLevel++;
            pXpToLevel = (int)(pXpToLevel * 1.5);
            pMaxHp += 20; pHp = pMaxHp; pDmg += 5; pDef += 2;

            Console.WriteLine($"🎉 Level Up! Now level {pLevel}");
            Console.WriteLine($"Stats: HP {pMaxHp}, DMG {pDmg}, DEF {pDef}");
        }
    }
}

void PlayerAttack(Enemy enemy)
{
    bool miss = rng.Next(100) < 10;
    bool crit = rng.Next(100) < 10;
    int damage = rng.Next(pDmg - 5, pDmg + 6);

    if (miss)
    {
        Console.WriteLine("❌ You missed!");
        return;
    }

    if (crit)
    {
        damage *= 2;
        Console.WriteLine("💥 Critical Hit!");
    }

    enemy.HP -= damage;
    Console.WriteLine($"You dealt {damage} damage. Enemy HP: {Math.Max(enemy.HP, 0)}");
}

void EnemyAttack(Enemy enemy)
{
    bool miss = rng.Next(100) < 10;
    bool crit = rng.Next(100) < 10;
    int rawDamage = rng.Next(enemy.Attack - 3, enemy.Attack + 4);

    if (miss)
    {
        Console.WriteLine($"😅 The {enemy.Name} missed!");
        return;
    }

    if (crit)
    {
        rawDamage *= 2;
        Console.WriteLine($"🔥 The {enemy.Name} lands a critical hit!");
    }

    int damage = Math.Max(0, rawDamage - pDef);
    pHp -= damage;
    Console.WriteLine($"The {enemy.Name} hits you for {damage}. Your HP: {Math.Max(pHp, 0)}");

    if (pHp <= 0)
    {
        Console.WriteLine("💀 You died. Game Over.");
        Environment.Exit(0);
    }
}

void Heal()
{
    int cost = 10;
    if (pMoney >= cost)
    {
        pMoney -= cost;
        pHp = Math.Min(pHp + 20, pMaxHp);
        Console.WriteLine($"❤️ You healed. Current HP: {pHp}");
    }
    else
    {
        Console.WriteLine("❌ Not enough money!");
    }
}

void Shop()
{
    Console.WriteLine("\n🏬 Shop:");
    Console.WriteLine("1 - HP Potion (5💰)");
    Console.WriteLine("2 - Sword (+5 DMG) (50💰)");
    Console.WriteLine("3 - Armor (+5 DEF) (50💰)");
    Console.WriteLine("4 - Exit");

    string choice = Console.ReadLine();
    switch (choice)
    {
        case "1":
            if (pMoney >= 5)
            {
                inventory["HP Potion"]++;
                pMoney -= 5;
                Console.WriteLine("Bought HP Potion.");
            }
            else Console.WriteLine("Not enough money.");
            break;
        case "2":
            if (pMoney >= 50)
            {
                inventory["Sword"]++;
                pMoney -= 50;
                pDmg += 5;
                Console.WriteLine("Bought Sword.");
            }
            else Console.WriteLine("Not enough money.");
            break;
        case "3":
            if (pMoney >= 50)
            {
                inventory["Armor"]++;
                pMoney -= 50;
                pDef += 5;
                Console.WriteLine("Bought Armor.");
            }
            else Console.WriteLine("Not enough money.");
            break;
        case "4":
            break;
        default:
            Console.WriteLine("Invalid choice.");
            break;
    }
}

void InventoryMenu()
{
    Console.WriteLine("\n🎒 Inventory:");
    foreach (var item in inventory)
    {
        Console.WriteLine($"{item.Key}: {item.Value}");
    }
}

void UseItem()
{
    Console.WriteLine("\nType item name to use (or leave blank):");
    string item = Console.ReadLine();

    if (item == "HP Potion" && inventory["HP Potion"] > 0)
    {
        pHp = Math.Min(pHp + 50, pMaxHp);
        inventory["HP Potion"]--;
        Console.WriteLine($"Used HP Potion. Current HP: {pHp}");
    }
    else
    {
        Console.WriteLine("❌ You don't have that item or it's not usable.");
    }
}

// Enemy definition
Enemy CreateEnemy(string name, int level)
{
    int scale = level;
    return name switch
    {
        "Goblin" => new Enemy(name, rng.Next(10 + scale * 5, 20 + scale * 5), rng.Next(5 + scale * 2, 10 + scale * 2), 10 + scale * 2, 10 + scale * 3, level),
        "Orc" => new Enemy(name, rng.Next(20 + scale * 6, 40 + scale * 6), rng.Next(10 + scale * 3, 20 + scale * 3), 15 + scale * 3, 15 + scale * 4, level),
        "Troll" => new Enemy(name, rng.Next(30 + scale * 8, 60 + scale * 8), rng.Next(15 + scale * 3, 25 + scale * 3), 20 + scale * 4, 20 + scale * 5, level),
        "Dragon" => new Enemy(name, rng.Next(100 + scale * 10, 200 + scale * 10), rng.Next(30 + scale * 5, 50 + scale * 5), 30 + scale * 6, 40 + scale * 8, level),
        _ => new Enemy(name, 10, 5, 5, 5, level),
    };
}

class Enemy
{
    public string Name;
    public int HP;
    public int Attack;
    public int MoneyReward;
    public int XpReward;
    public int Level;

    public Enemy(string name, int hp, int attack, int money, int xp, int level)
    {
        Name = name;
        HP = hp;
        Attack = attack;
        MoneyReward = money;
        XpReward = xp;
        Level = level;
    }
}
