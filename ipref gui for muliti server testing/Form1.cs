using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Renci.SshNet;
using System.Reflection;
using System.Deployment.Application;
using System.Runtime.InteropServices;



/// <summary>
/// TODO:
/// * Lav klasser
/// * Ryd op i kode der ikke bliver brugt 
/// </summary>
namespace ipref_gui_for_muliti_server_testing
{
    
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();
        
        
        // ---------------------------------------------------------- //
        // Start ting
        // ---------------------------------------------------------- //
        private bool debug = false;
        Process ipref3_server = new Process(); // Starting of the process ipref3_server for global access
        bool iperf_running = false; // Definning if iperf is still running 
        int test_number = 0; // Defining what test there is running for iperf
        string arg = ""; // Global access to the givning argument for the ipref client
        int test_number_ping = 0; // Defining what test there is running for ping
        string protocol = ""; // Global access to the protocol type for ipref "TCP/UDP"


        // ---------------------------------------------------------- //
        // Getting info about what version the program is runing
        // ---------------------------------------------------------- //
        /*
        public string CurrentVersion
        {
            get
            {
                return ApplicationDeployment.IsNetworkDeployed
                       ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString()
                       : Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        */

        // ---------------------------------------------------------- //
        // Start From1
        // ---------------------------------------------------------- //
        public Form1(string[] args)
        {
            InitializeComponent();
            if (Environment.GetCommandLineArgs().Contains("-debug")) //Tjeking if -debug arg is set and then ativate debug mode for more debug info
            {
                debug = true;
            }
            else
            {
                debug = false;
            }
            //var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.ProductVersion;
            //version = (FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location).ProductVersion).ToString();
            //Text = String.Format("iPerf test gui v" + version);

            if (ApplicationDeployment.IsNetworkDeployed == true)
            {
                version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            else
            {
                version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            Text = String.Format("Bandwidth And Ping Test Tool V" + version); //Setting the text in the top of the window like "Task Manger"
            try
            {
                foreach (var process in Process.GetProcessesByName("iperf3"))
                {
                    process.Kill(); // Trying to kill all iperf3 processes there is running on the system 
                }
            }
            catch (Exception e)
            {
                if (debug)
                {
                    MessageBox.Show(e.ToString(), "Error"); //If debug is on show error msg
                }
            }
        }


        // ---------------------------------------------------------- //
        // Function to get the current time and date
        // ---------------------------------------------------------- //
        static public string get_time_and_date()
        {
            //Console.WriteLine(DateTime.Now); //18-04-2016 10:12:41
            string date_and_time = DateTime.Now.ToString(); //18-04-2016 10:12:41

            return date_and_time.Replace(":", "-"); // Changeing ":" to "-" 18-04-2016 10-12-41 so it can be used in file and folder names
        }

        string shared_time_and_date = get_time_and_date(); // Getting the current time and saving it to a string for later use

        // ---------------------------------------------------------- //
        // Function to define the path where the log shoud be saved
        // ---------------------------------------------------------- //
        public string get_log_path(string log_type) // 
        {
            //string path = Directory.GetCurrentDirectory() + "\\log\\" + shared_time_and_date + " - " + log_type + ".csv";
            Directory.CreateDirectory(@Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\log - ping and speedtester"); // Making the log folder if not do exists
            string path = @Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\log - ping and speedtester\\" + log_type + " - " + shared_time_and_date + ".csv"; // Defining the folder path to a folder "log - ping and speedtester" on the desktop
            return path; // Retuning the log folder path
        }

        // ---------------------------------------------------------- //
        // Ipref3 TCP download
        // ---------------------------------------------------------- //
        //Thread th = new Thread(() => start_ipref3_async(arg));
        private void button1_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled == false) 
            {
                if (toolStripProgressBar1.Value == 1)
                {
                    iperf_running = true; 
                    MessageBox.Show(
                        "Please wait for iperf to exit!",
                        "Retard alert",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    shared_time_and_date = get_time_and_date(); // Updating the time and date becuse vi start a new test
                    test_number++; // Counting the test number for difrent log
                    timer1.Enabled = true; // Enabeling the time
                    toolStripProgressBar1.Value = 1; // Setting the ProgressBar atvive to visual se that the test is running
                    toolStripProgressBar2.Value = 1; // Setting the ProgressBar atviveto visual se that the test is running
                    //Task task = Task.Run((void));
                    if (textBox_TCP_bitrate.Text == "0") // If the bitrate is not set run this arg for max bitrate
                    {
                        arg = "-R -i " + numericUpDown_TCP_Interval.Value + // Setting the logging interval
                        " -P " + numericUpDown_TCP_parallele_streams.Value + // Setting the number of paralle streams
                        " -l " + numericUpDown_TCP_pakke_storlse.Value + // Setting packet size
                        " -c " + textBox_TCP_IP_DNS.Text + // Setting the target ip or host by dns
                        " -p " + numericUpDown_TCP_Port.Value + // Setting the target port
                        " -t " + numericUpDown_TCP_runtime.Value; // Setting the runtime 
                    }
                    else // If the bitrate is set run this arg for limiet bitrate
                    {
                        arg = "-R -i " + numericUpDown_TCP_Interval.Value + // Setting the logging interval
                        " -P " + numericUpDown_TCP_parallele_streams.Value + // Setting the number of paralle streams
                        " -l " + numericUpDown_TCP_pakke_storlse.Value + // Setting packet size
                        " -b " + textBox_TCP_bitrate.Text + // Setting the target bitrate
                        " -c " + textBox_TCP_IP_DNS.Text + // Setting the target ip or host by dns
                        " -p " + numericUpDown_TCP_Port.Value + // Setting the target port
                        " -t " + numericUpDown_TCP_runtime.Value; // Setting the runtime 
                    }
                    protocol = "TCP"; // Defining the protocol for the test for log info
                    start_ipref3_async(arg); // Starting iperf3 whit arg
                    timer1.Start(); // Stating the timer
                    this.btn_TCP_DL.Text = "Stop"; // Chagning the button text so the user knows how to stop the test
                }
            }
            else if (timer1.Enabled == true)
            {
                timer1.Enabled = false; // Deativating the timer
                timer1.Stop(); // Stopping the timer
                toolStripProgressBar2.Value = 0; // Chagning the ProgressBar for visual info
                this.btn_TCP_DL.Text = "Download"; // Chagning the buttom back to its orginal state
            }
        }

        // ---------------------------------------------------------- //
        // Ipref3 TCP upload
        // ---------------------------------------------------------- //
        private void button5_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled == false)
            {
                if (toolStripProgressBar1.Value == 1)
                {
                    iperf_running = true;
                    MessageBox.Show(
                        "Please wait for iperf to exit!",
                        "Retard alert",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    shared_time_and_date = get_time_and_date(); // Updating the time and date becuse vi start a new test
                    test_number++; // Counting the test number for difrent log
                    timer1.Enabled = true;  // Enabeling the timer
                    toolStripProgressBar1.Value = 1; // Setting the ProgressBar atvive to visual se that the test is running
                    toolStripProgressBar2.Value = 1; // Setting the ProgressBar atviveto visual se that the test is running
                    //Task task = Task.Run((void));
                    if (textBox_TCP_bitrate.Text == "0") // If the bitrate is not set run this arg for max bitrate
                    {
                        arg = "-i " + numericUpDown_TCP_Interval.Value + // Setting the logging interval
                        " -P " + numericUpDown_TCP_parallele_streams.Value +  // Setting the number of paralle streams
                        " -l " + numericUpDown_TCP_pakke_storlse.Value + // Setting packet size
                        " -c " + textBox_TCP_IP_DNS.Text + // Setting the target ip or host by dns
                        " -p " + numericUpDown_TCP_Port.Value + // Setting the target port
                        " -t " + numericUpDown_TCP_runtime.Value; // Setting the runtime
                    }
                    else
                    {
                        arg = "-i " + numericUpDown_TCP_Interval.Value + // Setting the logging interval
                        " -P " + numericUpDown_TCP_parallele_streams.Value + // Setting the number of paralle streams
                        " -l " + numericUpDown_TCP_pakke_storlse.Value + // Setting packet size
                        " -b " + textBox_TCP_bitrate.Text + // Setting the target bitrate
                        " -c " + textBox_TCP_IP_DNS.Text + // Setting the target ip or host by dns
                        " -p " + numericUpDown_TCP_Port.Value + // Setting the target port
                        " -t " + numericUpDown_TCP_runtime.Value; // Setting the runtime
                    }
                    protocol = "TCP"; // Defining the protocol for the test for log info
                    start_ipref3_async(arg); // Starting iperf3 whit arg
                    timer1.Start(); // Stating the timer
                    this.btn_TCP_UL.Text = "Stop";  // Chagning the button text so the user knows how to stop the test
                }
            }
            else if (timer1.Enabled == true)
            {
                timer1.Enabled = false; // Deativating the timer
                timer1.Stop(); // Stopping the timer
                toolStripProgressBar2.Value = 0; // Chagning the ProgressBar for visual info
                this.btn_TCP_UL.Text = "Upload"; // Chagning the buttom back to its orginal state
            }
        }

        // ---------------------------------------------------------- //
        // Ipref3 UDP download
        // ---------------------------------------------------------- //
        private void button7_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled == false)
            {
                if (toolStripProgressBar1.Value == 1)
                {
                    iperf_running = true;
                    MessageBox.Show(
                        "Please wait for iperf to exit!",
                        "Retard alert",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    shared_time_and_date = get_time_and_date();  // Updating the time and date becuse vi start a new test
                    test_number++; // Counting the test number for difrent log
                    timer1.Enabled = true; // Enabeling the time
                    toolStripProgressBar1.Value = 1; // Setting the ProgressBar atvive to visual se that the test is running
                    toolStripProgressBar2.Value = 1; // Setting the ProgressBar atviveto visual se that the test is running
                    //Task task = Task.Run((void));
                    arg = "-R -u" +
                        " -i " + numericUpDown_UDP_Interval.Value +  // Setting the logging interval
                        " -P " + numericUpDown_UDP_parallele_streams.Value + // Setting the number of paralle streams
                        " -l " + numericUpDown_UDP_pakke_storlse.Value + // Setting packet size
                        " -b " + textBox_UDP_bitrate.Text + // Setting the target bitrate
                        " -c " + textBox_UDP_IP_DNS.Text + // Setting the target ip or host by dns
                        " -p " + numericUpDown_UDP_Port.Value + // Setting the target port
                        " -t " + numericUpDown_UDP_runtime.Value; // Setting the runtime 
                    protocol = "UDP"; // Defining the protocol for the test for log info
                    start_ipref3_async(arg); // Starting iperf3 whit arg
                    timer1.Start(); // Stating the timer
                    this.btn_UDP_DL.Text = "Stop"; // Chagning the button text so the user knows how to stop the test
                }
            }
            else if (timer1.Enabled == true)
            {
                timer1.Enabled = false; // Deativating the timer
                timer1.Stop(); // Stopping the timer
                toolStripProgressBar2.Value = 0; // Chagning the ProgressBar for visual info
                this.btn_UDP_DL.Text = "Download"; // Chagning the buttom back to its orginal state
            }
        }

