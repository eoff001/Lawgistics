using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Data;
using System.Timers;
using System.Threading;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics;

namespace LG_Timer
{
    public partial class MainForm : Form
    {
        private static string _bak_loc = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\LGTimer";
        private static int numTimers = 0; //count of number of timers in form
        private static int _timerBac = 0; //******new****** counter used for custom backup time
        private static int screenHeight = SystemInformation.VirtualScreen.Height - 40;
        private static int maxTimers = screenHeight / 88; //maximum number of timers allowed in form
        private static TimerList listTimers;
        private static System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        private ToolStripMenuItem restoreToolStripMenuItem;
        private ToolStripMenuItem restoreToolStripMenuItem1;
        private ToolStripMenuItem saveToolStripMenuItem;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem restoreToolStripMenuItem2;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem alwaysOnTopToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem addTimerToolStripMenuItem;
        private static System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
        //private static System.Windows.Forms.Timer _timerBac = new System.Windows.Forms.Timer(); //possible backup timer for alternate backup time interval

        public class TimerObj //single timer
        {
            public Label timeDisplay; //main timer display
            public Label auxDisplay; //auxilary timer display
            public Button playButton; //play/pause button
            public Button restartButton; //reset button
            public Button removeButton; //remove timer button
            public TextBox text; //notes text box
            private SettableStopwatch stopWatch; //internal stopwatch form timer displays
            private Form F;

            public TimerObj(Form MainForm) //contructor
            {
                timeDisplay = new System.Windows.Forms.Label(); //initialize timer components
                auxDisplay = new System.Windows.Forms.Label();
                playButton = new System.Windows.Forms.Button();
                restartButton = new System.Windows.Forms.Button();
                removeButton = new System.Windows.Forms.Button();
                text = new System.Windows.Forms.TextBox();
                stopWatch = new SettableStopwatch(TimeSpan.Zero);
                F = MainForm;
                // 
                // timeDisplay
                // 
                this.timeDisplay.AutoSize = true;
                this.timeDisplay.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F);
                this.timeDisplay.Location = new System.Drawing.Point(161, 55 + numTimers * 88);
                this.timeDisplay.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
                this.timeDisplay.Name = "timeDisplay";
                this.timeDisplay.Size = new System.Drawing.Size(81, 31);
                this.timeDisplay.TabIndex = 9;
                this.timeDisplay.Text = "0.00";
                this.timeDisplay.ForeColor = Color.DeepSkyBlue;
                this.timeDisplay.BackColor = Color.Transparent;
                // 
                // auxDisplay
                // 
                this.auxDisplay.AutoSize = true;
                this.auxDisplay.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.auxDisplay.Location = new System.Drawing.Point(166, 86 + numTimers * 88);
                this.auxDisplay.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
                this.auxDisplay.Name = "auxDisplay";
                this.auxDisplay.Size = new System.Drawing.Size(73, 16);
                this.auxDisplay.TabIndex = 29;
                this.auxDisplay.Text = "00:00:00";
                this.auxDisplay.BackColor = Color.Transparent;
                this.auxDisplay.ForeColor = Color.DeepSkyBlue;
                // 
                // playButton
                // 
                this.playButton.BackColor = Color.Transparent;
                this.playButton.BackgroundImage = Properties.Resources.Play_64;
                this.playButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                this.playButton.Location = new System.Drawing.Point(12, 60 + numTimers * 88);
                this.playButton.Name = "playButton";
                this.playButton.Size = new System.Drawing.Size(40, 40);
                this.playButton.TabIndex = 10;
                this.playButton.UseVisualStyleBackColor = false;
                this.playButton.Click += new System.EventHandler(this.playButton_Click);
                // 
                // restartButton
                // 
                this.restartButton.BackColor = Color.Transparent;
                this.restartButton.BackgroundImage = Properties.Resources.Repeat_64;
                this.restartButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                this.restartButton.Location = new System.Drawing.Point(55, 60 + numTimers * 88);
                this.restartButton.Name = "restartButton";
                this.restartButton.Size = new System.Drawing.Size(40, 40);
                this.restartButton.TabIndex = 11;
                this.restartButton.UseVisualStyleBackColor = false;
                this.restartButton.Click += new System.EventHandler(this.restartButton_Click);
                // 
                // removeButton
                // 
                this.removeButton.BackColor = System.Drawing.Color.Transparent;
                this.removeButton.BackgroundImage = global::LG_Timer.Properties.Resources.Remove;
                this.removeButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
                this.removeButton.Location = new System.Drawing.Point(104, 60 + numTimers * 88);
                this.removeButton.Name = "removeButton";
                this.removeButton.Size = new System.Drawing.Size(40, 40);
                this.removeButton.TabIndex = 46;
                this.removeButton.UseVisualStyleBackColor = true;
                this.removeButton.Click += this.removeButton_Click;
                // 
                // text
                // 
                this.text.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.text.Location = new System.Drawing.Point(236, 38 + numTimers * 88);
                this.text.Multiline = true;
                this.text.Name = "text";
                this.text.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
                this.text.Size = new System.Drawing.Size(262, 72);
                this.text.TabIndex = 24;
                this.text.BackColor = Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
                this.text.ForeColor = Color.Azure;

