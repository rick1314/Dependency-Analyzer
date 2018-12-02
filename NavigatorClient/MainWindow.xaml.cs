////////////////////////////////////////////////////////////////////////////
// NavigatorClient.xaml.cs - Demonstrates Directory Navigation in WPF App //
// ver 2.0                                                                //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017        //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines WPF application processing by the client.  The client
 * displays a local FileFolder view, and a remote FileFolder view.  It supports
 * navigating into subdirectories, both locally and in the remote Server.
 * 
 * It also supports viewing local files.
 * 
 * Maintenance History:
 * --------------------
 * ver 2.1 : 26 Oct 2017
 * - relatively minor modifications to the Comm channel used to send messages
 *   between NavigatorClient and NavigatorServer
 * ver 2.0 : 24 Oct 2017
 * - added remote processing - Up functionality not yet implemented
 *   - defined NavigatorServer
 *   - added the CsCommMessagePassing prototype
 * ver 1.0 : 22 Oct 2017
 * - first release
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
    bool connected = false;

    /// <summary>
    /// ////////////////////////////////
    /// </summary>

    public string path { get; set; }


    public MainWindow()
    {
      InitializeComponent();
      initializeEnvironment();
      Console.Title = "Client";
      fileMgr = FileMgrFactory.create(FileMgrType.Local); // uses Environment
      getTopFiles();

      comm = new Comm(ClientEnvironment.address, ClientEnvironment.port);
      initializeMessageDispatcher();
      rcvThread = new Thread(rcvThreadProc);
      rcvThread.Start();

      //getRemoteFiles();

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
        //CodePopUp popup = new CodePopUp();
        //popup.codeView.Text = path;
        //popup.Show();

        if (!Selected.Items.Contains(path))
        {
          Selected.Items.Add(path);
        }

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
      //Selected.Items.Add("Hello Brother");

      //try
      //{
      //  string path = System.IO.Path.Combine(Environment.root, fileName);
      //  string contents = File.ReadAllText(path);
      //  CodePopUp popup = new CodePopUp();
      //  popup.codeView.Text = contents;
      //  popup.Show();
      //}
      //catch (Exception ex)
      //{
      //  string msg = ex.Message;
      //}
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
    //----< move to parent directory of current remote path >--------

    private void RemoteUp_Click(object sender, RoutedEventArgs e)
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
    //----< move into remote subdir and display files and subdirs >--
    /*
     * - sends messages to server to get files and dirs from folder
     * - recv thread will create Action<CommMessage>s for the UI thread
     *   to invoke to load the remoteFiles and remoteDirs listboxs
     */
    private void remoteDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
      msg1.from = ClientEnvironment.endPoint;
      msg1.to = ServerEnvironment.endPoint;
      msg1.command = "moveIntoFolderDirs";
      msg1.arguments.Add(remoteDirs.SelectedValue as string);
      comm.postMessage(msg1);
      CommMessage msg2 = msg1.clone();
      msg2.command = "moveIntoFolderFiles";
      comm.postMessage(msg2);
    }

    private void ConButton_Click(object sender, RoutedEventArgs e)
    {
      conBlock.Text = (connected == true) ? "Yes!" : "No!";
    }






    /////////////////////////////////////////////
    /*
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
          path = Directory.GetCurrentDirectory();
          path = getAncestorPath(2, path);
          LoadNavTab(path);
        }
        //----< find parent paths >--------------------------------------

        string getAncestorPath(int n, string path)
        {
          for (int i = 0; i < n; ++i)
            path = Directory.GetParent(path).FullName;
          return path;
        }
        //----< file Find Libs tab with subdirectories and files >-------

        void LoadNavTab(string path)
        {
          Dirs.Items.Clear();
          CurrPath.Text = path;
          string[] dirs = Directory.GetDirectories(path);
          Dirs.Items.Add("..");
          foreach (string dir in dirs)
          {
            DirectoryInfo di = new DirectoryInfo(dir);
            string name = System.IO.Path.GetDirectoryName(dir);
            Dirs.Items.Add(di.Name);
          }
          Files.Items.Clear();
          string[] filess = Directory.GetFiles(path);
          foreach (string file in filess)
          {
            string name = System.IO.Path.GetFileName(file);
            Files.Items.Add(name);
          }
        }
        //----< handle selections in files listbox >---------------------

        private void Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          if (unselecting)
          {
            unselecting = false;
            return;
          }
          //if (swin == null)
          //{
          //  swin = new SelectionWindow();
          //  swin.setMainWindow(this);
          //}
          //swin.Show();

          if (e.AddedItems.Count == 0)
            return;
          string selStr = e.AddedItems[0].ToString();
          selStr = System.IO.Path.Combine(path, selStr);
          if (!selectedFiles.Contains(selStr))
          {
            selectedFiles.Add(selStr);
            //swin.show(selStr);
          }
        }
        //----< unselect files called by unloading SelectionWindow >-----

        public void unselectFiles()
        {
          unselecting = true;  // needed to avoid using selection logic
          //selectedFiles.Clear();
          Files.UnselectAll();
        }
        //----< move into double-clicked directory, display contents >---

        private void Dirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
          string selectedDir = Dirs.SelectedItem.ToString();
          if (selectedDir == "..")
            path = getAncestorPath(1, path);
          else
            path = System.IO.Path.Combine(path, selectedDir);
          LoadNavTab(path);
        }
        //----< shut down the SelectionWindow if open >------------------

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
          //swin.Close();
        }
    */
    private void Anal1_Click(object sender, RoutedEventArgs e)
    {
      if (CurrPath.Text != "")
      {
        Console.WriteLine("\nOur path is: {0}", CurrPath.Text);

        tabControl.SelectedIndex = 2;
        //RawOut
        //StrongComp
        string[] paths = {"C:\\Users\\RICK\\Desktop\\First Sem\\SMA\\Project3\\CSE681Project3\\result\\result.txt",
                        "C:\\Users\\RICK\\Desktop\\First Sem\\SMA\\Project3\\CSE681Project3\\result\\strongCom.txt" };

        StreamReader rawout = new StreamReader(paths[0]);
        StreamReader strongcomp = new StreamReader(paths[1]);

        string sLine;
        while ((sLine = rawout.ReadLine()) != null)
        {
          RawOut.Items.Add(sLine);
        }

        while ((sLine = strongcomp.ReadLine()) != null)
        {
          StrongComp.Items.Add(sLine);
        }

      }

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

      }

    }

  }

}

