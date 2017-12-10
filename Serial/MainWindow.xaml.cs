using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO.Ports;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;


namespace Serial {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        bool isConnected = false;
        String[] ports;
        SerialPort port;


        public int Iset = -1;
        public float Iactual = -1;
        public int Imax = -1;

        public int rep = -1;
        public int trigger = -1;
        public float delay = -1;

        public int Tchannel = -1;
        public float Tactual = -1;
        public float Terror = -1;
        public float Tset = -1;
        string Tenable = "";
        string Tstatus = "";

        public bool running = false;
        public bool locked = false;
        public bool shutter = false;
        public bool error = true;

        public bool IsGetMessage = false;



        Stopwatch sw = new Stopwatch();
        



    List<Label> LableList = new List<Label>();

        DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();


        List<string> SendCommands = new List<string>();

        List<string> ReceieveMessages = new List<string>();

        private string lastSendMessage;
        private string lastGetMessage;

        public int SelectedLabel = 0;

        public int Commanddelay = 5;

        public string RecieveString = "";
        public string testString = "";
        public int SendCount = 0;
        public int RecieveCount = 0;
        public int ActualSendCount = 0;


        public object mySendLock = new object();
        public object myRecieveLock = new object();

        public MainWindow() {
            InitializeComponent();

            getAvailableComPorts();

            foreach (string port in ports) {
                SerialList.Items.Add(port);

                if (ports[0] != null) {
                    SerialList.SelectedItem = ports[0];
                }
            }

            EllipseConnection.Fill = Brushes.Red;
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);




            LableList.Add(LableISetValue);
            LableList.Add(LabelRepValue);
            LableList.Add(LableTriggerValue);

            LableList.Add(LableImaxValue);
            LableList.Add(LableDelayValue);

            LableList.Add(LableTchannelValue);


            LableList.Add(LableTsetValue);
            LableList.Add(LableTenableValue);