                playButton.TabStop = false; //set button styles
                playButton.FlatStyle = FlatStyle.Flat;
                playButton.FlatAppearance.BorderSize = 0;
                restartButton.TabStop = false;
                restartButton.FlatStyle = FlatStyle.Flat;
                restartButton.FlatAppearance.BorderSize = 0;
                removeButton.TabStop = false;
                removeButton.FlatStyle = FlatStyle.Flat;
                removeButton.FlatAppearance.BorderSize = 0;

                MainForm.Controls.Add(this.timeDisplay); //add new form controls
                MainForm.Controls.Add(this.auxDisplay);
                MainForm.Controls.Add(this.text);
                MainForm.Controls.Add(this.playButton);
                MainForm.Controls.Add(this.restartButton);
                MainForm.Controls.Add(this.removeButton);
            }

            ~TimerObj() //destructor
            { }

            public void playButton_Click(object sender, EventArgs e) //handles play/pause event
            {
                if (!stopWatch.IsRunning) //timer is not running, start it
                {
                    stopWatch.Start();
                    timeDisplay.ForeColor = Color.MediumSpringGreen;
                    playButton.BackgroundImage = Properties.Resources.Pause_64;
                }
                else //timer is running, pause it
                {
                    stopWatch.Stop();
                    timeDisplay.ForeColor = Color.DeepSkyBlue;
                    playButton.BackgroundImage = Properties.Resources.Play_64;
                }
            }
            public void restartButton_Click(object sender, EventArgs e) //handles reset event
            { wipe(); } //clear timer

            public void wipe() //handles clearing of timer fields
            {
                stopWatch.Stop();
                stopWatch.Reset();
                timeDisplay.Text = "0.00";
                auxDisplay.Text = "00:00:00";
                text.Text = "";
                timeDisplay.ForeColor = Color.DeepSkyBlue;
                playButton.BackgroundImage = Properties.Resources.Play_64;
            }

