﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Numerics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;
using System.Data.SQLite;

namespace Tibialyzer {
    public partial class MainForm : Form {
        public static MainForm mainForm;

        public Dictionary<string, Item> itemNameMap = new Dictionary<string, Item>();
        public Dictionary<int, Item> itemIdMap = new Dictionary<int, Item>();
        public Dictionary<string, Creature> creatureNameMap = new Dictionary<string, Creature>();
        public Dictionary<int, Creature> creatureIdMap = new Dictionary<int, Creature>();
        public Dictionary<string, NPC> npcNameMap = new Dictionary<string, NPC>();
        public Dictionary<int, NPC> npcIdMap = new Dictionary<int, NPC>();
        public Dictionary<string, HuntingPlace> huntingPlaceNameMap = new Dictionary<string, HuntingPlace>();
        public Dictionary<int, HuntingPlace> huntingPlaceIdMap = new Dictionary<int, HuntingPlace>();
        public Dictionary<string, Spell> spellNameMap = new Dictionary<string, Spell>();
        public Dictionary<int, Spell> spellIdMap = new Dictionary<int, Spell>();
        public Dictionary<int, Quest> questIdMap = new Dictionary<int, Quest>();
        public Dictionary<string, Quest> questNameMap = new Dictionary<string, Quest>();
        public Dictionary<int, Mount> mountIdMap = new Dictionary<int, Mount>();
        public Dictionary<string, Mount> mountNameMap = new Dictionary<string, Mount>();
        public Dictionary<string, Outfit> outfitNameMap = new Dictionary<string, Outfit>();
        public Dictionary<int, Outfit> outfitIdMap = new Dictionary<int, Outfit>();

        public static Creature getCreature(string name) {
            name = name.ToLower().Trim();
            if (!mainForm.creatureNameMap.ContainsKey(name)) return null;
            return mainForm.creatureNameMap[name];
        }
        public static NPC getNPC(string name) {
            name = name.ToLower().Trim();
            if (!mainForm.npcNameMap.ContainsKey(name)) return null;
            return mainForm.npcNameMap[name];
        }
        public static Item getItem(string name) {
            name = name.ToLower().Trim();
            if (!mainForm.itemNameMap.ContainsKey(name)) return null;
            return mainForm.itemNameMap[name];
        }

        public static List<string> vocations = new List<string> { "knight", "druid", "paladin", "sorcerer" };

        public static Color background_color = Color.FromArgb(0, 51, 102);
        public static double opacity = 0.8;
        public static bool transparent = true;
        public static Image[] image_numbers = new Image[10];
        private Form tooltipForm = null;
        public static Image tibia_store_image = null;
        private static Image tibia_image = null;
        public static Image back_image = null;
        public static Image prevpage_image = null;
        public static Image nextpage_image = null;
        public static Image item_background = null;
        public static Image cross_image = null;
        public static Image[] star_image = new Image[6];
        public static Image[] star_image_text = new Image[6];
        public static Image mapup_image = null;
        public static Image mapdown_image = null;
        public static Image checkmark_yes = null;
        public static Image checkmark_no = null;
        public static Image infoIcon = null;
        public static Dictionary<string, Image> vocationImages = new Dictionary<string, Image>();
        private bool keep_working = true;
        private static string databaseFile = @"Database\Database.db";
        private static string settingsFile = @"Database\settings.txt";
        private static string nodeDatabase = @"Database\Nodes.db";
        private static string pluralMapFile = @"Database\pluralMap.txt";
        private static string autohotkeyFile = @"Database\autohotkey.ahk";
        private List<string> character_names = new List<string>();
        public static List<Map> map_files = new List<Map>();
        public static Color label_text_color = Color.FromArgb(191, 191, 191);
        public static int max_creatures = 50;
        public List<string> new_names = null;
        private bool prevent_settings_update = false;
        private bool minimize_notification = true;
        public int notification_value = 2000;
        static HashSet<string> cities = new HashSet<string>() { "ab'dendriel", "carlin", "kazordoon", "venore", "thais", "ankrahmun", "farmine", "gray beach", "liberty bay", "port hope", "rathleton", "roshamuul", "yalahar", "svargrond", "edron", "darashia", "rookgaard", "dawnport", "gray beach" };
        public List<string> notification_items = new List<string>();
        private ToolTip scan_tooltip = new ToolTip();
        private Stack<string> command_stack = new Stack<string>();

        private SQLiteConnection conn;
        static Dictionary<string, Image> creatureImages = new Dictionary<string, Image>();

        enum ScanningState { Scanning, NoTibia, Stuck };
        ScanningState current_state;

        private Image loadingbar = null;
        private Image loadingbarred = null;
        private Image loadingbargray = null;

