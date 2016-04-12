using System;
using System.Linq;
using System.IO;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace DoubleOrNothing
{
    [ApiVersion(1, 22)]
    public class DoubleOrNothing : TerrariaPlugin
    {
        Timer Timer;
        public Config Config = new Config();
        Timer t = new Timer(15000);
        string currentUser = "None";
        int donval = 1;
        bool inProgress = false;

        public override string Name { get { return "Double or Nothing"; }}
        public override Version Version { get { return new Version(1, 0); }}
        public override string Author { get { return "Chimarex"; }}
        public override string Description { get { return "A fun plugin which allows you to gamble with configurable payment and rewards!"; }}

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
 /* ServerApi.Hooks.ServerLeave.Register(this, OnLeave); */
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
/* ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave); */
            }
            base.Dispose(disposing);
        }

        public DoubleOrNothing(Main game) : base(game)
        {
            Order = 2;
        }

        void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("gamble.base", DoNMain, "gamble")
            {
                HelpDesc = new[]
                {
                           "/gamble start - Begins a game of Double or Nothing.",
                           "/gamble help - Displays Commands And Starting Fee.",
                           "/gamble info - Displays information about a game of Double or Nothing in progress.",
                           "/gamble rewards - Displays rewards as set by the server administrator."
                }

            });

            if (File.Exists(Path.Combine(TShock.SavePath, "donconfig.json")))
            {
                Config = Config.Read(Path.Combine(TShock.SavePath, "donconfig.json"));
            }
            Config.Write(Path.Combine(TShock.SavePath, "donconfig.json"));
        }

      /*  void OnLeave(LeaveEventArgs args)
        {
            Timer cd = new Timer(Config.cooldown * 1000);
            string user = TShock.Players[args.Who].User.Name;
            if (user == currentUser)
            {
                TSPlayer.All.SendWarningMessage("{0} has forfeited {1} points in Double or Nothing by not collecting in time!", currentUser, donval);
                currentUser = "None";
                donval = 1;
                t.Stop();
                cd.Elapsed += new ElapsedEventHandler(OnCooldownFinish); ;
                cd.AutoReset = false;
                cd.Start();
            }
            
        }*/

        void DoNMain(CommandArgs args)
        {
            Item iReq = TShock.Utils.GetItemById(Config.itemReq);
            string plur = null;
            if (Config.stackReq > 1) { plur = "s"; }
            TSPlayer user = args.Player;
            if (!Main.ServerSideCharacter)
            {
                user.SendWarningMessage("This plugin is not intended to be used without Server Side Characters!");
            }
            if (args.Parameters.Count < 1)
            {
                user.SendInfoMessage("Type '/gamble start' to begin playing or '/gamble info' to see current game progress!");
            }
            else
            {
                switch (args.Parameters[0].ToLower())
                {
                    case "start":
                        {
                            if (currentUser == "None" && inProgress == true)
                            {
                                user.SendErrorMessage("Double or Nothing is on Cooldown!");
                                return;
                            }
                            var iCheck = user.TPlayer.inventory.FirstOrDefault(i => i.netID == Config.itemReq);
                            if (iCheck == null || iCheck.stack < Config.stackReq)
                            {
                                user.SendErrorMessage("You must pay {0} {1}{2} to play Double or Nothing!", Config.stackReq, iReq.name, plur);
                                return;
                            }
                            if (inProgress == false)
                            {
                                Item item;
                                for (int i = 0; i < 50; i++)
                                {
                                    item = user.TPlayer.inventory[i];
                                    if (item.type == iReq.type && item.stack >= Config.stackReq)
                                    {
                                        if (user.InventorySlotAvailable || item.stack == Config.stackReq)
                                        {
                                            user.TPlayer.inventory[i].stack -= Config.stackReq;
                                            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, String.Empty, user.Index, i);
                                            inProgress = true;
                                            currentUser = user.Name;
                                            t.Elapsed += new ElapsedEventHandler(OnTimedEvent); ;
                                            t.Start();
                                            TSPlayer.All.SendSuccessMessage("{0} has started a game of Double or Nothing!", currentUser, donval);
                                            user.SendInfoMessage("You have paid {0} {1}{2}!", Config.stackReq, iReq.name, plur);
                                            user.SendInfoMessage("You currently have {0} points", donval);
                                            user.SendMessage("Continue: '/gamble continue', Claim: '/gamble claim'", Color.DarkGoldenrod);
                                            user.SendMessage("You must make a decision within 15 seconds.", Color.DarkGoldenrod);
                                            return;
                                        }
                                        user.SendErrorMessage("To play Double or Nothing you must have a free inventory slot!");
                                        return;
                                    }
                                }
                                return;
                            }
                            user.SendErrorMessage("A game of Double or Nothing is already in progress!");
                            break;
                        }

                    case "continue":
                        {

                            if (inProgress == true)
                            {
                                if (currentUser == user.Name)
                                {
                                    Item item;
                                    for (int i = 0; i < 50; i++)
                                    {
                                        item = user.TPlayer.inventory[i];
                                        if (user.InventorySlotAvailable)
                                        {
                                            Random rnd = new Random();
                                            int rng = rnd.Next(1, 10);
                                            if (rng >= 7)
                                            {
                                                Timer cd = new Timer(Config.cooldown * 1000);
                                                TSPlayer.All.SendWarningMessage("{0} has lost {1} points in Double or Nothing!", user.Name, donval);
                                                donval = 1;
                                                t.Stop();
                                                currentUser = "None";
                                                cd.Elapsed += new ElapsedEventHandler(OnCooldownFinish); ;
                                                cd.AutoReset = false;
                                                cd.Start();
                                                return;
                                            }

                                            else
                                            {
                                                donval *= 2;
                                                t.Stop();
                                                t.Start();
                                                user.SendSuccessMessage("You have advanced to the next round! Current Points: {0}", donval);
                                                user.SendMessage("Continue: /gamble continue, Claim: /gamble claim", Color.DarkGoldenrod);
                                                user.SendMessage("You must make a decision within 15 seconds.", Color.DarkGoldenrod);
                                                return;
                                            }
                                        }
                                    }
                                    user.SendErrorMessage("To play Double or Nothing you must have a free inventory slot!");
                                    return;
                                }
                                else if (currentUser == "None")
                                {
                                    user.SendErrorMessage("Double or Nothing is on Cooldown!");
                                    return;
                                }
                                user.SendErrorMessage("Somebody else is currently playing Double or Nothing!");
                                return;
                            }
                            user.SendErrorMessage("A game of Double or Nothing is not in progress!");
                            break;
                        }
                    case "claim":
                        {
                            if (inProgress == true)
                            {
                                if (currentUser == user.Name)
                                {
                                    Timer cd = new Timer(Config.cooldown * 1000);
                                    int reward = 0;
                                    int rewardStack = 0;
                                    string plur2 = null;

                                    if (donval == 1)
                                    {
                                        reward = Config.reward1;
                                        rewardStack = Config.stack1;
                                    }
                                    if (donval == 2)
                                    {
                                        reward = Config.reward2;
                                        rewardStack = Config.stack2;
                                    }
                                    if (donval == 4)
                                    {
                                        reward = Config.reward3;
                                        rewardStack = Config.stack3;
                                    }
                                    if (donval == 8)
                                    {
                                        reward = Config.reward4;
                                        rewardStack = Config.stack4;
                                    }
                                    if (donval == 16)
                                    {
                                        reward = Config.reward5;
                                        rewardStack = Config.stack5;
                                    }
                                    if (donval == 32)
                                    {
                                        reward = Config.reward6;
                                        rewardStack = Config.stack6;
                                    }
                                    if (donval == 64)
                                    {
                                        reward = Config.reward7;
                                        rewardStack = Config.stack7;
                                    }
                                    if (donval == 128)
                                    {
                                        reward = Config.reward8;
                                        rewardStack = Config.stack8;
                                    }
                                    if (donval >= 256)
                                    {
                                        reward = Config.reward9;
                                        rewardStack = Config.stack9;
                                    }
                                    if (rewardStack > 1) { plur2 = "s"; }
                                    Item itemById = TShock.Utils.GetItemById(reward);
                                    user.GiveItem(itemById.type, itemById.name, itemById.width, itemById.height, rewardStack, 0);
                                    TSPlayer.All.SendSuccessMessage("{0} has scored {1} points in Double or Nothing and has recieved {2} {3}{4}!", user.Name, donval, rewardStack, itemById.name, plur2);
                                    if (donval >= 1024 && Config.hpBoostAt1024 == true)
                                    {
                                        TSPlayer.All.SendSuccessMessage("Due to scoring an outstanding amount points {0} has also recieved an increase in hp!", user.Name);
                                        user.TPlayer.statLifeMax += 25;
                                        user.TPlayer.statManaMax += 5;
                                        NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, String.Empty, user.Index);
                                        NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, String.Empty, user.Index);
                                    }
                                    donval = 1;
                                    currentUser = "None";
                                    t.Stop();
                                    cd.Elapsed += new ElapsedEventHandler(OnCooldownFinish); ;
                                    cd.AutoReset = false;
                                    cd.Start();
                                    return;
                                }
                                user.SendErrorMessage("Somebody else is currently playing Double or Nothing!");
                                return;
                            }
                            user.SendErrorMessage("A game of Double or Nothing is not in progress!");
                            break;
                        }

                    case "info":
                        {
                            if (currentUser != "None" && inProgress == true)
                            {
                                user.SendInfoMessage("Playing: {0}, Current Points: {1}", currentUser, donval);
                                return;
                            }
                            if (currentUser == "None" && inProgress == true)
                            {
                                user.SendInfoMessage("Double or Nothing is on Cooldown!");
                                return;
                            }
                            else
                            {
                                user.SendInfoMessage("A game of Double or Nothing is not in progress!");
                            }
                            break;
                        }

                    case "rewards":
                        {
                            Item i1 = TShock.Utils.GetItemById(Config.reward1);
                            Item i2 = TShock.Utils.GetItemById(Config.reward2);
                            Item i3 = TShock.Utils.GetItemById(Config.reward3);
                            Item i4 = TShock.Utils.GetItemById(Config.reward4);
                            Item i5 = TShock.Utils.GetItemById(Config.reward5);
                            Item i6 = TShock.Utils.GetItemById(Config.reward6);
                            Item i7 = TShock.Utils.GetItemById(Config.reward7);
                            Item i8 = TShock.Utils.GetItemById(Config.reward8);
                            Item i9 = TShock.Utils.GetItemById(Config.reward9);

                            user.SendInfoMessage("Starting Fee: {0} {1}{2}", Config.stackReq, iReq.name, plur);
                            user.SendInfoMessage("1 point = {1} {0}, 2 points = {3} {2}, 4 points = {5} {4},", i1.name, Config.stack1, i2.name, Config.stack2, i3.name, Config.stack3);
                            user.SendInfoMessage("8 points = {1} {0}, 16 points = {3} {2}, 32 points = {5} {4},", i4.name, Config.stack4, i5.name, Config.stack5, i6.name, Config.stack6);
                            user.SendInfoMessage("64 points = {1} {0}, 128 points = {3} {2}, 256 points = {5} {4}.", i7.name, Config.stack7, i8.name, Config.stack8, i9.name, Config.stack9);
                            if(Config.hpBoostAt1024 == true) { user.SendInfoMessage("1024 points = +25 hp, +5 mana."); }
                            break;
                        }

                    case "help":
                        {
                            user.SendInfoMessage("/gamble start - Begins a game of Double or Nothing.");
                            user.SendInfoMessage("/gamble info - Displays information about a game of Double or Nothing in progress.");
                            user.SendInfoMessage("/gamble rewards - Displays rewards as set by the server administrator.");
                            if (user.Group.HasPermission("gamble.admin"))
                            {
                                user.SendInfoMessage("/gamble reload - Reloads Plugin and Config File");
                            }
                            user.SendInfoMessage("Starting Fee: {0} {1}{2}", Config.stackReq, iReq.name, plur);
                            break;
                        }

                    case "reload":
                        {
                            if (!user.Group.HasPermission("gamble.admin"))
                            {
                                user.SendErrorMessage("You do not have access to this command.");
                                break;
                            }
                            Config = Config.Read(Path.Combine(TShock.SavePath, "donconfig.json"));
                            user.SendSuccessMessage("Reload Successful!");
                            break;
                        }

                    default:
                        {
                            user.SendInfoMessage("Type '/gamble start' to begin playing or '/gamble info' to see current game progress!");
                            break;
                        }

                }
            }
        }

        public void OnTimedEvent(object source, ElapsedEventArgs args)
        {
            Timer cd = new Timer(Config.cooldown * 1000);
            if (currentUser != "None")
            {
                TSPlayer.All.SendWarningMessage("{0} has forfeited {1} points in Double or Nothing by not collecting in time!", currentUser, donval);
            }
            currentUser = "None";
            donval = 1;
            t.Stop();
            cd.Elapsed += new ElapsedEventHandler(OnCooldownFinish); ;
            cd.AutoReset = false;
            cd.Start();
        }

        public void OnCooldownFinish(object source, ElapsedEventArgs args)
        {
            if (currentUser == "None")
            {
                TSPlayer.All.SendInfoMessage("Double or Nothing is now off cooldown!");
                inProgress = false;
                Timer.Stop();
            }
        }
    }
}


    