        // ---------------------------------------------------------- //
        // Ipref3 UDP upload
        // ---------------------------------------------------------- //
        private void button6_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled == false)
            {
                if (toolStripProgressBar1.Value == 1)
                {
                    iperf_running = true;
                    MessageBox.Show(
                        "Please wait for iperf to exit!",
                        "Retard alert",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    shared_time_and_date = get_time_and_date(); // Updating the time and date becuse vi start a new test
                    test_number++; // Counting the test number for difrent log
                    timer1.Enabled = true; // Enabeling the time
                    toolStripProgressBar1.Value = 1; // Setting the ProgressBar atvive to visual se that the test is running
                    toolStripProgressBar2.Value = 1; // Setting the ProgressBar atviveto visual se that the test is running
                    //Task task = Task.Run((void));
                    arg = "-u" +
                        "-i " + numericUpDown_UDP_Interval.Value + // Setting the logging interval
                        " -P " + numericUpDown_UDP_parallele_streams.Value + // Setting the number of paralle streams
                        " -l " + numericUpDown_UDP_pakke_storlse.Value + // Setting packet size
                        " -b " + textBox_UDP_bitrate.Text + // Setting the target bitrate
                        " -c " + textBox_UDP_IP_DNS.Text + // Setting the target ip or host by dns
                        " -p " + numericUpDown_UDP_Port.Value + // Setting the target port
                        " -t " + numericUpDown_UDP_runtime.Value; // Setting the runtime 
                    protocol = "UDP"; // Defining the protocol for the test for log info
                    start_ipref3_async(arg); // Starting iperf3 whit arg
                    timer1.Start(); // Stating the timer
                    this.btn_UDP_UL.Text = "Stop"; // Chagning the button text so the user knows how to stop the test
                }
            }
            else if (timer1.Enabled == true)
            {
                timer1.Enabled = false; // Deativating the timer
                timer1.Stop(); // Stopping the timer
                toolStripProgressBar2.Value = 0; // Chagning the ProgressBar for visual info
                this.btn_UDP_UL.Text = "Upload"; // Chagning the buttom back to its orginal state
            }
        }

        // ---------------------------------------------------------- //
        // Ipref3 client
        // ---------------------------------------------------------- //
        private void start_ipref3_async(string arguments)
        {
            //* Create your Process
            Process process = new Process(); // Create the process "process"
            process.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\iperf3.exe"; // Define what file we want too run
            process.StartInfo.Arguments = arguments; // Defining the arg too run the tile whit
            process.StartInfo.CreateNoWindow = true; // Dont make a window
            process.StartInfo.UseShellExecute = false; // Dont use shell exexute
            process.StartInfo.RedirectStandardOutput = true; // Redirect Standard Output for log use
            process.StartInfo.RedirectStandardError = true; // Redirect Standard Error for debug and log use
            //* Set your output and error (asynchronous) handlers
            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler); // Use OutputHandler for the Standard Output data
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler); // Use OutputHandler for the Error data
            //* Start process and handlers
            using (StreamWriter file = new StreamWriter(get_log_path("ipref3_"+protocol+"_log"), true)) // Getting log path and defineing the name of the log and make it ready for write
            {
                file.WriteLine(""); //Write "" (nothing) in the file (making a new line)
                file.WriteLine("Test nr: " + test_number + ". - " + DateTime.Now.ToString()); // Write the testnumber and the date plus time in the file
                file.WriteLine("-----------------------------------------------------------"); // Add som "-" to the file for separation
            }
            using (StreamWriter file = new StreamWriter(get_log_path("server_log"), true)) // Getting log path and defineing the name of the log and make it ready for write
            {
                file.WriteLine(""); //Write "" (nothing) in the file (making a new line)
                file.WriteLine("Test nr: " + test_number + ". - " + DateTime.Now.ToString()); // Write the testnumber and the date plus time in the file
            }

            process.Start(); // Starting the process "process" (iperf3)
            process.BeginOutputReadLine(); // Read Output
            process.BeginErrorReadLine(); // Read Errors
            //process.WaitForExit();
        }

        // ---------------------------------------------------------- //
        // OutputHandler
        // ---------------------------------------------------------- //
        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            //Console.WriteLine(outLine.Data);
            this.Invoke((new MethodInvoker(delegate () // Output when posiabel 
            {
                try
                {
                    if (timer1.Enabled == false && iperf_running == true)
                    {
                        iperf_running = false;
                        MessageBox.Show(
                                "iperf has exited",
                                "",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information); // Tell the user that iperf has exited after the user stoped the timer

                    }
                    if (protocol == "TCP")
                    {
                        textBox_TCP_log.AppendText(outLine.Data + Environment.NewLine); // White Output data to textbox in interface
                    }
                    if (protocol == "UDP")
                    {
                        textBox_UDP_log.AppendText(outLine.Data + Environment.NewLine); // White Output data to textbox in interface
                    }
                    toolStripProgressBar1.Value = 0; // Showing that iperf3 is not running anymore

                    using (StreamWriter file = new StreamWriter(get_log_path("ipref3_log"), true))
                    {
                        file.WriteLine(outLine.Data); // White Output data to file
                    }

                }
                catch (Exception)
                {
                }

            })));
            //richTextBox1.Text = outLine.Data;
        }

        // ---------------------------------------------------------- //
        // Timer 1 Tick
        // ---------------------------------------------------------- //
        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripProgressBar1.Value = 1; // Showing that iperf3 is running
            start_ipref3_async(arg); // Starting iperf3 whit the lateste definded arg
        }


        // ---------------------------------------------------------- //
        // Ping function
        // ---------------------------------------------------------- //
        /// <summary>
        /// Ping1s this instance.
        /// </summary>
        /// <param name="Ip">The ip.</param>
        /// <param name="times">Times to ping.</param>
        /// <param name="name">The name of the log file.</param>
        public bool ping(string Ip, string times, string name, bool echo)
        {
            string cmd = Ip + " I -q -i 0 -n " + times; // Defining the arg for psping
            Process proc = new Process(); // Making a new process "proc"
            proc.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\psping.exe"; // Defining the file to open
            proc.StartInfo.Arguments = cmd; // Defining the arg to start the process whit
            proc.StartInfo.UseShellExecute = false; // Dont use Shell execute
            proc.StartInfo.RedirectStandardOutput = true; //Redirect Standard Output 
            proc.StartInfo.CreateNoWindow = true; // Dont make at window

            try
            {
                proc.Start(); // Start the process "proc"
                string output = proc.StandardOutput.ReadToEnd(); // Read the output for the process "proc"
                output = output.Substring(output.LastIndexOf(" ")); // Splitting the ourput by " " (space)
                output = output.TrimEnd(Environment.NewLine.ToCharArray()).Replace("ms", ""); // Trim to only get what we want of the output and remove "ms" from the output wee trimed
                if (echo)
                {
                    try
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            if (name == "ping_1")
                                tekst_boks_ping_ud_1.Text = output; // Set textbox to output
                            if (name == "ping_2")
                                tekst_boks_ping_ud_2.Text = output; // Set textbox to output
                        });
                    }
                    catch (Exception e)
                    {
                        if (debug)
                            MessageBox.Show(e.ToString(), "Error"); // If it coud not set the textbox show an error if debug is on
                        return false;
                    }
                }
                Console.WriteLine("Ping: " + output); // Write the ping result in the console           
                using (StreamWriter file = new StreamWriter(get_log_path(name), true)) // Getting the log path and setting its name
                {
                    file.WriteLine(output); // Add the output to the file
                    return true;
                }
            }
            catch (Exception e)
            {
                if(debug)
                    MessageBox.Show(e.ToString(),"Error");
                //throw;
                return false;
            }
            
        }       
        // ---------------------------------------------------------- //
        // Ping all
        // ---------------------------------------------------------- //
        private void knap_alle_Click(object sender, EventArgs e)
        {
            shared_time_and_date = get_time_and_date(); // Update time and date for new log file
            test_number_ping++; // What test is it since start
            progressBar1.Maximum = (int)numericUpDown1.Value; // Chagning the max of the progressbar to the number of ping we have to prefome
            Thread th1 = new Thread(() => run_more_times((int)numericUpDown1.Value, tekst_boks_ip_adresse_1.Text, antal_ping_1.Value.ToString(), "ping_1", true)); // Stating a new thread that pings to not block the interface
            th1.IsBackground = true; // Let it run in the backgrund
            th1.Start(); // Start the thread
            Thread th2 = new Thread(() => run_more_times((int)numericUpDown1.Value, tekst_boks_ip_adresse_2.Text, antal_ping_2.Value.ToString(), "ping_2", true)); // Stating a new thread that pings to not block the interface
            th2.IsBackground = true; // Let it run in the backgrund
            th2.Start(); // Start the thread
        }

        // ---------------------------------------------------------- //
        // Function for Ping all
        // ---------------------------------------------------------- //
        private void run_more_times(int antal, string Ip1, string times1, string name1, bool echo)
        {
            Stopwatch sw = new Stopwatch(); // make a new stopwatch
            if (echo)
            {
                try
                {
                    Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = 0; // Set the progessbar to 0
                    });
                }
                catch (Exception e)
                {
                    if (debug)
                        MessageBox.Show(e.ToString(), "Error"); // Show error if debug is on
                }
            }

            for (int i = 0; i < antal; i++)
            {
                sw.Start(); // Start the stopwatch
                if (echo)
                {
                    try
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            progressBar1.Value = i; // Set the progessbar to that ping test number we is on to visualle se how long the test is
                        });
                    }
                    catch (Exception)
                    {
                        //throw;
                    }
                }
                Thread th = new Thread(() => ping(Ip1, times1, name1, echo)); // Start a new Thread for the ping function
                th.IsBackground = true; // Let it run in the backgrund

                th.Start(); // Start the ping

                th.Join(); // Whait until done
                th.Abort(); // Close the thread
                sw.Stop(); // Stop the stopwatch
                Console.WriteLine(name1 + " tog " + (int)sw.ElapsedMilliseconds + "ms"); // Tell how log it took to run the ping (only for debug info)
                //Thread.Sleep(1000 - (int)sw.ElapsedMilliseconds);
                Thread.Sleep(1000); // Let the thread sleep so we do not spamm ping's
                sw.Reset(); // Reamember to reset the stopwatch
            }
            if (echo)
            {
                try
                {
                    Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = 0; // Set the progressbar to 0
                    });
                }
                catch (Exception e)
                {
                    if (debug)
                        MessageBox.Show(e.ToString(), "Error");
                }
            }
            

        }

        // ---------------------------------------------------------- //
        // Iperf server start buttom
        // ---------------------------------------------------------- //
        private void button2_Click_1(object sender, EventArgs e)
        {

            if (button2.Text == "Start")
            {
                button2.Text = "Stop"; // Set the buttom text to "stop"
                string port = numericUpDown_server_port.Value.ToString(); // Getting the port number for the server to start listen on
                arg = " -s -i 1 -p " + port; // Setting the arg for the server 
                start_ipref3_async_server(arg); // Stating the server
                toolStripProgressBar3.Value = 1; // Setting the progressbar to 1
            }
            else if (button2.Text == "Stop")
            {
                try
                {
                    //ipref3_server.Kill();
                    //ipref3_server.CancelOutputRead();
                    //ipref3_server.CancelErrorRead();
                    //ipref3_server.Close();
                    toolStripProgressBar3.Value = 0; // Setting the progressbar to 
                    foreach (var process in Process.GetProcessesByName("iperf3"))
                    {
                        process.Kill(); // Kill all iperf3 to stop the server
                }
                }
                catch (Exception)
                {
                    if (debug)
                        MessageBox.Show(e.ToString(), "Error");
                }
                button2.Text = "Start"; // Set the buttom text to "start
            }
        }

        // ---------------------------------------------------------- //
        // Iperf server
        // ---------------------------------------------------------- //
        private void start_ipref3_async_server(string arguments)
        {
            //* Create your Process
            Process ipref3_server = new Process(); // Create the process "ipref3_server"
            ipref3_server.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\iperf3.exe"; // Define what file we want too run
            ipref3_server.StartInfo.Arguments = arguments; // Defining the arg too run the tile whit
            ipref3_server.StartInfo.CreateNoWindow = true; // Dont make a window
            ipref3_server.StartInfo.UseShellExecute = false; // Dont use shell exexute
            ipref3_server.StartInfo.RedirectStandardOutput = true; // Redirect Standard Output for log use
            ipref3_server.StartInfo.RedirectStandardError = true; // Redirect Standard Error for debug and log use
            //* Set your output and error (asynchronous) handlers
            ipref3_server.OutputDataReceived += new DataReceivedEventHandler(OutputHandler_server); // Use OutputHandler for the Standard Output data
            ipref3_server.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler_server); // Use OutputHandler for the Error data


            using (StreamWriter file = new StreamWriter(get_log_path("server_log"), true)) // Getting log path and defineing the name of the log and make it ready for write
            {
                file.WriteLine("Server startet - " + DateTime.Now.ToString()); // Write some info in the file and the date plus time in the file
                file.WriteLine(""); // Write "" (nothing) in the file (making a new line)
            }
            

            //* Start process and handlers
            ipref3_server.Start(); // Starting the process "ipref3_server"
            try
            {
                ipref3_server.BeginOutputReadLine(); // Read Output
                ipref3_server.BeginErrorReadLine(); // Read Errors
            }
            catch (Exception e)
            {
                if (debug)
                    MessageBox.Show(e.ToString(), "Error"); // Show if error if debug is on
            }
            //process.WaitForExit();
        }

        private void OutputHandler_server(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            //Console.WriteLine(outLine.Data);
            try
            {
                this.Invoke((new MethodInvoker(delegate () // Output when posiabel 
                {
                    try
                    {
                        textBox_server_log.AppendText(outLine.Data + Environment.NewLine); // White Output data to textbox in interface

                        using (StreamWriter file = new StreamWriter(get_log_path("server_log"), true))
                        {
                            file.WriteLine(outLine.Data); // White Output data to file
                        }
                    }
                    catch (Exception e)
                    {
                        if (debug)
                            MessageBox.Show(e.ToString(), "Error"); // Show if error if debug is on
                    }

                })));
            }
            catch (Exception e)
            {
                if (debug)
                    MessageBox.Show(e.ToString(), "Error"); // Show if error if debug is on
            }
            //richTextBox1.Text = outLine.Data;
        }


        // ---------------------------------------------------------- //
        // Iperf3 TCP Nulstil
        // ---------------------------------------------------------- //
        private void button8_Click(object sender, EventArgs e)
        {
            //Sets all the default settings for iperf3 TCP for interface reset
            numericUpDown_TCP_Interval.Value = 1; // Sets interval to 1
            numericUpDown_TCP_parallele_streams.Value = 1; // Sets paralle stream to 1
            numericUpDown_TCP_pakke_storlse.Value = 16000; // Sets packes size to 16000
            textBox_TCP_IP_DNS.Text = "localhost"; // Sets ip/dns name to localhost
            numericUpDown_TCP_Port.Value = 5201; // Sets port to 5201
            textBox_TCP_bitrate.Text = "0"; // Sets bitrate to 0
            numericUpDown_TCP_runtime.Value = 10; // Sets runtime to 10 sec
        }

        // ---------------------------------------------------------- //
        // Ipref3 UDP nulstil
        // ---------------------------------------------------------- //
        private void button14_Click(object sender, EventArgs e)
        {
            //Sets all the default settings for iperf3 UDP for interface reset
            numericUpDown_UDP_Interval.Value = 1; // Sets interval to 1
            numericUpDown_UDP_parallele_streams.Value = 1; // Sets paralle stream to 1
            numericUpDown_UDP_pakke_storlse.Value = 8000; // Sets packes size to 8000
            textBox_UDP_IP_DNS.Text = "localhost"; // Sets ip/dns name to localhost
            numericUpDown_UDP_Port.Value = 5201; // Sets port to 5201
            textBox_UDP_bitrate.Text = "1M"; // Sets bitrate to 1 Mbit
            numericUpDown_UDP_runtime.Value = 10; // Sets runtime to 10 sec
        }

        private void antal_ping_1_ValueChanged(object sender, EventArgs e)
        {

        }

        // ---------------------------------------------------------- //
        // Events
        // ---------------------------------------------------------- //
        private void tekst_boks_ip_adresse_1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                btn_ping1.PerformClick(); // If enter is hit when ip/dns (for ping test) box is chosen then klik buttom to start ping test
            }
        }

        private void tekst_boks_ip_adresse_2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                btn_ping2.PerformClick(); // If enter is hit when ip/dns (for ping test) box is chosen then klik buttom to start ping test
            }
        }

        private void antal_ping_1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                btn_ping1.PerformClick();  // If enter is hit when number of pings (for ping test) box is chosen then klik buttom to start ping test
            }
        }

        private void antal_ping_2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                btn_ping2.PerformClick(); // If enter is hit when number of pings (for ping test) box is chosen then klik buttom to start ping test
            }
        }

        private void btn_ping1_Click(object sender, EventArgs e)
        {
            shared_time_and_date = get_time_and_date(); // Update time and date for new log file 
            test_number_ping++; // Setting new test number for log info
            Thread th = new Thread(() => ping(tekst_boks_ip_adresse_1.Text, antal_ping_1.Value.ToString(), "ping_1", true)); // make new thread for ping to not stop interface setting ping target and number of pings to perform
            if (!th.IsAlive)
            {
                th.IsBackground = true; // Let it run in the backgrund
                th.Start(); // Start the ping 
            }
        }

        private void btn_ping2_Click(object sender, EventArgs e)
        {
            shared_time_and_date = get_time_and_date(); // Update time and date for new log file 
            test_number_ping++; // Setting new test number for log info
            Thread th = new Thread(() => ping(tekst_boks_ip_adresse_2.Text, antal_ping_2.Value.ToString(), "ping_2", true)); // make new thread for ping to not stop interface setting ping target and number of pings to perform
            if (!th.IsAlive)
            {
                th.IsBackground = true; // Let it run in the backgrund
                th.Start(); // Start the ping
            }
        }

        private void tekst_boks_ping_ud_1_MouseClick(object sender, MouseEventArgs e)
        {
            tekst_boks_ping_ud_1.SelectAll(); // When mouse click in text box select it all ->
            tekst_boks_ping_ud_1.Copy(); // <- And then Copy it
        }

        private void textBox_TCP_IP_DNS_TextChanged(object sender, EventArgs e)
        {
            textBox_UDP_IP_DNS.Text = textBox_TCP_IP_DNS.Text; // When the text is chagend update it in the other textbox
        }

        private void textBox_UDP_IP_DNS_TextChanged(object sender, EventArgs e)
        {
            textBox_TCP_IP_DNS.Text = textBox_UDP_IP_DNS.Text; // When the text is chagend update it in the other textbox
        }

        private void numericUpDown_TCP_Port_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown_UDP_Port.Value = numericUpDown_TCP_Port.Value; // When the value of the port is chagend update the other port value
            numericUpDown_server_port.Value = numericUpDown_TCP_Port.Value; // When the value of the port is chagend update the other port value
        }

        private void numericUpDown_UDP_Port_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown_TCP_Port.Value = numericUpDown_UDP_Port.Value; // When the value of the port is chagend update the other port value
            numericUpDown_server_port.Value = numericUpDown_UDP_Port.Value; // When the value of the port is chagend update the other port value
        }

        private void numericUpDown_server_port_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown_TCP_Port.Value = numericUpDown_server_port.Value; // When the value of the port is chagend update the other port value
            numericUpDown_UDP_Port.Value = numericUpDown_server_port.Value; // When the value of the port is chagend update the other port value
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (toolStripProgressBar1.Value == 1)
            {
                MessageBox.Show("Testen er startet", "Error"); // Show error if the test is running and the buttom is click
            }
            else
            {
                Thread th = new Thread(() => test("tcp", 1)); // Else make a new thread for the test
                th.Start(); // Start the test
            }
        }

        private void btn_udpTest1_Click(object sender, EventArgs e)
        {
            if (toolStripProgressBar1.Value == 1)
            {
                MessageBox.Show("Testen er startet", "Error"); // Show error if the test is running and the buttom is click
            }
            else
            {
                Thread th = new Thread(() => test("udp", 1)); // Else make a new thread for the test
                th.Start(); // Start the test
            }
        }

        private void button_test2_tcp_Click(object sender, EventArgs e)
        {
            if (toolStripProgressBar1.Value == 1)
            {
                MessageBox.Show("Testen er startet", "Error"); // Show error if the test is running and the buttom is click
            }
            else
            {
                Thread th = new Thread(() => test("tcp", 2)); // Else make a new thread for the test
                th.Start(); // Start the test
            }
        }

        private void button_test2_udp_Click(object sender, EventArgs e)
        {
            if (toolStripProgressBar1.Value == 1)
            {
                MessageBox.Show("Testen er startet", "Error"); // Show error if the test is running and the buttom is click
            }
            else
            {
                Thread th = new Thread(() => test("udp", 2)); // Else make a new thread for the test
                th.Start(); // Start the test
            }
        }

        private void button_test3_tcp_Click(object sender, EventArgs e)
        {
            if (toolStripProgressBar1.Value == 1)
            {
                MessageBox.Show("Testen er startet", "Error"); // Show error if the test is running and the buttom is click
            }
            else
            {
                Thread th = new Thread(() => test("tcp", 3)); // Else make a new thread for the test
                th.Start(); // Start the test
            }
        }

        private void button_test3_udp_Click(object sender, EventArgs e)
        {
            if (toolStripProgressBar1.Value == 1)
            {
                MessageBox.Show("Testen er startet", "Error"); // Show error if the test is running and the buttom is click
            }
            else
            {
                Thread th = new Thread(() => test("udp", 3)); // Else make a new thread for the test
                th.Start(); // Start the test
            }
        }

        // ---------------------------------------------------------- //
        // Test function 
        // ---------------------------------------------------------- //
        private void test(string prot, int testnumber)
        {
            Stopwatch sw = new Stopwatch(); // Make a new stopwatch
            for (int i = 1; i < 31; i++) // Run it 30 times
            {
                this.Invoke((MethodInvoker)delegate
                {
                    toolStripProgressBar1.Value = 1; // Set the progressbar to "on" state
                    toolStripProgressBar2.Value = 1; // Set the progressbar to "on" state
                });
                shared_time_and_date = get_time_and_date(); // Update time and date for log use
                if (textBox_TCP_IP_DNS.Text != "localhost") // If the test target is not localhost
                {
                    startIperf(); // Then start iperf on the target using ssh
                    if (debug)
                    {
                        Console.WriteLine("iPerf started"); // Show ipref is startet on the target if debug is on
                    }
                }
                
                Thread.Sleep(1000); // Just take it easy and relax so things not fuck up

                Process process = new Process(); // Make a new process "process"

                if (testnumber == 1) // If the test type is 1 
                {
                    if (prot == "tcp")
                    {
                        arg = "-R -i " + numericUpDown_TCP_Interval.Value + // Setting the logging interval
                        " -P " + numericUpDown_TCP_parallele_streams.Value + // Setting the number of paralle streams
                        " -l " + numericUpDown_TCP_pakke_storlse.Value + // Setting packet size
                        " -b " + i + "M" + // Setting the target bitrate using the for loop to increase the target bitrate
                        " -c " + textBox_TCP_IP_DNS.Text + // Setting the target ip or host by dns
                        " -p " + numericUpDown_TCP_Port.Value + // Setting the target port
                        " -t 30000"; // Setting the runtime 
                    }
                    else if (prot == "udp")
                    {
                        arg = "-R -u" + // Defining udp by "-u"
                            " -i " + numericUpDown_UDP_Interval.Value + // Setting the logging interval
                            " -P " + numericUpDown_UDP_parallele_streams.Value + // Setting the number of paralle streams
                            " -l " + numericUpDown_UDP_pakke_storlse.Value + // Setting packet size
                            " -b " + i + "M" + // Setting the target bitrate using the for loop to increase the target bitrate
                            " -c " + textBox_UDP_IP_DNS.Text + // Setting the target ip or host by dns
                            " -p " + numericUpDown_UDP_Port.Value + // Setting the target port
                            " -t 30000"; // Setting the runtime 
                    }

                    protocol = prot; // Defining the protocol uses for log info
                    sw.Start(); // Start the stopwatch
                    //* Create your Process
                    process.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\iperf3.exe"; // Define what file we want too run
                    process.StartInfo.Arguments = arg; // Defining the arg too run the tile whit
                    process.StartInfo.CreateNoWindow = true; // Dont make a window
                    process.StartInfo.UseShellExecute = false; // Dont use shell exexute
                    process.StartInfo.RedirectStandardOutput = true; // Redirect Standard Output for log use
                    process.StartInfo.RedirectStandardError = true; // Redirect Standard Error for debug and log use
                    process.Start(); //Start the process
                }
                else if (testnumber == 2) // If the test type is 2
                {
                    if (i == 1 || i == 2 || i == 5 || i == 10 || i == 15 || i == 20 || i == 25 || i == 30) // Only make test whit bitrate of 0, 1, 5, 10, 15, 20, 25, 30
                    {
                        if (i == 1) // If 1 dont start iperf3 only do ping
                        {

                        }
                        else if (i == 2)
                        {
                            if (prot == "tcp")
                            {
                                arg = "-R -i " + numericUpDown_TCP_Interval.Value + // Setting the logging interval
                                    " -P " + numericUpDown_TCP_parallele_streams.Value + // Setting the number of paralle streams
                                    " -l " + numericUpDown_TCP_pakke_storlse.Value + // Setting packet size
                                    " -b " + 1 + "M" + // Setting the target bitrate to 1 mbit
                                    " -c " + textBox_TCP_IP_DNS.Text + // Setting the target ip or host by dns
                                    " -p " + numericUpDown_TCP_Port.Value + // Setting the target port
                                    " -t 86400"; // Setting the runtime 
                            }
                            else if (prot == "udp")
                            {
                                arg = "-R -u" +
                                    " -i " + numericUpDown_UDP_Interval.Value + // Setting the logging interval
                                    " -P " + numericUpDown_UDP_parallele_streams.Value + // Setting the number of paralle streams
                                    " -l " + numericUpDown_UDP_pakke_storlse.Value + // Setting packet size
                                    " -b " + 1 + "M" + // Setting the target bitrate to 1 mbit
                                    " -c " + textBox_UDP_IP_DNS.Text + // Setting the target ip or host by dns
                                    " -p " + numericUpDown_UDP_Port.Value + // Setting the target port
                                    " -t 86400"; // Setting the runtime
                            }
                            protocol = prot; // Defining the protocol uses for log info
                            sw.Start(); // Start the stopwatch
                            //* Create your Process
                            process.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\iperf3.exe"; // Define what file we want too run
                            process.StartInfo.Arguments = arg; // Defining the arg too run the tile whit
                            process.StartInfo.CreateNoWindow = true; // Dont make a window
                            process.StartInfo.UseShellExecute = false; // Dont use shell exexute
                            process.StartInfo.RedirectStandardOutput = true; // Redirect Standard Output for log use
                            process.StartInfo.RedirectStandardError = true; // Redirect Standard Error for debug and log use
                            process.Start(); //Start the process
                        }
                        else
                        {
                            if (prot == "tcp")
                            {
                                arg = "-R -i " + numericUpDown_TCP_Interval.Value +  // Setting the logging interval
                                    " -P " + numericUpDown_TCP_parallele_streams.Value + // Setting the number of paralle streams
                                    " -l " + numericUpDown_TCP_pakke_storlse.Value + // Setting packet size
                                    " -b " + i + "M" + // Setting the target bitrate using the for loop to increase the target bitrate
                                    " -c " + textBox_TCP_IP_DNS.Text + // Setting the target ip or host by dns
                                    " -p " + numericUpDown_TCP_Port.Value + // Setting the target port
                                    " -t 86400"; // Setting the runtime 
                            }
                            else if (prot == "udp")
                            {
                                arg = "-R -u" + // Defining udp by "-u"
                                    " -i " + numericUpDown_UDP_Interval.Value +  // Setting the logging interval
                                    " -P " + numericUpDown_UDP_parallele_streams.Value + // Setting the number of paralle streams
                                    " -l " + numericUpDown_UDP_pakke_storlse.Value +// Setting packet size
                                    " -b " + i + "M" + // Setting the target bitrate using the for loop to increase the target bitrate
                                    " -c " + textBox_UDP_IP_DNS.Text + // Setting the target ip or host by dns
                                    " -p " + numericUpDown_UDP_Port.Value + // Setting the target port
                                    " -t 86400"; // Setting the runtime 
                            }
                            protocol = prot; // Defining the protocol uses for log info
                            sw.Start(); // Start the stopwatch
                            //* Create your Process
                            process.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\iperf3.exe"; // Define what file we want too run
                            process.StartInfo.Arguments = arg; // Defining the arg too run the tile whit
                            process.StartInfo.CreateNoWindow = true; // Dont make a window
                            process.StartInfo.UseShellExecute = false; // Dont use shell exexute
                            process.StartInfo.RedirectStandardOutput = true; // Redirect Standard Output for log use
                            process.StartInfo.RedirectStandardError = true; // Redirect Standard Error for debug and log use
                            process.Start(); //Start the process
                        }
                    }
                }
                else if (testnumber == 3)
                {
                    if (prot == "tcp")
                    {

                    }
                    if (prot == "udp")
                    {

                    }
                }
                
                Console.WriteLine("Ping started!"); // print in console that ping is startet
                if (testnumber == 1) // Ping for test type 1
                {
                    for (int j = 0; j < 200; j++)
                    {
                        if (prot == "tcp")
                        {
                            ping(textBox_TCP_IP_DNS.Text, "1", "ping test 1 " + textBox_TCP_IP_DNS.Text + " " + prot + " +" + i.ToString().PadLeft(2, '0') + " M " + prot, false); // Start the ping
                            Console.WriteLine("Ping sent: " + j); // Print in console what ping number we is on
                            this.Invoke((MethodInvoker)delegate
                            {
                                Test_status_label.Text = "Speed " + i + "Mbit"; // Show in interface what iperf3 speed we is running at
                                Test_status_label2.Text = "Ping sent: " + j; // Show in interface what ping number we is on
                            });
                            Thread.Sleep(1000);
                            Console.WriteLine("Slept");
                        }
                        if (prot == "udp")
                        {
                            ping(textBox_UDP_IP_DNS.Text, "1", "ping test 1 " + textBox_UDP_IP_DNS.Text + " " + prot + " +" + i.ToString().PadLeft(2, '0') + " M " + prot, false); // Start the ping
                            Console.WriteLine("Ping sent: " + j); // Print in console what ping number we is on
                            this.Invoke((MethodInvoker)delegate
                            {
                                Test_status_label.Text = "Speed " + i + "Mbit"; // Show in interface what iperf3 speed we is running at
                                Test_status_label2.Text = "Ping sent: " + j; // Show in interface what ping number we is on
                            });
                            Thread.Sleep(1000);
                            Console.WriteLine("Slept");
                        }
                    }
                }
                else if (testnumber == 2) // Ping for test type 2
                {
                    if (i == 1 || i == 2 || i == 5 || i == 10 || i == 15 || i == 20 || i == 25 || i == 30)
                    {
                        for (int j = 0; j < 10000; j++)
                        {
                            if (prot == "tcp")
                            {
                                if (i == 1)
                                {
                                    ping(textBox_TCP_IP_DNS.Text, "1", "ping test 2 " + textBox_TCP_IP_DNS.Text + " " + prot + " + 00 M " + prot, false); // Start the ping
                                }
                                else if (i == 2)
                                {
                                    ping(textBox_TCP_IP_DNS.Text, "1", "ping test 2 " + textBox_TCP_IP_DNS.Text + " " + prot + " + 01 M " + prot, false); // Start the ping
                                }
                                else
                                {
                                    ping(textBox_TCP_IP_DNS.Text, "1", "ping test 2 " + textBox_TCP_IP_DNS.Text + " " + prot + " +" + i.ToString().PadLeft(2, '0') + " M " + prot, false); // Start the ping
                                }

                                Console.WriteLine("Ping sent: " + j); // Print in console what ping number we is on
                                this.Invoke((MethodInvoker)delegate
                                {
                                    if (i == 1)
                                    {
                                        Test_status_label.Text = "Speed " + 0 + "Mbit"; // Show in interface what iperf3 speed we is running at
                                        Test_status_label2.Text = "Ping sent: " + j; // Show in interface what ping number we is on
                                    }
                                    else if (i == 2)
                                    {
                                        Test_status_label.Text = "Speed " + 1 + "Mbit"; // Show in interface what iperf3 speed we is running at
                                        Test_status_label2.Text = "Ping sent: " + j; // Show in interface what ping number we is on
                                    }
                                    else
                                    {
                                        Test_status_label.Text = "Speed " + i + "Mbit"; // Show in interface what iperf3 speed we is running at
                                        Test_status_label2.Text = "Ping sent: " + j; // Show in interface what ping number we is on
                                    }
                                });
                                Thread.Sleep(250);
                                Console.WriteLine("Slept");
                            }
                            if (prot == "udp")
                            {
                                if (i == 1)
                                {
                                    ping(textBox_TCP_IP_DNS.Text, "1", "ping test 2 " + textBox_TCP_IP_DNS.Text + " " + prot + " + 00 M " + prot, false); // Start the ping
                                }
                                else if (i == 2)
                                {
                                    ping(textBox_TCP_IP_DNS.Text, "1", "ping test 2 " + textBox_TCP_IP_DNS.Text + " " + prot + " + 01 M " + prot, false); // Start the ping
                                }
                                else
                                {
                                    ping(textBox_TCP_IP_DNS.Text, "1", "ping test 2 " + textBox_TCP_IP_DNS.Text + " " + prot + " +" + i.ToString().PadLeft(2, '0') + " M " + prot, false); // Start the ping
                                }
                                Console.WriteLine("Ping sent: " + j); // Print in console what ping number we is on
                                this.Invoke((MethodInvoker)delegate
                                {
                                    if (i == 1)
                                    {
                                        Test_status_label.Text = "Speed " + 0 + "Mbit"; // Show in interface what iperf3 speed we is running at
                                        Test_status_label2.Text = "Ping sent: " + j; // Show in interface what ping number we is on
                                    }
                                    else if (i == 2)
                                    {
                                        Test_status_label.Text = "Speed " + 1 + "Mbit"; // Show in interface what iperf3 speed we is running at
                                        Test_status_label2.Text = "Ping sent: " + j; // Show in interface what ping number we is on
                                    }
                                    else
                                    {
                                        Test_status_label.Text = "Speed " + i + "Mbit"; // Show in interface what iperf3 speed we is running at
                                        Test_status_label2.Text = "Ping sent: " + j; // Show in interface what ping number we is on
                                    }
                                });
                                Thread.Sleep(250);
                                Console.WriteLine("Slept");
                            }
                        }
                    }
                }
                else if (testnumber == 3)
                {
                    if (prot == "tcp")
                    {

                    }
                    if (prot == "udp")
                    {

                    }
                }

                Console.WriteLine("Ping Done!" + sw.Elapsed.ToString()); // Print in console that ping is done
               
                sw.Reset(); // Reset the stopwatch
                this.Invoke((MethodInvoker)delegate
                {
                    toolStripProgressBar1.Value = 0; // Set the progressbar to 0
                    //toolStripProgressBar2.Value = 0;
                });
                Console.WriteLine("Thread sleeping");
                Thread.Sleep(2000);
                Console.WriteLine("Thead slept");
                if (testnumber == 2 || i == 1)
                {

                }
                else if (!process.HasExited)
                {
                    process.Kill(); // Kill the process if it is running
                    Console.WriteLine("iPerf was killed");

                }
                Console.WriteLine("Thread sleeping");
                Thread.Sleep(2000);
                Console.WriteLine("Thead slept");
            }
            this.Invoke((MethodInvoker)delegate
            {
                //toolStripProgressBar1.Value = 0;
                toolStripProgressBar2.Value = 0; // set the progressbar to 0 
                Test_status_label.Text = "Speed DONE"; // Showing that the test is done
                Test_status_label2.Text = "Ping DONE"; // Showing that the test is done
            });
            Console.WriteLine("Test done"); // Print in the console that the test is done
        }

        // ---------------------------------------------------------- //
        // Start ipref3 on the target using ssh
        // ---------------------------------------------------------- //
        private void startIperf()
        {
            using(var client = new SshClient(textBox_TCP_IP_DNS.Text, "pi", "raspberry")) // Making a new ssh client 
            {
                try
                {
                    client.Connect(); // Connet to the ssh client
                    Console.WriteLine("SSH established"); // print that ssh is established
                    client.RunCommand("killall iperf3"); // Run the command "killall ipref3"
                    Thread.Sleep(500); // Just wait a "bit"
                    client.RunCommand("iperf3 -s -D --d -V --logfile iperf3log-" + shared_time_and_date.Replace(' ','-')); // Start iperf3 on the target and use log file
                    Console.WriteLine("iPerf started with: -s -D --d -V --logfile iperf3log-" + shared_time_and_date.Replace(' ', '-')); // print what it have done
                    client.Disconnect(); // Disconnet from the SSH
                    Console.WriteLine("SSH Closed"); // Tell its done
                }
                catch (Exception)
                {
                    //throw;
                }
            }
        }

        // ---------------------------------------------------------- //
        // Andet
        // ---------------------------------------------------------- //
        private void Form1_Load(object sender, EventArgs e)
        {
            if(debug)
                AllocConsole();
            get_time_and_date();
            try
            {
                foreach (var process in Process.GetProcessesByName("iperf3"))
                {
                    process.Kill(); // When the program load kill all iperf3 on the system
                }
            }
            catch (Exception)
            {
                if (debug)
                    MessageBox.Show(e.ToString(), "Error"); // Show if error if debug is on
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                //ipref3_server.Kill();
                foreach (var process in Process.GetProcessesByName("iperf3"))
                {
                    process.Kill(); // When the program is closed kill all iperf3 on the system
                }
            }
            catch (Exception)
            {
                if (debug)
                    MessageBox.Show(e.ToString(), "Error"); // Show if error if debug is on
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                ipref3_server.Kill();
                foreach (var process in Process.GetProcessesByName("iperf3"))
                {
                    process.Kill(); // When the program close kill all iperf3 on the system
                }
            }
            catch (Exception)
            {
                if (debug)
                    MessageBox.Show(e.ToString(), "Error"); // Show if error if debug is on
            }
        }       
    }
}