        public MainForm() {
            mainForm = this;
            InitializeComponent();

            conn = new SQLiteConnection(String.Format("Data Source={0};Version=3;", databaseFile));
            conn.Open();

            back_image = Image.FromFile(@"Images\back.png");
            prevpage_image = Image.FromFile(@"Images\prevpage.png");
            nextpage_image = Image.FromFile(@"Images\nextpage.png");
            cross_image = Image.FromFile(@"Images\cross.png");
            tibia_image = Image.FromFile(@"Images\tibia.png");
            mapup_image = Image.FromFile(@"Images\mapup.png");
            mapdown_image = Image.FromFile(@"Images\mapdown.png");
            checkmark_no = Image.FromFile(@"Images\checkmark-no.png");
            checkmark_yes = Image.FromFile(@"Images\checkmark-yes.png");
            infoIcon = Image.FromFile(@"Images\defaulticon.png");
            tibia_store_image = Image.FromFile(@"Images\tibiastore.png");

            item_background = System.Drawing.Image.FromFile(@"Images\item_background.png");
            for (int i = 0; i < 10; i++) {
                image_numbers[i] = System.Drawing.Image.FromFile(@"Images\" + i.ToString() + ".png");
            }

            vocationImages.Add("knight", Image.FromFile(@"Images\Knight.png"));
            vocationImages.Add("paladin", Image.FromFile(@"Images\Paladin.png"));
            vocationImages.Add("druid", Image.FromFile(@"Images\Druid.png"));
            vocationImages.Add("sorcerer", Image.FromFile(@"Images\Sorcerer.png"));

            NotificationForm.Initialize();
            CreatureStatsForm.InitializeCreatureStats();
            HuntListForm.Initialize();

            for (int i = 0; i < 5; i++) {
                star_image[i] = Image.FromFile(@"Images\star" + i + ".png");
                star_image_text[i] = Image.FromFile(@"Images\star" + i + "_text.png");
            }
            star_image[5] = Image.FromFile(@"Images\starunknown.png");
            star_image_text[5] = Image.FromFile(@"Images\starunknown_text.png");

            prevent_settings_update = true;
            this.initializePluralMap();
            this.loadDatabaseData();
            this.loadSettings();
            this.initializeNames();
            this.initializeHunts();
            this.initializeSettings();
            this.initializeMaps();
            Pathfinder.LoadFromDatabase(nodeDatabase);
            prevent_settings_update = false;

            if (getSettingBool("StartAutohotkeyAutomatically")) {
                startAutoHotkey_Click(null, null);
            }

            ignoreStamp = createStamp();

            this.backgroundBox.Image = NotificationForm.background_image;

            BackgroundWorker bw = new BackgroundWorker();
            makeDraggable(this.Controls);
            bw.DoWork += bw_DoWork;
            bw.RunWorkerAsync();

            scan_tooltip.AutoPopDelay = 60000;
            scan_tooltip.InitialDelay = 500;
            scan_tooltip.ReshowDelay = 0;
            scan_tooltip.ShowAlways = true;
            scan_tooltip.UseFading = true;

            this.loadingbar = new Bitmap(@"Images\scanningbar.gif");
            this.loadingbarred = new Bitmap(@"Images\scanningbar-red.gif");
            this.loadingbargray = new Bitmap(@"Images\scanningbar-gray.gif");

            this.loadTimerImage.Image = this.loadingbarred;
            this.current_state = ScanningState.NoTibia;
            this.loadTimerImage.Enabled = true;
            scan_tooltip.SetToolTip(this.loadTimerImage, "No Tibia Client Found...");
        }

        public static int DATABASE_NULL = -127;
        public static string DATABASE_STRING_NULL = "";
        private void loadDatabaseData() {
            // first load creatures from the database
            SQLiteCommand command = new SQLiteCommand("SELECT id, name, health, experience, maxdamage, summon, illusionable, pushable, pushes, physical, holy, death, fire, energy, ice, earth, drown, lifedrain, paralysable, senseinvis, image, abilities, title, speed, armor FROM Creatures", conn);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                Creature cr = new Creature();
                cr.id = reader.GetInt32(0);
                cr.name = reader["name"].ToString();
                cr.health = reader.IsDBNull(2) ? DATABASE_NULL : reader.GetInt32(2);
                cr.experience = reader.IsDBNull(3) ? DATABASE_NULL : reader.GetInt32(3);
                cr.maxdamage = reader.IsDBNull(4) ? DATABASE_NULL : reader.GetInt32(4);
                cr.summoncost = reader.IsDBNull(5) ? DATABASE_NULL : reader.GetInt32(5);
                cr.illusionable = reader.GetBoolean(6);
                cr.pushable = reader.GetBoolean(7);
                cr.pushes = reader.GetBoolean(8);
                cr.res_phys = reader.IsDBNull(9) ? 100 : reader.GetInt32(9);
                cr.res_holy = reader.IsDBNull(10) ? 100 : reader.GetInt32(10);
                cr.res_death = reader.IsDBNull(11) ? 100 : reader.GetInt32(11);
                cr.res_fire = reader.IsDBNull(12) ? 100 : reader.GetInt32(12);
                cr.res_energy = reader.IsDBNull(13) ? 100 : reader.GetInt32(13);
                cr.res_ice = reader.IsDBNull(14) ? 100 : reader.GetInt32(14);
                cr.res_earth = reader.IsDBNull(15) ? 100 : reader.GetInt32(15);
                cr.res_drown = reader.IsDBNull(16) ? 100 : reader.GetInt32(16);
                cr.res_lifedrain = reader.IsDBNull(17) ? 100 : reader.GetInt32(17);
                cr.paralysable = reader.GetBoolean(18);
                cr.senseinvis = reader.GetBoolean(19);
                if (reader.IsDBNull(20)) {
                    continue;
                }
                cr.image = Image.FromStream(reader.GetStream(20));
                cr.abilities = reader.IsDBNull(21) ? DATABASE_STRING_NULL : reader["abilities"].ToString();
                cr.title = reader[22].ToString();
                cr.speed = reader.IsDBNull(23) ? DATABASE_NULL : reader.GetInt32(23);
                cr.armor = reader.IsDBNull(24) ? DATABASE_NULL : reader.GetInt32(24);

                if (creatureNameMap.ContainsKey(cr.name.ToLower())) {
                    if (cr.name.ToLower() == cr.title.ToLower()) {
                        creatureNameMap[cr.name.ToLower()] = cr;
                    }
                } else {
                    creatureNameMap.Add(cr.name.ToLower(), cr);
                }
                creatureIdMap.Add(cr.id, cr);
            }

            // now load items
            command = new SQLiteCommand("SELECT id, name, actual_value, vendor_value, stackable, capacity, category, image, discard, convert_to_gold, look_text, title, currency FROM Items", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                Item item = new Item();
                item.id = reader.GetInt32(0);
                item.name = reader.GetString(1);
                item.actual_value = reader.IsDBNull(2) ? DATABASE_NULL : reader.GetInt32(2);
                item.vendor_value = reader.IsDBNull(3) ? DATABASE_NULL : reader.GetInt32(3);
                item.stackable = reader.GetBoolean(4);
                item.capacity = reader.IsDBNull(5) ? DATABASE_NULL : reader.GetFloat(5);
                item.category = reader.IsDBNull(6) ? "Unknown" : reader.GetString(6);
                item.image = Image.FromStream(reader.GetStream(7));
                item.discard = reader.GetBoolean(8);
                item.convert_to_gold = reader.GetBoolean(9);
                item.look_text = reader.IsDBNull(10) ? String.Format("You see a {0}.", item.name) : reader.GetString(10);
                item.title = reader.GetString(11);
                item.currency = reader.IsDBNull(12) ? DATABASE_NULL : reader.GetInt32(12);

                if (item.image.RawFormat.Guid == ImageFormat.Gif.Guid) {
                    int frames = item.image.GetFrameCount(FrameDimension.Time);
                    if (frames == 1) {
                        Bitmap new_bitmap = new Bitmap(item.image);
                        new_bitmap.MakeTransparent();
                        item.image.Dispose();
                        item.image = new_bitmap;
                    }
                }

                if (itemNameMap.ContainsKey(item.name.ToLower())) {
                    if (item.name.ToLower() == item.title.ToLower()) {
                        itemNameMap[item.name.ToLower()] = item;
                    }
                } else {
                    itemNameMap.Add(item.name.ToLower(), item);
                }
                itemIdMap.Add(item.id, item);
            }

            // skins for the creatures
            command = new SQLiteCommand("SELECT creatureid, skinitemid, knifeitemid, percentage FROM Skins", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                Skin skin = new Skin();
                int creatureid = reader.GetInt32(0);
                int itemid = reader.GetInt32(1);
                int skinitemid = reader.GetInt32(2);
                skin.percentage = reader.IsDBNull(3) ? DATABASE_NULL : reader.GetFloat(3);

                Creature creature = creatureIdMap[creatureid];
                Item item = itemIdMap[itemid];
                Item skinItem = itemIdMap[skinitemid];

                skin.drop_item = item;
                skin.skin_item = skinItem;
                creature.skin = skin;
            }


            // creature drops
            command = new SQLiteCommand("SELECT creatureid, itemid, percentage FROM CreatureDrops;", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                int creatureid = reader.GetInt32(0);
                int itemid = reader.GetInt32(1);
                float percentage = reader.IsDBNull(2) ? DATABASE_NULL : reader.GetFloat(2);

                Item item = itemIdMap[itemid];
                Creature creature = creatureIdMap[creatureid];
                ItemDrop itemDrop = new ItemDrop();
                itemDrop.item = item;
                itemDrop.creature = creature;
                itemDrop.percentage = percentage;

                item.itemdrops.Add(itemDrop);
                creature.itemdrops.Add(itemDrop);
            }

            // NPCs
            command = new SQLiteCommand("SELECT id,name,city,x,y,z,image,job FROM NPCs;", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                NPC npc = new NPC();
                npc.id = reader.GetInt32(0);
                npc.name = reader["name"].ToString();
                npc.city = reader["city"].ToString();
                npc.pos.x = reader.IsDBNull(3) ? DATABASE_NULL : (int)(Coordinate.MaxWidth * reader.GetFloat(3));
                npc.pos.y = reader.IsDBNull(4) ? DATABASE_NULL : (int)(Coordinate.MaxHeight * reader.GetFloat(4));
                npc.pos.z = reader.IsDBNull(5) ? DATABASE_NULL : reader.GetInt32(5);
                npc.image = Image.FromStream(reader.GetStream(6));
                npc.job = reader.IsDBNull(7) ? "" : reader.GetString(7);

                // special case for rashid: change location based on day of the week
                if (npc != null && npc.name == "Rashid") {
                    SQLiteCommand rashidCommand = new SQLiteCommand(String.Format("SELECT city, x, y, z FROM RashidPositions WHERE day='{0}'", DateTime.Now.DayOfWeek.ToString()), conn);
                    SQLiteDataReader rashidReader = rashidCommand.ExecuteReader();
                    if (rashidReader.Read()) {
                        npc.city = rashidReader["city"].ToString();
                        npc.pos.x = (int)(Coordinate.MaxWidth * rashidReader.GetFloat(1));
                        npc.pos.y = (int)(Coordinate.MaxHeight * rashidReader.GetFloat(2));
                        npc.pos.z = rashidReader.GetInt32(3);
                    }
                }
                npcNameMap.Add(npc.name.ToLower(), npc);
                npcIdMap.Add(npc.id, npc);
            }

            // items that you can buy from NPCs
            command = new SQLiteCommand("SELECT itemid, vendorid, value FROM BuyItems;", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                ItemSold buyItem = new ItemSold();
                int itemid = reader.GetInt32(0);
                int vendorid = reader.GetInt32(1);
                buyItem.price = reader.GetInt32(2);

                Item item = itemIdMap[itemid];
                NPC npc = npcIdMap[vendorid];
                buyItem.npc = npc;
                buyItem.item = item;
                item.buyItems.Add(buyItem);
                npc.buyItems.Add(buyItem);
            }
            // items that you can sell to NPCs
            command = new SQLiteCommand("SELECT itemid, vendorid, value FROM SellItems;", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                ItemSold sellItem = new ItemSold();
                int itemid = reader.GetInt32(0);
                int vendorid = reader.GetInt32(1);
                sellItem.price = reader.GetInt32(2);

                Item item = itemIdMap[itemid];
                NPC npc = npcIdMap[vendorid];
                sellItem.npc = npc;
                sellItem.item = item;
                item.sellItems.Add(sellItem);
                npc.sellItems.Add(sellItem);
            }

            // Hunting Places
            command = new SQLiteCommand("SELECT id, name, level, exprating, lootrating, image, city FROM HuntingPlaces;", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                HuntingPlace huntingPlace = new HuntingPlace();
                huntingPlace.id = reader.GetInt32(0);
                huntingPlace.name = reader["name"].ToString();
                huntingPlace.level = reader.IsDBNull(2) ? DATABASE_NULL : reader.GetInt32(2);
                huntingPlace.exp_quality = reader.IsDBNull(3) ? DATABASE_NULL : reader.GetInt32(3);
                huntingPlace.loot_quality = reader.IsDBNull(4) ? DATABASE_NULL : reader.GetInt32(4);
                string imageName = reader.GetString(5).ToLower();
                if (creatureNameMap.ContainsKey(imageName)) {
                    huntingPlace.image = creatureNameMap[imageName].image;
                } else {
                    huntingPlace.image = npcNameMap[imageName].image;
                }
                huntingPlace.city = reader["city"].ToString();

                string huntNameLower = huntingPlace.name.ToLower();
                huntingPlaceNameMap.Add(huntingPlace.name.ToLower(), huntingPlace);
                huntingPlaceIdMap.Add(huntingPlace.id, huntingPlace);
            }

            // Coordinates for hunting places
            command = new SQLiteCommand("SELECT huntingplaceid, x, y, z FROM HuntingPlaceCoordinates", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                Coordinate c = new Coordinate();
                int huntingplaceid = reader.GetInt32(0);
                c.x = reader.IsDBNull(1) ? DATABASE_NULL : (int)(Coordinate.MaxWidth * reader.GetFloat(1));
                c.y = reader.IsDBNull(2) ? DATABASE_NULL : (int)(Coordinate.MaxHeight * reader.GetFloat(2));
                c.z = reader.IsDBNull(3) ? DATABASE_NULL : reader.GetInt32(3);
                if (huntingPlaceIdMap.ContainsKey(huntingplaceid)) huntingPlaceIdMap[huntingplaceid].coordinates.Add(c);
            }

            // Hunting place directions
            command = new SQLiteCommand("SELECT huntingplaceid, beginx, beginy, beginz,endx, endy, endz, ordering, description FROM HuntDirections ORDER BY ordering", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                Directions d = new Directions();
                d.huntingplaceid = reader.GetInt32(0);
                d.begin = new Coordinate(reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3));
                d.end = new Coordinate(reader.GetInt32(4), reader.GetInt32(5), reader.GetInt32(6));
                d.ordering = reader.GetInt32(7);
                d.description = reader["description"].ToString();
                if (huntingPlaceIdMap.ContainsKey(d.huntingplaceid)) huntingPlaceIdMap[d.huntingplaceid].directions.Add(d);
            }

            // Hunting place creatures
            command = new SQLiteCommand("SELECT huntingplaceid, creatureid FROM HuntingPlaceCreatures", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                int huntingplaceid = reader.GetInt32(0);
                int creatureid = reader.GetInt32(1);
                if (huntingPlaceIdMap.ContainsKey(huntingplaceid) && creatureIdMap.ContainsKey(creatureid)) {
                    huntingPlaceIdMap[huntingplaceid].creatures.Add(creatureIdMap[creatureid]);
                }
            }
            foreach (HuntingPlace h in huntingPlaceIdMap.Values) {
                h.creatures = h.creatures.OrderBy(o => o.experience).ToList();
            }

