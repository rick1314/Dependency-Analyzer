/////////////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - Displays the features of the Analyzer, enables users to    //
// select files for analysis and get the result of the analysis                    //
// ver 3.1                                                                         //
// Author: Debopriyo Bhattacharya                                                  //
// Original Code by Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2018//
/////////////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package is the client side of the Analyzer. 
 * It displays a local FileFolder view, and a remote FileFolder view.  It supports
 * navigating into subdirectories, both locally and in the remote Server.
 * The client can select files or just path.
 * It can then pass that to the server which responds with the analysis file paths.
 * It can then display the results which include the TypeTable, dependecy and strong components.
 * 
 * 
 * Maintenance History:
 * --------------------
 * ver 3.1 : 04 Dec 2018
 * - added test case which is invoked by "Client_Test" which spawns a new thread
 *   which uses Dispatcher.Invoke to interact with the UI and display test case
 *   defined in Testing function
 * ver 3.0 : 02 Dec 2018
 * - modified to work as a client gui for file dependency analyzer program
 * - added functions to analyse files by calling the server thread and passing paths
 * ver 2.2 : 01 Dec 2018
 * - added functionalities for Connection checking , Remote Files and Folder selecting 
 * ver 2.1 : 26 Oct 2017
 * - relatively minor modifications to the Comm channel used to send messages
 *   between NavigatorClient and NavigatorServer
 * ver 2.0 : 24 Oct 2017
 * - added remote processing - Up functionality not yet implemented
 *   - defined NavigatorServer
 *   - added the CsCommMessagePassing prototype
 * ver 1.0 : 22 Oct 2017
 * - first release
 * 
 * Requirements: Toker, SemiExp, Parser, MessagePassingCommService,
 * FileMgr, Environment, Element, Dependency Analysis, AnalyzerServer packages. 
 * 
 * USE: Client_Test for the testing the functionality
 * 
 * 
 */



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
using System.IO;
using System.Threading;
using MessagePassingComm;

namespace Navigator
{
  public partial class MainWindow : Window
  {
    private IFileMgr fileMgr { get; set; } = null;  // note: Navigator just uses interface declarations
    Comm comm { get; set; } = null;
    Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
    Thread rcvThread = null;
    Thread testingThread = null;
    bool connected { get; set; } = false;

    private string path { get; set; }


    public MainWindow()
    {
      InitializeComponent();
      initializeEnvironment();
      Console.Title = "Analyzer Client Console";
      fileMgr = FileMgrFactory.create(FileMgrType.Local); // uses Environment
      getTopFiles();

      comm = new Comm(ClientEnvironment.address, ClientEnvironment.port);
      initializeMessageDispatcher();
      rcvThread = new Thread(rcvThreadProc);
      rcvThread.Start();

#if(Client_Test)
      testingThread = new Thread(Testing);
      testingThread.Start();
#endif
      /*
      tabControl.SelectedIndex = 1;  // moves to second tab
      */
    }
    //----< make Environment equivalent to ClientEnvironment >-------

    void initializeEnvironment()
    {
      Environment.root = ClientEnvironment.root;
      Environment.address = ClientEnvironment.address;
      Environment.port = ClientEnvironment.port;
      Environment.endPoint = ClientEnvironment.endPoint;
    }
    //----< define how to process each message command >-------------

