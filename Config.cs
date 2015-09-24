using System;
using System.IO;
using Newtonsoft.Json;

namespace DoubleOrNothing
{
    public class Config
    {
        public int itemReq = 2767; //Pay to Start
        public int stackReq = 5;

        public int reward1 = 1922; //Reward for 1 point
        public int stack1 = 1;

        public int reward2 = 3093; //Reward for 2 points
        public int stack2 = 2;

        public int reward3 = 2767; //Reward for 4 points
        public int stack3 = 10;

        public int reward4 = 2335; //Reward for 8 points
        public int stack4 = 5;

        public int reward5 = 3084; //Reward for 16 points
        public int stack5 = 1;

        public int reward6 = 2336; //Reward for 32 points
        public int stack6 = 10;

        public int reward7 = 2348; //Reward for 64 points
        public int stack7 = 20;

        public int reward8 = 438; //Reward for 128 points
        public int stack8 = 3;

        public int reward9 = 473; //Reward for 256+ points
        public int stack9 = 3;

        public bool hpBoostAt1024 = false; // Allows the player to recieve 25 extra max hp at 1024+ points
        public int cooldown = 60; // Cooldown in Seconds

        public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Write(fs);
            }
        }
        public void Write(Stream stream)
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(str);
            }
        }

        public static Config Read(string path)
        {
            if (!File.Exists(path))
            {
                return new Config();
            }
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs);
            }
        }
        public static Config Read(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            }
        }
    }
}