            // Spells
            command = new SQLiteCommand("SELECT id, name, words, element, cooldown, premium, promotion, levelrequired, goldcost, manacost, knight, paladin, sorcerer, druid, image FROM Spells", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                Spell spell = new Spell();
                spell.id = reader.GetInt32(0);
                spell.name = reader["name"].ToString();
                spell.words = reader["words"].ToString();
                spell.element = reader.IsDBNull(3) ? "Unknown" : reader.GetString(3);
                spell.cooldown = reader.IsDBNull(4) ? DATABASE_NULL : reader.GetInt32(4);
                spell.premium = reader.GetBoolean(5);
                spell.promotion = reader.GetBoolean(6);
                spell.levelrequired = reader.GetInt32(7);
                spell.goldcost = reader.GetInt32(8);
                spell.manacost = reader.GetInt32(9);
                spell.knight = reader.GetBoolean(10);
                spell.paladin = reader.GetBoolean(11);
                spell.sorcerer = reader.GetBoolean(12);
                spell.druid = reader.GetBoolean(13);
                spell.image = Image.FromStream(reader.GetStream(14));

                spellIdMap.Add(spell.id, spell);
                spellNameMap.Add(spell.name.ToLower(), spell);
            }

            // Spell NPCs
            command = new SQLiteCommand("SELECT spellid, npcid, paladin, knight, druid, sorcerer FROM SpellNPCs", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                int spellid = reader.GetInt32(0);
                int npcid = reader.GetInt32(1);
                bool paladin = reader.GetBoolean(2);
                bool knight = reader.GetBoolean(3);
                bool druid = reader.GetBoolean(4);
                bool sorcerer = reader.GetBoolean(5);

                Spell spell = spellIdMap[spellid];
                NPC npc = npcIdMap[npcid];

                SpellTaught teach = new SpellTaught();
                teach.spell = spell;
                teach.npc = npc;
                teach.paladin = paladin;
                teach.knight = knight;
                teach.sorcerer = sorcerer;
                teach.druid = druid;

                npc.spellsTaught.Add(teach);
                spell.teachNPCs.Add(teach);
            }

            // Outfits
            command = new SQLiteCommand("SELECT id, title, name, premium FROM Outfits", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                Outfit outfit = new Outfit();
                outfit.id = reader.GetInt32(0);
                outfit.title = reader.GetString(1);
                outfit.name = reader.GetString(2);
                outfit.premium = reader.GetBoolean(3);
                outfitNameMap.Add(outfit.name.ToLower(), outfit);
                outfitIdMap.Add(outfit.id, outfit);
            }

            // Outfit Images
            command = new SQLiteCommand("SELECT outfitid, male, addon, image FROM OutfitImages", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                int outfitid = reader.GetInt32(0);
                bool male = reader.GetBoolean(1);
                int addon = reader.GetInt32(2);
                Image image = Image.FromStream(reader.GetStream(3));

                if (male) {
                    outfitIdMap[outfitid].maleImages[addon] = image;
                } else {
                    outfitIdMap[outfitid].femaleImages[addon] = image;
                }
            }

            // Mounts
            command = new SQLiteCommand("SELECT id, title, name, tameitemid, tamecreatureid, speed, tibiastore, image FROM Mounts", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                Mount mount = new Mount();
                mount.id = reader.GetInt32(0);
                mount.title = reader.GetString(1);
                mount.name = reader.GetString(2);

                int tameitem = reader.IsDBNull(3) ? DATABASE_NULL : reader.GetInt32(3);
                if (tameitem > 0) mount.tameitem = itemIdMap[tameitem];
                int tamecreature = reader.IsDBNull(4) ? DATABASE_NULL : reader.GetInt32(4);
                if (tamecreature > 0) mount.tamecreature = creatureIdMap[tamecreature];
                mount.speed = reader.GetInt32(5);
                mount.tibiastore = reader.GetBoolean(6);
                mount.image = Image.FromStream(reader.GetStream(7));

                mountIdMap.Add(mount.id, mount);
                mountNameMap.Add(mount.name.ToLower(), mount);
            }

            // Quests
            command = new SQLiteCommand("SELECT id, title, name, minlevel, premium, city, legend FROM Quests", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                Quest quest = new Quest();
                quest.id = reader.GetInt32(0);
                quest.title = reader.GetString(1);
                quest.name = reader.GetString(2);
                quest.minlevel = reader.GetInt32(3);
                quest.premium = reader.GetBoolean(4);
                quest.city = reader.IsDBNull(5) ? "Unknown" : reader.GetString(5);
                quest.legend = reader.IsDBNull(6) ? "No legend available." : reader.GetString(6);
                if (quest.legend == "..." || quest.legend == "")
                    quest.legend = "No legend available.";

                questIdMap.Add(quest.id, quest);
                questNameMap.Add(quest.name.ToLower(), quest);
            }

            // Quest Rewards
            command = new SQLiteCommand("SELECT questid, itemid FROM QuestRewards", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                questIdMap[reader.GetInt32(0)].rewardItems.Add(itemIdMap[reader.GetInt32(1)]);
            }

            // Quest Outfits
            command = new SQLiteCommand("SELECT questid, outfitid FROM QuestOutfits", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                int questid = reader.GetInt32(0);
                int outfitid = reader.GetInt32(1);
                if (outfitIdMap.ContainsKey(outfitid)) {
                    questIdMap[questid].rewardOutfits.Add(outfitIdMap[outfitid]);
                    outfitIdMap[outfitid].quest = questIdMap[questid];
                }
            }

            // Quest Dangers
            command = new SQLiteCommand("SELECT questid, creatureid FROM QuestDangers", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                questIdMap[reader.GetInt32(0)].questDangers.Add(creatureIdMap[reader.GetInt32(1)]);
            }

            // Quest Item Requirements
            command = new SQLiteCommand("SELECT questid, count, itemid FROM QuestItemRequirements", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                questIdMap[reader.GetInt32(0)].questRequirements.Add(new Tuple<int, Item>(reader.GetInt32(1), itemIdMap[reader.GetInt32(2)]));
            }

            // Quest Additional Requirements
            command = new SQLiteCommand("SELECT questid, requirementtext FROM QuestAdditionalRequirements", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                questIdMap[reader.GetInt32(0)].additionalRequirements.Add(reader.GetString(1));
            }

            // Quest Instructions
            command = new SQLiteCommand("SELECT questid, beginx, beginy, beginz, endx, endy, endz, description, ordering FROM QuestInstructions ORDER BY ordering", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                QuestInstruction instruction = new QuestInstruction();
                instruction.quest = questIdMap[reader.GetInt32(0)];
                instruction.begin = new Coordinate(reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3));
                if (reader.IsDBNull(4)) {
                    instruction.end = new Coordinate(DATABASE_NULL, DATABASE_NULL, reader.GetInt32(6));
                } else {
                    instruction.end = new Coordinate(reader.GetInt32(4), reader.GetInt32(5), reader.GetInt32(6));
                }
                instruction.description = reader.GetString(7);
                instruction.ordering = reader.GetInt32(8);