    void initializeMessageDispatcher()
    {
      // load remoteFiles listbox with files from root
      messageDispatcher["getTopFiles"] = (CommMessage msg) =>
      {
        remoteFiles.Items.Clear();
        foreach (string file in msg.arguments)
        {
          remoteFiles.Items.Add(file);
        }
      };
      // load remoteDirs listbox with dirs from root
      messageDispatcher["getTopDirs"] = (CommMessage msg) =>
      {
        remoteDirs.Items.Clear();
        string path = msg.arguments[0];
        foreach (string dir in msg.arguments.Skip(1))
        {
          remoteDirs.Items.Add(dir);
        }
        CurrPath.Text = System.IO.Path.GetFullPath(path);
      };
      // load remoteFiles listbox with files from folder
      messageDispatcher["moveIntoFolderFiles"] = (CommMessage msg) =>
      {
        remoteFiles.Items.Clear();
        foreach (string file in msg.arguments)
        {
          remoteFiles.Items.Add(file);
        }
      };
      // load remoteDirs listbox with dirs from folder
      messageDispatcher["moveIntoFolderDirs"] = (CommMessage msg) =>
      {
        remoteDirs.Items.Clear();
        string path = msg.arguments[0];
        foreach (string dir in msg.arguments.Skip(1))
        {
          remoteDirs.Items.Add(dir);
        }
        CurrPath.Text = System.IO.Path.GetFullPath(path);
      };
      // moveUp remoteFiles listbox with files from ancestor
      messageDispatcher["moveUpFiles"] = (CommMessage msg) =>
      {
        remoteFiles.Items.Clear();
        foreach (string file in msg.arguments)
        {
          remoteFiles.Items.Add(file);
        }
      };
      // moveUp remoteDirs listbox with dirs from ancestor
      messageDispatcher["moveUpDirs"] = (CommMessage msg) =>
      {
        remoteDirs.Items.Clear();
        string path = msg.arguments[0];
        foreach (string dir in msg.arguments.Skip(1))
        {
          remoteDirs.Items.Add(dir);
        }
        CurrPath.Text = System.IO.Path.GetFullPath(path);
      };
      messageDispatcher["getFileDetails"] = (CommMessage msg) =>
      {
        string path = msg.arguments[0];
        if (!Selected.Items.Contains(path))
        {
          Selected.Items.Add(path);
        }
      };
      messageDispatcher["displayAnalysis"] = (CommMessage msg) =>
      {
        tabControl.SelectedIndex = 2;
        RawOut.Items.Clear();
        StrongComp.Items.Clear();
        StreamReader rawout = new StreamReader(msg.arguments[0]);
        StreamReader strongcomp = new StreamReader(msg.arguments[1]);
        string sLine;
        while ((sLine = rawout.ReadLine()) != null)
        {
          RawOut.Items.Add(sLine);
        }
        while ((sLine = strongcomp.ReadLine()) != null)
        {
          StrongComp.Items.Add(sLine);
        }
        rawout.Close();
        strongcomp.Close();
      };
    }
    
    //----< define processing for GUI's receive thread >-------------

    void rcvThreadProc()
    {
      Console.Write("\n  starting client's receive thread");
      while (true)
      {
        CommMessage msg = comm.getMessage();
        msg.show();
        if (msg.command == null)
          continue;

        // pass the Dispatcher's action value to the main thread for execution

        Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
      }
    }
    //----< shut down comm when the main window closes >-------------

    private void Window_Closed(object sender, EventArgs e)
    {
      comm.close();

      // The step below should not be nessary, but I've apparently caused a closing event to 
      // hang by manually renaming packages instead of getting Visual Studio to rename them.

      System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
    //----< not currently being used >-------------------------------

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
    }
    //----< show files and dirs in root path >-----------------------

    public void getTopFiles()
    {
      List<string> files = fileMgr.getFiles().ToList<string>();
      localFiles.Items.Clear();
      foreach (string file in files)
      {
         localFiles.Items.Add(file);
      }
      List<string> dirs = fileMgr.getDirs().ToList<string>();
      localDirs.Items.Clear();
      foreach (string dir in dirs)
      {
        localDirs.Items.Add(dir);
      }
    }
    //----< move to directory root and display files and subdirs >---

    private void localTop_Click(object sender, RoutedEventArgs e)
    {
      fileMgr.currentPath = "";
      path = System.IO.Path.Combine(Environment.root, fileMgr.currentPath);
      CurrPath.Text = System.IO.Path.GetFullPath(path);
      getTopFiles();
    }
    //----< show selected file in code popup window >----------------
    

    private void selectedFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
       Selected.Items.Remove(Selected.SelectedValue);
      
    }

    private void clear_Click(object sender, RoutedEventArgs e)
    {
      Selected.Items.Clear();
    }

    private void localFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      
      string fileName = localFiles.SelectedValue as string;

