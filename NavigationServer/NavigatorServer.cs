////////////////////////////////////////////////////////////////////////////
// NavigatorServer.cs - File Server for WPF NavigatorClient Application   //
// ver 2.0                                                                //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017        //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines a single NavigatorServer class that returns file
 * and directory information about its rootDirectory subtree.  It uses
 * a message dispatcher that handles processing of all incoming and outgoing
 * messages.
 * 
 * Maintanence History:
 * --------------------
 * ver 2.0 - 24 Oct 2017
 * - added message dispatcher which works very well - see below
 * - added these comments
 * ver 1.0 - 22 Oct 2017
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;

namespace Navigator
{
  public class NavigatorServer
  {
    IFileMgr localFileMgr { get; set; } = null;
    Comm comm { get; set; } = null;
//    bool both = true;
    Dictionary<string, Func<CommMessage, CommMessage>> messageDispatcher = 
      new Dictionary<string, Func<CommMessage, CommMessage>>();

    /*----< initialize server processing >-------------------------*/

    public NavigatorServer()
    {
      initializeEnvironment();
      Console.Title = "Navigator Server";
      localFileMgr = FileMgrFactory.create(FileMgrType.Local);
    }
    /*----< set Environment properties needed by server >----------*/

    //update Path
    //void pathupdate()
    //{
    //  if (localFileMgr.currentPath != "")
    //  {
    //    //   localFileMgr.currentPath = localFileMgr.pathStack.Peek();
    //    localFileMgr.currentPath = localFileMgr.pathStack.Pop();
    //    if (both == false)
    //    {
    //      localFileMgr.pathStack.Pop();
    //      both = true;
    //    }
    //    else
    //      both = false;
    //  }
    //}
    void initializeEnvironment()
    {
      Environment.root = ServerEnvironment.root;
      Environment.address = ServerEnvironment.address;
      Environment.port = ServerEnvironment.port;
      Environment.endPoint = ServerEnvironment.endPoint;
    }
    /*----< define how each message will be processed >------------*/

    void initializeDispatcher()
    {
      Func<CommMessage, CommMessage> getTopFiles = (CommMessage msg) =>
      {
        localFileMgr.currentPath = "";
        CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
        reply.to = msg.from;
        reply.from = msg.to;
        reply.command = "getTopFiles";
        reply.arguments = localFileMgr.getFiles().ToList<string>();
        return reply;
      };
      messageDispatcher["getTopFiles"] = getTopFiles;

      Func<CommMessage, CommMessage> getTopDirs = (CommMessage msg) =>
      {
        localFileMgr.currentPath = "";
        CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
        reply.to = msg.from;
        reply.from = msg.to;
        reply.command = "getTopDirs";

        string path = System.IO.Path.Combine(Environment.root, localFileMgr.currentPath);

        reply.arguments = new List<string>();
        reply.arguments.Add(path);
        foreach (var dir in localFileMgr.getDirs().ToList<string>())
        { reply.arguments.Add(dir); }

        return reply;
      };
      messageDispatcher["getTopDirs"] = getTopDirs;

      Func<CommMessage, CommMessage> moveIntoFolderFiles = (CommMessage msg) =>
      {
        if (msg.arguments.Count() == 1)
          localFileMgr.currentPath = msg.arguments[0];
        CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
        reply.to = msg.from;
        reply.from = msg.to;
        reply.command = "moveIntoFolderFiles";
        reply.arguments = localFileMgr.getFiles().ToList<string>();
        return reply;
      };
      messageDispatcher["moveIntoFolderFiles"] = moveIntoFolderFiles;

      Func<CommMessage, CommMessage> moveIntoFolderDirs = (CommMessage msg) =>
      {
        if (msg.arguments.Count() == 1)
          localFileMgr.currentPath = msg.arguments[0];
        CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
        reply.to = msg.from;
        reply.from = msg.to;
        reply.command = "moveIntoFolderDirs";

        string path = System.IO.Path.Combine(Environment.root, localFileMgr.currentPath);

        reply.arguments = new List<string>();
        reply.arguments.Add(path);
        foreach (var dir in localFileMgr.getDirs().ToList<string>())
        { reply.arguments.Add(dir); }

        localFileMgr.pathStack.Push(localFileMgr.currentPath);
        return reply;
      };
      messageDispatcher["moveIntoFolderDirs"] = moveIntoFolderDirs;
    

    Func<CommMessage, CommMessage> moveUpFiles = (CommMessage msg) =>
    {
      CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
      reply.to = msg.from;
      reply.from = msg.to;
      reply.command = "moveUpFiles";
      reply.arguments = localFileMgr.getFiles().ToList<string>();
      return reply;
    };
    messageDispatcher["moveUpFiles"] = moveUpFiles;

      Func<CommMessage, CommMessage> moveUpDirs = (CommMessage msg) =>
      {
        if (localFileMgr.currentPath != "")
        {
          localFileMgr.pathStack.Pop();
          localFileMgr.currentPath = localFileMgr.pathStack.Peek();
        }
        CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
        reply.to = msg.from;
        reply.from = msg.to;
        reply.command = "moveUpDirs";

        string path = System.IO.Path.Combine(Environment.root, localFileMgr.currentPath);

        reply.arguments = new List<string>();
        reply.arguments.Add(path);
        foreach (var dir in localFileMgr.getDirs().ToList<string>())
        { reply.arguments.Add(dir); }

        return reply;
      };
    messageDispatcher["moveUpDirs"] = moveUpDirs;


      Func<CommMessage, CommMessage> getFileDetails = (CommMessage msg) =>
      {

        string path = System.IO.Path.Combine(Environment.root, msg.arguments[0]);

        CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
        reply.to = msg.from;
        reply.from = msg.to;
        reply.command = "getFileDetails";
        reply.arguments.Add(System.IO.Path.GetFullPath(path));
        return reply;
      };
      messageDispatcher["getFileDetails"] = getFileDetails;


    }

  /*----< Server processing >------------------------------------*/
  /*
   * - all server processing is implemented with the simple loop, below,
   *   and the message dispatcher lambdas defined above.
   */
  static void Main(string[] args)
    {
      TestUtilities.title("Starting Navigation Server", '=');
      try
      {
        NavigatorServer server = new NavigatorServer();
        server.initializeDispatcher();
        server.comm = new MessagePassingComm.Comm(ServerEnvironment.address, ServerEnvironment.port);
        
        while (true)
        {
          CommMessage msg = server.comm.getMessage();
          if (msg.type == CommMessage.MessageType.closeReceiver)
            break;
          msg.show();
          if (msg.command == null)
            continue;
          CommMessage reply = server.messageDispatcher[msg.command](msg);
          reply.show();
          server.comm.postMessage(reply);
        }
      }
      catch(Exception ex)
      {
        Console.Write("\n  exception thrown:\n{0}\n\n", ex.Message);
      }
    }
  }
}