                instruction.quest.questInstructions.Add(instruction);
            }
            // Hunting place requirements
            command = new SQLiteCommand("SELECT huntingplaceid, questid, requirementtext FROM HuntRequirements", conn);
            reader = command.ExecuteReader();
            while (reader.Read()) {
                Requirements r = new Requirements();
                r.huntingplaceid = reader.GetInt32(0);
                int questid = reader.IsDBNull(1) ? DATABASE_NULL : reader.GetInt32(1);
                r.quest = questIdMap[questid];
                r.notes = reader["requirementtext"].ToString();
                if (huntingPlaceIdMap.ContainsKey(r.huntingplaceid)) huntingPlaceIdMap[r.huntingplaceid].requirements.Add(r);
            }
        }

        void initializePluralMap() {
            StreamReader reader = new StreamReader(pluralMapFile);
            string line;
            while ((line = reader.ReadLine()) != null) {
                if (line.Contains('=')) {
                    string[] split = line.Split('=');
                    if (!pluralMap.ContainsKey(split[0])) {
                        pluralMap.Add(split[0], split[1]);
                    }
                }
            }
            reader.Close();
        }

        class Hunt {
            public string name;
            public bool temporary;
            public bool trackAllCreatures;
            public string trackedCreatures;
            public Loot loot = new Loot();

            public override string ToString() {
                return name + "#" + trackAllCreatures.ToString() + "#" + trackedCreatures.Replace("\n", "#");
            }
        };

        class Loot {
            public Dictionary<string, List<string>> logMessages = new Dictionary<string, List<string>>();
            public Dictionary<Creature, Dictionary<Item, int>> creatureLoot = new Dictionary<Creature, Dictionary<Item, int>>();
            public Dictionary<Creature, int> killCount = new Dictionary<Creature, int>();
        };

        private Hunt activeHunt = null;
        List<Hunt> hunts = new List<Hunt>();
        bool showNotifications = true;
        bool showNotificationsValue = true;
        bool showNotificationsSpecific = false;
        bool lootNotificationRich = false;
        bool copyAdvances = true;
        bool simpleNotifications = true;
        bool richNotifications = true;
        public int notificationLength = 20;

        public Dictionary<string, List<string>> settings = new Dictionary<string, List<string>>();
        void loadSettings() {
            string line;
            string currentSetting = null;

            StreamReader file = new StreamReader(settingsFile);
            while ((line = file.ReadLine()) != null) {
                if (line.Length == 0) continue;
                if (line[0] == '@') {
                    currentSetting = line.Substring(1, line.Length - 1);
                    if (!settings.ContainsKey(currentSetting))
                        settings.Add(currentSetting, new List<string>());
                } else if (currentSetting != null) {
                    settings[currentSetting].Add(line);
                }
            }
            file.Close();
        }

        void saveSettings() {
            StreamWriter file = new StreamWriter(settingsFile);
            foreach (KeyValuePair<string, List<string>> pair in settings) {
                file.WriteLine("@" + pair.Key);
                foreach (string str in pair.Value) {
                    file.WriteLine(str);
                }
            }
            file.Close();
        }

        void initializeNames() {
            if (!settings.ContainsKey("Names")) settings.Add("Names", new List<string>());

            string massiveString = "";
            foreach (string str in settings["Names"]) {
                massiveString += str + "\n";
            }
            this.nameTextBox.Text = massiveString;
        }

        void initializeHunts() {
            //"Name#Track#Creature#Creature#Creature#Creature"
            if (!settings.ContainsKey("Hunts")) {
                settings.Add("Hunts", new List<string>() { "New Hunt#True#" });
            }
            int activeHuntIndex = 0, index = 0;
            foreach (string str in settings["Hunts"]) {
                SQLiteCommand command; SQLiteDataReader reader;
                Hunt hunt = new Hunt();
                string[] splits = str.Split('#');
                if (splits.Length >= 3) {
                    hunt.name = splits[0];
                    hunt.trackAllCreatures = splits[1] == "True";
                    hunt.temporary = false;
                    string massiveString = "";
                    for (int i = 2; i < splits.Length; i++) {
                        if (splits[i].Length > 0) {
                            massiveString += splits[i] + "\n";
                        }
                    }
                    hunt.trackedCreatures = massiveString;
                    // set this hunt to the active hunt if it is the active hunt
                    if (settings.ContainsKey("ActiveHunt") && settings["ActiveHunt"].Count > 0 && settings["ActiveHunt"][0] == hunt.name)
                        activeHuntIndex = index;

                    // create the hunt table if it does not exist
                    command = new SQLiteCommand(String.Format("CREATE TABLE IF NOT EXISTS \"{0}\"(day INTEGER, hour INTEGER, minute INTEGER, message STRING);", hunt.name.ToLower()), conn);
                    command.ExecuteNonQuery();
                    // load the data for the hunt from the database
                    command = new SQLiteCommand(String.Format("SELECT message FROM \"{0}\" ORDER BY day, hour, minute;", hunt.name.ToLower()), conn);
                    reader = command.ExecuteReader();
                    while (reader.Read()) {
                        string message = reader["message"].ToString();
                        Tuple<Creature, List<Tuple<Item, int>>> resultList = ParseLootMessage(message);
                        if (resultList == null) continue;

                        string t = message.Substring(0, 5);
                        if (!hunt.loot.logMessages.ContainsKey(t)) hunt.loot.logMessages.Add(t, new List<string>());
                        hunt.loot.logMessages[t].Add(message);

                        Creature cr = resultList.Item1;
                        if (!hunt.loot.creatureLoot.ContainsKey(cr)) hunt.loot.creatureLoot.Add(cr, new Dictionary<Item, int>());
                        foreach (Tuple<Item, int> tpl in resultList.Item2) {
                            Item item = tpl.Item1;
                            int count = tpl.Item2;
                            if (!hunt.loot.creatureLoot[cr].ContainsKey(item)) hunt.loot.creatureLoot[cr].Add(item, count);
                            else hunt.loot.creatureLoot[cr][item] += count;
                        }
                        if (!hunt.loot.killCount.ContainsKey(cr)) hunt.loot.killCount.Add(cr, 1);
                        else hunt.loot.killCount[cr] += 1;
                    }
                    hunts.Add(hunt);
                    index++;
                }
            }

            skip_hunt_refresh = true;
            huntBox.Items.Clear();
            foreach (Hunt h in hunts) {
                huntBox.Items.Add(h.name);
            }
            activeHunt = hunts[activeHuntIndex];
            skip_hunt_refresh = false;
            huntBox.SelectedIndex = activeHuntIndex;
        }

        void initializeSettings() {
            this.notificationLength = getSettingInt("NotificationDuration") < 0 ? notificationLength : getSettingInt("NotificationDuration");
            this.simpleNotifications = getSettingBool("EnableSimpleNotifications");
            this.richNotifications = getSettingBool("EnableRichNotifications");
            this.copyAdvances = getSettingBool("CopyAdvances");
            this.showNotifications = getSettingBool("ShowNotifications");
            this.lootNotificationRich = getSettingBool("UseRichNotificationType");
            this.showNotificationsValue = getSettingBool("ShowNotificationsValue");
            this.notification_value = getSettingInt("NotificationValue") < 0 ? notification_value : getSettingInt("NotificationValue");
            this.showNotificationsSpecific = getSettingBool("ShowNotificationsSpecific");

            this.richNotificationsPanel.Enabled = richNotifications;
            this.notificationPanel.Enabled = showNotifications;
            this.specificNotificationTextbox.Enabled = showNotificationsSpecific;
            this.notificationLabel.Text = "Notification Length: " + notificationLength.ToString() + " Seconds";

            this.notificationLengthSlider.Value = notificationLength;
            this.enableSimpleNotifications.Checked = simpleNotifications;
            this.enableRichNotificationsCheckbox.Checked = richNotifications;
            this.advanceCopyCheckbox.Checked = copyAdvances;
            this.showNotificationCheckbox.Checked = showNotifications;
            this.notificationTypeBox.SelectedIndex = lootNotificationRich ? 1 : 0;
            this.rareDropNotificationValueCheckbox.Checked = showNotificationsValue;
            this.notificationValue.Text = notification_value.ToString();
            this.specificNotificationCheckbox.Checked = showNotificationsSpecific;
            this.lookCheckBox.Checked = getSettingBool("LookMode");
            this.alwaysShowLoot.Checked = getSettingBool("AlwaysShowLoot");
            this.startAutohotkeyScript.Checked = getSettingBool("StartAutohotkeyAutomatically");
            this.shutdownOnExit.Checked = getSettingBool("ShutdownAutohotkeyOnExit");
            string massiveString = "";
            if (settings.ContainsKey("NotificationItems")) {
                foreach (string str in settings["NotificationItems"]) {
                    massiveString += str + "\n";
                }
            }
            this.specificNotificationTextbox.Text = massiveString;
            massiveString = "";
            if (settings.ContainsKey("AutoHotkeySettings")) {
                foreach (string str in settings["AutoHotkeySettings"]) {
                    massiveString += str + "\n";
                }
            }
            this.autoHotkeyGridSettings.Text = massiveString;
            (this.autoHotkeyGridSettings as RichTextBoxAutoHotkey).RefreshSyntax();

            this.autoScreenshotAdvance.Checked = getSettingBool("AutoScreenshotAdvance");
            this.autoScreenshotDrop.Checked = getSettingBool("AutoScreenshotItemDrop");
            this.autoScreenshotDeath.Checked = getSettingBool("AutoScreenshotDeath");

            this.enableScreenshotBox.Checked = getSettingBool("EnableScreenshots");
            this.screenshotPanel.Enabled = enableScreenshotBox.Checked;
            if (getSettingString("ScreenshotPath") == null) {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                setSetting("ScreenshotPath", path);
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
            }

            screenshotDirectoryBox.Text = getSettingString("ScreenshotPath");
            refreshScreenshots();
        }

        void makeDraggable(Control.ControlCollection controls) {
            foreach (Control c in controls) {
                if (c == this.closeButton || c == this.minimizeButton) continue;
                if (c is Label || c is Panel) {
                    c.MouseDown += new System.Windows.Forms.MouseEventHandler(this.draggable_MouseDown);
                }
                if (c is Panel || c is TabPage || c is TabControl) {
                    makeDraggable(c.Controls);
                }
            }
        }

        System.Timers.Timer circleTimer = null;
        void bw_DoWork(object sender, DoWorkEventArgs e) {
            while (keep_working) {
                if (circleTimer == null) {
                    circleTimer = new System.Timers.Timer(1000);
                    circleTimer.Elapsed += circleTimer_Elapsed;
                    circleTimer.Enabled = true;
                }
                bool success = ScanMemory();
                circleTimer.Dispose();
                circleTimer = null;
                if (success) {
                    if (this.current_state != ScanningState.Scanning) {
                        this.current_state = ScanningState.Scanning;
                        this.BeginInvoke((MethodInvoker)delegate {
                            this.loadTimerImage.Image = this.loadingbar;
                            this.loadTimerImage.Enabled = true;
                            scan_tooltip.SetToolTip(this.loadTimerImage, "Scanning Memory...");
                        });
                    }
                } else {
                    if (this.current_state != ScanningState.NoTibia) {
                        this.current_state = ScanningState.NoTibia;
                        this.BeginInvoke((MethodInvoker)delegate {
                            this.loadTimerImage.Image = this.loadingbarred;
                            this.loadTimerImage.Enabled = true;
                            scan_tooltip.SetToolTip(this.loadTimerImage, "No Tibia Client Found...");
                        });
                    }
                }
            }
        }

        void circleTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
            if (this.current_state != ScanningState.Stuck) {
                this.current_state = ScanningState.Stuck;
                this.Invoke((MethodInvoker)delegate {
                    this.loadTimerImage.Image = this.loadingbargray;
                    scan_tooltip.SetToolTip(this.loadTimerImage, "Waiting, possibly stuck...");
                    this.loadTimerImage.Enabled = false;
                });
            }
        }


        public static string ToTitle(string str) {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(str);
        }

        private void initializeMaps() {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM WorldMap", conn);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                Map m = new Map();
                m.z = reader.GetInt32(0);
                m.image = new Bitmap(Image.FromStream(reader.GetStream(1)));
                map_files.Add(m);
            }
        }

        private void ShowSimpleNotification(string title, string text, Image image) {
            if (!simpleNotifications) return;
            notifyIcon1.BalloonTipText = text;
            notifyIcon1.BalloonTipTitle = title;
            notifyIcon1.Icon = Icon.FromHandle(((Bitmap)image).GetHicon());
            notifyIcon1.ShowBalloonTip(5000);
        }

        public void CloseNotification() {
            if (tooltipForm != null) {
                tooltipForm.Close();
            }
        }


        bool clearSimpleNotifications = false;
        int notificationSpacing = 5;
        List<SimpleNotification> notificationStack = new List<SimpleNotification>();
        private void ShowSimpleNotification(SimpleNotification f) {
            int position_x = 0, position_y = 0;
            Screen screen;
            Process[] tibia_process = Process.GetProcessesByName("Tibia");
            if (tibia_process.Length == 0) {
                screen = Screen.FromControl(this);
            } else {
                Process tibia = tibia_process[0];
                screen = Screen.FromHandle(tibia.MainWindowHandle);
            }
            position_x = screen.WorkingArea.Right - f.Width - notificationSpacing;
            int basePosition = screen.WorkingArea.Bottom;
            foreach (SimpleNotification notification in notificationStack) {
                basePosition -= notification.Height + notificationSpacing;
            }
            position_y = basePosition - (f.Height + notificationSpacing);
            f.StartPosition = FormStartPosition.Manual;
            f.SetDesktopLocation(position_x + f.Width + notificationSpacing, position_y);
            Console.WriteLine(position_y);
            f.targetPositionX = position_x;
            f.targetPositionY = position_y;
            f.FormClosed += simpleNotificationClosed;

            notificationStack.Add(f);

            f.TopMost = true;
            f.Show();
        }

        private void ClearSimpleNotifications() {
            clearSimpleNotifications = true;
            foreach (SimpleNotification f in notificationStack) {
                f.ClearTimers();
                f.Close();
            }
            notificationStack.Clear();
            clearSimpleNotifications = false;
        }

        private void simpleNotificationClosed(object sender, FormClosedEventArgs e) {
            if (clearSimpleNotifications) return;
            SimpleNotification notification = sender as SimpleNotification;
            if (notification == null) return;
            bool moveDown = false;
            int positionModification = 0;
            foreach (SimpleNotification f in notificationStack) {
                if (f == notification) {
                    positionModification = f.Height + notificationSpacing;
                    moveDown = true;
                } else if (moveDown) {
                    f.targetPositionY += positionModification;
                }
            }
            notificationStack.Remove(notification);
        }

        private void ShowNotification(NotificationForm f, string command, string screenshot_path = "") {
            if (!richNotifications) return;

            f.LoadForm();
            if (screenshot_path != "") {
                f.Visible = false;
                Bitmap bitmap = new Bitmap(f.Width, f.Height);
                f.DrawToBitmap(bitmap, new Rectangle(0, 0, f.Width, f.Height));
                foreach (Control c in f.Controls) {
                    c.DrawToBitmap(bitmap, new Rectangle(new Point(Math.Min(Math.Max(c.Location.X, 0), f.Width), Math.Min(Math.Max(c.Location.Y, 0), f.Height)), c.Size));
                }
                bitmap.Save(screenshot_path);
                bitmap.Dispose();
                f.Dispose();
                return;
            }
            command_stack.Push(command);
            Console.WriteLine(command_stack.Count);
            if (tooltipForm != null) {
                tooltipForm.Close();
            }
            int position_x = 0, position_y = 0;
            Screen screen;
            Process[] tibia_process = Process.GetProcessesByName("Tibia");
            if (tibia_process.Length == 0) {
                screen = Screen.FromControl(this);
            } else {
                Process tibia = tibia_process[0];
                screen = Screen.FromHandle(tibia.MainWindowHandle);
            }
            position_x = screen.WorkingArea.Left + 30;
            position_y = screen.WorkingArea.Top + 30;
            f.StartPosition = FormStartPosition.Manual;
            f.SetDesktopLocation(position_x, position_y);
            f.TopMost = true;
            f.Show();
            tooltipForm = f;
        }

        public void Back() {
            if (command_stack.Count <= 1) return;
            command_stack.Pop(); // remove the current command
            string command = command_stack.Pop();
            this.ExecuteCommand(command);
        }

        public bool HasBack() {
            return command_stack.Count > 1;
        }

        private void ShowCreatureDrops(Creature c, string comm) {
            if (c == null) return;
            CreatureDropsForm f = new CreatureDropsForm();
            f.creature = c;

            ShowNotification(f, comm);
        }

        private void ShowCreatureStats(Creature c, string comm) {
            if (c == null) return;
            CreatureStatsForm f = new CreatureStatsForm();
            f.creature = c;

            ShowNotification(f, comm);
        }
        private void ShowCreatureList(List<TibiaObject> c, string title, string prefix, string comm) {
            if (c == null) return;
            CreatureList f = new CreatureList();
            f.objects = c;
            f.title = title;
            f.prefix = prefix;

            ShowNotification(f, comm);
        }

        private void ShowItemView(Item i, Dictionary<NPC, int> BuyNPCs, Dictionary<NPC, int> SellNPCs, List<Creature> creatures, string comm) {
            if (i == null) return;
            ItemViewForm f = new ItemViewForm();
            f.item = i;
            f.buyNPCs = BuyNPCs;
            f.sellNPCs = SellNPCs;
            f.creatures = creatures;

            ShowNotification(f, comm);
        }

        private void ShowNPCForm(NPC c, string comm) {
            if (c == null) return;
            NPCForm f = new NPCForm();
            f.npc = c;

            ShowNotification(f, comm);
        }

        private void ShowDamageMeter(Dictionary<string, int> dps, string comm, string filter = "", string screenshot_path = "") {
            DamageChart f = new DamageChart();
            f.dps = dps;
            f.filter = filter;

            ShowNotification(f, comm, screenshot_path);
        }

        private void ShowLootDrops(Dictionary<Creature, int> creatures, List<Tuple<Item, int>> items, string comm, string screenshot_path) {
            LootDropForm ldf = new LootDropForm();
            ldf.creatures = creatures;
            ldf.items = items;
            
            ShowNotification(ldf, comm, screenshot_path);
        }

        private void ShowHuntList(List<HuntingPlace> h, string header, string comm) {
            if (h != null) h = h.OrderBy(o => o.level).ToList();
            HuntListForm f = new HuntListForm();
            f.hunting_places = h;
            f.header = header;

            ShowNotification(f, comm);
        }

        private void ShowHuntingPlace(HuntingPlace h, string comm) {
            HuntingPlaceForm f = new HuntingPlaceForm();
            f.hunting_place = h;

            ShowNotification(f, comm);
        }

        private void ShowSpellNotification(Spell spell, string comm) {
            SpellForm f = new SpellForm(spell);

            ShowNotification(f, comm);
        }

        private void ShowOutfitNotification(Outfit outfit, string comm) {
            OutfitForm f = new OutfitForm(outfit);

            ShowNotification(f, comm);
        }
        private void ShowQuestNotification(Quest quest, string comm) {
            QuestForm f = new QuestForm(quest);

            ShowNotification(f, comm);
        }

        private void ShowQuestList(List<Quest> questList, string header, string comm) {
            if (questList != null) questList = questList.OrderBy(o => o.minlevel).ToList();
            HuntListForm f = new HuntListForm();
            f.quests = questList;
            f.header = header;

            ShowNotification(f, comm);
        }

        private void ShowHuntGuideNotification(HuntingPlace hunt, string comm) {
            if (hunt.directions.Count == 0) return;
            QuestGuideForm f = new QuestGuideForm(hunt);

            ShowNotification(f, comm);
        }

        private void ShowQuestGuideNotification(Quest quest, string comm) {
            QuestGuideForm f = new QuestGuideForm(quest);

            ShowNotification(f, comm);
        }
        private void ShowMountNotification(Mount mount, string comm) {
            MountForm f = new MountForm(mount);

            ShowNotification(f, comm);
        }

        private void ShowListNotification(List<Command> commands, int type, string comm) {
            ListNotification f = new ListNotification(commands);
            f.type = type;

            ShowNotification(f, comm);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            notifyIcon1.Visible = false;
        }


        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void draggable_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        public static int convertX(double x, Rectangle sourceRectangle, Rectangle pictureRectangle) {
            return (int)((x - (double)sourceRectangle.X) / (double)sourceRectangle.Width * (double)pictureRectangle.Width);
        }
        public static int convertY(double y, Rectangle sourceRectangle, Rectangle pictureRectangle) {
            return (int)((y - (double)sourceRectangle.Y) / (double)sourceRectangle.Height * (double)pictureRectangle.Height);
        }


        private static Pen pathPen = new Pen(Color.FromArgb(25, 25, 112), 1);
        private static Pen startPen = new Pen(Color.FromArgb(191, 191, 191), 1);
        private static Pen endPen = new Pen(Color.FromArgb(34, 139, 34), 1);
        public static PictureBox DrawRoute(Coordinate begin, Coordinate end, Size pictureBoxSize, Size minSize, Size maxSize) {
            if (end.x >= 0 && begin.z != end.z) {
                throw new Exception("Can't draw route with different z-coordinates");
            }
            Bitmap mapImage;
            Rectangle sourceRectangle;
            MapPictureBox pictureBox = new MapPictureBox();
            if (pictureBoxSize.Width != 0) {
                pictureBox.Size = pictureBoxSize;
            }
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            if (end.x < 0) {
                if (pictureBoxSize.Width == 0) {
                    pictureBoxSize = new Size(Math.Min(Math.Max(end.z, minSize.Width),maxSize.Width),
                        Math.Min(Math.Max(end.z, minSize.Height), maxSize.Height));
                    pictureBox.Size = pictureBoxSize;
                }
                mapImage = new Bitmap(map_files[begin.z].image);
                using (Graphics gr = Graphics.FromImage(mapImage)) {
                    int crossSize = 4;
                    gr.DrawImage(MainForm.cross_image, new Rectangle(begin.x - crossSize,begin.y - crossSize, crossSize * 2, crossSize * 2));
                }
                pictureBox.mapImage = mapImage;
                pictureBox.sourceWidth = end.z;
                pictureBox.mapCoordinate = new Coordinate(begin.x, begin.y, begin.z);
                pictureBox.zCoordinate = begin.z;
                pictureBox.UpdateMap();
                return pictureBox;

            }

            // First find the route at a high level
            Node beginNode = Pathfinder.GetNode(begin.x, begin.y, begin.z);
            Node endNode = Pathfinder.GetNode(end.x, end.y, end.z);

            List<Rectangle> collisionBounds = null;
            DijkstraNode highresult = Dijkstra.FindRoute(beginNode, endNode);
            if (highresult != null) {
                collisionBounds = new List<Rectangle>();
                while (highresult != null) {
                    highresult.rect.Inflate(5, 5);
                    collisionBounds.Add(highresult.rect);
                    highresult = highresult.previous;
                }
                if (collisionBounds.Count == 0) collisionBounds = null;
            }

            DijkstraPoint result = Dijkstra.FindRoute(map_files[begin.z].image, new Point(begin.x, begin.y), new Point(end.x, end.y), collisionBounds);
            if (result == null) {
                throw new Exception("Couldn't find route.");
            }

            // create a rectangle from the result
            double minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            DijkstraPoint node = result;
            while (node != null) {
                if (node.point.X < minX) minX = node.point.X;
                if (node.point.Y < minY) minY = node.point.Y;
                if (node.point.X > maxX) maxX = node.point.X;
                if (node.point.Y > maxY) maxY = node.point.Y;
                node = node.previous;
            }

            minX -= 10;
            minY -= 10;
            maxX += 10;
            maxY += 10;

            int size = (int)Math.Max(maxX - minX, maxY - minY);
            sourceRectangle = new Rectangle((int)minX, (int)minY, size, size);
            if (pictureBoxSize.Width == 0) {
                pictureBoxSize = new Size(Math.Min(Math.Max(sourceRectangle.Width, minSize.Width), maxSize.Width),
                    Math.Min(Math.Max(sourceRectangle.Height, minSize.Height), maxSize.Height));
                pictureBox.Size = pictureBoxSize;
            }
            mapImage = new Bitmap(map_files[begin.z].image);

            using (Graphics gr = Graphics.FromImage(mapImage)) {
                node = result;
                while (node.previous != null) {
                    gr.DrawLine(pathPen,
                        new Point(node.point.X, node.point.Y),
                        new Point(node.previous.point.X, node.previous.point.Y));
                    node = node.previous;
                }
                int crossSize = 2;
                gr.DrawEllipse(startPen, new Rectangle(begin.x - crossSize, begin.y - crossSize, crossSize * 2, crossSize * 2));
                gr.DrawEllipse(endPen, new Rectangle(end.x - crossSize, end.y - crossSize, crossSize * 2, crossSize * 2));
            }
            pictureBox.mapImage = mapImage;
            pictureBox.sourceWidth = size;
            pictureBox.mapCoordinate = new Coordinate(sourceRectangle.X + sourceRectangle.Width / 2, sourceRectangle.Y + sourceRectangle.Height / 2, begin.z);
            pictureBox.zCoordinate = begin.z;
            pictureBox.UpdateMap();

            return pictureBox;
        }

        public static int DisplayCreatureList(System.Windows.Forms.Control.ControlCollection controls, List<TibiaObject> l, int base_x, int base_y, int max_x, int spacing, bool transparent, Func<TibiaObject, string> tooltip_function = null, float magnification = 1.0f, List<Control> createdControls = null) {
            int x = 0, y = 0;

            int height = 0;
            // add a tooltip that displays the creature names
            ToolTip value_tooltip = new ToolTip();
            value_tooltip.AutoPopDelay = 60000;
            value_tooltip.InitialDelay = 500;
            value_tooltip.ReshowDelay = 0;
            value_tooltip.ShowAlways = true;
            value_tooltip.UseFading = true;
            foreach (TibiaObject cr in l) {
                Image image = cr.GetImage();
                string name = cr.GetName();
                if (max_x < (x + base_x + (int)(image.Width * magnification) + spacing)) {
                    x = 0;
                    y = y + spacing + height;
                    height = 0;
                }
                if ((int)(image.Height * magnification) > height) {
                    height = (int)(image.Height * magnification);
                }
                PictureBox image_box;
                if (transparent) image_box = new PictureBox();
                else image_box = new PictureBox();
                image_box.Image = image;
                image_box.BackColor = Color.Transparent;
                image_box.Size = new Size((int)(image.Width * magnification), height);
                image_box.Location = new Point(base_x + x, base_y + y);
                image_box.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
                image_box.Name = name;
                controls.Add(image_box);
                if (createdControls != null) createdControls.Add(image_box);
                image_box.Image = image;
                if (tooltip_function == null) {
                    value_tooltip.SetToolTip(image_box, System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name));
                } else {
                    value_tooltip.SetToolTip(image_box, tooltip_function(cr));
                }

                x = x + (int)(image.Width * magnification) + spacing;
            }
            x = 0;
            y = y + height;
            return y;
        }

        private void creatureSearch_TextChanged(object sender, EventArgs e) {
            string creature = (sender as TextBox).Text.ToLower();
            this.SuspendLayout();
            this.creaturePanel.Controls.Clear();
            int count = 0;
            DisplayCreatureList(this.creaturePanel.Controls, creatureNameMap.Values.Where(o => o.name.ToLower().Contains(creature) && count++ < 40).ToList<TibiaObject>(), 10, 10, this.creaturePanel.Width - 20, 4, false);
            foreach (Control c in creaturePanel.Controls) {
                if (c is PictureBox) {
                    c.Click += ShowCreatureInformation;
                }
            }
            this.ResumeLayout(false);
        }
        private void itemSearchBox_TextChanged(object sender, EventArgs e) {
            string item = (sender as TextBox).Text;
            this.SuspendLayout();
            this.itemPanel.Controls.Clear();
            int count = 0;
            DisplayCreatureList(this.itemPanel.Controls, itemNameMap.Values.Where(o => o.name.ToLower().Contains(item) && count++ < 40).ToList<TibiaObject>(), 10, 10, this.itemPanel.Width - 20, 4, false);
            foreach (Control c in itemPanel.Controls) {
                if (c is PictureBox) {
                    c.Click += ShowItemInformation;
                }
            }
            this.ResumeLayout(false);
        }

        void ShowCreatureInformation(object sender, EventArgs e) {
            string creature_name = (sender as Control).Name;
            this.ExecuteCommand("creature" + MainForm.commandSymbol + creature_name);
        }

        void ShowItemInformation(object sender, EventArgs e) {
            string item_name = (sender as Control).Name;
            this.ExecuteCommand("item" + MainForm.commandSymbol + item_name);
        }

        private void exportLogButton_Click(object sender, EventArgs e) {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Export Log File";
            if (File.Exists("exported_log")) {
                int i = 1;
                while (File.Exists("exported_log (" + i.ToString() + ")")) i++;
                dialog.FileName = "exported_log (" + i.ToString() + ")";
            } else {
                dialog.FileName = "exported_log";
            }
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {
                saveLog(getSelectedHunt(), dialog.FileName);
            }
        }

        private void resetButton_Click(object sender, EventArgs e) {
            this.ExecuteCommand("reset" + MainForm.commandSymbol);
        }

        private void importLogFile_Click(object sender, EventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Import Log File";
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {
                loadLog(getSelectedHunt(), dialog.FileName);
                refreshHunts();
            }
        }

        private void saveLootImage_Click(object sender, EventArgs e) {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = "png";
            dialog.Title = "Save Loot Image";
            if (File.Exists("loot_screenshot.png")) {
                int i = 1;
                while (File.Exists("loot_screenshot (" + i.ToString() + ").png")) i++;
                dialog.FileName = "loot_screenshot (" + i.ToString() + ").png";
            } else {
                dialog.FileName = "loot_screenshot.png";
            }
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {
                this.ExecuteCommand("loot" + MainForm.commandSymbol + "screenshot" + MainForm.commandSymbol + dialog.FileName.Replace("\\\\", "/").Replace("\\", "/"));
            }

        }

        private void damageButton_Click(object sender, EventArgs e) {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = "png";
            dialog.Title = "Save Damage Image";
            if (File.Exists("damage_screenshot.png")) {
                int i = 1;
                while (File.Exists("damage_screenshot (" + i.ToString() + ").png")) i++;
                dialog.FileName = "damage_screenshot (" + i.ToString() + ").png";
            } else {
                dialog.FileName = "damage_screenshot.png";
            }
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {
                this.ExecuteCommand("damage" + MainForm.commandSymbol + "screenshot" + MainForm.commandSymbol + dialog.FileName.Replace("\\\\", "/").Replace("\\", "/"));
            }
        }

        private void applyRatioButton_Click(object sender, EventArgs e) {
            double val = 0;
            if (double.TryParse(goldRatioTextBox.Text, out val)) {
                this.ExecuteCommand("setdiscardgoldratio" + MainForm.commandSymbol + goldRatioTextBox.Text);
            }
        }

        private void stackableConvertApply_Click(object sender, EventArgs e) {
            double val = 0;
            if (double.TryParse(stackableConvertTextBox.Text, out val)) {
                this.ExecuteCommand("setconvertgoldratio" + MainForm.commandSymbol + "1-" + stackableConvertTextBox.Text);
            }
        }

        private void unstackableConvertApply_Click(object sender, EventArgs e) {
            double val = 0;
            if (double.TryParse(unstackableConvertTextBox.Text, out val)) {
                this.ExecuteCommand("setconvertgoldratio" + MainForm.commandSymbol + "0-" + unstackableConvertTextBox.Text);
            }
        }

        public static void OpenUrl(string str) {
            // Weird command prompt escape characters
            str = str.Trim().Replace(" ", "%20").Replace("&", "^&").Replace("|", "^|").Replace("(", "^(").Replace(")", "^)");
            // Always start with http:// or https://
            if (!str.StartsWith("http://") && !str.StartsWith("https://")) {
                str = "http://" + str;
            }
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/C start " + str);

            procStartInfo.UseShellExecute = true;

            // Do not show the cmd window to the user.
            procStartInfo.CreateNoWindow = true;
            procStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            System.Diagnostics.Process.Start(procStartInfo);
        }

        private void closeButton_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void minimizeButton_Click(object sender, EventArgs e) {
            this.Hide();
            this.minimizeIcon.Visible = true;
            if (minimize_notification) {
                this.minimize_notification = false;
                this.minimizeIcon.ShowBalloonTip(3000);
            }
        }

        private static Color hoverColor = Color.FromArgb(200, 55, 55);
        private static Color normalColor = Color.FromArgb(172, 24, 24);
        private void closeButton_MouseEnter(object sender, EventArgs e) {
            (sender as Control).BackColor = hoverColor;
        }

        private void closeButton_MouseLeave(object sender, EventArgs e) {
            (sender as Control).BackColor = normalColor;
        }

        private static Color minimizeHoverColor = Color.FromArgb(191, 191, 191);
        private static Color minimizeNormalColor = Color.Transparent;
        private void minimizeButton_MouseEnter(object sender, EventArgs e) {
            (sender as Control).BackColor = minimizeHoverColor;
        }

        private void minimizeButton_MouseLeave(object sender, EventArgs e) {
            (sender as Control).BackColor = minimizeNormalColor;
        }

        private void minimizeIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            this.minimizeIcon.Visible = false;
            this.Show();
        }

        private void commandTextBox_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\r') {
                this.ExecuteCommand((sender as TextBox).Text);
                e.Handled = true;
            }
        }

        private void executeCommand_Click(object sender, EventArgs e) {
            this.ExecuteCommand(commandTextBox.Text);
        }

        private Hunt getActiveHunt() {
            return activeHunt;
        }
        private Hunt getSelectedHunt() {
            if (huntBox.SelectedIndex < 0) return null;
            return hunts[huntBox.SelectedIndex];
        }

        bool nameExists(string str) {
            foreach (Hunt h in hunts) {
                if (h.name == str) {
                    return true;
                }
            }
            return false;
        }

        private void newHuntButton_Click(object sender, EventArgs e) {
            Hunt h = new Hunt();
            if (!nameExists("New Hunt")) {
                h.name = "New Hunt";
            } else {
                int index = 1;
                while (nameExists("New Hunt " + index)) index++;
                h.name = "New Hunt " + index;
            }
            activeHunt = h;
            h.trackAllCreatures = true;
            h.trackedCreatures = "";
            hunts.Add(h);
            refreshHunts();
        }

        private void deleteHuntButton_Click(object sender, EventArgs e) {
            if (hunts.Count <= 1) return;
            Hunt h = getSelectedHunt();
            hunts.Remove(h);
            saveHunts();
            refreshHunts(true);
        }

        private void startupHuntCheckbox_CheckedChanged(object sender, EventArgs e) {

        }

        bool skip_hunt_refresh = false;
        bool switch_hunt = false;
        private void huntBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (skip_hunt_refresh) return;
            switch_hunt = true;
            Hunt h = getSelectedHunt();
            this.huntNameBox.Text = h.name;
            trackCreaturesCheckbox.Checked = h.trackAllCreatures;
            if (h == activeHunt) {
                activeHuntButton.Text = "Currently Active";
                activeHuntButton.Enabled = false;
            } else {
                activeHuntButton.Text = "Set As Active Hunt";
                activeHuntButton.Enabled = true;
            }
            trackCreaturesBox.Enabled = !h.trackAllCreatures;
            trackCreaturesBox.Text = h.trackedCreatures;
            refreshHuntImages(h);
            refreshHuntLog(h);
            switch_hunt = false;
        }

        void refreshHuntLog(Hunt h) {
            string massiveString = "";
            List<string> timestamps = h.loot.logMessages.Keys.OrderByDescending(o => o).ToList();
            foreach (string t in timestamps) {
                List<string> strings = h.loot.logMessages[t].ToArray().ToList();
                strings.Reverse();
                foreach (string str in strings) {
                    massiveString += str + "\n";
                }
            }
            this.logMessageTextBox.Text = massiveString;
        }

        void refreshHunts(bool refreshSelection = false) {
            Hunt h = getSelectedHunt();
            int currentHunt = 0;
            skip_hunt_refresh = true;

            huntBox.Items.Clear();
            foreach (Hunt hunt in hunts) {
                huntBox.Items.Add(hunt.name);
                if (hunt == h) currentHunt = huntBox.Items.Count - 1;
            }
            huntBox.SelectedIndex = refreshSelection ? 0 : currentHunt;
            activeHunt = hunts[huntBox.SelectedIndex];

            skip_hunt_refresh = false;
            huntBox_SelectedIndexChanged(huntBox, null);
        }

        void saveHunts() {
            List<string> huntStrings = new List<string>();
            foreach (Hunt hunt in hunts) {
                if (hunt.temporary) continue;
                huntStrings.Add(hunt.ToString());
            }
            settings["Hunts"] = huntStrings;
            if (activeHunt != null) {
                setSetting("ActiveHunt", activeHunt.name);
            }
            saveSettings();
        }

        private void huntNameBox_TextChanged(object sender, EventArgs e) {
            if (switch_hunt) return;
            Hunt h = getSelectedHunt();
            string oldTable = h.name;
            string newTable = (sender as TextBox).Text;
            if (oldTable == newTable || newTable.Length <= 0) return;
            h.name = newTable;
            SQLiteCommand comm = new SQLiteCommand(String.Format("ALTER TABLE \"{0}\" RENAME TO \"{1}\";", oldTable, h.name), conn);
            comm.ExecuteNonQuery();
            saveHunts();
            refreshHunts();
        }

        private void activeHuntButton_Click(object sender, EventArgs e) {
            if (switch_hunt) return;
            Hunt h = getSelectedHunt();
            activeHuntButton.Text = "Currently Active";
            activeHuntButton.Enabled = false;
            activeHunt = h;
            saveHunts();
        }

        List<string> lootCreatures = new List<string>();
        void refreshHuntImages(Hunt h) {
            int spacing = 4;
            string[] creatures = h.trackedCreatures.Split('\n');
            List<TibiaObject> creatureObjects = new List<TibiaObject>();
            int totalWidth = spacing + spacing;
            int maxHeight = -1;
            foreach (string cr in creatures) {
                string name = cr.ToLower();
                if (creatureNameMap.ContainsKey(name) && !creatureObjects.Any(item => item.GetName() == name)) {
                    Creature cc = creatureNameMap[name];
                    totalWidth += cc.image.Width + spacing;
                    maxHeight = Math.Max(maxHeight, cc.image.Height);
                    creatureObjects.Add(cc);
                    lootCreatures.Add(name);
                }
            }
            float magnification = 1.0f;
            if (totalWidth < creatureImagePanel.Width) {
                // fits on one line
                magnification = ((float)creatureImagePanel.Width) / totalWidth;
                //also consider the height
                float maxMagnification = ((float)creatureImagePanel.Height) / maxHeight;
                if (magnification > maxMagnification) magnification = maxMagnification;
            } else if (totalWidth < creatureImagePanel.Width * 2) {
                // make it fit on two lines
                magnification = (creatureImagePanel.Width * 1.7f) / totalWidth;
                //also consider the height
                float maxMagnification = creatureImagePanel.Height / (maxHeight * 2.0f);
                if (magnification > maxMagnification) magnification = maxMagnification;
            } else {
                // make it fit on three lines
                magnification = (creatureImagePanel.Width * 2.7f) / totalWidth;
                //also consider the height
                float maxMagnification = creatureImagePanel.Height / (maxHeight * 3.0f);
                if (magnification > maxMagnification) magnification = maxMagnification;
            }
            creatureImagePanel.Controls.Clear();
            DisplayCreatureList(creatureImagePanel.Controls, creatureObjects, 0, 0, creatureImagePanel.Width, spacing, false, null, magnification);
        }

        private void trackCreaturesBox_TextChanged(object sender, EventArgs e) {
            if (switch_hunt) return;
            Hunt h = hunts[huntBox.SelectedIndex];
            h.trackedCreatures = (sender as RichTextBox).Text;

            saveHunts();
            refreshHuntImages(h);
        }

        private void trackCreaturesCheckbox_CheckedChanged(object sender, EventArgs e) {
            if (switch_hunt) return;
            bool chk = (sender as CheckBox).Checked;
            this.creatureTrackLabel.Visible = !chk;
            this.trackCreaturesBox.Enabled = !chk;

            Hunt h = getActiveHunt();
            h.trackAllCreatures = chk;

            saveHunts();
        }

        private bool getSettingBool(string key) {
            if (!settings.ContainsKey(key) || settings[key].Count == 0) return false;
            return settings[key][0] == "True";
        }

        private int getSettingInt(string key) {
            if (!settings.ContainsKey(key) || settings[key].Count == 0) return -1;
            int v;
            if (int.TryParse(settings[key][0], out v)) {
                return v;
            }
            return -1;
        }

        private string getSettingString(string key) {
            if (!settings.ContainsKey(key) || settings[key].Count == 0) return null;
            return settings[key][0];
        }

        private void setSetting(string key, string value) {
            if (!settings.ContainsKey(key)) settings.Add(key, new List<string>());
            settings[key].Clear();
            settings[key].Add(value);
        }

        private void rareDropNotificationValueCheckbox_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("ShowNotificationsValue", (sender as CheckBox).Checked.ToString());
            saveSettings();

            this.showNotificationsValue = (sender as CheckBox).Checked;
        }

        private void notificationValue_TextChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;
            int value;
            if (int.TryParse((sender as TextBox).Text, out value)) {
                this.notification_value = value;
                setSetting("NotificationValue", notification_value.ToString());
                saveSettings();
            }
        }

        private void notificationSpecific_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("ShowNotificationsSpecific", (sender as CheckBox).Checked.ToString());
            saveSettings();

            this.showNotificationsSpecific = (sender as CheckBox).Checked;
            specificNotificationTextbox.Enabled = (sender as CheckBox).Checked;
        }

        private void specificNotificationTextbox_TextChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;
            List<string> names = new List<string>();

            string[] lines = (sender as RichTextBox).Text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
                names.Add(lines[i].ToLower());
            settings["NotificationItems"] = names;

            saveSettings();
        }

        private void showNotificationCheckbox_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;
            string chk = (sender as CheckBox).Checked.ToString();

            setSetting("ShowNotifications", chk);
            saveSettings();

            this.showNotifications = (sender as CheckBox).Checked;

            notificationPanel.Enabled = (sender as CheckBox).Checked;
        }

        private void notificationTypeBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("UseRichNotificationType", ((sender as ComboBox).SelectedIndex == 1).ToString());
            saveSettings();

            this.lootNotificationRich = (sender as ComboBox).SelectedIndex == 1;
        }

        private void notificationLengthSlider_Scroll(object sender, EventArgs e) {
            if (prevent_settings_update) return;
            setSetting("NotificationDuration", (sender as TrackBar).Value.ToString());
            saveSettings();

            this.notificationLength = (sender as TrackBar).Value;
            this.notificationLabel.Text = "Notification Length: " + notificationLength.ToString() + " Seconds";
        }

        private void enableRichNotificationsCheckbox_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("EnableRichNotifications", (sender as CheckBox).Checked.ToString());
            saveSettings();

            this.richNotifications = (sender as CheckBox).Checked;

            richNotificationsPanel.Enabled = (sender as CheckBox).Checked;
        }

        private void enableSimpleNotifications_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("EnableSimpleNotifications", (sender as CheckBox).Checked.ToString());
            saveSettings();

            this.simpleNotifications = (sender as CheckBox).Checked;
        }

        private void advanceCopyCheckbox_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("CopyAdvances", (sender as CheckBox).Checked.ToString());
            saveSettings();

            this.copyAdvances = (sender as CheckBox).Checked;
        }

        private void nameTextBox_TextChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;
            List<string> names = new List<string>();

            string[] lines = (sender as RichTextBox).Text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
                names.Add(lines[i]);
            settings["Names"] = names;

            saveSettings();
        }

        private void lookCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("LookMode", (sender as CheckBox).Checked.ToString());
            saveSettings();
        }

        private void alwaysShowLoot_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("AlwaysShowLoot", (sender as CheckBox).Checked.ToString());
            saveSettings();
        }

        private void enableScreenshotBox_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("EnableScreenshots", (sender as CheckBox).Checked.ToString());
            saveSettings();

            this.screenshotPanel.Enabled = (sender as CheckBox).Checked;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);

        void takeScreenshot(string name) {
            string path = getSettingString("ScreenshotPath");
            if (path == null) return;

            Process[] tibia_process = Process.GetProcessesByName("Tibia");
            if (tibia_process.Length == 0) return; //no tibia to take screenshot of

            RECT Rect = new RECT();
            if (!GetWindowRect(tibia_process[0].MainWindowHandle, ref Rect)) return;

            Bitmap bitmap = new Bitmap(Rect.right - Rect.left, Rect.bottom - Rect.top);
            using (Graphics gr = Graphics.FromImage(bitmap)) {
                gr.CopyFromScreen(new Point(Rect.left, Rect.top), Point.Empty, bitmap.Size);
            }
            DateTime dt = DateTime.Now;
            name = String.Format("{0} - {1}-{2}-{3} {4}h{5}m{6}s{7}ms.png", name, dt.Year.ToString("D4"), dt.Month.ToString("D2"), dt.Day.ToString("D2"), dt.Hour.ToString("D2"), dt.Minute.ToString("D2"), dt.Second.ToString("D2"), dt.Millisecond.ToString("D4"));
            path = Path.Combine(path, name);
            bitmap.Save(path, ImageFormat.Png);

            refreshScreenshots();
        }

        List<string> imageExtensions = new List<string> { ".jpg", ".bmp", ".gif", ".png" };
        void refreshScreenshots() {
            string selectedValue = screenshotList.SelectedIndex >= 0 ? screenshotList.Items[screenshotList.SelectedIndex].ToString() : null;
            int index = 0;

            string path = getSettingString("ScreenshotPath");
            if (path == null) return;
            string[] files = Directory.GetFiles(path);

            refreshingScreenshots = true;

            screenshotList.Items.Clear();
            foreach (string file in files) {
                if (imageExtensions.Contains(Path.GetExtension(file).ToLower())) { //check if file is an image
                    string f = Path.GetFileName(file);
                    if (f == selectedValue) {
                        index = screenshotList.Items.Count;
                    }
                    screenshotList.Items.Add(f);
                }
            }

            refreshingScreenshots = false;
            if (screenshotList.Items.Count > 0) {
                screenshotList.SelectedIndex = index;
            }
        }

        private void screenshotBrowse_Click(object sender, EventArgs e) {
            folderBrowserDialog1.SelectedPath = getSettingString("ScreenshotPath");
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) {
                setSetting("ScreenshotPath", folderBrowserDialog1.SelectedPath);
                screenshotDirectoryBox.Text = getSettingString("ScreenshotPath");
                refreshScreenshots();
            }
        }

        private void autoScreenshot_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("AutoScreenshotAdvance", (sender as CheckBox).Checked.ToString());
            saveSettings();
        }

        private void autoScreenshotDrop_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("AutoScreenshotItemDrop", (sender as CheckBox).Checked.ToString());
            saveSettings();
        }

        private void autoScreenshotDeath_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("AutoScreenshotDeath", (sender as CheckBox).Checked.ToString());
            saveSettings();
        }

        bool refreshingScreenshots = false;
        private void screenshotList_SelectedIndexChanged(object sender, EventArgs e) {
            if (refreshingScreenshots) return;
            if (screenshotList.SelectedIndex >= 0) {
                string selectedImage = screenshotList.Items[screenshotList.SelectedIndex].ToString();

                string path = getSettingString("ScreenshotPath");
                if (path == null) return;

                string imagePath = Path.Combine(path, selectedImage);

                Image image = Image.FromFile(imagePath);
                if (image != null) {
                    if (screenshotBox.Image != null) {
                        screenshotBox.Image.Dispose();
                    }
                    screenshotBox.Image = image;
                }
            }
        }

        private void openInExplorer_Click(object sender, EventArgs e) {
            string path = getSettingString("ScreenshotPath");
            if (path == null) return;
            Process.Start(path);
        }

        private void startAutohotkeyScript_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("StartAutohotkeyAutomatically", (sender as CheckBox).Checked.ToString());
            saveSettings();
        }
        private void shutdownOnExit_CheckedChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;

            setSetting("ShutdownAutohotkeyOnExit", (sender as CheckBox).Checked.ToString());
            saveSettings();
        }

        static string autoHotkeyURL = "http://ahkscript.org/download/ahk-install.exe";
        private void downloadAutoHotkey_Click(object sender, EventArgs e) {
            WebClient client = new WebClient();

            client.DownloadDataCompleted += Client_DownloadDataCompleted;
            client.DownloadProgressChanged += Client_DownloadProgressChanged;

            downloadBar.Visible = true;
            downloadLabel.Visible = true;
            downloadLabel.Text = "Downloading...";

            client.DownloadDataAsync(new Uri(autoHotkeyURL));
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            this.downloadBar.Value = e.ProgressPercentage;
            this.downloadBar.Maximum = 100;
        }

        private void Client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e) {
            downloadLabel.Text = "Writing...";
            try {
                string filepath = System.IO.Path.GetTempPath() + "autohotkeyinstaller.exe";
                Console.WriteLine(filepath);
                File.WriteAllBytes(filepath, e.Result);
                System.Diagnostics.Process.Start(filepath);
                downloadLabel.Text = "Download successful.";
            } catch {
                downloadLabel.Text = "Failed to download.";
            }
            downloadBar.Visible = false;
        }

        private string modifyKeyString(string value) {
            if (value.Contains("alt+")) {
                value = value.Replace("alt+", "!");
            }
            if (value.Contains("ctrl+")) {
                value = value.Replace("ctrl+", "^");
            }
            if (value.Contains("shift+")) {
                value = value.Replace("shift+", "+");
            }
            if (value.Contains("command=")) {
                string[] split = value.Split(new string[] { "command=" }, StringSplitOptions.None);
                value = split[0] + "SendMessage, 0xC, 0, \"" + split[1] + "\",,Tibialyzer"; //command is send through the WM_SETTEXT message
            }

            return value;
        }

        private void autoHotkeyGridSettings_TextChanged(object sender, EventArgs e) {
            if (prevent_settings_update) return;
            if (!settings.ContainsKey("AutoHotkeySettings")) settings.Add("AutoHotkeySettings", autoHotkeyGridSettings.Text.Split('\n').ToList());
            else settings["AutoHotkeySettings"] = autoHotkeyGridSettings.Text.Split('\n').ToList();
            saveSettings();
            this.autohotkeyWarningLabel.Visible = true;
        }

        private void writeToAutoHotkeyFile() {
            if (!settings.ContainsKey("AutoHotkeySettings")) return;
            using (StreamWriter writer = new StreamWriter(autohotkeyFile)) {
                writer.WriteLine("#SingleInstance force");
                writer.WriteLine("#IfWinActive ahk_class TibiaClient");
                foreach (string l in settings["AutoHotkeySettings"]) {
                    string line = l.ToLower();
                    if (line.Length == 0 || line[0] == '#') continue;
                    if (line.Contains("suspend")) {
                        // if the key is set to suspend the hotkey layout, we set it up so it sends a message to us 
                        writer.WriteLine(modifyKeyString(line.ToLower().Split(new string[] { "suspend" }, StringSplitOptions.None)[0]));
                        writer.WriteLine("suspend");
                        writer.WriteLine("if (A_IsSuspended)");
                        // message 32 is suspend
                        writer.WriteLine("PostMessage, 0x317,32,32,,Tibialyzer");
                        writer.WriteLine("else");
                        // message 33 is not suspended
                        writer.WriteLine("PostMessage, 0x317,33,33,,Tibialyzer");
                        writer.WriteLine("return");
                    } else {
                        writer.WriteLine(modifyKeyString(line));
                    }
                }
            }
        }

        private void startAutoHotkey_Click(object sender, EventArgs e) {
            this.autohotkeyWarningLabel.Visible = false;
            writeToAutoHotkeyFile();
            System.Diagnostics.Process.Start(autohotkeyFile);
        }

        private void shutdownAutoHotkey_Click(object sender, EventArgs e) {
            foreach (var process in Process.GetProcessesByName("AutoHotkey")) {
                process.Kill();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (getSettingBool("ShutdownAutohotkeyOnExit")) {
                shutdownAutoHotkey_Click(null, null);
            }
        }

        AutoHotkeySuspendedMode window = null;
        protected override void WndProc(ref Message m) {
            if (m.Msg == 0xC) {
                // This messages is send by AutoHotkey to execute a command
                string command = Marshal.PtrToStringUni(m.LParam);
                if (command != null) {
                    if (this.ExecuteCommand(command)) {
                        return; //if the passed along string is a command, we have executed it successfully
                    }
                }
            }
            if (m.Msg == 0x317) {
                // We intercept this message because this message signifies the AutoHotkey state (suspended or not)

                int wParam = m.WParam.ToInt32();
                if (wParam == 32) {
                    // 32 signifies we have entered suspended mode, so we warn the user with a popup
                    if (window == null) {
                        Screen screen;
                        Process[] tibia_process = Process.GetProcessesByName("Tibia");
                        if (tibia_process.Length == 0) {
                            screen = Screen.FromControl(this);
                        } else {
                            Process tibia = tibia_process[0];
                            screen = Screen.FromHandle(tibia.MainWindowHandle);
                        }
                        window = new AutoHotkeySuspendedMode();
                        window.StartPosition = FormStartPosition.Manual;
                        window.SetDesktopLocation(screen.WorkingArea.Right - window.Width - 10, screen.WorkingArea.Top + 10);
                        window.TopMost = true;
                        window.Show();
                    }
                } else if (wParam == 33) {
                    // 33 signifies we are not suspended, destroy the suspended window (if it exists)
                    if (window != null && !window.IsDisposed) {
                        try {
                            window.Close();
                        } catch {

                        }
                        window = null;
                    }
                }
            }
            base.WndProc(ref m);
        }
    }
}
