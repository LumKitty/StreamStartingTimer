using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Text;
using System.IO.MemoryMappedFiles;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;


namespace StreamStartingTimer
{
    class Options {
        [Option('s', "seconds", Required = false, HelpText = "Run clock for this number of seconds")]                          public int?    Seconds { get; set; }
        [Option('m', "minutes", Required = false, HelpText = "Run clock for this number of minutes")]                          public int?    Minutes { get; set; }
        [Option('p', "past",    Required = false, HelpText = "Run clock until time is this many minutes past the hour")]       public int?    Past { get; set; }
        [Option('e', "events",  Required = false, HelpText = "Load the startup events from this file instead of default")]     public string? Events { get; set; }
        [Option('c', "config",  Required = false, HelpText = "Load the clock configuation from this file instead of default")] public string? Config { get; set; }
        [Option('a', "alter",   Required = false, HelpText = "Alter the timer of an already running copy of this program")]    public bool    Alter { get; set; }
    }

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main(string[] args) {

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            uint StartTime = 0;
            bool AdjustingTime = false;
            string ConfigFile = Application.StartupPath + "DefaultConfig.json";
            string EventsFile = Application.StartupPath + "DefaultEvents.json";
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o => {
                    if (o.Alter ) {
                        Console.WriteLine("edit");
                        StartTime = Shared.SecondsToGo;
                        Console.WriteLine("Clock is currently at " + StartTime.ToString() + " seconds");
                        AdjustingTime = true;
                    } else {
                        Console.WriteLine("Non-edit");
                        StartTime = 0;
                    }
                    if (o.Events != null) {
                        EventsFile = o.Events;
                    }
                    if (o.Config != null) {
                        ConfigFile = o.Config;
                    }
                    if (o.Past != null) {
                        DateTime dateTime = DateTime.Now;
                        DateTime trgTime;
                        if (dateTime.Minute < o.Past) {
                            trgTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, (int)o.Past, 0, 0);
                        } else {
                            trgTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour + 1, (int)o.Past, 0, 0);
                        }
                        StartTime = (uint)(trgTime - DateTime.Now).TotalSeconds;
                    }
                    if (o.Seconds != null) {
                        StartTime += (uint)o.Seconds;
                    }
                    if (o.Minutes != null) {
                        StartTime += (uint)o.Minutes * 60;
                    }
                }
            );
            if (AdjustingTime) {
                Shared.SecondsToGo = StartTime;
            } else {
                while (!Shared.IsSingleInstance()) {
                    if (MessageBox.Show("Another copy of this program is already running", "Mutex Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Cancel) {
                        return;
                    }
                }
                Shared.CurSettings = new CSettings();
                if (!File.Exists(ConfigFile)) {
                    if (MessageBox.Show("This appears to be the first time you have run this program. Would you like to view the instructions", "Welcome", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        Process myProcess = new Process();
                        myProcess.StartInfo.UseShellExecute = true;
                        myProcess.StartInfo.FileName = "https://github.com/LumKitty/StreamStartingTimer/blob/master/README.md";
                        myProcess.Start();
                        myProcess.Dispose();
                    }
                } else {
                    Shared.CurSettings.LoadConfig(ConfigFile);
                }
                if (File.Exists(EventsFile)) {
                    Shared.TimerEvents = Shared.LoadEvents(EventsFile);
                }
                Shared.frmClock = new Clock(StartTime, EventsFile);
                Application.Run(Shared.frmClock);
            }
        }
    }

}