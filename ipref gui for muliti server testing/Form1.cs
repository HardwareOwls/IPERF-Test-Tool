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
        public bool test1 = false; // Askes snask, ingen ved det, alle sankker om det

        public string CurrentVersion
        {
            get
            {
                return ApplicationDeployment.IsNetworkDeployed
                       ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString()
                       : Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        // ---------------------------------------------------------- //
        // Start From1
        // ---------------------------------------------------------- //
        public Form1(string[] args)
        {
            InitializeComponent();
            if (Environment.GetCommandLineArgs().Contains("-debug"))
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
            version = (FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location).ProductVersion).ToString();
            Text = String.Format("iPerf test gui v" + version);

            if (ApplicationDeployment.IsNetworkDeployed == true)
            {
                version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            else
            {
                version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            Text = String.Format("Ninja tester V" + version);
            try
            {
                foreach (var process in Process.GetProcessesByName("iperf3"))
                {
                    process.Kill();
        }
            }
            catch (Exception e)
            {
                if (debug)
                    MessageBox.Show(e.ToString(), "Error");
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
            string path = @Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\log - ping and speedtester\\" + shared_time_and_date + "  - " + log_type + ".csv"; // Defining the folder path to a folder "log - ping and speedtester" on the desktop
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
                    test_number++;
                    timer1.Enabled = true;
                    toolStripProgressBar1.Value = 1;
                    toolStripProgressBar2.Value = 1;
                    //Task task = Task.Run((void));
                    if (textBox_TCP_bitrate.Text == "0")
                    {
                        arg = "-i " + numericUpDown_TCP_Interval.Value +
                        " -P " + numericUpDown_TCP_parallele_streams.Value +
                        " -l " + numericUpDown_TCP_pakke_storlse.Value +
                        " -c " + textBox_TCP_IP_DNS.Text +
                        " -p " + numericUpDown_TCP_Port.Value +
                        " -t " + numericUpDown_TCP_runtime.Value;
                    }
                    else
                    {
                        arg = "-i " + numericUpDown_TCP_Interval.Value +
                        " -P " + numericUpDown_TCP_parallele_streams.Value +
                        " -l " + numericUpDown_TCP_pakke_storlse.Value +
                        " -b " + textBox_TCP_bitrate.Text +
                        " -c " + textBox_TCP_IP_DNS.Text +
                        " -p " + numericUpDown_TCP_Port.Value +
                        " -t " + numericUpDown_TCP_runtime.Value;
                    }
                    protocol = "TCP";
                    start_ipref3_async(arg);
                    timer1.Start();
                    this.btn_TCP_DL.Text = "Stop";
                }
            }
            else if (timer1.Enabled == true)
            {
                timer1.Enabled = false;
                timer1.Stop();
                toolStripProgressBar2.Value = 0;
                this.btn_TCP_DL.Text = "Download";
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
                    test_number++;
                    timer1.Enabled = true;
                    toolStripProgressBar1.Value = 1;
                    toolStripProgressBar2.Value = 1;
                    //Task task = Task.Run((void));
                    if (textBox_TCP_bitrate.Text == "0")
                    {
                        arg = "-i " + numericUpDown_TCP_Interval.Value +
                        " -P " + numericUpDown_TCP_parallele_streams.Value +
                        " -l " + numericUpDown_TCP_pakke_storlse.Value +
                        " -c " + textBox_TCP_IP_DNS.Text +
                        " -p " + numericUpDown_TCP_Port.Value +
                        " -t " + numericUpDown_TCP_runtime.Value;
                    }
                    else
                    {
                        arg = "-i " + numericUpDown_TCP_Interval.Value +
                        " -P " + numericUpDown_TCP_parallele_streams.Value +
                        " -l " + numericUpDown_TCP_pakke_storlse.Value +
                        " -b " + textBox_TCP_bitrate.Text +
                        " -c " + textBox_TCP_IP_DNS.Text +
                        " -p " + numericUpDown_TCP_Port.Value +
                        " -t " + numericUpDown_TCP_runtime.Value;
                    }
                    protocol = "TCP";
                    start_ipref3_async(arg);
                    timer1.Start();
                    this.btn_TCP_UL.Text = "Stop";
                }
            }
            else if (timer1.Enabled == true)
            {
                timer1.Enabled = false;
                timer1.Stop();
                toolStripProgressBar2.Value = 0;
                this.btn_TCP_UL.Text = "Upload";
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
                    test_number++;
                    timer1.Enabled = true;
                    toolStripProgressBar1.Value = 1;
                    toolStripProgressBar2.Value = 1;
                    //Task task = Task.Run((void));
                    arg = "-R" +
                        " -i " + numericUpDown_UDP_Interval.Value +
                        " -P " + numericUpDown_UDP_parallele_streams.Value +
                        " -l " + numericUpDown_UDP_pakke_storlse.Value +
                        " -b " + textBox_UDP_bitrate.Text +
                        " -c " + textBox_UDP_IP_DNS.Text +
                        " -p " + numericUpDown_UDP_Port.Value +
                        " -t " + numericUpDown_UDP_runtime.Value;
                    protocol = "UDP";
                    start_ipref3_async(arg);
                    timer1.Start();
                    this.btn_UDP_DL.Text = "Stop";
                }
            }
            else if (timer1.Enabled == true)
            {
                timer1.Enabled = false;
                timer1.Stop();
                toolStripProgressBar2.Value = 0;
                this.btn_UDP_DL.Text = "Download";
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
                    test_number++;
                    timer1.Enabled = true;
                    toolStripProgressBar1.Value = 1;
                    toolStripProgressBar2.Value = 1;
                    //Task task = Task.Run((void));
                    arg = "-i " + numericUpDown_UDP_Interval.Value +
                        " -P " + numericUpDown_UDP_parallele_streams.Value +
                        " -l " + numericUpDown_UDP_pakke_storlse.Value +
                        " -b " + textBox_UDP_bitrate.Text +
                        " -c " + textBox_UDP_IP_DNS.Text +
                        " -p " + numericUpDown_UDP_Port.Value +
                        " -t " + numericUpDown_UDP_runtime.Value;
                    protocol = "UDP";
                    start_ipref3_async(arg);
                    timer1.Start();
                    this.btn_UDP_UL.Text = "Stop";
                }
            }
            else if (timer1.Enabled == true)
            {
                timer1.Enabled = false;
                timer1.Stop();
                toolStripProgressBar2.Value = 0;
                this.btn_UDP_UL.Text = "Upload";
            }
        }

        // ---------------------------------------------------------- //
        // Ipref3 client
        // ---------------------------------------------------------- //
        private void start_ipref3_async(string arguments)
        {
            //* Create your Process
            Process process = new Process();
            process.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\iperf3.exe";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            //* Set your output and error (asynchronous) handlers
            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            //* Start process and handlers
            using (StreamWriter file = new StreamWriter(get_log_path("ipref3_"+protocol+"_log"), true))
            {
                file.WriteLine("");
                file.WriteLine("Test nr: " + test_number + ". - " + DateTime.Now.ToString());
                file.WriteLine("-----------------------------------------------------------");
            }
            using (StreamWriter file = new StreamWriter(get_log_path("server_log"), true))
            {
                file.WriteLine("");
                file.WriteLine("Test nr: " + test_number + ". - " + DateTime.Now.ToString());
            }

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            //process.WaitForExit();
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            //Console.WriteLine(outLine.Data);
            this.Invoke((new MethodInvoker(delegate () {
                try
                {
                    if (timer1.Enabled == false && iperf_running == true)
                    {
                        iperf_running = false;
                        MessageBox.Show(
                                "iperf has exited",
                                "",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                    }
                    if (protocol == "TCP")
                    {
                        textBox_TCP_log.AppendText(outLine.Data + Environment.NewLine);
                    }
                    if (protocol == "UDP")
                    {
                        textBox_UDP_log.AppendText(outLine.Data + Environment.NewLine);
                    }
                    toolStripProgressBar1.Value = 0;

                    using (StreamWriter file = new StreamWriter(get_log_path("ipref3_log"), true))
                    {
                        file.WriteLine(outLine.Data);
                    }

                }
                catch (Exception)
                {
                }

            })));
            //richTextBox1.Text = outLine.Data;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            toolStripProgressBar1.Value = 1;
            start_ipref3_async(arg);
        }

        /// <summary>
        /// Ping1s this instance.
        /// </summary>
        /// <param name="Ip">The ip.</param>
        /// <param name="times">Times to ping.</param>
        /// <param name="name">The name of the log file.</param>
        public bool ping(string Ip, string times, string name, bool echo)
        {
            string cmd = Ip + " I -q -i 0 -n " + times;
            Process proc = new Process();
            proc.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\psping.exe";
            proc.StartInfo.Arguments = cmd;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;
            try
            {
                proc.Start();
                string output = proc.StandardOutput.ReadToEnd();
                output = output.Substring(output.LastIndexOf(" "));
                output = output.TrimEnd(Environment.NewLine.ToCharArray()).Replace("ms", "");
                if (echo)
                {
                    try
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            if (name == "ping_1")
                                tekst_boks_ping_ud_1.Text = output;
                            if (name == "ping_2")
                                tekst_boks_ping_ud_2.Text = output;
                        });
                    }
                    catch (Exception e)
                    {
                        if (debug)
                            MessageBox.Show(e.ToString(), "Error");
                        return false;
                    }
                }
                Console.WriteLine("Ping: " + output);           
                using (StreamWriter file = new StreamWriter(get_log_path(name), true))
                {
                    file.WriteLine(output);
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
        // Ping alle
        // ---------------------------------------------------------- //
        private void knap_alle_Click(object sender, EventArgs e)
        {
            test_number_ping++;
            progressBar1.Maximum = (int)numericUpDown1.Value;
            Thread th1 = new Thread(() => run_more_times((int)numericUpDown1.Value, tekst_boks_ip_adresse_1.Text, antal_ping_1.Value.ToString(), "ping_1", true));
            th1.IsBackground = true;
            th1.Start();
            Thread th2 = new Thread(() => run_more_times((int)numericUpDown1.Value, tekst_boks_ip_adresse_2.Text, antal_ping_2.Value.ToString(), "ping_2", true));
            th2.IsBackground = true;
            th2.Start();
        }

        private void run_more_times(int antal, string Ip1, string times1, string name1, bool echo)
        {
            Stopwatch sw = new Stopwatch();
            if (echo)
            {
                try
                {
                    Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = 0;
                    });
                }
                catch (Exception e)
                {
                    if (debug)
                        MessageBox.Show(e.ToString(), "Error");
                }
            }

            for (int i = 0; i < antal; i++)
            {
                sw.Start();
                if (echo)
                {
                    try
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            progressBar1.Value = i;
                        });
                    }
                    catch (Exception)
                    {
                        //throw;
                    }
                }
                Thread th = new Thread(() => ping(Ip1, times1, name1, echo));
                th.IsBackground = true;

                th.Start();

                th.Join();
                th.Abort();
                sw.Stop();
                Console.WriteLine(name1 + " tog " + (int)sw.ElapsedMilliseconds + "ms");
                //Thread.Sleep(1000 - (int)sw.ElapsedMilliseconds);
                Thread.Sleep(1000);
                sw.Reset();
            }
            if (echo)
            {
                try
                {
                    Invoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = 0;
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
        // Ipref server
        // ---------------------------------------------------------- //
        private void button2_Click_1(object sender, EventArgs e)
        {

            if (button2.Text == "Start")
            {
                button2.Text = "Stop";
                string port = numericUpDown_server_port.Value.ToString();
                arg = " -s -i 1 -p " + port;
                start_ipref3_async_server(arg);
                toolStripProgressBar3.Value = 1;
            }
            else if (button2.Text == "Stop")
            {
                try
                {
                    //ipref3_server.Kill();
                    //ipref3_server.CancelOutputRead();
                    //ipref3_server.CancelErrorRead();
                    //ipref3_server.Close();
                    toolStripProgressBar3.Value = 0;
                    foreach (var process in Process.GetProcessesByName("iperf3"))
                    {
                        process.Kill();
                }
                }
                catch (Exception)
                {
                    if (debug)
                        MessageBox.Show(e.ToString(), "Error");
                }
                button2.Text = "Start";
            }
        }

        private void start_ipref3_async_server(string arguments)
        {
            //* Create your Process
            Process ipref3_server = new Process();
            ipref3_server.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\iperf3.exe";
            ipref3_server.StartInfo.Arguments = arguments;
            ipref3_server.StartInfo.CreateNoWindow = true;
            ipref3_server.StartInfo.UseShellExecute = false;
            ipref3_server.StartInfo.RedirectStandardOutput = true;
            ipref3_server.StartInfo.RedirectStandardError = true;
            //* Set your output and error (asynchronous) handlers
            ipref3_server.OutputDataReceived += new DataReceivedEventHandler(OutputHandler_server);
            ipref3_server.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler_server);

            
            using (StreamWriter file = new StreamWriter(get_log_path("server_log"), true))
            {
                file.WriteLine("Server startet - " + DateTime.Now.ToString());
                file.WriteLine("");
            }
            

            //* Start process and handlers
            ipref3_server.Start();
            try
            {
                ipref3_server.BeginOutputReadLine();
                ipref3_server.BeginErrorReadLine();
            }
            catch (Exception e)
            {
                if (debug)
                    MessageBox.Show(e.ToString(), "Error");
            }
            //process.WaitForExit();
        }

        private void OutputHandler_server(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            //Console.WriteLine(outLine.Data);
            try
            {
                this.Invoke((new MethodInvoker(delegate ()
                {
                    try
                    {
                        textBox_server_log.AppendText(outLine.Data + Environment.NewLine);

                        using (StreamWriter file = new StreamWriter(get_log_path("server_log"), true))
                        {
                            file.WriteLine(outLine.Data);
                            test1 = true;
                        }
                    }
                    catch (Exception e)
                    {
                        if (debug)
                            MessageBox.Show(e.ToString(), "Error");
                    }

                })));
            }
            catch (Exception e)
            {
                if (debug)
                    MessageBox.Show(e.ToString(), "Error");
            }
            //richTextBox1.Text = outLine.Data;
        }


        // ---------------------------------------------------------- //
        // Ipref3 TCP Nulstil
        // ---------------------------------------------------------- //
        private void button8_Click(object sender, EventArgs e)
        {
            numericUpDown_TCP_Interval.Value = 1;
            numericUpDown_TCP_parallele_streams.Value = 1;
            numericUpDown_TCP_pakke_storlse.Value = 16000;
            textBox_TCP_IP_DNS.Text = "localhost";
            numericUpDown_TCP_Port.Value = 5201;
            textBox_TCP_bitrate.Text = "0";
            numericUpDown_TCP_runtime.Value = 10;
        }

        // ---------------------------------------------------------- //
        // Ipref3 UDP nulstil
        // ---------------------------------------------------------- //
        private void button14_Click(object sender, EventArgs e)
        {
            numericUpDown_UDP_Interval.Value = 1;
            numericUpDown_UDP_parallele_streams.Value = 1;
            numericUpDown_UDP_pakke_storlse.Value = 8000;
            textBox_UDP_IP_DNS.Text = "localhost";
            numericUpDown_UDP_Port.Value = 5201;
            textBox_UDP_bitrate.Text = "1M";
            numericUpDown_UDP_runtime.Value = 10;
        }

        private void antal_ping_1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void tekst_boks_ip_adresse_1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                btn_ping1.PerformClick();
            }
        }

        private void tekst_boks_ip_adresse_2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                btn_ping2.PerformClick();
            }
        }

        private void antal_ping_1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                btn_ping1.PerformClick();
            }
        }

        private void antal_ping_2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                btn_ping2.PerformClick();
            }
        }

        private void btn_ping1_Click(object sender, EventArgs e)
        {
            test_number_ping++;
            Thread th = new Thread(() => ping(tekst_boks_ip_adresse_1.Text, antal_ping_1.Value.ToString(), "ping_1", true));
            if (!th.IsAlive)
            {
                th.IsBackground = true;
                th.Start();
            }
        }

        private void btn_ping2_Click(object sender, EventArgs e)
        {
            test_number_ping++;
            Thread th = new Thread(() => ping(tekst_boks_ip_adresse_2.Text, antal_ping_2.Value.ToString(), "ping_2", true));
            if (!th.IsAlive)
            {
                th.IsBackground = true;
                th.Start();
            }
        }

        private void tekst_boks_ping_ud_1_MouseClick(object sender, MouseEventArgs e)
        {
            tekst_boks_ping_ud_1.SelectAll();
            tekst_boks_ping_ud_1.Copy();
        }

        private void textBox_TCP_IP_DNS_TextChanged(object sender, EventArgs e)
        {
            textBox_UDP_IP_DNS.Text = textBox_TCP_IP_DNS.Text;
        }

        private void textBox_UDP_IP_DNS_TextChanged(object sender, EventArgs e)
        {
            textBox_TCP_IP_DNS.Text = textBox_UDP_IP_DNS.Text;
        }

        private void numericUpDown_TCP_Port_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown_UDP_Port.Value = numericUpDown_TCP_Port.Value;
            numericUpDown_server_port.Value = numericUpDown_TCP_Port.Value;
        }

        private void numericUpDown_UDP_Port_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown_TCP_Port.Value = numericUpDown_UDP_Port.Value;
            numericUpDown_server_port.Value = numericUpDown_UDP_Port.Value;
        }

        private void numericUpDown_server_port_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown_TCP_Port.Value = numericUpDown_server_port.Value;
            numericUpDown_UDP_Port.Value = numericUpDown_server_port.Value;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(() => test("tcp"));
            th.Start();
        }
        private void test(string prot)
        {
            Stopwatch sw = new Stopwatch();
            for (int i = 1; i < 31; i++)
            {
                if (textBox_TCP_IP_DNS.Text != "localhost")
                    startIperf();
               
                if (debug)
                    Console.WriteLine("iPerf started");

                Thread.Sleep(1000);
                this.Invoke((MethodInvoker)delegate
                {
                    toolStripProgressBar1.Value = 1;
                    toolStripProgressBar2.Value = 1;
                });
                arg = "-i " + numericUpDown_TCP_Interval.Value +
                    " -P " + numericUpDown_TCP_parallele_streams.Value +
                    " -l " + numericUpDown_TCP_pakke_storlse.Value +
                    " -b " + i + "M" +
                    " -c " + textBox_TCP_IP_DNS.Text +
                    " -p " + numericUpDown_TCP_Port.Value +
                    " -t 500";
                protocol = prot;
                sw.Start();
                Process process = new Process();
                process.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\iperf3.exe";
                process.StartInfo.Arguments = arg;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                
                Console.WriteLine("Ping started!");
                for (int j = 0; j < 200; j++)
                {
                    ping(textBox_TCP_IP_DNS.Text, "1", "ping_1_" + textBox_TCP_IP_DNS.Text +  " +" + i.ToString().PadLeft(2, '0') + " M " + prot, false);
                    Console.WriteLine("Ping sent: " + j);
                    Thread.Sleep(1000);
                    Console.WriteLine("Slept");
                }
                Console.WriteLine("Ping Done!" + sw.Elapsed.ToString());
               
                sw.Reset();
                this.Invoke((MethodInvoker)delegate
                {
                    toolStripProgressBar1.Value = 0;
                    toolStripProgressBar2.Value = 0;
                });
                Console.WriteLine("Thread sleeping");
                Thread.Sleep(60000);
                Console.WriteLine("Thead slept");
                if (!process.HasExited)
                {
                    process.Kill();
                    Console.WriteLine("iPerf was killed");

                }
            }
            Console.WriteLine("Test done");
        }
        private void startIperf()
        {
            using(var client = new SshClient(textBox_TCP_IP_DNS.Text, "pi", "raspberry"))
            {
                try
                {
                    client.Connect();
                    Console.WriteLine("SSH established");
                    client.RunCommand("iperf3 -s -1");
                    Console.WriteLine("iPerf started with: -s -1 ");
                    client.Disconnect();
                    Console.WriteLine("SSH Closed");
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
            AllocConsole();
            get_time_and_date();
            try
            {
                foreach (var process in Process.GetProcessesByName("iperf3"))
                {
                    process.Kill();
                }
            }
            catch (Exception)
            {
                if (debug)
                    MessageBox.Show(e.ToString(), "Error");
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                //ipref3_server.Kill();
                foreach (var process in Process.GetProcessesByName("iperf3"))
                {
                    process.Kill();
                }
            }
            catch (Exception)
            {
                if (debug)
                    MessageBox.Show(e.ToString(), "Error");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                ipref3_server.Kill();
                foreach (var process in Process.GetProcessesByName("iperf3"))
                {
                    process.Kill();
                }
            }
            catch (Exception)
            {
                if (debug)
                    MessageBox.Show(e.ToString(), "Error");
            }
        }

        private void btn_udpTest1_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(() => test("udp"));
            th.Start();
        }
    }
}