            LableList[0].Background = Brushes.Green;
        }



        public void CommandProcess() {
            bool changed = false;

            lock (ReceieveMessages) {

                while (ReceieveMessages.Count() > 0) {

                    

                    string temp = ReceieveMessages[0];
                    lastGetMessage = ReceieveMessages[0];
                    ReceieveMessages.RemoveAt(0);


                    if (temp == "") {

                        continue;
                    }

                    string[] mylist = temp.Split(':');
                    

                    if (mylist.Length < 2) {

                        continue;
                    }
                    try {

                     
                        switch (mylist[0]) {


                          

                            case "F110E":

                                Iset = Int32.Parse(mylist[1]);
                                changed = true;
                                IsGetMessage = true;
                                break;


                            case "F112E":
                                Iactual = float.Parse(mylist[1]);
                                changed = true;
                                IsGetMessage = true;
                                break;

                            case "F111E":
                                Imax = Int32.Parse(mylist[1]);
                                changed = true;
                                IsGetMessage = true;
                                break;


                            case "F116E":
                                Tchannel = Int32.Parse(mylist[1]);
                                changed = true;
                                IsGetMessage = true;
                                break;
                            case "F118E":
                                Tactual = float.Parse(mylist[1]);
                                changed = true;
                                IsGetMessage = true;
                                break;



                            case "F119E":
                                Terror = float.Parse(mylist[1]);
                                changed = true;
                                IsGetMessage = true;
                                break;

                            case "F113E":
                                rep = Int32.Parse(mylist[1]);
                                changed = true;
                                IsGetMessage = true;
                                break;
                            case "F115E":
                                trigger = Int32.Parse(mylist[1]);
                                changed = true;
                                IsGetMessage = true;
                                break;


                            case "F114E":
                                delay = float.Parse(mylist[1]);
                                changed = true;
                                IsGetMessage = true;
                                break;


                            case "F117E":
                                Tset = float.Parse(mylist[1]);
                                changed = true;
                                IsGetMessage = true;
                                break;


                            case "F136E":
                                Tenable = (mylist[1]);
                                changed = true;
                                IsGetMessage = true;
                                break;


                            case "F130E":
                                if (Int32.Parse(mylist[1]) == 1) {
                                    running = true;
                                    changed = true;

                                } else {
                                    running = false;
                                    changed = true;
                                }
                                IsGetMessage = true;
                                break;

                            case "F134E":
                                if (Int32.Parse(mylist[1]) == 1) {
                                    changed = true;
                                    locked = true;

                                } else {
                                    changed = true;
                                    locked = false;
                                }
                                IsGetMessage = true;
                                break;
                            case "F132E":
                                if (Int32.Parse(mylist[1]) == 1) {
                                    changed = true;
                                    shutter = true;

                                } else {
                                    changed = true;
                                    shutter = false;
                                }
                                IsGetMessage = true;
                                break;

                            case "F199E":
                                if (mylist[1] != "000000") {
                                    changed = true;
                                    error = true;

                                    Tstatus = "ERROR";

                                } else {
                                    changed = true;

                                    error = false;

                                    Tstatus = "OK";

                                }
                                IsGetMessage = true;
                                break;
                            default:
                                IsGetMessage = true;
                                Debug.WriteLine("no match " + temp);
                                break;

                        }
                    } catch (Exception ee) {

                        MessageBox.Show(ee.ToString());

                    }





                }




            }

            if (changed) {

               // updateGUI();
            }

        }

        private void StringProcess() {

        

            lock (ReceieveMessages) {

                while (RecieveString.IndexOf("\n\r") != -1) {

                    int index = RecieveString.IndexOf("\n\r");

                    ReceieveMessages.Add(RecieveString.Substring(0, index));
                   
                    RecieveCount++;


                    RecieveString = RecieveString.Substring(index + 2);

                }


            }

       
            CommandProcess();




        }

        void HideButtons() {
            ButtonSelect_.Visibility = Visibility.Collapsed;
            ButtonSelectM.Visibility = Visibility.Collapsed;
            ButtonValue_.Visibility = Visibility.Collapsed;
            ButtonValue_1.Visibility = Visibility.Collapsed;
            ButtonSave.Visibility = Visibility.Collapsed;
            ButtonRun1.Visibility = Visibility.Collapsed;
            ButtonShutter.Visibility = Visibility.Collapsed;
            ButtonLock.Visibility = Visibility.Collapsed;


        }

        void Visibutton() {
            ButtonSelect_.Visibility = Visibility.Visible;
            ButtonSelectM.Visibility = Visibility.Visible;
            ButtonValue_.Visibility = Visibility.Visible;
            ButtonValue_1.Visibility = Visibility.Visible;
            ButtonSave.Visibility = Visibility.Visible;
            ButtonRun1.Visibility = Visibility.Visible;
            ButtonShutter.Visibility = Visibility.Visible;
            ButtonLock.Visibility = Visibility.Visible;

        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            // code goes here 



            ReadInformation();


            //ProcessGetMessage();



        }
        void getAvailableComPorts() {
            ports = SerialPort.GetPortNames();
        }


        private void HandleTimeout() {

            try {
              
                port.Dispose();
                port.Close();

                RecieveString = "";

            } catch (Exception e) {

            }

            dispatcherTimer.Stop();

            ActualSendCount = 0;
            RecieveCount = 0;
            testString = "";
            getAvailableComPorts();

            SerialList.Items.Clear();
            isConnected = false;

            EllipseConnection.Fill = Brushes.Blue;
            Connect.Content = "Connect";

            foreach (string port in ports) {
                SerialList.Items.Add(port);

                if (ports[0] != null) {
                    SerialList.SelectedItem = ports[0];
                }
            }

        }

        private bool lostConnection = false;
        private void waitForResponse() {
            lostConnection = false;
            sw.Reset();

            sw.Start();

            while (!IsGetMessage) {
                if (sw.ElapsedMilliseconds > 5000) {
                    sw.Stop();
                    lostConnection = true;
                    HandleTimeout();
                    return;

                }

            }
        }

        private void ReadInformation() {

            lock (SendCommands) {
           
             

                SendCommands.Add("F110E");
            }
                ProcessGetMessage();

                waitForResponse();

            if (lostConnection) {
                return;
            }

            lock (SendCommands) {
                SendCommands.Add("F112E");
            }
                ProcessGetMessage();
                waitForResponse();


            if (lostConnection) {
                return;
            }
            lock (SendCommands) {
                SendCommands.Add("F111E");
            }
            ProcessGetMessage();
            waitForResponse();

            if (lostConnection) {
                return;
            }

            lock (SendCommands) {
                SendCommands.Add("F116E");
            }
                ProcessGetMessage();
                waitForResponse();

            lock (SendCommands) {
                SendCommands.Add("F118E");
            }
            ProcessGetMessage();
            waitForResponse();

            lock (SendCommands) {
                SendCommands.Add("F119E");
            }
            ProcessGetMessage();
            waitForResponse();

            if (lostConnection) {
                return;
            }
            lock (SendCommands) {
                SendCommands.Add("F113E");
            }
            ProcessGetMessage();
            waitForResponse();

            if (lostConnection) {
                return;
            }
            lock (SendCommands) {
                SendCommands.Add("F115E");
            }
            ProcessGetMessage();
            waitForResponse();

            if (lostConnection) {
                return;
            }
            lock (SendCommands) {
                SendCommands.Add("F114E");
            }
            ProcessGetMessage();
            waitForResponse();

            if (lostConnection) {
                return;
            }
            lock (SendCommands) {
                SendCommands.Add("F117E");
            }
            ProcessGetMessage();
            waitForResponse();

            if (lostConnection) {
                return;
            }
            lock (SendCommands) {
                SendCommands.Add("F136E");
            }
            ProcessGetMessage();
            waitForResponse();

            if (lostConnection) {
                return;
            }
            lock (SendCommands) {
                SendCommands.Add("F134E");
            }
            ProcessGetMessage();
            waitForResponse();

            if (lostConnection) {
                return;
            }
            lock (SendCommands) {
                SendCommands.Add("F132E");
            }

            ProcessGetMessage();
            waitForResponse();

            if (lostConnection) {
                return;
            }
            lock (SendCommands) {
                SendCommands.Add("F130E");
            }
            ProcessGetMessage();
            waitForResponse();

            if (lostConnection) {
                return;
            }
            lock (SendCommands) {
                SendCommands.Add("F199E");
            }
            ProcessGetMessage();
            waitForResponse();
           
            //SendCommands.Add("F118E");
            //SendCommands.Add("F119E");
            //SendCommands.Add("F113E");
            //SendCommands.Add("F115E");
            //SendCommands.Add("F114E");
            //SendCommands.Add("F117E");
            //SendCommands.Add("F136E");
            //SendCommands.Add("F134E");
            //SendCommands.Add("F132E");
            //SendCommands.Add("F130E");
            //SendCommands.Add("F199E");


            updateGUI();


        }




        private void updateGUI() {

            LableISetValue.Content = Iset;
            LableIactualValue.Content = Iactual;
            LableImaxValue.Content = Imax;

            LableDelayValue.Content = delay;
            LabelRepValue.Content = rep.ToString() + "Hz";

            LableTchannelValue.Content = Tchannel;
            LableITactualValue.Content = Tactual;
            LableTerrorValue.Content = Terror;
            LableTsetValue.Content = Tset;

            if (Tenable == "1") {
                LableTenableValue.Content = "Enable";

            } else {
                LableTenableValue.Content = "Disable";
            }

            LableTstatusValue.Content = Tstatus;

            if (trigger == 0) {
                LableTriggerValue.Content = "Int";

            } else if (trigger == 1) {

                LableTriggerValue.Content = "Ext";

            } else {
                LableTriggerValue.Content = "unknow";
            }


            if (running) {

                EllipseRunning.Fill = Brushes.Green;
            } else {
                EllipseRunning.Fill = Brushes.White;
            }
            if (locked) {

                EllipseLocked.Fill = Brushes.Green;
                DisableButtons();
            } else {
                EllipseLocked.Fill = Brushes.White;
                EnableButtons();

            }
            if (shutter) {

                EllipseShutter.Fill = Brushes.Green;
            } else {
                EllipseShutter.Fill = Brushes.White;
            }
            if (error) {

                EllipseError.Fill = Brushes.Green;
            } else {
                EllipseError.Fill = Brushes.White;
            }

        }
        bool connect() {
            if (SerialList.SelectedItem == null) {

                return false;
            }
            string selectedPort = SerialList.SelectedItem.ToString();
            port = new SerialPort(selectedPort, 115200);

            port.Open();
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
            port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(Recieve);
            dispatcherTimer.Start();

            return port.IsOpen;
        }

        void disconnect() {
            try {




                port.Close();
                port.Dispose();
                dispatcherTimer.Stop();

                SendCommands.Clear();
                ReceieveMessages.Clear();
                RecieveString = "";

            } catch (Exception ee) {
                MessageBox.Show(ee.ToString());
            }
        }

        public bool errorFlag = false;

        private void Recieve(object sender, System.IO.Ports.SerialDataReceivedEventArgs e) {

           

            try {

                string indata = port.ReadExisting();

                testString += indata;

                lock (RecieveString) {

                    RecieveString += indata;
                }


             //   this.Dispatcher.Invoke(() => {


                    StringProcess();
              //  });



            } catch (System.IO.IOException error) {
                return;
            } catch (System.InvalidOperationException error) {
                Debug.WriteLine(error);
                return;
            }


        }

        private void Connect_Click(object sender, RoutedEventArgs e) {
            if (isConnected) {

                disconnect();

                Connect.Content = "Connect";
                isConnected = false;

                EllipseConnection.Fill = Brushes.Red;
            } else {
                bool flag = connect();
                if (flag) {

                    Connect.Content = "Disconnect";

                    isConnected = true;

                    EllipseConnection.Fill = Brushes.Green;

                } else {

                    MessageBox.Show("Error cannot connect");
                    isConnected = false;
                    EllipseConnection.Fill = Brushes.Red;

                }

            }
        }

        private void ProcessGetMessage() {
            if (port == null) {
                loseConnection();
                return;
            }
            if (!port.IsOpen) {

                loseConnection();

                return;
            }


            lock (SendCommands) {
                byte[] asciiBytes;


                while (SendCommands.Count() > 0) {


                    asciiBytes = Encoding.ASCII.GetBytes(SendCommands[0]);

                    try {

                        if(ActualSendCount != RecieveCount) {

                            Debug.WriteLine("last send: " + lastSendMessage);
                            Debug.WriteLine("last get: " + lastGetMessage);
                        }
                        port.Write(asciiBytes, 0, asciiBytes.Length);

                        //Thread.Sleep(Commanddelay);
                        IsGetMessage = false;
                        lastSendMessage = SendCommands[0];
                        SendCommands.RemoveAt(0);

                        ActualSendCount++;

                    } catch (Exception ee) {
                        MessageBox.Show(ee.ToString());
                    }


                }




            }




        }


        private void ButtonSelectM_Click(object sender, RoutedEventArgs e) {
            SelectedLabel++;
            if (SelectedLabel > 7) {
                SelectedLabel = 7;
            }

            foreach (Label a in LableList) {
                a.Background = null;
            }
            LableList[SelectedLabel].Background = Brushes.Green;
        }

        private void ButtonSelectP_Click(object sender, RoutedEventArgs e) {
            SelectedLabel--;
            if (SelectedLabel < 0) {
                SelectedLabel = 0;
            }

            foreach (Label a in LableList) {
                a.Background = null;
            }
            LableList[SelectedLabel].Background = Brushes.Green;
        }

        private void ButtonValueP_Click(object sender, RoutedEventArgs e) {

            if (port == null)
                return;
            if (!port.IsOpen) {
                return;
            }
            switch (SelectedLabel) {

                case 0:
                    SetISet(true);
                    break;
                case 3:
                    SetImax(true);
                    break;
                case 5:
                    SetTchannel(true);
                    break;
                case 1:
                    SetRep(true);
                    break;
                case 2:
                    SetTrigger(true);
                    break;
                case 4:
                    SetDelay(true);
                    break;
                case 6:
                    SetTset(true);
                    break;
                case 7:
                    SetTenable(true);
                    break;
                default:
                    break;
            }
        }

        private void ButtonValueM_Click(object sender, RoutedEventArgs e) {

            if (port == null)
                return;
            if (!port.IsOpen)
                return;
            switch (SelectedLabel) {

                case 0:
                    SetISet(false);
                    break;
                case 3:
                    SetImax(false);
                    break;
                case 5:
                    SetTchannel(false);
                    break;
                case 1:
                    SetRep(false);
                    break;
                case 2:
                    SetTrigger(false);
                    break;
                case 4:
                    SetDelay(false);
                    break;
                case 6:
                    SetTset(false);
                    break;
                case 7:
                    SetTenable(false);
                    break;
                default:
                    break;
            }

        }

        private void ButtonShutter_Click(object sender, RoutedEventArgs e) {

            if (port == null)
                return;
            if (!port.IsOpen)
                return;

            shutter = !shutter;
            if (shutter) {


                lock (SendCommands) {

                    SendCommands.Add("F1331E");

                }

                // SendSetMessage("F1331E");

            } else {

                lock (SendCommands) {

                    SendCommands.Add("F1330E");

                }
                // SendSetMessage("F1330E");

            }

            ProcessGetMessage();
            waitForResponse();
            updateGUI();

        }

        private void ButtonRun1_Click(object sender, RoutedEventArgs e) {

            if (port == null)
                return;
            if (!port.IsOpen)
                return;

            running = !running;
            if (running) {
                lock (SendCommands) {

                    SendCommands.Add("F1311E");

                }

                //  SendSetMessage("F1311E");

            } else {

                lock (SendCommands) {

                    SendCommands.Add("F1310E");

                }

                // SendSetMessage("F1310E");

            }

            ProcessGetMessage();
            waitForResponse();

            lock (SendCommands) {

                SendCommands.Add("F112E");

            }
            ProcessGetMessage();
            waitForResponse();

            updateGUI();


        }

        private void ButtonLock_Click(object sender, RoutedEventArgs e) {

            if (port == null)
                return;
            if (!port.IsOpen)
                return;
            locked = !locked;

            if (locked) {

                DisableButtons();
                lock (SendCommands) {

                    SendCommands.Add("F1351E");

                }

                // SendSetMessage("F1351E");

            } else {
                EnableButtons();

                lock (SendCommands) {

                    SendCommands.Add("F1350E");

                }
                // SendSetMessage("F1350E");

            }

            ProcessGetMessage();
            waitForResponse();
            updateGUI();

        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e) {

            if (port == null)
                return;
            if (!port.IsOpen)
                return;
            lock (SendCommands) {

                SendCommands.Add("F129E");

            }

            // SendSetMessage("F129E");

            ProcessGetMessage();
            waitForResponse();
            updateGUI();
        }

        private void SetISet(bool flag) {

            int temp;
            if (flag) {
                Iset = Iset + 10;
                if (Iset > Imax) {
                    Iset -= 10;
                    return;
                }

            } else {
                Iset = Iset - 10;
                if (Iset < 0) {

                    Iset += 10;
                    return;
                }


            }

            string buffer = "F121" + Iset.ToString() + "E";// string.Format("0:000", Iset) + "E";


            lock (SendCommands) {

                SendCommands.Add(buffer);
                //  SendCommands.Add("F110E");

            }

            //  SendSetMessage(buffer);

            ProcessGetMessage();
            waitForResponse();
            updateGUI();
        }

        private void SetImax(bool flag) {
            if (flag) {
                Imax += 10;
                if (Imax > 500) {
                    Imax -= 10;
                    return;
                }

            } else {
                Imax -= 10;
                if (Imax < 100) {

                    Imax += 10;
                    return;
                }


            }

            string buffer = "F122" + Imax.ToString() + "E";

            // SendSetMessage(buffer);
            lock (SendCommands) {

                SendCommands.Add(buffer);

            }

            ProcessGetMessage();
            waitForResponse();
            updateGUI();
        }
        private void SetTchannel(bool flag) {
            if (flag) {
                Tchannel += 1;
                if (Tchannel > 6) {
                    Tchannel -= 1;
                    return;
                }

            } else {
                Tchannel -= 1;
                if (Tchannel < 1) {

                    Tchannel += 1;
                    return;
                }


            }

            string buffer = "F126" + Tchannel.ToString() + "E";

            // SendSetMessage(buffer);

            lock (SendCommands) {

                SendCommands.Add(buffer);

            }
            dispatcherTimer.Stop();

            ProcessGetMessage();
            waitForResponse();
            ReadInformation();

           
            updateGUI();
            dispatcherTimer.Start();




        }


        private void SetRep(bool flag) {

            if (flag) {
                if (rep < 1000) {
                    rep += 10;
                } else {
                    rep += 1000;
                }
                if (rep > 10000) {
                    rep -= 1000;
                    return;
                }

            } else {
                if (rep <= 1000) {
                    rep -= 10;
                } else {
                    rep -= 1000;
                }

                if (rep < 10) {

                    rep += 10;
                    return;
                }


            }

            string buffer = "F123" + rep.ToString() + "E";

            // SendSetMessage(buffer);

            lock (SendCommands) {

                SendCommands.Add(buffer);

            }

            ProcessGetMessage();
            waitForResponse();
            updateGUI();
        }

        private void SetDelay(bool flag) {
            if (flag) {
                delay += 10;
                if (delay > 600) {
                    delay -= 10;
                    return;
                }

            } else {
                delay -= 10;
                if (delay < 0) {

                    delay += 10;
                    return;
                }


            }

            string buffer = "F125" + delay.ToString() + "E";

            // SendSetMessage(buffer);

            lock (SendCommands) {

                SendCommands.Add(buffer);

            }

            ProcessGetMessage();
            waitForResponse();
            updateGUI();
        }

        private void SetTrigger(bool flag) {
            if (flag) {
                trigger = 1;

                lock (SendCommands) {

                    SendCommands.Add("F1241E");

                }
                //  SendSetMessage("F1241E");
            } else {
                trigger = 0;

                lock (SendCommands) {

                    SendCommands.Add("F1240E");

                }
                //  SendSetMessage("F1240E");
            }

            ProcessGetMessage();
            waitForResponse();
            updateGUI();
        }

        private void SetTset(bool flag) {
            if (flag) {
                Tset += Convert.ToSingle(0.1);
                if (Tset > 100) {
                    Tset -= Convert.ToSingle(0.1);
                    return;
                }

            } else {
                Tset -= Convert.ToSingle(0.1);
                if (Tset < 10) {

                    Tset += Convert.ToSingle(0.1);
                    return;
                }


            }

            string buffer = "F127" + Tset.ToString() + "E";

            // SendSetMessage(buffer);


            lock (SendCommands) {

                SendCommands.Add(buffer);

            }

            ProcessGetMessage();
            waitForResponse();
            updateGUI();
        }

        private void SetTenable(bool flag) {
            if (flag) {


                Tenable = "1";

                lock (SendCommands) {

                    SendCommands.Add("F1371E");

                }
                // SendSetMessage("F1371E");
            } else {
                Tenable = "0";

                lock (SendCommands) {

                    SendCommands.Add("F1370E");

                }
                //SendSetMessage("F1370E");
            }

            ProcessGetMessage();
            waitForResponse();
            updateGUI();
        }

        private void DisableButtons() {

            ButtonSelect_.IsEnabled = false;
            ButtonSelectM.IsEnabled = false;
            ButtonValue_.IsEnabled = false;
            ButtonValue_1.IsEnabled = false;
            ButtonSave.IsEnabled = false;
            ButtonRun1.IsEnabled = false;
            ButtonShutter.IsEnabled = false;
        }

        private void EnableButtons() {

            ButtonSelect_.IsEnabled = true;
            ButtonSelectM.IsEnabled = true;
            ButtonValue_.IsEnabled = true;
            ButtonValue_1.IsEnabled = true;
            ButtonSave.IsEnabled = true;
            ButtonRun1.IsEnabled = true;
            ButtonShutter.IsEnabled = true;
        }

        private void loseConnection() {
            try {
                port.Dispose();

                port.Close();
            }catch(Exception exception) {


            }
            dispatcherTimer.Stop();
            SendCommands.Clear();
            ReceieveMessages.Clear();
            RecieveString = "";

            getAvailableComPorts();

            SerialList.Items.Clear();
            isConnected = false;

            EllipseConnection.Fill = Brushes.Red;
            Connect.Content = "Connect";

            foreach (string port in ports) {
                SerialList.Items.Add(port);

                if (ports[0] != null) {
                    SerialList.SelectedItem = ports[0];
                }
            }

            MessageBox.Show("Lost connection, try to reconnect.");

        }

        private void Refresh_Click(object sender, RoutedEventArgs e) {
            getAvailableComPorts();

            SerialList.Items.Clear();

            foreach (string port in ports) {
                SerialList.Items.Add(port);

                if (ports[0] != null) {
                    SerialList.SelectedItem = ports[0];
                }
            }
        }


    }




}