      string path = System.IO.Path.Combine(Environment.root, fileName);
      path = System.IO.Path.GetFullPath(path);
      if (!Selected.Items.Contains(path))
      {
        Selected.Items.Add(path);
      }
    }
    //----< move to parent directory and show files and subdirs >----

    private void localUp_Click(object sender, RoutedEventArgs e)
    {
      if (fileMgr.currentPath == "")
        return;
      fileMgr.currentPath = fileMgr.pathStack.Peek();
      fileMgr.pathStack.Pop();

      path = System.IO.Path.Combine(Environment.root, fileMgr.currentPath);
      CurrPath.Text = System.IO.Path.GetFullPath(path);

      getTopFiles();
    }
    //----< move into subdir and show files and subdirs >------------

    private void localDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      string dirName = localDirs.SelectedValue as string;
      fileMgr.pathStack.Push(fileMgr.currentPath);
      fileMgr.currentPath = dirName;

      path = System.IO.Path.Combine(Environment.root, fileMgr.currentPath);
      CurrPath.Text = System.IO.Path.GetFullPath(path);

      getTopFiles();
    }
    //----< move to root of remote directories >---------------------
    /*
     * - sends a message to server to get files from root
     * - recv thread will create an Action<CommMessage> for the UI thread
     *   to invoke to load the remoteFiles listbox
     */
    private void getRemoteFiles()
    {
      CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
      msg1.from = ClientEnvironment.endPoint;
      msg1.to = ServerEnvironment.endPoint;
      msg1.author = "Rick";
      msg1.command = "getTopFiles";
      msg1.arguments.Add("");
      comm.postMessage(msg1);
      CommMessage msg2 = msg1.clone();
      msg2.command = "getTopDirs";
      comm.postMessage(msg2);
    }

    private void RemoteTop_Click(object sender, RoutedEventArgs e)
    {
      connected = true;
      getRemoteFiles();
    }
    //----< download file and display source in popup window >-------

    private void remoteFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
      msg1.from = ClientEnvironment.endPoint;
      msg1.to = ServerEnvironment.endPoint;
      msg1.command = "getFileDetails";
      msg1.arguments.Add(remoteFiles.SelectedValue as string);
      comm.postMessage(msg1);
    }

    private void getRemoteDirs()
    {
      CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
      msg1.from = ClientEnvironment.endPoint;
      msg1.to = ServerEnvironment.endPoint;
      msg1.author = "Rick";
      msg1.command = "moveUpDirs";
      msg1.arguments.Add("");
      comm.postMessage(msg1);
      CommMessage msg2 = msg1.clone();
      msg2.command = "moveUpFiles";
      comm.postMessage(msg2);
    }
    
    //----< move to parent directory of current remote path >--------

    private void RemoteUp_Click(object sender, RoutedEventArgs e)
    {
      connected = true;
      getRemoteDirs();

    }
    //----< move into remote subdir and display files and subdirs >--
    /*
     * - sends messages to server to get files and dirs from folder
     * - recv thread will create Action<CommMessage>s for the UI thread
     *   to invoke to load the remoteFiles and remoteDirs listboxs
     */
    private void moveIntoDir(string DirName)
    {
      CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
      msg1.from = ClientEnvironment.endPoint;
      msg1.to = ServerEnvironment.endPoint;
      msg1.command = "moveIntoFolderDirs";
      msg1.arguments.Add(DirName);
      comm.postMessage(msg1);
      CommMessage msg2 = msg1.clone();
      msg2.command = "moveIntoFolderFiles";
      comm.postMessage(msg2);

    }

    private void remoteDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      moveIntoDir(remoteDirs.SelectedValue as string);
    }
    //Shows if a connection has been established with the server or not
    private void ConButton_Click(object sender, RoutedEventArgs e)
    {
      conBlock.Text = (connected == true) ? "Yes!" : "No!";
    }

    //This function is for analysing an entire directory
    private void analpath()
    {
      if (CurrPath.Text != "")
      {
        Console.WriteLine("\nOur path is: {0}", CurrPath.Text);


        //now we will send the path to analysePath
        CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
        msg1.from = ClientEnvironment.endPoint;
        msg1.to = ServerEnvironment.endPoint;
        msg1.command = "analysePath";
        msg1.arguments.Add(CurrPath.Text);
        comm.postMessage(msg1);

        //message is sent
        
      }
    }

    private void Anal1_Click(object sender, RoutedEventArgs e)
    {

      analpath();

    }

    private void Anal2_Click(object sender, RoutedEventArgs e)
    {
      if (Selected.Items.Count > 0)
      {
        // Selected
        List<string> files = new List<string>();
        //Console.WriteLine("Number of items selected: {0}",Selected.Items.Count);
        foreach (var item in Selected.Items)
        {
          // item.ToString()
          files.Add(item.ToString());

        }

        CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
        msg1.from = ClientEnvironment.endPoint;
        msg1.to = ServerEnvironment.endPoint;
        msg1.command = "analyseFiles";
        msg1.arguments = files;
        comm.postMessage(msg1);


      }

    }


    //Automated Testing GUI

    private void Testing()
    {
      Thread.Sleep(1000);
      Dispatcher.Invoke(() => { tabControl.SelectedIndex = 1; });
      Thread.Sleep(2000);
      Dispatcher.Invoke(() => { getRemoteDirs(); });
      Thread.Sleep(4000);
      Console.WriteLine("waiting 4 seconds before selecting directory");
      string dirName = remoteDirs.Items[remoteDirs.Items.Count / 2] as string;
      Console.WriteLine("Selected Directory: ", dirName);
      Thread.Sleep(1000);
      Dispatcher.Invoke(() => { moveIntoDir(dirName); });
      Thread.Sleep(1000);
      Dispatcher.Invoke(() => { analpath(); });
    }




  }

}