            public void removeButton_Click(object sender, EventArgs e) //handles remove timer event
            {
                if (numTimers > 1) //only remove if there is more than one timer
                {
                    F.Controls.Remove(this.timeDisplay); //remove controls
                    F.Controls.Remove(this.auxDisplay);
                    F.Controls.Remove(this.text);
                    F.Controls.Remove(this.playButton);
                    F.Controls.Remove(this.restartButton);
                    F.Controls.Remove(this.removeButton);

                    F.Height -= 88; //resize form

                    listTimers.removeTimer(this); //remove timer from timer list
                }

                else //last timer, just clear fields
                {
                    wipe();
                    MessageBox.Show("Cannot Remove Last Timer; Timer has Been Cleared", "Last Timer",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            public bool _running()
            {
                if (stopWatch.IsRunning)
                {
                    return true;
                }
                return false;
            }

            public static double CeilingTo(double value, double interval)
            {
                var remainder = value % interval;
                return remainder > 0 ? value + (interval - remainder) : value;
            }

            private double ToBillable(SettableStopwatch stopwatch)
            {
                double billable = 0.0;
                if (stopwatch.ElapsedTimeSpan.TotalHours == 0.0f)
                {
                    billable = 0.0f;
                }
                else if (stopwatch.ElapsedTimeSpan.TotalHours < .05f && stopwatch.ElapsedTimeSpan.TotalHours > 0.0f)
                {
                    billable = 0.1f;
                }
                else
                {
                    billable = CeilingTo(stopwatch.ElapsedTimeSpan.TotalHours, .05);
                }
                return billable;
            }

            public void UpdateBackup(List<RecordEntry> records) //updates the timer and its backup
            {
                timeDisplay.Text = ToBillable(stopWatch).ToString("F");
                auxDisplay.Text = stopWatch.ElapsedTimeSpan.ToString(@"hh\:mm\:ss");
                RecordEntry rec = new RecordEntry() { billableTime = timeDisplay.Text, actualTime = stopWatch.ElapsedTimeSpan, notes = text.Text };
                records.Add(rec);
            }

            public void UpdateTimer() //*****new***** updates the timers w/o perfomring backup
            {
                timeDisplay.Text = ToBillable(stopWatch).ToString("F");
                auxDisplay.Text = stopWatch.ElapsedTimeSpan.ToString(@"hh\:mm\:ss");
            }

            public void SetStopwatch(TimeSpan restore)
            {
                stopWatch.ElapsedTimeSpan = restore;
            }

            public string GetStopwatch()
            {
                return stopWatch.ElapsedTimeSpan.ToString(@"hh\:mm\:ss");
            }
        }

        public class TimerList //list of timer objects
        {
            public List<TimerObj> Timers; //actual list where timers are stored

            public TimerList(Form MainForm) //constructor
            {
                Timers = new List<TimerObj>();
                for(int i = 0; i < 5; i++) //start with 5 timers
                    addNewTimer(MainForm);
            }

            ~TimerList() //destructor
            { }

            public void addNewTimer(Form MainForm) //add a new timer to the list
            {
                if (numTimers >= maxTimers) //check if maximum number of timers has been reached
                    throw new MaximumTimers("Cannot Add Anymore Timers, Maximum Number Reached");

                else //space available
                {
                    MainForm.Height += 88; //adjust size of form
                    TimerObj newTimer = new TimerObj(MainForm); //create new timer
                    Timers.Add(newTimer); //add new timer to list and increment timer count
                    numTimers++; //increment number of timers count
                }
            }

            public void removeTimer(TimerObj timer) //removes a timer from the list
            {
                Timers.Remove(timer); //remove timer from list
                numTimers--; //decrement number of timers count

                for (int i = 0; i < Timers.Count; i++) //adjust remaining timer locations in form
                {
                    Timers[i].timeDisplay.Location = new System.Drawing.Point(161, 55 + i * 88);
                    Timers[i].auxDisplay.Location = new System.Drawing.Point(166, 86 + i * 88);
                    Timers[i].playButton.Location = new System.Drawing.Point(12, 60 + i * 88);
                    Timers[i].restartButton.Location = new System.Drawing.Point(58, 60 + i * 88);
                    Timers[i].removeButton.Location = new System.Drawing.Point(104, 60 + i * 88);
                    Timers[i].text.Location = new System.Drawing.Point(236, 38 + i * 88);
                }
            }
        }

        public MainForm()
        {
            InitializeComponent(); //initialize main form
            listTimers = new TimerList(this);
            ManualInit();
            Directory.CreateDirectory(_bak_loc);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.restoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addTimerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // restoreToolStripMenuItem
            // 
            this.restoreToolStripMenuItem.Name = "restoreToolStripMenuItem";
            this.restoreToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // restoreToolStripMenuItem1
            // 
            this.restoreToolStripMenuItem1.Name = "restoreToolStripMenuItem1";
            this.restoreToolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.menuStrip1.Font = new System.Drawing.Font("Constantia", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(455, 31);
            this.menuStrip1.TabIndex = 44;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTimerToolStripMenuItem,
            this.restoreToolStripMenuItem2});
            this.fileToolStripMenuItem.ForeColor = System.Drawing.Color.MediumTurquoise;
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(51, 27);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // addTimerToolStripMenuItem
            // 
            this.addTimerToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.addTimerToolStripMenuItem.ForeColor = System.Drawing.Color.MediumTurquoise;
            this.addTimerToolStripMenuItem.Name = "addTimerToolStripMenuItem";
            this.addTimerToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.addTimerToolStripMenuItem.Size = new System.Drawing.Size(267, 28);
            this.addTimerToolStripMenuItem.Text = "Add Timer";
            this.addTimerToolStripMenuItem.Click += new System.EventHandler(this.addTimerToolStripMenuItem_Click);
            // 
            // restoreToolStripMenuItem2
            // 
            this.restoreToolStripMenuItem2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.restoreToolStripMenuItem2.ForeColor = System.Drawing.Color.MediumTurquoise;
            this.restoreToolStripMenuItem2.Name = "restoreToolStripMenuItem2";
            this.restoreToolStripMenuItem2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.restoreToolStripMenuItem2.Size = new System.Drawing.Size(267, 28);
            this.restoreToolStripMenuItem2.Text = "Restore Timers";
            this.restoreToolStripMenuItem2.Click += new System.EventHandler(this.restoreToolStripMenuItem2_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.alwaysOnTopToolStripMenuItem});
            this.settingsToolStripMenuItem.ForeColor = System.Drawing.Color.MediumTurquoise;
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(89, 27);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // alwaysOnTopToolStripMenuItem
            // 
            this.alwaysOnTopToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.alwaysOnTopToolStripMenuItem.ForeColor = System.Drawing.Color.MediumTurquoise;
            this.alwaysOnTopToolStripMenuItem.Name = "alwaysOnTopToolStripMenuItem";
            this.alwaysOnTopToolStripMenuItem.Size = new System.Drawing.Size(205, 28);
            this.alwaysOnTopToolStripMenuItem.Text = "Always On Top";
            this.alwaysOnTopToolStripMenuItem.Click += new System.EventHandler(this.alwaysOnTopToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.ForeColor = System.Drawing.Color.MediumTurquoise;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(62, 27);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.aboutToolStripMenuItem.ForeColor = System.Drawing.Color.MediumTurquoise;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(132, 28);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(18F, 43F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.ClientSize = new System.Drawing.Size(505, 41);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Arial Narrow", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(9, 10, 9, 10);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "LG Timer";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ManualInit()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            _timer.Interval = 1000;
            _timer.Tick += new EventHandler(_update);
            _timer.Start();
        }

        private void toggleTopmost()
        {
            if (TopMost)
            {
                TopMost = false;
                alwaysOnTopToolStripMenuItem.Checked = false;
            }
            else
            {
                TopMost = true;
                alwaysOnTopToolStripMenuItem.Checked = true;
            }
        }

        public void _update(object sender, EventArgs e) //updates timer display and backs up timer data
        {
            _timerBac++; //increment backup time

            if (_timerBac % 10 == 0) //*****new if statement***** performs existing block below to back up every 10 seconds
            {
                List<RecordEntry> records = new List<RecordEntry>();

                foreach (TimerObj timer in listTimers.Timers)
                {
                    if (timer._running() || timer.timeDisplay.Text != "0.00" || timer.text.Text != "")
                        timer.UpdateBackup(records);
                }

                if (records.Count > 0)
                    BinarySerialization.WriteToBinaryFile(_bak_loc + "\\Billable_Time_Backup.bak", records);
            }

            else //*****new else for above if statement***** don't back up unless 10 sec interval has been reached
            {
                foreach (TimerObj timer in listTimers.Timers) //*****new****** modified for previous update loop above
                {
                    if (timer._running() || timer.timeDisplay.Text != "0.00" || timer.text.Text != "")
                        timer.UpdateTimer();
                }
            }
        }

        private void restoreToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            List<RecordEntry> restore = new List<RecordEntry>(); //restoration list

            if (File.Exists(_bak_loc + "\\Billable_Time_Backup.bak")) //automatic backup file exists
                restore = BinarySerialization.ReadFromBinaryFile<List<RecordEntry>>(_bak_loc + "\\Billable_Time_Backup.bak");

            if(restore != null) //check for at least one record
            {
                listTimers.Timers[0].wipe();
                listTimers.Timers[0].SetStopwatch(restore[0].actualTime);
                listTimers.Timers[0].auxDisplay.Text = listTimers.Timers[0].GetStopwatch();
                listTimers.Timers[0].text.Text = restore[0].notes;
                listTimers.Timers[0].timeDisplay.Text = restore[0].billableTime;
                     
                if (restore.Count > 1) //more than one record to restore, create more timers
                {
                    for (int i = 1; i < restore.Count; i++) //add one timer for each record
                    {
                        if (numTimers < restore.Count)
                            listTimers.addNewTimer(this); //call timer list to add timer

                        listTimers.Timers[i].wipe();
                        listTimers.Timers[i].SetStopwatch(restore[i].actualTime);
                        listTimers.Timers[i].auxDisplay.Text = listTimers.Timers[i].GetStopwatch();
                        listTimers.Timers[i].text.Text = restore[i].notes;
                        listTimers.Timers[i].timeDisplay.Text = restore[i].billableTime;
                    }
                }
            }
        }

        public class MaximumTimers : Exception //maximum number of timers exception
        {
            public MaximumTimers(string message) : base(message)
            { }
        }

        private void addTimerToolStripMenuItem_Click(object sender, EventArgs e) //adds a new timer to form
        {
            try //attempt to add timer
            {
                listTimers.addNewTimer(this); //call timer list to add timer
            }
            catch (MaximumTimers er) //exception
            { 
                Console.WriteLine("Maximum Number of Timers Reached", er);
                MessageBox.Show("Maximum Number of Timers Reached", "No More Timers",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            } 
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toggleTopmost();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }
    }
}
